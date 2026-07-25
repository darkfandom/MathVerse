namespace MathVerse.Math.Distributed.IncrementalComputation;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>Caches results of function evaluations keyed by string identifiers.</summary>
public sealed class CachedExecution
{
    private readonly ConcurrentDictionary<string, double[]> _cache = new();
    private readonly ConcurrentDictionary<string, long> _accessTimestamps = new();
    private long _totalRequests;
    private long _cacheHits;

    /// <summary>Gets the number of entries currently in the cache.</summary>
    public int Count => _cache.Count;

    /// <summary>Gets the total number of cache requests since creation.</summary>
    public long TotalRequests => Interlocked.Read(ref _totalRequests);

    /// <summary>Gets the total number of cache hits since creation.</summary>
    public long CacheHits => Interlocked.Read(ref _cacheHits);

    /// <summary>Executes the compute function and caches the result, or returns the cached result if available.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="compute">The compute function to invoke on cache miss.</param>
    /// <returns>The cached or freshly computed result.</returns>
    public double[] Execute(string key, Func<double[]> compute)
    {
        Interlocked.Increment(ref _totalRequests);

        if (_cache.TryGetValue(key, out var cached))
        {
            Interlocked.Increment(ref _cacheHits);
            _accessTimestamps[key] = Stopwatch.GetTimestamp();
            return cached;
        }

        double[] result = compute();
        _cache[key] = result;
        _accessTimestamps[key] = Stopwatch.GetTimestamp();
        return result;
    }

    /// <summary>Attempts to retrieve a cached result without computing.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="result">The cached result, if found.</param>
    /// <returns>True if the result was found in the cache.</returns>
    public bool TryGet(string key, out double[]? result)
    {
        Interlocked.Increment(ref _totalRequests);

        if (_cache.TryGetValue(key, out var cached))
        {
            Interlocked.Increment(ref _cacheHits);
            _accessTimestamps[key] = Stopwatch.GetTimestamp();
            result = cached;
            return true;
        }

        result = null;
        return false;
    }

    /// <summary>Invalidates the cache entry for the specified key.</summary>
    /// <param name="key">The cache key to invalidate.</param>
    /// <returns>True if the entry existed and was removed.</returns>
    public bool Invalidate(string key)
    {
        _accessTimestamps.TryRemove(key, out _);
        return _cache.TryRemove(key, out _);
    }

    /// <summary>Invalidates all cache entries whose keys start with the given prefix.</summary>
    /// <param name="prefix">The key prefix to match.</param>
    /// <returns>The number of entries invalidated.</returns>
    public int InvalidateByPrefix(string prefix)
    {
        int count = 0;
        foreach (var key in _cache.Keys.ToArray())
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
            {
                if (_cache.TryRemove(key, out _))
                {
                    _accessTimestamps.TryRemove(key, out _);
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>Gets the cache hit rate as a value between 0 and 1.</summary>
    /// <returns>The hit rate, or 0 if no requests have been made.</returns>
    public double GetCacheHitRate()
    {
        long requests = Interlocked.Read(ref _totalRequests);
        if (requests == 0)
        {
            return 0.0;
        }

        long hits = Interlocked.Read(ref _cacheHits);
        return (double)hits / (double)requests;
    }

    /// <summary>Clears all entries from the cache.</summary>
    public void Clear()
    {
        _cache.Clear();
        _accessTimestamps.Clear();
        Interlocked.Exchange(ref _totalRequests, 0);
        Interlocked.Exchange(ref _cacheHits, 0);
    }

    /// <summary>Returns the least recently used keys, up to the specified count.</summary>
    /// <param name="count">The maximum number of keys to return.</param>
    /// <returns>An array of the least recently used keys.</returns>
    public string[] GetLruKeys(int count)
    {
        return _accessTimestamps
            .OrderBy(kvp => kvp.Value)
            .Take(count)
            .Select(kvp => kvp.Key)
            .ToArray();
    }

    /// <summary>Evicts the specified number of least recently used entries.</summary>
    /// <param name="count">The number of entries to evict.</param>
    /// <returns>The number of entries actually evicted.</returns>
    public int EvictLru(int count)
    {
        var lruKeys = GetLruKeys(count);
        int evicted = 0;
        foreach (var key in lruKeys)
        {
            if (_cache.TryRemove(key, out _))
            {
                _accessTimestamps.TryRemove(key, out _);
                evicted++;
            }
        }
        return evicted;
    }
}
