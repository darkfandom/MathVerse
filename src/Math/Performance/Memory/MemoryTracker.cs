namespace MathVerse.Math.Performance.Memory;

/// <summary>
/// Thread-safe tracker for memory allocations and deallocations using atomic counters.
/// </summary>
public sealed class MemoryTracker
{
    private long _currentBytes;
    private long _peakBytes;
    private long _totalAllocations;
    private int _objectReuseCount;

    /// <summary>Gets the current number of live allocated bytes.</summary>
    public long CurrentBytes => Interlocked.Read(ref _currentBytes);

    /// <summary>Gets the peak number of allocated bytes observed since construction or last reset.</summary>
    public long PeakBytes => Interlocked.Read(ref _peakBytes);

    /// <summary>Records an allocation of the specified number of bytes.</summary>
    /// <param name="bytes">The number of bytes allocated.</param>
    public void RecordAllocation(long bytes)
    {
        if (bytes <= 0)
            return;

        var newCurrent = Interlocked.Add(ref _currentBytes, bytes);
        Interlocked.Add(ref _totalAllocations, bytes);

        long currentPeak;
        do
        {
            currentPeak = Interlocked.Read(ref _peakBytes);
            if (newCurrent <= currentPeak)
                break;
        }
        while (Interlocked.CompareExchange(ref _peakBytes, newCurrent, currentPeak) != currentPeak);
    }

    /// <summary>Records a deallocation of the specified number of bytes.</summary>
    /// <param name="bytes">The number of bytes deallocated.</param>
    public void RecordDeallocation(long bytes)
    {
        if (bytes <= 0)
            return;

        Interlocked.Add(ref _currentBytes, -bytes);
    }

    /// <summary>Records an object reuse that avoided a fresh allocation.</summary>
    public void RecordReuse()
    {
        Interlocked.Increment(ref _objectReuseCount);
    }

    /// <summary>Gets a snapshot of the current memory statistics.</summary>
    /// <returns>A <see cref="MemoryStatistics"/> value with current counters.</returns>
    public MemoryStatistics GetStatistics() =>
        new()
        {
            CurrentAllocations = Interlocked.Read(ref _currentBytes),
            PeakAllocations = Interlocked.Read(ref _peakBytes),
            TotalAllocations = Interlocked.Read(ref _totalAllocations),
            ObjectReuseCount = Volatile.Read(ref _objectReuseCount),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2)
        };

    /// <summary>Resets all tracked counters to their initial state.</summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _currentBytes, 0);
        Interlocked.Exchange(ref _peakBytes, 0);
        Interlocked.Exchange(ref _totalAllocations, 0);
        Interlocked.Exchange(ref _objectReuseCount, 0);
    }
}
