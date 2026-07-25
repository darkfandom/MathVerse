namespace MathVerse.Math.Interop.Performance;

using System;
using System.Collections.Concurrent;

/// <summary>
/// Thread-safe in-memory cache for serialized data keyed by content hash.
/// </summary>
public sealed class SerializationCache
{
    private readonly ConcurrentDictionary<string, byte[]> _cache = new();

    /// <summary>
    /// Gets the number of entries in the cache.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Tries to retrieve a cached byte array for the given key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="data">The cached data if found.</param>
    /// <returns>True if the entry exists.</returns>
    public bool TryGet(string key, out byte[]? data)
    {
        _ = key ?? throw new ArgumentNullException(nameof(key));
        return _cache.TryGetValue(key, out data);
    }

    /// <summary>
    /// Stores a byte array in the cache under the given key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="data">The data to cache.</param>
    public void Store(string key, byte[] data)
    {
        _ = key ?? throw new ArgumentNullException(nameof(key));
        _ = data ?? throw new ArgumentNullException(nameof(data));
        _cache[key] = data;
    }

    /// <summary>
    /// Removes the entry associated with the given key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if the entry was removed.</returns>
    public bool Remove(string key)
    {
        _ = key ?? throw new ArgumentNullException(nameof(key));
        return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
