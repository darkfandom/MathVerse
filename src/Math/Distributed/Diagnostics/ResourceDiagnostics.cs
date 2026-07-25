namespace MathVerse.Math.Distributed.Diagnostics;

using System.Diagnostics;
using System.Threading;

/// <summary>Monitors system resource utilization including CPU and memory.</summary>
public sealed class ResourceDiagnostics : IDisposable
{
    private long _cpuTimeMs;
    private long _gcGen0Collections;
    private long _gcGen1Collections;
    private long _gcGen2Collections;
    private long _totalMemoryAllocated;
    private long _peakWorkingSetBytes;
    private readonly Stopwatch _uptime;
    private bool _disposed;

    /// <summary>Snapshot of resource usage metrics.</summary>
    public sealed class ResourceSnapshot
    {
        /// <summary>CPU time consumed in milliseconds.</summary>
        public double CpuTimeMs { get; init; }

        /// <summary>Gen 0 garbage collection count.</summary>
        public long Gen0Collections { get; init; }

        /// <summary>Gen 1 garbage collection count.</summary>
        public long Gen1Collections { get; init; }

        /// <summary>Gen 2 garbage collection count.</summary>
        public long Gen2Collections { get; init; }

        /// <summary>Approximate total memory allocated in bytes.</summary>
        public long TotalMemoryAllocatedBytes { get; init; }

        /// <summary>Peak working set size in bytes.</summary>
        public long PeakWorkingSetBytes { get; init; }

        /// <summary>Current managed memory in bytes.</summary>
        public long CurrentManagedMemoryBytes { get; init; }

        /// <summary>Uptime of the diagnostics session in seconds.</summary>
        public double UptimeSeconds { get; init; }

        /// <summary>CPU utilization as a fraction between 0 and 1.</summary>
        public double CpuUtilization { get; init; }

        /// <summary>GC pause time estimate in milliseconds.</summary>
        public double GcPauseTimeMs { get; init; }
    }

    /// <summary>Initializes resource diagnostics and starts monitoring.</summary>
    public ResourceDiagnostics()
    {
        _uptime = Stopwatch.StartNew();
        SnapshotCounters();
    }

    /// <summary>Takes a snapshot of current resource usage.</summary>
    /// <returns>A resource usage snapshot.</returns>
    public ResourceSnapshot TakeSnapshot()
    {
        SnapshotCounters();

        double elapsed = _uptime.Elapsed.TotalSeconds;
        long cpuMs = Volatile.Read(ref _cpuTimeMs);
        long gen0 = GC.CollectionCount(0);
        long gen1 = GC.CollectionCount(1);
        long gen2 = GC.CollectionCount(2);
        long totalMem = GC.GetTotalMemory(false);
        Process currentProcess = Process.GetCurrentProcess();
        long peakWs = currentProcess.WorkingSet64;

        return new ResourceSnapshot
        {
            CpuTimeMs = cpuMs,
            Gen0Collections = gen0,
            Gen1Collections = gen1,
            Gen2Collections = gen2,
            TotalMemoryAllocatedBytes = totalMem,
            PeakWorkingSetBytes = peakWs,
            CurrentManagedMemoryBytes = totalMem,
            UptimeSeconds = elapsed,
            CpuUtilization = elapsed > 0 ? System.Math.Clamp(cpuMs / (elapsed * 1000.0), 0.0, 1.0) : 0.0,
            GcPauseTimeMs = 0.0
        };
    }

    /// <summary>Records CPU time consumed by the current operation.</summary>
    /// <param name="cpuMs">CPU time in milliseconds.</param>
    public void RecordCpuTime(long cpuMs)
    {
        Interlocked.Add(ref _cpuTimeMs, cpuMs);
    }

    /// <summary>Records memory allocation.</summary>
    /// <param name="bytes">Number of bytes allocated.</param>
    public void RecordAllocation(long bytes)
    {
        Interlocked.Add(ref _totalMemoryAllocated, bytes);
    }

    /// <summary>Resets all counters.</summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _cpuTimeMs, 0);
        Interlocked.Exchange(ref _totalMemoryAllocated, 0);
        Interlocked.Exchange(ref _peakWorkingSetBytes, 0);
        _uptime.Restart();
    }

    private void SnapshotCounters()
    {
        _gcGen0Collections = GC.CollectionCount(0);
        _gcGen1Collections = GC.CollectionCount(1);
        _gcGen2Collections = GC.CollectionCount(2);
    }

    /// <summary>Disposes the resource diagnostics.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _uptime.Stop();
            _disposed = true;
        }
    }
}
