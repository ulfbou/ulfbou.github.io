using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Ulfbou.Correlation.CleanupFunction;

public sealed class Function
{
    public Response FunctionHandler(Request request, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        context.Logger.LogInformation("capability=correlation-cleanup outcome=InfrastructureSmoke");
        return new("correlation-deletion-receipt-v1", request.ManifestId, "CapabilityDisabled");
    }
}
public sealed record Request(string ManifestId);
public sealed record Response(string SchemaVersion, string ManifestId, string Outcome);
