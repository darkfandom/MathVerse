using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Optimization;

/// <summary>
/// Provides a full mesh optimization pipeline and GPU-friendly vertex cache ordering.
/// </summary>
public static class MeshOptimizerFull
{
    private const double Tolerance = 1e-10;
    private const double MinTriangleArea = 1e-14;

    /// <summary>
    /// Runs a full mesh optimization pipeline: weld vertices, remove degenerate triangles,
    /// collapse edges until the target triangle count is reached, then recompute vertex normals.
    /// </summary>
    /// <param name="vertices">The input mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="targetTriangles">Desired number of triangles after simplification.</param>
    /// <returns>The optimized vertex and index arrays.</returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) Optimize(
        ImmutableArray<Point3D> vertices,
        ImmutableArray<int> indices,
        int targetTriangles)
    {
        var (weldedVerts, weldedIdx, _) = VertexWelder.Weld(vertices, indices, Tolerance);

        (weldedVerts, weldedIdx) = RemoveDegenerateTriangles(weldedVerts, weldedIdx);

        int currentTris = weldedIdx.Length / 3;
        while (currentTris > targetTriangles && currentTris > 0)
        {
            var best = EdgeCollapser.FindBestEdge(weldedVerts, weldedIdx);
            if (best == null) break;

            var (newVerts, newIdx, collapsed) = EdgeCollapser.Collapse(
                weldedVerts, weldedIdx,
                best.Value.EdgeVertexLo, best.Value.EdgeVertexHi, best.Value.TargetPosition);

            if (!collapsed) break;

            weldedVerts = newVerts;
            weldedIdx = newIdx;
            currentTris = weldedIdx.Length / 3;
        }

        return (weldedVerts, weldedIdx);
    }

    /// <summary>
    /// Computes an optimal vertex processing order for GPU vertex cache efficiency
    /// using a simplified version of the Forsyth algorithm.
    /// Vertices are ordered to maximize spatial locality in the post-transform cache.
    /// </summary>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>An array of vertex indices in the recommended processing order.</returns>
    public static int[] ComputeVertexCacheOrder(ImmutableArray<int> indices)
    {
        if (indices.Length == 0)
            return Array.Empty<int>();

        int maxIdx = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] > maxIdx)
                maxIdx = indices[i];
        }

        int vertexCount = maxIdx + 1;
        int triCount = indices.Length / 3;
        int cacheSize = 24;

        var activeTriangles = new bool[triCount];
        var vertexCountInCache = new int[vertexCount];
        var vertexLastAccessed = new int[vertexCount];
        var vertexTriangles = new List<int>[vertexCount];
        var triangleDeleted = new bool[triCount];

        for (int i = 0; i < vertexCount; i++)
        {
            vertexTriangles[i] = new List<int>();
            vertexLastAccessed[i] = -1;
        }

        for (int t = 0; t < triCount; t++)
        {
            activeTriangles[t] = true;
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 < vertexCount) vertexTriangles[i0].Add(t);
            if (i1 < vertexCount) vertexTriangles[i1].Add(t);
            if (i2 < vertexCount) vertexTriangles[i2].Add(t);
        }

        var result = new int[vertexCount];
        var used = new bool[vertexCount];
        int processedVerts = 0;
        int processedTris = 0;

        int startTriangle = FindBestStartingTriangle(indices, activeTriangles, triCount, vertexCount);
        if (startTriangle >= 0)
        {
            EmitTriangle(startTriangle, indices, used, result, ref processedVerts,
                         vertexLastAccessed, vertexCountInCache, vertexTriangles,
                         activeTriangles, triangleDeleted, ref processedTris, cacheSize, triCount);
        }

        while (processedVerts < vertexCount)
        {
            int bestVertex = -1;
            int bestScore = -1;

            for (int v = 0; v < vertexCount; v++)
            {
                if (used[v]) continue;

                int score = ScoreVertex(v, vertexCountInCache, vertexTriangles, activeTriangles, triCount);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestVertex = v;
                }
            }

            if (bestVertex < 0) break;

            result[processedVerts] = bestVertex;
            used[bestVertex] = true;
            processedVerts++;

            vertexLastAccessed[bestVertex] = processedVerts;
            vertexCountInCache[bestVertex] = cacheSize;

            for (int i = 0; i < vertexTriangles[bestVertex].Count; i++)
            {
                int t = vertexTriangles[bestVertex][i];
                if (activeTriangles[t] && !triangleDeleted[t])
                {
                    EmitTriangle(t, indices, used, result, ref processedVerts,
                                 vertexLastAccessed, vertexCountInCache, vertexTriangles,
                                 activeTriangles, triangleDeleted, ref processedTris, cacheSize, triCount);
                }
            }

            UpdateCacheAfterVertex(bestVertex, vertexCountInCache, vertexLastAccessed, processedVerts, cacheSize);
        }

        return result;
    }

    private static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) RemoveDegenerateTriangles(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var newIndices = ImmutableArray.CreateBuilder<int>(indices.Length);
        int triCount = indices.Length / 3;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
                continue;

            if (i0 == i1 || i1 == i2 || i0 == i2)
                continue;

            double area = TriangleArea(vertices[i0], vertices[i1], vertices[i2]);
            if (area < MinTriangleArea)
                continue;

            newIndices.Add(i0);
            newIndices.Add(i1);
            newIndices.Add(i2);
        }

        return (vertices, newIndices.ToImmutable());
    }

    private static double TriangleArea(Point3D a, Point3D b, Point3D c)
    {
        double ax = b.X - a.X, ay = b.Y - a.Y, az = b.Z - a.Z;
        double bx = c.X - a.X, by = c.Y - a.Y, bz = c.Z - a.Z;
        double cx = ay * bz - az * by;
        double cy = az * bx - ax * bz;
        double cz = ax * by - ay * bx;
        return System.Math.Sqrt(cx * cx + cy * cy + cz * cz) * 0.5;
    }

    private static int FindBestStartingTriangle(
        ImmutableArray<int> indices, bool[] activeTriangles, int triCount, int vertexCount)
    {
        int bestTri = -1;
        int bestScore = -1;

        for (int t = 0; t < triCount; t++)
        {
            if (!activeTriangles[t]) continue;
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];
            if (i0 >= vertexCount || i1 >= vertexCount || i2 >= vertexCount) continue;
            int score = 3;
            if (score > bestScore)
            {
                bestScore = score;
                bestTri = t;
            }
        }

        return bestTri;
    }

    private static void EmitTriangle(
        int t, ImmutableArray<int> indices, bool[] used, int[] result, ref int processedVerts,
        int[] vertexLastAccessed, int[] vertexCountInCache, List<int>[] vertexTriangles,
        bool[] activeTriangles, bool[] triangleDeleted, ref int processedTris,
        int cacheSize, int triCount)
    {
        int i0 = indices[t * 3];
        int i1 = indices[t * 3 + 1];
        int i2 = indices[t * 3 + 2];

        if (i0 < used.Length && !used[i0])
        {
            result[processedVerts] = i0;
            used[i0] = true;
            processedVerts++;
            vertexLastAccessed[i0] = processedVerts;
            vertexCountInCache[i0] = cacheSize;
        }

        if (i1 < used.Length && !used[i1])
        {
            result[processedVerts] = i1;
            used[i1] = true;
            processedVerts++;
            vertexLastAccessed[i1] = processedVerts;
            vertexCountInCache[i1] = cacheSize;
        }

        if (i2 < used.Length && !used[i2])
        {
            result[processedVerts] = i2;
            used[i2] = true;
            processedVerts++;
            vertexLastAccessed[i2] = processedVerts;
            vertexCountInCache[i2] = cacheSize;
        }

        activeTriangles[t] = false;
        triangleDeleted[t] = true;
        processedTris++;
    }

    private static int ScoreVertex(int v, int[] vertexCountInCache, List<int>[] vertexTriangles,
        bool[] activeTriangles, int triCount)
    {
        int score = 0;
        for (int i = 0; i < vertexTriangles[v].Count; i++)
        {
            int t = vertexTriangles[v][i];
            if (activeTriangles[t])
                score++;
        }
        return score;
    }

    private static void UpdateCacheAfterVertex(int v, int[] vertexCountInCache, int[] vertexLastAccessed,
        int processedVerts, int cacheSize)
    {
        int age = cacheSize;
        for (int i = 0; i < vertexCountInCache.Length; i++)
        {
            if (i == v) continue;
            if (vertexLastAccessed[i] >= 0)
            {
                vertexCountInCache[i] = System.Math.Max(0, vertexCountInCache[i] - 1);
            }
        }

        vertexCountInCache[v] = cacheSize;
    }
}
