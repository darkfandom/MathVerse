namespace MathVerse.Math.Compiler.Runtime;

using System;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// Collects runtime statistics including allocation rates, GC pressure, and thread utilization.
/// All counters are updated via lock-free Interlocked operations for thread safety.
/// </summary>
public sealed class RuntimeStatistics
{
    private long _totalAllocatedBytes;
    private long _allocationCount;
    private long _gcCollectionCount;
    private long _gcTotalPauseMilliseconds;
    private long _threadActiveTicks;
    private long _threadIdleTicks;
    private long _peakWorkingSetBytes;
    private long _startTimeTicks;
    private long _instructionCount;
    private long _compilationTimeTicks;

    /// <summary>Total bytes allocated since tracking began.</summary>
    public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

    /// <summary>Total number of allocation requests.</summary>
    public long AllocationCount => Interlocked.Read(ref _allocationCount);

    /// <summary>Total GC collections observed.</summary>
    public long GcCollectionCount => Interlocked.Read(ref _gcCollectionCount);

    /// <summary>Total GC pause time in milliseconds.</summary>
    public long GcTotalPauseMilliseconds => Interlocked.Read(ref _gcTotalPauseMilliseconds);

    /// <summary>Peak working set in bytes.</summary>
    public long PeakWorkingSetBytes => Interlocked.Read(ref _peakWorkingSetBytes);

    /// <summary>Total instructions compiled.</summary>
    public long InstructionCount => Interlocked.Read(ref _instructionCount);

    /// <summary>Total compilation time in ticks.</summary>
    public long CompilationTimeTicks => Interlocked.Read(ref _compilationTimeTicks);

    /// <summary>Total active thread time in ticks.</summary>
    public long ThreadActiveTicks => Interlocked.Read(ref _threadActiveTicks);

    /// <summary>Total idle thread time in ticks.</summary>
    public long ThreadIdleTicks => Interlocked.Read(ref _threadIdleTicks);

    /// <summary>Thread utilization ratio (0.0 to 1.0).</summary>
    public double ThreadUtilization
    {
        get
        {
            var active = Interlocked.Read(ref _threadActiveTicks);
            var idle = Interlocked.Read(ref _threadIdleTicks);
            var total = active + idle;
            return total > 0 ? (double)active / total : 0.0;
        }
    }

    /// <summary>Allocation rate in bytes per second (approximate).</summary>
    public double AllocationRate
    {
        get
        {
            var bytes = Interlocked.Read(ref _totalAllocatedBytes);
            var elapsed = Stopwatch.GetTimestamp() - Interlocked.Read(ref _startTimeTicks);
            var freq = (double)TimeSpan.TicksPerSecond;
            var seconds = elapsed / freq;
            return seconds > 0 ? bytes / seconds : 0.0;
        }
    }

    /// <summary>Average bytes per allocation.</summary>
    public double AverageAllocationSize
    {
        get
        {
            var bytes = Interlocked.Read(ref _totalAllocatedBytes);
            var count = Interlocked.Read(ref _allocationCount);
            return count > 0 ? (double)bytes / count : 0.0;
        }
    }

    /// <summary>
    /// Initializes a new runtime statistics collector.
    /// </summary>
    public RuntimeStatistics()
    {
        _startTimeTicks = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Records an allocation event.
    /// </summary>
    /// <param name="bytes">The number of bytes allocated.</param>
    public void RecordAllocation(long bytes)
    {
        Interlocked.Add(ref _totalAllocatedBytes, bytes);
        Interlocked.Increment(ref _allocationCount);

        var current = Volatile.Read(ref _peakWorkingSetBytes);
        var allocated = Interlocked.Read(ref _totalAllocatedBytes);
        if (allocated > current)
        {
            Interlocked.CompareExchange(ref _peakWorkingSetBytes, allocated, current);
        }
    }

    /// <summary>
    /// Records a GC collection event.
    /// </summary>
    /// <param name="generation">The GC generation collected.</param>
    /// <param name="pauseMilliseconds">The pause duration in milliseconds.</param>
    public void RecordGcCollection(int generation, long pauseMilliseconds)
    {
        Interlocked.Increment(ref _gcCollectionCount);
        Interlocked.Add(ref _gcTotalPauseMilliseconds, pauseMilliseconds);
    }

    /// <summary>
    /// Records active thread time.
    /// </summary>
    /// <param name="ticks">The number of ticks the thread was active.</param>
    public void RecordThreadActive(long ticks)
    {
        Interlocked.Add(ref _threadActiveTicks, ticks);
    }

    /// <summary>
    /// Records idle thread time.
    /// </summary>
    /// <param name="ticks">The number of ticks the thread was idle.</param>
    public void RecordThreadIdle(long ticks)
    {
        Interlocked.Add(ref _threadIdleTicks, ticks);
    }

    /// <summary>
    /// Records compilation statistics.
    /// </summary>
    /// <param name="instructionCount">Number of instructions compiled.</param>
    /// <param name="compilationTicks">Time spent compiling in ticks.</param>
    public void RecordCompilation(long instructionCount, long compilationTicks)
    {
        Interlocked.Add(ref _instructionCount, instructionCount);
        Interlocked.Add(ref _compilationTimeTicks, compilationTicks);
    }

    /// <summary>
    /// Snapshots the current statistics into an immutable record.
    /// </summary>
    /// <returns>A snapshot of the current runtime statistics.</returns>
    public RuntimeStatisticsSnapshot Snapshot()
    {
        return new RuntimeStatisticsSnapshot
        {
            TotalAllocatedBytes = Interlocked.Read(ref _totalAllocatedBytes),
            AllocationCount = Interlocked.Read(ref _allocationCount),
            GcCollectionCount = Interlocked.Read(ref _gcCollectionCount),
            GcTotalPauseMilliseconds = Interlocked.Read(ref _gcTotalPauseMilliseconds),
            PeakWorkingSetBytes = Interlocked.Read(ref _peakWorkingSetBytes),
            InstructionCount = Interlocked.Read(ref _instructionCount),
            CompilationTimeTicks = Interlocked.Read(ref _compilationTimeTicks),
            ThreadActiveTicks = Interlocked.Read(ref _threadActiveTicks),
            ThreadIdleTicks = Interlocked.Read(ref _threadIdleTicks)
        };
    }

    /// <summary>
    /// Resets all counters to zero.
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _totalAllocatedBytes, 0);
        Interlocked.Exchange(ref _allocationCount, 0);
        Interlocked.Exchange(ref _gcCollectionCount, 0);
        Interlocked.Exchange(ref _gcTotalPauseMilliseconds, 0);
        Interlocked.Exchange(ref _peakWorkingSetBytes, 0);
        Interlocked.Exchange(ref _instructionCount, 0);
        Interlocked.Exchange(ref _compilationTimeTicks, 0);
        Interlocked.Exchange(ref _threadActiveTicks, 0);
        Interlocked.Exchange(ref _threadIdleTicks, 0);
        _startTimeTicks = Stopwatch.GetTimestamp();
    }
}

/// <summary>
/// Immutable snapshot of runtime statistics at a point in time.
/// </summary>
public sealed class RuntimeStatisticsSnapshot
{
    /// <summary>Total bytes allocated.</summary>
    public long TotalAllocatedBytes { get; init; }

    /// <summary>Total allocation count.</summary>
    public long AllocationCount { get; init; }

    /// <summary>Total GC collection count.</summary>
    public long GcCollectionCount { get; init; }

    /// <summary>Total GC pause in milliseconds.</summary>
    public long GcTotalPauseMilliseconds { get; init; }

    /// <summary>Peak working set in bytes.</summary>
    public long PeakWorkingSetBytes { get; init; }

    /// <summary>Total instructions compiled.</summary>
    public long InstructionCount { get; init; }

    /// <summary>Total compilation time in ticks.</summary>
    public long CompilationTimeTicks { get; init; }

    /// <summary>Total active thread ticks.</summary>
    public long ThreadActiveTicks { get; init; }

    /// <summary>Total idle thread ticks.</summary>
    public long ThreadIdleTicks { get; init; }

    /// <summary>GC pressure score (higher = more pressure).</summary>
    public double GcPressure => GcCollectionCount > 0
        ? (double)GcTotalPauseMilliseconds / GcCollectionCount
        : 0.0;
}
