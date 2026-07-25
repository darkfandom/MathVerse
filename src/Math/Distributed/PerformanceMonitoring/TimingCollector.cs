namespace MathVerse.Math.Distributed.PerformanceMonitoring;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>Collects and aggregates timing data for operations with percentile calculations.</summary>
public sealed class TimingCollector
{
    private readonly ConcurrentBag<(string Operation, TimeSpan Elapsed)> _records = new();
    private readonly ConcurrentDictionary<string, List<TimeSpan>> _sortedCache = new();
    private long _totalRecords;

    /// <summary>Gets the total number of timing records collected.</summary>
    public long TotalRecords => Interlocked.Read(ref _totalRecords);

    /// <summary>Records a timing measurement for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <param name="elapsed">The elapsed time to record.</param>
    public void Record(string operation, TimeSpan elapsed)
    {
        _records.Add((operation, elapsed));
        Interlocked.Increment(ref _totalRecords);
        _sortedCache.TryRemove(operation, out _);
    }

    /// <summary>Measures and records the execution time of an action for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <param name="action">The action to measure.</param>
    public void RecordAction(string operation, Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        Record(operation, sw.Elapsed);
    }

    /// <summary>Measures and records the execution time of a function for the specified operation.</summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="operation">The operation name.</param>
    /// <param name="func">The function to measure.</param>
    /// <returns>The result of the function.</returns>
    public T RecordFunction<T>(string operation, Func<T> func)
    {
        var sw = Stopwatch.StartNew();
        T result = func();
        sw.Stop();
        Record(operation, sw.Elapsed);
        return result;
    }

    /// <summary>Returns the average elapsed time for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The average elapsed time, or <see cref="TimeSpan.Zero"/> if no records exist.</returns>
    public TimeSpan GetAverage(string operation)
    {
        var times = GetSortedTimes(operation);
        if (times.Count == 0)
        {
            return TimeSpan.Zero;
        }

        long totalTicks = 0;
        foreach (var t in times)
        {
            totalTicks += t.Ticks;
        }
        return TimeSpan.FromTicks(totalTicks / times.Count);
    }

    /// <summary>Returns the 95th percentile elapsed time for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The P95 elapsed time, or <see cref="TimeSpan.Zero"/> if no records exist.</returns>
    public TimeSpan GetP95(string operation)
    {
        return GetPercentile(operation, 0.95);
    }

    /// <summary>Returns the 99th percentile elapsed time for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The P99 elapsed time, or <see cref="TimeSpan.Zero"/> if no records exist.</returns>
    public TimeSpan GetP99(string operation)
    {
        return GetPercentile(operation, 0.99);
    }

    /// <summary>Returns the median (50th percentile) elapsed time for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The median elapsed time, or <see cref="TimeSpan.Zero"/> if no records exist.</returns>
    public TimeSpan GetMedian(string operation)
    {
        return GetPercentile(operation, 0.50);
    }

    /// <summary>Returns the minimum elapsed time for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The minimum elapsed time, or <see cref="TimeSpan.Zero"/> if no records exist.</returns>
    public TimeSpan GetMin(string operation)
    {
        var times = GetSortedTimes(operation);
        return times.Count > 0 ? times[0] : TimeSpan.Zero;
    }

    /// <summary>Returns the maximum elapsed time for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The maximum elapsed time, or <see cref="TimeSpan.Zero"/> if no records exist.</returns>
    public TimeSpan GetMax(string operation)
    {
        var times = GetSortedTimes(operation);
        return times.Count > 0 ? times[^1] : TimeSpan.Zero;
    }

    /// <summary>Returns the total elapsed time for the specified operation across all records.</summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The total elapsed time.</returns>
    public TimeSpan GetTotal(string operation)
    {
        long totalTicks = 0;
        foreach (var record in _records)
        {
            if (record.Operation == operation)
            {
                totalTicks += record.Elapsed.Ticks;
            }
        }
        return TimeSpan.FromTicks(totalTicks);
    }

    /// <summary>Returns the number of records for the specified operation.</summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The record count.</returns>
    public int GetCount(string operation)
    {
        int count = 0;
        foreach (var record in _records)
        {
            if (record.Operation == operation)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>Returns the names of all operations that have recorded timing data.</summary>
    /// <returns>An array of operation names.</returns>
    public string[] GetOperationNames()
    {
        return _records.Select(r => r.Operation).Distinct().ToArray();
    }

    /// <summary>Clears all recorded timing data.</summary>
    public void Clear()
    {
        while (_records.TryTake(out _)) { }
        _sortedCache.Clear();
        Interlocked.Exchange(ref _totalRecords, 0);
    }

    private TimeSpan GetPercentile(string operation, double percentile)
    {
        var times = GetSortedTimes(operation);
        if (times.Count == 0)
        {
            return TimeSpan.Zero;
        }

        int index = (int)System.Math.Ceiling(percentile * times.Count) - 1;
        index = System.Math.Max(0, System.Math.Min(index, times.Count - 1));
        return times[index];
    }

    private List<TimeSpan> GetSortedTimes(string operation)
    {
        return _sortedCache.GetOrAdd(operation, _ =>
        {
            var times = new List<TimeSpan>();
            foreach (var record in _records)
            {
                if (record.Operation == operation)
                {
                    times.Add(record.Elapsed);
                }
            }
            times.Sort();
            return times;
        });
    }
}
