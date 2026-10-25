using Ulfbou.Correlation.Application;
using Ulfbou.Correlation.Aws;
using Ulfbou.Correlation.Core;
namespace Ulfbou.Correlation.Runtime;
public sealed class BrowserHandler(RequestBroker broker,Func<ActiveCorrelation,ArchitectureScenario,CancellationToken,Task<CompletedCorrelation>> complete,bool enabled)
{
 private static readonly ArchitectureScenario Scenario=new("git-backed-correlation-cleanup","Git-governed terminal cleanup",["Active state is stored in S3."],["Can stale state be deleted?"]);
 public async Task<HttpResponse> HandleAsync(HttpRequest request,CancellationToken token)
 {
  try{if(!enabled)return Error(503,"CapabilityDisabled");if(request.IsBase64Encoded)return Error(400,"InvalidRequest");
   if(request.RouteKey=="POST /correlations"){var c=StrictJson.Read<CreateRequest>(request.Body,RuntimeLimits.BrowserRequestBytes,new HashSet<string>{"schemaVersion","scenarioId","scenarioSha256","clientOperationId"});if(c.SchemaVersion!="correlation-create-request-v1"||c.ScenarioId!=Scenario.ScenarioId)return Error(400,"InvalidRequest");var awaiting=await broker.StartAsync(Scenario,token);var result=await complete(awaiting,Scenario,token);return Json(200,new{schemaVersion="correlation-create-response-v1",correlationId=result.State.CorrelationId,operationId=result.State.OperationId,state="Completed",stateRevision=result.State.StateRevision,expiresAt=result.State.ExpiresAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),result=result.Outcome});}
   if(request.RouteKey=="POST /correlations/{correlationId}/cancel"){var c=StrictJson.Read<CancelRequest>(request.Body,RuntimeLimits.BrowserRequestBytes,new HashSet<string>{"schemaVersion","correlationId","operationId","expectedStateRevision"});var result=await broker.CancelAsync(c.CorrelationId,c.OperationId,c.ExpectedStateRevision,token);return Json(200,new{schemaVersion="correlation-cancel-response-v1",correlationId=result.State.CorrelationId,operationId=result.State.OperationId,state="Cancelled",stateRevision=result.State.StateRevision,outcome=result.WasAlreadyTerminal?"AlreadyTerminal":"Cancelled"});}return Error(404,"InvalidRequest");}
  catch(CorrelationFailure x){return Error(x.Code.Contains("QUOTA")?429:x.Code.Contains("CONFLICT")||x.Code.Contains("REVISION")?409:x.Code.Contains("EXPIRED")?410:400,x.Code=="PROVIDER-QUOTA"?"ProviderQuotaExhausted":x.Code=="BROKER-EXPIRED"?"CorrelationExpired":"InvalidRequest");}
 }
 private static HttpResponse Json(int status,object value)=>new(status,new Dictionary<string,string>{{"content-type","application/json"},{"cache-control","no-store"}},StrictJson.Write(value,RuntimeLimits.BrowserResponseBytes));
 private static HttpResponse Error(int status,string code)=>Json(status,new{schemaVersion="correlation-browser-error-v1",operationId="op-unavailable",code,retryable=false,message="The bounded operation could not be completed."});
}
public sealed class CleanupHandler(S3CorrelationStateStore store)
{
 public async Task<CleanupReceipt> HandleAsync(string json,CancellationToken token)
 {
  var m=StrictJson.Read<CleanupManifest>(json,RuntimeLimits.CleanupRequestBytes,new HashSet<string>{"schemaVersion","manifestId","entries"});if(m.SchemaVersion!="correlation-deletion-manifest-v1"||m.Entries.Count is <1 or >RuntimeLimits.CleanupEntries||m.Entries.Select(x=>x.EntryId).Distinct().Count()!=m.Entries.Count)throw new CorrelationFailure("CLEANUP-MANIFEST","Manifest invalid.");
  var r=new List<CleanupReceiptEntry>();foreach(var e in m.Entries){var d=await store.DeleteExactAsync(new(e.CorrelationId,e.ExpectedStateRevision,e.ExpectedStateSha256,"correlation-state-canonical-json-v1"),token);var o=d.Decision==DeletionDecision.DeleteConditionally?"Deleted":d.Decision.ToString();r.Add(new(e.EntryId,e.CorrelationId,o,o.ToUpperInvariant()));}
  if(r.Count!=m.Entries.Count||r.Select(x=>x.EntryId).Distinct().Count()!=m.Entries.Count)throw new CorrelationFailure("CLEANUP-BIJECTION","Receipt mismatch.");return new("correlation-deletion-receipt-v1",m.ManifestId,r,new(r.Count,r.Count(x=>x.Outcome=="Deleted"),r.Count(x=>x.Outcome=="AlreadyAbsent"),r.Count(x=>x.Outcome is "RevisionConflict" or "DigestConflict"),r.Count(x=>x.Outcome=="InvalidActiveState"),0));
 }
}
