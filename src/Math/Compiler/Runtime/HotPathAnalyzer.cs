namespace MathVerse.Math.Compiler.Runtime;

using System;
using System.Collections.Generic;

/// <summary>
/// Analyzes profiling data from an <see cref="ExecutionProfiler"/> to identify hot paths
/// and categorize functions by their performance impact.
/// </summary>
public sealed class HotPathAnalyzer
{
    private const double HotThresholdPercentage = 0.10;
    private const int HotCallCountThreshold = 100;
    private const double WarmThresholdPercentage = 0.03;

    /// <summary>
    /// Analyzes profiling data and returns detailed hot path analysis for each profiled function.
    /// </summary>
    /// <param name="profiler">The execution profiler containing recorded data.</param>
    /// <returns>An ordered list of hot path analyses, sorted by impact descending.</returns>
    public IReadOnlyList<HotPathAnalysis> Analyze(ExecutionProfiler profiler)
    {
        ArgumentNullException.ThrowIfNull(profiler);

        var result = profiler.GetStatistics();
        var analyses = new List<HotPathAnalysis>(result.FunctionProfiles.Count);
        var totalTime = result.TotalExecutionTime.Ticks;

        foreach (var kvp in result.FunctionProfiles)
        {
            var profile = kvp.Value;
            var percentage = totalTime > 0
                ? (double)profile.TotalTime.Ticks / totalTime
                : 0.0;

            var isHot = percentage > HotThresholdPercentage || profile.CallCount > HotCallCountThreshold;

            analyses.Add(new HotPathAnalysis(
                profile.FunctionName,
                profile.CallCount,
                profile.TotalTime,
                percentage * 100.0,
                isHot));
        }

        analyses.Sort((a, b) => b.PercentageOfTotal.CompareTo(a.PercentageOfTotal));
        return analyses;
    }

    /// <summary>
    /// Determines whether a specific function is considered hot based on profiling data.
    /// </summary>
    /// <param name="profiler">The execution profiler.</param>
    /// <param name="functionName">The function to check.</param>
    /// <returns>True if the function is considered hot.</returns>
    public bool IsFunctionHot(ExecutionProfiler profiler, string functionName)
    {
        ArgumentNullException.ThrowIfNull(profiler);
        ArgumentNullException.ThrowIfNull(functionName);

        var result = profiler.GetStatistics();
        if (!result.FunctionProfiles.TryGetValue(functionName, out var profile))
            return false;

        var totalTime = result.TotalExecutionTime.Ticks;
        if (totalTime <= 0) return false;

        var percentage = (double)profile.TotalTime.Ticks / totalTime;
        return percentage > HotThresholdPercentage || profile.CallCount > HotCallCountThreshold;
    }

    /// <summary>
    /// Determines whether a specific function is considered warm (moderately used).
    /// </summary>
    /// <param name="profiler">The execution profiler.</param>
    /// <param name="functionName">The function to check.</param>
    /// <returns>True if the function is considered warm.</returns>
    public bool IsFunctionWarm(ExecutionProfiler profiler, string functionName)
    {
        ArgumentNullException.ThrowIfNull(profiler);
        ArgumentNullException.ThrowIfNull(functionName);

        var result = profiler.GetStatistics();
        if (!result.FunctionProfiles.TryGetValue(functionName, out var profile))
            return false;

        var totalTime = result.TotalExecutionTime.Ticks;
        if (totalTime <= 0) return false;

        var percentage = (double)profile.TotalTime.Ticks / totalTime;
        return percentage > WarmThresholdPercentage && percentage <= HotThresholdPercentage;
    }

    /// <summary>
    /// Ranks all profiled functions by their performance impact and returns them in descending order.
    /// </summary>
    /// <param name="profiler">The execution profiler.</param>
    /// <returns>An ordered list of function names ranked by total execution time.</returns>
    public IReadOnlyList<string> RankFunctionsByImpact(ExecutionProfiler profiler)
    {
        ArgumentNullException.ThrowIfNull(profiler);

        var result = profiler.GetStatistics();
        var ranked = new List<string>(result.FunctionProfiles.Count);

        foreach (var kvp in result.FunctionProfiles)
            ranked.Add(kvp.Key);

        ranked.Sort((a, b) =>
        {
            var ta = result.FunctionProfiles[a].TotalTime;
            var tb = result.FunctionProfiles[b].TotalTime;
            return tb.CompareTo(ta);
        });

        return ranked;
    }
}
