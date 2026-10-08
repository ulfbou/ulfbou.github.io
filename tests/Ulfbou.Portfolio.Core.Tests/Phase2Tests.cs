
using System.Text.Json;
using FluentAssertions;
using Ulfbou.Portfolio.Core;
using Xunit;

namespace Ulfbou.Portfolio.Core.Tests;

public sealed class Phase2Tests
{
    private static string ContentDirectory =>
        Path.Combine(AppContext.BaseDirectory, "content");

    private static ApprovedPortfolio Load() =>
        new ApprovedContentLoader().Load(ContentDirectory);

    private static ContentValidationException Failure(
        Action action) =>
        action.Should()
            .Throw<ContentValidationException>()
            .Which;

    [Fact]
    public void Complete_source_loads_in_controlling_order()
    {
        var portfolio = Load();

        portfolio.Projects
            .Select(project => project.Id)
            .Should()
            .Equal(
                "dx-domain",
                "dx-cli",
                "mold-first-bloom",
                "verdant",
                "zentient-results",
                "collab",
                "prototype-knowledge");
    }

    [Fact]
    public void Missing_required_source_is_rejected()
    {
        using var content = TemporaryContent.Copy();

        File.Delete(
            Path.Combine(content.Path, "profile.json"));

        Failure(
                () => new ApprovedContentLoader()
                    .Load(content.Path))
            .Code.Should()
            .Be("LOAD-FILE");
    }

    [Fact]
    public void Malformed_json_is_rejected()
    {
        using var content = TemporaryContent.Copy();

        File.WriteAllText(
            Path.Combine(content.Path, "profile.json"),
            "{");

        Failure(
                () => new ApprovedContentLoader()
                    .Load(content.Path))
            .Code.Should()
            .Be("LOAD-JSON");
    }

    [Fact]
    public void Duplicate_ids_are_rejected()
    {
        var portfolio = Load();

        var duplicate = portfolio with
        {
            Technologies = portfolio.Technologies
                .Concat([portfolio.Technologies[0]])
                .ToArray()
        };

        Failure(() => ContentValidator.Validate(duplicate))
            .Code.Should()
            .Be("VAL-DUPLICATE-ID");
    }

    [Fact]
    public void Unknown_references_are_rejected()
    {
        var portfolio = Load();

        var project = portfolio.Projects[0] with
        {
            FacetIds = ["missing"]
        };

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects = [project]
                    }))
            .Code.Should()
            .Be("VAL-REFERENCE");
    }

    [Fact]
    public void Duplicate_references_are_rejected()
    {
        var portfolio = Load();
        var value = portfolio.Projects[0].FacetIds[0];

        var project = portfolio.Projects[0] with
        {
            FacetIds = [value, value]
        };

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects = [project]
                    }))
            .Code.Should()
            .Be("VAL-DUPLICATE-REFERENCE");
    }

    [Fact]
    public void Invalid_publication_metadata_is_rejected()
    {
        var portfolio = Load();

        var project = portfolio.Projects[0] with
        {
            Publication = new PublicationMetadata(
                PublicationState.Published,
                Visibility.Private,
                ReadingDepth.Detailed)
        };

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects = [project]
                    }))
            .Code.Should()
            .Be("VAL-METADATA");
    }

    [Fact]
    public void Non_public_definition_for_public_project_is_rejected()
    {
        var portfolio = Load();

        var privateFacet = portfolio.Facets[0] with
        {
            Publication = new PublicationMetadata(
                PublicationState.Approved,
                Visibility.Private,
                ReadingDepth.Detailed)
        };

        var facets = new[]
        {
            privateFacet
        }.Concat(portfolio.Facets.Skip(1)).ToArray();

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Facets = facets
                    }))
            .Code.Should()
            .Be("VAL-VISIBILITY");
    }

    [Theory]
    [InlineData(
        PublicationState.Draft,
        Visibility.Candidate)]
    [InlineData(
        PublicationState.Approved,
        Visibility.Private)]
    public void Non_public_projects_are_excluded(
        PublicationState state,
        Visibility visibility)
    {
        var portfolio = Load();

        var project = portfolio.Projects[0] with
        {
            Publication = new PublicationMetadata(
                state,
                visibility,
                ReadingDepth.Detailed)
        };

        var candidate = portfolio with
        {
            Projects = [project]
        };

        new CompiledPortfolioGenerator()
            .Compile(candidate)
            .Projects.Should()
            .BeEmpty();
    }

    [Theory]
    [InlineData(
        PublicationState.Draft,
        Visibility.Candidate)]
    [InlineData(
        PublicationState.Approved,
        Visibility.Private)]
    public void Non_public_profile_blocks_compilation(
        PublicationState state,
        Visibility visibility)
    {
        var portfolio = Load();

        var candidate = portfolio with
        {
            Profile = portfolio.Profile with
            {
                Publication = new PublicationMetadata(
                    state,
                    visibility,
                    ReadingDepth.Detailed)
            }
        };

        Failure(
                () => new CompiledPortfolioGenerator()
                    .Compile(candidate))
            .Code.Should()
            .Be("VAL-PROFILE-PUBLICATION");
    }

    [Fact]
    public void Incomplete_public_record_is_rejected()
    {
        var portfolio = Load();

        var project = portfolio.Projects[0] with
        {
            Title = ""
        };

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects = [project]
                    }))
            .Code.Should()
            .Be("VAL-STRING");
    }

    [Fact]
    public void Empty_limitations_are_rejected()
    {
        var portfolio = Load();

        var project = portfolio.Projects[0] with
        {
            Limitations = []
        };

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects = [project]
                    }))
            .Code.Should()
            .Be("VAL-LIMITATIONS");
    }

    [Fact]
    public void Empty_proof_is_rejected()
    {
        var portfolio = Load();

        var project = portfolio.Projects[0] with
        {
            Proof = []
        };

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects = [project]
                    }))
            .Code.Should()
            .Be("VAL-PROOF");
    }

    [Fact]
    public void Invalid_link_url_is_rejected()
    {
        var portfolio = Load();

        var candidate = portfolio with
        {
            Profile = portfolio.Profile with
            {
                Links =
                [
                    new LinkDefinition(
                        "Invalid",
                        "relative/path")
                ]
            }
        };

        Failure(() => ContentValidator.Validate(candidate))
            .Code.Should()
            .Be("VAL-URL");
    }

    [Fact]
    public void Generated_bytes_are_reproducible()
    {
        var loader = new ApprovedContentLoader();
        var compiler = new CompiledPortfolioGenerator();

        var first = compiler.CompileBytes(
            loader.Load(ContentDirectory));

        var second = compiler.CompileBytes(
            loader.Load(ContentDirectory));

        second.Should().Equal(first);

        File.ReadAllBytes(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "generated",
                    "portfolio.json"))
            .Should()
            .Equal(first);
    }

    [Fact]
    public void Generated_bytes_end_with_exactly_one_lf()
    {
        var bytes = new CompiledPortfolioGenerator()
            .CompileBytes(Load());

        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be((byte)'\n');
        bytes[^2].Should().NotBe((byte)'\n');
        bytes
            .AsSpan()
            .StartsWith(new byte[] { 0xEF, 0xBB, 0xBF })
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Legacy_and_generated_complete_graphs_are_equivalent()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var legacy = JsonSerializer.Deserialize<PortfolioData>(
            File.ReadAllBytes(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "legacy",
                    "portfolio.json")),
            options);
        var generated =
            JsonSerializer.Deserialize<PortfolioData>(
                File.ReadAllBytes(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "generated",
                        "portfolio.json")),
                options);

        legacy.Should().NotBeNull();
        generated.Should().NotBeNull();

        var legacyGraph = JsonSerializer.SerializeToElement(
            legacy,
            options);
        var generatedGraph = JsonSerializer.SerializeToElement(
            generated,
            options);

        JsonElement.DeepEquals(
                generatedGraph,
                legacyGraph)
            .Should()
            .BeTrue(
                "the independently deserialized complete ordered object graphs must remain semantically equivalent");

        generated!.Projects.Should().HaveCount(7);
    }

    private sealed class TemporaryContent : IDisposable
    {
        private TemporaryContent(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryContent Copy()
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                Guid.NewGuid().ToString("N"));

            CopyDirectory(ContentDirectory, path);

            return new TemporaryContent(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(
                    Path,
                    recursive: true);
            }
        }

        private static void CopyDirectory(
            string source,
            string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (var file in Directory.GetFiles(source))
            {
                File.Copy(
                    file,
                    System.IO.Path.Combine(
                        destination,
                        System.IO.Path.GetFileName(file)));
            }

            foreach (
                var directory in Directory.GetDirectories(source))
            {
                CopyDirectory(
                    directory,
                    System.IO.Path.Combine(
                        destination,
                        System.IO.Path.GetFileName(directory)));
            }
        }
    }
}
