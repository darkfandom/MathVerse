namespace MathVerse.Math.Distributed.JobScheduling;

using System;
using System.Collections.Generic;

/// <summary>
/// Describes a directed acyclic graph (DAG) of tasks with their dependency relationships.
/// </summary>
public sealed class ExecutionPlan
{
    /// <summary>Gets the unique identifier of this execution plan.</summary>
    public string PlanId { get; }

    /// <summary>Gets the list of all tasks in the plan.</summary>
    public IReadOnlyList<ExecutionTask> Tasks { get; }

    /// <summary>
    /// Gets the dependency graph mapping each task ID to the list of task IDs it depends on.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> DependencyGraph { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionPlan"/> class.
    /// </summary>
    /// <param name="planId">Unique plan identifier.</param>
    /// <param name="tasks">All tasks in the plan.</param>
    /// <param name="dependencyGraph">Dependency graph mapping task IDs to their prerequisite IDs.</param>
    public ExecutionPlan(
        string planId,
        IReadOnlyList<ExecutionTask> tasks,
        IReadOnlyDictionary<string, IReadOnlyList<string>> dependencyGraph)
    {
        PlanId = planId ?? throw new ArgumentNullException(nameof(planId));
        Tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        DependencyGraph = dependencyGraph ?? throw new ArgumentNullException(nameof(dependencyGraph));
    }
}

/// <summary>
/// A DAG-aware scheduler that performs topological sorting with level grouping using Kahn's algorithm.
/// Produces ordered batches of tasks where all tasks in a batch can execute in parallel.
/// </summary>
public sealed class DAGScheduler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DAGScheduler"/> class.
    /// </summary>
    public DAGScheduler() { }

    /// <summary>
    /// Schedules an execution plan by producing ordered batches of tasks.
    /// Each batch contains tasks whose dependencies are fully satisfied,
    /// allowing all tasks within a batch to execute concurrently.
    /// </summary>
    /// <param name="plan">The execution plan to schedule.</param>
    /// <returns>An ordered list of execution batches.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a cycle is detected in the dependency graph.</exception>
    public IReadOnlyList<IReadOnlyList<ExecutionTask>> Schedule(ExecutionPlan plan)
    {
        var taskMap = new Dictionary<string, ExecutionTask>();
        foreach (var task in plan.Tasks)
        {
            taskMap[task.TaskId] = task;
        }

        var inDegree = new Dictionary<string, int>();
        var dependents = new Dictionary<string, List<string>>();

        foreach (var task in plan.Tasks)
        {
            inDegree[task.TaskId] = 0;
            dependents[task.TaskId] = new List<string>();
        }

        foreach (var (taskId, deps) in plan.DependencyGraph)
        {
            if (!inDegree.ContainsKey(taskId))
                continue;

            foreach (var depId in deps)
            {
                if (dependents.ContainsKey(depId))
                {
                    dependents[depId].Add(taskId);
                    inDegree[taskId]++;
                }
            }
        }

        var batches = new List<List<ExecutionTask>>();
        var queue = new Queue<string>();

        foreach (var (taskId, degree) in inDegree)
        {
            if (degree == 0)
                queue.Enqueue(taskId);
        }

        int processedCount = 0;

        while (queue.Count > 0)
        {
            var batch = new List<ExecutionTask>();
            var nextQueue = new Queue<string>();

            while (queue.Count > 0)
            {
                var taskId = queue.Dequeue();
                batch.Add(taskMap[taskId]);
                processedCount++;

                foreach (var dependentId in dependents[taskId])
                {
                    inDegree[dependentId]--;
                    if (inDegree[dependentId] == 0)
                        nextQueue.Enqueue(dependentId);
                }
            }

            if (batch.Count > 0)
                batches.Add(batch);

            queue = nextQueue;
        }

        if (processedCount != plan.Tasks.Count)
            throw new InvalidOperationException(
                "Cycle detected in the dependency graph. " +
                $"Processed {processedCount} of {plan.Tasks.Count} tasks.");

        return batches;
    }

    /// <summary>
    /// Validates that the execution plan's dependency graph is a valid DAG (no cycles).
    /// </summary>
    /// <param name="plan">The execution plan to validate.</param>
    /// <returns>True if the plan is a valid DAG; false if a cycle exists.</returns>
    public bool ValidatePlan(ExecutionPlan plan)
    {
        var inDegree = new Dictionary<string, int>();
        var dependents = new Dictionary<string, List<string>>();

        foreach (var task in plan.Tasks)
        {
            inDegree[task.TaskId] = 0;
            dependents[task.TaskId] = new List<string>();
        }

        foreach (var (taskId, deps) in plan.DependencyGraph)
        {
            if (!inDegree.ContainsKey(taskId))
                continue;

            foreach (var depId in deps)
            {
                if (dependents.ContainsKey(depId))
                {
                    dependents[depId].Add(taskId);
                    inDegree[taskId]++;
                }
            }
        }

        var queue = new Queue<string>();
        foreach (var (taskId, degree) in inDegree)
        {
            if (degree == 0)
                queue.Enqueue(taskId);
        }

        int count = 0;
        while (queue.Count > 0)
        {
            var taskId = queue.Dequeue();
            count++;

            foreach (var dependentId in dependents[taskId])
            {
                inDegree[dependentId]--;
                if (inDegree[dependentId] == 0)
                    queue.Enqueue(dependentId);
            }
        }

        return count == plan.Tasks.Count;
    }
}
