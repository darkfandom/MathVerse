namespace MathVerse.Math.Geometry.Advanced.SweepLine;

/// <summary>
/// Provides an event queue for the sweep line algorithm.
/// Events are managed in sorted order by Y coordinate (top to bottom),
/// with secondary sorting by event type priority and X coordinate.
/// Uses a sorted list with binary search for efficient insertion and removal.
/// </summary>
public sealed class EventQueue
{
    private readonly List<SweepEvent> _events;
    private readonly List<double> _sortKeys;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventQueue"/> class.
    /// </summary>
    public EventQueue()
    {
        _events = new List<SweepEvent>();
        _sortKeys = new List<double>();
    }

    /// <summary>
    /// Gets the number of events currently in the queue.
    /// </summary>
    public int Count => _events.Count;

    /// <summary>
    /// Gets a value indicating whether the queue has any events remaining.
    /// </summary>
    public bool HasEvents => _events.Count > 0;

    /// <summary>
    /// Adds an event to the queue in its correct sorted position.
    /// Events are ordered by descending Y, then by event type priority,
    /// then by ascending X coordinate.
    /// </summary>
    /// <param name="evt">The event to enqueue.</param>
    public void Enqueue(SweepEvent evt)
    {
        double key = ComputeSortKey(evt);
        int pos = FindInsertionPosition(key);
        _events.Insert(pos, evt);
        _sortKeys.Insert(pos, key);
    }

    /// <summary>
    /// Removes and returns the event with the highest priority (topmost Y coordinate).
    /// </summary>
    /// <returns>The highest priority event from the queue.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public SweepEvent Dequeue()
    {
        if (_events.Count == 0)
            throw new InvalidOperationException("Event queue is empty.");

        SweepEvent evt = _events[0];
        _events.RemoveAt(0);
        _sortKeys.RemoveAt(0);
        return evt;
    }

    /// <summary>
    /// Peeks at the highest priority event without removing it.
    /// </summary>
    /// <returns>The highest priority event.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public SweepEvent Peek()
    {
        if (_events.Count == 0)
            throw new InvalidOperationException("Event queue is empty.");
        return _events[0];
    }

    /// <summary>
    /// Removes all events from the queue.
    /// </summary>
    public void Clear()
    {
        _events.Clear();
        _sortKeys.Clear();
    }

    private double ComputeSortKey(SweepEvent evt)
    {
        double yKey = -evt.Y;

        double typePriority = evt.Type switch
        {
            SweepEventType.LeftEndpoint => 0,
            SweepEventType.Intersection => 1,
            SweepEventType.RightEndpoint => 2,
            _ => 3
        };

        return yKey * 1000.0 + typePriority + evt.X * 1e-12;
    }

    private int FindInsertionPosition(double key)
    {
        int lo = 0, hi = _sortKeys.Count;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            if (_sortKeys[mid] <= key) lo = mid + 1;
            else hi = mid;
        }
        return lo;
    }
}
