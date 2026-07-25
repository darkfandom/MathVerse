using System;
using System.Collections.Concurrent;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Performance;

/// <summary>
/// Provides thread-safe object pooling for frequently allocated geometry data structures
/// such as double arrays and Point3D arrays. Pooling reduces GC pressure in high-throughput
/// geometry processing scenarios by reusing previously allocated buffers.
/// </summary>
public static class GeometryPerformancePool
{
    private const int MinArraySize = 16;

    private static readonly ConcurrentBag<double[]> _doublePool = new();
    private static readonly ConcurrentBag<Point3D[]> _point3DPool = new();

    /// <summary>
    /// Rents a double-precision floating-point array from the pool with at least the specified capacity.
    /// If no suitable array is available in the pool, a new array is allocated. The returned array
    /// may contain stale data from previous use and must be treated as uninitialized by the caller.
    /// </summary>
    /// <param name="size">The minimum number of elements required in the array.</param>
    /// <returns>A double array with capacity at least equal to the specified size.</returns>
    public static double[] RentDoubleArray(int size)
    {
        int targetSize = System.Math.Max(size, MinArraySize);
        var discarded = new List<double[]>();

        while (_doublePool.TryTake(out double[]? candidate))
        {
            if (candidate.Length >= targetSize)
            {
                foreach (var d in discarded)
                    _doublePool.Add(d);
                return candidate;
            }
            discarded.Add(candidate);
        }

        foreach (var d in discarded)
            _doublePool.Add(d);

        return new double[targetSize];
    }

    /// <summary>
    /// Returns a previously rented double array to the pool for future reuse.
    /// The array is not cleared; callers should not assume the contents are zeroed.
    /// The array will only be returned to the pool if its length meets the minimum threshold.
    /// </summary>
    /// <param name="array">The double array to return to the pool.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array reference is null.</exception>
    public static void ReturnDoubleArray(double[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (array.Length >= MinArraySize)
            _doublePool.Add(array);
    }

    /// <summary>
    /// Rents a Point3D array from the pool with at least the specified capacity.
    /// If no suitable array is available in the pool, a new array is allocated.
    /// The returned array may contain stale data and must be treated as uninitialized.
    /// </summary>
    /// <param name="size">The minimum number of Point3D elements required.</param>
    /// <returns>A Point3D array with capacity at least equal to the specified size.</returns>
    public static Point3D[] RentPoint3DArray(int size)
    {
        int targetSize = System.Math.Max(size, MinArraySize);
        var discarded = new List<Point3D[]>();

        while (_point3DPool.TryTake(out Point3D[]? candidate))
        {
            if (candidate.Length >= targetSize)
            {
                foreach (var d in discarded)
                    _point3DPool.Add(d);
                return candidate;
            }
            discarded.Add(candidate);
        }

        foreach (var d in discarded)
            _point3DPool.Add(d);

        return new Point3D[targetSize];
    }

    /// <summary>
    /// Returns a previously rented Point3D array to the pool for future reuse.
    /// The array contents are not cleared. Arrays below the minimum size threshold
    /// are discarded for garbage collection instead of being pooled.
    /// </summary>
    /// <param name="array">The Point3D array to return to the pool.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array reference is null.</exception>
    public static void ReturnPoint3DArray(Point3D[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (array.Length >= MinArraySize)
            _point3DPool.Add(array);
    }

    /// <summary>
    /// Clears all pooled arrays from both the double and Point3D pools, releasing them
    /// for garbage collection. Call this method during shutdown or when memory pressure
    /// indicates that pooled buffers should be released.
    /// </summary>
    public static void ClearPools()
    {
        while (_doublePool.TryTake(out _)) { }
        while (_point3DPool.TryTake(out _)) { }
    }
}
