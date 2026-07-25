namespace MathVerse.Math.AI.Core;

using System.Collections.Concurrent;
using System.Collections.Immutable;

/// <summary>Execution context for AI operations, carrying session state, cache, and metadata.</summary>
public sealed class AIContext
{
    private readonly ConcurrentDictionary<string, object> _cache = new();
    private readonly ConcurrentDictionary<string, string> _metadata = new();

    /// <summary>Unique identifier for this execution session.</summary>
    public string SessionId { get; }

    /// <summary>UTC timestamp when this context was created.</summary>
    public DateTime CreatedAt { get; }

    /// <summary>Configuration that governs this context's behaviour.</summary>
    public AIConfiguration Configuration { get; }

    /// <summary>Snapshot of accumulated metrics. Updated atomically on every <see cref="SetMetric"/> call.</summary>
    public ImmutableDictionary<string, double> Metrics { get; private set; }

    /// <summary>Initialises a new execution context.</summary>
    /// <param name="configuration">Optional configuration; uses <see cref="AIConfiguration.Default"/> when <c>null</c>.</param>
    public AIContext(AIConfiguration? configuration = null)
    {
        SessionId = Guid.NewGuid().ToString("N");
        CreatedAt = DateTime.UtcNow;
        Configuration = configuration ?? AIConfiguration.Default;
        Metrics = ImmutableDictionary<string, double>.Empty;
    }

    /// <summary>Stores a value in the computation cache.</summary>
    /// <param name="cacheKey">Unique key for the cached entry.</param>
    /// <param name="value">Object to cache.</param>
    public void CacheSet(string cacheKey, object value) => _cache[cacheKey] = value;

    /// <summary>Attempts to retrieve a typed value from the computation cache.</summary>
    /// <typeparam name="T">Expected type of the cached value.</typeparam>
    /// <param name="cacheKey">Key to look up.</param>
    /// <param name="value">The cached value when found; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the key was found and the value is of type <typeparamref name="T"/>.</returns>
    public bool CacheTryGet<T>(string cacheKey, out T? value) where T : class
    {
        if (_cache.TryGetValue(cacheKey, out object? obj) && obj is T typed)
        {
            value = typed;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>Records a named metric, replacing any previous value with the same name.</summary>
    /// <param name="name">Metric name.</param>
    /// <param name="value">Metric value.</param>
    public void SetMetric(string name, double value)
    {
        Metrics = Metrics.SetItem(name, value);
    }

    /// <summary>Stores a string metadata entry.</summary>
    /// <param name="key">Metadata key.</param>
    /// <param name="value">Metadata value.</param>
    public void SetMetadata(string key, string value) => _metadata[key] = value;

    /// <summary>Retrieves a previously stored metadata entry.</summary>
    /// <param name="key">Metadata key.</param>
    /// <param name="value">The metadata value when found; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the key exists.</returns>
    public bool TryGetMetadata(string key, out string? value) => _metadata.TryGetValue(key, out value);

    /// <summary>Removes all entries from the computation cache.</summary>
    public void ClearCache() => _cache.Clear();

    /// <summary>Returns the current number of cached entries.</summary>
    public int CacheCount => _cache.Count;

    /// <summary>Returns <c>true</c> when the cache contains the specified key.</summary>
    /// <param name="cacheKey">Key to probe.</param>
    /// <returns><c>true</c> if present.</returns>
    public bool CacheContains(string cacheKey) => _cache.ContainsKey(cacheKey);

    /// <summary>Removes a single entry from the computation cache.</summary>
    /// <param name="cacheKey">Key to remove.</param>
    /// <returns><c>true</c> if the entry was present and removed.</returns>
    public bool CacheRemove(string cacheKey) => _cache.TryRemove(cacheKey, out _);
}
