namespace MathVerse.Math.Compiler.Runtime;

/// <summary>
/// Defines optimization strategies that can be applied to functions based on runtime profiling data.
/// </summary>
public enum OptimizationStrategy
{
    /// <summary>No optimization applied.</summary>
    None,

    /// <summary>Skip optimization entirely for rarely-called functions.</summary>
    SkipOptimization,

    /// <summary>Apply basic peephole optimizations only.</summary>
    BasicOptimization,

    /// <summary>Apply standard optimizations including constant folding and dead code elimination.</summary>
    StandardOptimization,

    /// <summary>Apply aggressive optimizations including inlining and loop transformations.</summary>
    AggressiveOptimization,

    /// <summary>Apply all available optimizations including speculative devirtualization.</summary>
    FullOptimization
}
