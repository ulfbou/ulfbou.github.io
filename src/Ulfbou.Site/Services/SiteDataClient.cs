using System.Net.Http.Json;
using Ulfbou.Site.Models;

namespace Ulfbou.Site.Services;

public class SiteDataClient
{
    private readonly HttpClient _http;

    public SiteDataClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<NowModel?> GetNowAsync()
    {
        try { return await _http.GetFromJsonAsync<NowModel>("now.json"); }
        catch { return null; }
    }

    public async Task<MetricsModel?> GetMetricsAsync()
    {
        try { return await _http.GetFromJsonAsync<MetricsModel>("data/metrics.json"); }
        catch { return null; }
    }

    public async Task<List<RepoModel>?> GetReposAsync()
    {
        try { return await _http.GetFromJsonAsync<List<RepoModel>>("data/repos.json"); }
        catch { return null; }
    }

    public async Task<GraphModel?> GetGraphAsync()
    {
        try { return await _http.GetFromJsonAsync<GraphModel>("data/graph.json"); }
        catch { return null; }
    }

    public async Task<string?> GetWeeklyMarkdownAsync(string path)
    {
        try { return await _http.GetStringAsync(path.TrimStart('/')); }
        catch { return null; }
    }
}
