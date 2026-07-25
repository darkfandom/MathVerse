namespace MathVerse.Math.Compiler.Runtime;

/// <summary>
/// Represents a recommendation for optimizing a specific function based on profiling and IR analysis.
/// </summary>
public sealed class OptimizationAdvice
{
    /// <summary>The name of the function to optimize.</summary>
    public string FunctionName { get; }

    /// <summary>The recommended optimization strategy.</summary>
    public OptimizationStrategy Strategy { get; }

    /// <summary>A human-readable description of the advice.</summary>
    public string Description { get; }

    /// <summary>The category of optimization (e.g., "memory", "compute", "control-flow").</summary>
    public string Category { get; }

    /// <summary>The priority of this advice (higher = more important).</summary>
    public int Priority { get; }

    /// <summary>
    /// Initializes a new optimization advice record.
    /// </summary>
    public OptimizationAdvice(string functionName, OptimizationStrategy strategy, string description, string category, int priority)
    {
        FunctionName = functionName;
        Strategy = strategy;
        Description = description;
        Category = category;
        Priority = priority;
    }
}
