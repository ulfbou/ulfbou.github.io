using System.Text.Json;
using FluentAssertions;
using Ulfbou.Portfolio.Core;
using Xunit;

namespace Ulfbou.Portfolio.Core.Tests;

public sealed class PortfolioDataTests
{
    [Fact]
    public void Representative_data_satisfies_phase_one_contract()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "portfolio.json");
        var data = JsonSerializer.Deserialize<PortfolioData>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        data.Should().NotBeNull();
        data!.Projects.Should().HaveCountGreaterThanOrEqualTo(3);
        data.Projects.Select(project => project.Id).Should().OnlyHaveUniqueItems();
        data.Projects.Should().OnlyContain(project => project.Facets.All(data.Facets.Contains));
        data.Projects.Should().OnlyContain(project => project.Limitations.Count > 0);
        data.Projects.Should().OnlyContain(project => project.Proof.Count > 0);
    }
}
