namespace MathVerse.Math.DataScience.Core;

using System.Collections.Concurrent;

/// <summary>
/// Represents a data processing session with configuration and metrics tracking.
/// </summary>
public sealed class DataContext
{
    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public Guid SessionId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when the session was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the configuration for this session.
    /// </summary>
    public DataConfiguration Configuration { get; }

    /// <summary>
    /// Gets the metrics collection for this session.
    /// </summary>
    public ConcurrentDictionary<string, double> Metrics { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataContext"/> class.
    /// </summary>
    /// <param name="configuration">The data configuration to use. If null, the default configuration is used.</param>
    public DataContext(DataConfiguration? configuration = null)
    {
        Configuration = configuration ?? DataConfiguration.Default;
    }

    /// <summary>
    /// Sets a metric value in the metrics collection.
    /// </summary>
    /// <param name="name">The name of the metric.</param>
    /// <param name="value">The metric value.</param>
    public void SetMetric(string name, double value)
    {
        Metrics[name] = value;
    }

    /// <summary>
    /// Attempts to get a metric value from the metrics collection.
    /// </summary>
    /// <param name="name">The name of the metric.</param>
    /// <param name="value">When this method returns, contains the metric value if found; otherwise, the default value.</param>
    /// <returns>true if the metric was found; otherwise, false.</returns>
    public bool TryGetMetric(string name, out double value)
    {
        return Metrics.TryGetValue(name, out value);
    }
}