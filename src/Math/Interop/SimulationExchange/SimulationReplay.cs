namespace MathVerse.Math.Interop.SimulationExchange;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides replay capabilities for recorded simulation snapshots.
/// </summary>
public sealed class SimulationReplay
{
    private readonly List<SimulationSnapshot> _snapshots = new();
    private bool _sortedByTime;

    /// <summary>
    /// Gets the number of loaded snapshots.
    /// </summary>
    public int SnapshotCount => _snapshots.Count;

    /// <summary>
    /// Loads a collection of snapshots for replay.
    /// </summary>
    /// <param name="snapshots">The snapshots to load.</param>
    public void LoadSnapshots(IReadOnlyList<SimulationSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        _snapshots.Clear();
        _sortedByTime = false;

        for (var i = 0; i < snapshots.Count; i++)
        {
            _snapshots.Add(snapshots[i]);
        }
    }

    /// <summary>
    /// Gets the snapshot at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the snapshot.</param>
    /// <returns>The snapshot at the specified index, or null if out of range.</returns>
    public SimulationSnapshot? GetSnapshotAtIndex(int index)
    {
        if (index < 0 || index >= _snapshots.Count)
        {
            return null;
        }
        return _snapshots[index];
    }

    /// <summary>
    /// Gets the snapshot closest to the specified simulation time.
    /// </summary>
    /// <param name="time">The target simulation time.</param>
    /// <returns>The closest snapshot, or null if no snapshots are loaded.</returns>
    public SimulationSnapshot? GetSnapshotAtTime(double time)
    {
        if (_snapshots.Count == 0)
        {
            return null;
        }

        EnsureSortedByTime();

        SimulationSnapshot? closest = null;
        var minDiff = double.MaxValue;

        for (var i = 0; i < _snapshots.Count; i++)
        {
            var diff = System.Math.Abs(_snapshots[i].Timestamp.Ticks - (long)time);
            if (diff < minDiff)
            {
                minDiff = diff;
                closest = _snapshots[i];
            }
        }

        return closest;
    }

    private void EnsureSortedByTime()
    {
        if (_sortedByTime)
        {
            return;
        }

        _snapshots.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        _sortedByTime = true;
    }
}
