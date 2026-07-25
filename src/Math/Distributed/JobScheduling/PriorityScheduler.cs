namespace MathVerse.Math.Distributed.JobScheduling;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Describes a unit of work to be executed by the job scheduling system.
/// </summary>
public sealed class ExecutionTask
{
    /// <summary>Gets the unique identifier of this task.</summary>
    public string TaskId { get; }

    /// <summary>Gets the delegate that performs the actual computation.</summary>
    public Func<CancellationToken, ValueTask<double[]>> Execute { get; }

    /// <summary>Gets the list of task IDs that must complete before this task can start.</summary>
    public IReadOnlyList<string> Dependencies { get; }

    /// <summary>Gets the estimated duration of this task.</summary>
    public TimeSpan EstimatedDuration { get; }

    /// <summary>Gets the scheduling priority (higher values indicate higher priority).</summary>
    public int Priority { get; }

    /// <summary>Gets the maximum number of retry attempts on failure.</summary>
    public int MaxRetries { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionTask"/> class.
    /// </summary>
    /// <param name="taskId">Unique task identifier.</param>
    /// <param name="execute">Delegate that performs the computation.</param>
    /// <param name="dependencies">Task IDs that must complete first.</param>
    /// <param name="estimatedDuration">Estimated execution duration.</param>
    /// <param name="priority">Scheduling priority.</param>
    /// <param name="maxRetries">Maximum retry attempts.</param>
    public ExecutionTask(
        string taskId,
        Func<CancellationToken, ValueTask<double[]>> execute,
        IReadOnlyList<string>? dependencies,
        TimeSpan estimatedDuration,
        int priority,
        int maxRetries)
    {
        TaskId = taskId ?? throw new ArgumentNullException(nameof(taskId));
        Execute = execute ?? throw new ArgumentNullException(nameof(execute));
        Dependencies = dependencies ?? Array.Empty<string>();
        EstimatedDuration = estimatedDuration;
        Priority = priority;
        MaxRetries = maxRetries;
    }
}

/// <summary>
/// A priority-queue-based scheduler that dequeues <see cref="ExecutionTask"/> instances
/// in descending priority order using <see cref="PriorityQueue{TElement, TPriority}"/>.
/// </summary>
public sealed class PriorityScheduler
{
    private readonly PriorityQueue<ExecutionTask, int> _queue = new();
    private readonly object _lock = new();

    /// <summary>
    /// Enqueues a task with the specified priority.
    /// </summary>
    /// <param name="task">The task to enqueue.</param>
    /// <param name="priority">The scheduling priority (higher values are dequeued first).</param>
    public void Enqueue(ExecutionTask task, int priority)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        lock (_lock)
        {
            _queue.Enqueue(task, priority);
        }
    }

    /// <summary>
    /// Dequeues the highest-priority task.
    /// </summary>
    /// <returns>The highest-priority task, or null if the queue is empty.</returns>
    public ExecutionTask? Dequeue()
    {
        lock (_lock)
        {
            return _queue.TryDequeue(out var task, out _) ? task : null;
        }
    }

    /// <summary>
    /// Gets the number of tasks currently in the queue.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count;
            }
        }
    }

    /// <summary>
    /// Peeks at the highest-priority task without removing it.
    /// </summary>
    /// <returns>The highest-priority task, or null if the queue is empty.</returns>
    public ExecutionTask? Peek()
    {
        lock (_lock)
        {
            return _queue.TryPeek(out var task, out _) ? task : null;
        }
    }
}
