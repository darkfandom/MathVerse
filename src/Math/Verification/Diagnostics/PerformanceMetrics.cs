namespace MathVerse.Math.Verification.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// Performance metrics for verification operations.
/// </summary>
public sealed record PerformanceMetrics(
    TimeSpan TotalTime,
    TimeSpan VerificationTime,
    TimeSpan ProofSearchTime,
    TimeSpan ConstraintSolvingTime,
    TimeSpan SmtSolvingTime,
    TimeSpan ProofCheckingTime,
    long MemoryAllocated,
    long PeakMemory,
    int GcCollections,
    int CacheHits,
    int CacheMisses,
    ImmutableDictionary<string, TimeSpan> PhaseTimes,
    ImmutableDictionary<string, long> CounterValues)
{
    public double CacheHitRate => CacheHits + CacheMisses > 0 ? (double)CacheHits / (CacheHits + CacheMisses) : 1.0;
    public double VerificationTimeRatio => TotalTime > TimeSpan.Zero ? VerificationTime.TotalMilliseconds / TotalTime.TotalMilliseconds : 0;

    public static PerformanceMetrics Empty => new(
        TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero,
        TimeSpan.Zero, TimeSpan.Zero, 0, 0, 0, 0, 0,
        ImmutableDictionary<string, TimeSpan>.Empty,
        ImmutableDictionary<string, long>.Empty);

    public PerformanceMetrics Add(PerformanceMetrics other) => new(
        TotalTime + other.TotalTime,
        VerificationTime + other.VerificationTime,
        ProofSearchTime + other.ProofSearchTime,
        ConstraintSolvingTime + other.ConstraintSolvingTime,
        SmtSolvingTime + other.SmtSolvingTime,
        ProofCheckingTime + other.ProofCheckingTime,
        MemoryAllocated + other.MemoryAllocated,
        Math.Max(PeakMemory, other.PeakMemory),
        GcCollections + other.GcCollections,
        CacheHits + other.CacheHits,
        CacheMisses + other.CacheMisses,
        PhaseTimes.AddRange(other.PhaseTimes),
        CounterValues.AddRange(other.CounterValues));

    public override string ToString() =>
        $"Total: {TotalTime.TotalMilliseconds:F1}ms, Verification: {VerificationTime.TotalMilliseconds:F1}ms, " +
        $"Cache Hit Rate: {CacheHitRate:P1}, Memory: {PeakMemory / 1024 / 1024:F1} MB";
}

/// <summary>
/// Accumulates performance metrics during verification.
/// </summary>
public sealed class PerformanceMetricsCollector : IDisposable
{
    private readonly Dictionary<string, TimeSpan> _phaseTimes = new();
    private readonly Dictionary<string, long> _counters = new();
    private readonly System.Diagnostics.Stopwatch _totalWatch = System.Diagnostics.Stopwatch.StartNew();
    private readonly System.Diagnostics.Stopwatch _phaseWatch = System.Diagnostics.Stopwatch.StartNew();
    private long _initialMemory;
    private long _peakMemory;
    private string? _currentPhase;

    public PerformanceMetricsCollector()
    {
        _initialMemory = GC.GetTotalMemory(true);
        _peakMemory = _initialMemory;
    }

    public void StartPhase(string phaseName)
    {
        if (_currentPhase != null)
        {
            _phaseTimes[_currentPhase] = _phaseTimes.GetValueOrDefault(_currentPhase) + _phaseWatch.Elapsed;
        }
        _currentPhase = phaseName;
        _phaseWatch.Restart();
    }

    public void EndPhase()
    {
        if (_currentPhase != null)
        {
            _phaseTimes[_currentPhase] = _phaseTimes.GetValueOrDefault(_currentPhase) + _phaseWatch.Elapsed;
            _currentPhase = null;
        }
    }

    public void IncrementCounter(string name, long value = 1) =>
        _counters[name] = _counters.GetValueOrDefault(name) + value;

    public void RecordCacheHit() => IncrementCounter("CacheHits");
    public void RecordCacheMiss() => IncrementCounter("CacheMisses");
    public void RecordGcCollection() => IncrementCounter("GcCollections");

    public void UpdatePeakMemory()
    {
        var current = GC.GetTotalMemory(false);
        if (current > _peakMemory) _peakMemory = current;
    }

    public PerformanceMetrics Build()
    {
        EndPhase();
        _totalWatch.Stop();
        UpdatePeakMemory();

        return new PerformanceMetrics(
            _totalWatch.Elapsed,
            _phaseTimes.GetValueOrDefault("Verification", TimeSpan.Zero),
            _phaseTimes.GetValueOrDefault("ProofSearch", TimeSpan.Zero),
            _phaseTimes.GetValueOrDefault("ConstraintSolving", TimeSpan.Zero),
            _phaseTimes.GetValueOrDefault("SmtSolving", TimeSpan.Zero),
            _phaseTimes.GetValueOrDefault("ProofChecking", TimeSpan.Zero),
            GC.GetTotalMemory(false) - _initialMemory,
            _peakMemory - _initialMemory,
            (int)_counters.GetValueOrDefault("GcCollections", 0),
            (int)_counters.GetValueOrDefault("CacheHits", 0),
            (int)_counters.GetValueOrDefault("CacheMisses", 0),
            _phaseTimes.ToImmutableDictionary(),
            _counters.ToImmutableDictionary());
    }

    public void Dispose() => _totalWatch.Stop();
}