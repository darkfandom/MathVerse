namespace MathVerse.Math.Interop.ScientificWorkflow;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents an individual step within a scientific workflow.
/// </summary>
public sealed class WorkflowStep
{
    private readonly List<string> _dependencies = new();

    /// <summary>
    /// Gets or sets the unique step identifier.
    /// </summary>
    public string StepId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the step.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the step (e.g., "compute", "transform", "output").
    /// </summary>
    public string StepType { get; set; } = string.Empty;

    /// <summary>
    /// Gets the parameters dictionary for the step.
    /// </summary>
    public Dictionary<string, object> Parameters { get; } = new();

    /// <summary>
    /// Gets the list of step IDs that this step depends on.
    /// </summary>
    public List<string> Dependencies => _dependencies;

    /// <summary>
    /// Adds a dependency on the specified step.
    /// </summary>
    /// <param name="stepId">The step ID to depend on.</param>
    public void AddDependency(string stepId)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepId);
        if (!_dependencies.Contains(stepId))
        {
            _dependencies.Add(stepId);
        }
    }

    /// <summary>
    /// Removes a dependency on the specified step.
    /// </summary>
    /// <param name="stepId">The step ID to remove from dependencies.</param>
    public void RemoveDependency(string stepId)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepId);
        _dependencies.Remove(stepId);
    }

    /// <summary>
    /// Determines whether this step depends on the specified step.
    /// </summary>
    /// <param name="stepId">The step ID to check.</param>
    /// <returns>True if this step depends on the specified step.</returns>
    public bool DependsOn(string stepId)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepId);
        return _dependencies.Contains(stepId);
    }
}
