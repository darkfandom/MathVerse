namespace MathVerse.Math.Performance.Caching;

/// <summary>
/// Thread-safe cache mapping expressions to their inferred mathematical types
/// using structural expression equality.
/// </summary>
public sealed class TypeInferenceCache
{
    private readonly ConcurrentDictionary<Expression, MathType> _cache = new();
    private int _hits;
    private int _misses;

    /// <summary>Gets the cached inferred type for the given expression, or null if not cached.</summary>
    /// <param name="expr">The expression to look up.</param>
    /// <returns>The inferred <see cref="MathType"/> if cached; otherwise, null.</returns>
    public MathType? GetInferredType(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        if (_cache.TryGetValue(expr, out var type))
        {
            Interlocked.Increment(ref _hits);
            return type;
        }

        Interlocked.Increment(ref _misses);
        return null;
    }

    /// <summary>Stores the inferred type for the given expression.</summary>
    /// <param name="expr">The expression whose type was inferred.</param>
    /// <param name="type">The inferred mathematical type.</param>
    public void StoreInferredType(Expression expr, MathType type)
    {
        ArgumentNullException.ThrowIfNull(expr);
        ArgumentNullException.ThrowIfNull(type);

        _cache[expr] = type;
    }

    /// <summary>Removes all entries from the cache and resets statistics.</summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    /// <summary>Gets the current cache statistics.</summary>
    public CacheStatistics Statistics => new()
    {
        Hits = Volatile.Read(ref _hits),
        Misses = Volatile.Read(ref _misses),
        Count = _cache.Count,
        Capacity = int.MaxValue
    };
}
