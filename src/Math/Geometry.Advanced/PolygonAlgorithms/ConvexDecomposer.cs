namespace MathVerse.Math.Geometry.Advanced.PolygonAlgorithms;

/// <summary>
/// Provides convex decomposition of simple polygons using the Hertel-Mehlhorn algorithm.
/// Decomposes a polygon into a minimal set of convex sub-polygons by first triangulating
/// and then greedily merging adjacent triangles while maintaining convexity.
/// </summary>
public static class ConvexDecomposer
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Decomposes a simple polygon into convex sub-polygons using the Hertel-Mehlhorn algorithm.
    /// First triangulates the polygon using ear clipping, then merges adjacent triangles
    /// greedily whenever the merged polygon remains convex.
    /// </summary>
    /// <param name="polygon">The polygon vertices in winding order.</param>
    /// <returns>An immutable array of convex sub-polygons.</returns>
    public static ImmutableArray<ImmutableArray<Point2D>> Decompose(ImmutableArray<Point2D> polygon)
    {
        int n = polygon.Length;
        if (n < 3) return ImmutableArray<ImmutableArray<Point2D>>.Empty;
        if (n == 3) return ImmutableArray.Create(ImmutableArray.Create(polygon[0], polygon[1], polygon[2]));

        var triangleIndices = EarClipping.Triangulate(polygon);
        int triCount = triangleIndices.Length / 3;
        if (triCount == 0) return ImmutableArray<ImmutableArray<Point2D>>.Empty;

        var trianglePolygons = new List<ImmutableArray<Point2D>>(triCount);
        for (int i = 0; i < triCount; i++)
        {
            int i0 = triangleIndices[i * 3];
            int i1 = triangleIndices[i * 3 + 1];
            int i2 = triangleIndices[i * 3 + 2];
            trianglePolygons.Add(ImmutableArray.Create(polygon[i0], polygon[i1], polygon[i2]));
        }

        var merged = new bool[triCount];
        var result = ImmutableArray.CreateBuilder<ImmutableArray<Point2D>>();

        for (int i = 0; i < triCount; i++)
        {
            if (merged[i]) continue;

            var current = trianglePolygons[i];
            bool changed = true;

            while (changed)
            {
                changed = false;
                for (int j = 0; j < triCount; j++)
                {
                    if (i == j || merged[j]) continue;

                    var candidate = trianglePolygons[j];
                    var mergedPoly = TryMergeTriangles(current, candidate);

                    if (mergedPoly.Length > 0 && IsConvexPolygon(mergedPoly))
                    {
                        current = mergedPoly;
                        merged[j] = true;
                        changed = true;
                    }
                }
            }

            result.Add(current);
        }

        return result.ToImmutable();
    }

    private static ImmutableArray<Point2D> TryMergeTriangles(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b)
    {
        var aPoints = new List<Point2D>(a);
        var bPoints = new List<Point2D>(b);

        int sharedA1 = -1, sharedA2 = -1;
        int sharedB1 = -1, sharedB2 = -1;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (PointsEqual(a[i], b[j]))
                {
                    if (sharedA1 < 0) { sharedA1 = i; sharedB1 = j; }
                    else { sharedA2 = i; sharedB2 = j; }
                }
            }
        }

        if (sharedA1 < 0 || sharedA2 < 0) return ImmutableArray<Point2D>.Empty;

        if (System.Math.Abs(sharedA1 - sharedA2) == 1 || System.Math.Abs(sharedA1 - sharedA2) == 2)
        {
        }

        var result = new List<Point2D>();

        for (int i = 0; i < 3; i++)
        {
            if (i != sharedA1 && i != sharedA2)
                result.Add(a[i]);
        }

        int bStart = (sharedB2 + 1) % 3;
        for (int count = 0; count < 3; count++)
        {
            int idx = (bStart + count) % 3;
            if (idx != sharedB1 && idx != sharedB2)
                result.Add(b[idx]);
        }

        return result.Count >= 3 ? ImmutableArray.Create(result.ToArray()) : ImmutableArray<Point2D>.Empty;
    }

    private static bool IsConvexPolygon(ImmutableArray<Point2D> polygon)
    {
        int n = polygon.Length;
        if (n < 3) return false;

        bool hasPositive = false, hasNegative = false;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            int k = (j + 1) % n;

            double cross = (polygon[j].X - polygon[i].X) * (polygon[k].Y - polygon[j].Y)
                         - (polygon[j].Y - polygon[i].Y) * (polygon[k].X - polygon[j].X);

            if (cross > Tolerance) hasPositive = true;
            if (cross < -Tolerance) hasNegative = true;
            if (hasPositive && hasNegative) return false;
        }
        return true;
    }

    private static bool PointsEqual(Point2D a, Point2D b)
    {
        return System.Math.Abs(a.X - b.X) < Tolerance && System.Math.Abs(a.Y - b.Y) < Tolerance;
    }
}
