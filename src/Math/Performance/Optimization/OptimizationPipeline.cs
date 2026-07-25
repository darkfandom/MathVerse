namespace MathVerse.Math.Performance.Optimization;

/// <summary>
/// Manages and executes an ordered pipeline of optimization passes.
/// </summary>
public sealed class OptimizationPipeline
{
    private readonly List<OptimizationPass> _passes = [];
    private readonly object _lock = new();

    /// <summary>
    /// Gets the list of registered optimization passes.
    /// </summary>
    public IReadOnlyList<OptimizationPass> Passes
    {
        get
        {
            lock (_lock)
            {
                return _passes.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the cumulative statistics from the last optimization run.
    /// </summary>
    public OptimizationStatistics Statistics { get; } = new();

    /// <summary>
    /// Adds an optimization pass to the pipeline.
    /// </summary>
    /// <param name="pass">The pass to add.</param>
    public void AddPass(OptimizationPass pass)
    {
        if (pass is null)
            throw new ArgumentNullException(nameof(pass));

        lock (_lock)
        {
            _passes.Add(pass);
            _passes.Sort((a, b) =>
            {
                var stageCompare = a.Stage.CompareTo(b.Stage);
                return stageCompare != 0 ? stageCompare : a.Order.CompareTo(b.Order);
            });
        }
    }

    /// <summary>
    /// Removes all registered passes and resets statistics.
    /// </summary>
    public void ClearPasses()
    {
        lock (_lock)
        {
            _passes.Clear();
        }
    }

    /// <summary>
    /// Runs all registered passes on the input expression.
    /// </summary>
    /// <param name="input">The expression to optimize.</param>
    /// <returns>The optimized expression.</returns>
    public Expression Optimize(Expression input)
    {
        OptimizationPass[] snapshot;
        lock (_lock)
        {
            snapshot = [.. _passes];
        }

        var current = input;

        for (var i = 0; i < snapshot.Length; i++)
        {
            var pass = snapshot[i];
            var context = new OptimizationContext(current, pass.Stage, i);
            var sw = Stopwatch.StartNew();

            var output = pass.Optimize(current, context);
            sw.Stop();

            var nodesRemoved = current.NodeCount - output.NodeCount;
            if (nodesRemoved < 0)
                nodesRemoved = 0;

            var result = new OptimizationResult(
                output,
                pass.Stage,
                sw.Elapsed,
                nodesRemoved,
                context.HasChanges ? 1 : 0,
                context.HasChanges);

            Statistics.Record(result);
            current = output;
        }

        return current;
    }

    /// <summary>
    /// Runs only the passes matching the specified stages on the input expression.
    /// </summary>
    /// <param name="input">The expression to optimize.</param>
    /// <param name="stages">A bitmask of stages to execute.</param>
    /// <returns>The optimized expression.</returns>
    public Expression Optimize(Expression input, OptimizationStage stages)
    {
        OptimizationPass[] snapshot;
        lock (_lock)
        {
            snapshot = [.. _passes];
        }

        var current = input;
        var passNumber = 0;

        for (var i = 0; i < snapshot.Length; i++)
        {
            var pass = snapshot[i];

            if ((pass.Stage & stages) == 0)
                continue;

            var context = new OptimizationContext(current, pass.Stage, passNumber);
            var sw = Stopwatch.StartNew();

            var output = pass.Optimize(current, context);
            sw.Stop();

            var nodesRemoved = current.NodeCount - output.NodeCount;
            if (nodesRemoved < 0)
                nodesRemoved = 0;

            var result = new OptimizationResult(
                output,
                pass.Stage,
                sw.Elapsed,
                nodesRemoved,
                context.HasChanges ? 1 : 0,
                context.HasChanges);

            Statistics.Record(result);
            current = output;
            passNumber++;
        }

        return current;
    }
}
