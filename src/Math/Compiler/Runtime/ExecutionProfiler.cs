namespace MathVerse.Math.Compiler.Runtime;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// Records execution profiling data in a thread-safe manner. Tracks function call counts,
/// total time per function, and detects hot paths.
/// </summary>
public sealed class ExecutionProfiler
{
    private readonly ConcurrentDictionary<string, FunctionProfiler> _profiles = new();
    private long _totalTicks;
    private int _totalCalls;
    private readonly object _hotPathLock = new();
    private readonly List<HotPath> _hotPaths = new();
    private bool _profilingEnabled = true;

    /// <summary>Whether profiling is currently enabled.</summary>
    public bool ProfilingEnabled
    {
        get => Volatile.Read(ref _profilingEnabled);
        set => Volatile.Write(ref _profilingEnabled, value);
    }

    /// <summary>
    /// Records a single execution of a function with its measured duration.
    /// </summary>
    /// <param name="functionName">The name of the function that was executed.</param>
    /// <param name="duration">How long the function took to execute.</param>
    public void RecordExecution(string functionName, TimeSpan duration)
    {
        if (!Volatile.Read(ref _profilingEnabled))
            return;

        var profiler = _profiles.GetOrAdd(functionName, static name => new FunctionProfiler(name));
        profiler.Record(duration);
        Interlocked.Add(ref _totalTicks, duration.Ticks);
        Interlocked.Increment(ref _totalCalls);
    }

    /// <summary>
    /// Records a single execution using a pre-started Stopwatch.
    /// </summary>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="stopwatch">The stopwatch that measured the execution.</param>
    public void RecordExecution(string functionName, Stopwatch stopwatch)
    {
        ArgumentNullException.ThrowIfNull(stopwatch);
        RecordExecution(functionName, stopwatch.Elapsed);
    }

    /// <summary>
    /// Begins timing a function and returns a disposable handle that records the elapsed time when disposed.
    /// </summary>
    /// <param name="functionName">The function name to profile.</param>
    /// <returns>A disposable profiling handle.</returns>
    public ProfilingHandle BeginProfile(string functionName)
    {
        return new ProfilingHandle(this, functionName);
    }

    /// <summary>
    /// Gets all detected hot paths.
    /// </summary>
    /// <returns>An immutable list of hot paths.</returns>
    public IReadOnlyList<HotPath> GetHotPaths()
    {
        lock (_hotPathLock)
        {
            return _hotPaths.ToArray();
        }
    }

    /// <summary>
    /// Builds and returns the aggregated profile result for all recorded executions.
    /// </summary>
    /// <returns>A complete ProfileResult with per-function data and hot paths.</returns>
    public ProfileResult GetStatistics()
    {
        var profiles = new Dictionary<string, FunctionProfile>(_profiles.Count);
        foreach (var kvp in _profiles)
        {
            var snapshot = kvp.Value.Snapshot();
            profiles[snapshot.FunctionName] = snapshot;
        }

        var totalTime = TimeSpan.FromTicks(Volatile.Read(ref _totalTicks));
        var totalCalls = Volatile.Read(ref _totalCalls);
        var hotPaths = DetectHotPaths(profiles, totalTime);

        return new ProfileResult(profiles, totalTime, totalCalls, hotPaths);
    }

    /// <summary>
    /// Resets all profiling data.
    /// </summary>
    public void Reset()
    {
        _profiles.Clear();
        Interlocked.Exchange(ref _totalTicks, 0);
        Interlocked.Exchange(ref _totalCalls, 0);
        lock (_hotPathLock)
        {
            _hotPaths.Clear();
        }
    }

    private static List<HotPath> DetectHotPaths(Dictionary<string, FunctionProfile> profiles, TimeSpan totalTime)
    {
        var hotPaths = new List<HotPath>();
        if (totalTime.Ticks == 0) return hotPaths;

        foreach (var kvp in profiles)
        {
            var profile = kvp.Value;
            var percentage = (double)profile.TotalTime.Ticks / totalTime.Ticks;

            if (percentage > 0.1 || profile.CallCount > 100)
            {
                hotPaths.Add(new HotPath(
                    new[] { profile.FunctionName },
                    profile.CallCount,
                    profile.TotalTime));
            }
        }

        hotPaths.Sort((a, b) => b.TotalDuration.CompareTo(a.TotalDuration));
        return hotPaths;
    }

    /// <summary>
    /// Disposable profiling handle that records execution time on disposal.
    /// </summary>
    public readonly struct ProfilingHandle : IDisposable
    {
        private readonly ExecutionProfiler _profiler;
        private readonly string _functionName;
        private readonly Stopwatch _stopwatch;

        internal ProfilingHandle(ExecutionProfiler profiler, string functionName)
        {
            _profiler = profiler;
            _functionName = functionName;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _stopwatch.Stop();
            _profiler.RecordExecution(_functionName, _stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Thread-safe per-function profiler that tracks call counts and timing statistics.
    /// </summary>
    private sealed class FunctionProfiler
    {
        private readonly string _functionName;
        private int _callCount;
        private long _totalTicks;
        private long _minTicks = long.MaxValue;
        private long _maxTicks;
        private readonly object _lock = new();

        public FunctionProfiler(string functionName)
        {
            _functionName = functionName;
        }

        public void Record(TimeSpan duration)
        {
            var ticks = duration.Ticks;
            lock (_lock)
            {
                Interlocked.Increment(ref _callCount);
                Interlocked.Add(ref _totalTicks, ticks);

                if (ticks < _minTicks)
                    _minTicks = ticks;
                if (ticks > _maxTicks)
                    _maxTicks = ticks;
            }
        }

        public FunctionProfile Snapshot()
        {
            lock (_lock)
            {
                var count = Volatile.Read(ref _callCount);
                var total = Volatile.Read(ref _totalTicks);
                var min = Volatile.Read(ref _minTicks);
                var max = Volatile.Read(ref _maxTicks);

                return new FunctionProfile(
                    _functionName,
                    count,
                    TimeSpan.FromTicks(total),
                    min == long.MaxValue ? TimeSpan.Zero : TimeSpan.FromTicks(min),
                    TimeSpan.FromTicks(max));
            }
        }
    }
}
