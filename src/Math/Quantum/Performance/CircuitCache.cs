using System.Collections.Concurrent;

namespace MathVerse.Math.Quantum.Performance;

/// <summary>
/// Thread-safe cache for compiled quantum circuit objects.
/// </summary>
public sealed class CircuitCache
{
    private readonly ConcurrentDictionary<string, object> _cache;

    /// <summary>
    /// Gets the number of cached entries.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitCache"/> class.
    /// </summary>
    public CircuitCache()
    {
        _cache = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// Attempts to retrieve a compiled circuit by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">When this method returns, contains the compiled circuit if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the entry was found; otherwise, <c>false</c>.</returns>
    public bool TryGet(string key, out object? value)
    {
        return _cache.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out value);
    }

    /// <summary>
    /// Stores a compiled circuit in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The compiled circuit to cache.</param>
    public void Set(string key, object value)
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
