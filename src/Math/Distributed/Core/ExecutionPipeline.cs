namespace MathVerse.Math.Distributed.Core;

/// <summary>Represents a chain of processing stages executed sequentially.</summary>
public sealed class ExecutionPipeline
{
    private readonly List<Func<double[], CancellationToken, ValueTask<double[]>>> _stages;

    /// <summary>Number of stages in the pipeline.</summary>
    public int StageCount => _stages.Count;

    /// <summary>Initializes an empty execution pipeline.</summary>
    public ExecutionPipeline()
    {
        _stages = new List<Func<double[], CancellationToken, ValueTask<double[]>>>();
    }

    /// <summary>Adds a processing stage to the pipeline.</summary>
    /// <param name="stage">The stage function that transforms input to output.</param>
    /// <returns>This pipeline instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when stage is null.</exception>
    public ExecutionPipeline AddStage(Func<double[], CancellationToken, ValueTask<double[]>> stage)
    {
        if (stage == null)
        {
            throw new ArgumentNullException(nameof(stage));
        }
        _stages.Add(stage);
        return this;
    }

    /// <summary>Adds a synchronous processing stage to the pipeline.</summary>
    /// <param name="stage">The stage function that transforms input to output.</param>
    /// <returns>This pipeline instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when stage is null.</exception>
    public ExecutionPipeline AddStage(Func<double[], double[]> stage)
    {
        if (stage == null)
        {
            throw new ArgumentNullException(nameof(stage));
        }
        _stages.Add((input, ct) => ValueTask.FromResult(stage(input)));
        return this;
    }

    /// <summary>Executes all stages in the pipeline sequentially.</summary>
    /// <param name="input">The initial input values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final output after all stages have been applied.</returns>
    public async ValueTask<double[]> Execute(double[] input, CancellationToken ct = default)
    {
        var current = input;

        foreach (var stage in _stages)
        {
            ct.ThrowIfCancellationRequested();
            current = await stage(current, ct).ConfigureAwait(false);
        }

        return current;
    }

    /// <summary>Clears all stages from the pipeline.</summary>
    public void Clear()
    {
        _stages.Clear();
    }
}
