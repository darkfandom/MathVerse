using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Optimization;

/// <summary>
/// Describes the result of a proposed edge collapse operation.
/// </summary>
/// <param name="EdgeVertexLo">The index of the lower-indexed vertex of the edge to collapse.</param>
/// <param name="EdgeVertexHi">The index of the higher-indexed vertex of the edge to collapse.</param>
/// <param name="TargetPosition">The optimal target position for the merged vertex.</param>
/// <param name="Cost">The quadric error metric cost of this collapse.</param>
public readonly record struct CollapseInfo(int EdgeVertexLo, int EdgeVertexHi, Point3D TargetPosition, double Cost);

/// <summary>
/// Provides edge collapse operations for triangle mesh simplification using quadric error metrics.
/// </summary>
public static class EdgeCollapser
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Finds the edge with the minimum quadric error cost for collapse.
    /// Evaluates every unique edge in the mesh and returns the one that
    /// minimizes the sum of quadric error matrices at its endpoints.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>
    /// The best collapse candidate, or <c>null</c> if no valid edge exists.
    /// </returns>
    public static CollapseInfo? FindBestEdge(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        if (vertices.Length < 2 || indices.Length < 3)
            return null;

        int triCount = indices.Length / 3;
        var quadrics = new SymmetricMatrix[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            quadrics[i] = SymmetricMatrix.Zero;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
                continue;

            SymmetricMatrix q = ComputeFaceQuadric(vertices[i0], vertices[i1], vertices[i2]);
            quadrics[i0] = quadrics[i0].Add(q);
            quadrics[i1] = quadrics[i1].Add(q);
            quadrics[i2] = quadrics[i2].Add(q);
        }

        var edgeSet = new HashSet<(int, int)>();
        var best = (CollapseInfo?)null;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
                continue;

            CheckEdge(i0, i1, vertices, quadrics, edgeSet, ref best);
            CheckEdge(i1, i2, vertices, quadrics, edgeSet, ref best);
            CheckEdge(i2, i0, vertices, quadrics, edgeSet, ref best);
        }

        return best;
    }

    /// <summary>
    /// Performs a single edge collapse operation by merging two vertices into a target position.
    /// Updates the index buffer to reference the surviving vertex and removes degenerate triangles.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="edgeVertex0">Index of the first vertex of the edge to collapse.</param>
    /// <param name="edgeVertex1">Index of the second vertex of the edge to collapse.</param>
    /// <param name="target">The target position for the merged vertex.</param>
    /// <returns>
    /// A tuple containing the updated vertices (with the target replacing edgeVertex0),
    /// updated index buffer with degenerate triangles removed, and whether the collapse succeeded.
    /// </returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices, bool Collapsed) Collapse(
        ImmutableArray<Point3D> vertices,
        ImmutableArray<int> indices,
        int edgeVertex0,
        int edgeVertex1,
        Point3D target)
    {
        if (edgeVertex0 < 0 || edgeVertex0 >= vertices.Length ||
            edgeVertex1 < 0 || edgeVertex1 >= vertices.Length)
            return (vertices, indices, false);

        var newVertices = ImmutableArray.CreateBuilder<Point3D>(vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
        {
            if (i == edgeVertex0)
                newVertices.Add(target);
            else if (i == edgeVertex1)
                newVertices.Add(target);
            else
                newVertices.Add(vertices[i]);
        }

        var newIndices = ImmutableArray.CreateBuilder<int>(indices.Length);
        int triCount = indices.Length / 3;
        bool collapsed = false;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
                continue;

            if (i0 == edgeVertex1) i0 = edgeVertex0;
            if (i1 == edgeVertex1) i1 = edgeVertex0;
            if (i2 == edgeVertex1) i2 = edgeVertex0;

            if (i0 == i1 || i1 == i2 || i0 == i2)
            {
                collapsed = true;
                continue;
            }

            newIndices.Add(i0);
            newIndices.Add(i1);
            newIndices.Add(i2);
            collapsed = true;
        }

        return (newVertices.ToImmutable(), newIndices.ToImmutable(), collapsed);
    }

    private static void CheckEdge(
        int v0, int v1,
        ImmutableArray<Point3D> vertices,
        SymmetricMatrix[] quadrics,
        HashSet<(int, int)> edgeSet,
        ref CollapseInfo? best)
    {
        int lo = v0 < v1 ? v0 : v1;
        int hi = v0 < v1 ? v1 : v0;
        if (!edgeSet.Add((lo, hi)))
            return;

        SymmetricMatrix combined = quadrics[v0].Add(quadrics[v1]);
        Point3D target = ComputeOptimalTarget(combined, vertices[v0], vertices[v1]);
        double cost = EvaluateQuadric(combined, target);

        if (best == null || cost < best.Value.Cost)
            best = new CollapseInfo(lo, hi, target, cost);
    }

    private static Point3D ComputeOptimalTarget(SymmetricMatrix q, Point3D v0, Point3D v1)
    {
        double det = q.A11 * (q.A22 * q.A33 - q.A23 * q.A23)
                   - q.A12 * (q.A12 * q.A33 - q.A23 * q.A13)
                   + q.A13 * (q.A12 * q.A23 - q.A22 * q.A13);

        if (System.Math.Abs(det) > Tolerance)
        {
            double invDet = 1.0 / det;
            double rx = -(q.A14 * (q.A22 * q.A33 - q.A23 * q.A23)
                        - q.A24 * (q.A12 * q.A33 - q.A13 * q.A23)
                        + q.A34 * (q.A12 * q.A23 - q.A13 * q.A22)) * invDet;
            double ry = -(q.A12 * (q.A34 * q.A13 - q.A14 * q.A33)
                        - q.A24 * (q.A11 * q.A33 - q.A13 * q.A13)
                        + q.A34 * (q.A11 * q.A23 - q.A14 * q.A12)) * invDet;
            double rz = -(q.A12 * (q.A24 * q.A13 - q.A14 * q.A23)
                        - q.A22 * (q.A11 * q.A34 - q.A14 * q.A13)
                        + q.A24 * (q.A11 * q.A23 - q.A12 * q.A13)) * invDet;

            return new Point3D(rx, ry, rz);
        }

        double mx = (v0.X + v1.X) * 0.5;
        double my = (v0.Y + v1.Y) * 0.5;
        double mz = (v0.Z + v1.Z) * 0.5;
        Point3D mid = new Point3D(mx, my, mz);
        double costMid = EvaluateQuadric(q, mid);
        double costV0 = EvaluateQuadric(q, v0);
        double costV1 = EvaluateQuadric(q, v1);

        if (costMid <= costV0 && costMid <= costV1)
            return mid;
        return costV0 <= costV1 ? v0 : v1;
    }

    private static double EvaluateQuadric(SymmetricMatrix q, Point3D p)
    {
        double x = p.X, y = p.Y, z = p.Z;
        return q.A11 * x * x + 2.0 * q.A12 * x * y + 2.0 * q.A13 * x * z
             + 2.0 * q.A14 * x
             + q.A22 * y * y + 2.0 * q.A23 * y * z
             + 2.0 * q.A24 * y
             + q.A33 * z * z + 2.0 * q.A34 * z
             + q.A44;
    }

    private static SymmetricMatrix ComputeFaceQuadric(Point3D p0, Point3D p1, Point3D p2)
    {
        double ax = p1.X - p0.X, ay = p1.Y - p0.Y, az = p1.Z - p0.Z;
        double bx = p2.X - p0.X, by = p2.Y - p0.Y, bz = p2.Z - p0.Z;

        double nx = ay * bz - az * by;
        double ny = az * bx - ax * bz;
        double nz = ax * by - ay * bx;
        double len = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);

        if (len < Tolerance)
            return SymmetricMatrix.Zero;

        double invLen = 1.0 / len;
        nx *= invLen;
        ny *= invLen;
        nz *= invLen;
        double d = -(nx * p0.X + ny * p0.Y + nz * p0.Z);

        return new SymmetricMatrix(
            nx * nx, nx * ny, nx * nz, nx * d,
            ny * ny, ny * nz, ny * d,
            nz * nz, nz * d,
            d * d);
    }

    private readonly record struct SymmetricMatrix(
        double A11, double A12, double A13, double A14,
        double A22, double A23, double A24,
        double A33, double A34,
        double A44)
    {
        public static readonly SymmetricMatrix Zero = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        public SymmetricMatrix Add(SymmetricMatrix o) =>
            new(A11 + o.A11, A12 + o.A12, A13 + o.A13, A14 + o.A14,
                A22 + o.A22, A23 + o.A23, A24 + o.A24,
                A33 + o.A33, A34 + o.A34,
                A44 + o.A44);
    }
}
