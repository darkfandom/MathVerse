namespace MathVerse.Math.Distributed.DistributedComputing;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the priority level for a distributed task.
/// </summary>
public enum TaskPriority
{
    /// <summary>Low priority task, executed when resources are available.</summary>
    Low = 0,

    /// <summary>Normal priority task, default scheduling.</summary>
    Normal = 1,

    /// <summary>High priority task, scheduled before normal tasks.</summary>
    High = 2,

    /// <summary>Critical priority task, scheduled immediately.</summary>
    Critical = 3
}

/// <summary>
/// Describes a task to be executed across the distributed cluster.
/// </summary>
public sealed class DistributedTask
{
    /// <summary>Gets the unique identifier of the task.</summary>
    public string TaskId { get; }

    /// <summary>Gets the priority level for scheduling.</summary>
    public TaskPriority Priority { get; }

    /// <summary>Gets the delegate that performs the actual computation.</summary>
    public Func<CancellationToken, ValueTask<double[]>> Execute { get; }

    /// <summary>Gets the list of task IDs that must complete before this task can execute.</summary>
    public IReadOnlyList<string> Dependencies { get; }

    /// <summary>Gets the estimated duration of the task.</summary>
    public TimeSpan EstimatedDuration { get; }

    /// <summary>Gets the maximum number of retry attempts on failure.</summary>
    public int MaxRetries { get; }

    /// <summary>Gets the maximum allowed time for the task to complete.</summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedTask"/> class.
    /// </summary>
    /// <param name="taskId">Unique task identifier.</param>
    /// <param name="priority">Scheduling priority.</param>
    /// <param name="execute">Delegate that performs the computation.</param>
    /// <param name="dependencies">Task IDs that must complete first.</param>
    /// <param name="estimatedDuration">Estimated execution duration.</param>
    /// <param name="maxRetries">Maximum retry attempts.</param>
    /// <param name="timeout">Maximum allowed execution time.</param>
    public DistributedTask(
        string taskId,
        TaskPriority priority,
        Func<CancellationToken, ValueTask<double[]>> execute,
        IReadOnlyList<string>? dependencies,
        TimeSpan estimatedDuration,
        int maxRetries,
        TimeSpan timeout)
    {
        TaskId = taskId ?? throw new ArgumentNullException(nameof(taskId));
        Priority = priority;
        Execute = execute ?? throw new ArgumentNullException(nameof(execute));
        Dependencies = dependencies ?? Array.Empty<string>();
        EstimatedDuration = estimatedDuration;
        MaxRetries = maxRetries;
        Timeout = timeout;
    }
}
