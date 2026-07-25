namespace MathVerse.Math.Geometry.Advanced.PolygonAlgorithms;

/// <summary>
/// Provides ear clipping triangulation for simple polygons.
/// Ear clipping is an O(n²) algorithm that works for any simple polygon
/// by repeatedly finding and removing "ear" triangles.
/// </summary>
public static class EarClipping
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Triangulates a simple polygon using the ear clipping algorithm.
    /// The polygon vertices should be in either clockwise or counter-clockwise order.
    /// Returns an array of triangle vertex indices, every three consecutive indices forming a triangle.
    /// </summary>
    /// <param name="polygon">The polygon vertices in winding order.</param>
    /// <returns>An immutable array of indices defining triangles (groups of 3).</returns>
    public static ImmutableArray<int> Triangulate(ImmutableArray<Point2D> polygon)
    {
        var builder = ImmutableArray.CreateBuilder<int>();
        int n = polygon.Length;
        if (n < 3) return builder.ToImmutable();

        var indices = new List<int>(n);
        for (int i = 0; i < n; i++) indices.Add(i);

        double signedArea = 0;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            signedArea += polygon[i].X * polygon[j].Y - polygon[j].X * polygon[i].Y;
        }
        double expectedSign = signedArea >= 0 ? 1.0 : -1.0;

        int remaining = n;
        int failSafe = n * 2;
        int idx = 0;

        while (remaining > 2 && failSafe-- > 0)
        {
            int i0 = indices[idx % remaining];
            int i1 = indices[(idx + 1) % remaining];
            int i2 = indices[(idx + 2) % remaining];

            if (IsConvex(polygon, indices, idx % remaining, remaining, expectedSign))
            {
                if (IsEar(polygon, indices, idx % remaining, remaining))
                {
                    builder.Add(i0);
                    builder.Add(i1);
                    builder.Add(i2);
                    indices.RemoveAt((idx + 1) % remaining);
                    remaining--;
                    idx = 0;
                    continue;
                }
            }

            idx++;
            if (idx >= remaining)
                idx = 0;
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Determines whether the vertex at the given position in the index list is an ear.
    /// An ear is a convex vertex where no other polygon vertex lies inside the triangle formed by it and its neighbors.
    /// </summary>
    /// <param name="polygon">The polygon vertices.</param>
    /// <param name="indices">The current list of vertex indices.</param>
    /// <param name="localIndex">The local index in the indices list to test.</param>
    /// <param name="remaining">The number of remaining vertices.</param>
    /// <returns><c>true</c> if the vertex is an ear; otherwise, <c>false</c>.</returns>
    internal static bool IsEar(ImmutableArray<Point2D> polygon, List<int> indices, int localIndex, int remaining)
    {
        int prevIdx = indices[(localIndex + remaining - 1) % remaining];
        int currIdx = indices[localIndex];
        int nextIdx = indices[(localIndex + 1) % remaining];

        Point2D a = polygon[prevIdx];
        Point2D b = polygon[currIdx];
        Point2D c = polygon[nextIdx];

        for (int k = 0; k < remaining; k++)
        {
            int ki = indices[k];
            if (ki == prevIdx || ki == currIdx || ki == nextIdx) continue;
            Point2D pt = polygon[ki];
            if (PointInTriangle(pt, a, b, c))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the vertex at the given position forms a convex corner in the polygon.
    /// </summary>
    /// <param name="polygon">The polygon vertices.</param>
    /// <param name="indices">The current list of vertex indices.</param>
    /// <param name="localIndex">The local index in the indices list to test.</param>
    /// <param name="remaining">The number of remaining vertices.</param>
    /// <param name="expectedSign">The sign of the polygon's winding order (+1 for CCW, -1 for CW).</param>
    /// <returns><c>true</c> if the vertex is convex; otherwise, <c>false</c>.</returns>
    internal static bool IsConvex(ImmutableArray<Point2D> polygon, List<int> indices, int localIndex, int remaining, double expectedSign = 1.0)
    {
        int prevIdx = indices[(localIndex + remaining - 1) % remaining];
        int currIdx = indices[localIndex];
        int nextIdx = indices[(localIndex + 1) % remaining];

        Point2D a = polygon[prevIdx];
        Point2D b = polygon[currIdx];
        Point2D c = polygon[nextIdx];

        double cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
        return cross * expectedSign > Tolerance;
    }

    private static bool PointInTriangle(Point2D pt, Point2D a, Point2D b, Point2D c)
    {
        double d1 = Cross(b, a, pt);
        double d2 = Cross(c, b, pt);
        double d3 = Cross(a, c, pt);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    private static double Cross(Point2D a, Point2D b, Point2D c)
    {
        return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    }
}
