namespace MathVerse.Math.Performance.Optimization;

/// <summary>
/// Mutable context passed to optimization passes during a transformation.
/// </summary>
public sealed class OptimizationContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="OptimizationContext"/>.
    /// </summary>
    /// <param name="input">The input expression being optimized.</param>
    /// <param name="stage">The current optimization stage.</param>
    /// <param name="passNumber">The current pass number within the stage.</param>
    public OptimizationContext(Expression input, OptimizationStage stage, int passNumber)
    {
        Input = input;
        Stage = stage;
        PassNumber = passNumber;
    }

    /// <summary>
    /// Gets the input expression being optimized.
    /// </summary>
    public Expression Input { get; }

    /// <summary>
    /// Gets the current optimization stage.
    /// </summary>
    public OptimizationStage Stage { get; }

    /// <summary>
    /// Gets the current pass number within the stage.
    /// </summary>
    public int PassNumber { get; }

    /// <summary>
    /// Gets whether the pass has made any changes to the expression.
    /// </summary>
    public bool HasChanges { get; private set; }

    /// <summary>
    /// Marks the context as having changes.
    /// </summary>
    public void MarkChanged()
    {
        HasChanges = true;
    }

    /// <summary>
    /// Records a replacement of <paramref name="old"/> with <paramref name="new"/> and marks the context as changed.
    /// </summary>
    /// <param name="old">The original expression node.</param>
    /// <param name="new">The replacement expression node.</param>
    /// <returns>The replacement expression.</returns>
    public Expression Replace(Expression old, Expression @new)
    {
        if (!ReferenceEquals(old, @new))
        {
            MarkChanged();
        }

        return @new;
    }
}
