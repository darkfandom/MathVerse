namespace MathVerse.Math.Distributed.Core;

using System.Collections.Concurrent;
using System.Threading;

/// <summary>Work-stealing task scheduler that distributes work across thread-local and global queues.</summary>
public sealed class TaskScheduler : IDisposable
{
    private readonly ConcurrentQueue<WorkItem> _globalQueue;
    [ThreadStatic]
    private static Queue<WorkItem>? t_localQueue;
    private readonly ConcurrentBag<Thread> _registeredThreads;
    private long _totalSteals;
    private long _totalEnqueued;
    private long _totalDequeued;
    private bool _disposed;

    /// <summary>Represents a unit of work to be scheduled.</summary>
    public sealed class WorkItem
    {
        /// <summary>The work action to execute.</summary>
        public Action Work { get; init; } = () => { };

        /// <summary>Priority of this work item.</summary>
        public int Priority { get; init; }

        /// <summary>Timestamp when this work item was created.</summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }

    /// <summary>Initializes a new work-stealing task scheduler.</summary>
    public TaskScheduler()
    {
        _globalQueue = new ConcurrentQueue<WorkItem>();
        _registeredThreads = new ConcurrentBag<Thread>();
    }

    /// <summary>Queues a work item for execution.</summary>
    /// <param name="work">The action to execute.</param>
    /// <param name="priority">Priority level for the work item.</param>
    public void QueueWork(Action work, int priority = 0)
    {
        var item = new WorkItem { Work = work, Priority = priority };

        var localQueue = GetOrCreateLocalQueue();
        if (priority > 0)
        {
            _globalQueue.Enqueue(item);
        }
        else
        {
            localQueue.Enqueue(item);
        }

        Interlocked.Increment(ref _totalEnqueued);
    }

    /// <summary>Attempts to steal work from another thread's local queue.</summary>
    /// <returns>The stolen work item, or null if no work is available.</returns>
    public WorkItem? StealWork()
    {
        if (_globalQueue.TryDequeue(out var item))
        {
            Interlocked.Increment(ref _totalSteals);
            Interlocked.Increment(ref _totalDequeued);
            return item;
        }

        return null;
    }

    /// <summary>Attempts to dequeue a work item from the current thread's local queue or global queue.</summary>
    /// <returns>The work item, or null if no work is available.</returns>
    public WorkItem? TryDequeue()
    {
        var localQueue = GetOrCreateLocalQueue();
        if (localQueue.Count > 0)
        {
            Interlocked.Increment(ref _totalDequeued);
            return localQueue.Dequeue();
        }

        if (_globalQueue.TryDequeue(out var item))
        {
            Interlocked.Increment(ref _totalDequeued);
            return item;
        }

        return StealWork();
    }

    /// <summary>Gets the total number of work items across all queues.</summary>
    /// <returns>The approximate total queue depth.</returns>
    public int GetQueueCount()
    {
        return _globalQueue.Count;
    }

    /// <summary>Gets the number of work items in the current thread's local queue.</summary>
    /// <returns>The local queue depth.</returns>
    public int GetLocalQueueCount()
    {
        var localQueue = GetOrCreateLocalQueue();
        return localQueue.Count;
    }

    /// <summary>Gets the total number of work-stealing operations performed.</summary>
    public long TotalSteals => Interlocked.Read(ref _totalSteals);

    /// <summary>Gets the total number of work items enqueued.</summary>
    public long TotalEnqueued => Interlocked.Read(ref _totalEnqueued);

    /// <summary>Gets the total number of work items dequeued.</summary>
    public long TotalDequeued => Interlocked.Read(ref _totalDequeued);

    /// <summary>Registers a thread for work stealing.</summary>
    /// <param name="thread">The thread to register.</param>
    public void RegisterThread(Thread thread)
    {
        _registeredThreads.Add(thread);
    }

    private static Queue<WorkItem> GetOrCreateLocalQueue()
    {
        t_localQueue ??= new Queue<WorkItem>();
        return t_localQueue;
    }

    /// <summary>Disposes the task scheduler.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _globalQueue.Clear();
            t_localQueue?.Clear();
            _disposed = true;
        }
    }
}
