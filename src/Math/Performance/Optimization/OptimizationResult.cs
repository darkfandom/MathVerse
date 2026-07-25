namespace MathVerse.Math.Performance.Optimization;

/// <summary>
/// Represents the result of a single optimization pass.
/// </summary>
public sealed class OptimizationResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="OptimizationResult"/>.
    /// </summary>
    /// <param name="output">The resulting expression after optimization.</param>
    /// <param name="stage">The stage that produced this result.</param>
    /// <param name="duration">How long the pass took.</param>
    /// <param name="nodesRemoved">The number of nodes removed by the pass.</param>
    /// <param name="nodesSimplified">The number of nodes simplified by the pass.</param>
    /// <param name="hasChanges">Whether the pass made any changes.</param>
    public OptimizationResult(
        Expression output,
        OptimizationStage stage,
        TimeSpan duration,
        int nodesRemoved,
        int nodesSimplified,
        bool hasChanges)
    {
        Output = output;
        Stage = stage;
        Duration = duration;
        NodesRemoved = nodesRemoved;
        NodesSimplified = nodesSimplified;
        HasChanges = hasChanges;
    }

    /// <summary>
    /// Gets the output expression after optimization.
    /// </summary>
    public Expression Output { get; }

    /// <summary>
    /// Gets the optimization stage that produced this result.
    /// </summary>
    public OptimizationStage Stage { get; }

    /// <summary>
    /// Gets the duration of the optimization pass.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets the number of expression nodes removed by the pass.
    /// </summary>
    public int NodesRemoved { get; }

    /// <summary>
    /// Gets the number of expression nodes simplified (e.g., folded) by the pass.
    /// </summary>
    public int NodesSimplified { get; }

    /// <summary>
    /// Gets whether the pass changed the expression.
    /// </summary>
    public bool HasChanges { get; }

    /// <summary>
    /// Creates an <see cref="OptimizationResult"/> indicating no changes were made.
    /// </summary>
    /// <param name="input">The unchanged input expression.</param>
    /// <param name="stage">The stage that was executed.</param>
    /// <returns>An optimization result with zero changes.</returns>
    public static OptimizationResult Unchanged(Expression input, OptimizationStage stage) =>
        new(input, stage, TimeSpan.Zero, 0, 0, false);
}
