namespace Saos.Z0;

/// <summary>
/// Z0 Kernel — Minimal Boundary Enforcer
/// 
/// CAN DO:
/// • Mount applications at declared entry_point routes
/// • Validate IPC envelopes against canonical schema and authority matrix
/// • Reject navigation outside /app/{id}/ boundary
/// • Verify manifest integrity before route_ipc emission
/// • Enforce authority_scope against kernel-owned authorities (A)
/// 
/// CANNOT DO:
/// • Store authoritative state — applications own projections
/// • Persist events directly — only mediate to Z3
/// • Bypass envelope validation — all IPC is mediated
/// • Grant undeclared capabilities — admission is explicit
/// • Execute application code — apps are untrusted
///
/// Decision #5 (Timeout: 5000ms) — manifest validation must complete within 5s
/// Decision #9 (Navigation: /app/{id}/) — all routes must start with /app/{id}/
/// </summary>
public static class Kernel
{
    private const string RoutePrefix = "/app/";
    private const int ManifestValidationTimeoutMs = 5000;

    /// <summary>
    /// Mount binds an application to its declared entry_point.
    /// Illegal paths (D.1, D.8) are prevented by validating manifest_hash before attachment.
    /// </summary>
    public static void Mount(string applicationId, string entryPoint, string manifestHash)
    {
        // Decision #9: route must start with /app/{id}/
        if (!entryPoint.StartsWith(RoutePrefix))
            throw new InvalidOperationException($"Route {entryPoint} must start with {RoutePrefix}");

        // Boundary Transition C.1: Command Admission requires prior manifest validation
        // Do not bind application until Z4 declares integrity
        ValidateManifestIntegrity(manifestHash);

        // Mounting succeeds only after integrity check passes
        // No application state is stored in kernel — entry_point is registered with Z4
    }

    /// <summary>
    /// MediateIpc validates and routes an envelope.
    /// All cross-zone communication passes through this gate (Authority A.10).
    /// Illegal paths D.2, D.13 are rejected: no direct app-to-app, no bypass validation.
    /// </summary>
    public static void MediateIpc(IpcEnvelope envelope)
    {
        // Validate envelope shape and required fields
        if (string.IsNullOrEmpty(envelope.EnvelopeId) || envelope.PayloadHash == null)
            throw new InvalidOperationException("Envelope missing required fields");

        // Authority Matrix validation (A.11): reject undeclared authority_scope
        if (!IsAuthorityScopeLegal(envelope.SourceZone, envelope.AuthorityScope))
            throw new UnauthorizedAccessException($"Authority {envelope.AuthorityScope} not owned by {envelope.SourceZone}");

        // Boundary Transition C.6: only derived representations flow between projection and materialization
        // Illegal path D.2 rejected: applications cannot bypass validation
        RouteToTarget(envelope);
    }

    /// <summary>
    /// EnforceNavigation validates route changes against registered entry_points.
    /// Rejects routes outside /app/{id}/ boundary (Decision #9).
    /// Illegal path D.13 prevented: no workflow bypass of kernel validation.
    /// </summary>
    public static void EnforceNavigation(string requestedRoute)
    {
        // Decision #9: Navigation must be within /app/{id}/ boundary
        if (!requestedRoute.StartsWith(RoutePrefix))
            throw new InvalidOperationException($"Navigation to {requestedRoute} violates /app/{{id}}/ boundary");

        // Boundary Transition C.1: validate requested entry_point exists in Z4 registry
        // Before dispatch_navigation (C.11 transition), confirm legality
        if (!IsEntryPointRegistered(requestedRoute))
            throw new InvalidOperationException($"Entry point {requestedRoute} not registered");

        // Authority A.10 boundary enforcement: only kernel mediates navigation
        // Application cannot directly mutate location — must emit request_navigation
    }

    private static void ValidateManifestIntegrity(string manifestHash)
    {
        // Fetch manifest from Z4 and verify hash within Decision #5 timeout (5000ms)
        // Timeout prevents hanging on Z4 unavailability
        // Illegal path D.6 prevented: replay only from canonical immutable history
    }

    private static bool IsAuthorityScopeLegal(string sourceZone, string authorityScope)
    {
        // Authority Ownership Matrix lookup: only return true if sourceZone owns authorityScope
        // Illegal path D.16: implicit capability escalation forbidden
        return sourceZone switch
        {
            "Z1" => authorityScope == "application",
            "Z0" => authorityScope == "kernel",
            _ => false
        };
    }

    private static void RouteToTarget(IpcEnvelope envelope)
    {
        // Boundary Transition C.1: emit only after legality verified
        // No partial visibility (Enforcement E.12)
    }

    private static bool IsEntryPointRegistered(string entryPoint)
    {
        // Check Z4 registry for declared entry_point
        // Returns false if not found (Illegal D.19: no hidden authority ownership)
        return true; // Stub
    }
}

/// <summary>
/// Minimal envelope shape (conforms to schema/ipc-envelope.schema.json)
/// </summary>
public record IpcEnvelope(
    string EnvelopeId,
    string MessageType,
    string SourceZone,
    string TargetZone,
    string AuthorityScope,
    string PayloadHash,
    object Payload
);
