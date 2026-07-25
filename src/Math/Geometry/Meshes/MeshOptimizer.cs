using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Provides static methods for mesh optimization, analysis, and modification.</summary>
public static class MeshOptimizer
{
    /// <summary>Merges vertices that are within the specified tolerance distance.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <param name="tolerance">The distance threshold for welding.</param>
    /// <returns>A new mesh with welded vertices and remapped face indices.</returns>
    public static TriangleMesh WeldVertices(TriangleMesh mesh, double tolerance)
    {
        if (mesh.VertexCount == 0)
            return mesh;

        double toleranceSq = tolerance * tolerance;
        int[] remap = new int[mesh.VertexCount];
        List<Vertex> uniqueVertices = new();
        Dictionary<int, int> canonicalToNew = new();

        for (int i = 0; i < mesh.VertexCount; i++)
        {
            bool found = false;
            Point3D pi = mesh.Vertices[i].Position;

            foreach (KeyValuePair<int, int> kvp in canonicalToNew)
            {
                Point3D pj = uniqueVertices[kvp.Value].Position;
                if (pi.DistanceSquaredTo(pj) <= toleranceSq)
                {
                    remap[i] = kvp.Value;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                int newIndex = uniqueVertices.Count;
                uniqueVertices.Add(mesh.Vertices[i]);
                canonicalToNew[i] = newIndex;
                remap[i] = newIndex;
            }
        }

        TriangleFace[] newFaces = new TriangleFace[mesh.Faces.Length];
        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            newFaces[i] = new TriangleFace(remap[f.V0], remap[f.V1], remap[f.V2]);
        }

        return new TriangleMesh(
            uniqueVertices.ToImmutableArray(),
            ImmutableArray.Create(newFaces));
    }

    /// <summary>Removes triangles with area below the specified tolerance.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <param name="tolerance">The minimum triangle area threshold.</param>
    /// <returns>A new mesh with degenerate triangles removed.</returns>
    public static TriangleMesh RemoveDegenerateTriangles(TriangleMesh mesh, double tolerance)
    {
        List<TriangleFace> valid = new();

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D a = mesh.Vertices[f.V0].Position;
            Point3D b = mesh.Vertices[f.V1].Position;
            Point3D c = mesh.Vertices[f.V2].Position;

            Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector3D ac = new(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            double area = ab.Cross(ac).Length * 0.5;

            if (area >= tolerance)
                valid.Add(f);
        }

        return new TriangleMesh(mesh.Vertices, valid.ToImmutableArray());
    }

    /// <summary>Computes the length of each edge in every triangle face.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>An immutable array of edge lengths, three per face.</returns>
    public static ImmutableArray<double> ComputeEdgeLengths(TriangleMesh mesh)
    {
        ImmutableArray<double>.Builder builder = ImmutableArray.CreateBuilder<double>(mesh.Faces.Length * 3);

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D p0 = mesh.Vertices[f.V0].Position;
            Point3D p1 = mesh.Vertices[f.V1].Position;
            Point3D p2 = mesh.Vertices[f.V2].Position;

            builder.Add(p0.DistanceTo(p1));
            builder.Add(p1.DistanceTo(p2));
            builder.Add(p2.DistanceTo(p0));
        }

        return builder.ToImmutable();
    }

    /// <summary>Computes the valence (number of adjacent faces) for each vertex.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>An immutable array of valence counts, one per vertex.</returns>
    public static ImmutableArray<int> ComputeVertexValences(TriangleMesh mesh)
    {
        int[] valences = new int[mesh.VertexCount];

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            valences[f.V0]++;
            valences[f.V1]++;
            valences[f.V2]++;
        }

        return ImmutableArray.Create(valences);
    }

    /// <summary>Computes the area of each triangle face.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>An immutable array of triangle areas, one per face.</returns>
    public static ImmutableArray<double> ComputeTriangleAreas(TriangleMesh mesh)
    {
        ImmutableArray<double>.Builder builder = ImmutableArray.CreateBuilder<double>(mesh.Faces.Length);

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D a = mesh.Vertices[f.V0].Position;
            Point3D b = mesh.Vertices[f.V1].Position;
            Point3D c = mesh.Vertices[f.V2].Position;

            Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector3D ac = new(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            builder.Add(ab.Cross(ac).Length * 0.5);
        }

        return builder.ToImmutable();
    }

    /// <summary>Computes the signed volume enclosed by the mesh using the divergence theorem.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>The signed volume of the enclosed region.</returns>
    public static double ComputeMeshVolume(TriangleMesh mesh)
    {
        double volume = 0.0;

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D a = mesh.Vertices[f.V0].Position;
            Point3D b = mesh.Vertices[f.V1].Position;
            Point3D c = mesh.Vertices[f.V2].Position;

            volume += a.X * (b.Y * c.Z - c.Y * b.Z)
                    - b.X * (a.Y * c.Z - c.Y * a.Z)
                    + c.X * (a.Y * b.Z - b.Y * a.Z);
        }

        return volume / 6.0;
    }

    /// <summary>Computes the total surface area of the mesh.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>The sum of all triangle face areas.</returns>
    public static double ComputeSurfaceArea(TriangleMesh mesh)
    {
        double area = 0.0;

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D a = mesh.Vertices[f.V0].Position;
            Point3D b = mesh.Vertices[f.V1].Position;
            Point3D c = mesh.Vertices[f.V2].Position;

            Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector3D ac = new(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            area += ab.Cross(ac).Length * 0.5;
        }

        return area;
    }

    /// <summary>Determines whether every edge is shared by at most two faces.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>True if the mesh is manifold; otherwise, false.</returns>
    public static bool IsManifold(TriangleMesh mesh)
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

        foreach (KeyValuePair<(int, int), int> kvp in edgeCount)
        {
            if (kvp.Value > 2)
                return false;
        }

        return true;
    }

    /// <summary>Determines whether the mesh is watertight (every edge shared by exactly two faces).</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>True if the mesh is watertight; otherwise, false.</returns>
    public static bool IsWatertight(TriangleMesh mesh)
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

        foreach (KeyValuePair<(int, int), int> kvp in edgeCount)
        {
            if (kvp.Value != 2)
                return false;
        }

        return true;
    }

    /// <summary>Computes the Euler characteristic of the mesh (V - E + F).</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>The Euler characteristic.</returns>
    public static int ComputeEulerCharacteristic(TriangleMesh mesh)
    {
        int v = mesh.VertexCount;
        int f = mesh.Faces.Length;
        int e = mesh.GetEdges().Length;
        return v - e + f;
    }

    /// <summary>Performs an edge flip between two adjacent triangles sharing an edge.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <param name="faceIndex0">Index of the first triangle.</param>
    /// <param name="faceIndex1">Index of the second triangle.</param>
    /// <returns>A new mesh with the edge flipped, or the original mesh if the flip is invalid.</returns>
    public static TriangleMesh FlipEdge(TriangleMesh mesh, int faceIndex0, int faceIndex1)
    {
        if (faceIndex0 < 0 || faceIndex0 >= mesh.Faces.Length ||
            faceIndex1 < 0 || faceIndex1 >= mesh.Faces.Length ||
            faceIndex0 == faceIndex1)
        {
            return mesh;
        }

        TriangleFace f0 = mesh.Faces[faceIndex0];
        TriangleFace f1 = mesh.Faces[faceIndex1];

        int[] a = [f0.V0, f0.V1, f0.V2];
        int[] b = [f1.V0, f1.V1, f1.V2];

        int sharedA = -1, sharedB = -1;
        int uniqueA = -1, uniqueB = -1;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (a[i] == b[j])
                {
                    if (sharedA == -1)
                        sharedA = a[i];
                    else
                        sharedB = a[i];
                }
            }
        }

        if (sharedA == -1 || sharedB == -1)
            return mesh;

        for (int i = 0; i < 3; i++)
        {
            if (a[i] != sharedA && a[i] != sharedB)
                uniqueA = a[i];
            if (b[i] != sharedA && b[i] != sharedB)
                uniqueB = b[i];
        }

        if (uniqueA == -1 || uniqueB == -1)
            return mesh;

        TriangleFace[] newFaces = new TriangleFace[mesh.Faces.Length];
        for (int i = 0; i < mesh.Faces.Length; i++)
            newFaces[i] = mesh.Faces[i];

        newFaces[faceIndex0] = new TriangleFace(uniqueA, uniqueB, sharedA);
        newFaces[faceIndex1] = new TriangleFace(uniqueA, uniqueB, sharedB);

        return new TriangleMesh(mesh.Vertices, ImmutableArray.Create(newFaces));
    }
}
