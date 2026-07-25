namespace MathVerse.Math.Distributed.Core;

using System.Collections.Concurrent;

/// <summary>Represents the runtime state of a task execution graph.</summary>
public sealed class ExecutionGraph
{
    private readonly ExecutionPlan _plan;
    private readonly ConcurrentDictionary<int, TaskState> _taskStates;
    private readonly ConcurrentDictionary<int, double[]> _taskResults;
    private readonly ConcurrentDictionary<int, Exception> _taskErrors;

    /// <summary>State of an individual task in the execution graph.</summary>
    public enum TaskState
    {
        /// <summary>Task is waiting to execute.</summary>
        Pending,

        /// <summary>Task is currently executing.</summary>
        Running,

        /// <summary>Task completed successfully.</summary>
        Completed,

        /// <summary>Task failed with an error.</summary>
        Failed,

        /// <summary>Task was cancelled.</summary>
        Cancelled
    }

    /// <summary>Initializes a new execution graph wrapping the given plan.</summary>
    /// <param name="plan">The execution plan to track.</param>
    public ExecutionGraph(ExecutionPlan plan)
    {
        _plan = plan ?? throw new ArgumentNullException(nameof(plan));
        _taskStates = new ConcurrentDictionary<int, TaskState>();
        _taskResults = new ConcurrentDictionary<int, double[]>();
        _taskErrors = new ConcurrentDictionary<int, Exception>();

        foreach (var task in plan.Tasks)
        {
            _taskStates[task.TaskId] = TaskState.Pending;
        }
    }

    /// <summary>The underlying execution plan.</summary>
    public ExecutionPlan Plan => _plan;

    /// <summary>Marks a task as complete with its result.</summary>
    /// <param name="taskId">The ID of the completed task.</param>
    /// <param name="result">The result produced by the task.</param>
    public void MarkComplete(int taskId, double[] result)
    {
        _taskStates[taskId] = TaskState.Completed;
        _taskResults[taskId] = result;
    }

    /// <summary>Marks a task as failed with an exception.</summary>
    /// <param name="taskId">The ID of the failed task.</param>
    /// <param name="ex">The exception that caused the failure.</param>
    public void MarkFailed(int taskId, Exception ex)
    {
        _taskStates[taskId] = TaskState.Failed;
        _taskErrors[taskId] = ex;
    }

    /// <summary>Marks a task as currently running.</summary>
    /// <param name="taskId">The ID of the task.</param>
    public void MarkRunning(int taskId)
    {
        _taskStates[taskId] = TaskState.Running;
    }

    /// <summary>Marks a task as cancelled.</summary>
    /// <param name="taskId">The ID of the task.</param>
    public void MarkCancelled(int taskId)
    {
        _taskStates[taskId] = TaskState.Cancelled;
    }

    /// <summary>Returns tasks whose dependencies are all completed and are ready to execute.</summary>
    /// <returns>List of tasks ready for execution.</returns>
    public List<ExecutionTask> GetReadyTasks()
    {
        var ready = new List<ExecutionTask>();

        foreach (var task in _plan.Tasks)
        {
            if (_taskStates[task.TaskId] != TaskState.Pending)
            {
                continue;
            }

            bool allDepsComplete = true;
            foreach (var depId in task.Dependencies)
            {
                if (_taskStates.TryGetValue(depId, out var state) && state != TaskState.Completed)
                {
                    allDepsComplete = false;
                    break;
                }
            }

            if (allDepsComplete)
            {
                ready.Add(task);
            }
        }

        return ready;
    }

    /// <summary>Gets the state of a specific task.</summary>
    /// <param name="taskId">The task ID to query.</param>
    /// <returns>The current state of the task.</returns>
    public TaskState GetTaskState(int taskId)
    {
        return _taskStates.TryGetValue(taskId, out var state) ? state : TaskState.Pending;
    }

    /// <summary>Gets the result of a completed task.</summary>
    /// <param name="taskId">The task ID to query.</param>
    /// <returns>The result, or null if not available.</returns>
    public double[]? GetTaskResult(int taskId)
    {
        return _taskResults.TryGetValue(taskId, out var result) ? result : null;
    }

    /// <summary>Gets the error from a failed task.</summary>
    /// <param name="taskId">The task ID to query.</param>
    /// <returns>The exception, or null if the task did not fail.</returns>
    public Exception? GetTaskError(int taskId)
    {
        return _taskErrors.TryGetValue(taskId, out var ex) ? ex : null;
    }

    /// <summary>Whether all tasks in the graph have completed (successfully or otherwise).</summary>
    public bool IsComplete
    {
        get
        {
            foreach (var kvp in _taskStates)
            {
                if (kvp.Value != TaskState.Completed && kvp.Value != TaskState.Failed && kvp.Value != TaskState.Cancelled)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>Gets the progress as a fraction between 0 and 1.</summary>
    public double GetProgress()
    {
        if (_plan.Tasks.Count == 0)
        {
            return 1.0;
        }

        int completed = 0;
        foreach (var kvp in _taskStates)
        {
            if (kvp.Value == TaskState.Completed || kvp.Value == TaskState.Failed || kvp.Value == TaskState.Cancelled)
            {
                completed++;
            }
        }

        return System.Math.Clamp((double)completed / _plan.Tasks.Count, 0.0, 1.0);
    }
}
