using System.Xml.Linq;

namespace SoloPulse.Collector;

/// <summary>
/// Pure, stateless calculation functions for all SoloPulse metrics.
/// No I/O, no side effects — all methods are safe to unit-test in isolation.
/// </summary>
static class MetricsCalculator
{
    // ─── Tuning constants ─────────────────────────────────────────────────────

    /// <summary>Gap between commits that ends a focus session.</summary>
    private static readonly TimeSpan SessionGap = TimeSpan.FromMinutes(90);

    /// <summary>Window in which a repo switch counts as a context switch.</summary>
    private static readonly TimeSpan ContextSwitchWindow = TimeSpan.FromHours(2);

    /// <summary>Minimum gap between commits before treating it as a departure from a repo.</summary>
    private static readonly TimeSpan ReturnGap = TimeSpan.FromHours(24);

    /// <summary>Window in which cross-repo commits produce a temporal edge.</summary>
    private static readonly TimeSpan TemporalEdgeWindow = TimeSpan.FromHours(2);

    // ─── Momentum ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Calculates raw (un-normalised) momentum per repo using the formula:
    ///   score = commits_last_7d × 1.0 + commits_8–14d × 0.5 + commits_15–30d × 0.2
    /// Then normalises all repos to 0–100.
    /// </summary>
    public static Dictionary<string, int> CalculateMomentum(
        Dictionary<string, List<DateTime>> commitsByRepo)
    {
        var now = DateTime.UtcNow;
        var raw = new Dictionary<string, double>();

        foreach (var (repo, dates) in commitsByRepo)
        {
            double score = 0;
            foreach (var d in dates)
            {
                double ageDays = (now - d).TotalDays;
                score += ageDays <= 7   ? 1.0 :
                         ageDays <= 14  ? 0.5 :
                         ageDays <= 30  ? 0.2 : 0;
            }
            raw[repo] = score;
        }

        double max = raw.Values.DefaultIfEmpty(0).Max();

        return raw.ToDictionary(
            kv => kv.Key,
            kv => max > 0 ? (int)Math.Round(kv.Value / max * 100) : 0);
    }

    // ─── Focus minutes ────────────────────────────────────────────────────────

    /// <summary>
    /// Groups commits within the last 7 days into sessions (gap &lt; 90 min counts
    /// as the same session) and returns the total of all session durations in minutes.
    /// A session with a single commit contributes 0 minutes (no elapsed time).
    /// </summary>
    public static int CalculateFocusMinutes7d(List<DateTime> commits)
    {
        var now = DateTime.UtcNow;
        var recent = commits
            .Where(d => (now - d).TotalDays <= 7)
            .OrderBy(d => d)
            .ToList();

        if (recent.Count == 0) return 0;

        int totalMinutes = 0;
        var sessionStart = recent[0];
        var sessionEnd   = recent[0];

        for (int i = 1; i < recent.Count; i++)
        {
            if (recent[i] - sessionEnd < SessionGap)
            {
                sessionEnd = recent[i];
            }
            else
            {
                totalMinutes += (int)(sessionEnd - sessionStart).TotalMinutes;
                sessionStart = recent[i];
                sessionEnd   = recent[i];
            }
        }
        totalMinutes += (int)(sessionEnd - sessionStart).TotalMinutes;

        return totalMinutes;
    }

    // ─── Context switches ─────────────────────────────────────────────────────

    /// <summary>
    /// Counts the number of times each repo was abandoned for a different repo within
    /// a 2-hour window.  Operates on the global commit timeline across all repos.
    /// </summary>
    public static Dictionary<string, int> CalculateContextSwitches(
        List<RepoCommit> allCommits)
    {
        var sorted = allCommits.OrderBy(c => c.CommittedAt).ToList();

        var switches = allCommits
            .Select(c => c.RepoName)
            .Distinct(StringComparer.Ordinal)
            .ToDictionary(r => r, _ => 0, StringComparer.Ordinal);

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            var curr = sorted[i];
            var next = sorted[i + 1];

            if (!string.Equals(curr.RepoName, next.RepoName, StringComparison.Ordinal)
                && next.CommittedAt - curr.CommittedAt < ContextSwitchWindow)
            {
                switches[curr.RepoName]++;
            }
        }

        return switches;
    }

    // ─── Return rate ──────────────────────────────────────────────────────────

    /// <summary>
    /// Measures how reliably you return to a repo after leaving it.
    /// Formula: interior_returns / (interior_returns + terminal_departure_flag).
    /// Interior return = gap &gt; 24 h between two consecutive commits in this repo.
    /// Terminal departure = last commit was &gt; 24 h ago (you have not returned yet).
    /// Returns 1.0 when there are no gaps (steady engagement).
    /// </summary>
    public static double CalculateReturnRate(List<DateTime> commits)
    {
        if (commits.Count < 2) return 1.0;

        var sorted = commits.OrderBy(d => d).ToList();
        int interiorReturns = 0;

        for (int i = 0; i < sorted.Count - 1; i++)
            if (sorted[i + 1] - sorted[i] > ReturnGap)
                interiorReturns++;

        if (interiorReturns == 0) return 1.0;

        bool terminalDeparture = DateTime.UtcNow - sorted[^1] > ReturnGap;
        int totalDepartures = interiorReturns + (terminalDeparture ? 1 : 0);

        return Math.Round((double)interiorReturns / totalDepartures, 2);
    }

    // ─── Graph edges ──────────────────────────────────────────────────────────

    /// <summary>
    /// Builds temporal edges: every time commits in repo A and repo B fall within
    /// a 2-hour window, their edge weight increases by 0.1 (capped at 1.0).
    /// </summary>
    public static List<GraphEdge> BuildTemporalEdges(List<RepoCommit> allCommits)
    {
        var sorted = allCommits.OrderBy(c => c.CommittedAt).ToList();
        var weights = new Dictionary<(string, string), double>();

        for (int i = 0; i < sorted.Count; i++)
        {
            for (int j = i + 1; j < sorted.Count; j++)
            {
                var gap = sorted[j].CommittedAt - sorted[i].CommittedAt;
                if (gap > TemporalEdgeWindow) break;

                if (string.Equals(sorted[i].RepoName, sorted[j].RepoName,
                        StringComparison.Ordinal))
                    continue;

                var key = (sorted[i].RepoName, sorted[j].RepoName);
                weights[key] = Math.Min(1.0, weights.GetValueOrDefault(key) + 0.1);
            }
        }

        return weights
            .Select(kv => new GraphEdge(
                kv.Key.Item1, kv.Key.Item2,
                "temporal",
                Math.Round(kv.Value, 1)))
            .ToList();
    }

    /// <summary>
    /// Parses ProjectReference elements from a .csproj XML string.
    /// Returns repo names (lowercased file stem, without path or extension).
    /// Invalid XML is silently ignored.
    /// </summary>
    public static List<string> ParseProjectReferences(string csprojContent)
    {
        var refs = new List<string>();
        try
        {
            var doc = XDocument.Parse(csprojContent);
            foreach (var pr in doc.Descendants("ProjectReference"))
            {
                var include = pr.Attribute("Include")?.Value;
                if (string.IsNullOrWhiteSpace(include)) continue;
                refs.Add(Path.GetFileNameWithoutExtension(include).ToLowerInvariant());
            }
        }
        catch
        {
            // Malformed XML — skip silently.
        }
        return refs;
    }

    // ─── Rhythm ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Derives rhythm metrics from the complete commit timeline.
    /// </summary>
    /// <param name="allCommits">All commits across all repos.</param>
    public static (
        List<int> PeakHours,
        int AvgSessionMinutes,
        double FragmentationIndex,
        int DeepWorkStreakDays,
        string? LastQuietPeriod
    ) CalculateRhythm(List<RepoCommit> allCommits)
    {
        if (allCommits.Count == 0)
            return ([], 0, 0.0, 0, null);

        var now = DateTime.UtcNow;
        var recent7d  = allCommits.Where(c => (now - c.CommittedAt).TotalDays <= 7).ToList();
        var recent30d = allCommits.Where(c => (now - c.CommittedAt).TotalDays <= 30).ToList();

        // Peak commit hours (top 3) from all-time history.
        var peakHours = allCommits
            .GroupBy(c => c.CommittedAt.Hour)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .OrderBy(h => h)
            .ToList();

        // Average session minutes across all repos for the last 7 days.
        int avgSessionMinutes = CalculateAvgSessionMinutes(
            recent7d.Select(c => c.CommittedAt).ToList());

        // Fragmentation: distinct repos touched per day, 7-day average.
        double fragmentationIndex = 0;
        if (recent7d.Count > 0)
        {
            var reposPerDay = recent7d
                .GroupBy(c => c.CommittedAt.Date)
                .Select(g => g.Select(c => c.RepoName).Distinct().Count())
                .ToList();
            fragmentationIndex = reposPerDay.Count > 0
                ? Math.Round(reposPerDay.Average(), 1) : 0;
        }

        // Consecutive-day commit streak (most recent first).
        var commitDays = recent30d
            .Select(c => c.CommittedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        int streakDays = 0;
        if (commitDays.Count > 0 && (now.Date - commitDays[0]).TotalDays <= 1)
        {
            streakDays = 1;
            for (int i = 1; i < commitDays.Count; i++)
            {
                if ((commitDays[i - 1] - commitDays[i]).TotalDays == 1)
                    streakDays++;
                else
                    break;
            }
        }

        // Most recent quiet day (no commits) in the last 30 days.
        var activeSet = commitDays.ToHashSet();
        var lastQuietPeriod = Enumerable
            .Range(0, 30)
            .Select(i => now.Date.AddDays(-i))
            .FirstOrDefault(d => !activeSet.Contains(d))
            is DateTime quiet && quiet != default
                ? quiet.ToString("yyyy-MM-dd")
                : null;

        return (peakHours, avgSessionMinutes, fragmentationIndex, streakDays, lastQuietPeriod);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private static int CalculateAvgSessionMinutes(List<DateTime> timestamps)
    {
        var sorted = timestamps.OrderBy(d => d).ToList();
        if (sorted.Count == 0) return 0;

        int totalMinutes = 0;
        int sessionCount = 1;
        var sessionStart = sorted[0];
        var sessionEnd   = sorted[0];

        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] - sessionEnd < SessionGap)
            {
                sessionEnd = sorted[i];
            }
            else
            {
                totalMinutes += (int)(sessionEnd - sessionStart).TotalMinutes;
                sessionStart  = sorted[i];
                sessionEnd    = sorted[i];
                sessionCount++;
            }
        }
        totalMinutes += (int)(sessionEnd - sessionStart).TotalMinutes;

        return totalMinutes / sessionCount;
    }
}
