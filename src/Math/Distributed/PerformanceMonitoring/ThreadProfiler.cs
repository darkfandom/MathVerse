namespace MathVerse.Math.Distributed.PerformanceMonitoring;

using System.Collections.Concurrent;

/// <summary>Represents a snapshot of utilization data for a single thread.</summary>
public sealed class ThreadUtilizationSnapshot
{
    /// <summary>The thread identifier.</summary>
    public int ThreadId { get; init; }

    /// <summary>The recorded utilization value (0.0 to 1.0).</summary>
    public double Utilization { get; init; }

    /// <summary>The timestamp when this snapshot was recorded.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>Profiles thread utilization across the application.</summary>
public sealed class ThreadProfiler
{
    private readonly ConcurrentDictionary<int, ThreadStats> _threadStats = new();
    private readonly ConcurrentBag<ThreadUtilizationSnapshot> _history = new();
    private long _idleUpdates;

    /// <summary>Represents accumulated utilization statistics for a single thread.</summary>
    private sealed class ThreadStats
    {
        /// <summary>The sum of all utilization values recorded.</summary>
        public double UtilizationSum;

        /// <summary>The number of utilization records for this thread.</summary>
        public int RecordCount;
    }

    /// <summary>Gets the number of distinct threads being tracked.</summary>
    public int ThreadCount => _threadStats.Count;

    /// <summary>Gets the total number of utilization records collected.</summary>
    public long TotalRecords
    {
        get
        {
            long total = 0;
            foreach (var stats in _threadStats.Values)
            {
                total += stats.RecordCount;
            }
            return total;
        }
    }

    /// <summary>Records a utilization measurement for the specified thread.</summary>
    /// <param name="threadId">The thread identifier.</param>
    /// <param name="utilization">The utilization value (0.0 to 1.0).</param>
    public void RecordThreadUsage(int threadId, double utilization)
    {
        double clamped = System.Math.Max(0.0, System.Math.Min(1.0, utilization));

        var stats = _threadStats.GetOrAdd(threadId, _ => new ThreadStats());
        lock (stats)
        {
            stats.UtilizationSum += clamped;
            stats.RecordCount++;
        }

        _history.Add(new ThreadUtilizationSnapshot
        {
            ThreadId = threadId,
            Utilization = clamped
        });

        if (clamped < 0.01)
        {
            Interlocked.Increment(ref _idleUpdates);
        }
    }

    /// <summary>Returns the average utilization across all threads.</summary>
    /// <returns>The average utilization as a value between 0 and 1, or 0 if no records exist.</returns>
    public double GetAverageUtilization()
    {
        if (_threadStats.IsEmpty)
        {
            return 0.0;
        }

        double totalUtilization = 0.0;
        int threadCount = 0;

        foreach (var stats in _threadStats.Values)
        {
            lock (stats)
            {
                if (stats.RecordCount > 0)
                {
                    totalUtilization += stats.UtilizationSum / stats.RecordCount;
                    threadCount++;
                }
            }
        }

        return threadCount > 0 ? totalUtilization / threadCount : 0.0;
    }

    /// <summary>Returns the average utilization for the specified thread.</summary>
    /// <param name="threadId">The thread identifier.</param>
    /// <returns>The average utilization, or 0 if the thread has no records.</returns>
    public double GetThreadUtilization(int threadId)
    {
        if (!_threadStats.TryGetValue(threadId, out var stats))
        {
            return 0.0;
        }

        lock (stats)
        {
            return stats.RecordCount > 0
                ? stats.UtilizationSum / stats.RecordCount
                : 0.0;
        }
    }

    /// <summary>Returns the number of distinct threads tracked.</summary>
    /// <returns>The thread count.</returns>
    public int GetThreadCount()
    {
        return _threadStats.Count;
    }

    /// <summary>Returns the idle percentage across all recorded data.</summary>
    /// <returns>The idle percentage as a value between 0 and 100, or 0 if no records exist.</returns>
    public double GetIdlePercentage()
    {
        long totalRecords = TotalRecords;
        if (totalRecords == 0)
        {
            return 0.0;
        }

        long idleCount = Interlocked.Read(ref _idleUpdates);
        return (double)idleCount / (double)totalRecords * 100.0;
    }

    /// <summary>Returns the thread with the highest average utilization.</summary>
    /// <returns>A tuple of thread ID and average utilization, or (0, 0) if no records exist.</returns>
    public (int ThreadId, double Utilization) GetMostUtilizedThread()
    {
        int bestId = 0;
        double bestUtil = 0.0;

        foreach (var kvp in _threadStats)
        {
            lock (kvp.Value)
            {
                if (kvp.Value.RecordCount > 0)
                {
                    double avg = kvp.Value.UtilizationSum / kvp.Value.RecordCount;
                    if (avg > bestUtil)
                    {
                        bestUtil = avg;
                        bestId = kvp.Key;
                    }
                }
            }
        }

        return (bestId, bestUtil);
    }

    /// <summary>Returns the thread with the lowest average utilization.</summary>
    /// <returns>A tuple of thread ID and average utilization, or (0, 0) if no records exist.</returns>
    public (int ThreadId, double Utilization) GetLeastUtilizedThread()
    {
        int bestId = 0;
        double bestUtil = 1.0;

        foreach (var kvp in _threadStats)
        {
            lock (kvp.Value)
            {
                if (kvp.Value.RecordCount > 0)
                {
                    double avg = kvp.Value.UtilizationSum / kvp.Value.RecordCount;
                    if (avg < bestUtil)
                    {
                        bestUtil = avg;
                        bestId = kvp.Key;
                    }
                }
            }
        }

        return (bestId, bestUtil);
    }

    /// <summary>Returns the utilization history for the specified thread.</summary>
    /// <param name="threadId">The thread identifier.</param>
    /// <returns>An array of utilization snapshots for the thread.</returns>
    public ThreadUtilizationSnapshot[] GetHistory(int threadId)
    {
        return _history
            .Where(s => s.ThreadId == threadId)
            .ToArray();
    }

    /// <summary>Resets all profiling data.</summary>
    public void Reset()
    {
        _threadStats.Clear();
        while (_history.TryTake(out _)) { }
        Interlocked.Exchange(ref _idleUpdates, 0);
    }
}
