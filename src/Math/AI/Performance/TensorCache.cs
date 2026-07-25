namespace MathVerse.Math.AI.Performance;

using System.Collections.Concurrent;

/// <summary>Thread-safe cache for tensor (double array) allocations to reduce GC pressure through object pooling.</summary>
public sealed class TensorCache
{
    private readonly ConcurrentDictionary<int, ConcurrentQueue<double[]>> _pools = new();
    private int _rentedCount;
    private int _returnedCount;
    private int _totalAllocated;
    private const int MaxPoolSizePerBucket = 64;

    /// <summary>Gets the total number of arrays currently rented out.</summary>
    public int RentedCount => Volatile.Read(ref _rentedCount);

    /// <summary>Gets the total number of arrays returned to the pool.</summary>
    public int ReturnedCount => Volatile.Read(ref _returnedCount);

    /// <summary>Gets the total number of arrays allocated (not pooled).</summary>
    public int TotalAllocated => Volatile.Read(ref _totalAllocated);

    /// <summary>Rents a double array of the specified size from the pool, or allocates a new one if the pool is empty.</summary>
    /// <param name="size">The minimum required array length.</param>
    /// <returns>A double array of at least the specified size.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when size is zero or negative.</exception>
    public double[] Rent(int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");

        int bucketSize = GetBucketSize(size);
        var queue = _pools.GetOrAdd(bucketSize, _ => new ConcurrentQueue<double[]>());

        if (queue.TryDequeue(out var array) && array.Length >= size)
        {
            Interlocked.Increment(ref _rentedCount);
            return array;
        }

        Interlocked.Increment(ref _totalAllocated);
        Interlocked.Increment(ref _rentedCount);
        return new double[bucketSize];
    }

    /// <summary>Returns a rented array to the pool for future reuse.</summary>
    /// <param name="array">The array to return.</param>
    /// <exception cref="ArgumentNullException">Thrown when array is null.</exception>
    public void Return(double[] array)
    {
        ArgumentNullException.ThrowIfNull(array);

        ClearArray(array);

        int bucketSize = GetBucketSize(array.Length);
        var queue = _pools.GetOrAdd(bucketSize, _ => new ConcurrentQueue<double[]>());

        if (queue.Count < MaxPoolSizePerBucket)
        {
            queue.Enqueue(array);
        }

        Interlocked.Decrement(ref _rentedCount);
        Interlocked.Increment(ref _returnedCount);
    }

    /// <summary>Returns a rented array to the pool, optionally clearing its contents first.</summary>
    /// <param name="array">The array to return.</param>
    /// <param name="clearArray">Whether to zero the array contents before returning.</param>
    public void Return(double[] array, bool clearArray)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (clearArray)
        {
            ClearArray(array);
        }

        int bucketSize = GetBucketSize(array.Length);
        var queue = _pools.GetOrAdd(bucketSize, _ => new ConcurrentQueue<double[]>());

        if (queue.Count < MaxPoolSizePerBucket)
        {
            queue.Enqueue(array);
        }

        Interlocked.Decrement(ref _rentedCount);
        Interlocked.Increment(ref _returnedCount);
    }

    /// <summary>Clears all pooled arrays from all buckets.</summary>
    public void Clear()
    {
        foreach (var kvp in _pools)
        {
            while (kvp.Value.TryDequeue(out _)) { }
        }

        _pools.Clear();
    }

    /// <summary>Gets the number of idle arrays in the pool across all buckets.</summary>
    public int PooledCount
    {
        get
        {
            int count = 0;
            foreach (var kvp in _pools)
            {
                count += kvp.Value.Count;
            }
            return count;
        }
    }

    /// <summary>Maps a requested size to the next power-of-two bucket size.</summary>
    /// <param name="requestedSize">The requested array size.</param>
    /// <returns>The bucket size (power of 2, minimum 4).</returns>
    private static int GetBucketSize(int requestedSize)
    {
        if (requestedSize <= 4) return 4;
        if (requestedSize <= 8) return 8;
        if (requestedSize <= 16) return 16;
        if (requestedSize <= 32) return 32;
        if (requestedSize <= 64) return 64;
        if (requestedSize <= 128) return 128;
        if (requestedSize <= 256) return 256;
        if (requestedSize <= 512) return 512;
        if (requestedSize <= 1024) return 1024;
        if (requestedSize <= 2048) return 2048;
        if (requestedSize <= 4096) return 4096;

        int bucket = 1;
        while (bucket < requestedSize)
        {
            bucket *= 2;
        }
        return bucket;
    }

    /// <summary>Zeros all elements of an array.</summary>
    /// <param name="array">The array to clear.</param>
    private static void ClearArray(double[] array)
    {
        Array.Clear(array, 0, array.Length);
    }
}
