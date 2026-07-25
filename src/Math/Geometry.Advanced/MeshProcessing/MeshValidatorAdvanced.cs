using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>Enumerates the types of mesh issues that can be detected during validation.</summary>
public enum MeshIssueType
{
    /// <summary>An edge is shared by more than two faces.</summary>
    NonManifoldEdge,

    /// <summary>A triangle has near-zero area.</summary>
    DegenerateTriangle,

    /// <summary>Multiple vertices occupy the same position within tolerance.</summary>
    DuplicateVertex,

    /// <summary>A vertex is not referenced by any triangle.</summary>
    OrphanVertex,

    /// <summary>Adjacent triangles have inconsistent winding order.</summary>
    IncorrectWinding,

    /// <summary>Two non-adjacent triangles intersect each other.</summary>
    SelfIntersection
}

/// <summary>Describes a mesh issue detected during validation.</summary>
/// <param name="Type">The type of issue detected.</param>
/// <param name="Description">A human-readable description of the issue.</param>
/// <param name="ElementIndex">The index of the element (vertex, edge, or triangle) where the issue was found.</param>
public readonly record struct MeshIssue(MeshIssueType Type, string Description, int ElementIndex);

/// <summary>Provides comprehensive mesh validation for triangle meshes, detecting non-manifold edges,
/// degenerate triangles, duplicate vertices, orphan vertices, incorrect winding, and basic self-intersections.</summary>
public static class MeshValidatorAdvanced
{
    private const double Tolerance = 1e-10;

    /// <summary>Validates the given triangle mesh and returns a list of all detected issues.</summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>An immutable array of <see cref="MeshIssue"/> describing all detected problems.</returns>
    public static ImmutableArray<MeshIssue> Validate(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var issues = ImmutableArray.CreateBuilder<MeshIssue>();
        int triCount = indices.Length / 3;
        if (triCount == 0) return issues.ToImmutable();

        CheckDegenerateTriangles(vertices, indices, triCount, issues);
        CheckDuplicateVertices(vertices, issues);
        CheckOrphanVertices(vertices.Length, indices, triCount, issues);
        CheckNonManifoldEdges(indices, triCount, issues);
        CheckIncorrectWinding(indices, triCount, issues);
        CheckSelfIntersections(vertices, indices, triCount, issues);
        return issues.ToImmutable();
    }

    private static void CheckDegenerateTriangles(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, int triCount, ImmutableArray<MeshIssue>.Builder issues)
    {
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3], i1 = indices[t * 3 + 1], i2 = indices[t * 3 + 2];
            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length) continue;
            Point3D p0 = vertices[i0], p1 = vertices[i1], p2 = vertices[i2];
            double ax = p1.X - p0.X, ay = p1.Y - p0.Y, az = p1.Z - p0.Z;
            double bx = p2.X - p0.X, by = p2.Y - p0.Y, bz = p2.Z - p0.Z;
            double nx = ay * bz - az * by;
            double ny = az * bx - ax * bz;
            double nz = ax * by - ay * bx;
            double area = System.Math.Sqrt(nx * nx + ny * ny + nz * nz) * 0.5;
            if (area < Tolerance)
                issues.Add(new MeshIssue(MeshIssueType.DegenerateTriangle, $"Triangle {t} has near-zero area ({area}).", t));
        }
    }

    private static void CheckDuplicateVertices(ImmutableArray<Point3D> vertices, ImmutableArray<MeshIssue>.Builder issues)
    {
        var seen = new Dictionary<(long, long, long), int>();
        for (int i = 0; i < vertices.Length; i++)
        {
            long x = (long)System.Math.Round(vertices[i].X / Tolerance);
            long y = (long)System.Math.Round(vertices[i].Y / Tolerance);
            long z = (long)System.Math.Round(vertices[i].Z / Tolerance);
            var key = (x, y, z);
            if (seen.TryGetValue(key, out int first))
                issues.Add(new MeshIssue(MeshIssueType.DuplicateVertex, $"Vertex {i} duplicates vertex {first}.", i));
            else
                seen[key] = i;
        }
    }

    private static void CheckOrphanVertices(int vertexCount, ImmutableArray<int> indices, int triCount, ImmutableArray<MeshIssue>.Builder issues)
    {
        bool[] referenced = new bool[vertexCount];
        for (int t = 0; t < triCount; t++)
        {
            for (int k = 0; k < 3; k++)
            {
                int idx = indices[t * 3 + k];
                if (idx >= 0 && idx < vertexCount)
                    referenced[idx] = true;
            }
        }
        for (int i = 0; i < vertexCount; i++)
        {
            if (!referenced[i])
                issues.Add(new MeshIssue(MeshIssueType.OrphanVertex, $"Vertex {i} is not referenced by any triangle.", i));
        }
    }

    private static void CheckNonManifoldEdges(ImmutableArray<int> indices, int triCount, ImmutableArray<MeshIssue>.Builder issues)
    {
        var edgeCount = new Dictionary<(int, int), int>();
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3], i1 = indices[t * 3 + 1], i2 = indices[t * 3 + 2];
            int[] tri = { i0, i1, i2 };
            for (int e = 0; e < 3; e++)
            {
                int v0 = tri[e], v1 = tri[(e + 1) % 3];
                (int, int) key = v0 < v1 ? (v0, v1) : (v1, v0);
                edgeCount.TryGetValue(key, out int count);
                edgeCount[key] = count + 1;
            }
        }
        foreach (var kvp in edgeCount)
        {
            if (kvp.Value > 2)
                issues.Add(new MeshIssue(MeshIssueType.NonManifoldEdge,
                    $"Edge ({kvp.Key.Item1}, {kvp.Key.Item2}) is shared by {kvp.Value} faces.", kvp.Key.Item1));
        }
    }

    private static void CheckIncorrectWinding(ImmutableArray<int> indices, int triCount, ImmutableArray<MeshIssue>.Builder issues)
    {
        var edgeFaces = new Dictionary<(int, int), (int Face, int Orientation)>();
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3], i1 = indices[t * 3 + 1], i2 = indices[t * 3 + 2];
            int[] tri = { i0, i1, i2 };
            for (int e = 0; e < 3; e++)
            {
                int v0 = tri[e], v1 = tri[(e + 1) % 3];
                (int, int) key = v0 < v1 ? (v0, v1) : (v1, v0);
                int orientation = v0 < v1 ? 1 : -1;
                if (edgeFaces.TryGetValue(key, out var existing))
                {
                    if (existing.Orientation == orientation)
                        issues.Add(new MeshIssue(MeshIssueType.IncorrectWinding,
                            $"Edge ({key.Item1}, {key.Item2}) in triangles {existing.Face} and {t} have same winding.", t));
                }
                else
                    edgeFaces[key] = (t, orientation);
            }
        }
    }

    private static void CheckSelfIntersections(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, int triCount, ImmutableArray<MeshIssue>.Builder issues)
    {
        int maxPairs = System.Math.Min(triCount * (triCount - 1) / 2, 500000);
        var checkedPairs = new HashSet<(int, int)>();
        int checked_count = 0;
        for (int t0 = 0; t0 < triCount && checked_count < maxPairs; t0++)
        {
            for (int t1 = t0 + 1; t1 < triCount && checked_count < maxPairs; t1++)
            {
                if (!AreTrianglesClose(vertices, indices, t0, t1)) continue;
                checked_count++;
                if (TrianglesIntersect(vertices, indices, t0, t1))
                    issues.Add(new MeshIssue(MeshIssueType.SelfIntersection,
                        $"Triangles {t0} and {t1} intersect.", t0));
            }
        }
    }

    private static bool AreTrianglesClose(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, int t0, int t1)
    {
        BoundingBox3D b0 = ComputeTriBounds(vertices, indices, t0);
        BoundingBox3D b1 = ComputeTriBounds(vertices, indices, t1);
        return b0.Intersects(b1);
    }

    private static BoundingBox3D ComputeTriBounds(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, int tri)
    {
        int i0 = indices[tri * 3], i1 = indices[tri * 3 + 1], i2 = indices[tri * 3 + 2];
        Point3D p0 = vertices[i0], p1 = vertices[i1], p2 = vertices[i2];
        double minX = System.Math.Min(p0.X, System.Math.Min(p1.X, p2.X));
        double minY = System.Math.Min(p0.Y, System.Math.Min(p1.Y, p2.Y));
        double minZ = System.Math.Min(p0.Z, System.Math.Min(p1.Z, p2.Z));
        double maxX = System.Math.Max(p0.X, System.Math.Max(p1.X, p2.X));
        double maxY = System.Math.Max(p0.Y, System.Math.Max(p1.Y, p2.Y));
        double maxZ = System.Math.Max(p0.Z, System.Math.Max(p1.Z, p2.Z));
        return new BoundingBox3D(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, maxZ));
    }

    private static bool TrianglesIntersect(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, int t0, int t1)
    {
        Point3D a0 = vertices[indices[t0 * 3]], a1 = vertices[indices[t0 * 3 + 1]], a2 = vertices[indices[t0 * 3 + 2]];
        Point3D b0 = vertices[indices[t1 * 3]], b1 = vertices[indices[t1 * 3 + 1]], b2 = vertices[indices[t1 * 3 + 2]];

        Vector3D n1 = EdgeCross(a0, a1, a2);
        double d1 = n1.Dot(a0.ToVector3D());
        double d0 = n1.Dot(b0.ToVector3D()) - d1;
        double d1v = n1.Dot(b1.ToVector3D()) - d1;
        double d2 = n1.Dot(b2.ToVector3D()) - d1;
        if (d0 * d1 > Tolerance && d0 * d2 > Tolerance) return false;
        if (d1 * d2 > Tolerance) return false;

        Vector3D n2 = EdgeCross(b0, b1, b2);
        double dd1 = n2.Dot(b0.ToVector3D());
        double da0 = n2.Dot(a0.ToVector3D()) - dd1;
        double da1 = n2.Dot(a1.ToVector3D()) - dd1;
        double da2 = n2.Dot(a2.ToVector3D()) - dd1;
        if (da0 * da1 > Tolerance && da0 * da2 > Tolerance) return false;
        if (da1 * da2 > Tolerance) return false;

        Vector3D dir = n1.Cross(n2);
        if (dir.Length < Tolerance) return false;

        Vector3D v = a0.ToVector3D().Subtract(b0.ToVector3D());
        double denom = dir.Dot(dir);
        if (System.Math.Abs(denom) < Tolerance) return false;
        double t = dir.Dot(v) / denom;
        Point3D closest = new Point3D(a0.X + dir.X * t, a0.Y + dir.Y * t, a0.Z + dir.Z * t);

        Point3D ca0 = a0, ca1 = a1, ca2 = a2;
        Point3D cb0 = b0, cb1 = b1, cb2 = b2;
        return PointInTriangle3D(closest, ca0, ca1, ca2, n1) && PointInTriangle3D(closest, cb0, cb1, cb2, n2);
    }

    private static Vector3D EdgeCross(Point3D p0, Point3D p1, Point3D p2)
    {
        double ax = p1.X - p0.X, ay = p1.Y - p0.Y, az = p1.Z - p0.Z;
        double bx = p2.X - p0.X, by = p2.Y - p0.Y, bz = p2.Z - p0.Z;
        return new Vector3D(ay * bz - az * by, az * bx - ax * bz, ax * by - ay * bx);
    }

    private static bool PointInTriangle3D(Point3D p, Point3D a, Point3D b, Point3D c, Vector3D normal)
    {
        Vector3D ap = p.ToVector3D().Subtract(a.ToVector3D());
        Vector3D bp = p.ToVector3D().Subtract(b.ToVector3D());
        Vector3D cp = p.ToVector3D().Subtract(c.ToVector3D());
        Vector3D ab = b.ToVector3D().Subtract(a.ToVector3D());
        Vector3D bc = c.ToVector3D().Subtract(b.ToVector3D());
        Vector3D ca = a.ToVector3D().Subtract(c.ToVector3D());
        double d1 = normal.Dot(ap.Cross(ab));
        double d2 = normal.Dot(bp.Cross(bc));
        double d3 = normal.Dot(cp.Cross(ca));
        bool hasNeg = (d1 < -Tolerance) || (d2 < -Tolerance) || (d3 < -Tolerance);
        bool hasPos = (d1 > Tolerance) || (d2 > Tolerance) || (d3 > Tolerance);
        return !(hasNeg && hasPos);
    }
}
