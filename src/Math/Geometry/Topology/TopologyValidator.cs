using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Meshes;

namespace MathVerse.Math.Geometry.Topology;

/// <summary>Provides static methods for validating triangle mesh topology and geometry.</summary>
public static class TopologyValidator
{
    /// <summary>Validates the mesh for topological and geometric correctness.</summary>
    /// <param name="mesh">The triangle mesh to validate.</param>
    /// <returns>A geometry result indicating success or describing the first error found.</returns>
    public static GeometryResult Validate(TriangleMesh mesh)
    {
        if (mesh.VertexCount == 0 && mesh.Faces.Length == 0)
            return GeometryResult.Ok();

        if (mesh.VertexCount == 0)
            return GeometryResult.Failure("Mesh has faces but no vertices.", GeometryDiagnosticType.EmptyMesh);

        if (mesh.Faces.Length == 0)
            return GeometryResult.Failure("Mesh has no faces.", GeometryDiagnosticType.EmptyMesh);

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];

            if (f.V0 < 0 || f.V0 >= mesh.VertexCount ||
                f.V1 < 0 || f.V1 >= mesh.VertexCount ||
                f.V2 < 0 || f.V2 >= mesh.VertexCount)
            {
                return GeometryResult.Failure(
                    $"Face {i} references out-of-range vertex index.",
                    GeometryDiagnosticType.InvalidFace);
            }

            if (f.V0 == f.V1 || f.V1 == f.V2 || f.V0 == f.V2)
            {
                return GeometryResult.Failure(
                    $"Face {i} is degenerate (duplicate vertex indices).",
                    GeometryDiagnosticType.DegenerateGeometry);
            }

            Point3D a = mesh.Vertices[f.V0].Position;
            Point3D b = mesh.Vertices[f.V1].Position;
            Point3D c = mesh.Vertices[f.V2].Position;

            Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector3D ac = new(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            double area = ab.Cross(ac).Length * 0.5;

            if (area < 1e-30)
            {
                return GeometryResult.Failure(
                    $"Face {i} has near-zero area (degenerate triangle).",
                    GeometryDiagnosticType.DegenerateGeometry);
            }
        }

        ImmutableArray<Edge> nonManifold = DetectNonManifoldEdges(mesh);
        if (nonManifold.Length > 0)
        {
            return GeometryResult.Failure(
                $"Mesh has {nonManifold.Length} non-manifold edge(s).",
                    GeometryDiagnosticType.NonManifold);
        }

        return GeometryResult.Ok();
    }

    /// <summary>Detects boundary edges that are shared by only one face.</summary>
    /// <param name="mesh">The triangle mesh to analyze.</param>
    /// <returns>An immutable array of boundary edges.</returns>
    public static ImmutableArray<Edge> DetectBoundaryEdges(TriangleMesh mesh)
    {
        Dictionary<(int, int), int> edgeCount = new();

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            int[] verts = { f.V0, f.V1, f.V2 };

            for (int j = 0; j < 3; j++)
            {
                int a = verts[j];
                int b = verts[(j + 1) % 3];
                (int, int) key = a < b ? (a, b) : (b, a);
                edgeCount.TryGetValue(key, out int count);
                edgeCount[key] = count + 1;
            }
        }

        ImmutableArray<Edge>.Builder builder = ImmutableArray.CreateBuilder<Edge>();

        foreach (KeyValuePair<(int, int), int> kvp in edgeCount)
        {
            if (kvp.Value == 1)
                builder.Add(new Edge(kvp.Key.Item1, kvp.Key.Item2));
        }

        return builder.ToImmutable();
    }

    /// <summary>Detects non-manifold edges shared by more than two faces.</summary>
    /// <param name="mesh">The triangle mesh to analyze.</param>
    /// <returns>An immutable array of non-manifold edges.</returns>
    public static ImmutableArray<Edge> DetectNonManifoldEdges(TriangleMesh mesh)
    {
        Dictionary<(int, int), int> edgeCount = new();

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            int[] verts = { f.V0, f.V1, f.V2 };

            for (int j = 0; j < 3; j++)
            {
                int a = verts[j];
                int b = verts[(j + 1) % 3];
                (int, int) key = a < b ? (a, b) : (b, a);
                edgeCount.TryGetValue(key, out int count);
                edgeCount[key] = count + 1;
            }
        }

        ImmutableArray<Edge>.Builder builder = ImmutableArray.CreateBuilder<Edge>();

        foreach (KeyValuePair<(int, int), int> kvp in edgeCount)
        {
            if (kvp.Value > 2)
                builder.Add(new Edge(kvp.Key.Item1, kvp.Key.Item2));
        }

        return builder.ToImmutable();
    }

    /// <summary>Detects degenerate triangles with area below the specified tolerance.</summary>
    /// <param name="mesh">The triangle mesh to analyze.</param>
    /// <param name="tolerance">The minimum area threshold.</param>
    /// <returns>An immutable array of indices of degenerate triangles.</returns>
    public static ImmutableArray<int> DetectDegenerateTriangles(TriangleMesh mesh, double tolerance)
    {
        ImmutableArray<int>.Builder builder = ImmutableArrayCreateBuilderForIndices(mesh.Faces.Length);

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D a = mesh.Vertices[f.V0].Position;
            Point3D b = mesh.Vertices[f.V1].Position;
            Point3D c = mesh.Vertices[f.V2].Position;

            Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector3D ac = new(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            double area = ab.Cross(ac).Length * 0.5;

            if (area < tolerance)
                builder.Add(i);
        }

        return builder.ToImmutable();
    }

    /// <summary>Detects pairs of vertices that overlap within the specified tolerance.</summary>
    /// <param name="mesh">The triangle mesh to analyze.</param>
    /// <param name="tolerance">The distance threshold for overlap detection.</param>
    /// <returns>An immutable array of overlapping vertex index pairs.</returns>
    public static ImmutableArray<(int, int)> DetectOverlappingVertices(TriangleMesh mesh, double tolerance)
    {
        double toleranceSq = tolerance * tolerance;
        List<(int, int)> overlaps = new();

        for (int i = 0; i < mesh.VertexCount; i++)
        {
            for (int j = i + 1; j < mesh.VertexCount; j++)
            {
                double distSq = mesh.Vertices[i].Position.DistanceSquaredTo(mesh.Vertices[j].Position);
                if (distSq <= toleranceSq)
                    overlaps.Add((i, j));
            }
        }

        return overlaps.ToImmutableArray();
    }

    private static ImmutableArray<int>.Builder ImmutableArrayCreateBuilderForIndices(int capacity)
    {
        return ImmutableArray.CreateBuilder<int>(capacity);
    }
}
