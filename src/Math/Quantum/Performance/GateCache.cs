using System.Collections.Concurrent;
using System.Numerics;

namespace MathVerse.Math.Quantum.Performance;

/// <summary>
/// Thread-safe cache for quantum gate matrix representations.
/// </summary>
public sealed class GateCache
{
    private readonly ConcurrentDictionary<string, Complex[,]> _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="GateCache"/> class.
    /// </summary>
    public GateCache()
    {
        _cache = new ConcurrentDictionary<string, Complex[,]>();
    }

    /// <summary>
    /// Attempts to retrieve a cached gate matrix by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">When this method returns, contains the cached matrix if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the entry was found; otherwise, <c>false</c>.</returns>
    public bool TryGet(string key, out Complex[,]? value)
    {
        return _cache.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out value);
    }

    /// <summary>
    /// Stores a gate matrix in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The gate matrix to cache.</param>
    public void Set(string key, Complex[,] value)
    {
        _cache[key ?? throw new ArgumentNullException(nameof(key))] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Clears all cached entries.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
