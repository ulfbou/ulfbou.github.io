
using System.Diagnostics.CodeAnalysis;

namespace Ulfbou.Portfolio.Core;

public static class ContentValidator
{
    public static void Validate(ApprovedPortfolio? portfolio)
    {
        if (portfolio is null)
        {
            Fail(
                "VAL-DOCUMENT",
                "portfolio",
                "Approved portfolio is null.");
        }

        if (portfolio.Profile is null)
        {
            Fail(
                "VAL-DOCUMENT",
                "profile",
                "Required profile document is null.");
        }

        if (portfolio.Facets is null)
        {
            Fail(
                "VAL-COLLECTION",
                "facets",
                "Required collection is null.");
        }

        if (portfolio.Technologies is null)
        {
            Fail(
                "VAL-COLLECTION",
                "technologies",
                "Required collection is null.");
        }

        if (portfolio.Projects is null)
        {
            Fail(
                "VAL-COLLECTION",
                "projects",
                "Required collection is null.");
        }

        ValidateProfile(portfolio.Profile);
        ValidateUniqueIds(
            portfolio.Facets.Select(value => value.Id),
            "facets");
        ValidateUniqueIds(
            portfolio.Technologies.Select(value => value.Id),
            "technologies");
        ValidateUniqueIds(
            portfolio.Projects.Select(value => value.Id),
            "projects");
        ValidateUniqueOrders(portfolio.Projects);

        var facets = new Dictionary<string, FacetDefinition>(
            StringComparer.Ordinal);

        foreach (var facet in portfolio.Facets)
        {
            var path = $"facets/{facet.Id}";
            Required(facet.Id, $"{path}.id");
            Required(facet.Name, $"{path}.name");
            ValidateProvenance(facet.Provenance, path);
            ValidatePublication(facet.Publication, path);
            facets.Add(facet.Id, facet);
        }

        var technologies =
            new Dictionary<string, TechnologyDefinition>(
                StringComparer.Ordinal);

        foreach (var technology in portfolio.Technologies)
        {
            var path = $"technologies/{technology.Id}";
            Required(technology.Id, $"{path}.id");
            Required(technology.Name, $"{path}.name");
            ValidateProvenance(technology.Provenance, path);
            ValidatePublication(technology.Publication, path);
            technologies.Add(technology.Id, technology);
        }

        foreach (var project in portfolio.Projects)
        {
            ValidateProject(
                project,
                facets,
                technologies);
        }
    }

    private static void ValidateProfile(ApprovedProfile profile)
    {
        const string path = "profile";

        ValidateProvenance(profile.Provenance, path);
        ValidatePublication(profile.Publication, path);

        if (
            profile.Publication.State
                != PublicationState.Published
            || profile.Publication.Visibility
                != Visibility.Public)
        {
            Fail(
                "VAL-PROFILE-PUBLICATION",
                path,
                "Public compilation requires a published public profile.");
        }

        Required(profile.Name, $"{path}.name");
        Required(profile.Signal, $"{path}.signal");
        Required(profile.Introduction, $"{path}.introduction");
        Required(profile.Direction, $"{path}.direction");

        if (profile.Links is null)
        {
            Fail(
                "VAL-COLLECTION",
                $"{path}.links",
                "Required collection is null.");
        }

        for (var index = 0; index < profile.Links.Count; index++)
        {
            var link = profile.Links[index];
            var linkPath = $"{path}.links[{index}]";

            if (link is null)
            {
                Fail(
                    "VAL-DOCUMENT",
                    linkPath,
                    "Link is null.");
            }

            Required(link.Label, $"{linkPath}.label");
            Required(link.Url, $"{linkPath}.url");

            if (
                !Uri.TryCreate(
                    link.Url,
                    UriKind.Absolute,
                    out var uri)
                || (
                    uri.Scheme != Uri.UriSchemeHttp
                    && uri.Scheme != Uri.UriSchemeHttps))
            {
                Fail(
                    "VAL-URL",
                    $"{linkPath}.url",
                    $"Invalid absolute HTTP or HTTPS URL '{link.Url}'.");
            }
        }
    }

    private static void ValidateProject(
        ApprovedProject project,
        IReadOnlyDictionary<string, FacetDefinition> facets,
        IReadOnlyDictionary<
            string,
            TechnologyDefinition> technologies)
    {
        var path = $"projects/{project.Id}";

        Required(project.Id, $"{path}.id");
        ValidateProvenance(project.Provenance, path);
        ValidatePublication(project.Publication, path);

        RequireCollection(project.FacetIds, $"{path}.facetIds");
        RequireCollection(
            project.TechnologyIds,
            $"{path}.technologyIds");
        RequireCollection(project.Proof, $"{path}.proof");
        RequireCollection(
            project.Limitations,
            $"{path}.limitations");

        if (project.Reflection is null)
        {
            Fail(
                "VAL-DOCUMENT",
                $"{path}.reflection",
                "Required reflection is null.");
        }

        if (project.Demo is null)
        {
            Fail(
                "VAL-DOCUMENT",
                $"{path}.demo",
                "Required demo is null.");
        }

        RequireCollection(
            project.Demo.Lines,
            $"{path}.demo.lines");

        var publishable =
            project.Publication.State
                == PublicationState.Published
            && project.Publication.Visibility
                == Visibility.Public;

        if (publishable)
        {
            Required(project.Title, $"{path}.title");
            Required(project.ShortTitle, $"{path}.shortTitle");
            Required(
                project.Proposition,
                $"{path}.proposition");
            Required(project.Story, $"{path}.story");
            Required(
                project.Contribution,
                $"{path}.contribution");
            Required(
                project.Reflection.Summary,
                $"{path}.reflection.summary");
            Required(project.Status, $"{path}.status");
            Required(project.Accent, $"{path}.accent");
            Required(project.Demo.Kind, $"{path}.demo.kind");
            Required(
                project.Demo.Caption,
                $"{path}.demo.caption");

            if (project.Limitations.Count == 0)
            {
                Fail(
                    "VAL-LIMITATIONS",
                    $"{path}.limitations",
                    "Published public project has no limitations.");
            }

            if (project.Proof.Count == 0)
            {
                Fail(
                    "VAL-PROOF",
                    $"{path}.proof",
                    "Published public project has no proof pointers.");
            }
        }

        ValidateReferences(
            project.FacetIds,
            $"{path}.facetIds",
            "facet",
            reference =>
            {
                if (!facets.TryGetValue(reference, out var facet))
                {
                    return false;
                }

                if (
                    publishable
                    && (
                        facet.Publication.State
                            != PublicationState.Published
                        || facet.Publication.Visibility
                            != Visibility.Public))
                {
                    Fail(
                        "VAL-VISIBILITY",
                        $"{path}.facetIds",
                        $"Published public project references non-public facet '{reference}'.");
                }

                return true;
            });

        ValidateReferences(
            project.TechnologyIds,
            $"{path}.technologyIds",
            "technology",
            reference =>
            {
                if (
                    !technologies.TryGetValue(
                        reference,
                        out var technology))
                {
                    return false;
                }

                if (
                    publishable
                    && (
                        technology.Publication.State
                            != PublicationState.Published
                        || technology.Publication.Visibility
                            != Visibility.Public))
                {
                    Fail(
                        "VAL-VISIBILITY",
                        $"{path}.technologyIds",
                        $"Published public project references non-public technology '{reference}'.");
                }

                return true;
            });

        for (var index = 0; index < project.Proof.Count; index++)
        {
            var pointer = project.Proof[index];
            var pointerPath = $"{path}.proof[{index}]";

            if (pointer is null)
            {
                Fail(
                    "VAL-DOCUMENT",
                    pointerPath,
                    "Proof pointer is null.");
            }

            Required(pointer.Label, $"{pointerPath}.label");
            Required(pointer.Value, $"{pointerPath}.value");

            if (!Enum.IsDefined(pointer.Visibility))
            {
                Fail(
                    "VAL-ENUM",
                    $"{pointerPath}.visibility",
                    "Visibility is undefined.");
            }
        }

        for (
            var index = 0;
            index < project.Limitations.Count;
            index++)
        {
            Required(
                project.Limitations[index],
                $"{path}.limitations[{index}]");
        }

        for (
            var index = 0;
            index < project.Demo.Lines.Count;
            index++)
        {
            if (project.Demo.Lines[index] is null)
            {
                Fail(
                    "VAL-STRING",
                    $"{path}.demo.lines[{index}]",
                    "Demo line is null.");
            }
        }
    }

    private static void ValidateUniqueIds(
        IEnumerable<string> values,
        string path)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var value in values)
        {
            Required(value, path);

            if (!seen.Add(value))
            {
                Fail(
                    "VAL-DUPLICATE-ID",
                    path,
                    $"Duplicate ID '{value}'.");
            }
        }
    }

    private static void ValidateUniqueOrders(
        IEnumerable<ApprovedProject> projects)
    {
        var seen = new HashSet<int>();

        foreach (var project in projects)
        {
            if (project.Order < 0)
            {
                Fail(
                    "VAL-ORDER",
                    $"projects/{project.Id}.order",
                    "Project order must not be negative.");
            }

            if (!seen.Add(project.Order))
            {
                Fail(
                    "VAL-DUPLICATE-ORDER",
                    "projects",
                    $"Duplicate project order '{project.Order}'.");
            }
        }
    }

    private static void ValidateReferences(
        IReadOnlyList<string> references,
        string path,
        string kind,
        Func<string, bool> exists)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var reference in references)
        {
            Required(reference, path);

            if (!seen.Add(reference))
            {
                Fail(
                    "VAL-DUPLICATE-REFERENCE",
                    path,
                    $"Duplicate {kind} reference '{reference}'.");
            }

            if (!exists(reference))
            {
                Fail(
                    "VAL-REFERENCE",
                    path,
                    $"Unknown {kind} reference '{reference}'.");
            }
        }
    }

    private static void ValidateProvenance(
        Provenance? provenance,
        string path)
    {
        if (provenance is null)
        {
            Fail(
                "VAL-PROVENANCE",
                $"{path}.provenance",
                "Provenance is null.");
        }

        Required(
            provenance.SourceId,
            $"{path}.provenance.sourceId");
        Required(
            provenance.Description,
            $"{path}.provenance.description");
    }

    private static void ValidatePublication(
        PublicationMetadata? publication,
        string path)
    {
        if (publication is null)
        {
            Fail(
                "VAL-METADATA",
                $"{path}.publication",
                "Publication metadata is null.");
        }

        if (
            !Enum.IsDefined(publication.State)
            || !Enum.IsDefined(publication.Visibility)
            || !Enum.IsDefined(publication.ReadingDepth))
        {
            Fail(
                "VAL-METADATA",
                $"{path}.publication",
                "Publication metadata contains an undefined enum value.");
        }

        if (
            publication.State == PublicationState.Published
            && publication.Visibility != Visibility.Public)
        {
            Fail(
                "VAL-METADATA",
                $"{path}.publication",
                "Published records must be public.");
        }
    }

    private static void RequireCollection<T>(
        [NotNull] IReadOnlyList<T>? values,
        string path)
    {
        if (values is null)
        {
            Fail(
                "VAL-COLLECTION",
                path,
                "Required collection is null.");
        }
    }

    private static void Required(string? value, string path)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Fail(
                "VAL-STRING",
                path,
                "Required public string is empty.");
        }
    }

    [DoesNotReturn]
    private static void Fail(
        string code,
        string path,
        string reason)
    {
        throw new ContentValidationException(
            code,
            path,
            reason);
    }
}
