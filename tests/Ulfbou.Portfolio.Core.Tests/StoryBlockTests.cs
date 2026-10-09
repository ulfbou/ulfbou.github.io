using System.Text.Json;
using FluentAssertions;
using Ulfbou.Portfolio.Core;
using Xunit;

namespace Ulfbou.Portfolio.Core.Tests;

public sealed class StoryBlockTests
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
    public void Every_admitted_type_has_renderer_coverage()
    {
        Enum.GetValues<StoryBlockType>()
            .Should()
            .OnlyContain(type => StoryBlockContract.HasRenderer(type));
    }

    [Fact]
    public void Approved_project_demonstrates_multiple_block_types()
    {
        Load().Projects.Should().Contain(
            project =>
                project.Blocks
                    .Select(block => block.Type)
                    .Distinct()
                    .Count() >= 5);
    }

    [Fact]
    public void Unknown_block_type_is_rejected()
    {
        var portfolio = Load();
        var project = portfolio.Projects[0];
        var invalid = project.Blocks[0] with
        {
            Type = (StoryBlockType)999
        };

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects =
                        [
                            project with
                            {
                                Blocks = [invalid]
                            }
                        ]
                    }))
            .Code.Should()
            .Be("VAL-BLOCK-TYPE");
    }

    [Fact]
    public void Image_without_alternative_is_rejected()
    {
        var portfolio = Load();
        var project = portfolio.Projects[0];
        var image = Block(
            StoryBlockType.Image,
            """
            {
              "source": "images/example.svg",
              "alternative": ""
            }
            """);

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects =
                        [
                            project with
                            {
                                Blocks = [image]
                            }
                        ]
                    }))
            .Code.Should()
            .Be("VAL-BLOCK-PAYLOAD");
    }

    [Fact]
    public void Quote_without_attribution_is_rejected()
    {
        var portfolio = Load();
        var project = portfolio.Projects[0];
        var quote = Block(
            StoryBlockType.Quote,
            """
            {
              "text": "Bounded quotation.",
              "attribution": ""
            }
            """);

        Failure(
                () => ContentValidator.Validate(
                    portfolio with
                    {
                        Projects =
                        [
                            project with
                            {
                                Blocks = [quote]
                            }
                        ]
                    }))
            .Code.Should()
            .Be("VAL-BLOCK-PAYLOAD");
    }

    [Fact]
    public void Reflection_and_evidence_are_distinct()
    {
        StoryBlockType.Reflection.Should()
            .NotBe(StoryBlockType.Evidence);
    }

    [Fact]
    public void Compiled_blocks_preserve_order()
    {
        var compiled = new CompiledPortfolioGenerator()
            .Compile(Load());

        var blocks = compiled.Projects
            .Single(project => project.Id == "dx-domain")
            .Blocks;

        blocks.Select(block => block.Order)
            .Should()
            .BeInAscendingOrder();
    }

    [Fact]
    public void Home_preserves_filtering_while_retaining_printable_details()
    {
        var home = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "repository",
                "Home.razor"));

        home.Should().Contain(
            "@foreach (var project in data.Projects)");
        home.Should().Contain(
            "ProjectMatchesFacet(project)");
        home.Should().Contain(
            "collapsed-detail");
        home.Should().Contain(
            "@if (project.Blocks.Count > 0)");
        home.Should().Contain(
            "narrative compatibility-detail");
        home.Should().NotContain(
            "@if (openId == project.Id)");
    }

    [Fact]
    public void Print_includes_all_projects_and_details()
    {
        var css = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "repository",
                "story-blocks.css"));

        css.Should().Contain(
            ".filtered-out");
        css.Should().Contain(
            "display: block !important;");
        css.Should().Contain(
            ".collapsed-detail");
        css.Should().Contain(
            "display: grid !important;");
        css.Should().Contain(
            "footer a");
        css.Should().Contain(
            "white-space: pre-wrap;");
    }

    [Fact]
    public void Site_loads_story_block_styles_after_base_styles()
    {
        var index = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "repository",
                "index.html"));

        var accessibilityStyles = index.IndexOf(
            "css/accessibility.css",
            StringComparison.Ordinal);
        var storyBlockStyles = index.IndexOf(
            "css/story-blocks.css",
            StringComparison.Ordinal);

        accessibilityStyles.Should().BeGreaterThanOrEqualTo(0);
        storyBlockStyles.Should().BeGreaterThan(accessibilityStyles);
    }

    private static StoryBlockDefinition Block(
        StoryBlockType type,
        string payload) =>
        new(
            0,
            "test.block",
            type,
            "Test block",
            ReadingDepth.Detailed,
            new Provenance("test", "Test provenance."),
            new PublicationMetadata(
                PublicationState.Published,
                Visibility.Public,
                ReadingDepth.Detailed),
            JsonDocument.Parse(payload).RootElement.Clone());
}
