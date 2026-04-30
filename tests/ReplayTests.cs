using System.Text.Json;
using Xunit;

namespace Saos.Tests;

/// <summary>
/// Tests for Z3 ReplayEngine deterministic event replay.
/// Validates Decision #3 (DomainEvent shape) and Decision #1 (SHA256 hash).
/// </summary>
public class ReplayTests
{
    [Fact]
    public void Replay_WithThreeEvents_ProducesDeterministicTerminalHash()
    {
        // Arrange: 3 sample DomainEvents per Decision #3
        var events = new[]
        {
            new DomainEvent(
                Id: "a1b2c3d4-0001-4e5f-8901-234567890abc",
                Ts: DateTimeOffset.Parse("2026-04-29T10:00:00Z"),
                Type: "inventory.item.created",
                Payload: JsonDocument.Parse("{\"itemId\":\"item-001\",\"quantity\":10}").RootElement
            ),
            new DomainEvent(
                Id: "a1b2c3d4-0002-4e5f-8901-234567890abc",
                Ts: DateTimeOffset.Parse("2026-04-29T10:01:00Z"),
                Type: "inventory.item.updated",
                Payload: JsonDocument.Parse("{\"itemId\":\"item-001\",\"quantity\":15}").RootElement
            ),
            new DomainEvent(
                Id: "a1b2c3d4-0003-4e5f-8901-234567890abc",
                Ts: DateTimeOffset.Parse("2026-04-29T10:02:00Z"),
                Type: "inventory.item.removed",
                Payload: JsonDocument.Parse("{\"itemId\":\"item-001\"}").RootElement
            )
        };

        int initialState = 0;
        Func<int, DomainEvent, int> reducer = (state, evt) => state + 1;

        // Act: Replay twice to verify determinism
        var (finalState1, terminalHash1) = Z3.ReplayEngine.Replay(initialState, events, reducer);
        var (finalState2, terminalHash2) = Z3.ReplayEngine.Replay(initialState, events, reducer);

        // Assert: Deterministic terminal_hash (Decision #1: SHA256 hex lowercase)
        Assert.Equal(terminalHash1, terminalHash2);
        Assert.Equal(3, finalState1);
        Assert.Equal(3, finalState2);
        Assert.Matches("^[a-f0-9]{64}$", terminalHash1); // SHA256 hex lowercase
    }

    [Fact]
    public void Replay_WithNoEvents_ProducesEmptyHash()
    {
        // Arrange: Empty event stream
        var events = Array.Empty<DomainEvent>();
        int initialState = 42;
        Func<int, DomainEvent, int> reducer = (state, evt) => state;

        // Act
        var (finalState, terminalHash) = Z3.ReplayEngine.Replay(initialState, events, reducer);

        // Assert: State unchanged, hash produced
        Assert.Equal(42, finalState);
        Assert.NotNull(terminalHash);
        Assert.Matches("^[a-f0-9]{64}$", terminalHash);
    }

    [Fact]
    public void Replay_WithDifferentReducers_ProducesDifferentState()
    {
        // Arrange
        var events = new[]
        {
            new DomainEvent(
                Id: "test-001",
                Ts: DateTimeOffset.UtcNow,
                Type: "test.event",
                Payload: JsonDocument.Parse("{\"value\":5}").RootElement
            )
        };

        Func<int, DomainEvent, int> reducer1 = (state, evt) => state + 1;
        Func<int, DomainEvent, int> reducer2 = (state, evt) => state + 2;

        // Act
        var (finalState1, _) = Z3.ReplayEngine.Replay(0, events, reducer1);
        var (finalState2, _) = Z3.ReplayEngine.Replay(0, events, reducer2);

        // Assert: Different reducers produce different outcomes
        Assert.NotEqual(finalState1, finalState2);
    }
}
