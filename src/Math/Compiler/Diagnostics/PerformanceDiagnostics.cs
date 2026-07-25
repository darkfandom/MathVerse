namespace MathVerse.Math.Compiler.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>Collects performance metrics during compilation: time per pass, memory usage.
/// Thread-safe.</summary>
public sealed class PerformanceDiagnostics
{
    private readonly ConcurrentDictionary<string, TimeSpan> _passTimings = new();
    private readonly Stopwatch _totalTimer = Stopwatch.StartNew();
    private readonly DateTime _startTime = DateTime.UtcNow;
    private long _peakMemoryBytes;

    /// <summary>Records timing for a specific compilation pass.</summary>
    /// <param name="passName">The name of the pass.</param>
    /// <param name="duration">How long the pass took.</param>
    public void RecordPassTiming(string passName, TimeSpan duration)
    {
        if (passName is null) throw new ArgumentNullException(nameof(passName));
        _passTimings.AddOrUpdate(passName, duration, (_, existing) => existing + duration);

        var currentMemory = GC.GetTotalMemory(false);
        InterlockedAdd(ref _peakMemoryBytes, currentMemory);
    }

    /// <summary>Records pass timing using a start and end time.</summary>
    public void RecordPassTiming(string passName, DateTime start, DateTime end)
    {
        RecordPassTiming(passName, end - start);
    }

    /// <summary>Produces a performance report with all collected metrics.</summary>
    public PerformanceReport GetReport()
    {
        _totalTimer.Stop();
        var endTime = DateTime.UtcNow;
        return new PerformanceReport(
            new Dictionary<string, TimeSpan>(_passTimings),
            InterlockedRead(ref _peakMemoryBytes),
            _totalTimer.Elapsed,
            _startTime,
            endTime);
    }

    /// <summary>Clears all collected metrics.</summary>
    public void Clear()
    {
        _passTimings.Clear();
        _totalTimer.Restart();
        InterlockedExchange(ref _peakMemoryBytes, 0);
    }

    private static void InterlockedAdd(ref long value, long addend)
    {
        while (true)
        {
            var old = InterlockedRead(ref value);
            if (Interlocked.CompareExchange(ref value, Math.Max(old, addend), old) == old)
                break;
        }
    }

    private static long InterlockedRead(ref long value) => System.Threading.Interlocked.Read(ref value);
    private static void InterlockedExchange(ref long value, long newVal) => System.Threading.Interlocked.Exchange(ref value, newVal);
}
