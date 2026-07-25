using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>A 4x4 symmetric matrix used for quadric error metric computation in mesh simplification.</summary>
internal struct QuadricMatrix4x4
{
    /// <summary>The matrix elements stored in row-major order.</summary>
    public double M00, M01, M02, M03;
    public double M11, M12, M13;
    public double M22, M23;
    public double M33;

    /// <summary>Creates a quadric matrix from the coefficients of a plane equation ax + by + cz + d = 0.</summary>
    /// <param name="a">The x coefficient.</param>
    /// <param name="b">The y coefficient.</param>
    /// <param name="c">The z coefficient.</param>
    /// <param name="d">The constant term.</param>
    /// <returns>The quadric matrix for the given plane.</returns>
    public static QuadricMatrix4x4 FromPlane(double a, double b, double c, double d)
    {
        QuadricMatrix4x4 q = default;
        q.M00 = a * a; q.M01 = a * b; q.M02 = a * c; q.M03 = a * d;
        q.M11 = b * b; q.M12 = b * c; q.M13 = b * d;
        q.M22 = c * c; q.M23 = c * d;
        q.M33 = d * d;
        return q;
    }

    /// <summary>Computes the error of this quadric at a given point.</summary>
    /// <param name="p">The point to evaluate the error at.</param>
    /// <returns>The quadric error value.</returns>
    public double Evaluate(Point3D p)
    {
        double x = p.X, y = p.Y, z = p.Z;
        return M00 * x * x + 2 * M01 * x * y + 2 * M02 * x * z + 2 * M03 * x +
               M11 * y * y + 2 * M12 * y * z + 2 * M13 * y +
               M22 * z * z + 2 * M23 * z +
               M33;
    }

    /// <summary>Adds two quadric matrices together.</summary>
    /// <param name="a">The first quadric matrix.</param>
    /// <param name="b">The second quadric matrix.</param>
    /// <returns>The sum of the two quadric matrices.</returns>
    public static QuadricMatrix4x4 operator +(QuadricMatrix4x4 a, QuadricMatrix4x4 b)
    {
        QuadricMatrix4x4 r = default;
        r.M00 = a.M00 + b.M00; r.M01 = a.M01 + b.M01; r.M02 = a.M02 + b.M02; r.M03 = a.M03 + b.M03;
        r.M11 = a.M11 + b.M11; r.M12 = a.M12 + b.M12; r.M13 = a.M13 + b.M13;
        r.M22 = a.M22 + b.M22; r.M23 = a.M23 + b.M23;
        r.M33 = a.M33 + b.M33;
        return r;
    }
}

/// <summary>Provides mesh simplification using the Quadric Error Metric (QEM) edge collapse algorithm.</summary>
public static class MeshSimplifier
{
    private const double Tolerance = 1e-10;

    /// <summary>Simplifies a triangle mesh using quadric error metric edge collapse until the target triangle ratio is reached.</summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="targetRatio">The target ratio of remaining triangles to original triangles (0 to 1).</param>
    /// <returns>A tuple containing the simplified vertex positions and index buffer.</returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) Simplify(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, double targetRatio)
    {
        if (vertices.Length == 0 || indices.Length == 0)
            return (vertices, indices);

        int originalTriCount = indices.Length / 3;
        int targetTriCount = System.Math.Max(1, (int)(originalTriCount * targetRatio));
        if (targetTriCount >= originalTriCount)
            return (vertices, indices);

        var verts = new List<Point3D>(vertices);
        var tris = new List<(int I0, int I1, int I2)>();
        for (int i = 0; i < indices.Length; i += 3)
            tris.Add((indices[i], indices[i + 1], indices[i + 2]));

        var quadrics = new QuadricMatrix4x4[verts.Count];
        for (int t = 0; t < tris.Count; t++)
        {
            (int i0, int i1, int i2) = tris[t];
            Point3D p0 = verts[i0], p1 = verts[i1], p2 = verts[i2];
            QuadricMatrix4x4 q = ComputeTriangleQuadric(p0, p1, p2);
            quadrics[i0] = quadrics[i0] + q;
            quadrics[i1] = quadrics[i1] + q;
            quadrics[i2] = quadrics[i2] + q;
        }

        var edgeCosts = new SortedSet<(double Cost, int V0, int V1)>(Comparer<(double, int, int)>.Create((a, b) =>
        {
            int cmp = a.Item1.CompareTo(b.Item1);
            return cmp != 0 ? cmp : a.Item2.CompareTo(b.Item2);
        }));
        var edgePositions = new Dictionary<(int, int), Point3D>();
        var edgeSet = new HashSet<(int, int)>();
        bool[] alive = new bool[verts.Count];
        for (int i = 0; i < alive.Length; i++) alive[i] = true;

        for (int t = 0; t < tris.Count; t++)
        {
            (int i0, int i1, int i2) = tris[t];
            TryAddEdge(i0, i1, quadrics, verts, edgeCosts, edgePositions, edgeSet);
            TryAddEdge(i1, i2, quadrics, verts, edgeCosts, edgePositions, edgeSet);
            TryAddEdge(i2, i0, quadrics, verts, edgeCosts, edgePositions, edgeSet);
        }

        int currentTriCount = tris.Count;
        int safetyCounter = verts.Count * 10;
        while (currentTriCount > targetTriCount && edgeCosts.Count > 0 && safetyCounter-- > 0)
        {
            var cheapest = edgeCosts.Min;
            edgeCosts.Remove(cheapest);
            int v0 = cheapest.V0, v1 = cheapest.V1;
            if (!alive[v0] || !alive[v1]) continue;
            if (v0 == v1) continue;

            Point3D newPos = cheapest.Cost > Tolerance ? edgePositions[(v0, v1)] : verts[v0];
            quadrics[v0] = quadrics[v0] + quadrics[v1];
            verts[v0] = newPos;
            alive[v1] = false;

            for (int t = tris.Count - 1; t >= 0; t--)
            {
                (int t0, int t1, int t2) = tris[t];
                int[] triVerts = { t0, t1, t2 };
                bool hasV0 = false, hasV1 = false;
                for (int k = 0; k < 3; k++)
                {
                    if (triVerts[k] == v0) hasV0 = true;
                    if (triVerts[k] == v1) hasV1 = true;
                }
                if (hasV0 && hasV1)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        int b = (a + 1) % 3;
                        int e0 = System.Math.Min(triVerts[a], triVerts[b]);
                        int e1 = System.Math.Max(triVerts[a], triVerts[b]);
                        edgeSet.Remove((e0, e1));
                    }
                    tris.RemoveAt(t);
                    currentTriCount--;
                    continue;
                }
                for (int k = 0; k < 3; k++)
                {
                    if (triVerts[k] == v1)
                        triVerts[k] = v0;
                }
                tris[t] = (triVerts[0], triVerts[1], triVerts[2]);
            }

            for (int k = 0; k < 3; k++)
            {
                int vOther = tris.Count > 0 ? GetOtherVertex(tris, v0, k) : -1;
            }

            var affectedEdges = new HashSet<(int, int)>();
            for (int t = 0; t < tris.Count; t++)
            {
                (int t0, int t1, int t2) = tris[t];
                if (t0 == v0 || t1 == v0 || t2 == v0)
                {
                    int[] triVerts = { t0, t1, t2 };
                    for (int a = 0; a < 3; a++)
                    {
                        int b = (a + 1) % 3;
                        if (triVerts[a] != v1 && triVerts[b] != v1)
                        {
                            int e0 = System.Math.Min(triVerts[a], triVerts[b]);
                            int e1 = System.Math.Max(triVerts[a], triVerts[b]);
                            affectedEdges.Add((e0, e1));
                        }
                    }
                }
            }

            foreach (var edge in affectedEdges)
            {
                if (alive[edge.Item1] && alive[edge.Item2] && edge.Item1 != edge.Item2)
                    TryAddEdge(edge.Item1, edge.Item2, quadrics, verts, edgeCosts, edgePositions, edgeSet);
            }
        }

        var aliveIndices = new Dictionary<int, int>();
        var newVerts = ImmutableArray.CreateBuilder<Point3D>();
        var newIndices = ImmutableArray.CreateBuilder<int>();
        for (int i = 0; i < verts.Count; i++)
        {
            if (alive[i])
            {
                aliveIndices[i] = newVerts.Count;
                newVerts.Add(verts[i]);
            }
        }
        for (int t = 0; t < tris.Count; t++)
        {
            (int t0, int t1, int t2) = tris[t];
            if (alive[t0] && alive[t1] && alive[t2])
            {
                newIndices.Add(aliveIndices[t0]);
                newIndices.Add(aliveIndices[t1]);
                newIndices.Add(aliveIndices[t2]);
            }
        }

        return (newVerts.ToImmutable(), newIndices.ToImmutable());
    }

    private static QuadricMatrix4x4 ComputeTriangleQuadric(Point3D p0, Point3D p1, Point3D p2)
    {
        double ax = p1.X - p0.X, ay = p1.Y - p0.Y, az = p1.Z - p0.Z;
        double bx = p2.X - p0.X, by = p2.Y - p0.Y, bz = p2.Z - p0.Z;
        double nx = ay * bz - az * by;
        double ny = az * bx - ax * bz;
        double nz = ax * by - ay * bx;
        double len = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);
        if (len < Tolerance)
            return default;
        nx /= len; ny /= len; nz /= len;
        double d = -(nx * p0.X + ny * p0.Y + nz * p0.Z);
        return QuadricMatrix4x4.FromPlane(nx, ny, nz, d);
    }

    private static void TryAddEdge(int v0, int v1, QuadricMatrix4x4[] quadrics, List<Point3D> verts,
        SortedSet<(double Cost, int V0, int V1)> edgeCosts,
        Dictionary<(int, int), Point3D> edgePositions,
        HashSet<(int, int)> edgeSet)
    {
        if (v0 == v1) return;
        (int key0, int key1) = v0 < v1 ? (v0, v1) : (v1, v0);
        if (!edgeSet.Add((key0, key1))) return;
        QuadricMatrix4x4 q = quadrics[v0] + quadrics[v1];
        Point3D pos = OptimizedPoint(q, verts[v0], verts[v1]);
        double cost = q.Evaluate(pos);
        edgeCosts.Add((cost, key0, key1));
        edgePositions[(key0, key1)] = pos;
    }

    private static Point3D OptimizedPoint(QuadricMatrix4x4 q, Point3D v0, Point3D v1)
    {
        double det = (q.M00 * (q.M11 * q.M22 - q.M12 * q.M12))
                    - (q.M01 * (q.M01 * q.M22 - q.M12 * q.M02))
                    + (q.M02 * (q.M01 * q.M12 - q.M11 * q.M02));
        if (System.Math.Abs(det) > Tolerance)
        {
            double invDet = 1.0 / det;
            double x = -(q.M03 * (q.M11 * q.M22 - q.M12 * q.M12)
                       - q.M13 * (q.M01 * q.M22 - q.M12 * q.M02)
                       + q.M23 * (q.M01 * q.M12 - q.M11 * q.M02)) * invDet;
            double y = -(q.M00 * (q.M13 * q.M22 - q.M12 * q.M23)
                       - q.M01 * (q.M03 * q.M22 - q.M12 * q.M03)
                       + q.M02 * (q.M03 * q.M12 - q.M13 * q.M02)) * invDet;
            double z = -(q.M00 * (q.M11 * q.M23 - q.M13 * q.M12)
                       - q.M01 * (q.M01 * q.M23 - q.M13 * q.M02)
                       + q.M02 * (q.M01 * q.M13 - q.M11 * q.M03)) * invDet;
            return new Point3D(x, y, z);
        }
        return v0;
    }

    private static int GetOtherVertex(List<(int I0, int I1, int I2)> tris, int shared, int index)
    {
        if (index >= tris.Count) return -1;
        (int t0, int t1, int t2) = tris[index];
        if (t0 != shared) return t0;
        if (t1 != shared) return t1;
        return t2;
    }
}
