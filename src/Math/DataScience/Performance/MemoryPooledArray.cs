namespace MathVerse.Math.DataScience.Performance;

using System;
using System.Buffers;
using System.Collections.Generic;

/// <summary>
/// Provides pooled array rental and return operations to minimize garbage collection pressure.
/// </summary>
/// <typeparam name="T">The element type of the pooled array.</typeparam>
public sealed class PooledArray<T> : IDisposable where T : struct
{
    private T[]? _array;
    private readonly int _requestedSize;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledArray{T}"/> class.
    /// </summary>
    /// <param name="array">The rented array.</param>
    /// <param name="requestedSize">The originally requested size.</param>
    internal PooledArray(T[] array, int requestedSize)
    {
        _array = array;
        _requestedSize = requestedSize;
    }

    /// <summary>
    /// Gets the underlying array. Do not hold references beyond the lifetime of this instance.
    /// </summary>
    public T[] Array => _array ?? throw new ObjectDisposedException(nameof(PooledArray<T>));

    /// <summary>
    /// Gets the requested size of the array.
    /// </summary>
    public int Length => _requestedSize;

    /// <summary>
    /// Gets a span over the valid portion of the array.
    /// </summary>
    public Span<T> Span => _array.AsSpan(0, _requestedSize);

    /// <summary>
    /// Returns the pooled array to the pool and marks this instance as disposed.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed && _array is not null)
        {
            ArrayPool<T>.Shared.Return(_array, clearArray: true);
            _array = null;
            _disposed = true;
        }
    }
}

/// <summary>
/// Manages pooled array rental and return operations for high-performance data processing.
/// Tracks total rented and returned arrays.
/// </summary>
public sealed class MemoryPooledArray
{
    private long _totalRented;
    private long _totalReturned;

    /// <summary>
    /// Gets the total number of arrays that have been rented.
    /// </summary>
    public long TotalRented => _totalRented;

    /// <summary>
    /// Gets the total number of arrays that have been returned.
    /// </summary>
    public long TotalReturned => _totalReturned;

    /// <summary>
    /// Gets the number of arrays currently outstanding (rented but not returned).
    /// </summary>
    public long Outstanding => _totalRented - _totalReturned;

    /// <summary>
    /// Rents a <see cref="PooledArray{T}"/> of the specified minimum size from the shared array pool.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="size">The minimum number of elements required.</param>
    /// <returns>A <see cref="PooledArray{T}"/> that wraps the rented array.</returns>
    public PooledArray<T> Rent<T>(int size) where T : struct
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be non-negative.");

        T[] array = ArrayPool<T>.Shared.Rent(size);
        Interlocked.Increment(ref _totalRented);

        return new PooledArray<T>(array, size);
    }

    /// <summary>
    /// Returns a previously rented <see cref="PooledArray{T}"/> to the pool.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="pooledArray">The pooled array to return.</param>
    public void Return<T>(PooledArray<T> pooledArray) where T : struct
    {
        if (pooledArray is null) throw new ArgumentNullException(nameof(pooledArray));

        pooledArray.Dispose();
        Interlocked.Increment(ref _totalReturned);
    }

    /// <summary>
    /// Gets the overall pool utilization as a ratio of outstanding to total rented.
    /// </summary>
    /// <returns>The utilization ratio (0-1).</returns>
    public double GetUtilization()
    {
        long rented = Interlocked.Read(ref _totalRented);
        if (rented == 0) return 0.0;
        long returned = Interlocked.Read(ref _totalReturned);
        return (double)(rented - returned) / rented;
    }
}
