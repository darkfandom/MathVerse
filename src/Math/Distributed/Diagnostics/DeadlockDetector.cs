namespace MathVerse.Math.Distributed.Diagnostics;

using MathVerse.Math.Distributed.Core;

/// <summary>Monitors task dependency graphs for cycles that would cause deadlocks.</summary>
public sealed class DeadlockDetector
{
    /// <summary>Represents a detected cycle in the dependency graph.</summary>
    public sealed class CycleResult
    {
        /// <summary>The task IDs forming the cycle.</summary>
        public List<int> CyclePath { get; init; } = new();

        /// <summary>Description of the cycle.</summary>
        public string Description { get; init; } = "";

        /// <summary>Number of tasks involved in the cycle.</summary>
        public int Length => CyclePath.Count;
    }

    /// <summary>Initializes a new deadlock detector.</summary>
    public DeadlockDetector()
    {
    }

    /// <summary>Detects all cycles in the given execution plan's dependency graph.</summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>List of detected cycles.</returns>
    public List<CycleResult> DetectCycles(ExecutionPlan plan)
    {
        var cycles = new List<CycleResult>();
        var taskIds = plan.Tasks.Select(t => t.TaskId).ToList();

        // Build adjacency list
        var adjacency = new Dictionary<int, List<int>>();
        foreach (var task in plan.Tasks)
        {
            adjacency[task.TaskId] = new List<int>(task.Dependencies);
        }

        // Use DFS to find cycles
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();
        var path = new List<int>();

        foreach (var taskId in taskIds)
        {
            if (!visited.Contains(taskId))
            {
                DetectCyclesDFS(taskId, adjacency, visited, recursionStack, path, cycles);
            }
        }

        return cycles;
    }

    /// <summary>Checks if the execution plan is deadlock-free.</summary>
    /// <param name="plan">The execution plan to validate.</param>
    /// <returns>True if no cycles exist.</returns>
    public bool IsDeadlockFree(ExecutionPlan plan)
    {
        return DetectCycles(plan).Count == 0;
    }

    private void DetectCyclesDFS(
        int nodeId,
        Dictionary<int, List<int>> adjacency,
        HashSet<int> visited,
        HashSet<int> recursionStack,
        List<int> path,
        List<CycleResult> cycles)
    {
        visited.Add(nodeId);
        recursionStack.Add(nodeId);
        path.Add(nodeId);

        if (adjacency.TryGetValue(nodeId, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    DetectCyclesDFS(neighbor, adjacency, visited, recursionStack, path, cycles);
                }
                else if (recursionStack.Contains(neighbor))
                {
                    // Found a cycle
                    int cycleStart = path.IndexOf(neighbor);
                    var cyclePath = new List<int>(path.GetRange(cycleStart, path.Count - cycleStart));
                    cyclePath.Add(neighbor);

                    cycles.Add(new CycleResult
                    {
                        CyclePath = cyclePath,
                        Description = $"Cycle detected: {string.Join(" -> ", cyclePath)}"
                    });
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        recursionStack.Remove(nodeId);
    }
}
