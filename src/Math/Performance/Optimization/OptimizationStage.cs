namespace MathVerse.Math.Performance.Optimization;

/// <summary>
/// Enumerates the stages of the expression optimization pipeline.
/// </summary>
[Flags]
public enum OptimizationStage
{
    /// <summary>No optimization stage.</summary>
    None = 0,

    /// <summary>Normalize expression structure.</summary>
    Canonicalization = 1,

    /// <summary>Fold constant subexpressions.</summary>
    ConstantFolding = 2,

    /// <summary>Eliminate duplicate subexpressions.</summary>
    CommonSubexpressionElimination = 4,

    /// <summary>Remove unused subexpressions.</summary>
    DeadExpressionElimination = 8,

    /// <summary>Apply algebraic identities.</summary>
    AlgebraicOptimization = 16,

    /// <summary>Optimize cache layout of the expression tree.</summary>
    CacheOptimization = 32,

    /// <summary>All optimization stages.</summary>
    All = Canonicalization | ConstantFolding | CommonSubexpressionElimination | DeadExpressionElimination | AlgebraicOptimization | CacheOptimization
}
