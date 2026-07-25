namespace MathVerse.Math.Performance.Metrics;

/// <summary>
/// Disposable timer for measuring the duration and allocations of an operation.
/// Records a <see cref="PerformanceEvent"/> to a static shared bag on <see cref="Stop"/>.
/// </summary>
public sealed class OperationTimer : IDisposable
{
    private static readonly ConcurrentBag<PerformanceEvent> s_events = [];

    private readonly Stopwatch _stopwatch;
    private readonly long _allocatedBefore;
    private bool _disposed;

    private OperationTimer(string operationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        OperationName = operationName;
        _stopwatch = Stopwatch.StartNew();
        _allocatedBefore = GC.GetTotalAllocatedBytes();
        IsRunning = true;
    }

    /// <summary>
    /// Gets the name of the operation being timed.
    /// </summary>
    public string OperationName { get; }

    /// <summary>
    /// Gets whether the timer is currently running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Gets the elapsed time since the timer was started.
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets all recorded performance events from any <see cref="OperationTimer"/> instance.
    /// </summary>
    /// <returns>A read-only collection of all recorded events.</returns>
    public static IReadOnlyList<PerformanceEvent> GetAllEvents() => [.. s_events];

    /// <summary>
    /// Creates and starts a new <see cref="OperationTimer"/>.
    /// </summary>
    /// <param name="name">The name of the operation to measure.</param>
    /// <returns>A running <see cref="OperationTimer"/> instance.</returns>
    public static OperationTimer StartNew(string name) => new(name);

    /// <summary>
    /// Stops the timer and records the performance event to the shared collection.
    /// </summary>
    public void Stop()
    {
        if (_disposed || !IsRunning)
            return;

        _stopwatch.Stop();
        IsRunning = false;

        var allocatedAfter = GC.GetTotalAllocatedBytes();
        s_events.Add(new PerformanceEvent(
            OperationName,
            _stopwatch.ElapsedTicks,
            allocatedAfter - _allocatedBefore,
            true,
            null));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }
}
