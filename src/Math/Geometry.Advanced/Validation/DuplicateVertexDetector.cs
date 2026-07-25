using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Validation;

/// <summary>
/// Describes a pair of duplicate vertices found within tolerance.
/// </summary>
/// <param name="IndexA">The index of the first vertex in the pair.</param>
/// <param name="IndexB">The index of the second vertex in the pair.</param>
/// <param name="Distance">The Euclidean distance between the two vertices.</param>
public readonly record struct DuplicatePair(int IndexA, int IndexB, double Distance);

/// <summary>
/// Detects and removes duplicate vertices in 3D meshes using spatial hashing.
/// </summary>
public static class DuplicateVertexDetector
{
    private const double DefaultTolerance = 1e-10;

    /// <summary>
    /// Finds all pairs of vertices within the specified tolerance distance.
    /// Uses spatial hashing for efficient proximity lookups.
    /// </summary>
    /// <param name="vertices">The vertex positions to search.</param>
    /// <param name="tolerance">Maximum distance between vertices to be considered duplicates.</param>
    /// <returns>An immutable array of duplicate vertex pairs with distances.</returns>
    public static ImmutableArray<DuplicatePair> FindDuplicates(
        ImmutableArray<Point3D> vertices, double tolerance)
    {
        var results = ImmutableArray.CreateBuilder<DuplicatePair>();
        if (vertices.Length < 2) return results.ToImmutable();

        double cellSize = tolerance * 2.0;
        var grid = new Dictionary<(int, int, int), List<int>>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Point3D v = vertices[i];
            (int cx, int cy, int cz) = CellKey(v, cellSize);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        var key = (cx + dx, cy + dy, cz + dz);
                        if (grid.TryGetValue(key, out var candidates))
                        {
                            foreach (int j in candidates)
                            {
                                if (j >= i) continue;
                                double dist = v.DistanceTo(vertices[j]);
                                if (dist <= tolerance)
                                    results.Add(new DuplicatePair(j, i, dist));
                            }
                        }
                    }
                }
            }

            var cellKey = (cx, cy, cz);
            if (!grid.ContainsKey(cellKey))
                grid[cellKey] = new List<int>();
            grid[cellKey].Add(i);
        }

        return results.ToImmutable();
    }

    /// <summary>
    /// Removes duplicate vertices and updates the index buffer to reference the surviving vertices.
    /// When duplicates are found, the first occurrence is kept and all later occurrences
    /// are remapped to the first.
    /// </summary>
    /// <param name="vertices">The vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="tolerance">Maximum distance between vertices to be considered duplicates.</param>
    /// <returns>
    /// A tuple containing the deduplicated vertex array, remapped index buffer,
    /// and the count of vertices removed.
    /// </returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> NewIndices, int RemovedCount) RemoveDuplicates(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, double tolerance)
    {
        if (vertices.Length == 0)
            return (vertices, indices, 0);

        double cellSize = tolerance * 2.0;
        var canonical = new int[vertices.Length];
        var grid = new Dictionary<(int, int, int), List<int>>();
        var uniqueVertices = ImmutableArray.CreateBuilder<Point3D>(vertices.Length);
        int removedCount = 0;

        for (int i = 0; i < vertices.Length; i++)
        {
            Point3D v = vertices[i];
            (int cx, int cy, int cz) = CellKey(v, cellSize);
            bool found = false;

            for (int dx = -1; dx <= 1 && !found; dx++)
            {
                for (int dy = -1; dy <= 1 && !found; dy++)
                {
                    for (int dz = -1; dz <= 1 && !found; dz++)
                    {
                        var key = (cx + dx, cy + dy, cz + dz);
                        if (grid.TryGetValue(key, out var candidates))
                        {
                            foreach (int j in candidates)
                            {
                                double dist = v.DistanceTo(uniqueVertices[j]);
                                if (dist <= tolerance)
                                {
                                    canonical[i] = j;
                                    found = true;
                                    removedCount++;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                canonical[i] = uniqueVertices.Count;
                var cellKey = (cx, cy, cz);
                if (!grid.ContainsKey(cellKey))
                    grid[cellKey] = new List<int>();
                grid[cellKey].Add(uniqueVertices.Count);
                uniqueVertices.Add(v);
            }
        }

        var newIndices = ImmutableArray.CreateBuilder<int>(indices.Length);
        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] >= 0 && indices[i] < vertices.Length)
                newIndices.Add(canonical[indices[i]]);
            else
                newIndices.Add(indices[i]);
        }

        return (uniqueVertices.ToImmutable(), newIndices.ToImmutable(), removedCount);
    }

    private static (int, int, int) CellKey(Point3D v, double cellSize)
    {
        int cx = v.X >= 0 ? (int)System.Math.Floor(v.X / cellSize) : (int)System.Math.Ceiling(v.X / cellSize) - 1;
        int cy = v.Y >= 0 ? (int)System.Math.Floor(v.Y / cellSize) : (int)System.Math.Ceiling(v.Y / cellSize) - 1;
        int cz = v.Z >= 0 ? (int)System.Math.Floor(v.Z / cellSize) : (int)System.Math.Ceiling(v.Z / cellSize) - 1;
        return (cx, cy, cz);
    }
}
