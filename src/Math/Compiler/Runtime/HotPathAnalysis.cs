namespace MathVerse.Math.Compiler.Runtime;

using System;

/// <summary>
/// Represents the analysis result for a single function's execution profile, including whether it qualifies as hot.
/// </summary>
public sealed class HotPathAnalysis
{
    /// <summary>The name of the analyzed function.</summary>
    public string FunctionName { get; }

    /// <summary>How many times the function was called.</summary>
    public int CallCount { get; }

    /// <summary>The total time spent executing this function.</summary>
    public TimeSpan TotalTime { get; }

    /// <summary>The average time per invocation.</summary>
    public TimeSpan AverageTime { get; }

    /// <summary>The percentage of total profiled time this function consumed.</summary>
    public double PercentageOfTotal { get; }

    /// <summary>Whether this function is considered a hot path.</summary>
    public bool IsHot { get; }

    /// <summary>
    /// Initializes a new hot path analysis record.
    /// </summary>
    public HotPathAnalysis(string functionName, int callCount, TimeSpan totalTime, double percentageOfTotal, bool isHot)
    {
        FunctionName = functionName;
        CallCount = callCount;
        TotalTime = totalTime;
        AverageTime = callCount > 0 ? TimeSpan.FromTicks(totalTime.Ticks / callCount) : TimeSpan.Zero;
        PercentageOfTotal = percentageOfTotal;
        IsHot = isHot;
    }
}
