namespace MathVerse.Math.Performance.Memory;

/// <summary>
/// Provides a snapshot of memory usage statistics for the performance subsystem.
/// </summary>
public readonly record struct MemoryStatistics
{
    /// <summary>Gets the current number of live allocation bytes.</summary>
    public long CurrentAllocations { get; init; }

    /// <summary>Gets the peak number of allocation bytes observed.</summary>
    public long PeakAllocations { get; init; }

    /// <summary>Gets the cumulative total of allocation bytes.</summary>
    public long TotalAllocations { get; init; }

    /// <summary>Gets the number of bytes used by caches.</summary>
    public long CacheMemoryBytes { get; init; }

    /// <summary>Gets the number of bytes used by object pools.</summary>
    public long PoolMemoryBytes { get; init; }

    /// <summary>Gets the number of bytes used by reusable buffers.</summary>
    public long BufferMemoryBytes { get; init; }

    /// <summary>Gets the number of object reuses that avoided fresh allocations.</summary>
    public int ObjectReuseCount { get; init; }

    /// <summary>Gets the number of Generation 0 garbage collections.</summary>
    public int Gen0Collections { get; init; }

    /// <summary>Gets the number of Generation 1 garbage collections.</summary>
    public int Gen1Collections { get; init; }

    /// <summary>Gets the number of Generation 2 garbage collections.</summary>
    public int Gen2Collections { get; init; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"CurrentAllocations={CurrentAllocations}, PeakAllocations={PeakAllocations}, " +
        $"TotalAllocations={TotalAllocations}, CacheMemoryBytes={CacheMemoryBytes}, " +
        $"PoolMemoryBytes={PoolMemoryBytes}, BufferMemoryBytes={BufferMemoryBytes}, " +
        $"ObjectReuseCount={ObjectReuseCount}, Gen0Collections={Gen0Collections}, " +
        $"Gen1Collections={Gen1Collections}, Gen2Collections={Gen2Collections}";
}
