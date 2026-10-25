using Ulfbou.Correlation.Core;

namespace Ulfbou.Correlation.Application;

public static class CorrelationApplicationContract
{
    public const string Capability = "architecture-scenario-classification-v1";
    public const string ProviderAlias = "google-gemini-prototype";
    public const string ResultClassification = "CandidateAnalysis";
    public const string PublicLimitation = "Provider-generated candidate analysis; not accepted architectural truth.";
}

public sealed record ArchitectureScenario(
    string ScenarioId,
    string Title,
    IReadOnlyList<string> Facts,
    IReadOnlyList<string> Questions);

public sealed record ProviderObservation(string Question, string Assessment, string Rationale);

public sealed record ProviderOutcome(
    string OperationId,
    string CorrelationId,
    string ProviderAlias,
    string Classification,
    IReadOnlyList<ProviderObservation> Observations,
    IReadOnlyList<string> Limitations,
    bool Authoritative);

public sealed record ActiveCorrelation(
    string CorrelationId,
    string OperationId,
    string RequestId,
    CorrelationState State,
    int StateRevision,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset ExpiresAt,
    string PreviousStateSha256,
    string StateSha256);

public sealed record TombstoneEvent(
    string EventId,
    string CorrelationId,
    string OperationId,
    CorrelationState TerminalState,
    string ReasonCode,
    int ExpectedStateRevision,
    string ExpectedStateSha256,
    DateTimeOffset TerminatedAt);

public sealed record CompletedCorrelation(ActiveCorrelation State, ProviderOutcome Outcome, TombstoneEvent Tombstone);
public sealed record CancelledCorrelation(ActiveCorrelation State, TombstoneEvent Tombstone, bool WasAlreadyTerminal);

public interface ICorrelationStateStore
{
    Task CreateAsync(ActiveCorrelation state, CancellationToken cancellationToken);
    Task<ActiveCorrelation?> ReadAsync(string correlationId, CancellationToken cancellationToken);
    Task<ActiveCorrelation> AdvanceAsync(ActiveCorrelation expected, ActiveCorrelation next, CancellationToken cancellationToken);
}

public interface IArchitectureProvider
{
    Task<ProviderOutcome> ClassifyAsync(
        string operationId,
        string correlationId,
        ArchitectureScenario scenario,
        CancellationToken cancellationToken);
}

public interface ICorrelationIdentitySource
{
    string NewCorrelationId();
    string NewOperationId();
    string NewRequestId();
}

public interface ICorrelationClock
{
    DateTimeOffset UtcNow { get; }
}
