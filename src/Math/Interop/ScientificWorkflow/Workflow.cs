namespace MathVerse.Math.Interop.ScientificWorkflow;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a complete scientific workflow definition containing steps and variables.
/// </summary>
public sealed class Workflow
{
    private readonly List<WorkflowStep> _steps = new();

    /// <summary>
    /// Gets or sets the unique workflow identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the workflow.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the ordered list of workflow steps.
    /// </summary>
    public List<WorkflowStep> Steps => _steps;

    /// <summary>
    /// Gets the variables dictionary shared across steps.
    /// </summary>
    public Dictionary<string, string> Variables { get; } = new();

    /// <summary>
    /// Adds a step to the workflow.
    /// </summary>
    /// <param name="step">The step to add.</param>
    public void AddStep(WorkflowStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _steps.Add(step);
    }

    /// <summary>
    /// Removes a step by its identifier.
    /// </summary>
    /// <param name="stepId">The step ID to remove.</param>
    /// <returns>True if the step was found and removed.</returns>
    public bool RemoveStep(string stepId)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepId);
        for (var i = _steps.Count - 1; i >= 0; i--)
        {
            if (string.Equals(_steps[i].StepId, stepId, StringComparison.Ordinal))
            {
                _steps.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets a step by its identifier.
    /// </summary>
    /// <param name="stepId">The step ID to find.</param>
    /// <returns>The matching step, or null if not found.</returns>
    public WorkflowStep? GetStep(string stepId)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepId);
        for (var i = 0; i < _steps.Count; i++)
        {
            if (string.Equals(_steps[i].StepId, stepId, StringComparison.Ordinal))
            {
                return _steps[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the total number of steps in the workflow.
    /// </summary>
    public int StepCount => _steps.Count;
}
