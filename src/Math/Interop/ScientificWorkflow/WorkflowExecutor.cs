namespace MathVerse.Math.Interop.ScientificWorkflow;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents the result of a workflow execution.
/// </summary>
public sealed class WorkflowExecutionResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the workflow completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of steps that completed successfully.
    /// </summary>
    public int StepsCompleted { get; set; }

    /// <summary>
    /// Gets or sets the number of steps that failed.
    /// </summary>
    public int StepsFailed { get; set; }

    /// <summary>
    /// Gets or sets the error message if the workflow failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the dictionary of final results from the last completed step.
    /// </summary>
    public Dictionary<string, object> FinalResults { get; } = new();

    /// <summary>
    /// Gets or sets the total execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Executes workflows step by step, supporting both synchronous and asynchronous execution.
/// </summary>
public sealed class WorkflowExecutor
{
    /// <summary>
    /// Executes a workflow synchronously using the provided step executor function.
    /// </summary>
    /// <param name="workflow">The workflow to execute.</param>
    /// <param name="stepExecutor">
    /// A function that executes a single step and returns its output state.
    /// </param>
    /// <returns>The execution result.</returns>
    public WorkflowExecutionResult Execute(
        Workflow workflow,
        Func<WorkflowStep, Dictionary<string, object>, Dictionary<string, object>> stepExecutor)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(stepExecutor);

        var sw = Stopwatch.StartNew();
        var result = new WorkflowExecutionResult();
        var state = new Dictionary<string, object>();
        var graph = new WorkflowGraph(workflow);

        if (!graph.Validate())
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            result.ErrorMessage = "Workflow contains cycles or missing dependencies.";
            return result;
        }

        var executionOrder = graph.GetExecutionOrder();
        var completedSteps = new HashSet<string>();

        try
        {
            foreach (var level in executionOrder)
            {
                foreach (var stepId in level)
                {
                    var step = workflow.GetStep(stepId);
                    if (step == null)
                    {
                        result.StepsFailed++;
                        result.ErrorMessage = $"Step '{stepId}' not found.";
                        sw.Stop();
                        result.Duration = sw.Elapsed;
                        return result;
                    }

                    try
                    {
                        state = stepExecutor(step, state);
                        completedSteps.Add(stepId);
                        result.StepsCompleted++;
                    }
                    catch (Exception ex)
                    {
                        result.StepsFailed++;
                        result.ErrorMessage = $"Step '{stepId}' failed: {ex.Message}";
                        sw.Stop();
                        result.Duration = sw.Elapsed;
                        return result;
                    }
                }
            }
        }
        finally
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
        }

        foreach (var kvp in state)
        {
            result.FinalResults[kvp.Key] = kvp.Value;
        }

        result.Success = result.StepsFailed == 0;
        return result;
    }

    /// <summary>
    /// Executes a workflow asynchronously using the provided asynchronous step executor function.
    /// </summary>
    /// <param name="workflow">The workflow to execute.</param>
    /// <param name="stepExecutor">
    /// An asynchronous function that executes a single step and returns its output state.
    /// </param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task containing the execution result.</returns>
    public async ValueTask<WorkflowExecutionResult> ExecuteAsync(
        Workflow workflow,
        Func<WorkflowStep, Dictionary<string, object>, CancellationToken, ValueTask<Dictionary<string, object>>> stepExecutor,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(stepExecutor);

        var sw = Stopwatch.StartNew();
        var result = new WorkflowExecutionResult();
        var state = new Dictionary<string, object>();
        var graph = new WorkflowGraph(workflow);

        if (!graph.Validate())
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            result.ErrorMessage = "Workflow contains cycles or missing dependencies.";
            return result;
        }

        var executionOrder = graph.GetExecutionOrder();

        try
        {
            foreach (var level in executionOrder)
            {
                foreach (var stepId in level)
                {
                    ct.ThrowIfCancellationRequested();

                    var step = workflow.GetStep(stepId);
                    if (step == null)
                    {
                        result.StepsFailed++;
                        result.ErrorMessage = $"Step '{stepId}' not found.";
                        sw.Stop();
                        result.Duration = sw.Elapsed;
                        return result;
                    }

                    try
                    {
                        state = await stepExecutor(step, state, ct).ConfigureAwait(false);
                        result.StepsCompleted++;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        result.StepsFailed++;
                        result.ErrorMessage = $"Step '{stepId}' failed: {ex.Message}";
                        sw.Stop();
                        result.Duration = sw.Elapsed;
                        return result;
                    }
                }
            }
        }
        finally
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
        }

        foreach (var kvp in state)
        {
            result.FinalResults[kvp.Key] = kvp.Value;
        }

        result.Success = result.StepsFailed == 0;
        return result;
    }
}
