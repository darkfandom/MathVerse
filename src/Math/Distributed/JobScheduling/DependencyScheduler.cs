namespace MathVerse.Math.Distributed.JobScheduling;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// A scheduler that respects task dependencies using reference counting.
/// A task becomes ready for execution only when all of its dependencies have completed.
/// </summary>
public sealed class DependencyScheduler
{
    private readonly ConcurrentDictionary<string, ExecutionTask> _allTasks = new();
    private readonly ConcurrentDictionary<string, int> _inDegree = new();
    private readonly ConcurrentDictionary<string, List<string>> _dependents = new();
    private readonly ConcurrentDictionary<string, bool> _completed = new();
    private int _totalSubmitted;

    /// <summary>
    /// Submits a task to the scheduler, registering its dependencies.
    /// </summary>
    /// <param name="task">The task to submit.</param>
    public void Submit(ExecutionTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        _allTasks[task.TaskId] = task;
        _inDegree.TryAdd(task.TaskId, 0);
        _dependents.TryAdd(task.TaskId, new List<string>());
        _completed.TryAdd(task.TaskId, false);

        foreach (var depId in task.Dependencies)
        {
            _inDegree.AddOrUpdate(task.TaskId, 1, (_, current) => current + 1);

            var dependents = _dependents.GetOrAdd(depId, _ => new List<string>());
            lock (dependents)
            {
                dependents.Add(task.TaskId);
            }
        }

        Interlocked.Increment(ref _totalSubmitted);
    }

    /// <summary>
    /// Returns all tasks whose dependencies have been fully satisfied and are ready for execution.
    /// </summary>
    /// <returns>A list of tasks ready to be executed.</returns>
    public IReadOnlyList<ExecutionTask> GetReadyTasks()
    {
        var ready = new List<ExecutionTask>();

        foreach (var kvp in _inDegree)
        {
            if (kvp.Value == 0 && _completed.TryGetValue(kvp.Key, out var done) && !done)
            {
                if (_allTasks.TryGetValue(kvp.Key, out var task))
                {
                    ready.Add(task);
                }
            }
        }

        return ready;
    }

    /// <summary>
    /// Marks a task as completed and decrements the in-degree of all its dependents.
    /// </summary>
    /// <param name="taskId">The ID of the completed task.</param>
    public void MarkCompleted(string taskId)
    {
        _completed[taskId] = true;

        if (_dependents.TryGetValue(taskId, out var dependents))
        {
            List<string> snapshot;
            lock (dependents)
            {
                snapshot = new List<string>(dependents);
            }

            foreach (var dependentId in snapshot)
            {
                _inDegree.AddOrUpdate(dependentId, 0, (_, current) => System.Math.Max(0, current - 1));
            }
        }
    }

    /// <summary>
    /// Gets the total number of tasks submitted to this scheduler.
    /// </summary>
    public int TotalSubmitted => _totalSubmitted;

    /// <summary>
    /// Gets the number of tasks that have been marked as completed.
    /// </summary>
    public int CompletedCount
    {
        get
        {
            int count = 0;
            foreach (var kvp in _completed)
            {
                if (kvp.Value) count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Checks whether all submitted tasks have been completed.
    /// </summary>
    public bool AllCompleted => CompletedCount == _totalSubmitted && _totalSubmitted > 0;
}
