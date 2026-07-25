namespace MathVerse.Math.Distributed.PerformanceMonitoring;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>Represents a profiling report for a single region.</summary>
public sealed class RegionReport
{
    /// <summary>The name of the profiled region.</summary>
    public string Name { get; init; } = "";

    /// <summary>The total elapsed time for the region.</summary>
    public TimeSpan TotalElapsed { get; init; }

    /// <summary>The number of times the region was entered.</summary>
    public int ExecutionCount { get; init; }

    /// <summary>The average elapsed time per execution.</summary>
    public TimeSpan AverageElapsed { get; init; }

    /// <summary>The minimum elapsed time across all executions.</summary>
    public TimeSpan MinElapsed { get; init; }

    /// <summary>The maximum elapsed time across all executions.</summary>
    public TimeSpan MaxElapsed { get; init; }
}

/// <summary>Represents the complete profiling report.</summary>
public sealed class ProfilerReport
{
    /// <summary>Reports for each profiled region.</summary>
    public List<RegionReport> Regions { get; init; } = new();

    /// <summary>The total elapsed time for the entire profiling session.</summary>
    public TimeSpan TotalSessionElapsed { get; init; }

    /// <summary>The timestamp when profiling started.</summary>
    public DateTime StartTime { get; init; }

    /// <summary>The timestamp when profiling ended.</summary>
    public DateTime EndTime { get; init; }
}

/// <summary>High-level profiler that uses <see cref="Stopwatch"/> for timing regions.</summary>
public sealed class PerformanceProfiler
{
    private readonly ConcurrentDictionary<string, RegionState> _regions = new();
    private readonly Stopwatch _sessionStopwatch = Stopwatch.StartNew();
    private readonly DateTime _startTime = DateTime.UtcNow;

    /// <summary>Represents the internal state of a profiling region.</summary>
    private sealed class RegionState
    {
        /// <summary>The accumulated elapsed time across all executions.</summary>
        public long TotalTicks;

        /// <summary>The number of times the region was entered.</summary>
        public int ExecutionCount;

        /// <summary>The minimum ticks observed in a single execution.</summary>
        public long MinTicks = long.MaxValue;

        /// <summary>The maximum ticks observed in a single execution.</summary>
        public long MaxTicks;

        /// <summary>Active stopwatch instances for currently-open region scopes.</summary>
        public ConcurrentBag<Stopwatch> ActiveTimers = new();
    }

    /// <summary>Gets the total number of regions being profiled.</summary>
    public int RegionCount => _regions.Count;

    /// <summary>Starts profiling a named region.</summary>
    /// <param name="name">The region name.</param>
    public void StartRegion(string name)
    {
        var sw = Stopwatch.StartNew();
        var state = _regions.GetOrAdd(name, _ => new RegionState());
        state.ActiveTimers.Add(sw);
    }

    /// <summary>Stops profiling a named region and records the elapsed time.</summary>
    /// <param name="name">The region name.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no active timer exists for the specified region.
    /// </exception>
    public void StopRegion(string name)
    {
        if (!_regions.TryGetValue(name, out var state))
        {
            throw new InvalidOperationException($"Region '{name}' was never started.");
        }

        if (state.ActiveTimers.IsEmpty)
        {
            throw new InvalidOperationException($"Region '{name}' has no active timers to stop.");
        }

        if (state.ActiveTimers.TryTake(out var sw))
        {
            sw.Stop();
            long elapsed = sw.ElapsedTicks;

            Interlocked.Add(ref state.TotalTicks, elapsed);
            Interlocked.Increment(ref state.ExecutionCount);

            long currentMin;
            do
            {
                currentMin = Interlocked.Read(ref state.MinTicks);
                if (elapsed >= currentMin)
                {
                    break;
                }
            }
            while (Interlocked.CompareExchange(ref state.MinTicks, elapsed, currentMin) != currentMin);

            long currentMax;
            do
            {
                currentMax = Interlocked.Read(ref state.MaxTicks);
                if (elapsed <= currentMax)
                {
                    break;
                }
            }
            while (Interlocked.CompareExchange(ref state.MaxTicks, elapsed, currentMax) != currentMax);
        }
    }

    /// <summary>Measures the execution time of an action within a named region.</summary>
    /// <param name="name">The region name.</param>
    /// <param name="action">The action to measure.</param>
    public void Measure(string name, Action action)
    {
        StartRegion(name);
        try
        {
            action();
        }
        finally
        {
            StopRegion(name);
        }
    }

    /// <summary>Measures the execution time of a function within a named region.</summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="name">The region name.</param>
    /// <param name="func">The function to measure.</param>
    /// <returns>The result of the function.</returns>
    public T Measure<T>(string name, Func<T> func)
    {
        StartRegion(name);
        try
        {
            return func();
        }
        finally
        {
            StopRegion(name);
        }
    }

    /// <summary>Generates a complete profiling report with statistics for all regions.</summary>
    /// <returns>A <see cref="ProfilerReport"/> containing all profiling data.</returns>
    public ProfilerReport GetReport()
    {
        _sessionStopwatch.Stop();
        var report = new ProfilerReport
        {
            TotalSessionElapsed = _sessionStopwatch.Elapsed,
            StartTime = _startTime,
            EndTime = DateTime.UtcNow
        };

        foreach (var kvp in _regions)
        {
            var state = kvp.Value;
            int count = Interlocked.CompareExchange(ref state.ExecutionCount, 0, 0);
            long total = Interlocked.CompareExchange(ref state.TotalTicks, 0, 0);
            long min = Interlocked.CompareExchange(ref state.MinTicks, 0, 0);
            long max = Interlocked.CompareExchange(ref state.MaxTicks, 0, 0);

            report.Regions.Add(new RegionReport
            {
                Name = kvp.Key,
                TotalElapsed = TimeSpan.FromTicks(total),
                ExecutionCount = count,
                AverageElapsed = count > 0 ? TimeSpan.FromTicks(total / count) : TimeSpan.Zero,
                MinElapsed = min < long.MaxValue ? TimeSpan.FromTicks(min) : TimeSpan.Zero,
                MaxElapsed = TimeSpan.FromTicks(max)
            });
        }

        return report;
    }

    /// <summary>Resets all profiling data.</summary>
    public void Reset()
    {
        _regions.Clear();
    }
}
