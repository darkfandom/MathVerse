namespace MathVerse.Math.Performance.Metrics;

/// <summary>
/// Immutable snapshot of all performance metrics captured at a single point in time.
/// </summary>
public sealed record PerformanceSnapshot
{
    /// <summary>
    /// Initializes a new performance snapshot.
    /// </summary>
    /// <param name="Timestamp">The UTC time of the snapshot.</param>
    /// <param name="TotalOperations">The total number of operations observed.</param>
    /// <param name="ElapsedMs">The total elapsed time in milliseconds.</param>
    /// <param name="AllocatedBytes">The total allocated bytes observed.</param>
    /// <param name="Gen0Collections">The number of Generation 0 GC collections.</param>
    /// <param name="Gen1Collections">The number of Generation 1 GC collections.</param>
    /// <param name="Gen2Collections">The number of Generation 2 GC collections.</param>
    /// <param name="CacheHitRatio">The cache hit ratio (0.0 to 1.0).</param>
    /// <param name="OperationsPerSecond">The throughput in operations per second.</param>
    public PerformanceSnapshot(
        DateTime Timestamp,
        long TotalOperations,
        double ElapsedMs,
        long AllocatedBytes,
        int Gen0Collections,
        int Gen1Collections,
        int Gen2Collections,
        double CacheHitRatio,
        double OperationsPerSecond)
    {
        this.Timestamp = Timestamp;
        this.TotalOperations = TotalOperations;
        this.ElapsedMs = ElapsedMs;
        this.AllocatedBytes = AllocatedBytes;
        this.Gen0Collections = Gen0Collections;
        this.Gen1Collections = Gen1Collections;
        this.Gen2Collections = Gen2Collections;
        this.CacheHitRatio = CacheHitRatio;
        this.OperationsPerSecond = OperationsPerSecond;
    }

    /// <summary>Gets the UTC time of the snapshot.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Gets the total number of operations observed.</summary>
    public long TotalOperations { get; init; }

    /// <summary>Gets the total elapsed time in milliseconds.</summary>
    public double ElapsedMs { get; init; }

    /// <summary>Gets the total allocated bytes observed.</summary>
    public long AllocatedBytes { get; init; }

    /// <summary>Gets the number of Generation 0 GC collections.</summary>
    public int Gen0Collections { get; init; }

    /// <summary>Gets the number of Generation 1 GC collections.</summary>
    public int Gen1Collections { get; init; }

    /// <summary>Gets the number of Generation 2 GC collections.</summary>
    public int Gen2Collections { get; init; }

    /// <summary>Gets the cache hit ratio (0.0 to 1.0).</summary>
    public double CacheHitRatio { get; init; }

    /// <summary>Gets the throughput in operations per second.</summary>
    public double OperationsPerSecond { get; init; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Timestamp={Timestamp:O}, Operations={TotalOperations}, Elapsed={ElapsedMs:F2}ms, " +
        $"Allocated={AllocatedBytes}B, GC=[{Gen0Collections}/{Gen1Collections}/{Gen2Collections}], " +
        $"CacheHit={CacheHitRatio:F4}, Ops/s={OperationsPerSecond:F2}";
}
