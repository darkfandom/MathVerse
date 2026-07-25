using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Tessellation;

/// <summary>Provides static methods for subdividing curves by inserting interpolated points.</summary>
public static class CurveSubdivider
{
    /// <summary>Subdivides a 2D polyline by inserting midpoints between each pair of consecutive points.</summary>
    /// <param name="points">The input polyline vertices.</param>
    /// <param name="subdivisions">The number of subdivision iterations.</param>
    /// <returns>An immutable array of refined 2D points.</returns>
    public static ImmutableArray<Point2D> Subdivide(IReadOnlyList<Point2D> points, int subdivisions)
    {
        if (points.Count < 2 || subdivisions <= 0)
            return points.ToImmutableArray();

        List<Point2D> current = new(points);

        for (int iter = 0; iter < subdivisions; iter++)
        {
            List<Point2D> next = new(current.Count * 2 - 1);
            next.Add(current[0]);

            for (int i = 0; i < current.Count - 1; i++)
            {
                Point2D mid = current[i].Lerp(current[i + 1], 0.5);
                next.Add(mid);
                next.Add(current[i + 1]);
            }

            current = next;
        }

        return current.ToImmutableArray();
    }

    /// <summary>Subdivides a 3D polyline by inserting midpoints between each pair of consecutive points.</summary>
    /// <param name="points">The input polyline vertices.</param>
    /// <param name="subdivisions">The number of subdivision iterations.</param>
    /// <returns>An immutable array of refined 3D points.</returns>
    public static ImmutableArray<Point3D> Subdivide(IReadOnlyList<Point3D> points, int subdivisions)
    {
        if (points.Count < 2 || subdivisions <= 0)
            return points.ToImmutableArray();

        List<Point3D> current = new(points);

        for (int iter = 0; iter < subdivisions; iter++)
        {
            List<Point3D> next = new(current.Count * 2 - 1);
            next.Add(current[0]);

            for (int i = 0; i < current.Count - 1; i++)
            {
                Point3D mid = current[i].Lerp(current[i + 1], 0.5);
                next.Add(mid);
                next.Add(current[i + 1]);
            }

            current = next;
        }

        return current.ToImmutableArray();
    }

    /// <summary>Applies Chaikin's corner-cutting subdivision to a 2D polyline.</summary>
    /// <param name="points">The input polyline vertices.</param>
    /// <param name="iterations">The number of subdivision iterations.</param>
    /// <returns>An immutable array of refined 2D points.</returns>
    public static ImmutableArray<Point2D> ChaikinSubdivide(IReadOnlyList<Point2D> points, int iterations)
    {
        if (points.Count < 2 || iterations <= 0)
            return points.ToImmutableArray();

        List<Point2D> current = new(points);

        for (int iter = 0; iter < iterations; iter++)
        {
            List<Point2D> next = new(current.Count * 2);

            for (int i = 0; i < current.Count - 1; i++)
            {
                next.Add(current[i].Lerp(current[i + 1], 0.25));
                next.Add(current[i].Lerp(current[i + 1], 0.75));
            }

            current = next;
        }

        return current.ToImmutableArray();
    }
}
