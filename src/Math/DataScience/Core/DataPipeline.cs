namespace MathVerse.Math.DataScience.Core;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a pipeline of data transformations that can be applied to a dataset.
/// </summary>
public sealed class DataPipeline
{
    private readonly List<(string Name, Func<Dataset, Dataset> Step)> _steps = new();
    private readonly List<(string StepName, DateTimeOffset ExecutedAt, int RowsBefore, int RowsAfter)> _executionHistory = new();

    /// <summary>
    /// Gets the number of steps in the pipeline.
    /// </summary>
    public int StepCount => _steps.Count;

    /// <summary>
    /// Gets the execution history of this pipeline.
    /// </summary>
    public IReadOnlyList<(string StepName, DateTimeOffset ExecutedAt, int RowsBefore, int RowsAfter)> ExecutionHistory => _executionHistory;

    /// <summary>
    /// Adds a transformation step to the pipeline.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="step">The transformation function.</param>
    /// <returns>The pipeline instance for method chaining.</returns>
    public DataPipeline AddStep(string name, Func<Dataset, Dataset> step)
    {
        _steps.Add((name, step));
        return this;
    }

    /// <summary>
    /// Executes all steps in the pipeline on the input dataset.
    /// </summary>
    /// <param name="input">The input dataset to transform.</param>
    /// <returns>The transformed dataset after all steps are applied.</returns>
    public Dataset Execute(Dataset input)
    {
        var current = input;
        foreach (var (name, step) in _steps)
        {
            int rowsBefore = current.Count;
            current = step(current);
            int rowsAfter = current.Count;
            _executionHistory.Add((name, DateTimeOffset.UtcNow, rowsBefore, rowsAfter));
        }
        return current;
    }
}