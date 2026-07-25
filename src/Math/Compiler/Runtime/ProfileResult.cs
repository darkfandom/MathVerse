namespace MathVerse.Math.Compiler.Runtime;

using System;
using System.Collections.Generic;

/// <summary>
/// Aggregated profiling result containing per-function profiles and detected hot paths.
/// </summary>
public sealed class ProfileResult
{
    /// <summary>Per-function profiling data keyed by function name.</summary>
    public IReadOnlyDictionary<string, FunctionProfile> FunctionProfiles { get; }

    /// <summary>The total wall-clock time of the profiled session.</summary>
    public TimeSpan TotalExecutionTime { get; }

    /// <summary>The total number of function calls observed.</summary>
    public int TotalCallCount { get; }

    /// <summary>The detected hot paths across all profiled functions.</summary>
    public IReadOnlyList<HotPath> HotPaths { get; }

    /// <summary>
    /// Initializes a new profile result.
    /// </summary>
    public ProfileResult(
        IReadOnlyDictionary<string, FunctionProfile> functionProfiles,
        TimeSpan totalExecutionTime,
        int totalCallCount,
        IReadOnlyList<HotPath> hotPaths)
    {
        FunctionProfiles = functionProfiles ?? new Dictionary<string, FunctionProfile>();
        TotalExecutionTime = totalExecutionTime;
        TotalCallCount = totalCallCount;
        HotPaths = hotPaths ?? Array.Empty<HotPath>();
    }
}

/// <summary>
/// Profiling data for a single function.
/// </summary>
public sealed class FunctionProfile
{
    /// <summary>The function name.</summary>
    public string FunctionName { get; }

    /// <summary>How many times the function was called.</summary>
    public int CallCount { get; }

    /// <summary>The total time spent in this function.</summary>
    public TimeSpan TotalTime { get; }

    /// <summary>The minimum single-invocation duration observed.</summary>
    public TimeSpan MinTime { get; }

    /// <summary>The maximum single-invocation duration observed.</summary>
    public TimeSpan MaxTime { get; }

    /// <summary>
    /// Initializes a new function profile.
    /// </summary>
    public FunctionProfile(string functionName, int callCount, TimeSpan totalTime, TimeSpan minTime, TimeSpan maxTime)
    {
        FunctionName = functionName;
        CallCount = callCount;
        TotalTime = totalTime;
        MinTime = minTime;
        MaxTime = maxTime;
    }
}
