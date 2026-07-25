namespace MathVerse.Math.Distributed.Core;

/// <summary>Represents a single task within an execution plan.</summary>
public sealed class ExecutionTask
{
    /// <summary>Unique identifier for this task.</summary>
    public int TaskId { get; init; }

    /// <summary>The function to execute. Accepts a cancellation token and returns an array of doubles.</summary>
    public Func<CancellationToken, ValueTask<double[]>> Execute { get; init; } = ct => ValueTask.FromResult(Array.Empty<double>());

    /// <summary>Task IDs that must complete before this task can execute.</summary>
    public List<int> Dependencies { get; init; } = new();

    /// <summary>Estimated computational cost for scheduling decisions.</summary>
    public double EstimatedCost { get; init; }

    /// <summary>Human-readable name for this task.</summary>
    public string Name { get; init; } = "";
}

/// <summary>Represents a directed acyclic graph of tasks for execution.</summary>
public sealed class ExecutionPlan
{
    /// <summary>Unique identifier for this execution plan.</summary>
    public Guid PlanId { get; init; } = Guid.NewGuid();

    /// <summary>Human-readable name for this plan.</summary>
    public string Name { get; init; } = "";

    /// <summary>The ordered list of tasks in this plan.</summary>
    public List<ExecutionTask> Tasks { get; init; } = new();

    /// <summary>Performs a topological sort of tasks based on dependencies.</summary>
    /// <returns>A list of tasks in valid execution order.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the graph contains a cycle.</exception>
    public List<ExecutionTask> TopologicalSort()
    {
        var taskMap = new Dictionary<int, ExecutionTask>();
        foreach (var task in Tasks)
        {
            taskMap[task.TaskId] = task;
        }

        var inDegree = new Dictionary<int, int>();
        var adjacency = new Dictionary<int, List<int>>();

        foreach (var task in Tasks)
        {
            inDegree[task.TaskId] = 0;
            adjacency[task.TaskId] = new List<int>();
        }

        foreach (var task in Tasks)
        {
            foreach (var depId in task.Dependencies)
            {
                if (adjacency.ContainsKey(depId))
                {
                    adjacency[depId].Add(task.TaskId);
                    inDegree[task.TaskId]++;
                }
            }
        }

        var queue = new Queue<int>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
            {
                queue.Enqueue(kvp.Key);
            }
        }

        var sorted = new List<ExecutionTask>();
        while (queue.Count > 0)
        {
            var taskId = queue.Dequeue();
            sorted.Add(taskMap[taskId]);

            foreach (var dependentId in adjacency[taskId])
            {
                inDegree[dependentId]--;
                if (inDegree[dependentId] == 0)
                {
                    queue.Enqueue(dependentId);
                }
            }
        }

        if (sorted.Count != Tasks.Count)
        {
            throw new InvalidOperationException("Execution plan contains a cycle.");
        }

        return sorted;
    }

    /// <summary>Validates that all dependencies reference existing tasks and no cycles exist.</summary>
    /// <returns>True if the plan is valid.</returns>
    public bool Validate()
    {
        var taskIds = new HashSet<int>(Tasks.Select(t => t.TaskId));

        foreach (var task in Tasks)
        {
            foreach (var depId in task.Dependencies)
            {
                if (!taskIds.Contains(depId))
                {
                    return false;
                }
            }
        }

        try
        {
            TopologicalSort();
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Computes the critical path through the task graph.</summary>
    /// <returns>The list of tasks on the critical path.</returns>
    public List<ExecutionTask> CriticalPath()
    {
        var sorted = TopologicalSort();
        var taskMap = new Dictionary<int, ExecutionTask>();
        foreach (var task in Tasks)
        {
            taskMap[task.TaskId] = task;
        }

        var earliestFinish = new Dictionary<int, double>();
        var predecessor = new Dictionary<int, int?>();

        foreach (var task in sorted)
        {
            double earliestStart = 0.0;
            int? pred = null;

            foreach (var depId in task.Dependencies)
            {
                if (earliestFinish.ContainsKey(depId) && earliestFinish[depId] > earliestStart)
                {
                    earliestStart = earliestFinish[depId];
                    pred = depId;
                }
            }

            earliestFinish[task.TaskId] = earliestStart + task.EstimatedCost;
            predecessor[task.TaskId] = pred;
        }

        int? endTask = null;
        double maxFinish = 0.0;
        foreach (var task in Tasks)
        {
            if (earliestFinish.ContainsKey(task.TaskId) && earliestFinish[task.TaskId] > maxFinish)
            {
                maxFinish = earliestFinish[task.TaskId];
                endTask = task.TaskId;
            }
        }

        var criticalTasks = new List<ExecutionTask>();
        var current = endTask;
        while (current.HasValue && taskMap.ContainsKey(current.Value))
        {
            criticalTasks.Add(taskMap[current.Value]);
            current = predecessor[current.Value];
        }

        criticalTasks.Reverse();
        return criticalTasks;
    }
}
