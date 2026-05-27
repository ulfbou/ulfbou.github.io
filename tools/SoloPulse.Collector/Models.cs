using System.Text.Json.Serialization;

namespace SoloPulse.Collector;

// ─── Output: repos.json ──────────────────────────────────────────────────────

/// <summary>Root document written to repos.json.</summary>
record ReposFile(
    string SchemaVersion,
    string GeneratedAt,
    List<RepoEntry> Repos);

/// <summary>Per-repository solo-developer metrics.</summary>
record RepoEntry(
    string Name,
    string DisplayName,
    string Language,
    /// <summary>0-100, weighted by recency (7d×1.0 + 8-14d×0.5 + 15-30d×0.2), normalised.</summary>
    int Momentum,
    /// <summary>Sum of commit-session durations in the last 7 days (sessions separated by ≥90 min gap).</summary>
    int FocusMinutes7d,
    /// <summary>Times you switched away to another repo within a 2-hour window.</summary>
    int ContextSwitchesOut,
    string? LastDeepWork,
    /// <summary>Interior gaps ÷ (interior gaps + terminal departure); 1.0 when no departures.</summary>
    double ReturnRate,
    /// <summary>Open PRs + open issues (proxy for unresolved work).</summary>
    int OpenLoops,
    string Summary);

// ─── Output: graph.json ───────────────────────────────────────────────────────

/// <summary>Root document written to graph.json.</summary>
record GraphFile(
    string SchemaVersion,
    string GeneratedAt,
    List<GraphNode> Nodes,
    List<GraphEdge> Edges);

record GraphNode(string Id, string Label, string Group);

/// <summary>
/// Edge types:
///   "dependency" — repo A has a ProjectReference to repo B in its .csproj.
///   "temporal"   — commits to A and B occurred within 2 hours of each other.
/// </summary>
record GraphEdge(string Source, string Target, string Type, double Weight);

// ─── Output: rhythm.json ──────────────────────────────────────────────────────

/// <summary>Root document written to rhythm.json.</summary>
record RhythmFile(
    string SchemaVersion,
    string GeneratedAt,
    List<int> PeakHours,
    int AvgSessionMinutes,
    /// <summary>Average number of distinct repos touched per day over the last 7 days.</summary>
    double FragmentationIndex,
    int DeepWorkStreakDays,
    string? LastQuietPeriod);

// ─── GitHub GraphQL response models ──────────────────────────────────────────

record GraphQlRequest(string Query, object Variables);

record GraphQlResponse<T>(T? Data, List<GraphQlError>? Errors);

record GraphQlError(string Message);

// User + repositories query
record UserQueryData(UserData? User);

record UserData(RepositoryConnection? Repositories);

record RepositoryConnection(List<RepositoryNode>? Nodes);

record RepositoryNode(
    string Name,
    string? Description,
    PrimaryLanguage? PrimaryLanguage,
    string PushedAt,
    bool IsArchived,
    bool IsFork,
    /// <summary>Root tree (expression "HEAD:"); mapped from the JSON key "object".</summary>
    [property: JsonPropertyName("object")] TreeObject? Tree,
    DefaultBranchRefData? DefaultBranchRef,
    PullRequestConnection? PullRequests,
    IssueConnection? Issues);

record PrimaryLanguage(string Name);

record TreeObject(List<TreeEntry>? Entries);

record TreeEntry(string Name, string Type);

record DefaultBranchRefData(CommitTarget? Target);

record CommitTarget(CommitHistory? History);

record CommitHistory(List<CommitNode>? Nodes);

record CommitNode(string CommittedDate, string Message);

record PullRequestConnection(List<PullRequestNode>? Nodes);

record PullRequestNode(string CreatedAt, string? MergedAt, string? ClosedAt, string State);

record IssueConnection(List<IssueNode>? Nodes);

record IssueNode(string CreatedAt, string? ClosedAt, string State);

// File-content query (for .csproj dependency parsing)
record FileQueryData(RepositoryBlob? Repository);

record RepositoryBlob(
    [property: JsonPropertyName("object")] BlobContent? Object);

record BlobContent(string? Text);

// ─── Internal processing model ────────────────────────────────────────────────

/// <summary>Flattened commit used across all metric calculations.</summary>
record RepoCommit(string RepoName, DateTime CommittedAt, string Message);
