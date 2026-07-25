namespace MathVerse.Math.Performance.Optimization;

/// <summary>
/// Abstract base class for a single optimization pass in the pipeline.
/// </summary>
public abstract class OptimizationPass
{
    /// <summary>
    /// Gets the human-readable name of this optimization pass.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the optimization stage this pass belongs to.
    /// </summary>
    public abstract OptimizationStage Stage { get; }

    /// <summary>
    /// Gets the execution order of this pass within its stage (lower runs first).
    /// </summary>
    public abstract int Order { get; }

    /// <summary>
    /// Applies the optimization to the given expression.
    /// </summary>
    /// <param name="input">The expression to optimize.</param>
    /// <param name="context">The context for this optimization pass.</param>
    /// <returns>The optimized expression.</returns>
    public abstract Expression Optimize(Expression input, OptimizationContext context);
}
