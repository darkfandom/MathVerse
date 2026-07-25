using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>Provides advanced mesh optimization operations including vertex welding, degenerate triangle removal,
/// and topology optimization for improved rendering performance.</summary>
public static class MeshOptimizerAdvanced
{
    private const double Tolerance = 1e-10;

    /// <summary>Merges vertices that are within the specified tolerance distance, updating the index buffer accordingly.</summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="tolerance">The maximum distance between vertices for them to be considered duplicates.</param>
    /// <returns>A tuple containing the welded vertex positions and updated index buffer.</returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) WeldVertices(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, double tolerance)
    {
        if (vertices.Length == 0) return (vertices, indices);
        double tolSq = tolerance * tolerance;
        int[] remap = new int[vertices.Length];
        var welded = ImmutableArray.CreateBuilder<Point3D>();
        var vertexMap = new List<int>();
        bool[] used = new bool[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            remap[i] = -1;
            for (int j = 0; j < welded.Count; j++)
            {
                double dx = vertices[i].X - welded[j].X;
                double dy = vertices[i].Y - welded[j].Y;
                double dz = vertices[i].Z - welded[j].Z;
                if (dx * dx + dy * dy + dz * dz <= tolSq)
                {
                    remap[i] = j;
                    break;
                }
            }
            if (remap[i] == -1)
            {
                remap[i] = welded.Count;
                welded.Add(vertices[i]);
            }
        }

        var newIndices = ImmutableArray.CreateBuilder<int>(indices.Length);
        for (int i = 0; i < indices.Length; i++)
            newIndices.Add(remap[indices[i]]);

        return (welded.ToImmutable(), newIndices.ToImmutable());
    }

    /// <summary>Removes triangles with area below the specified minimum threshold.</summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="minArea">The minimum triangle area threshold.</param>
    /// <returns>A tuple containing the vertex positions (unchanged) and the filtered index buffer.</returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) RemoveDegenerateTriangles(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, double minArea)
    {
        var newIndices = ImmutableArray.CreateBuilder<int>();
        int triCount = indices.Length / 3;
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
            if (area >= minArea)
            {
                newIndices.Add(i0);
                newIndices.Add(i1);
                newIndices.Add(i2);
            }
        }
        return (vertices, newIndices.ToImmutable());
    }

    /// <summary>Optimizes the mesh topology by reordering indices for better vertex cache locality using a
    /// Forsyth-style algorithm, improving GPU rendering performance.</summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>A tuple containing the vertex positions and the reordered index buffer.</returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) OptimizeTopology(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        if (indices.Length == 0) return (vertices, indices);
        int triCount = indices.Length / 3;
        var triScores = new double[triCount];
        var triAdded = new bool[triCount];
        var vertexCount = new int[vertices.Length];
        var vertexScore = new double[vertices.Length];
        var inCache = new bool[vertices.Length];

        for (int t = 0; t < triCount; t++)
        {
            for (int k = 0; k < 3; k++)
                vertexCount[indices[t * 3 + k]]++;
        }

        const int CacheSize = 16;
        const int CacheMissRateDecay = 2;
        const int CacheMissScore = 7;
        var lruCache = new int[CacheSize];
        for (int i = 0; i < CacheSize; i++) lruCache[i] = -1;

        for (int t = 0; t < triCount; t++)
        {
            triScores[t] = 0;
            for (int k = 0; k < 3; k++)
            {
                int v = indices[t * 3 + k];
                triScores[t] += vertexScore[v];
            }
        }

        var result = ImmutableArray.CreateBuilder<int>(indices.Length);
        int remaining = triCount;

        while (remaining > 0)
        {
            int bestTri = -1;
            double bestScore = double.MinValue;
            for (int t = 0; t < triCount; t++)
            {
                if (triAdded[t]) continue;
                if (triScores[t] > bestScore)
                {
                    bestScore = triScores[t];
                    bestTri = t;
                }
            }
            if (bestTri < 0) break;

            triAdded[bestTri] = true;
            remaining--;

            for (int k = 0; k < 3; k++)
                result.Add(indices[bestTri * 3 + k]);

            for (int k = 0; k < 3; k++)
            {
                int v = indices[bestTri * 3 + k];
                vertexCount[v]--;
                inCache[v] = true;
                lruCache[v % CacheSize] = v;
                vertexScore[v] = CacheMissScore - vertexCount[v] * CacheMissRateDecay;
                if (vertexScore[v] < 0) vertexScore[v] = 0;
            }

            for (int t = 0; t < triCount; t++)
            {
                if (triAdded[t]) continue;
                triScores[t] = 0;
                for (int k = 0; k < 3; k++)
                    triScores[t] += vertexScore[indices[t * 3 + k]];
            }
        }

        return (vertices, result.ToImmutable());
    }
}
