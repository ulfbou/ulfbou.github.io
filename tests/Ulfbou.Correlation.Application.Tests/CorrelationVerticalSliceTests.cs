using FluentAssertions;
using Ulfbou.Correlation.Application;
using Ulfbou.Correlation.Core;
using Xunit;

namespace Ulfbou.Correlation.Application.Tests;

public sealed class CorrelationVerticalSliceTests
{
    private static readonly ArchitectureScenario Scenario = new(
        "git-backed-correlation-cleanup",
        "Git-governed terminal cleanup",
        ["Active correlation state is stored in S3."],
        ["Can a stale tombstone delete newer active state?"]);

    [Fact]
    public async Task Bounded_scenario_completes_with_non_authoritative_candidate_analysis()
    {
        var store = new FakeStore();
        var clock = new FakeClock();
        var request = new RequestBroker(store, new FakeIdentities(), clock);
        var awaiting = await request.StartAsync(Scenario);
        var result = await new ResultBroker(store, new FakeProvider(), clock).CompleteAsync(awaiting, Scenario);

        result.State.State.Should().Be(CorrelationState.Completed);
        result.Outcome.Authoritative.Should().BeFalse();
        result.Outcome.Classification.Should().Be("CandidateAnalysis");
        result.Tombstone.ExpectedStateRevision.Should().Be(result.State.StateRevision);
        result.Tombstone.ExpectedStateSha256.Should().Be(result.State.StateSha256);
    }

    [Fact]
    public async Task Cancellation_is_terminal_and_late_result_is_rejected()
    {
        var store = new FakeStore();
        var clock = new FakeClock();
        var request = new RequestBroker(store, new FakeIdentities(), clock);
        var awaiting = await request.StartAsync(Scenario);
        var cancelled = await request.CancelAsync(awaiting.CorrelationId, awaiting.OperationId, awaiting.StateRevision);
        cancelled.State.State.Should().Be(CorrelationState.Cancelled);

        var action = () => new ResultBroker(store, new FakeProvider(), clock).CompleteAsync(awaiting, Scenario);
        await action.Should().ThrowAsync<CorrelationFailure>().Where(x => x.Code == "STATE-REVISION" || x.Code == "BROKER-LATE-RESULT");
    }

    [Fact]
    public async Task Stale_state_cannot_advance_storage()
    {
        var store = new FakeStore();
        var clock = new FakeClock();
        var request = new RequestBroker(store, new FakeIdentities(), clock);
        var awaiting = await request.StartAsync(Scenario);
        await request.CancelAsync(awaiting.CorrelationId, awaiting.OperationId, awaiting.StateRevision);
        var staleNext = awaiting with { State = CorrelationState.ResultReceived, StateRevision = awaiting.StateRevision + 1 };
        var action = () => store.AdvanceAsync(awaiting, staleNext, default);
        await action.Should().ThrowAsync<CorrelationFailure>().Where(x => x.Code == "STORE-CONFLICT");
    }

    private sealed class FakeClock : ICorrelationClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 10, 24, 10, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeIdentities : ICorrelationIdentitySource
    {
        public string NewCorrelationId() => "corr-01jk6m2w8g6wmx9zq7v4c2ab";
        public string NewOperationId() => "op-01jk6m2w8g6wmx9zq7v4c2ac";
        public string NewRequestId() => "req-01jk6m2w8g6wmx9zq7v4c2ad";
    }

    private sealed class FakeProvider : IArchitectureProvider
    {
        public Task<ProviderOutcome> ClassifyAsync(string operationId, string correlationId, ArchitectureScenario scenario, CancellationToken cancellationToken) =>
            Task.FromResult(new ProviderOutcome(
                operationId,
                correlationId,
                "google-gemini-prototype",
                "CandidateAnalysis",
                [new(scenario.Questions[0], "No", "Deletion requires exact revision and digest identity.")],
                [CorrelationApplicationContract.PublicLimitation],
                false));
    }

    private sealed class FakeStore : ICorrelationStateStore
    {
        private readonly Dictionary<string, ActiveCorrelation> states = new(StringComparer.Ordinal);
        public Task CreateAsync(ActiveCorrelation state, CancellationToken cancellationToken)
        {
            if (!states.TryAdd(state.CorrelationId, state)) throw new CorrelationFailure("STORE-EXISTS", "Correlation already exists.");
            return Task.CompletedTask;
        }
        public Task<ActiveCorrelation?> ReadAsync(string correlationId, CancellationToken cancellationToken) =>
            Task.FromResult(states.GetValueOrDefault(correlationId));
        public Task<ActiveCorrelation> AdvanceAsync(ActiveCorrelation expected, ActiveCorrelation next, CancellationToken cancellationToken)
        {
            if (!states.TryGetValue(expected.CorrelationId, out var observed) || observed.StateRevision != expected.StateRevision || observed.StateSha256 != expected.StateSha256)
                throw new CorrelationFailure("STORE-CONFLICT", "Expected state is stale.");
            if (next.StateRevision != expected.StateRevision + 1 || next.PreviousStateSha256 != expected.StateSha256)
                throw new CorrelationFailure("STORE-ADVANCE", "Next state does not form an exact revision chain.");
            states[expected.CorrelationId] = next;
            return Task.FromResult(next);
        }
    }
}
