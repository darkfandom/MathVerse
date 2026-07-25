namespace MathVerse.Math.Performance.Hashing;

/// <summary>
/// Thread-safe cache that stores precomputed hash codes for expression trees.
/// </summary>
public sealed class HashCache
{
    private readonly ConcurrentDictionary<Expression, int> _cache = new();
    private int _hits;
    private int _misses;

    /// <summary>
    /// Gets the number of cached hash codes.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Attempts to retrieve a cached hash code for the given expression.
    /// </summary>
    /// <param name="expression">The expression to look up.</param>
    /// <param name="hashCode">The cached hash code if found.</param>
    /// <returns>True if a cached hash code was found.</returns>
    public bool TryGet(Expression expression, out int hashCode)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (_cache.TryGetValue(expression, out var cached))
        {
            Interlocked.Increment(ref _hits);
            hashCode = cached;
            return true;
        }

        Interlocked.Increment(ref _misses);
        hashCode = 0;
        return false;
    }

    /// <summary>
    /// Stores a hash code for the given expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="hashCode">The computed hash code.</param>
    public void Store(Expression expression, int hashCode)
    {
        ArgumentNullException.ThrowIfNull(expression);
        _cache[expression] = hashCode;
    }

    /// <summary>
    /// Removes all cached entries.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }
}
