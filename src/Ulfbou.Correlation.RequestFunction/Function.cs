using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Ulfbou.Correlation.RequestFunction;

public sealed class Function
{
    public Response FunctionHandler(Request request, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        context.Logger.LogInformation("capability=correlation-request outcome=InfrastructureSmoke");
        return new("correlation-browser-error-v1", request.OperationId, "CapabilityDisabled", false, "Prototype capability is not enabled.");
    }
}
public sealed record Request(string OperationId);
public sealed record Response(string SchemaVersion, string OperationId, string Code, bool Retryable, string Message);
