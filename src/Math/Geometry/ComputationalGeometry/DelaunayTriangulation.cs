using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.ComputationalGeometry;

/// <summary>Computes a Delaunay triangulation of a set of 2D points using Bowyer-Watson.</summary>
public static class DelaunayTriangulation
{
    /// <summary>Computes the Delaunay triangulation.</summary>
    /// <param name="points">The input points.</param>
    /// <returns>An immutable array of triangles forming the triangulation.</returns>
    public static ImmutableArray<Triangle2D> Triangulate(IReadOnlyList<Point2D> points)
    {
        if (points.Count < 3) return ImmutableArray<Triangle2D>.Empty;

        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].X < minX) minX = points[i].X;
            if (points[i].X > maxX) maxX = points[i].X;
            if (points[i].Y < minY) minY = points[i].Y;
            if (points[i].Y > maxY) maxY = points[i].Y;
        }

        double dx = maxX - minX, dy = maxY - minY;
        double dmax = System.Math.Max(dx, dy);
        double midx = (minX + maxX) * 0.5, midy = (minY + maxY) * 0.5;

        Point2D p1 = new(midx - 20 * dmax, midy - dmax);
        Point2D p2 = new(midx, midy + 20 * dmax);
        Point2D p3 = new(midx + 20 * dmax, midy - dmax);

        List<Triangle2D> triangles = new() { new Triangle2D(p1, p2, p3) };

        for (int i = 0; i < points.Count; i++)
        {
            Point2D p = points[i];
            List<Segment2D> polygon = new();

            List<Triangle2D> badTriangles = new();
            for (int j = triangles.Count - 1; j >= 0; j--)
            {
                if (InCircumcircle(p, triangles[j]))
                {
                    Triangle2D t = triangles[j];
                    AddEdge(polygon, new Segment2D(t.A, t.B));
                    AddEdge(polygon, new Segment2D(t.B, t.C));
                    AddEdge(polygon, new Segment2D(t.C, t.A));
                    triangles.RemoveAt(j);
                }
            }

            for (int j = 0; j < polygon.Count; j++)
            {
                triangles.Add(new Triangle2D(polygon[j].P1, polygon[j].P2, p));
            }
        }

        for (int i = triangles.Count - 1; i >= 0; i--)
        {
            Triangle2D t = triangles[i];
            if (SharesVertex(t, p1) || SharesVertex(t, p2) || SharesVertex(t, p3))
                triangles.RemoveAt(i);
        }

        return triangles.ToImmutableArray();
    }

    /// <summary>Gets the edges of the Delaunay triangulation.</summary>
    public static ImmutableArray<Segment2D> GetEdges(IReadOnlyList<Point2D> points)
    {
        ImmutableArray<Triangle2D> tris = Triangulate(points);
        HashSet<(int, int)> seen = new();
        List<Segment2D> edges = new();
        for (int i = 0; i < tris.Length; i++)
        {
            AddUniqueEdge(edges, seen, tris[i].A, tris[i].B);
            AddUniqueEdge(edges, seen, tris[i].B, tris[i].C);
            AddUniqueEdge(edges, seen, tris[i].C, tris[i].A);
        }
        return edges.ToImmutableArray();
    }

    private static bool InCircumcircle(Point2D p, Triangle2D tri)
    {
        double ax = tri.A.X - p.X, ay = tri.A.Y - p.Y;
        double bx = tri.B.X - p.X, by = tri.B.Y - p.Y;
        double cx = tri.C.X - p.X, cy = tri.C.Y - p.Y;
        double det = (ax * ax + ay * ay) * (bx * cy - cx * by)
                    - (bx * bx + by * by) * (ax * cy - cx * ay)
                    + (cx * cx + cy * cy) * (ax * by - bx * ay);
        return det > 1e-10;
    }

    private static bool SharesVertex(Triangle2D t, Point2D p) =>
        PointsEqual(t.A, p) || PointsEqual(t.B, p) || PointsEqual(t.C, p);

    private static bool PointsEqual(Point2D a, Point2D b) =>
        System.Math.Abs(a.X - b.X) < 1e-10 && System.Math.Abs(a.Y - b.Y) < 1e-10;

    private static void AddEdge(List<Segment2D> polygon, Segment2D edge)
    {
        for (int i = polygon.Count - 1; i >= 0; i--)
        {
            if (SegmentsEqual(polygon[i], edge)) { polygon.RemoveAt(i); return; }
        }
        polygon.Add(edge);
    }

    private static bool SegmentsEqual(Segment2D a, Segment2D b) =>
        (PointsEqual(a.P1, b.P1) && PointsEqual(a.P2, b.P2)) ||
        (PointsEqual(a.P1, b.P2) && PointsEqual(a.P2, b.P1));

    private static void AddUniqueEdge(List<Segment2D> edges, HashSet<(int, int)> seen, Point2D a, Point2D b)
    {
        int hash1 = HashCode.Combine(System.Math.Round(a.X, 8), System.Math.Round(a.Y, 8));
        int hash2 = HashCode.Combine(System.Math.Round(b.X, 8), System.Math.Round(b.Y, 8));
        (int, int) key = hash1 < hash2 ? (hash1, hash2) : (hash2, hash1);
        if (seen.Add(key)) edges.Add(new Segment2D(a, b));
    }
}
