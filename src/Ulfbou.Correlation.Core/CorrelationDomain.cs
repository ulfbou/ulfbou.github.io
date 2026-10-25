using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ulfbou.Correlation.Core;

public sealed class CorrelationFailure(string code, string message) : Exception($"{code}: {message}")
{
    public string Code { get; } = code;
}

public enum CorrelationState { Created, Dispatching, AwaitingProvider, ResultReceived, Completed, Cancelled, Revoked, Expired, Invalid, FailedTerminal, Superseded }
public enum CorrelationSignal { DispatchStarted, ProviderAccepted, ValidProviderResult, ResultNormalized, CancelAccepted, ExpiryObserved, TerminalFailure, InvalidStateObserved, ExactRevisionSuperseded }

public static class CorrelationStateMachine
{
    private static readonly Dictionary<(CorrelationState, CorrelationSignal), CorrelationState> Map = new()
    {
        [(CorrelationState.Created, CorrelationSignal.DispatchStarted)] = CorrelationState.Dispatching,
        [(CorrelationState.Dispatching, CorrelationSignal.ProviderAccepted)] = CorrelationState.AwaitingProvider,
        [(CorrelationState.AwaitingProvider, CorrelationSignal.ValidProviderResult)] = CorrelationState.ResultReceived,
        [(CorrelationState.ResultReceived, CorrelationSignal.ResultNormalized)] = CorrelationState.Completed,
        [(CorrelationState.AwaitingProvider, CorrelationSignal.CancelAccepted)] = CorrelationState.Cancelled,
        [(CorrelationState.Created, CorrelationSignal.ExpiryObserved)] = CorrelationState.Expired,
        [(CorrelationState.Dispatching, CorrelationSignal.ExpiryObserved)] = CorrelationState.Expired,
        [(CorrelationState.AwaitingProvider, CorrelationSignal.ExpiryObserved)] = CorrelationState.Expired,
        [(CorrelationState.ResultReceived, CorrelationSignal.ExpiryObserved)] = CorrelationState.Expired,
        [(CorrelationState.Created, CorrelationSignal.TerminalFailure)] = CorrelationState.FailedTerminal,
        [(CorrelationState.Dispatching, CorrelationSignal.TerminalFailure)] = CorrelationState.FailedTerminal,
        [(CorrelationState.AwaitingProvider, CorrelationSignal.TerminalFailure)] = CorrelationState.FailedTerminal,
        [(CorrelationState.ResultReceived, CorrelationSignal.TerminalFailure)] = CorrelationState.FailedTerminal,
        [(CorrelationState.AwaitingProvider, CorrelationSignal.InvalidStateObserved)] = CorrelationState.Invalid,
        [(CorrelationState.AwaitingProvider, CorrelationSignal.ExactRevisionSuperseded)] = CorrelationState.Superseded
    };
    public static bool IsTerminal(CorrelationState s) => s is CorrelationState.Completed or CorrelationState.Cancelled or CorrelationState.Revoked or CorrelationState.Expired or CorrelationState.Invalid or CorrelationState.FailedTerminal or CorrelationState.Superseded;
    public static CorrelationState Next(CorrelationState current, CorrelationSignal signal)
    {
        if (IsTerminal(current)) throw new CorrelationFailure("STATE-TERMINAL", $"'{current}' is terminal.");
        if (!Map.TryGetValue((current, signal), out var next)) throw new CorrelationFailure("STATE-TRANSITION", $"'{signal}' is not admitted from '{current}'.");
        return next;
    }
    public static int NextRevision(int observed, int expected)
    {
        if (observed != expected) throw new CorrelationFailure("STATE-REVISION", "Expected and observed revisions differ.");
        if (observed < 1 || observed >= 100) throw new CorrelationFailure("STATE-REVISION-RANGE", "Revision cannot advance.");
        return observed + 1;
    }
}

public static class CanonicalJson
{
    public static byte[] Canonicalize(ReadOnlySpan<byte> json)
    {
        if (json.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF })) throw new CorrelationFailure("CANON-BOM", "BOM prohibited.");
        CheckTokens(json);
        try
        {
            using var document = JsonDocument.Parse(json.ToArray(), new JsonDocumentOptions { MaxDepth = 32 });
            if (document.RootElement.ValueKind != JsonValueKind.Object) throw new CorrelationFailure("CANON-ROOT", "Root must be object.");
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping })) Write(writer, document.RootElement);
            return buffer.WrittenSpan.ToArray();
        }
        catch (JsonException error) { throw new CorrelationFailure("CANON-JSON", error.Message); }
    }
    public static string Identity(ReadOnlySpan<byte> bytes) => "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static void CheckTokens(ReadOnlySpan<byte> json)
    {
        var reader = new Utf8JsonReader(json, new JsonReaderOptions { MaxDepth = 32 });
        var names = new Stack<HashSet<string>>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject) names.Push(new(StringComparer.Ordinal));
            else if (reader.TokenType == JsonTokenType.EndObject) names.Pop();
            else if (reader.TokenType == JsonTokenType.PropertyName && !names.Peek().Add(reader.GetString()!)) throw new CorrelationFailure("CANON-DUPLICATE", "Duplicate property.");
            else if (reader.TokenType == JsonTokenType.Number)
            {
                var raw = Encoding.UTF8.GetString(reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan);
                if (!Regex.IsMatch(raw, "^(0|-?[1-9][0-9]*)$", RegexOptions.CultureInvariant)) throw new CorrelationFailure("CANON-NUMBER", "Only canonical integers are admitted.");
            }
        }
    }
    private static void Write(Utf8JsonWriter writer, JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject(); foreach (var p in value.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal)) { writer.WritePropertyName(p.Name); Write(writer, p.Value); } writer.WriteEndObject(); break;
            case JsonValueKind.Array:
                writer.WriteStartArray(); foreach (var x in value.EnumerateArray()) Write(writer, x); writer.WriteEndArray(); break;
            case JsonValueKind.String: writer.WriteStringValue(value.GetString()); break;
            case JsonValueKind.Number: writer.WriteRawValue(value.GetRawText()); break;
            case JsonValueKind.True: writer.WriteBooleanValue(true); break;
            case JsonValueKind.False: writer.WriteBooleanValue(false); break;
            case JsonValueKind.Null: throw new CorrelationFailure("CANON-NULL", "Null is not admitted.");
            default: throw new CorrelationFailure("CANON-KIND", "Unsupported JSON kind.");
        }
    }
}

public enum ActiveStatePresence { Absent, Present, Malformed }
public enum DeletionDecision { DeleteConditionally, AlreadyAbsent, RevisionConflict, DigestConflict, InvalidActiveState }
public sealed record DeletionRequest(string CorrelationId, int ExpectedStateRevision, string ExpectedStateSha256, string CanonicalRepresentation);
public sealed record ObservedActiveState(ActiveStatePresence Presence, string? CorrelationId = null, int? StateRevision = null, string? StateSha256 = null, string? CanonicalRepresentation = null, string? VersionId = null, bool DeletePreconditionPassed = true);
public static class DeletionAuthorizer
{
    public static DeletionDecision Authorize(DeletionRequest r, ObservedActiveState o)
    {
        if (o.Presence == ActiveStatePresence.Absent) return DeletionDecision.AlreadyAbsent;
        if (o.Presence == ActiveStatePresence.Malformed || r.CorrelationId != o.CorrelationId || r.CanonicalRepresentation != o.CanonicalRepresentation) return DeletionDecision.InvalidActiveState;
        if (r.ExpectedStateRevision != o.StateRevision || !o.DeletePreconditionPassed) return DeletionDecision.RevisionConflict;
        if (r.ExpectedStateSha256 != o.StateSha256) return DeletionDecision.DigestConflict;
        return string.IsNullOrWhiteSpace(o.VersionId) ? DeletionDecision.InvalidActiveState : DeletionDecision.DeleteConditionally;
    }
    public static string DeriveKey(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length < 2 || !Regex.IsMatch(id, "^[a-z0-9][a-z0-9-]{7,127}$")) throw new CorrelationFailure("DELETE-CORRELATION-ID", "Identity is not admitted.");
        return $"active/v1/{id[..2]}/{id}.json";
    }
}
