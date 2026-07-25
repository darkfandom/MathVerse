namespace MathVerse.Math.DataScience.Performance;

using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Provides a cache with incremental update support and hit rate tracking.
/// </summary>
public sealed class IncrementalCache
{
    private readonly Dictionary<string, CacheEntry> _entries = new();
    private readonly object _lock = new();
    private long _hits;
    private long _misses;

    /// <summary>
    /// Represents a single cached entry with its value and metadata.
    /// </summary>
    private sealed class CacheEntry
    {
        /// <summary>
        /// Gets or sets the cached value.
        /// </summary>
        public double[] Value { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Gets or sets the timestamp when the entry was created or last refreshed.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the number of times this entry has been accessed.
        /// </summary>
        public long AccessCount { get; set; }
    }

    /// <summary>
    /// Gets the number of entries currently in the cache.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _entries.Count;
            }
        }
    }

    /// <summary>
    /// Gets the cache hit rate as a ratio (0-1).
    /// Returns 0 if no lookups have been performed.
    /// </summary>
    public double HitRate
    {
        get
        {
            long totalRequests = Interlocked.Read(ref _hits) + Interlocked.Read(ref _misses);
            return totalRequests > 0 ? (double)Interlocked.Read(ref _hits) / totalRequests : 0.0;
        }
    }

    /// <summary>
    /// Gets the total number of cache hits.
    /// </summary>
    public long Hits => Interlocked.Read(ref _hits);

    /// <summary>
    /// Gets the total number of cache misses.
    /// </summary>
    public long Misses => Interlocked.Read(ref _misses);

    /// <summary>
    /// Gets the cached value for the specified key, or computes and stores it if not present.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="compute">The function to compute the value on cache miss.</param>
    /// <returns>The cached or newly computed value.</returns>
    public double[] GetOrCompute(string key, Func<double[]> compute)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        if (compute is null) throw new ArgumentNullException(nameof(compute));

        lock (_lock)
        {
            if (_entries.TryGetValue(key, out CacheEntry? entry))
            {
                entry.AccessCount++;
                Interlocked.Increment(ref _hits);
                return entry.Value;
            }
        }

        Interlocked.Increment(ref _misses);

        double[] value = compute();

        lock (_lock)
        {
            _entries[key] = new CacheEntry
            {
                Value = value,
                CreatedAt = DateTimeOffset.UtcNow,
                AccessCount = 1
            };
        }

        return value;
    }

    /// <summary>
    /// Invalidates (removes) a cached entry by key.
    /// </summary>
    /// <param name="key">The cache key to invalidate.</param>
    /// <returns>true if the entry was found and removed; otherwise, false.</returns>
    public bool Invalidate(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        lock (_lock)
        {
            return _entries.Remove(key);
        }
    }

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }

        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    /// <summary>
    /// Determines whether the specified key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>true if the key exists; otherwise, false.</returns>
    public bool Contains(string key)
    {
        lock (_lock)
        {
            return _entries.ContainsKey(key);
        }
    }

    /// <summary>
    /// Gets all cache keys currently stored.
    /// </summary>
    /// <returns>A list of cache keys.</returns>
    public List<string> GetKeys()
    {
        lock (_lock)
        {
            return new List<string>(_entries.Keys);
        }
    }
}
