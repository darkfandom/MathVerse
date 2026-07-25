using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Delaunay;

/// <summary>
/// Computes the Delaunay triangulation of a set of 2D points using incremental insertion with edge flipping.
/// </summary>
public static class IncrementalDelaunay
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the Delaunay triangulation of a set of 2D points using incremental insertion with edge flipping.
    /// </summary>
    /// <param name="points">The input point set.</param>
    /// <returns>
    /// A tuple containing all vertices and the list of Delaunay triangles as vertex index triples.
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
                Point2D circumcenter = ComputeCircumcenter(allPoints[tri.V0], allPoints[tri.V1], allPoints[tri.V2], out double radius);
                double dist = points[i].DistanceTo(circumcenter);
                if (dist < radius + Tolerance)
                    badTriangles.Add(t);
            }

            var boundaryEdges = new List<(int, int)>();
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
                        boundaryEdges.Add((edges[e][0], edges[e][1]));
                }
            }

            for (int b = badTriangles.Count - 1; b >= 0; b--)
                triangles.RemoveAt(badTriangles[b]);

            for (int e = 0; e < boundaryEdges.Count; e++)
            {
                (int a, int bb) = boundaryEdges[e];
                triangles.Add(new DelaunayTriangle(a, bb, i));
            }

            FlipEdges(triangles, allPoints.ToImmutable());
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

    /// <summary>
    /// Performs edge flipping to restore the Delaunay property after inserting a point.
    /// </summary>
    /// <param name="triangles">The mutable list of triangles to modify.</param>
    /// <param name="vertices">The immutable vertex array for circumcircle tests.</param>
    public static void FlipEdges(List<DelaunayTriangle> triangles, ImmutableArray<Point2D> vertices)
    {
        var worklist = new Queue<(int, int)>();
        var inWorklist = new HashSet<(int, int)>();
        int maxFlips = triangles.Count * 4;
        int flipCount = 0;

        for (int i = 0; i < triangles.Count; i++)
        {
            for (int j = i + 1; j < triangles.Count; j++)
            {
                if (FindOppositeEdge(triangles[i], triangles[j], out _, out _, out _, out _))
                {
                    worklist.Enqueue((i, j));
                    inWorklist.Add((i, j));
                }
            }
        }

        while (worklist.Count > 0 && flipCount < maxFlips)
        {
            (int i, int j) = worklist.Dequeue();
            inWorklist.Remove((i, j));

            if (i >= triangles.Count || j >= triangles.Count)
                continue;

            DelaunayTriangle t1 = triangles[i];
            DelaunayTriangle t2 = triangles[j];

            if (!FindOppositeEdge(t1, t2, out int a, out int b, out int c, out int d))
                continue;

            if (BowyerWatson.PointInCircumcircle(vertices[c], vertices[t1.V0], vertices[t1.V1], vertices[t1.V2]))
            {
                if (BowyerWatson.PointInCircumcircle(vertices[d], vertices[t2.V0], vertices[t2.V1], vertices[t2.V2]))
                {
                    triangles[i] = new DelaunayTriangle(a, c, d);
                    triangles[j] = new DelaunayTriangle(b, c, d);
                    flipCount++;

                    for (int k = 0; k < triangles.Count; k++)
                    {
                        if (k == i || k == j) continue;
                        if (FindOppositeEdge(triangles[i], triangles[k], out _, out _, out _, out _))
                        {
                            var edge = i < k ? (i, k) : (k, i);
                            if (inWorklist.Add(edge))
                                worklist.Enqueue(edge);
                        }
                        if (FindOppositeEdge(triangles[j], triangles[k], out _, out _, out _, out _))
                        {
                            var edge = j < k ? (j, k) : (k, j);
                            if (inWorklist.Add(edge))
                                worklist.Enqueue(edge);
                        }
                    }
                }
            }
        }
    }

    private static bool FindOppositeEdge(DelaunayTriangle t1, DelaunayTriangle t2,
        out int a, out int b, out int c, out int d)
    {
        a = b = c = d = -1;
        int[] v1 = { t1.V0, t1.V1, t1.V2 };
        int[] v2 = { t2.V0, t2.V1, t2.V2 };

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (v1[i] == v2[(j + 1) % 3] && v1[(i + 1) % 3] == v2[j])
                {
                    c = v1[i];
                    d = v1[(i + 2) % 3];
                    a = v1[(i + 1) % 3];
                    b = v2[(j + 2) % 3];
                    return true;
                }
            }
        }
        return false;
    }

    private static Point2D ComputeCircumcenter(Point2D a, Point2D b, Point2D c, out double radius)
    {
        double d = 2.0 * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
        if (System.Math.Abs(d) < Tolerance)
        {
            radius = double.MaxValue;
            return new Point2D((a.X + b.X + c.X) / 3.0, (a.Y + b.Y + c.Y) / 3.0);
        }

        double aSq = a.X * a.X + a.Y * a.Y;
        double bSq = b.X * b.X + b.Y * b.Y;
        double cSq = c.X * c.X + c.Y * c.Y;

        double cx = (aSq * (b.Y - c.Y) + bSq * (c.Y - a.Y) + cSq * (a.Y - b.Y)) / d;
        double cy = (aSq * (c.X - b.X) + bSq * (a.X - c.X) + cSq * (b.X - a.X)) / d;
        Point2D center = new Point2D(cx, cy);
        radius = a.DistanceTo(center);
        return center;
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
