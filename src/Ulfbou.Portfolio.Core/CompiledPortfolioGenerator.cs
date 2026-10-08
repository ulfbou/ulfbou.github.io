
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Ulfbou.Portfolio.Core;

public sealed class CompiledPortfolioGenerator
{
    private static readonly JsonSerializerOptions Options =
        new(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

    public PortfolioData Compile(ApprovedPortfolio approved)
    {
        ContentValidator.Validate(approved);

        var facets = approved.Facets.ToDictionary(
            value => value.Id,
            StringComparer.Ordinal);

        var technologies = approved.Technologies.ToDictionary(
            value => value.Id,
            StringComparer.Ordinal);

        var profile = new Profile(
            approved.Profile.Name,
            approved.Profile.Signal,
            approved.Profile.Introduction,
            approved.Profile.Direction,
            approved.Profile.Links
                .Select(
                    link => new Link(
                        link.Label,
                        link.Url))
                .ToArray());

        var projects = approved.Projects
            .Where(
                project =>
                    project.Publication.State
                        == PublicationState.Published
                    && project.Publication.Visibility
                        == Visibility.Public)
            .OrderBy(project => project.Order)
            .Select(
                project => new Project(
                    project.Id,
                    project.Title,
                    project.ShortTitle,
                    project.Proposition,
                    project.Story,
                    project.Contribution,
                    project.Reflection.Summary,
                    project.Status,
                    project.Accent,
                    project.FacetIds
                        .Select(id => facets[id].Name)
                        .ToArray(),
                    project.TechnologyIds
                        .Select(id => technologies[id].Name)
                        .ToArray(),
                    project.Proof
                        .Where(
                            pointer =>
                                pointer.Visibility
                                    == Visibility.Public)
                        .Select(
                            pointer => new Proof(
                                pointer.Label,
                                pointer.Value))
                        .ToArray(),
                    new Demo(
                        project.Demo.Kind,
                        project.Demo.Caption,
                        project.Demo.Lines.ToArray()),
                    project.Limitations.ToArray()))
            .ToArray();

        var publicFacets = approved.Facets
            .Where(
                facet =>
                    facet.Publication.State
                        == PublicationState.Published
                    && facet.Publication.Visibility
                        == Visibility.Public)
            .Select(facet => facet.Name)
            .ToArray();

        return new PortfolioData(
            profile,
            projects,
            publicFacets);
    }

    public byte[] CompileBytes(ApprovedPortfolio approved)
    {
        var compiled = Compile(approved);

        var projection = new
        {
            profile = compiled.Profile,
            facets = compiled.Facets,
            projects = compiled.Projects
        };

        var json = JsonSerializer.Serialize(
            projection,
            Options);

        var normalized =
            json
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\r", "\n", StringComparison.Ordinal)
                .TrimEnd('\n')
            + "\n";

        return new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false)
            .GetBytes(normalized);
    }

    public string CompileJson(ApprovedPortfolio approved)
    {
        return Encoding.UTF8.GetString(
            CompileBytes(approved));
    }
}
