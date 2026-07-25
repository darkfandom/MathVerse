namespace MathVerse.Math.Interop.SimulationExchange;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a captured snapshot of simulation state at a specific point in time.
/// </summary>
public sealed class SimulationSnapshot
{
    /// <summary>
    /// Gets or sets the unique snapshot identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this snapshot was taken.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the simulation type identifier.
    /// </summary>
    public string SimulationType { get; set; } = string.Empty;

    /// <summary>
    /// Gets the state data dictionary.
    /// </summary>
    public Dictionary<string, object> StateData { get; } = new();

    /// <summary>
    /// Gets or sets the serialized binary state data.
    /// </summary>
    public byte[] SerializedState { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Manages capturing and restoring simulation snapshots.
/// </summary>
public sealed class SnapshotManager
{
    /// <summary>
    /// Captures the current simulation state into a snapshot.
    /// </summary>
    /// <param name="simulationType">The type of simulation being captured.</param>
    /// <param name="state">The current state data.</param>
    /// <returns>A new simulation snapshot.</returns>
    public SimulationSnapshot Capture(string simulationType, Dictionary<string, object> state)
    {
        ArgumentException.ThrowIfNullOrEmpty(simulationType);
        ArgumentNullException.ThrowIfNull(state);

        var snapshot = new SimulationSnapshot
        {
            Id = Guid.NewGuid().ToString("N"),
            Timestamp = DateTimeOffset.UtcNow,
            SimulationType = simulationType
        };

        foreach (var kvp in state)
        {
            snapshot.StateData[kvp.Key] = kvp.Value;
        }

        return snapshot;
    }

    /// <summary>
    /// Restores the simulation state from a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore from.</param>
    /// <returns>A dictionary containing the restored state data.</returns>
    public Dictionary<string, object> Restore(SimulationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var restored = new Dictionary<string, object>();
        foreach (var kvp in snapshot.StateData)
        {
            restored[kvp.Key] = kvp.Value;
        }
        return restored;
    }
}
