namespace MathVerse.Math.Performance.Parallel;

/// <summary>
/// Thread-safe priority work scheduler that dispatches actions from priority buckets.
/// </summary>
public sealed class WorkScheduler
{
    private const int MaxPriorityLevels = 16;

    private readonly ConcurrentQueue<Action>[] _queues;
    private int _count;

    /// <summary>
    /// Initializes a new instance of <see cref="WorkScheduler"/>.
    /// </summary>
    public WorkScheduler()
    {
        _queues = new ConcurrentQueue<Action>[MaxPriorityLevels];
        for (var i = 0; i < MaxPriorityLevels; i++)
        {
            _queues[i] = new ConcurrentQueue<Action>();
        }
    }

    /// <summary>
    /// Gets the total number of enqueued work items across all priorities.
    /// </summary>
    public int Count => Volatile.Read(ref _count);

    /// <summary>
    /// Enqueues a work item with the specified priority.
    /// Priority 0 is the highest; larger values are lower priority.
    /// </summary>
    /// <param name="priority">The priority level (0-based). Clamped to [0, 15].</param>
    /// <param name="work">The work item to enqueue.</param>
    public void Enqueue(int priority, Action work)
    {
        if (work is null)
            throw new ArgumentNullException(nameof(work));

        var clampedPriority = System.Math.Clamp(priority, 0, MaxPriorityLevels - 1);
        _queues[clampedPriority].Enqueue(work);
        Interlocked.Increment(ref _count);
    }

    /// <summary>
    /// Dequeues the highest-priority work item available.
    /// </summary>
    /// <returns>The next work item, or <c>null</c> if no work is available.</returns>
    public Action? Dequeue()
    {
        for (var i = 0; i < MaxPriorityLevels; i++)
        {
            if (_queues[i].TryDequeue(out var work))
            {
                Interlocked.Decrement(ref _count);
                return work;
            }
        }

        return null;
    }

    /// <summary>
    /// Removes all enqueued work items from all priority levels.
    /// </summary>
    public void Clear()
    {
        for (var i = 0; i < MaxPriorityLevels; i++)
        {
            while (_queues[i].TryDequeue(out _))
            {
            }
        }

        Interlocked.Exchange(ref _count, 0);
    }
}
