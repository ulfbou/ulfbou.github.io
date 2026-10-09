namespace Ulfbou.Portfolio.Core;

public enum Visibility
{
    Public,
    Candidate,
    Private
}

public enum ReadingDepth
{
    Summary,
    Detailed
}

public enum PublicationState
{
    Draft,
    Approved,
    Published
}

public sealed record Provenance(
    string SourceId,
    string Description);

public sealed record PublicationMetadata(
    PublicationState State,
    Visibility Visibility,
    ReadingDepth ReadingDepth);

public sealed record LinkDefinition(
    string Label,
    string Url);

public sealed record EvidencePointer(
    string Label,
    string Value,
    Visibility Visibility);

public sealed record DemoDefinition(
    string Kind,
    string Caption,
    IReadOnlyList<string> Lines);

public sealed record ReflectionDefinition(
    string Summary);

public sealed record FacetDefinition(
    string Id,
    string Name,
    Provenance Provenance,
    PublicationMetadata Publication);

public sealed record TechnologyDefinition(
    string Id,
    string Name,
    Provenance Provenance,
    PublicationMetadata Publication);

public sealed record ApprovedProfile(
    string Name,
    string Signal,
    string Introduction,
    string Direction,
    IReadOnlyList<LinkDefinition> Links,
    Provenance Provenance,
    PublicationMetadata Publication);

public sealed record ApprovedProject(
    int Order,
    string Id,
    string Title,
    string ShortTitle,
    string Proposition,
    string Story,
    string Contribution,
    ReflectionDefinition Reflection,
    string Status,
    string Accent,
    IReadOnlyList<string> FacetIds,
    IReadOnlyList<string> TechnologyIds,
    IReadOnlyList<EvidencePointer> Proof,
    DemoDefinition Demo,
    IReadOnlyList<string> Limitations,
    Provenance Provenance,
    PublicationMetadata Publication)
{
    public IReadOnlyList<StoryBlockDefinition> Blocks { get; init; } = [];
}

public sealed record ApprovedPortfolio(
    ApprovedProfile Profile,
    IReadOnlyList<FacetDefinition> Facets,
    IReadOnlyList<TechnologyDefinition> Technologies,
    IReadOnlyList<ApprovedProject> Projects);
