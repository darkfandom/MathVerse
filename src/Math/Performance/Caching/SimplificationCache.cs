namespace MathVerse.Math.Performance.Caching;

/// <summary>
/// Thread-safe cache mapping original expressions to their simplified equivalents
/// using structural expression equality.
/// </summary>
public sealed class SimplificationCache
{
    private readonly ConcurrentDictionary<Expression, Expression> _cache = new();
    private int _hits;
    private int _misses;

    /// <summary>Gets the cached simplified expression for the given original, or null if not cached.</summary>
    /// <param name="original">The original expression to look up.</param>
    /// <returns>The simplified expression if cached; otherwise, null.</returns>
    public Expression? GetSimplified(Expression original)
    {
        ArgumentNullException.ThrowIfNull(original);

        if (_cache.TryGetValue(original, out var simplified))
        {
            Interlocked.Increment(ref _hits);
            return simplified;
        }

        Interlocked.Increment(ref _misses);
        return null;
    }

    /// <summary>Stores the result of simplifying the given expression.</summary>
    /// <param name="original">The original expression before simplification.</param>
    /// <param name="simplified">The simplified expression result.</param>
    public void Store(Expression original, Expression simplified)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(simplified);

        _cache[original] = simplified;
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
