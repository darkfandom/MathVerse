namespace MathVerse.Math.Distributed.Caching;

using System.Collections.Concurrent;

/// <summary>File-backed persistent cache that stores data as files on disk.</summary>
public sealed class PersistentCache
{
    private readonly string _cacheDirectory;
    private readonly ConcurrentDictionary<string, CacheMetadata> _metadata = new();
    private long _totalGets;
    private long _totalSets;
    private long _totalBytesWritten;

    /// <summary>Represents metadata for a cached entry.</summary>
    private sealed class CacheMetadata
    {
        /// <summary>The file name within the cache directory.</summary>
        public string FileName { get; init; } = "";

        /// <summary>The size of the cached data in bytes.</summary>
        public long DataSize { get; init; }

        /// <summary>The timestamp when this entry was stored.</summary>
        public DateTime StoredAt { get; init; } = DateTime.UtcNow;
    }

    /// <summary>Initializes a new instance of the <see cref="PersistentCache"/> class.</summary>
    /// <param name="cacheDirectory">The directory to store cache files in.</param>
    public PersistentCache(string? cacheDirectory = null)
    {
        _cacheDirectory = cacheDirectory
            ?? Path.Combine(Path.GetTempPath(), "MathVerse", "PersistentCache");

        Directory.CreateDirectory(_cacheDirectory);
    }

    /// <summary>Gets the number of entries currently cached on disk.</summary>
    public int Count => _metadata.Count;

    /// <summary>Gets the total number of bytes written to disk.</summary>
    public long TotalBytesWritten => Interlocked.Read(ref _totalBytesWritten);

    /// <summary>Gets the full path of the cache directory.</summary>
    public string CacheDirectory => _cacheDirectory;

    /// <summary>Retrieves cached data by key.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached data, or null if not found.</returns>
    public byte[]? Get(string key)
    {
        Interlocked.Increment(ref _totalGets);

        if (!_metadata.TryGetValue(key, out var metadata))
        {
            return null;
        }

        string filePath = Path.Combine(_cacheDirectory, metadata.FileName);

        if (!File.Exists(filePath))
        {
            _metadata.TryRemove(key, out _);
            return null;
        }

        try
        {
            return File.ReadAllBytes(filePath);
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>Stores data in the cache with the specified key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="data">The data to cache.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public void Set(string key, byte[] data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        Interlocked.Increment(ref _totalSets);

        string sanitizedKey = SanitizeKey(key);
        string fileName = sanitizedKey + ".cache";
        string filePath = Path.Combine(_cacheDirectory, fileName);

        try
        {
            File.WriteAllBytes(filePath, data);
            Interlocked.Add(ref _totalBytesWritten, data.Length);
        }
        catch (IOException)
        {
            return;
        }

        _metadata[key] = new CacheMetadata
        {
            FileName = fileName,
            DataSize = data.Length,
            StoredAt = DateTime.UtcNow
        };
    }

    /// <summary>Removes the cached entry for the specified key from both disk and metadata.</summary>
    /// <param name="key">The cache key to remove.</param>
    /// <returns>True if the entry existed and was removed.</returns>
    public bool Remove(string key)
    {
        if (_metadata.TryRemove(key, out var metadata))
        {
            string filePath = Path.Combine(_cacheDirectory, metadata.FileName);
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    /// <summary>Returns whether the cache contains an entry for the specified key.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if the entry exists on disk.</returns>
    public bool Contains(string key)
    {
        if (!_metadata.TryGetValue(key, out var metadata))
        {
            return false;
        }

        string filePath = Path.Combine(_cacheDirectory, metadata.FileName);
        return File.Exists(filePath);
    }

    /// <summary>Flushes all pending operations and removes all cache files from disk.</summary>
    /// <returns>The number of files removed.</returns>
    public int Flush()
    {
        int removed = 0;
        foreach (var key in _metadata.Keys.ToArray())
        {
            if (Remove(key))
            {
                removed++;
            }
        }
        _metadata.Clear();
        return removed;
    }

    /// <summary>Returns the total size in bytes of all cached entries.</summary>
    /// <returns>The total cache size in bytes.</returns>
    public long GetTotalSize()
    {
        long total = 0;
        foreach (var meta in _metadata.Values)
        {
            total += meta.DataSize;
        }
        return total;
    }

    /// <summary>Returns all cached keys.</summary>
    /// <returns>An array of cache keys.</returns>
    public string[] GetAllKeys()
    {
        return _metadata.Keys.ToArray();
    }

    /// <summary>Removes cache entries older than the specified duration.</summary>
    /// <param name="maxAge">The maximum age of entries to keep.</param>
    /// <returns>The number of entries purged.</returns>
    public int PurgeExpired(TimeSpan maxAge)
    {
        DateTime cutoff = DateTime.UtcNow - maxAge;
        int purged = 0;

        foreach (var kvp in _metadata)
        {
            if (kvp.Value.StoredAt < cutoff)
            {
                if (Remove(kvp.Key))
                {
                    purged++;
                }
            }
        }

        return purged;
    }

    private static string SanitizeKey(string key)
    {
        var sb = new StringBuilder(key.Length);
        foreach (char c in key)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }
        return sb.ToString();
    }
}
