using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Ulfbou.Portfolio.Core;

public static class StoryBlockValidator
{
    public static void Validate(
        [NotNull] IReadOnlyList<StoryBlockDefinition>? blocks,
        string projectPath,
        bool projectIsPublishable)
    {
        if (blocks is null)
        {
            Fail(
                "VAL-COLLECTION",
                $"{projectPath}.blocks",
                "Required collection is null.");
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        var orders = new HashSet<int>();

        for (var index = 0; index < blocks.Count; index++)
        {
            var block = blocks[index];
            var path = $"{projectPath}.blocks[{index}]";

            if (block is null)
            {
                Fail("VAL-DOCUMENT", path, "Story block is null.");
            }

            Required(block.Id, $"{path}.id");
            Required(block.AccessibleLabel, $"{path}.accessibleLabel");

            if (!ids.Add(block.Id))
            {
                Fail(
                    "VAL-DUPLICATE-ID",
                    $"{projectPath}.blocks",
                    $"Duplicate block ID '{block.Id}'.");
            }

            if (block.Order < 0)
            {
                Fail(
                    "VAL-ORDER",
                    $"{path}.order",
                    "Block order must not be negative.");
            }

            if (!orders.Add(block.Order))
            {
                Fail(
                    "VAL-DUPLICATE-ORDER",
                    $"{projectPath}.blocks",
                    $"Duplicate block order '{block.Order}'.");
            }

            if (!Enum.IsDefined(block.Type))
            {
                Fail(
                    "VAL-BLOCK-TYPE",
                    $"{path}.type",
                    "Story-block type is undefined.");
            }

            if (!Enum.IsDefined(block.ReadingDepth))
            {
                Fail(
                    "VAL-ENUM",
                    $"{path}.readingDepth",
                    "Reading depth is undefined.");
            }

            ValidateProvenance(block.Provenance, path);
            ValidatePublication(block.Publication, path);

            var publishes =
                projectIsPublishable
                && block.Publication.State
                    == PublicationState.Published
                && block.Publication.Visibility
                    == Visibility.Public;

            if (!publishes)
            {
                continue;
            }

            StoryBlockContract.RequireRenderer(
                block.Type,
                $"{path}.type");

            ValidatePayload(block, path);
        }
    }

    private static void ValidatePayload(
        StoryBlockDefinition block,
        string path)
    {
        if (block.Payload.ValueKind != JsonValueKind.Object)
        {
            Fail(
                "VAL-BLOCK-PAYLOAD",
                $"{path}.payload",
                "Published block payload must be an object.");
        }

        switch (block.Type)
        {
            case StoryBlockType.Prose:
            case StoryBlockType.Reflection:
            case StoryBlockType.Callout:
                RequiredString(block.Payload, "text", path);
                break;

            case StoryBlockType.Quote:
                RequiredString(block.Payload, "text", path);
                RequiredString(block.Payload, "attribution", path);
                break;

            case StoryBlockType.Code:
            case StoryBlockType.Terminal:
                RequiredString(block.Payload, "caption", path);
                RequiredStringArray(block.Payload, "lines", path);
                break;

            case StoryBlockType.Image:
                RequiredString(block.Payload, "source", path);
                RequiredString(block.Payload, "alternative", path);
                break;

            case StoryBlockType.Links:
                RequiredObjectArray(
                    block.Payload,
                    "items",
                    path,
                    "label",
                    "url");
                ValidateLinkUrls(block.Payload, path);
                break;

            case StoryBlockType.Evidence:
                RequiredObjectArray(
                    block.Payload,
                    "items",
                    path,
                    "label",
                    "value");
                break;

            case StoryBlockType.Relationship:
                RequiredString(block.Payload, "subject", path);
                RequiredString(block.Payload, "relationship", path);
                RequiredString(block.Payload, "target", path);
                break;

            case StoryBlockType.Replay:
                RequiredStringArray(block.Payload, "steps", path);
                break;

            case StoryBlockType.DiagnosticComparison:
                RequiredString(block.Payload, "before", path);
                RequiredString(block.Payload, "after", path);
                break;

            default:
                Fail(
                    "VAL-BLOCK-TYPE",
                    $"{path}.type",
                    $"Unsupported block type '{block.Type}'.");
                break;
        }
    }

    private static void RequiredString(
        JsonElement source,
        string name,
        string path)
    {
        if (
            !source.TryGetProperty(name, out var value)
            || value.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(value.GetString()))
        {
            Fail(
                "VAL-BLOCK-PAYLOAD",
                $"{path}.payload.{name}",
                "Required payload string is empty.");
        }
    }

    private static void RequiredStringArray(
        JsonElement payload,
        string name,
        string path)
    {
        if (
            !payload.TryGetProperty(name, out var value)
            || value.ValueKind != JsonValueKind.Array
            || value.GetArrayLength() == 0)
        {
            Fail(
                "VAL-BLOCK-PAYLOAD",
                $"{path}.payload.{name}",
                "Required payload array is empty.");
        }

        var index = 0;

        foreach (var item in value.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.String
                || item.GetString() is null)
            {
                Fail(
                    "VAL-BLOCK-PAYLOAD",
                    $"{path}.payload.{name}[{index}]",
                    "Payload item must be a string.");
            }

            index++;
        }
    }

    private static void RequiredObjectArray(
        JsonElement payload,
        string name,
        string path,
        params string[] properties)
    {
        if (
            !payload.TryGetProperty(name, out var value)
            || value.ValueKind != JsonValueKind.Array
            || value.GetArrayLength() == 0)
        {
            Fail(
                "VAL-BLOCK-PAYLOAD",
                $"{path}.payload.{name}",
                "Required payload array is empty.");
        }

        var index = 0;

        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                Fail(
                    "VAL-BLOCK-PAYLOAD",
                    $"{path}.payload.{name}[{index}]",
                    "Payload item must be an object.");
            }

            foreach (var property in properties)
            {
                RequiredString(
                    item,
                    property,
                    $"{path}.payload.{name}[{index}]");
            }

            index++;
        }
    }

    private static void ValidateLinkUrls(
        JsonElement payload,
        string path)
    {
        var index = 0;

        foreach (
            var item in payload
                .GetProperty("items")
                .EnumerateArray())
        {
            var value = item.GetProperty("url").GetString();

            if (
                !Uri.TryCreate(
                    value,
                    UriKind.Absolute,
                    out var uri)
                || (
                    uri.Scheme != Uri.UriSchemeHttp
                    && uri.Scheme != Uri.UriSchemeHttps))
            {
                Fail(
                    "VAL-URL",
                    $"{path}.payload.items[{index}].url",
                    $"Invalid absolute HTTP or HTTPS URL '{value}'.");
            }

            index++;
        }
    }

    private static void ValidateProvenance(
        [NotNull] Provenance? provenance,
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
        [NotNull] PublicationMetadata? publication,
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
                "Publication metadata contains an undefined value.");
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

    private static void Required(string? value, string path)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Fail(
                "VAL-STRING",
                path,
                "Required string is empty.");
        }
    }

    [DoesNotReturn]
    private static void Fail(
        string code,
        string path,
        string reason)
    {
        throw new ContentValidationException(code, path, reason);
    }
}
