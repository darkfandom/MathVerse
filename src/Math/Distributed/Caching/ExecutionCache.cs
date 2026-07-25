namespace MathVerse.Math.Distributed.Caching;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>LRU cache for execution results backed by a concurrent dictionary with access timestamps.</summary>
public sealed class ExecutionCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();
    private readonly int _maxCapacity;
    private long _hits;
    private long _misses;

    /// <summary>Represents a single entry in the execution cache.</summary>
    private sealed class CacheEntry
    {
        /// <summary>The cached result data.</summary>
        public double[] Result { get; init; } = Array.Empty<double>();

        /// <summary>The timestamp of the last access.</summary>
        public long LastAccessTimestamp { get; set; } = Stopwatch.GetTimestamp();

        /// <summary>Optional expiry timestamp. Long.MaxValue means no expiry.</summary>
        public long ExpiryTimestamp { get; init; } = long.MaxValue;

        /// <summary>Returns whether this entry has expired.</summary>
        public bool IsExpired => Stopwatch.GetTimestamp() > ExpiryTimestamp;
    }

    /// <summary>Initializes a new instance of the <see cref="ExecutionCache"/> class.</summary>
    /// <param name="maxCapacity">The maximum number of entries in the cache. Use 0 for unlimited.</param>
    public ExecutionCache(int maxCapacity = 1024)
    {
        _maxCapacity = maxCapacity > 0 ? maxCapacity : int.MaxValue;
    }

    /// <summary>Gets the current number of entries in the cache.</summary>
    public int Count => _entries.Count;

    /// <summary>Gets the total number of cache hits.</summary>
    public long Hits => Interlocked.Read(ref _hits);

    /// <summary>Gets the total number of cache misses.</summary>
    public long Misses => Interlocked.Read(ref _misses);

    /// <summary>Retrieves a cached result by key, returning null if not found or expired.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached result, or null if not found.</returns>
    public double[]? Get(string key)
    {
        if (_entries.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _entries.TryRemove(key, out _);
                Interlocked.Increment(ref _misses);
                return null;
            }

            entry.LastAccessTimestamp = Stopwatch.GetTimestamp();
            Interlocked.Increment(ref _hits);
            return entry.Result;
        }

        Interlocked.Increment(ref _misses);
        return null;
    }

    /// <summary>Stores a result in the cache with an optional expiry duration.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="result">The result data to cache.</param>
    /// <param name="expiry">Optional time-to-live. Null means no expiry.</param>
    public void Set(string key, double[] result, TimeSpan? expiry = null)
    {
        if (_entries.Count >= _maxCapacity && !_entries.ContainsKey(key))
        {
            EvictLru(1);
        }

        long expiryTimestamp = expiry.HasValue
            ? Stopwatch.GetTimestamp() + (long)(expiry.Value.TotalSeconds * Stopwatch.Frequency)
            : long.MaxValue;

        var entry = new CacheEntry
        {
            Result = result,
            LastAccessTimestamp = Stopwatch.GetTimestamp(),
            ExpiryTimestamp = expiryTimestamp
        };

        _entries[key] = entry;
    }

    /// <summary>Removes a cached entry by key.</summary>
    /// <param name="key">The cache key to remove.</param>
    /// <returns>True if the entry was found and removed.</returns>
    public bool Remove(string key)
    {
        return _entries.TryRemove(key, out _);
    }

    /// <summary>Clears all entries from the cache.</summary>
    public void Clear()
    {
        _entries.Clear();
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    /// <summary>Returns whether the cache contains a valid (non-expired) entry for the given key.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if a valid entry exists.</returns>
    public bool Contains(string key)
    {
        if (_entries.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _entries.TryRemove(key, out _);
                return false;
            }
            return true;
        }
        return false;
    }

    /// <summary>Returns the cache hit rate as a value between 0 and 1.</summary>
    /// <returns>The hit rate, or 0 if no requests have been made.</returns>
    public double GetHitRate()
    {
        long total = Interlocked.Read(ref _hits) + Interlocked.Read(ref _misses);
        if (total == 0)
        {
            return 0.0;
        }
        return (double)Interlocked.Read(ref _hits) / (double)total;
    }

    /// <summary>Removes all expired entries from the cache.</summary>
    /// <returns>The number of expired entries removed.</returns>
    public int PurgeExpired()
    {
        int removed = 0;
        long now = Stopwatch.GetTimestamp();

        foreach (var kvp in _entries)
        {
            if (now > kvp.Value.ExpiryTimestamp)
            {
                if (_entries.TryRemove(kvp.Key, out _))
                {
                    removed++;
                }
            }
        }

        return removed;
    }

    /// <summary>Evicts the specified number of least recently used entries.</summary>
    /// <param name="count">The number of entries to evict.</param>
    /// <returns>The number of entries actually evicted.</returns>
    public int EvictLru(int count)
    {
        var sorted = _entries
            .OrderBy(kvp => kvp.Value.LastAccessTimestamp)
            .Take(count)
            .Select(kvp => kvp.Key)
            .ToArray();

        int evicted = 0;
        foreach (var key in sorted)
        {
            if (_entries.TryRemove(key, out _))
            {
                evicted++;
            }
        }
        return evicted;
    }
}
