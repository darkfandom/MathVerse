namespace MathVerse.Math.Distributed.Diagnostics;

using System.Threading;

/// <summary>Tracks scheduler performance metrics including queue depths and throughput.</summary>
public sealed class SchedulerDiagnostics : IDisposable
{
    private readonly Core.TaskScheduler? _scheduler;
    private long _totalTasksScheduled;
    private long _totalTasksCompleted;
    private long _totalStealAttempts;
    private long _totalStealSuccesses;
    private long _totalQueueDepthSamples;
    private long _totalQueueDepthSum;
    private int _peakQueueDepth;
    private DateTime _startTime;
    private bool _disposed;

    /// <summary>Initializes scheduler diagnostics with an optional scheduler reference.</summary>
    /// <param name="scheduler">The task scheduler to monitor.</param>
    public SchedulerDiagnostics(Core.TaskScheduler? scheduler = null)
    {
        _scheduler = scheduler;
        _startTime = DateTime.UtcNow;
    }

    /// <summary>Records that a task was scheduled.</summary>
    public void RecordTaskScheduled()
    {
        Interlocked.Increment(ref _totalTasksScheduled);
    }

    /// <summary>Records that a task was completed.</summary>
    public void RecordTaskCompleted()
    {
        Interlocked.Increment(ref _totalTasksCompleted);
    }

    /// <summary>Records a work-stealing attempt.</summary>
    /// <param name="success">Whether the steal was successful.</param>
    public void RecordStealAttempt(bool success)
    {
        Interlocked.Increment(ref _totalStealAttempts);
        if (success)
        {
            Interlocked.Increment(ref _totalStealSuccesses);
        }
    }

    /// <summary>Records the current queue depth for averaging.</summary>
    /// <param name="depth">Current total queue depth.</param>
    public void RecordQueueDepth(int depth)
    {
        Interlocked.Increment(ref _totalQueueDepthSamples);
        Interlocked.Add(ref _totalQueueDepthSum, depth);

        int currentPeak;
        do
        {
            currentPeak = Volatile.Read(ref _peakQueueDepth);
        }
        while (depth > currentPeak && Interlocked.CompareExchange(ref _peakQueueDepth, depth, currentPeak) != currentPeak);
    }

    /// <summary>Gets the current scheduler metrics snapshot.</summary>
    /// <returns>A diagnostics summary with current metrics.</returns>
    public SchedulerMetricsSnapshot GetMetrics()
    {
        long samples = Volatile.Read(ref _totalQueueDepthSamples);
        long sum = Volatile.Read(ref _totalQueueDepthSum);

        return new SchedulerMetricsSnapshot
        {
            TotalTasksScheduled = Volatile.Read(ref _totalTasksScheduled),
            TotalTasksCompleted = Volatile.Read(ref _totalTasksCompleted),
            TotalStealAttempts = Volatile.Read(ref _totalStealAttempts),
            TotalStealSuccesses = Volatile.Read(ref _totalStealSuccesses),
            AverageQueueDepth = samples > 0 ? (double)sum / samples : 0.0,
            PeakQueueDepth = Volatile.Read(ref _peakQueueDepth),
            UptimeSeconds = (DateTime.UtcNow - _startTime).TotalSeconds,
            ThroughputPerSecond = CalculateThroughput()
        };
    }

    /// <summary>Resets all counters.</summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _totalTasksScheduled, 0);
        Interlocked.Exchange(ref _totalTasksCompleted, 0);
        Interlocked.Exchange(ref _totalStealAttempts, 0);
        Interlocked.Exchange(ref _totalStealSuccesses, 0);
        Interlocked.Exchange(ref _totalQueueDepthSamples, 0);
        Interlocked.Exchange(ref _totalQueueDepthSum, 0);
        Interlocked.Exchange(ref _peakQueueDepth, 0);
        _startTime = DateTime.UtcNow;
    }

    private double CalculateThroughput()
    {
        double elapsed = (DateTime.UtcNow - _startTime).TotalSeconds;
        long completed = Volatile.Read(ref _totalTasksCompleted);
        return elapsed > 0 ? completed / elapsed : 0.0;
    }

    /// <summary>Disposes the scheduler diagnostics.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}

/// <summary>Snapshot of scheduler performance metrics.</summary>
public sealed class SchedulerMetricsSnapshot
{
    /// <summary>Total tasks that have been scheduled.</summary>
    public long TotalTasksScheduled { get; init; }

    /// <summary>Total tasks that have completed.</summary>
    public long TotalTasksCompleted { get; init; }

    /// <summary>Total work-stealing attempts.</summary>
    public long TotalStealAttempts { get; init; }

    /// <summary>Successful work-stealing attempts.</summary>
    public long TotalStealSuccesses { get; init; }

    /// <summary>Average queue depth across all samples.</summary>
    public double AverageQueueDepth { get; init; }

    /// <summary>Peak queue depth observed.</summary>
    public int PeakQueueDepth { get; init; }

    /// <summary>Scheduler uptime in seconds.</summary>
    public double UptimeSeconds { get; init; }

    /// <summary>Tasks completed per second.</summary>
    public double ThroughputPerSecond { get; init; }
}
