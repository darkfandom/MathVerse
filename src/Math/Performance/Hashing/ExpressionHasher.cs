namespace MathVerse.Math.Performance.Hashing;

/// <summary>
/// Computes hash codes for expressions using structural equality.
/// Thread-safe and compatible with Native AOT.
/// </summary>
public sealed class CachedExpressionHasher
{
    private readonly StructuralHasher _structural = new();
    private readonly HashCache _cache = new();

    /// <summary>
    /// Computes the hash code for the given expression, using a cache when available.
    /// </summary>
    /// <param name="expression">The expression to hash.</param>
    /// <returns>The hash code.</returns>
    public int ComputeHash(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (_cache.TryGet(expression, out var cached))
            return cached;

        var hash = _structural.Hash(expression);
        _cache.Store(expression, hash);
        return hash;
    }

    /// <summary>
    /// Clears the hash cache.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }
}
