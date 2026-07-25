using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.ConvexHull;

/// <summary>
/// Computes the convex hull of a set of 2D points using Andrew's monotone chain algorithm.
/// </summary>
public static class AndrewMonotoneChain
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the convex hull of a set of 2D points using Andrew's monotone chain algorithm.
    /// </summary>
    /// <param name="points">The input point set.</param>
    /// <returns>
    /// An immutable array of points forming the convex hull in counter-clockwise order.
    /// Returns an empty array if fewer than 3 non-collinear points exist.
    /// </returns>
    public static ImmutableArray<Point2D> Compute(ImmutableArray<Point2D> points)
    {
        if (points.Length <= 2)
            return ImmutableArray.CreateRange(points);

        Point2D[] pts = points.ToArray();
        System.Array.Sort(pts, (a, b) =>
        {
            int cmp = a.X.CompareTo(b.X);
            return cmp != 0 ? cmp : a.Y.CompareTo(b.Y);
        });

        var lower = new List<Point2D>();
        for (int i = 0; i < pts.Length; i++)
        {
            while (lower.Count >= 2)
            {
                int n = lower.Count;
                Point2D a = lower[n - 2];
                Point2D b = lower[n - 1];
                Point2D c = pts[i];
                double cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
                if (cross > Tolerance)
                    break;
                lower.RemoveAt(n - 1);
            }
            lower.Add(pts[i]);
        }

        var upper = new List<Point2D>();
        for (int i = pts.Length - 1; i >= 0; i--)
        {
            while (upper.Count >= 2)
            {
                int n = upper.Count;
                Point2D a = upper[n - 2];
                Point2D b = upper[n - 1];
                Point2D c = pts[i];
                double cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
                if (cross > Tolerance)
                    break;
                upper.RemoveAt(n - 1);
            }
            upper.Add(pts[i]);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);

        var result = ImmutableArray.CreateBuilder<Point2D>(lower.Count + upper.Count);
        for (int i = 0; i < lower.Count; i++)
            result.Add(lower[i]);
        for (int i = 0; i < upper.Count; i++)
            result.Add(upper[i]);

        return result.ToImmutable();
    }
}
