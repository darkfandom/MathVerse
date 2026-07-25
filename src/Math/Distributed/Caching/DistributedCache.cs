namespace MathVerse.Math.Distributed.Caching;

using System.Collections.Concurrent;

/// <summary>Represents a cache entry with node attribution metadata.</summary>
public sealed class DistributedCacheEntry
{
    /// <summary>The cached value data.</summary>
    public double[] Value { get; init; } = Array.Empty<double>();

    /// <summary>The node identifier that originally stored this entry.</summary>
    public string SourceNodeId { get; init; } = "";

    /// <summary>The timestamp when this entry was stored.</summary>
    public DateTime StoredAt { get; init; } = DateTime.UtcNow;

    /// <summary>The number of times this entry has been accessed across all nodes.</summary>
    public long AccessCount;

    /// <summary>The timestamp of the last access.</summary>
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Simulated distributed cache that stores entries locally with node tagging.</summary>
public sealed class DistributedCache
{
    private readonly ConcurrentDictionary<string, DistributedCacheEntry> _store = new();
    private readonly string _localNodeId;
    private long _totalGets;
    private long _totalSets;
    private long _totalRemoves;

    /// <summary>Initializes a new instance of the <see cref="DistributedCache"/> class.</summary>
    /// <param name="localNodeId">The identifier of the local node.</param>
    public DistributedCache(string? localNodeId = null)
    {
        _localNodeId = localNodeId ?? Environment.MachineName;
    }

    /// <summary>Gets the total number of entries in the cache.</summary>
    public int Count => _store.Count;

    /// <summary>Gets the local node identifier.</summary>
    public string LocalNodeId => _localNodeId;

    /// <summary>Gets the total number of get operations performed.</summary>
    public long TotalGets => Interlocked.Read(ref _totalGets);

    /// <summary>Gets the total number of set operations performed.</summary>
    public long TotalSets => Interlocked.Read(ref _totalSets);

    /// <summary>Retrieves a cached value by key.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value, or null if not found.</returns>
    public double[]? Get(string key)
    {
        Interlocked.Increment(ref _totalGets);

        if (_store.TryGetValue(key, out var entry))
        {
            Interlocked.Increment(ref entry.AccessCount);
            entry.LastAccessedAt = DateTime.UtcNow;
            return entry.Value;
        }

        return null;
    }

    /// <summary>Retrieves the full cache entry including metadata by key.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The full cache entry, or null if not found.</returns>
    public DistributedCacheEntry? GetEntry(string key)
    {
        Interlocked.Increment(ref _totalGets);

        if (_store.TryGetValue(key, out var entry))
        {
            Interlocked.Increment(ref entry.AccessCount);
            entry.LastAccessedAt = DateTime.UtcNow;
            return entry;
        }

        return null;
    }

    /// <summary>Stores a value in the cache with node attribution.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="sourceNodeId">The originating node identifier. Defaults to the local node.</param>
    public void Set(string key, double[] value, string? sourceNodeId = null)
    {
        Interlocked.Increment(ref _totalSets);

        var entry = new DistributedCacheEntry
        {
            Value = value,
            SourceNodeId = sourceNodeId ?? _localNodeId,
            StoredAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };

        _store[key] = entry;
    }

    /// <summary>Removes a cached entry by key.</summary>
    /// <param name="key">The cache key to remove.</param>
    /// <returns>True if the entry was found and removed.</returns>
    public bool Remove(string key)
    {
        Interlocked.Increment(ref _totalRemoves);
        return _store.TryRemove(key, out _);
    }

    /// <summary>Returns all keys stored in the cache.</summary>
    /// <returns>An array of cache keys.</returns>
    public string[] GetAllKeys()
    {
        return _store.Keys.ToArray();
    }

    /// <summary>Returns all entries that originated from the specified node.</summary>
    /// <param name="nodeId">The node identifier to filter by.</param>
    /// <returns>A dictionary of key-value pairs from the specified node.</returns>
    public Dictionary<string, double[]> GetEntriesByNode(string nodeId)
    {
        var result = new Dictionary<string, double[]>();
        foreach (var kvp in _store)
        {
            if (kvp.Value.SourceNodeId == nodeId)
            {
                result[kvp.Key] = kvp.Value.Value;
            }
        }
        return result;
    }

    /// <summary>Returns the total number of entries originating from each node.</summary>
    /// <returns>A dictionary mapping node IDs to their entry counts.</returns>
    public Dictionary<string, int> GetEntryCountByNode()
    {
        var counts = new Dictionary<string, int>();
        foreach (var kvp in _store)
        {
            string node = kvp.Value.SourceNodeId;
            if (!counts.TryGetValue(node, out int count))
            {
                count = 0;
            }
            counts[node] = count + 1;
        }
        return counts;
    }

    /// <summary>Clears all entries from the cache.</summary>
    public void Clear()
    {
        _store.Clear();
    }

    /// <summary>Returns whether the cache contains the specified key.</summary>
    /// <param name="key">The cache key to check.</param>
    /// <returns>True if the key exists.</returns>
    public bool Contains(string key)
    {
        return _store.ContainsKey(key);
    }
}
