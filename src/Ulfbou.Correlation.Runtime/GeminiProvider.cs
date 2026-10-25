using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ulfbou.Correlation.Application;
using Ulfbou.Correlation.Core;
namespace Ulfbou.Correlation.Runtime;
public interface IProviderCredentialSource { Task<string> ReadAsync(CancellationToken cancellationToken); }
public sealed class GeminiProvider(HttpClient http,IProviderCredentialSource credentials,string pinnedModel):IArchitectureProvider
{
 public async Task<ProviderOutcome> ClassifyAsync(string operationId,string correlationId,ArchitectureScenario scenario,CancellationToken cancellationToken)
 {
  if(pinnedModel.Length is <1 or >120)throw new CorrelationFailure("PROVIDER-MODEL","Pinned model is invalid.");
  var key=await credentials.ReadAsync(cancellationToken);using var req=new HttpRequestMessage(HttpMethod.Post,$"v1beta/models/{Uri.EscapeDataString(pinnedModel)}:generateContent?key={Uri.EscapeDataString(key)}");
  req.Content=JsonContent.Create(new{contents=new[]{new{role="user",parts=new[]{new{text=JsonSerializer.Serialize(new{instruction="Return provider-outcome-v1 candidate analysis. You are not authoritative.",scenario},StrictJson.Options)}}}},generationConfig=new{responseMimeType="application/json",maxOutputTokens=1024,temperature=0}});
  using var response=await http.SendAsync(req,HttpCompletionOption.ResponseHeadersRead,cancellationToken);
  if(response.StatusCode==HttpStatusCode.TooManyRequests)throw new CorrelationFailure("PROVIDER-QUOTA","Quota exhausted.");
  if(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)throw new CorrelationFailure("PROVIDER-AUTHORIZATION","Authorization failed.");
  if((int)response.StatusCode>=500)throw new CorrelationFailure("PROVIDER-UNAVAILABLE","Provider unavailable.");
  if(!response.IsSuccessStatusCode)throw new CorrelationFailure("PROVIDER-REQUEST","Provider rejected request.");
  await using var stream=await response.Content.ReadAsStreamAsync(cancellationToken);using var buffer=new MemoryStream();var block=new byte[4096];while(true){var n=await stream.ReadAsync(block,cancellationToken);if(n==0)break;if(buffer.Length+n>RuntimeLimits.ProviderResponseBytes)throw new CorrelationFailure("PROVIDER-OVERSIZED","Provider response exceeds limit.");buffer.Write(block,0,n);}
  try{using var envelope=JsonDocument.Parse(buffer.ToArray());var text=envelope.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();var outcome=JsonSerializer.Deserialize<ProviderOutcome>(text!,StrictJson.Options)??throw new JsonException();if(outcome.OperationId!=operationId||outcome.CorrelationId!=correlationId||outcome.Authoritative||outcome.Classification!="CandidateAnalysis"||outcome.ProviderAlias!="google-gemini-prototype")throw new CorrelationFailure("PROVIDER-INVALID","Outcome violates identity or authority.");return outcome;}catch(JsonException){throw new CorrelationFailure("PROVIDER-INVALID","Provider JSON is invalid.");}
 }
}
