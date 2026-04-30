using Saos.Core;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace Saos.Z3;

/// <summary>
/// Z3 Replay Engine — Deterministic Event Replay
/// Replays event stream using pure reducers, produces terminal_hash (SHA256).
/// Authority A.13 (Replay Authority): rejects replay divergence.
/// Authority A.4 (Reducer Determinism): ensures no external dependencies.
/// </summary>
public static class ReplayEngine
{
    /// <summary>
    /// Replays a sequence of DomainEvents using the provided reducer.
    /// Returns the final state and SHA256 terminal_hash of the event stream.
    /// Decision #1: Hash is SHA-256 hex lowercase.
    /// </summary>
    public static (TState FinalState, string TerminalHash) Replay<TState>(
        TState initialState,
        IEnumerable<DomainEvent> events,
        Func<TState, DomainEvent, TState> reducer)
    {
        TState state = initialState;

        using var sha256 = SHA256.Create();
        var hashBuilder = new StringBuilder();

        foreach (var @event in events)
        {
            // Apply reducer deterministically (Authority A.5)
            state = reducer(state, @event);

            // Accumulate event into hash (immutable history, Authority A.6)
            string eventJson = JsonSerializer.Serialize(@event);
            byte[] eventBytes = Encoding.UTF8.GetBytes(eventJson);
            byte[] eventHash = sha256.ComputeHash(eventBytes);
            hashBuilder.Append(BitConverter.ToString(eventHash).Replace("-", "").ToLowerInvariant());
        }

        // Final hash of concatenated event hashes (Decision #1: SHA256 hex lowercase)
        byte[] finalHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashBuilder.ToString()));
        string terminalHash = BitConverter.ToString(finalHashBytes).Replace("-", "").ToLowerInvariant();

        return (state, terminalHash);
    }
}
