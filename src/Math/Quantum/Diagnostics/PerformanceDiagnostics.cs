namespace MathVerse.Math.Quantum.Diagnostics;

/// <summary>
/// Tracks performance metrics for quantum operations.
/// </summary>
public sealed class PerformanceDiagnostics
{
    private long _totalOperations;
    private TimeSpan _totalTime;
    private long _memoryAllocated;
    private readonly object _lock = new();

    /// <summary>
    /// Gets the total number of operations recorded.
    /// </summary>
    public long TotalOperations => Interlocked.Read(ref _totalOperations);

    /// <summary>
    /// Gets the total time spent across all operations.
    /// </summary>
    public TimeSpan TotalTime
    {
        get { lock (_lock) { return _totalTime; } }
    }

    /// <summary>
    /// Gets the total memory allocated across all operations.
    /// </summary>
    public long MemoryAllocated => Interlocked.Read(ref _memoryAllocated);

    /// <summary>
    /// Gets the operations per second rate.
    /// </summary>
    public double OperationsPerSecond
    {
        get
        {
            long ops = Interlocked.Read(ref _totalOperations);
            if (ops == 0) return 0.0;
            lock (_lock)
            {
                return _totalTime.TotalSeconds > 0 ? ops / _totalTime.TotalSeconds : 0.0;
            }
        }
    }

    /// <summary>
    /// Gets the average operation time.
    /// </summary>
    public TimeSpan AverageOperationTime
    {
        get
        {
            long ops = Interlocked.Read(ref _totalOperations);
            if (ops == 0) return TimeSpan.Zero;
            lock (_lock)
            {
                return TimeSpan.FromTicks(_totalTime.Ticks / ops);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceDiagnostics"/> class.
    /// </summary>
    public PerformanceDiagnostics()
    {
        _totalOperations = 0;
        _totalTime = TimeSpan.Zero;
        _memoryAllocated = 0;
    }

    /// <summary>
    /// Records a completed operation with its duration and memory usage.
    /// </summary>
    /// <param name="duration">The operation duration.</param>
    /// <param name="memoryBytes">The memory allocated in bytes.</param>
    public void RecordOperation(TimeSpan duration, long memoryBytes)
    {
        Interlocked.Increment(ref _totalOperations);
        lock (_lock)
        {
            _totalTime += duration;
        }
        Interlocked.Add(ref _memoryAllocated, memoryBytes);
    }

    /// <summary>
    /// Returns a summary string of the performance diagnostics.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public string GetSummary()
    {
        return $"Performance: {TotalOperations} ops, avg {AverageOperationTime.TotalMilliseconds:F2}ms, {OperationsPerSecond:F2} ops/s, {MemoryAllocated} bytes allocated";
    }
}
