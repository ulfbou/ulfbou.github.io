using System.Text;
using System.Text.Json;
using Ulfbou.Correlation.Core;

namespace Ulfbou.Correlation.Application;

public sealed class RequestBroker(
    ICorrelationStateStore store,
    ICorrelationIdentitySource identities,
    ICorrelationClock clock)
{
    public async Task<ActiveCorrelation> StartAsync(ArchitectureScenario scenario, CancellationToken cancellationToken = default)
    {
        ValidateScenario(scenario);
        var now = clock.UtcNow;
        var created = NewState(
            identities.NewCorrelationId(),
            identities.NewOperationId(),
            identities.NewRequestId(),
            CorrelationState.Created,
            1,
            now,
            now,
            now.AddMinutes(15),
            "none");
        await store.CreateAsync(created, cancellationToken);
        var dispatching = Advance(created, CorrelationSignal.DispatchStarted, now);
        dispatching = await store.AdvanceAsync(created, dispatching, cancellationToken);
        var awaiting = Advance(dispatching, CorrelationSignal.ProviderAccepted, now);
        return await store.AdvanceAsync(dispatching, awaiting, cancellationToken);
    }

    public async Task<CancelledCorrelation> CancelAsync(
        string correlationId,
        string operationId,
        int expectedStateRevision,
        CancellationToken cancellationToken = default)
    {
        var observed = await store.ReadAsync(correlationId, cancellationToken)
            ?? throw new CorrelationFailure("BROKER-NOT-FOUND", "Correlation was not found.");
        if (!StringComparer.Ordinal.Equals(observed.OperationId, operationId))
            throw new CorrelationFailure("BROKER-OPERATION", "Operation identity does not match.");
        if (CorrelationStateMachine.IsTerminal(observed.State))
            return new(observed, Tombstone(observed, ReasonFor(observed.State), observed.UpdatedAt), true);
        if (observed.StateRevision != expectedStateRevision)
            throw new CorrelationFailure("STATE-REVISION", "Expected and observed revisions differ.");
        if (observed.State != CorrelationState.AwaitingProvider)
            throw new CorrelationFailure("BROKER-CANCEL-STATE", "Cancellation is admitted only while awaiting the provider.");
        var cancelled = Advance(observed, CorrelationSignal.CancelAccepted, clock.UtcNow);
        cancelled = await store.AdvanceAsync(observed, cancelled, cancellationToken);
        return new(cancelled, Tombstone(cancelled, "CancelledByVisitor", clock.UtcNow), false);
    }

    internal static ActiveCorrelation Advance(ActiveCorrelation current, CorrelationSignal signal, DateTimeOffset now)
    {
        var next = current with
        {
            State = CorrelationStateMachine.Next(current.State, signal),
            StateRevision = CorrelationStateMachine.NextRevision(current.StateRevision, current.StateRevision),
            UpdatedAt = now,
            PreviousStateSha256 = current.StateSha256,
            StateSha256 = "pending"
        };
        return next with { StateSha256 = StateIdentity(next) };
    }

    internal static TombstoneEvent Tombstone(ActiveCorrelation state, string reasonCode, DateTimeOffset now)
    {
        if (!CorrelationStateMachine.IsTerminal(state.State))
            throw new CorrelationFailure("BROKER-TOMBSTONE", "Only terminal state can produce a tombstone event.");
        var eventBytes = Encoding.UTF8.GetBytes(string.Join("|", state.CorrelationId, state.OperationId, state.State, state.StateRevision, state.StateSha256, now.ToString("O")));
        return new(
            CanonicalJson.Identity(eventBytes),
            state.CorrelationId,
            state.OperationId,
            state.State,
            reasonCode,
            state.StateRevision,
            state.StateSha256,
            now);
    }

    internal static string StateIdentity(ActiveCorrelation state)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["canonicalRepresentation"] = "correlation-state-canonical-json-v1",
            ["correlationId"] = state.CorrelationId,
            ["createdAt"] = state.CreatedAt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
            ["expiresAt"] = state.ExpiresAt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
            ["operationId"] = state.OperationId,
            ["previousStateSha256"] = state.PreviousStateSha256,
            ["requestId"] = state.RequestId,
            ["state"] = state.State.ToString(),
            ["stateRevision"] = state.StateRevision,
            ["updatedAt"] = state.UpdatedAt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")
        });
        return CanonicalJson.Identity(CanonicalJson.Canonicalize(json));
    }

    private static ActiveCorrelation NewState(string correlationId, string operationId, string requestId, CorrelationState state, int revision, DateTimeOffset createdAt, DateTimeOffset updatedAt, DateTimeOffset expiresAt, string previous)
    {
        var provisional = new ActiveCorrelation(correlationId, operationId, requestId, state, revision, createdAt, updatedAt, expiresAt, previous, "pending");
        return provisional with { StateSha256 = StateIdentity(provisional) };
    }

    private static void ValidateScenario(ArchitectureScenario scenario)
    {
        if (scenario.ScenarioId != "git-backed-correlation-cleanup" || scenario.Facts.Count is < 1 or > 8 || scenario.Questions.Count is < 1 or > 8)
            throw new CorrelationFailure("BROKER-SCENARIO", "Only the bounded reviewed scenario is admitted.");
        if (scenario.Facts.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 240) || scenario.Questions.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 240))
            throw new CorrelationFailure("BROKER-SCENARIO", "Scenario fields exceed their admitted bounds.");
    }

    private static string ReasonFor(CorrelationState state) => state switch
    {
        CorrelationState.Completed => "CompletedNormally",
        CorrelationState.Cancelled => "CancelledByVisitor",
        CorrelationState.Expired => "ExpiredByPolicy",
        CorrelationState.Invalid => "InvalidState",
        CorrelationState.FailedTerminal => "ProviderUnavailable",
        CorrelationState.Superseded => "SupersededRevision",
        CorrelationState.Revoked => "RevokedByAuthority",
        _ => throw new CorrelationFailure("BROKER-REASON", "Terminal state has no admitted reason.")
    };
}

public sealed class ResultBroker(
    ICorrelationStateStore store,
    IArchitectureProvider provider,
    ICorrelationClock clock)
{
    public async Task<CompletedCorrelation> CompleteAsync(
        ActiveCorrelation expected,
        ArchitectureScenario scenario,
        CancellationToken cancellationToken = default)
    {
        var observed = await store.ReadAsync(expected.CorrelationId, cancellationToken)
            ?? throw new CorrelationFailure("BROKER-NOT-FOUND", "Correlation was not found.");
        if (observed.StateRevision != expected.StateRevision || !StringComparer.Ordinal.Equals(observed.StateSha256, expected.StateSha256))
            throw new CorrelationFailure("STATE-REVISION", "Expected state identity is stale.");
        if (observed.State != CorrelationState.AwaitingProvider)
            throw new CorrelationFailure("BROKER-LATE-RESULT", "Result is not admitted after terminality or outside AwaitingProvider.");
        if (observed.ExpiresAt <= clock.UtcNow)
            throw new CorrelationFailure("BROKER-EXPIRED", "Correlation has expired.");

        var outcome = await provider.ClassifyAsync(observed.OperationId, observed.CorrelationId, scenario, cancellationToken);
        ValidateOutcome(observed, outcome);
        var received = RequestBroker.Advance(observed, CorrelationSignal.ValidProviderResult, clock.UtcNow);
        received = await store.AdvanceAsync(observed, received, cancellationToken);
        var completed = RequestBroker.Advance(received, CorrelationSignal.ResultNormalized, clock.UtcNow);
        completed = await store.AdvanceAsync(received, completed, cancellationToken);
        return new(completed, outcome, RequestBroker.Tombstone(completed, "CompletedNormally", clock.UtcNow));
    }

    private static void ValidateOutcome(ActiveCorrelation state, ProviderOutcome outcome)
    {
        if (outcome.OperationId != state.OperationId || outcome.CorrelationId != state.CorrelationId || outcome.ProviderAlias != CorrelationApplicationContract.ProviderAlias)
            throw new CorrelationFailure("PROVIDER-IDENTITY", "Provider outcome identity does not match.");
        if (outcome.Authoritative || outcome.Classification != CorrelationApplicationContract.ResultClassification)
            throw new CorrelationFailure("PROVIDER-AUTHORITY", "Provider output cannot be authoritative.");
        if (outcome.Observations.Count is < 1 or > 8 || outcome.Limitations.Count is < 1 or > 8)
            throw new CorrelationFailure("PROVIDER-BOUNDS", "Provider outcome exceeds admitted bounds.");
    }
}
