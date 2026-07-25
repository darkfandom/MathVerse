namespace MathVerse.Math.Performance.Diagnostics;

/// <summary>
/// Thread-safe collector and reporter for performance diagnostics and events.
/// </summary>
public sealed class DiagnosticReporter
{
    private readonly ConcurrentBag<PerformanceDiagnostic> _diagnostics = [];
    private readonly ConcurrentBag<PerformanceEvent> _events = [];
    private PerformanceWarning _minimumSeverity = PerformanceWarning.None;

    /// <summary>
    /// Gets or sets the minimum warning severity to retain.
    /// Diagnostics below this threshold are discarded by <see cref="Report"/>.
    /// </summary>
    public PerformanceWarning MinimumSeverity
    {
        get => _minimumSeverity;
        set => _minimumSeverity = value;
    }

    /// <summary>
    /// Records a diagnostic if its warning meets the minimum severity threshold.
    /// </summary>
    /// <param name="diagnostic">The diagnostic to record.</param>
    public void Report(PerformanceDiagnostic diagnostic)
    {
        if (diagnostic is null)
            throw new ArgumentNullException(nameof(diagnostic));

        if (_minimumSeverity == PerformanceWarning.None || (diagnostic.Warning & _minimumSeverity) != PerformanceWarning.None)
        {
            _diagnostics.Add(diagnostic);
        }
    }

    /// <summary>
    /// Records a performance event.
    /// </summary>
    /// <param name="evt">The event to record.</param>
    public void ReportEvent(PerformanceEvent evt)
    {
        if (evt is null)
            throw new ArgumentNullException(nameof(evt));

        _events.Add(evt);
    }

    /// <summary>
    /// Gets all recorded diagnostics.
    /// </summary>
    /// <returns>A read-only list of all recorded diagnostics.</returns>
    public IReadOnlyList<PerformanceDiagnostic> GetDiagnostics()
    {
        return [.. _diagnostics];
    }

    /// <summary>
    /// Gets all recorded performance events.
    /// </summary>
    /// <returns>A read-only list of all recorded events.</returns>
    public IReadOnlyList<PerformanceEvent> GetEvents()
    {
        return [.. _events];
    }

    /// <summary>
    /// Removes all recorded diagnostics and events.
    /// </summary>
    public void Clear()
    {
        while (_diagnostics.TryTake(out _)) { }
        while (_events.TryTake(out _)) { }
    }

    /// <summary>
    /// Builds a comprehensive <see cref="PerformanceReport"/> from all collected data.
    /// </summary>
    /// <returns>A performance report summarizing all recorded diagnostics and events.</returns>
    public PerformanceReport Summary()
    {
        var snapshot = new PerformanceSnapshot(
            DateTime.UtcNow,
            _events.Count,
            0L,
            0L,
            GC.CollectionCount(0),
            GC.CollectionCount(1),
            GC.CollectionCount(2),
            0.0,
            0.0);

        var sortedEvents = _events
            .OrderByDescending(e => e.DurationTicks)
            .ToList();

        return new PerformanceReport(snapshot, sortedEvents, [], []);
    }
}
