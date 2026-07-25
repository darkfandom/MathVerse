using System.Collections.Concurrent;
using System.Numerics;

namespace MathVerse.Math.Quantum.Performance;

/// <summary>
/// Thread-safe cache for computed quantum state amplitudes.
/// </summary>
public sealed class AmplitudeCache
{
    private readonly ConcurrentDictionary<string, Complex[]> _cache;

    /// <summary>
    /// Gets the number of cached entries.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmplitudeCache"/> class.
    /// </summary>
    public AmplitudeCache()
    {
        _cache = new ConcurrentDictionary<string, Complex[]>();
    }

    /// <summary>
    /// Attempts to retrieve a cached amplitude array by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">When this method returns, contains the cached amplitudes if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the entry was found; otherwise, <c>false</c>.</returns>
    public bool TryGet(string key, out Complex[]? value)
    {
        return _cache.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out value);
    }

    /// <summary>
    /// Stores an amplitude array in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amplitude array to cache.</param>
    public void Set(string key, Complex[] value)
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
