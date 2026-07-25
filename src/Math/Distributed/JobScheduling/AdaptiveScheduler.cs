namespace MathVerse.Math.Distributed.JobScheduling;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// An adaptive scheduler that monitors task completion rates and dynamically adjusts
/// the degree of parallelism to maximize throughput.
/// </summary>
public sealed class AdaptiveScheduler
{
    private readonly ConcurrentBag<(DateTime Start, DateTime End)> _completionHistory = new();
    private readonly int _minParallelism;
    private readonly int _maxParallelism;
    private int _currentParallelism;
    private readonly object _adjustLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveScheduler"/> class.
    /// </summary>
    /// <param name="minParallelism">Minimum allowed parallelism degree.</param>
    /// <param name="maxParallelism">Maximum allowed parallelism degree.</param>
    public AdaptiveScheduler(int minParallelism, int maxParallelism)
    {
        _minParallelism = minParallelism;
        _maxParallelism = maxParallelism;
        _currentParallelism = minParallelism;
    }

    /// <summary>
    /// Records the completion of a task with its start and end timestamps.
    /// </summary>
    /// <param name="start">UTC timestamp when the task started.</param>
    /// <param name="end">UTC timestamp when the task completed.</param>
    public void RecordCompletion(DateTime start, DateTime end)
    {
        _completionHistory.Add((start, end));
    }

    /// <summary>
    /// Gets the optimal degree of parallelism based on observed task completion patterns.
    /// Compares the average completion time of the first half of recorded tasks
    /// against the second half to detect throughput trends.
    /// </summary>
    /// <returns>The recommended number of concurrent tasks.</returns>
    public int GetOptimalParallelism()
    {
        lock (_adjustLock)
        {
            var snapshot = _completionHistory.ToArray();
            if (snapshot.Length < 4)
                return _currentParallelism;

            var sorted = new (DateTime Start, DateTime End)[snapshot.Length];
            Array.Copy(snapshot, sorted, snapshot.Length);
            Array.Sort(sorted, (a, b) => a.Start.CompareTo(b.Start));

            int midpoint = sorted.Length / 2;

            double avgFirstHalf = ComputeAverageDuration(sorted, 0, midpoint);
            double avgSecondHalf = ComputeAverageDuration(sorted, midpoint, sorted.Length);

            if (avgSecondHalf < avgFirstHalf && _currentParallelism < _maxParallelism)
            {
                _currentParallelism = System.Math.Min(_currentParallelism + 1, _maxParallelism);
            }
            else if (avgSecondHalf > avgFirstHalf && _currentParallelism > _minParallelism)
            {
                _currentParallelism = System.Math.Max(_currentParallelism - 1, _minParallelism);
            }

            return _currentParallelism;
        }
    }

    /// <summary>
    /// Gets the current parallelism degree without making adjustments.
    /// </summary>
    /// <returns>The current number of concurrent tasks allowed.</returns>
    public int GetCurrentParallelism()
    {
        lock (_adjustLock)
        {
            return _currentParallelism;
        }
    }

    /// <summary>
    /// Manually sets the parallelism degree, clamped to the configured bounds.
    /// </summary>
    /// <param name="degree">The desired parallelism degree.</param>
    public void SetParallelism(int degree)
    {
        lock (_adjustLock)
        {
            _currentParallelism = System.Math.Clamp(degree, _minParallelism, _maxParallelism);
        }
    }

    /// <summary>
    /// Gets the total number of recorded task completions.
    /// </summary>
    public int RecordedCount => _completionHistory.Count;

    /// <summary>
    /// Computes the average task duration for a slice of the sorted array.
    /// </summary>
    private static double ComputeAverageDuration((DateTime Start, DateTime End)[] sorted, int fromInclusive, int toExclusive)
    {
        if (toExclusive <= fromInclusive)
            return 0.0;

        double totalMs = 0.0;
        int count = 0;
        for (int i = fromInclusive; i < toExclusive; i++)
        {
            totalMs += (sorted[i].End - sorted[i].Start).TotalMilliseconds;
            count++;
        }
        return count > 0 ? totalMs / count : 0.0;
    }
}
