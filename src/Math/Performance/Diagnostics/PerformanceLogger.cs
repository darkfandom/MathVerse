namespace MathVerse.Math.Performance.Diagnostics;

/// <summary>
/// Structured performance logger that measures and records operations using <see cref="DiagnosticReporter"/>.
/// Thread-safe.
/// </summary>
public sealed class PerformanceLogger
{
    private readonly DiagnosticReporter _reporter;

    /// <summary>
    /// Initializes a new <see cref="PerformanceLogger"/> with the specified reporter.
    /// </summary>
    /// <param name="reporter">The diagnostic reporter to emit events to.</param>
    public PerformanceLogger(DiagnosticReporter reporter)
    {
        _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
    }

    /// <summary>
    /// Measures the execution time and allocation delta of an action, recording a <see cref="PerformanceEvent"/>.
    /// </summary>
    /// <param name="operation">The name of the operation being measured.</param>
    /// <param name="action">The action to execute and measure.</param>
    public void Log(string operation, Action action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentNullException.ThrowIfNull(action);

        var allocatedBefore = GC.GetTotalAllocatedBytes();
        var sw = Stopwatch.StartNew();
        var success = false;

        try
        {
            action();
            success = true;
        }
        finally
        {
            sw.Stop();
            var allocatedAfter = GC.GetTotalAllocatedBytes();
            _reporter.ReportEvent(new PerformanceEvent(operation, sw.ElapsedTicks, allocatedAfter - allocatedBefore, success, null));
        }
    }

    /// <summary>
    /// Measures the execution time, allocation delta, and result of a function, recording a <see cref="PerformanceEvent"/>.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="operation">The name of the operation being measured.</param>
    /// <param name="func">The function to execute and measure.</param>
    /// <returns>The result returned by the function.</returns>
    public T Log<T>(string operation, Func<T> func)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentNullException.ThrowIfNull(func);

        var allocatedBefore = GC.GetTotalAllocatedBytes();
        var sw = Stopwatch.StartNew();
        T result = default!;
        var success = false;

        try
        {
            result = func();
            success = true;
        }
        finally
        {
            sw.Stop();
            var allocatedAfter = GC.GetTotalAllocatedBytes();
            _reporter.ReportEvent(new PerformanceEvent(operation, sw.ElapsedTicks, allocatedAfter - allocatedBefore, success, null));
        }

        return result;
    }

    /// <summary>
    /// Gets all recorded performance events from the underlying reporter.
    /// </summary>
    /// <returns>A read-only list of all recorded events.</returns>
    public IReadOnlyList<PerformanceEvent> GetEvents() => _reporter.GetEvents();

    /// <summary>
    /// Clears all recorded events from the underlying reporter.
    /// </summary>
    public void Clear() => _reporter.Clear();

    /// <summary>
    /// Computes the average duration in milliseconds for all events matching the specified operation name.
    /// </summary>
    /// <param name="operation">The operation name to filter by.</param>
    /// <returns>The average duration in milliseconds, or zero if no matching events exist.</returns>
    public double AverageOperationMs(string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        var events = _reporter.GetEvents();
        double totalMs = 0.0;
        var count = 0;

        foreach (var evt in events)
        {
            if (evt.Operation == operation)
            {
                totalMs += evt.DurationMs;
                count++;
            }
        }

        return count > 0 ? totalMs / count : 0.0;
    }
}
