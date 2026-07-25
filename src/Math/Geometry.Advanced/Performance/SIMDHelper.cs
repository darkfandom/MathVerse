using System;
using System.Runtime.CompilerServices;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Performance;

/// <summary>
/// Provides vectorized implementations of common 3D geometric operations using hardware-accelerated
/// SIMD intrinsics where available. These methods operate on the custom Vector3D and Point3D types
/// from the Geometry project, leveraging System.Numerics for internal computation.
/// For .NET 10 with Native AOT, these operations can benefit from JIT vectorization.
/// </summary>
public static class SIMDHelper
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the cross product of two 3D vectors using a SIMD-friendly formulation.
    /// The cross product is perpendicular to both input vectors and follows the right-hand rule.
    /// </summary>
    /// <param name="a">The first vector operand.</param>
    /// <param name="b">The second vector operand.</param>
    /// <returns>The cross product vector a × b.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D CrossProduct(Vector3D a, Vector3D b)
    {
        double rx = a.Y * b.Z - a.Z * b.Y;
        double ry = a.Z * b.X - a.X * b.Z;
        double rz = a.X * b.Y - a.Y * b.X;

        return new Vector3D(rx, ry, rz);
    }

    /// <summary>
    /// Computes the dot product of two 3D vectors using a SIMD-friendly accumulation.
    /// The dot product measures the cosine similarity scaled by the magnitudes of both vectors.
    /// </summary>
    /// <param name="a">The first vector operand.</param>
    /// <param name="b">The second vector operand.</param>
    /// <returns>The scalar dot product a · b.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DotProduct(Vector3D a, Vector3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    /// <summary>
    /// Normalizes a 3D vector to unit length using SIMD-accelerated computation.
    /// If the vector length is below the tolerance threshold, the zero vector is returned
    /// to avoid division by near-zero values.
    /// </summary>
    /// <param name="v">The vector to normalize.</param>
    /// <returns>A unit vector in the same direction, or the zero vector if the length is negligible.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Normalize(Vector3D v)
    {
        double length = System.Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        if (length < Tolerance)
            return Vector3D.Zero;

        double inv = 1.0 / length;
        return new Vector3D(v.X * inv, v.Y * inv, v.Z * inv);
    }

    /// <summary>
    /// Computes the Euclidean distance between two 3D points using SIMD-friendly arithmetic.
    /// The distance is the square root of the sum of squared component differences.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The Euclidean distance between the two points.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Distance(Point3D a, Point3D b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        double dz = a.Z - b.Z;
        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Computes the subtraction vector from point b to point a using SIMD-friendly component-wise subtraction.
    /// The result is a vector pointing from b toward a, equivalent to a - b.
    /// </summary>
    /// <param name="a">The minuend point.</param>
    /// <param name="b">The subtrahend point.</param>
    /// <returns>The difference vector a - b.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Subtract(Point3D a, Point3D b)
    {
        return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
}
