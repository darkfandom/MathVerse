namespace MathVerse.Math.Compiler.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>Tracks optimization pass statistics: what was applied, how many times, what changed.
/// Thread-safe collection of <see cref="OptimizationRecord"/>.</summary>
public sealed class OptimizationDiagnostics
{
    private readonly ConcurrentBag<OptimizationRecord> _records = new();

    /// <summary>Records a completed optimization pass.</summary>
    /// <param name="passName">The name of the optimization pass.</param>
    /// <param name="beforeCount">Instruction count before the pass.</param>
    /// <param name="afterCount">Instruction count after the pass.</param>
    /// <param name="duration">Time taken by the pass.</param>
    public void RecordPass(string passName, int beforeCount, int afterCount, TimeSpan duration)
    {
        if (passName is null) throw new ArgumentNullException(nameof(passName));
        _records.Add(new OptimizationRecord(passName, beforeCount, afterCount, duration));
    }

    /// <summary>Returns all optimization records.</summary>
    public IReadOnlyList<OptimizationRecord> GetRecords()
    {
        return _records.ToList();
    }

    /// <summary>Returns records for a specific pass name.</summary>
    public IReadOnlyList<OptimizationRecord> GetRecords(string passName)
    {
        if (passName is null) throw new ArgumentNullException(nameof(passName));
        return _records.Where(r => r.PassName == passName).ToList();
    }

    /// <summary>Returns the total number of optimization passes recorded.</summary>
    public int TotalPasses => _records.Count;

    /// <summary>Returns the total number of passes that changed the IR.</summary>
    public int TotalChanges => _records.Count(r => r.Changed);

    /// <summary>Clears all records.</summary>
    public void Clear()
    {
        while (_records.TryTake(out _)) { }
    }
}
