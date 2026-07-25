namespace MathVerse.Math.Distributed.PerformanceMonitoring;

using System.Collections.Concurrent;

/// <summary>Profiles cache hit and miss rates across named cache instances.</summary>
public sealed class CacheProfiler
{
    private readonly ConcurrentDictionary<string, CacheStats> _stats = new();
    private long _globalHits;
    private long _globalMisses;

    /// <summary>Represents accumulated statistics for a single cache.</summary>
    private sealed class CacheStats
    {
        /// <summary>The number of cache hits.</summary>
        public long Hits;

        /// <summary>The number of cache misses.</summary>
        public long Misses;
    }

    /// <summary>Gets the total number of cache hits across all caches.</summary>
    public long GlobalHits => Interlocked.Read(ref _globalHits);

    /// <summary>Gets the total number of cache misses across all caches.</summary>
    public long GlobalMisses => Interlocked.Read(ref _globalMisses);

    /// <summary>Gets the number of named caches being profiled.</summary>
    public int CacheCount => _stats.Count;

    /// <summary>Records a cache hit for the specified cache.</summary>
    /// <param name="cacheName">The name of the cache.</param>
    public void RecordHit(string cacheName)
    {
        Interlocked.Increment(ref _globalHits);
        var stats = _stats.GetOrAdd(cacheName, _ => new CacheStats());
        Interlocked.Increment(ref stats.Hits);
    }

    /// <summary>Records a cache miss for the specified cache.</summary>
    /// <param name="cacheName">The name of the cache.</param>
    public void RecordMiss(string cacheName)
    {
        Interlocked.Increment(ref _globalMisses);
        var stats = _stats.GetOrAdd(cacheName, _ => new CacheStats());
        Interlocked.Increment(ref stats.Misses);
    }

    /// <summary>Records both a hit and a miss batch for the specified cache.</summary>
    /// <param name="cacheName">The name of the cache.</param>
    /// <param name="hits">The number of hits to record.</param>
    /// <param name="misses">The number of misses to record.</param>
    public void RecordBatch(string cacheName, long hits, long misses)
    {
        Interlocked.Add(ref _globalHits, hits);
        Interlocked.Add(ref _globalMisses, misses);

        var stats = _stats.GetOrAdd(cacheName, _ => new CacheStats());
        Interlocked.Add(ref stats.Hits, hits);
        Interlocked.Add(ref stats.Misses, misses);
    }

    /// <summary>Returns the hit rate for the specified cache as a value between 0 and 1.</summary>
    /// <param name="cacheName">The name of the cache.</param>
    /// <returns>The hit rate, or 0 if no records exist for this cache.</returns>
    public double GetHitRate(string cacheName)
    {
        if (!_stats.TryGetValue(cacheName, out var stats))
        {
            return 0.0;
        }

        long hits = Interlocked.Read(ref stats.Hits);
        long misses = Interlocked.Read(ref stats.Misses);
        long total = hits + misses;

        if (total == 0)
        {
            return 0.0;
        }

        return (double)hits / (double)total;
    }

    /// <summary>Returns the global hit rate across all caches as a value between 0 and 1.</summary>
    /// <returns>The global hit rate, or 0 if no records exist.</returns>
    public double GetGlobalHitRate()
    {
        long hits = Interlocked.Read(ref _globalHits);
        long misses = Interlocked.Read(ref _globalMisses);
        long total = hits + misses;

        if (total == 0)
        {
            return 0.0;
        }

        return (double)hits / (double)total;
    }

    /// <summary>Returns detailed statistics for the specified cache.</summary>
    /// <param name="cacheName">The name of the cache.</param>
    /// <returns>A tuple of hits and misses, or (0, 0) if the cache is not tracked.</returns>
    public (long Hits, long Misses) GetStats(string cacheName)
    {
        if (_stats.TryGetValue(cacheName, out var stats))
        {
            return (Interlocked.Read(ref stats.Hits), Interlocked.Read(ref stats.Misses));
        }
        return (0, 0);
    }

    /// <summary>Returns statistics for all profiled caches.</summary>
    /// <returns>A dictionary mapping cache names to (hits, misses) tuples.</returns>
    public Dictionary<string, (long Hits, long Misses, double HitRate)> GetAllStats()
    {
        var result = new Dictionary<string, (long Hits, long Misses, double HitRate)>();
        foreach (var kvp in _stats)
        {
            long hits = Interlocked.Read(ref kvp.Value.Hits);
            long misses = Interlocked.Read(ref kvp.Value.Misses);
            long total = hits + misses;
            double rate = total > 0 ? (double)hits / (double)total : 0.0;
            result[kvp.Key] = (hits, misses, rate);
        }
        return result;
    }

    /// <summary>Resets all profiling statistics.</summary>
    public void Reset()
    {
        _stats.Clear();
        Interlocked.Exchange(ref _globalHits, 0);
        Interlocked.Exchange(ref _globalMisses, 0);
    }
}
