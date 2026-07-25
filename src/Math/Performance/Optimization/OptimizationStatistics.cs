namespace MathVerse.Math.Performance.Optimization;

/// <summary>
/// Aggregates statistics across multiple optimization passes.
/// </summary>
public sealed class OptimizationStatistics
{
    private readonly List<OptimizationStage> _stagesExecuted = [];

    /// <summary>
    /// Gets the total number of passes executed.
    /// </summary>
    public int TotalPasses { get; private set; }

    /// <summary>
    /// Gets the total number of nodes removed across all passes.
    /// </summary>
    public int TotalNodesRemoved { get; private set; }

    /// <summary>
    /// Gets the total number of nodes simplified across all passes.
    /// </summary>
    public int TotalNodesSimplified { get; private set; }

    /// <summary>
    /// Gets the total duration across all passes.
    /// </summary>
    public TimeSpan TotalDuration { get; private set; }

    /// <summary>
    /// Gets the list of stages that were executed.
    /// </summary>
    public IReadOnlyList<OptimizationStage> StagesExecuted => _stagesExecuted;

    /// <summary>
    /// Records the result of a single optimization pass.
    /// </summary>
    /// <param name="result">The result to record.</param>
    public void Record(OptimizationResult result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        TotalPasses++;
        TotalNodesRemoved += result.NodesRemoved;
        TotalNodesSimplified += result.NodesSimplified;
        TotalDuration += result.Duration;

        if (!_stagesExecuted.Contains(result.Stage))
        {
            _stagesExecuted.Add(result.Stage);
        }
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Passes={TotalPasses}, NodesRemoved={TotalNodesRemoved}, NodesSimplified={TotalNodesSimplified}, " +
        $"Duration={TotalDuration.TotalMilliseconds:F2}ms, Stages=[{string.Join(", ", _stagesExecuted)}]";
}
