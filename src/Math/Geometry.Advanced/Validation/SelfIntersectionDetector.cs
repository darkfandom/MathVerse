using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Validation;

/// <summary>
/// Describes a self-intersection point in a 2D polygon.
/// </summary>
/// <param name="Location">The point where the two edges intersect.</param>
/// <param name="EdgeIndex1">The index of the first intersecting edge.</param>
/// <param name="EdgeIndex2">The index of the second intersecting edge.</param>
public readonly record struct SelfIntersection(Point2D Location, int EdgeIndex1, int EdgeIndex2);

/// <summary>
/// Detects self-intersections in 2D polygons using O(n^2) pairwise edge intersection tests.
/// </summary>
public static class SelfIntersectionDetector
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Finds all self-intersection points in a 2D polygon.
    /// Tests every pair of non-adjacent edges for intersection and computes
    /// the exact intersection point for each crossing found.
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <returns>An immutable array of all self-intersection points with edge information.</returns>
    public static ImmutableArray<SelfIntersection> FindPolygonSelfIntersections(ImmutableArray<Point2D> polygon)
    {
        var results = ImmutableArray.CreateBuilder<SelfIntersection>();
        int n = polygon.Length;
        if (n < 3) return results.ToImmutable();

        for (int i = 0; i < n; i++)
        {
            int iNext = (i + 1) % n;
            Point2D a1 = polygon[i];
            Point2D a2 = polygon[iNext];

            for (int j = i + 2; j < n; j++)
            {
                if (i == 0 && j == n - 1) continue;
                int jNext = (j + 1) % n;
                Point2D b1 = polygon[j];
                Point2D b2 = polygon[jNext];

                if (ComputeSegmentIntersection(a1, a2, b1, b2, out Point2D intersection))
                {
                    results.Add(new SelfIntersection(intersection, i, j));
                }
            }
        }

        return results.ToImmutable();
    }

    /// <summary>
    /// Tests whether a 2D polygon is simple (has no self-intersections).
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <returns><c>true</c> if the polygon is simple; otherwise, <c>false</c>.</returns>
    public static bool IsSimplePolygon(ImmutableArray<Point2D> polygon)
    {
        int n = polygon.Length;
        if (n < 3) return false;

        for (int i = 0; i < n; i++)
        {
            int iNext = (i + 1) % n;
            Point2D a1 = polygon[i];
            Point2D a2 = polygon[iNext];

            for (int j = i + 2; j < n; j++)
            {
                if (i == 0 && j == n - 1) continue;
                int jNext = (j + 1) % n;
                Point2D b1 = polygon[j];
                Point2D b2 = polygon[jNext];

                if (SegmentsStrictlyIntersect(a1, a2, b1, b2))
                    return false;
            }
        }

        return true;
    }

    private static bool ComputeSegmentIntersection(
        Point2D a1, Point2D a2, Point2D b1, Point2D b2, out Point2D intersection)
    {
        intersection = Point2D.Origin;

        double d1x = a2.X - a1.X, d1y = a2.Y - a1.Y;
        double d2x = b2.X - b1.X, d2y = b2.Y - b1.Y;
        double cross = d1x * d2y - d1y * d2x;

        if (System.Math.Abs(cross) < Tolerance)
            return false;

        double dx = b1.X - a1.X;
        double dy = b1.Y - a1.Y;
        double t = (dx * d2y - dy * d2x) / cross;
        double u = (dx * d1y - dy * d1x) / cross;

        if (t >= -Tolerance && t <= 1.0 + Tolerance && u >= -Tolerance && u <= 1.0 + Tolerance)
        {
            if (t > Tolerance && t < 1.0 - Tolerance && u > Tolerance && u < 1.0 - Tolerance)
            {
                intersection = new Point2D(a1.X + t * d1x, a1.Y + t * d1y);
                return true;
            }
        }

        return false;
    }

    private static bool SegmentsStrictlyIntersect(Point2D a1, Point2D a2, Point2D b1, Point2D b2)
    {
        double d1 = Cross2D(b1, b2, a1);
        double d2 = Cross2D(b1, b2, a2);
        double d3 = Cross2D(a1, a2, b1);
        double d4 = Cross2D(a1, a2, b2);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        return false;
    }

    private static double Cross2D(Point2D o, Point2D a, Point2D b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }
}
