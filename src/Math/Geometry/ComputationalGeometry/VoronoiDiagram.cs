using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.ComputationalGeometry;

/// <summary>Computes a Voronoi diagram from a set of 2D points via dual of Delaunay triangulation.</summary>
public static class VoronoiDiagram
{
    /// <summary>Computes the Voronoi cell for each site point.</summary>
    /// <param name="sites">The site points.</param>
    /// <returns>An immutable array of polygons representing Voronoi cells.</returns>
    public static ImmutableArray<Polygon2D> Compute(IReadOnlyList<Point2D> sites)
    {
        if (sites.Count < 3) return ImmutableArray<Polygon2D>.Empty;

        ImmutableArray<Triangle2D> triangulation = DelaunayTriangulation.Triangulate(sites);

        Dictionary<(int, int), List<int>> edgeTriangles = new();
        for (int i = 0; i < triangulation.Length; i++)
        {
            Triangle2D t = triangulation[i];
            AddEdgeDual(edgeTriangles, t.A, t.B, i, sites);
            AddEdgeDual(edgeTriangles, t.B, t.C, i, sites);
            AddEdgeDual(edgeTriangles, t.C, t.A, i, sites);
        }

        Point2D[] circumcenters = new Point2D[triangulation.Length];
        for (int i = 0; i < triangulation.Length; i++)
            circumcenters[i] = ComputeCircumcenter(triangulation[i]);

        List<Polygon2D> cells = new();
        double extent = ComputeExtent(sites) * 10;

        for (int s = 0; s < sites.Count; s++)
        {
            List<int> adjTris = new();
            foreach (var kvp in edgeTriangles)
            {
                bool hasSite = false;
                int triA = kvp.Value[0];
                Triangle2D ta = triangulation[triA];
                if (PointsEqual(ta.A, sites[s]) || PointsEqual(ta.B, sites[s]) || PointsEqual(ta.C, sites[s]))
                    hasSite = true;

                if (!hasSite && kvp.Value.Count > 1)
                {
                    int triB = kvp.Value[1];
                    Triangle2D tb = triangulation[triB];
                    if (PointsEqual(tb.A, sites[s]) || PointsEqual(tb.B, sites[s]) || PointsEqual(tb.C, sites[s]))
                        hasSite = true;
                }

                if (hasSite)
                    foreach (int idx in kvp.Value)
                        if (!adjTris.Contains(idx)) adjTris.Add(idx);
            }

            if (adjTris.Count < 3)
            {
                Point2D center = sites[s];
                cells.Add(new Polygon2D(ImmutableArray.Create(
                    new Point2D(center.X - extent, center.Y - extent),
                    new Point2D(center.X + extent, center.Y - extent),
                    new Point2D(center.X + extent, center.Y + extent),
                    new Point2D(center.X - extent, center.Y + extent))));
                continue;
            }

            List<Point2D> cellPoints = new();
            for (int i = 0; i < adjTris.Count; i++)
                cellPoints.Add(circumcenters[adjTris[i]]);

            Point2D sc = sites[s];
            cellPoints.Sort((a, b) =>
            {
                double aa = System.Math.Atan2(a.Y - sc.Y, a.X - sc.X);
                double ab = System.Math.Atan2(b.Y - sc.Y, b.X - sc.X);
                return aa.CompareTo(ab);
            });

            if (cellPoints.Count >= 3)
                cells.Add(new Polygon2D(cellPoints.ToImmutableArray()));
        }

        return cells.ToImmutableArray();
    }

    /// <summary>Computes circumcenters for all Delaunay triangles.</summary>
    public static ImmutableArray<Point2D> ComputeCircumcenters(IReadOnlyList<Point2D> sites)
    {
        ImmutableArray<Triangle2D> tris = DelaunayTriangulation.Triangulate(sites);
        var result = ImmutableArray.CreateBuilder<Point2D>(tris.Length);
        for (int i = 0; i < tris.Length; i++)
            result.Add(ComputeCircumcenter(tris[i]));
        return result.ToImmutable();
    }

    internal static Point2D ComputeCircumcenter(Triangle2D t)
    {
        double ax = t.A.X, ay = t.A.Y;
        double bx = t.B.X, by = t.B.Y;
        double cx = t.C.X, cy = t.C.Y;
        double d = 2.0 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
        if (System.Math.Abs(d) < 1e-30) return new Point2D((ax + bx + cx) / 3.0, (ay + by + cy) / 3.0);
        double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
        double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
        return new Point2D(ux, uy);
    }

    private static void AddEdgeDual(Dictionary<(int, int), List<int>> dict, Point2D a, Point2D b, int triIdx, IReadOnlyList<Point2D> sites)
    {
        int hashA = FindSiteIndex(a, sites);
        int hashB = FindSiteIndex(b, sites);
        (int, int) key = hashA < hashB ? (hashA, hashB) : (hashB, hashA);
        if (!dict.ContainsKey(key)) dict[key] = new List<int>();
        dict[key].Add(triIdx);
    }

    private static int FindSiteIndex(Point2D p, IReadOnlyList<Point2D> sites)
    {
        for (int i = 0; i < sites.Count; i++)
            if (PointsEqual(sites[i], p)) return i;
        return -1;
    }

    private static bool PointsEqual(Point2D a, Point2D b) =>
        System.Math.Abs(a.X - b.X) < 1e-10 && System.Math.Abs(a.Y - b.Y) < 1e-10;

    private static double ComputeExtent(IReadOnlyList<Point2D> sites)
    {
        double maxDist = 0;
        Point2D center = new(0, 0);
        for (int i = 0; i < sites.Count; i++)
        {
            center = new Point2D(center.X + sites[i].X, center.Y + sites[i].Y);
        }
        center = new Point2D(center.X / sites.Count, center.Y / sites.Count);
        for (int i = 0; i < sites.Count; i++)
        {
            double d = sites[i].DistanceTo(center);
            if (d > maxDist) maxDist = d;
        }
        return maxDist;
    }
}
