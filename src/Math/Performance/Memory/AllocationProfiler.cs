namespace MathVerse.Math.Performance.Memory;

/// <summary>
/// Statistics for a single allocation category.
/// </summary>
/// <param name="Bytes">Total bytes allocated in this category.</param>
/// <param name="Count">Number of allocations in this category.</param>
public readonly record struct CategoryStats(long Bytes, int Count);

/// <summary>
/// Thread-safe profiler that tracks memory allocations broken down by named category.
/// </summary>
public sealed class AllocationProfiler
{
    private readonly ConcurrentDictionary<string, long> _bytesByCategory = new();
    private readonly ConcurrentDictionary<string, int> _countByCategory = new();

    /// <summary>Records an allocation in the specified category.</summary>
    /// <param name="category">The allocation category name.</param>
    /// <param name="bytes">The number of bytes allocated.</param>
    public void Record(string category, long bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        _bytesByCategory.AddOrUpdate(category, bytes, (_, existing) => existing + bytes);
        _countByCategory.AddOrUpdate(category, 1, static (_, existing) => existing + 1);
    }

    /// <summary>Gets a snapshot of the current allocation profile.</summary>
    /// <returns>An <see cref="AllocationProfile"/> containing all category statistics.</returns>
    public AllocationProfile GetProfile()
    {
        var stats = new Dictionary<string, CategoryStats>(StringComparer.Ordinal);

        foreach (var kvp in _bytesByCategory)
        {
            var count = _countByCategory.TryGetValue(kvp.Key, out var c) ? c : 0;
            stats[kvp.Key] = new CategoryStats(kvp.Value, count);
        }

        return new AllocationProfile(stats);
    }

    /// <summary>Resets all tracked allocation data.</summary>
    public void Reset()
    {
        _bytesByCategory.Clear();
        _countByCategory.Clear();
    }

    /// <summary>Gets allocation statistics grouped by category.</summary>
    /// <returns>A read-only dictionary mapping category names to their statistics.</returns>
    public IReadOnlyDictionary<string, CategoryStats> GetByCategory()
    {
        var stats = new Dictionary<string, CategoryStats>(StringComparer.Ordinal);

        foreach (var kvp in _bytesByCategory)
        {
            var count = _countByCategory.TryGetValue(kvp.Key, out var c) ? c : 0;
            stats[kvp.Key] = new CategoryStats(kvp.Value, count);
        }

        return stats;
    }
}

/// <summary>
/// Immutable snapshot of allocation profiling data across all categories.
/// </summary>
public sealed class AllocationProfile
{
    private readonly IReadOnlyDictionary<string, CategoryStats> _categories;

    /// <summary>Initializes an allocation profile.</summary>
    /// <param name="categories">The category statistics to include.</param>
    public AllocationProfile(IReadOnlyDictionary<string, CategoryStats> categories)
    {
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
    }

    /// <summary>Gets the statistics for all categories.</summary>
    public IReadOnlyDictionary<string, CategoryStats> Categories => _categories;

    /// <summary>Gets the total bytes across all categories.</summary>
    public long TotalBytes
    {
        get
        {
            long total = 0;
            foreach (var kvp in _categories)
                total += kvp.Value.Bytes;
            return total;
        }
    }

    /// <summary>Gets the total allocation count across all categories.</summary>
    public int TotalCount
    {
        get
        {
            var total = 0;
            foreach (var kvp in _categories)
                total += kvp.Value.Count;
            return total;
        }
    }
}
