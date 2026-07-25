using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Curves;

/// <summary>
/// Provides adaptive subdivision algorithms for polyline curves in both 2D and 3D, inserting additional points
/// where the midpoint deviation from the chord exceeds a specified error tolerance.
/// </summary>
public static class AdaptiveSubdivider
{
    /// <summary>
    /// Adaptively subdivides a 3D polyline curve by recursively inserting midpoints where the deviation from
    /// the chord exceeds the specified error tolerance. This produces a refined polyline that better approximates
    /// the underlying smooth curve.
    /// </summary>
    /// <param name="curve">The 3D polyline curve to subdivide.</param>
    /// <param name="maxError">The maximum allowed perpendicular distance between a midpoint and its chord. Must be positive.</param>
    /// <param name="maxDepth">The maximum recursion depth to prevent unbounded subdivision. Default is 10.</param>
    /// <returns>An immutable array of <see cref="Point3D"/> representing the adaptively subdivided curve.</returns>
    /// <exception cref="ArgumentException">Thrown when the curve has fewer than 2 points or maxError is not positive.</exception>
    public static ImmutableArray<Point3D> Subdivide(ImmutableArray<Point3D> curve, double maxError, int maxDepth = 10)
    {
        if (curve.Length < 2)
            throw new ArgumentException("Curve must have at least 2 points.", nameof(curve));
        if (maxError <= 0.0)
            throw new ArgumentException("maxError must be positive.", nameof(maxError));
        if (maxDepth < 0)
            throw new ArgumentException("maxDepth must be non-negative.", nameof(maxDepth));

        var result = ImmutableArray.CreateBuilder<Point3D>();
        result.Add(curve[0]);

        for (int i = 0; i < curve.Length - 1; i++)
        {
            SubdivideSegment3D(curve[i], curve[i + 1], maxError, maxDepth, 0, result);
        }

        return result.MoveToImmutable();
    }

    /// <summary>
    /// Adaptively subdivides a 2D polyline curve by recursively inserting midpoints where the deviation from
    /// the chord exceeds the specified error tolerance.
    /// </summary>
    /// <param name="curve">The 2D polyline curve to subdivide.</param>
    /// <param name="maxError">The maximum allowed perpendicular distance between a midpoint and its chord. Must be positive.</param>
    /// <param name="maxDepth">The maximum recursion depth to prevent unbounded subdivision. Default is 10.</param>
    /// <returns>An immutable array of <see cref="Point2D"/> representing the adaptively subdivided curve.</returns>
    /// <exception cref="ArgumentException">Thrown when the curve has fewer than 2 points or maxError is not positive.</exception>
    public static ImmutableArray<Point2D> Subdivide2D(ImmutableArray<Point2D> curve, double maxError, int maxDepth = 10)
    {
        if (curve.Length < 2)
            throw new ArgumentException("Curve must have at least 2 points.", nameof(curve));
        if (maxError <= 0.0)
            throw new ArgumentException("maxError must be positive.", nameof(maxError));
        if (maxDepth < 0)
            throw new ArgumentException("maxDepth must be non-negative.", nameof(maxDepth));

        var result = ImmutableArray.CreateBuilder<Point2D>();
        result.Add(curve[0]);

        for (int i = 0; i < curve.Length - 1; i++)
        {
            SubdivideSegment2D(curve[i], curve[i + 1], maxError, maxDepth, 0, result);
        }

        return result.MoveToImmutable();
    }

    /// <summary>
    /// Recursively subdivides a 3D segment if the midpoint deviation exceeds the tolerance.
    /// </summary>
    private static void SubdivideSegment3D(
        Point3D a, Point3D b, double maxError, int maxDepth, int depth,
        ImmutableArray<Point3D>.Builder result)
    {
        if (depth >= maxDepth)
        {
            result.Add(b);
            return;
        }

        Point3D mid = Midpoint(a, b);
        double deviation = PointToSegmentDistance3D(mid, a, b);

        if (deviation <= maxError)
        {
            result.Add(b);
            return;
        }

        SubdivideSegment3D(a, mid, maxError, maxDepth, depth + 1, result);
        SubdivideSegment3D(mid, b, maxError, maxDepth, depth + 1, result);
    }

    /// <summary>
    /// Recursively subdivides a 2D segment if the midpoint deviation exceeds the tolerance.
    /// </summary>
    private static void SubdivideSegment2D(
        Point2D a, Point2D b, double maxError, int maxDepth, int depth,
        ImmutableArray<Point2D>.Builder result)
    {
        if (depth >= maxDepth)
        {
            result.Add(b);
            return;
        }

        Point2D mid = Midpoint2D(a, b);
        double deviation = PointToSegmentDistance2D(mid, a, b);

        if (deviation <= maxError)
        {
            result.Add(b);
            return;
        }

        SubdivideSegment2D(a, mid, maxError, maxDepth, depth + 1, result);
        SubdivideSegment2D(mid, b, maxError, maxDepth, depth + 1, result);
    }

    /// <summary>
    /// Computes the perpendicular distance from a 3D point to the line segment defined by two endpoints.
    /// </summary>
    private static double PointToSegmentDistance3D(Point3D p, Point3D a, Point3D b)
    {
        double abx = b.X - a.X;
        double aby = b.Y - a.Y;
        double abz = b.Z - a.Z;
        double abLenSq = abx * abx + aby * aby + abz * abz;

        if (abLenSq < 1e-30)
        {
            double dx = p.X - a.X;
            double dy = p.Y - a.Y;
            double dz = p.Z - a.Z;
            return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        double apx = p.X - a.X;
        double apy = p.Y - a.Y;
        double apz = p.Z - a.Z;
        double t = Clamp01((apx * abx + apy * aby + apz * abz) / abLenSq);

        double projX = a.X + t * abx;
        double projY = a.Y + t * aby;
        double projZ = a.Z + t * abz;

        double dx2 = p.X - projX;
        double dy2 = p.Y - projY;
        double dz2 = p.Z - projZ;
        return System.Math.Sqrt(dx2 * dx2 + dy2 * dy2 + dz2 * dz2);
    }

    /// <summary>
    /// Computes the perpendicular distance from a 2D point to the line segment defined by two endpoints.
    /// </summary>
    private static double PointToSegmentDistance2D(Point2D p, Point2D a, Point2D b)
    {
        double abx = b.X - a.X;
        double aby = b.Y - a.Y;
        double abLenSq = abx * abx + aby * aby;

        if (abLenSq < 1e-30)
        {
            double dx = p.X - a.X;
            double dy = p.Y - a.Y;
            return System.Math.Sqrt(dx * dx + dy * dy);
        }

        double apx = p.X - a.X;
        double apy = p.Y - a.Y;
        double t = Clamp01((apx * abx + apy * aby) / abLenSq);

        double projX = a.X + t * abx;
        double projY = a.Y + t * aby;

        double dx2 = p.X - projX;
        double dy2 = p.Y - projY;
        return System.Math.Sqrt(dx2 * dx2 + dy2 * dy2);
    }

    /// <summary>
    /// Computes the midpoint of two 3D points.
    /// </summary>
    private static Point3D Midpoint(Point3D a, Point3D b)
    {
        return new Point3D(
            (a.X + b.X) * 0.5,
            (a.Y + b.Y) * 0.5,
            (a.Z + b.Z) * 0.5);
    }

    /// <summary>
    /// Computes the midpoint of two 2D points.
    /// </summary>
    private static Point2D Midpoint2D(Point2D a, Point2D b)
    {
        return new Point2D(
            (a.X + b.X) * 0.5,
            (a.Y + b.Y) * 0.5);
    }

    /// <summary>
    /// Clamps a value to the range [0, 1].
    /// </summary>
    private static double Clamp01(double value)
    {
        if (value < 0.0) return 0.0;
        if (value > 1.0) return 1.0;
        return value;
    }
}
