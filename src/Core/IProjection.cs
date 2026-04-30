using System.Text.Json;

namespace Saos.Core;

/// <summary>
/// DomainEvent shape per Decision #3: {id:uuidv4, ts:ISO8601, type:string, payload:object}
/// Immutable event record for deterministic replay.
/// </summary>
public record DomainEvent(
    string Id, // UUID v4
    DateTimeOffset Ts, // ISO8601 UTC timestamp
    string Type, // Event type string
    JsonElement Payload // Arbitrary object payload
);

/// <summary>
/// Pure reducer signature for state transitions.
/// Implements deterministic state evolution from events.
/// </summary>
public delegate TState Reducer<TState>(TState state, DomainEvent @event);

/// <summary>
/// Projection interface for derived read models.
/// Projections are non-authoritative, owned by applications (Authority B.2).
/// </summary>
public interface IProjection<TState>
{
    /// <summary>
    /// Applies a DomainEvent to the projection state using the pure reducer.
    /// Must be deterministic and free of external dependencies.
    /// </summary>
    TState Apply(TState state, DomainEvent @event);

    /// <summary>
    /// Returns the initial (empty) state for this projection.
    /// </summary>
    TState InitialState { get; }
}
