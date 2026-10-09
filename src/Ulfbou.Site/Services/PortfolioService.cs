using System.Net.Http.Json;
using Ulfbou.Portfolio.Core;

namespace Ulfbou.Site.Services;

public sealed class PortfolioService(HttpClient http)
{
    private PortfolioData? cached;

    public async Task<PortfolioData> LoadAsync() =>
        cached ??= await http.GetFromJsonAsync<PortfolioData>("data/generated/portfolio.json")
            ?? throw new InvalidOperationException("Portfolio data is unavailable.");
}
