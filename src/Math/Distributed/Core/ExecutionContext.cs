namespace MathVerse.Math.Distributed.Core;

using System.Collections.Concurrent;

/// <summary>Represents the session context for a distributed execution.</summary>
public sealed class ExecutionContext : IDisposable
{
    private bool _disposed;

    /// <summary>Unique identifier for this execution session.</summary>
    public Guid SessionId { get; }

    /// <summary>Timestamp when this context was created.</summary>
    public DateTime CreatedAt { get; }

    /// <summary>The execution configuration for this session.</summary>
    public ExecutionOptions Configuration { get; }

    /// <summary>Token used to signal cancellation of this session.</summary>
    public CancellationToken CancellationToken => _cts.Token;

    /// <summary>Metrics collected during this execution session.</summary>
    public ConcurrentDictionary<string, double> Metrics { get; }

    /// <summary>Internal cancellation token source for session control.</summary>
    private readonly CancellationTokenSource _cts;

    /// <summary>Initializes a new execution context with default options.</summary>
    public ExecutionContext() : this(ExecutionOptions.Default)
    {
    }

    /// <summary>Initializes a new execution context with the specified options.</summary>
    /// <param name="configuration">The execution options for this session.</param>
    public ExecutionContext(ExecutionOptions configuration)
    {
        SessionId = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _cts = new CancellationTokenSource(configuration.Timeout);
        Metrics = new ConcurrentDictionary<string, double>();
    }

    /// <summary>Sets a metric value for this session.</summary>
    /// <param name="key">The metric name.</param>
    /// <param name="value">The metric value.</param>
    public void SetMetric(string key, double value)
    {
        Metrics[key] = value;
    }

    /// <summary>Attempts to retrieve a metric value.</summary>
    /// <param name="key">The metric name.</param>
    /// <param name="value">The metric value if found.</param>
    /// <returns>True if the metric exists.</returns>
    public bool TryGetMetric(string key, out double value)
    {
        return Metrics.TryGetValue(key, out value);
    }

    /// <summary>Requests cancellation of this execution session.</summary>
    public void RequestCancellation()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    /// <summary>Disposes the execution context and its resources.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _cts.Dispose();
            _disposed = true;
        }
    }
}
