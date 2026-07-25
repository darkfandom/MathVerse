using System.Runtime.CompilerServices;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Performance;

/// <summary>
/// Provides optimized implementations of common 3D geometric operations
/// using scalar math. Operates on the custom Vector3D and Point3D types
/// from the Geometry project.
/// </summary>
public static class MathHelper
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the cross product of two 3D vectors.
    /// </summary>
    /// <param name="a">The first vector operand.</param>
    /// <param name="b">The second vector operand.</param>
    /// <returns>The cross product vector a x b.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D CrossProduct(Vector3D a, Vector3D b)
    {
        return new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);
    }

    /// <summary>
    /// Computes the dot product of two 3D vectors.
    /// </summary>
    /// <param name="a">The first vector operand.</param>
    /// <param name="b">The second vector operand.</param>
    /// <returns>The scalar dot product a . b.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DotProduct(Vector3D a, Vector3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    /// <summary>
    /// Normalizes a 3D vector to unit length.
    /// If the vector length is below the tolerance threshold, the zero vector is returned.
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
    /// Computes the Euclidean distance between two 3D points.
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
    /// Computes the subtraction vector from point b to point a (a - b).
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
