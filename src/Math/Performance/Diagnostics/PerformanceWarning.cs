namespace MathVerse.Math.Performance.Diagnostics;

/// <summary>
/// Categorizes performance warnings that can be issued by the diagnostics subsystem.
/// </summary>
[Flags]
public enum PerformanceWarning
{
    /// <summary>No warnings.</summary>
    None = 0,

    /// <summary>An operation took longer than the configured threshold.</summary>
    SlowEvaluation = 1,

    /// <summary>A cache lookup failed to find a previously stored entry.</summary>
    CacheMiss = 2,

    /// <summary>An allocation exceeded the configured size threshold.</summary>
    LargeAllocation = 4,

    /// <summary>Recursion depth exceeded the configured threshold.</summary>
    ExcessiveRecursion = 8,

    /// <summary>Expression tree depth exceeded the configured threshold.</summary>
    DeepExpressionTree = 16,

    /// <summary>Identical expressions were interned more than once.</summary>
    DuplicateExpressions = 32,

    /// <summary>System memory pressure exceeded the warning threshold.</summary>
    MemoryPressure = 64,

    /// <summary>Parallel execution encountered thread contention.</summary>
    ThreadContention = 128
}
