namespace MathVerse.Math.Compiler.Runtime;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a hot execution path — a chain of frequently called or time-consuming functions.
/// </summary>
public sealed class HotPath
{
    /// <summary>The ordered chain of function names forming the hot path.</summary>
    public IReadOnlyList<string> FunctionChain { get; }

    /// <summary>The number of times this path was executed.</summary>
    public int CallCount { get; }

    /// <summary>The total time spent on this path.</summary>
    public TimeSpan TotalDuration { get; }

    /// <summary>The average duration per invocation.</summary>
    public TimeSpan AverageDuration => CallCount > 0
        ? TimeSpan.FromTicks(TotalDuration.Ticks / CallCount)
        : TimeSpan.Zero;

    /// <summary>
    /// Initializes a new hot path record.
    /// </summary>
    /// <param name="functionChain">The chain of functions in the path.</param>
    /// <param name="callCount">Total invocation count.</param>
    /// <param name="totalDuration">Total execution time.</param>
    public HotPath(IReadOnlyList<string> functionChain, int callCount, TimeSpan totalDuration)
    {
        FunctionChain = functionChain ?? Array.Empty<string>();
        CallCount = callCount;
        TotalDuration = totalDuration;
    }
}
