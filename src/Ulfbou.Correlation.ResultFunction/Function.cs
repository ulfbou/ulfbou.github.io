using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Ulfbou.Correlation.ResultFunction;

public sealed class Function
{
    public Response FunctionHandler(Request request, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        context.Logger.LogInformation("capability=correlation-result outcome=InfrastructureSmoke");
        return new("correlation-browser-error-v1", request.OperationId, "CapabilityDisabled", false, "Provider mediation is not enabled.");
    }
}
public sealed record Request(string OperationId);
public sealed record Response(string SchemaVersion, string OperationId, string Code, bool Retryable, string Message);
