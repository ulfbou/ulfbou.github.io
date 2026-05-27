# SoloPulse.Collector

A .NET 10 console app that runs in GitHub Actions, queries the GitHub GraphQL API
for a given user, calculates solo-developer metrics, and writes three static JSON
files consumed by the Blazor WASM front-end.

---

## Purpose

The Blazor site at `src/Ulfbou.Site` is a living logbook with no backend.
All dynamic data comes from JSON files committed to the repo by a weekly
GitHub Action that runs this collector.  The collector is the single source
of truth for `repos.json`, `graph.json`, and `rhythm.json`.

---

## Required environment variables

| Variable       | Required | Default   | Description                            |
|----------------|----------|-----------|----------------------------------------|
| `GITHUB_TOKEN` | **yes**  | —         | PAT or `secrets.GITHUB_TOKEN`; needs `repo:read` scope |
| `TARGET_USER`  | no       | `ulfbou`  | GitHub login to collect data for       |
| `OUTPUT_DIR`   | no       | auto      | Absolute path for output files; auto-detected relative to binary when omitted |

---

## Run locally

```bash
# From the solution root
export GITHUB_TOKEN=ghp_your_token_here
export TARGET_USER=ulfbou

dotnet run --project tools/SoloPulse.Collector
```

Expected console output:

```
Collected 8 repos, 12 edges, 340 minutes focus, written to "…/src/Ulfbou.Site/wwwroot/data"
```

---

## Run in GitHub Actions

```yaml
- name: Run SoloPulse Collector
  run: dotnet run --project tools/SoloPulse.Collector
  env:
	GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
	TARGET_USER: ulfbou
```

---

## Output files

All files are written to `src/Ulfbou.Site/wwwroot/data/` and include
`"schemaVersion": "1"` and `"generatedAt"` (UTC ISO 8601).

### repos.json
Per-repo solo metrics.  Key fields:

| Field               | Type   | Notes |
|---------------------|--------|-------|
| `momentum`          | 0–100  | `commits_7d×1.0 + commits_8-14d×0.5 + commits_15-30d×0.2`, normalised |
| `focusMinutes7d`    | int    | Commits in last 7 days clustered into sessions (gap < 90 min); sum of session durations |
| `contextSwitchesOut`| int    | Times you left this repo for another within a 2-hour window |
| `returnRate`        | 0–1    | Interior 24h+ gaps ÷ (interior + terminal departure); 1.0 = always returned |
| `openLoops`         | int    | Open PRs + open issues |

### graph.json
Dependency and temporal relationship graph.  Edge types:

| Type         | Meaning |
|--------------|---------|
| `dependency` | `.csproj` `ProjectReference` detected (top 5 repos only) |
| `temporal`   | Commits to both repos occurred within 2 hours of each other |

### rhythm.json
Coding rhythm metrics:

| Field                | Notes |
|----------------------|-------|
| `peakHours`          | Top 3 commit hours (UTC) across all history |
| `avgSessionMinutes`  | Average session length in last 7 days |
| `fragmentationIndex` | Average distinct repos per day (7-day window) |
| `deepWorkStreakDays` | Consecutive days with at least one commit (last 30 days) |
| `lastQuietPeriod`    | Most recent day with no commits in last 30 days |

---

## Package notes

The task spec references `Octokit.GraphQL 8.0.0`.  That package version does not
currently exist on NuGet; the implementation uses `HttpClient` with raw GraphQL
POST requests, which is functionally equivalent and compatible with .NET 10 out
of the box.  If a suitable `Octokit.GraphQL` release becomes available, the
`GitHubClient.cs` queries can be migrated to its LINQ-based API by adding:

```xml
<PackageReference Include="Octokit.GraphQL" Version="x.y.z" />
```

---

## Verify output

```bash
# Valid JSON?
jq . src/Ulfbou.Site/wwwroot/data/repos.json

# Number of repos collected
jq '.repos | length' src/Ulfbou.Site/wwwroot/data/repos.json

# Schema version present?
jq '.schemaVersion' src/Ulfbou.Site/wwwroot/data/rhythm.json
```
