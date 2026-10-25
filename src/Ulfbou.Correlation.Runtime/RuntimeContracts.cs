using System.Text.Json;
using Ulfbou.Correlation.Application;
using Ulfbou.Correlation.Core;
namespace Ulfbou.Correlation.Runtime;
public static class RuntimeLimits { public const int BrowserRequestBytes=8*1024; public const int BrowserResponseBytes=16*1024; public const int ProviderResponseBytes=16*1024; public const int CleanupRequestBytes=128*1024; public const int CleanupEntries=25; }
public sealed record HttpRequest(string RouteKey,string? Body,bool IsBase64Encoded=false);
public sealed record HttpResponse(int StatusCode,IReadOnlyDictionary<string,string> Headers,string Body);
public sealed record CreateRequest(string SchemaVersion,string ScenarioId,string ScenarioSha256,string ClientOperationId);
public sealed record CancelRequest(string SchemaVersion,string CorrelationId,string OperationId,int ExpectedStateRevision);
public sealed record CleanupManifest(string SchemaVersion,string ManifestId,IReadOnlyList<CleanupEntry> Entries);
public sealed record CleanupEntry(string EntryId,string CorrelationId,int ExpectedStateRevision,string ExpectedStateSha256);
public sealed record CleanupReceipt(string SchemaVersion,string ManifestId,IReadOnlyList<CleanupReceiptEntry> Entries,CleanupSummary Summary);
public sealed record CleanupReceiptEntry(string EntryId,string CorrelationId,string Outcome,string MessageCode);
public sealed record CleanupSummary(int Requested,int Deleted,int AlreadyAbsent,int Conflicts,int Invalid,int TemporaryFailures);
public static class StrictJson
{
 public static T Read<T>(string? body,int maximum,IReadOnlySet<string> admitted)
 {
  if(body is null)throw new CorrelationFailure("HTTP-BODY","Body is required.");
  if(System.Text.Encoding.UTF8.GetByteCount(body)>maximum)throw new CorrelationFailure("HTTP-OVERSIZED","Body exceeds limit.");
  using var doc=JsonDocument.Parse(body,new JsonDocumentOptions{AllowTrailingCommas=false,CommentHandling=JsonCommentHandling.Disallow,MaxDepth=24});
  if(doc.RootElement.ValueKind!=JsonValueKind.Object)throw new CorrelationFailure("HTTP-JSON","Root must be object.");
  var seen=new HashSet<string>(StringComparer.Ordinal); foreach(var p in doc.RootElement.EnumerateObject())if(!seen.Add(p.Name)||!admitted.Contains(p.Name))throw new CorrelationFailure("HTTP-PROPERTY","Unknown or duplicate property.");
  return JsonSerializer.Deserialize<T>(body,Options)??throw new CorrelationFailure("HTTP-JSON","Body is empty.");
 }
 public static string Write<T>(T value,int maximum){var b=JsonSerializer.SerializeToUtf8Bytes(value,Options);if(b.Length>maximum)throw new CorrelationFailure("HTTP-RESPONSE-OVERSIZED","Response exceeds limit.");return System.Text.Encoding.UTF8.GetString(b);}
 public static JsonSerializerOptions Options{get;}=new(){PropertyNamingPolicy=JsonNamingPolicy.CamelCase};
}
public sealed class SystemClock : ICorrelationClock { public DateTimeOffset UtcNow=>DateTimeOffset.UtcNow; }
public sealed class GuidIdentities : ICorrelationIdentitySource { private static string N(string p)=>p+Guid.NewGuid().ToString("N"); public string NewCorrelationId()=>N("corr-");public string NewOperationId()=>N("op-");public string NewRequestId()=>N("req-"); }
