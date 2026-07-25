namespace MathVerse.Math.Distributed.PerformanceMonitoring;

using System.Collections.Concurrent;

/// <summary>Tracks memory allocation sizes by tag using atomic operations.</summary>
public sealed class AllocationProfiler
{
    private readonly ConcurrentDictionary<string, long> _taggedAllocations = new();
    private long _totalAllocated;

    /// <summary>Gets the total bytes allocated across all tags.</summary>
    public long TotalAllocated => Interlocked.Read(ref _totalAllocated);

    /// <summary>Gets the number of distinct allocation tags being tracked.</summary>
    public int TagCount => _taggedAllocations.Count;

    /// <summary>Records an allocation of the specified size under the given tag.</summary>
    /// <param name="tag">The allocation category tag.</param>
    /// <param name="bytes">The number of bytes allocated.</param>
    public void RecordAllocation(string tag, long bytes)
    {
        Interlocked.Add(ref _totalAllocated, bytes);
        _taggedAllocations.AddOrUpdate(tag, bytes, (_, current) =>
        {
            Interlocked.Add(ref current, bytes);
            return current;
        });
    }

    /// <summary>Returns the total bytes allocated under the specified tag.</summary>
    /// <param name="tag">The allocation category tag.</param>
    /// <returns>The total bytes allocated, or 0 if the tag has no records.</returns>
    public long GetTotalAllocations(string tag)
    {
        if (_taggedAllocations.TryGetValue(tag, out long value))
        {
            return Interlocked.Read(ref value);
        }
        return 0;
    }

    /// <summary>Returns the total bytes allocated across all tags.</summary>
    /// <returns>The total bytes allocated.</returns>
    public long GetTotalAllocated()
    {
        return Interlocked.Read(ref _totalAllocated);
    }

    /// <summary>Returns all tracked tags and their total allocation sizes.</summary>
    /// <returns>A dictionary mapping tags to their total byte counts.</returns>
    public Dictionary<string, long> GetAllAllocations()
    {
        var result = new Dictionary<string, long>();
        foreach (var kvp in _taggedAllocations)
        {
            long val = kvp.Value;
            result[kvp.Key] = Interlocked.Read(ref val);
        }
        return result;
    }

    /// <summary>Returns the allocation size for the specified tag as a fraction of total allocations.</summary>
    /// <param name="tag">The allocation category tag.</param>
    /// <returns>The fraction of total allocations, or 0 if no allocations exist.</returns>
    public double GetAllocationRatio(string tag)
    {
        long total = Interlocked.Read(ref _totalAllocated);
        if (total == 0)
        {
            return 0.0;
        }

        long tagTotal = GetTotalAllocations(tag);
        return (double)tagTotal / (double)total;
    }

    /// <summary>Returns the top N tags by allocation size.</summary>
    /// <param name="count">The number of top tags to return.</param>
    /// <returns>An array of tag-allocation pairs ordered by size descending.</returns>
    public (string Tag, long Bytes)[] GetTopAllocations(int count)
    {
        return _taggedAllocations
            .Select(kvp => { long v = kvp.Value; return (Tag: kvp.Key, Bytes: Interlocked.Read(ref v)); })
            .OrderByDescending(a => a.Bytes)
            .Take(count)
            .ToArray();
    }

    /// <summary>Resets all allocation tracking data.</summary>
    public void Reset()
    {
        _taggedAllocations.Clear();
        Interlocked.Exchange(ref _totalAllocated, 0);
    }
}
