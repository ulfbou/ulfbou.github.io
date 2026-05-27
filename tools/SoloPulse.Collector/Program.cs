using System.Text.Json;
using System.Text.Json.Serialization;
using SoloPulse.Collector;

// ─── Configuration ────────────────────────────────────────────────────────────

const int MaxPublicRepos = 15;
const int MaxPublicGraphEdges = 30;

var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
if (string.IsNullOrWhiteSpace(token))
{
    Console.Error.WriteLine("GITHUB_TOKEN required");
    return 1;
}

var targetUser = Environment.GetEnvironmentVariable("TARGET_USER") ?? "ulfbou";
var outputDir  = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? FindOutputDir();

Directory.CreateDirectory(outputDir);

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
    WriteIndented               = true,
    DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    NumberHandling              = JsonNumberHandling.Strict,
};

var generatedAt = DateTime.UtcNow.ToString("o");
using var cts   = new CancellationTokenSource(TimeSpan.FromSeconds(120));

// ─── Fetch ────────────────────────────────────────────────────────────────────

List<RepositoryNode> repos = [];

try
{
    using var client = new GitHubGraphQlClient(token);
    repos = await client.FetchReposAsync(targetUser, cts.Token);

    // Dependency edges: parse .csproj from top 5 repos by pushedAt.
    var depEdges = new List<GraphEdge>();

    foreach (var repo in repos.Take(5))
    {
        var csprojName = repo.Tree?.Entries?
            .FirstOrDefault(e => e.Name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            ?.Name;

        if (csprojName is null) continue;

        var content = await client.FetchFileContentAsync(
            targetUser, repo.Name, csprojName, cts.Token);

        if (content is null) continue;

        var targets = MetricsCalculator.ParseProjectReferences(content);
        var repoNames = repos
            .Select(r => NormalizeRepoId(r.Name))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var target in targets)
        {
            var sourceId = NormalizeRepoId(repo.Name);
            var targetId = NormalizeRepoId(target);

            // Only emit edges to repos we actually fetched.
            if (repoNames.Contains(targetId)
                && !targetId.Equals(sourceId, StringComparison.Ordinal))
            {
                depEdges.Add(new GraphEdge(
                    sourceId,
                    targetId,
                    "dependency",
                    0.8));
            }
        }
    }

    // ─── Build metrics inputs ─────────────────────────────────────────────────

    var allCommits = repos
        .SelectMany(r => r.DefaultBranchRef?.Target?.History?.Nodes?
            .Select(c => new RepoCommit(
                r.Name,
                DateTime.Parse(c.CommittedDate, null,
                    System.Globalization.DateTimeStyles.RoundtripKind),
                c.Message))
            ?? [])
        .ToList();

    var commitsByRepo = allCommits
        .GroupBy(c => c.RepoName, StringComparer.Ordinal)
        .ToDictionary(g => g.Key, g => g.Select(c => c.CommittedAt).ToList());

    var momentum         = MetricsCalculator.CalculateMomentum(commitsByRepo);
    var contextSwitches  = MetricsCalculator.CalculateContextSwitches(allCommits);
    var temporalEdges    = MetricsCalculator.BuildTemporalEdges(allCommits);

    var (peakHours, avgSessionMinutes, fragmentationIndex, streakDays, lastQuietPeriod)
        = MetricsCalculator.CalculateRhythm(allCommits);

    // ─── Build output ─────────────────────────────────────────────────────────

    int medianMomentum = repos.Count > 0
        ? momentum.Values.OrderBy(v => v).ElementAt(momentum.Count / 2)
        : 0;

    var repoEntries = repos.Select(r =>
    {
        var commits  = commitsByRepo.GetValueOrDefault(r.Name) ?? [];
        int mom      = momentum.GetValueOrDefault(r.Name);
        int openPRs  = r.PullRequests?.Nodes?.Count(p => p.State == "OPEN") ?? 0;
        int openIss  = r.Issues?.Nodes?.Count(i => i.State == "OPEN") ?? 0;

        // Last commit timestamp = latest deep-work timestamp
        var lastCommit = commits.Count > 0
            ? (DateTime?)commits.Max()
            : null;

        return new RepoEntry(
            Name:               NormalizeRepoId(r.Name),
            DisplayName:        ToDisplayName(r.Name),
            Language:           r.PrimaryLanguage?.Name ?? "unknown",
            Momentum:           mom,
            FocusMinutes7d:     MetricsCalculator.CalculateFocusMinutes7d(commits),
            ContextSwitchesOut: contextSwitches.GetValueOrDefault(r.Name),
            LastDeepWork:       lastCommit?.ToString("o"),
            ReturnRate:         MetricsCalculator.CalculateReturnRate(commits),
            OpenLoops:          openPRs + openIss,
            Summary:            r.Description ?? string.Empty);
    }).ToList();

    var publicRepos = repoEntries
        .GroupBy(r => NormalizeRepoId(r.Name))
        .Select(g => g
            .OrderByDescending(r => r.Momentum)
            .ThenByDescending(r => r.FocusMinutes7d)
            .ThenByDescending(r => r.OpenLoops)
            .ThenByDescending(r => r.LastDeepWork ?? string.Empty)
            .First())
        .OrderByDescending(r => r.Momentum)
        .ThenByDescending(r => r.FocusMinutes7d)
        .ThenByDescending(r => r.OpenLoops)
        .ThenByDescending(r => r.LastDeepWork ?? string.Empty)
        .Take(MaxPublicRepos)
        .ToList();

    var publicRepoIds = publicRepos
        .Select(r => NormalizeRepoId(r.Name))
        .ToHashSet(StringComparer.Ordinal);

    var publicNodes = publicRepos.Select(r => new GraphNode(
        Id:    NormalizeRepoId(r.Name),
        Label: r.Name,
        Group: r.Momentum >= medianMomentum ? "core" : "support"))
        .ToList();

    var publicEdges = depEdges
        .Concat(temporalEdges)
        .Select(e => new GraphEdge(
            NormalizeRepoId(e.Source),
            NormalizeRepoId(e.Target),
            e.Type,
            e.Weight))
        .Where(e => publicRepoIds.Contains(e.Source))
        .Where(e => publicRepoIds.Contains(e.Target))
        .Where(e => e.Source != e.Target)
        .GroupBy(e => new { e.Source, e.Target, e.Type })
        .Select(g => g
            .OrderByDescending(e => e.Weight)
            .First())
        .OrderByDescending(e => e.Weight)
        .Take(MaxPublicGraphEdges)
        .ToList();

    // ─── Write files ──────────────────────────────────────────────────────────

    var reposFile = new ReposFile("1", generatedAt, publicRepos);
    var graphFile = new GraphFile("1", generatedAt, publicNodes, publicEdges);
    var rhythmFile = new RhythmFile(
        SchemaVersion:      "1",
        GeneratedAt:        generatedAt,
        PeakHours:          peakHours,
        AvgSessionMinutes:  avgSessionMinutes,
        FragmentationIndex: fragmentationIndex,
        DeepWorkStreakDays: streakDays,
        LastQuietPeriod:    lastQuietPeriod);

    await WriteJsonAsync(Path.Combine(outputDir, "repos.json"),  reposFile,  jsonOptions);
    await WriteJsonAsync(Path.Combine(outputDir, "graph.json"),  graphFile,  jsonOptions);
    await WriteJsonAsync(Path.Combine(outputDir, "rhythm.json"), rhythmFile, jsonOptions);

    int totalFocusMinutes = publicRepos.Sum(r => r.FocusMinutes7d);
    Console.WriteLine(
        $"Collected {publicRepos.Count} public repos, {publicEdges.Count} public edges, " +
        $"{totalFocusMinutes} minutes focus, written to \"{outputDir}\"");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex.Message}");

    // Write valid but empty output so the Blazor site does not get a 404.
    var empty = new ReposFile("1", generatedAt, []);
    var emptyGraph = new GraphFile("1", generatedAt, [], []);
    var emptyRhythm = new RhythmFile("1", generatedAt, [], 0, 0.0, 0, null);

    await WriteJsonAsync(Path.Combine(outputDir, "repos.json"),  empty,        jsonOptions);
    await WriteJsonAsync(Path.Combine(outputDir, "graph.json"),  emptyGraph,   jsonOptions);
    await WriteJsonAsync(Path.Combine(outputDir, "rhythm.json"), emptyRhythm,  jsonOptions);

    return 0; // Exit 0 so GitHub Actions does not fail the whole job on empty data.
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

static async Task WriteJsonAsync<T>(string path, T value, JsonSerializerOptions opts)
{
    await using var stream = File.Open(path, FileMode.Create, FileAccess.Write);
    await JsonSerializer.SerializeAsync(stream, value, opts);
    await stream.FlushAsync();
}

/// <summary>
/// Walks up the directory tree starting from the binary location to find the
/// solution root (the directory that contains src/Ulfbou.Site).  Falls back to
/// the current working directory, which is the solution root when invoked via
/// <c>dotnet run --project tools/SoloPulse.Collector</c>.
/// </summary>
static string FindOutputDir()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);

    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "src", "Ulfbou.Site")))
            return Path.Combine(dir.FullName, "src", "Ulfbou.Site", "wwwroot", "data");

        dir = dir.Parent;
    }

    return Path.Combine(Directory.GetCurrentDirectory(),
        "src", "Ulfbou.Site", "wwwroot", "data");
}

/// <summary>Normalizes repository names for public JSON IDs and graph endpoints.</summary>
static string NormalizeRepoId(string repoName) =>
    repoName.Trim().ToLowerInvariant();

/// <summary>Converts "dx.metadata" → "Dx.Metadata".</summary>
static string ToDisplayName(string repoName) =>
    string.Join('.', repoName
        .Split('.')
        .Select(p => p.Length > 0
            ? char.ToUpperInvariant(p[0]) + p[1..]
            : p));
