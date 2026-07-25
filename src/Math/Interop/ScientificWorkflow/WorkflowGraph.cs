namespace MathVerse.Math.Interop.ScientificWorkflow;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides DAG (Directed Acyclic Graph) representation and analysis of a workflow
/// using Kahn's algorithm for topological sorting.
/// </summary>
public sealed class WorkflowGraph
{
    private readonly Workflow _workflow;
    private readonly Dictionary<string, int> _stepIndexMap = new();
    private bool _hasCycles = true;
    private List<IReadOnlyList<string>>? _executionOrder;

    /// <summary>
    /// Gets a value indicating whether the workflow graph contains cycles.
    /// </summary>
    public bool HasCycles => _hasCycles;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowGraph"/> class.
    /// </summary>
    /// <param name="workflow">The workflow to build a graph from.</param>
    public WorkflowGraph(Workflow workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        _workflow = workflow;

        for (var i = 0; i < workflow.Steps.Count; i++)
        {
            _stepIndexMap[workflow.Steps[i].StepId] = i;
        }
    }

    /// <summary>
    /// Validates the workflow graph by checking for cycles and missing dependencies.
    /// </summary>
    /// <returns>True if the graph is valid (acyclic with all dependencies satisfied).</returns>
    public bool Validate()
    {
        _executionOrder = null;
        _hasCycles = false;

        var stepIds = new HashSet<string>();
        foreach (var step in _workflow.Steps)
        {
            stepIds.Add(step.StepId);
        }

        foreach (var step in _workflow.Steps)
        {
            foreach (var dep in step.Dependencies)
            {
                if (!stepIds.Contains(dep))
                {
                    return false;
                }
            }
        }

        var order = TopologicalSort();
        if (order == null)
        {
            _hasCycles = true;
            return false;
        }

        _executionOrder = order;
        return true;
    }

    /// <summary>
    /// Gets the execution order as a list of levels (topological sort).
    /// Each level contains step IDs that can be executed in parallel.
    /// </summary>
    /// <returns>The ordered levels, or null if the graph has cycles.</returns>
    public IReadOnlyList<IReadOnlyList<string>> GetExecutionOrder()
    {
        if (_executionOrder == null)
        {
            Validate();
        }
        return _executionOrder ?? (IReadOnlyList<IReadOnlyList<string>>)Array.Empty<IReadOnlyList<string>>();
    }

    /// <summary>
    /// Gets the depth of each step in the DAG.
    /// Depth is the length of the longest path from any root to the step.
    /// </summary>
    /// <returns>An array where index corresponds to step position in the workflow.</returns>
    public int[] GetStepDepths()
    {
        var depths = new int[_workflow.Steps.Count];
        var stepDepths = new Dictionary<string, int>();

        var order = GetExecutionOrder();
        for (var level = 0; level < order.Count; level++)
        {
            foreach (var stepId in order[level])
            {
                stepDepths[stepId] = level;
            }
        }

        for (var i = 0; i < _workflow.Steps.Count; i++)
        {
            if (stepDepths.TryGetValue(_workflow.Steps[i].StepId, out var depth))
            {
                depths[i] = depth;
            }
        }

        return depths;
    }

    private List<IReadOnlyList<string>>? TopologicalSort()
    {
        var stepIds = new HashSet<string>();
        foreach (var step in _workflow.Steps)
        {
            stepIds.Add(step.StepId);
        }

        var inDegree = new Dictionary<string, int>();
        var dependents = new Dictionary<string, List<string>>();

        foreach (var stepId in stepIds)
        {
            inDegree[stepId] = 0;
            dependents[stepId] = new List<string>();
        }

        foreach (var step in _workflow.Steps)
        {
            foreach (var dep in step.Dependencies)
            {
                if (stepIds.Contains(dep))
                {
                    inDegree[step.StepId]++;
                    dependents[dep].Add(step.StepId);
                }
            }
        }

        var levels = new List<IReadOnlyList<string>>();
        var queue = new Queue<string>();

        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
            {
                queue.Enqueue(kvp.Key);
            }
        }

        var processedCount = 0;

        while (queue.Count > 0)
        {
            var level = new List<string>();
            var levelSize = queue.Count;

            for (var i = 0; i < levelSize; i++)
            {
                var current = queue.Dequeue();
                level.Add(current);
                processedCount++;

                foreach (var dependent in dependents[current])
                {
                    inDegree[dependent]--;
                    if (inDegree[dependent] == 0)
                    {
                        queue.Enqueue(dependent);
                    }
                }
            }

            levels.Add(level);
        }

        if (processedCount != _workflow.Steps.Count)
        {
            return null;
        }

        return levels;
    }
}
