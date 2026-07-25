using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.ConvexHull;

/// <summary>
/// Computes the convex hull of a set of 2D points using Chan's O(n log h) algorithm.
/// </summary>
public static class ChanAlgorithm
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the convex hull of a set of 2D points using Chan's algorithm.
    /// Combines Graham scan on subsets with Jarvis march for an O(n log h) result.
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

        int n = points.Length;

        for (int m = 1; m <= n; m *= 2)
        {
            int subsetSize = (n + m - 1) / m;
            var subsets = new List<ImmutableArray<Point2D>>();

            for (int i = 0; i < m; i++)
            {
                int start = i * subsetSize;
                int end = System.Math.Min(start + subsetSize, n);
                if (start >= n) break;

                var subset = ImmutableArray.CreateBuilder<Point2D>(end - start);
                for (int j = start; j < end; j++)
                    subset.Add(points[j]);

                subsets.Add(AndrewMonotoneChain.Compute(subset.ToImmutable()));
            }

            int leftIdx = FindLeftmost(points);
            var hull = new List<Point2D>();
            int p = leftIdx;
            int iterations = 0;

            do
            {
                hull.Add(points[p]);
                int q = (p + 1) % n;
                bool updated = true;

                while (updated)
                {
                    updated = false;
                    for (int s = 0; s < subsets.Count; s++)
                    {
                        ImmutableArray<Point2D> subset = subsets[s];
                        if (subset.Length == 0) continue;

                        int bestR = FindTangentRight(subset, points[p], points[q]);
                        double cross = Cross2D(points[p], points[q], subset[bestR]);
                        if (cross < -Tolerance || (System.Math.Abs(cross) < Tolerance &&
                            points[p].DistanceSquaredTo(subset[bestR]) > points[p].DistanceSquaredTo(points[q])))
                        {
                            int idx = PointIndex(points, subset[bestR]);
                            if (idx < 0)
                                break;
                            q = idx;
                            updated = true;
                        }
                    }
                }

                p = q;
                iterations++;
                if (iterations > n)
                    break;
            }
            while (p != leftIdx);

            if (iterations <= n)
                return ImmutableArray.CreateRange(hull);
        }

        return AndrewMonotoneChain.Compute(points);
    }

    private static int FindLeftmost(ImmutableArray<Point2D> points)
    {
        int idx = 0;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].X < points[idx].X ||
                (System.Math.Abs(points[i].X - points[idx].X) < Tolerance && points[i].Y < points[idx].Y))
                idx = i;
        }
        return idx;
    }

    private static int FindTangentRight(ImmutableArray<Point2D> convexHull, Point2D p, Point2D current)
    {
        if (convexHull.Length == 1)
        return -1;

        int n = convexHull.Length;

        for (int i = 0; i < n; i++)
        {
            int prev = (i - 1 + n) % n;
            int next = (i + 1) % n;
            double crossPrev = Cross2D(p, convexHull[i], convexHull[prev]);
            double crossNext = Cross2D(p, convexHull[i], convexHull[next]);
            if (crossPrev <= Tolerance && crossNext <= Tolerance)
                return i;
        }

        int best = 0;
        double bestAngle = double.MinValue;
        Vector2D refDir = new Vector2D(current.X - p.X, current.Y - p.Y).Normalize();

        for (int i = 0; i < n; i++)
        {
            Vector2D toPoint = new Vector2D(convexHull[i].X - p.X, convexHull[i].Y - p.Y);
            double angle = System.Math.Atan2(refDir.Cross(toPoint), refDir.Dot(toPoint));
            if (angle > bestAngle + Tolerance)
            {
                bestAngle = angle;
                best = i;
            }
        }

        return best;
    }

    private static int PointIndex(ImmutableArray<Point2D> points, Point2D target)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (System.Math.Abs(points[i].X - target.X) < Tolerance &&
                System.Math.Abs(points[i].Y - target.Y) < Tolerance)
                return i;
        }
        return 0;
    }

    private static double Cross2D(Point2D o, Point2D a, Point2D b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }
}
