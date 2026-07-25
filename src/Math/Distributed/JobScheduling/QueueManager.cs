namespace MathVerse.Math.Distributed.JobScheduling;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Manages multiple named priority queues for task scheduling.
/// Each queue is independently prioritized and can be dequeued separately.
/// </summary>
public sealed class QueueManager
{
    private readonly ConcurrentDictionary<string, PriorityQueue<ExecutionTask, int>> _queues = new();
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueManager"/> class.
    /// </summary>
    public QueueManager() { }

    /// <summary>
    /// Creates a new named queue for task scheduling.
    /// </summary>
    /// <param name="name">The unique name for the queue.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a queue with the same name already exists.</exception>
    public void CreateQueue(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        var newQueue = new PriorityQueue<ExecutionTask, int>();
        if (!_queues.TryAdd(name, newQueue))
            throw new InvalidOperationException($"Queue '{name}' already exists.");
    }

    /// <summary>
    /// Enqueues a task into the named queue with the specified priority.
    /// Creates the queue if it does not exist.
    /// </summary>
    /// <param name="queue">The name of the queue.</param>
    /// <param name="task">The task to enqueue.</param>
    /// <param name="priority">The priority value (higher values are dequeued first).</param>
    public void Enqueue(string queue, ExecutionTask task, int priority)
    {
        if (queue == null) throw new ArgumentNullException(nameof(queue));
        if (task == null) throw new ArgumentNullException(nameof(task));

        var pq = _queues.GetOrAdd(queue, _ => new PriorityQueue<ExecutionTask, int>());
        lock (_lock)
        {
            pq.Enqueue(task, priority);
        }
    }

    /// <summary>
    /// Dequeues the highest-priority task from the named queue.
    /// </summary>
    /// <param name="queue">The name of the queue.</param>
    /// <returns>The highest-priority task, or null if the queue is empty or does not exist.</returns>
    public ExecutionTask? Dequeue(string queue)
    {
        if (queue == null) throw new ArgumentNullException(nameof(queue));

        if (!_queues.TryGetValue(queue, out var pq))
            return null;

        lock (_lock)
        {
            return pq.TryDequeue(out var task, out _) ? task : null;
        }
    }

    /// <summary>
    /// Gets the number of tasks in the named queue.
    /// </summary>
    /// <param name="queue">The name of the queue.</param>
    /// <returns>The number of tasks in the queue, or 0 if the queue does not exist.</returns>
    public int GetQueueCount(string queue)
    {
        if (queue == null) throw new ArgumentNullException(nameof(queue));

        if (!_queues.TryGetValue(queue, out var pq))
            return 0;

        lock (_lock)
        {
            return pq.Count;
        }
    }

    /// <summary>
    /// Gets the names of all existing queues.
    /// </summary>
    /// <returns>An array of queue names.</returns>
    public IReadOnlyCollection<string> GetQueueNames()
    {
        return _queues.Keys.ToArray();
    }

    /// <summary>
    /// Removes the named queue and all of its tasks.
    /// </summary>
    /// <param name="queue">The name of the queue to remove.</param>
    /// <returns>True if the queue was found and removed; otherwise, false.</returns>
    public bool RemoveQueue(string queue)
    {
        if (queue == null) throw new ArgumentNullException(nameof(queue));
        return _queues.TryRemove(queue, out _);
    }

    /// <summary>
    /// Peeks at the highest-priority task in the named queue without removing it.
    /// </summary>
    /// <param name="queue">The name of the queue.</param>
    /// <returns>The highest-priority task, or null if the queue is empty or does not exist.</returns>
    public ExecutionTask? Peek(string queue)
    {
        if (queue == null) throw new ArgumentNullException(nameof(queue));

        if (!_queues.TryGetValue(queue, out var pq))
            return null;

        lock (_lock)
        {
            return pq.TryPeek(out var task, out _) ? task : null;
        }
    }
}
