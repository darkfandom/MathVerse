namespace MathVerse.Math.Performance.Caching;

/// <summary>
/// Provides a snapshot of cache hit/miss/eviction metrics.
/// </summary>
public readonly record struct CacheStatistics
{
    /// <summary>Gets the number of cache hits.</summary>
    public int Hits { get; init; }

    /// <summary>Gets the number of cache misses.</summary>
    public int Misses { get; init; }

    /// <summary>Gets the number of entries evicted from the cache.</summary>
    public int Evictions { get; init; }

    /// <summary>Gets the current number of entries in the cache.</summary>
    public int Count { get; init; }

    /// <summary>Gets the maximum capacity of the cache.</summary>
    public int Capacity { get; init; }

    /// <summary>Gets the ratio of hits to total lookups (hits + misses).</summary>
    public double HitRatio =>
        Hits + Misses > 0 ? (double)Hits / (Hits + Misses) : 0.0;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Hits={Hits}, Misses={Misses}, Evictions={Evictions}, " +
        $"Count={Count}, Capacity={Capacity}, HitRatio={HitRatio:F4}";
}
