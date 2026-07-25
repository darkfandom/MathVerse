namespace MathVerse.Math.Performance.Interning;

/// <summary>
/// Provides a snapshot of expression interning statistics.
/// </summary>
public readonly record struct InternStatistics
{
    /// <summary>Gets the total number of interning lookups.</summary>
    public int TotalLookups { get; init; }

    /// <summary>Gets the number of cache hits (expressions already interned).</summary>
    public int Hits { get; init; }

    /// <summary>Gets the number of cache misses (new expressions added).</summary>
    public int Misses { get; init; }

    /// <summary>Gets the number of unique interned expressions.</summary>
    public int UniqueCount { get; init; }

    /// <summary>Gets the hit ratio.</summary>
    public double HitRatio =>
        TotalLookups > 0 ? (double)Hits / TotalLookups : 0.0;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Lookups={TotalLookups}, Hits={Hits}, Misses={Misses}, Unique={UniqueCount}, HitRatio={HitRatio:F4}";
}
