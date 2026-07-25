using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Delaunay;

/// <summary>
/// Represents a triangle in a Delaunay triangulation, specified by three vertex indices.
/// </summary>
public readonly record struct DelaunayTriangle(int V0, int V1, int V2);

/// <summary>
/// Represents an edge in a Delaunay triangulation, specified by two vertex indices.
/// </summary>
public readonly record struct DelaunayEdge(int V0, int V1);

/// <summary>
/// Computes the Delaunay triangulation of a set of 2D points using the Bowyer-Watson algorithm.
/// </summary>
public static class BowyerWatson
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the Delaunay triangulation of a set of 2D points using the Bowyer-Watson algorithm.
    /// </summary>
    /// <param name="points">The input point set.</param>
    /// <returns>
    /// A tuple containing all vertices (input points plus super-triangle vertices removed)
    /// and the list of Delaunay triangles as vertex index triples.
    /// </returns>
    public static (ImmutableArray<Point2D> Vertices, ImmutableArray<DelaunayTriangle> Triangulation) Triangulate(ImmutableArray<Point2D> points)
    {
        if (points.Length < 3)
        {
            return (points, ImmutableArray<DelaunayTriangle>.Empty);
        }

        ComputeSuperTriangle(points, out Point2D st0, out Point2D st1, out Point2D st2);

        int s0 = points.Length;
        int s1 = points.Length + 1;
        int s2 = points.Length + 2;

        var allPoints = ImmutableArray.CreateBuilder<Point2D>(points.Length + 3);
        for (int i = 0; i < points.Length; i++)
            allPoints.Add(points[i]);
        allPoints.Add(st0);
        allPoints.Add(st1);
        allPoints.Add(st2);

        var triangles = new List<DelaunayTriangle>();
        triangles.Add(new DelaunayTriangle(s0, s1, s2));

        for (int i = 0; i < points.Length; i++)
        {
            var badTriangles = new List<int>();
            for (int t = 0; t < triangles.Count; t++)
            {
                DelaunayTriangle tri = triangles[t];
                if (PointInCircumcircle(points[i], allPoints[tri.V0], allPoints[tri.V1], allPoints[tri.V2]))
                    badTriangles.Add(t);
            }

            var polygon = new List<(int, int)>();
            for (int b = 0; b < badTriangles.Count; b++)
            {
                DelaunayTriangle tri = triangles[badTriangles[b]];
                int[][] edges = {
                    new[] { tri.V0, tri.V1 },
                    new[] { tri.V1, tri.V2 },
                    new[] { tri.V2, tri.V0 }
                };

                for (int e = 0; e < 3; e++)
                {
                    bool shared = false;
                    for (int b2 = 0; b2 < badTriangles.Count; b2++)
                    {
                        if (b == b2) continue;
                        DelaunayTriangle tri2 = triangles[badTriangles[b2]];
                        if (EdgeShared(edges[e][0], edges[e][1], tri2))
                        {
                            shared = true;
                            break;
                        }
                    }
                    if (!shared)
                        polygon.Add((edges[e][0], edges[e][1]));
                }
            }

            for (int b = badTriangles.Count - 1; b >= 0; b--)
                triangles.RemoveAt(badTriangles[b]);

            for (int p = 0; p < polygon.Count; p++)
            {
                (int a, int bb) = polygon[p];
                triangles.Add(new DelaunayTriangle(a, bb, i));
            }
        }

        var superVertexSet = new HashSet<int> { s0, s1, s2 };
        var finalTriangles = ImmutableArray.CreateBuilder<DelaunayTriangle>();
        for (int i = 0; i < triangles.Count; i++)
        {
            DelaunayTriangle tri = triangles[i];
            if (!superVertexSet.Contains(tri.V0) && !superVertexSet.Contains(tri.V1) && !superVertexSet.Contains(tri.V2))
                finalTriangles.Add(tri);
        }

        return (allPoints.ToImmutable(), finalTriangles.ToImmutable());
    }

    internal static bool PointInCircumcircle(Point2D p, Point2D a, Point2D b, Point2D c)
    {
        double ax = a.X - p.X;
        double ay = a.Y - p.Y;
        double bx = b.X - p.X;
        double by = b.Y - p.Y;
        double cx = c.X - p.X;
        double cy = c.Y - p.Y;

        double det = (ax * ax + ay * ay) * (bx * cy - cx * by)
                    - (bx * bx + by * by) * (ax * cy - cx * ay)
                    + (cx * cx + cy * cy) * (ax * by - bx * ay);

        return det > Tolerance;
    }

    private static void ComputeSuperTriangle(ImmutableArray<Point2D> points, out Point2D st0, out Point2D st1, out Point2D st2)
    {
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].X < minX) minX = points[i].X;
            if (points[i].Y < minY) minY = points[i].Y;
            if (points[i].X > maxX) maxX = points[i].X;
            if (points[i].Y > maxY) maxY = points[i].Y;
        }

        double dx = maxX - minX;
        double dy = maxY - minY;
        double margin = System.Math.Max(dx, dy) * 2;

        st0 = new Point2D(minX - margin * 0.5, minY - margin);
        st1 = new Point2D(minX + dx * 0.5, maxY + margin);
        st2 = new Point2D(maxX + margin * 0.5, minY - margin);
    }

    private static bool EdgeShared(int v0, int v1, DelaunayTriangle tri)
    {
        return (tri.V0 == v0 && tri.V1 == v1) || (tri.V1 == v0 && tri.V0 == v1) ||
               (tri.V1 == v0 && tri.V2 == v1) || (tri.V2 == v0 && tri.V1 == v1) ||
               (tri.V2 == v0 && tri.V0 == v1) || (tri.V0 == v0 && tri.V2 == v1);
    }
}
