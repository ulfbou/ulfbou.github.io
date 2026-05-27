using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SoloPulse.Collector;

/// <summary>
/// Thin wrapper around the GitHub GraphQL API (https://api.github.com/graphql).
/// Uses a single batched query for repo+commit+PR data, then up to 5 additional
/// requests to fetch .csproj content for dependency-edge resolution.
/// </summary>
sealed class GitHubGraphQlClient : IDisposable
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    // Main query: repos + last-100 commits + open PRs + open issues + root tree
    // Filters: isFork=false, privacy=PUBLIC — archived repos are filtered client-side.
    private const string ReposQuery = """
        query($login: String!, $repoCount: Int!) {
          user(login: $login) {
            repositories(
              first: $repoCount
              isFork: false
              privacy: PUBLIC
              orderBy: { field: PUSHED_AT, direction: DESC }
            ) {
              nodes {
                name
                description
                isArchived
                primaryLanguage { name }
                pushedAt
                object(expression: "HEAD:") {
                  ... on Tree {
                    entries { name type }
                  }
                }
                defaultBranchRef {
                  target {
                    ... on Commit {
                      history(first: 100) {
                        nodes { committedDate message }
                      }
                    }
                  }
                }
                pullRequests(
                  first: 20
                  states: [OPEN]
                  orderBy: { field: CREATED_AT, direction: DESC }
                ) {
                  nodes { createdAt state }
                }
                issues(
                  first: 20
                  states: [OPEN]
                  orderBy: { field: CREATED_AT, direction: DESC }
                ) {
                  nodes { createdAt state }
                }
              }
            }
          }
        }
        """;

    // Secondary query: fetch a single file's text content from a repo.
    private const string FileContentQuery = """
        query($owner: String!, $name: String!, $expr: String!) {
          repository(owner: $owner, name: $name) {
            object(expression: $expr) {
              ... on Blob { text }
            }
          }
        }
        """;

    public GitHubGraphQlClient(string token)
    {
        _http = new HttpClient { BaseAddress = new Uri("https://api.github.com") };
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("SoloPulse.Collector/1.0");
        _http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v4+json");
        _http.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Returns all public non-fork repositories for <paramref name="login"/>.
    /// Archived repos returned by the API are filtered out client-side.
    /// </summary>
    public async Task<List<RepositoryNode>> FetchReposAsync(
        string login, CancellationToken ct = default)
    {
        var data = await ExecuteWithRetryAsync<UserQueryData>(
            ReposQuery,
            new { login, repoCount = 30 },
            ct);

        return (data?.User?.Repositories?.Nodes ?? [])
            .Where(r => !r.IsArchived)
            .ToList();
    }

    /// <summary>
    /// Fetches the text content of a single file from a repository.
    /// Returns <c>null</c> when the file does not exist or could not be read.
    /// </summary>
    public async Task<string?> FetchFileContentAsync(
        string owner, string repo, string path, CancellationToken ct = default)
    {
        var data = await ExecuteWithRetryAsync<FileQueryData>(
            FileContentQuery,
            new { owner, name = repo, expr = $"HEAD:{path}" },
            ct);

        return data?.Repository?.Object?.Text;
    }

    // ─── Infrastructure ───────────────────────────────────────────────────────

    private async Task<T?> ExecuteWithRetryAsync<T>(
        string query, object variables, CancellationToken ct)
        where T : class
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                var payload = JsonSerializer.Serialize(
                    new { query, variables }, JsonOpts);

                using var response = await _http.PostAsync(
                    "/graphql",
                    new StringContent(payload, Encoding.UTF8, "application/json"),
                    ct);

                if (response.StatusCode is HttpStatusCode.Forbidden
                                        or HttpStatusCode.TooManyRequests)
                {
                    if (attempt == 0)
                    {
                        // Honour Retry-After when present; default 60 s.
                        int waitSeconds = 60;
                        if (response.Headers.TryGetValues("Retry-After", out var values)
                            && int.TryParse(values.FirstOrDefault(), out int ra))
                            waitSeconds = ra;

                        Console.Error.WriteLine(
                            $"Rate limited — waiting {waitSeconds}s before retry…");
                        await Task.Delay(TimeSpan.FromSeconds(waitSeconds), ct);
                        continue;
                    }

                    Console.Error.WriteLine(
                        "Rate limit persists after retry — returning empty data.");
                    return default;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(ct);
                var result = JsonSerializer.Deserialize<GraphQlResponse<T>>(json, JsonOpts);

                if (result?.Errors is { Count: > 0 })
                    foreach (var err in result.Errors)
                        Console.Error.WriteLine($"GraphQL error: {err.Message}");

                return result?.Data;
            }
            catch (HttpRequestException ex) when (attempt == 0)
            {
                Console.Error.WriteLine($"Network error: {ex.Message} — retrying once…");
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Network error: {ex.Message} — returning empty data.");
                return default;
            }
        }

        return default;
    }

    public void Dispose() => _http.Dispose();
}
