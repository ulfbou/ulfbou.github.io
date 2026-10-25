using System.Text.Json;
using Ulfbou.Correlation.Application;
using Ulfbou.Correlation.Core;

namespace Ulfbou.Correlation.Aws;

internal sealed record ActiveStateDocument(
    string SchemaVersion,
    string CorrelationId,
    string OperationId,
    string Capability,
    string ProviderAlias,
    string State,
    int StateRevision,
    string CreatedAt,
    string UpdatedAt,
    string ExpiresAt,
    int Attempt,
    int MaximumAttempts,
    string RequestId,
    string ExpectedResultMode,
    string ResultTarget,
    string PreviousStateSha256,
    string CanonicalRepresentation);

internal static class ActiveStateCodec
{
    public const int MaximumBytes = 16 * 1024;
    public static byte[] Encode(ActiveCorrelation state)
    {
        var document = new ActiveStateDocument(
            "correlation-active-state-v1", state.CorrelationId, state.OperationId,
            CorrelationApplicationContract.Capability, CorrelationApplicationContract.ProviderAlias,
            state.State.ToString(), state.StateRevision, Timestamp(state.CreatedAt), Timestamp(state.UpdatedAt),
            Timestamp(state.ExpiresAt), 1, 2, state.RequestId, "Synchronous",
            "browser-active-interaction-v1", state.PreviousStateSha256,
            "correlation-state-canonical-json-v1");
        var bytes = CanonicalJson.Canonicalize(JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions));
        if (bytes.Length > MaximumBytes) throw new CorrelationFailure("STORE-OVERSIZED", "Active state exceeds 16 KiB.");
        return bytes;
    }

    public static ActiveCorrelation Decode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length > MaximumBytes) throw new CorrelationFailure("STORE-OVERSIZED", "Active state exceeds 16 KiB.");
        var canonical = CanonicalJson.Canonicalize(bytes);
        var value = JsonSerializer.Deserialize<ActiveStateDocument>(canonical, JsonOptions)
            ?? throw new CorrelationFailure("STORE-MALFORMED", "Active state is empty.");
        if (value.SchemaVersion != "correlation-active-state-v1" || value.CanonicalRepresentation != "correlation-state-canonical-json-v1" ||
            value.Capability != CorrelationApplicationContract.Capability || value.ProviderAlias != CorrelationApplicationContract.ProviderAlias)
            throw new CorrelationFailure("STORE-CONTRACT", "Active state contract identity is not admitted.");
        if (!Enum.TryParse<CorrelationState>(value.State, false, out var state)) throw new CorrelationFailure("STORE-STATE", "State is not admitted.");
        return new(value.CorrelationId, value.OperationId, value.RequestId, state, value.StateRevision,
            Parse(value.CreatedAt), Parse(value.UpdatedAt), Parse(value.ExpiresAt), value.PreviousStateSha256,
            CanonicalJson.Identity(canonical));
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static string Timestamp(DateTimeOffset value) => value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
    private static DateTimeOffset Parse(string value) => DateTimeOffset.ParseExact(value, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture);
}
