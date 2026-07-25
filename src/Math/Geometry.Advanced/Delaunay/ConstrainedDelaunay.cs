using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Delaunay;

/// <summary>
/// Computes a constrained Delaunay triangulation by first computing an unconstrained DT
/// then inserting constraint edges by splitting and flipping.
/// </summary>
public static class ConstrainedDelaunay
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes a constrained Delaunay triangulation of a set of points with required constraint segments.
    /// </summary>
    /// <param name="points">The input point set.</param>
    /// <param name="constraints">The constraint edges that must appear in the final triangulation.</param>
    /// <returns>
    /// A tuple containing all vertices and the list of constrained Delaunay triangles.
    /// </returns>
    public static (ImmutableArray<Point2D> Vertices, ImmutableArray<DelaunayTriangle> Triangulation) Triangulate(
        ImmutableArray<Point2D> points, ImmutableArray<Segment2D> constraints)
    {
        if (points.Length < 3)
        {
            return (points, ImmutableArray<DelaunayTriangle>.Empty);
        }

        var (allPoints, baseTriangles) = BowyerWatson.Triangulate(points);
        var triangles = new List<DelaunayTriangle>(baseTriangles);

        var constraintEdges = new HashSet<(int, int)>();
        var edgeMap = new Dictionary<(int, int), int>();
        for (int i = 0; i < allPoints.Length; i++)
            edgeMap[(i, i)] = i;

        for (int c = 0; c < constraints.Length; c++)
        {
            Segment2D seg = constraints[c];
            int idx1 = FindNearestPoint(allPoints, seg.P1);
            int idx2 = FindNearestPoint(allPoints, seg.P2);

            if (idx1 == idx2) continue;

            int min = System.Math.Min(idx1, idx2);
            int max = System.Math.Max(idx1, idx2);
            if (constraintEdges.Contains((min, max))) continue;

            constraintEdges.Add((min, max));

            InsertConstraintEdge(triangles, allPoints, idx1, idx2);

            RestoreDelaunay(triangles, allPoints);
        }

        var result = ImmutableArray.CreateBuilder<DelaunayTriangle>(triangles.Count);
        for (int i = 0; i < triangles.Count; i++)
            result.Add(triangles[i]);

        return (allPoints, result.ToImmutable());
    }

    private static void InsertConstraintEdge(List<DelaunayTriangle> triangles, ImmutableArray<Point2D> vertices, int v0, int v1)
    {
        int maxIterations = triangles.Count * 4;
        int iteration = 0;

        while (iteration < maxIterations)
        {
            iteration++;
            bool found = false;

            for (int i = 0; i < triangles.Count; i++)
            {
                DelaunayTriangle tri = triangles[i];
                Segment2D constraint = new Segment2D(vertices[v0], vertices[v1]);

                int[] verts = { tri.V0, tri.V1, tri.V2 };
                for (int e = 0; e < 3; e++)
                {
                    int ea = verts[e];
                    int eb = verts[(e + 1) % 3];
                    Segment2D edge = new Segment2D(vertices[ea], vertices[eb]);

                    if (ea == v0 || ea == v1 ||
                        eb == v0 || eb == v1)
                        continue;

                    if (edge.Intersect(constraint).hit)
                    {
                        int ec = verts[(e + 2) % 3];
                        for (int j = 0; j < triangles.Count; j++)
                        {
                            if (i == j) continue;
                            DelaunayTriangle tri2 = triangles[j];
                            if (EdgeShared(ea, eb, tri2))
                            {
                                int ed = FindOppositeVertex(tri2, ea, eb);

                                triangles[i] = new DelaunayTriangle(v0, ec, ed);
                                triangles[j] = new DelaunayTriangle(v1, ec, ed);

                                if (FindVertexIndex(triangles, ea, i) >= 0)
                                    InsertSplitTriangle(triangles, vertices, v0, ea, ec);
                                if (FindVertexIndex(triangles, eb, j) >= 0)
                                    InsertSplitTriangle(triangles, vertices, v1, eb, ec);

                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }
                }
                if (found) break;
            }

            if (!found) break;
        }
    }

    private static void InsertSplitTriangle(List<DelaunayTriangle> triangles, ImmutableArray<Point2D> vertices, int newV, int existingV, int thirdV)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            DelaunayTriangle tri = triangles[i];
            int[] verts = { tri.V0, tri.V1, tri.V2 };
            bool hasExisting = false;
            int existingIdx = -1;
            int other1 = -1, other2 = -1;

            for (int j = 0; j < 3; j++)
            {
                if (verts[j] == existingV)
                {
                    hasExisting = true;
                    existingIdx = j;
                }
                else
                {
                    if (other1 < 0) other1 = verts[j];
                    else other2 = verts[j];
                }
            }

            if (hasExisting && other1 >= 0 && other2 >= 0)
            {
                Segment2D edge = new Segment2D(vertices[newV], vertices[existingV]);
                Segment2D triEdge = new Segment2D(vertices[other1], vertices[other2]);

                if (edge.Intersect(triEdge).hit)
                {
                    triangles[i] = new DelaunayTriangle(newV, other1, other2);
                    return;
                }
            }
        }
    }

    private static void RestoreDelaunay(List<DelaunayTriangle> triangles, ImmutableArray<Point2D> vertices)
    {
        IncrementalDelaunay.FlipEdges(triangles, vertices);
    }

    private static int FindNearestPoint(ImmutableArray<Point2D> points, Point2D target)
    {
        int best = 0;
        double bestDist = double.MaxValue;
        for (int i = 0; i < points.Length; i++)
        {
            double dist = points[i].DistanceSquaredTo(target);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }
        return best;
    }

    private static bool EdgeShared(int v0, int v1, DelaunayTriangle tri)
    {
        int[] verts = { tri.V0, tri.V1, tri.V2 };
        for (int i = 0; i < 3; i++)
        {
            int a = verts[i];
            int b = verts[(i + 1) % 3];
            if ((a == v0 && b == v1) || (a == v1 && b == v0))
                return true;
        }
        return false;
    }

    private static int FindOppositeVertex(DelaunayTriangle tri, int v0, int v1)
    {
        if (tri.V0 != v0 && tri.V0 != v1) return tri.V0;
        if (tri.V1 != v0 && tri.V1 != v1) return tri.V1;
        return tri.V2;
    }

    private static int FindVertexIndex(List<DelaunayTriangle> triangles, int vertex, int triangleIdx)
    {
        if (triangleIdx < 0 || triangleIdx >= triangles.Count) return -1;
        DelaunayTriangle tri = triangles[triangleIdx];
        if (tri.V0 == vertex) return 0;
        if (tri.V1 == vertex) return 1;
        if (tri.V2 == vertex) return 2;
        return -1;
    }
}
