namespace MathVerse.Math.Distributed.DistributedComputing;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Distributes jobs across worker nodes and tracks task lifecycle: submission, execution, completion, and failure.
/// Supports retry logic and provides completion notification via <see cref="TaskCompletionSource{T}"/>.
/// </summary>
public sealed class JobScheduler
{
    private readonly ConcurrentDictionary<string, DistributedTask> _pendingTasks = new();
    private readonly ConcurrentDictionary<string, int> _retryCounts = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<double[]>> _completionSources = new();
    private readonly PriorityQueue<DistributedTask, int> _jobQueue = new();
    private readonly object _queueLock = new();

    /// <summary>
    /// Submits a distributed task to the scheduler for execution.
    /// </summary>
    /// <param name="task">The task to submit.</param>
    public void SubmitJob(DistributedTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        _pendingTasks[task.TaskId] = task;
        _retryCounts[task.TaskId] = 0;

        var tcs = new TaskCompletionSource<double[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        _completionSources[task.TaskId] = tcs;

        int priorityValue = (int)task.Priority;
        lock (_queueLock)
        {
            _jobQueue.Enqueue(task, priorityValue);
        }
    }

    /// <summary>
    /// Gets the next job appropriate for the specified worker from the queue.
    /// Returns the highest-priority pending task.
    /// </summary>
    /// <param name="workerId">The ID of the worker requesting a job.</param>
    /// <returns>The next task to execute, or null if no tasks are available.</returns>
    public DistributedTask? GetNextJob(string workerId)
    {
        lock (_queueLock)
        {
            if (_jobQueue.TryDequeue(out var task, out _))
            {
                return task;
            }
        }
        return null;
    }

    /// <summary>
    /// Reports that a task has completed successfully with the given result.
    /// </summary>
    /// <param name="taskId">The ID of the completed task.</param>
    /// <param name="result">The computation result.</param>
    public void ReportCompletion(string taskId, double[] result)
    {
        if (taskId == null) throw new ArgumentNullException(nameof(taskId));

        _pendingTasks.TryRemove(taskId, out _);
        _retryCounts.TryRemove(taskId, out _);

        if (_completionSources.TryRemove(taskId, out var tcs))
        {
            tcs.TrySetResult(result ?? Array.Empty<double>());
        }
    }

    /// <summary>
    /// Reports that a task has failed. If retries remain, the task is re-enqueued.
    /// Otherwise, the failure is propagated to any waiting caller.
    /// </summary>
    /// <param name="taskId">The ID of the failed task.</param>
    /// <param name="error">Description of the failure.</param>
    public void ReportFailure(string taskId, string error)
    {
        if (taskId == null) throw new ArgumentNullException(nameof(taskId));

        int retryCount = _retryCounts.AddOrUpdate(taskId, 0, (_, c) => c + 1);

        if (_pendingTasks.TryGetValue(taskId, out var task) && retryCount <= task.MaxRetries)
        {
            int priorityValue = (int)task.Priority;
            lock (_queueLock)
            {
                _jobQueue.Enqueue(task, priorityValue);
            }
        }
        else
        {
            _pendingTasks.TryRemove(taskId, out _);
            _retryCounts.TryRemove(taskId, out _);

            if (_completionSources.TryRemove(taskId, out var tcs))
            {
                tcs.TrySetException(new InvalidOperationException(
                    $"Task '{taskId}' failed after {retryCount} retries: {error}"));
            }
        }
    }

    /// <summary>
    /// Asynchronously waits for a specific task to complete.
    /// </summary>
    /// <param name="taskId">The ID of the task to wait for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the completed task.</returns>
    public async ValueTask<double[]> WaitForCompletion(string taskId, CancellationToken ct)
    {
        if (_completionSources.TryGetValue(taskId, out var tcs))
        {
            return await tcs.Task.WaitAsync(ct).ConfigureAwait(false);
        }
        throw new KeyNotFoundException($"Task '{taskId}' is not tracked by this scheduler.");
    }

    /// <summary>
    /// Gets the number of tasks currently pending execution.
    /// </summary>
    public int PendingCount => _pendingTasks.Count;
}
