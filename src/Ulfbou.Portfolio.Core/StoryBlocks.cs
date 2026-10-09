using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ulfbou.Portfolio.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StoryBlockType
{
    Prose,
    Quote,
    Reflection,
    Code,
    Terminal,
    Image,
    Links,
    Evidence,
    Callout,
    Relationship,
    Replay,
    DiagnosticComparison
}

public sealed record StoryBlockDefinition(
    int Order,
    string Id,
    StoryBlockType Type,
    string AccessibleLabel,
    ReadingDepth ReadingDepth,
    Provenance Provenance,
    PublicationMetadata Publication,
    JsonElement Payload);

public sealed record StoryBlock(
    int Order,
    string Id,
    StoryBlockType Type,
    string AccessibleLabel,
    ReadingDepth ReadingDepth,
    Provenance Provenance,
    JsonElement Payload);

public static class StoryBlockContract
{
    private static readonly IReadOnlySet<StoryBlockType> RenderedTypes =
        new HashSet<StoryBlockType>(
            Enum.GetValues<StoryBlockType>());

    public static IReadOnlySet<StoryBlockType> SupportedTypes =>
        RenderedTypes;

    public static bool HasRenderer(StoryBlockType type) =>
        Enum.IsDefined(type) && RenderedTypes.Contains(type);

    public static void RequireRenderer(
        StoryBlockType type,
        string path)
    {
        if (!HasRenderer(type))
        {
            throw new ContentValidationException(
                "VAL-BLOCK-RENDERER",
                path,
                $"No publication renderer exists for block type '{type}'.");
        }
    }
}
