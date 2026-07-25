namespace MathVerse.Math.Compiler.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

/// <summary>Aggregated performance report containing timing and memory usage metrics.</summary>
public sealed class PerformanceReport
{
    /// <summary>Timing records for each pass, keyed by pass name.</summary>
    public IReadOnlyDictionary<string, TimeSpan> PassTimings { get; }
    /// <summary>Peak memory usage in bytes during compilation.</summary>
    public long PeakMemoryBytes { get; }
    /// <summary>Total compilation wall-clock time.</summary>
    public TimeSpan TotalTime { get; }
    /// <summary>The time when compilation started.</summary>
    public DateTime StartTime { get; }
    /// <summary>The time when compilation ended.</summary>
    public DateTime EndTime { get; }

    /// <summary>Initializes a new instance of the <see cref="PerformanceReport"/> class.</summary>
    public PerformanceReport(IReadOnlyDictionary<string, TimeSpan> passTimings, long peakMemoryBytes, TimeSpan totalTime, DateTime startTime, DateTime endTime)
    {
        PassTimings = passTimings ?? throw new ArgumentNullException(nameof(passTimings));
        PeakMemoryBytes = peakMemoryBytes;
        TotalTime = totalTime;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>Returns a formatted summary string of this report.</summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Performance Report:");
        sb.AppendLine($"  Total time: {TotalTime.TotalMilliseconds:F2}ms");
        sb.AppendLine($"  Peak memory: {PeakMemoryBytes / 1024.0:F2} KB");

        if (PassTimings.Count > 0)
        {
            sb.AppendLine("  Pass timings:");
            foreach (var (pass, time) in PassTimings.OrderByDescending(kv => kv.Value))
                sb.AppendLine($"    {pass}: {time.TotalMilliseconds:F2}ms");
        }

        return sb.ToString();
    }
}
