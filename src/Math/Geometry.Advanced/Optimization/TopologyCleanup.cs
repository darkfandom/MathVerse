using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Optimization;

/// <summary>
/// Describes a single topology modification made during cleanup.
/// </summary>
/// <param name="Description">Human-readable description of the change.</param>
/// <param name="Count">Number of elements affected by this change.</param>
public readonly record struct TopologyChange(string Description, int Count);

/// <summary>
/// Provides a full topology cleanup pipeline for triangle meshes.
/// Removes duplicate faces, fixes non-manifold edges, removes orphan vertices,
/// and fixes winding inconsistencies.
/// </summary>
public static class TopologyCleanup
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Runs a full topology cleanup pipeline on the input mesh.
    /// Performs duplicate face removal, non-manifold edge fixing, orphan vertex removal,
    /// and winding consistency correction. Reports all changes made.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>
    /// A tuple containing the cleaned vertices, cleaned indices, and a list of all topology changes made.
    /// </returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices, ImmutableArray<TopologyChange> Changes) Cleanup(
        ImmutableArray<Point3D> vertices,
        ImmutableArray<int> indices)
    {
        var changes = ImmutableArray.CreateBuilder<TopologyChange>();
        var currentIndices = indices;
        var currentVertices = vertices;

        var (dedupedIndices, dedupCount) = RemoveDuplicateFaces(currentIndices);
        if (dedupCount > 0)
        {
            changes.Add(new TopologyChange("Removed duplicate faces", dedupCount));
            currentIndices = dedupedIndices;
        }

        var (fixedIndices, fixedCount) = FixNonManifoldEdges(currentIndices);
        if (fixedCount > 0)
        {
            changes.Add(new TopologyChange("Fixed non-manifold edges", fixedCount));
            currentIndices = fixedIndices;
        }

        var (woundIndices, woundCount) = FixWindingConsistency(currentVertices, currentIndices);
        if (woundCount > 0)
        {
            changes.Add(new TopologyChange("Fixed winding inconsistencies", woundCount));
            currentIndices = woundIndices;
        }

        var (orphanVerts, orphanIndices, orphanCount) = RemoveOrphanVertices(currentVertices, currentIndices);
        if (orphanCount > 0)
        {
            changes.Add(new TopologyChange("Removed orphan vertices", orphanCount));
            currentVertices = orphanVerts;
            currentIndices = orphanIndices;
        }

        return (currentVertices, currentIndices, changes.ToImmutable());
    }

    private static (ImmutableArray<int> Indices, int DuplicateCount) RemoveDuplicateFaces(ImmutableArray<int> indices)
    {
        int triCount = indices.Length / 3;
        var seen = new HashSet<(int, int, int)>();
        var result = ImmutableArray.CreateBuilder<int>(indices.Length);
        int duplicates = 0;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            int lo = System.Math.Min(i0, System.Math.Min(i1, i2));
            int hi = System.Math.Max(i0, System.Math.Max(i1, i2));
            int mid = i0 + i1 + i2 - lo - hi;
            var key = (lo, mid, hi);

            if (seen.Add(key))
            {
                result.Add(i0);
                result.Add(i1);
                result.Add(i2);
            }
            else
            {
                duplicates++;
            }
        }

        return (result.ToImmutable(), duplicates);
    }

    private static (ImmutableArray<int> Indices, int FixedCount) FixNonManifoldEdges(ImmutableArray<int> indices)
    {
        int triCount = indices.Length / 3;
        var edgeCount = new Dictionary<(int, int), int>();

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            CountEdge(edgeCount, i0, i1);
            CountEdge(edgeCount, i1, i2);
            CountEdge(edgeCount, i2, i0);
        }

        int fixedCount = 0;
        var toRemove = new HashSet<int>();

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            int maxEdge = MaxEdgeUse(edgeCount, i0, i1, i2);
            if (maxEdge > 2)
            {
                toRemove.Add(t);
                fixedCount++;
            }
        }

        if (fixedCount == 0)
            return (indices, 0);

        var result = ImmutableArray.CreateBuilder<int>(indices.Length - fixedCount * 3);
        for (int t = 0; t < triCount; t++)
        {
            if (!toRemove.Contains(t))
            {
                result.Add(indices[t * 3]);
                result.Add(indices[t * 3 + 1]);
                result.Add(indices[t * 3 + 2]);
            }
        }

        return (result.ToImmutable(), fixedCount);
    }

    private static (ImmutableArray<int> Indices, int FixedCount) FixWindingConsistency(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        int triCount = indices.Length / 3;
        if (triCount == 0)
            return (indices, 0);

        var edgeFaces = new Dictionary<(int, int), List<int>>();
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];
            AddFaceToEdge(edgeFaces, i0, i1, t);
            AddFaceToEdge(edgeFaces, i1, i2, t);
            AddFaceToEdge(edgeFaces, i2, i0, t);
        }

        var faceAdj = new List<(int Other, int SharedA, int SharedB)>[triCount];
        for (int t = 0; t < triCount; t++)
            faceAdj[t] = new List<(int, int, int)>();

        foreach (var kvp in edgeFaces)
        {
            if (kvp.Value.Count == 2)
            {
                int f0 = kvp.Value[0], f1 = kvp.Value[1];
                faceAdj[f0].Add((f1, kvp.Key.Item1, kvp.Key.Item2));
                faceAdj[f1].Add((f0, kvp.Key.Item2, kvp.Key.Item1));
            }
        }

        var flipped = new bool[triCount];
        var currentIndices = new int[indices.Length];
        for (int i = 0; i < indices.Length; i++)
            currentIndices[i] = indices[i];

        var visited = new bool[triCount];
        var queue = new Queue<int>();
        queue.Enqueue(0);
        visited[0] = true;

        int fixedCount = 0;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            bool currentFlipped = flipped[current];
            int ci0 = currentIndices[current * 3];
            int ci1 = currentIndices[current * 3 + 1];
            int ci2 = currentIndices[current * 3 + 2];

            foreach (var (neighbor, sharedA, sharedB) in faceAdj[current])
            {
                if (visited[neighbor]) continue;
                visited[neighbor] = true;

                int ni0 = indices[neighbor * 3];
                int ni1 = indices[neighbor * 3 + 1];
                int ni2 = indices[neighbor * 3 + 2];

                bool neighborInOriginal = (ni0 == sharedA && ni1 == sharedB) ||
                                          (ni1 == sharedA && ni2 == sharedB) ||
                                          (ni2 == sharedA && ni0 == sharedB);

                bool shouldFlip = currentFlipped ? neighborInOriginal : !neighborInOriginal;

                if (shouldFlip)
                {
                    currentIndices[neighbor * 3 + 1] = ni2;
                    currentIndices[neighbor * 3 + 2] = ni1;
                    flipped[neighbor] = !currentFlipped;
                    fixedCount++;
                }
                else
                {
                    flipped[neighbor] = currentFlipped;
                }

                queue.Enqueue(neighbor);
            }
        }

        if (fixedCount == 0)
            return (indices, 0);

        var result = ImmutableArray.CreateBuilder<int>(indices.Length);
        for (int i = 0; i < indices.Length; i++)
            result.Add(currentIndices[i]);

        return (result.ToImmutable(), fixedCount);
    }

    private static void AddFaceToEdge(Dictionary<(int, int), List<int>> edgeFaces, int v0, int v1, int face)
    {
        int lo = v0 < v1 ? v0 : v1;
        int hi = v0 < v1 ? v1 : v0;
        var key = (lo, hi);
        if (!edgeFaces.ContainsKey(key))
            edgeFaces[key] = new List<int>();
        if (edgeFaces[key].Count < 2)
            edgeFaces[key].Add(face);
    }

    private static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices, int RemovedCount) RemoveOrphanVertices(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        bool[] referenced = new bool[vertices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] >= 0 && indices[i] < vertices.Length)
                referenced[indices[i]] = true;
        }

        int removedCount = 0;
        var remap = new int[vertices.Length];
        var newVertices = ImmutableArray.CreateBuilder<Point3D>(vertices.Length);

        for (int i = 0; i < vertices.Length; i++)
        {
            if (referenced[i])
            {
                remap[i] = newVertices.Count;
                newVertices.Add(vertices[i]);
            }
            else
            {
                remap[i] = -1;
                removedCount++;
            }
        }

        if (removedCount == 0)
            return (vertices, indices, 0);

        var newIndices = ImmutableArray.CreateBuilder<int>(indices.Length);
        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] >= 0 && indices[i] < vertices.Length)
                newIndices.Add(remap[indices[i]]);
        }

        return (newVertices.ToImmutable(), newIndices.ToImmutable(), removedCount);
    }

    private static void CountEdge(Dictionary<(int, int), int> edgeCount, int v0, int v1)
    {
        int lo = v0 < v1 ? v0 : v1;
        int hi = v0 < v1 ? v1 : v0;
        var key = (lo, hi);
        if (edgeCount.ContainsKey(key))
            edgeCount[key]++;
        else
            edgeCount[key] = 1;
    }

    private static int MaxEdgeUse(Dictionary<(int, int), int> edgeCount, int i0, int i1, int i2)
    {
        int max = 0;
        max = System.Math.Max(max, GetEdgeCount(edgeCount, i0, i1));
        max = System.Math.Max(max, GetEdgeCount(edgeCount, i1, i2));
        max = System.Math.Max(max, GetEdgeCount(edgeCount, i2, i0));
        return max;
    }

    private static int GetEdgeCount(Dictionary<(int, int), int> edgeCount, int v0, int v1)
    {
        int lo = v0 < v1 ? v0 : v1;
        int hi = v0 < v1 ? v1 : v0;
        return edgeCount.TryGetValue((lo, hi), out int c) ? c : 0;
    }

    private static bool HasSameEdge(int ci0, int ci1, int ci2, int ni0, int ni1, int ni2)
    {
        int shared = 0;
        if (ci0 == ni0 || ci0 == ni1 || ci0 == ni2) shared++;
        if (ci1 == ni0 || ci1 == ni1 || ci1 == ni2) shared++;
        if (ci2 == ni0 || ci2 == ni1 || ci2 == ni2) shared++;
        return shared >= 2;
    }

    private static Vector3D ComputeFaceNormal(ImmutableArray<Point3D> vertices, int i0, int i1, int i2)
    {
        double ax = vertices[i1].X - vertices[i0].X;
        double ay = vertices[i1].Y - vertices[i0].Y;
        double az = vertices[i1].Z - vertices[i0].Z;
        double bx = vertices[i2].X - vertices[i0].X;
        double by = vertices[i2].Y - vertices[i0].Y;
        double bz = vertices[i2].Z - vertices[i0].Z;
        double nx = ay * bz - az * by;
        double ny = az * bx - ax * bz;
        double nz = ax * by - ay * bx;
        double len = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);
        if (len < Tolerance) return Vector3D.Zero;
        double inv = 1.0 / len;
        return new Vector3D(nx * inv, ny * inv, nz * inv);
    }
}
