using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.MeshProcessing;

/// <summary>Provides Laplacian and HC Laplacian mesh smoothing algorithms for vertex position refinement.</summary>
public static class LaplacianSmoother
{
    private const double Tolerance = 1e-10;

    /// <summary>Applies standard Laplacian smoothing to the mesh vertices.
    /// Each vertex is moved toward the weighted average of its neighbors, blended by the smoothing factor.</summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="iterations">The number of smoothing iterations to perform.</param>
    /// <param name="lambda">The blending factor between the original and smoothed position (0 to 1).</param>
    /// <returns>The smoothed vertex positions.</returns>
    public static ImmutableArray<Point3D> Smooth(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, int iterations, double lambda = 0.5)
    {
        if (vertices.Length == 0 || iterations <= 0)
            return vertices;

        var adjacency = BuildVertexAdjacency(vertices.Length, indices);
        var current = vertices.ToArray();
        var smoothed = new Point3D[current.Length];

        for (int iter = 0; iter < iterations; iter++)
        {
            for (int v = 0; v < current.Length; v++)
            {
                HashSet<int> neighbors = adjacency[v];
                if (neighbors.Count == 0)
                {
                    smoothed[v] = current[v];
                    continue;
                }
                double cx = 0, cy = 0, cz = 0;
                foreach (int n in neighbors)
                {
                    cx += current[n].X;
                    cy += current[n].Y;
                    cz += current[n].Z;
                }
                double inv = 1.0 / neighbors.Count;
                cx *= inv; cy *= inv; cz *= inv;
                smoothed[v] = new Point3D(
                    current[v].X + lambda * (cx - current[v].X),
                    current[v].Y + lambda * (cy - current[v].Y),
                    current[v].Z + lambda * (cz - current[v].Z));
            }
            for (int v = 0; v < current.Length; v++)
                current[v] = smoothed[v];
        }

        return ImmutableArray.Create(current);
    }

    /// <summary>Applies HC (Humphrey's Classes) Laplacian smoothing that preserves mesh volume.
    /// Corrects the drift caused by standard Laplacian smoothing by blending back toward original positions.</summary>
    /// <param name="vertices">The vertex positions of the mesh.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="iterations">The number of smoothing iterations to perform.</param>
    /// <param name="lambda">The blending factor between the original and smoothed position (0 to 1).</param>
    /// <returns>The volume-preserving smoothed vertex positions.</returns>
    public static ImmutableArray<Point3D> SmoothHC(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, int iterations, double lambda = 0.5)
    {
        if (vertices.Length == 0 || iterations <= 0)
            return vertices;

        var adjacency = BuildVertexAdjacency(vertices.Length, indices);
        var original = vertices.ToArray();
        var current = vertices.ToArray();
        var smoothed = new Point3D[current.Length];
        var b = new Point3D[current.Length];

        for (int iter = 0; iter < iterations; iter++)
        {
            for (int v = 0; v < current.Length; v++)
            {
                HashSet<int> neighbors = adjacency[v];
                if (neighbors.Count == 0)
                {
                    smoothed[v] = current[v];
                    continue;
                }
                double cx = 0, cy = 0, cz = 0;
                foreach (int n in neighbors)
                {
                    cx += current[n].X;
                    cy += current[n].Y;
                    cz += current[n].Z;
                }
                double inv = 1.0 / neighbors.Count;
                smoothed[v] = new Point3D(cx * inv, cy * inv, cz * inv);
            }

            for (int v = 0; v < current.Length; v++)
            {
                b[v] = new Point3D(
                    smoothed[v].X - original[v].X,
                    smoothed[v].Y - original[v].Y,
                    smoothed[v].Z - original[v].Z);
            }

            for (int v = 0; v < current.Length; v++)
            {
                HashSet<int> neighbors = adjacency[v];
                if (neighbors.Count == 0)
                {
                    current[v] = smoothed[v];
                    continue;
                }
                double bx = 0, by = 0, bz = 0;
                foreach (int n in neighbors)
                {
                    bx += b[n].X;
                    by += b[n].Y;
                    bz += b[n].Z;
                }
                double inv = 1.0 / neighbors.Count;
                bx *= inv; by *= inv; bz *= inv;
                current[v] = new Point3D(
                    smoothed[v].X + lambda * (original[v].X - bx),
                    smoothed[v].Y + lambda * (original[v].Y - by),
                    smoothed[v].Z + lambda * (original[v].Z - bz));
            }
        }

        return ImmutableArray.Create(current);
    }

    private static List<HashSet<int>> BuildVertexAdjacency(int vertexCount, ImmutableArray<int> indices)
    {
        var adjacency = new List<HashSet<int>>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            adjacency.Add(new HashSet<int>());
        for (int i = 0; i + 2 < indices.Length; i += 3)
        {
            int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
            if (i0 < vertexCount && i1 < vertexCount && i2 < vertexCount)
            {
                adjacency[i0].Add(i1); adjacency[i0].Add(i2);
                adjacency[i1].Add(i0); adjacency[i1].Add(i2);
                adjacency[i2].Add(i0); adjacency[i2].Add(i1);
            }
        }
        return adjacency;
    }
}
