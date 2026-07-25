namespace MathVerse.Math.HPC.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using MathVerse.Math.HPC.Diagnostics;

/// <summary>
/// Tracks a single HPC operation lifecycle.
/// </summary>
public sealed class HpcSession
{
    private readonly ConcurrentBag<HpcResult> _results;
    private readonly ConcurrentDictionary<string, object> _metadata;
    private readonly Stopwatch _stopwatch;

    public HpcSession(Guid sessionId, HpcContext context)
    {
        SessionId = sessionId;
        Context = context;
        _results = new ConcurrentBag<HpcResult>();
        _metadata = new ConcurrentDictionary<string, object>();
        _stopwatch = Stopwatch.StartNew();
        StartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public Guid SessionId { get; }

    /// <summary>
    /// Gets the HPC context.
    /// </summary>
    public HpcContext Context { get; }

    /// <summary>
    /// Gets the start time.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// Gets the elapsed time.
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets the results collected during the session.
    /// </summary>
    public IReadOnlyCollection<HpcResult> Results => _results.ToArray();

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <summary>
    /// Adds a result to the session.
    /// </summary>
    /// <param name="result">The result to add.</param>
    public void AddResult(HpcResult result) => _results.Add(result);

    /// <summary>
    /// Adds metadata to the session.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    public void AddMetadata(string key, object value) => _metadata.TryAdd(key, value);

    /// <summary>
    /// Tries to get metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>True if found; otherwise, false.</returns>
    public bool TryGetMetadata(string key, out object? value) => _metadata.TryGetValue(key, out value);

    /// <summary>
    /// Stops the session timer.
    /// </summary>
    public void Stop() => _stopwatch.Stop();

    /// <summary>
    /// Gets a summary of the session.
    /// </summary>
    public SessionSummary GetSummary()
    {
        var results = _results.ToArray();
return new SessionSummary(
            SessionId,
            StartTime,
            Elapsed,
            results.Length,
            results.Count(r => r.Success),
            results.Count(r => !r.Success),
            results.Sum(r => r.Duration.Ticks) / (results.Length > 0 ? results.Length : 1),
            _metadata.ToArray().ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }
}

/// <summary>
/// Summary of an HPC session.
/// </summary>
public sealed record SessionSummary(
    Guid SessionId,
    DateTime StartTime,
    TimeSpan TotalDuration,
    int TotalOperations,
    int SuccessfulOperations,
    int FailedOperations,
    long AverageDurationTicks,
    IReadOnlyDictionary<string, object> Metadata)
{
    public TimeSpan AverageDuration => TimeSpan.FromTicks(AverageDurationTicks);
    public double SuccessRate => TotalOperations > 0 ? (double)SuccessfulOperations / TotalOperations : 0.0;
}
