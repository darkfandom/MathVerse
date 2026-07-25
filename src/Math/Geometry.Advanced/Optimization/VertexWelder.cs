using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Optimization;

/// <summary>
/// Merges vertices within a tolerance distance using a spatial hash grid.
/// </summary>
public static class VertexWelder
{
    private const double DefaultTolerance = 1e-10;

    /// <summary>
    /// Welds 3D vertices that are within the specified tolerance distance.
    /// Uses a spatial hash grid for efficient proximity lookups and updates the
    /// index buffer to reference the merged canonical vertices.
    /// </summary>
    /// <param name="vertices">The input vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="tolerance">Maximum distance between vertices to be merged.</param>
    /// <returns>
    /// A tuple containing the welded vertex array, updated index buffer,
    /// and the count of vertices removed by the welding process.
    /// </returns>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices, int RemovedCount) Weld(
        ImmutableArray<Point3D> vertices,
        ImmutableArray<int> indices,
        double tolerance)
    {
        if (vertices.Length == 0 || indices.Length == 0)
            return (vertices, indices, 0);

        double cellSize = tolerance * 2.0;
        var grid = new Dictionary<(int, int, int), int>();
        var canonical = new int[vertices.Length];
        var merged = ImmutableArray.CreateBuilder<Point3D>(vertices.Length);
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
                        if (grid.TryGetValue(key, out int candidate))
                        {
                            Point3D cv = merged[candidate];
                            double distSq = (v.X - cv.X) * (v.X - cv.X)
                                          + (v.Y - cv.Y) * (v.Y - cv.Y)
                                          + (v.Z - cv.Z) * (v.Z - cv.Z);
                            if (distSq <= tolerance * tolerance)
                            {
                                canonical[i] = candidate;
                                found = true;
                                removedCount++;
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                canonical[i] = merged.Count;
                grid[(cx, cy, cz)] = merged.Count;
                merged.Add(v);
            }
        }

        var newIndices = ImmutableArray.CreateBuilder<int>(indices.Length);
        for (int i = 0; i < indices.Length; i++)
            newIndices.Add(canonical[indices[i]]);

        return (merged.ToImmutable(), newIndices.ToImmutable(), removedCount);
    }

    /// <summary>
    /// Welds 2D vertices that are within the specified tolerance distance.
    /// Uses a spatial hash grid for efficient proximity lookups and updates the
    /// index buffer to reference the merged canonical vertices.
    /// </summary>
    /// <param name="vertices">The input 2D vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="tolerance">Maximum distance between vertices to be merged.</param>
    /// <returns>
    /// A tuple containing the welded vertex array, updated index buffer,
    /// and the count of vertices removed by the welding process.
    /// </returns>
    public static (ImmutableArray<Point2D> Vertices, ImmutableArray<int> Indices, int RemovedCount) Weld2D(
        ImmutableArray<Point2D> vertices,
        ImmutableArray<int> indices,
        double tolerance)
    {
        if (vertices.Length == 0 || indices.Length == 0)
            return (vertices, indices, 0);

        double cellSize = tolerance * 2.0;
        var grid = new Dictionary<(int, int), int>();
        var canonical = new int[vertices.Length];
        var merged = ImmutableArray.CreateBuilder<Point2D>(vertices.Length);
        int removedCount = 0;

        for (int i = 0; i < vertices.Length; i++)
        {
            Point2D v = vertices[i];
            (int cx, int cy) = CellKey2D(v, cellSize);
            bool found = false;

            for (int dx = -1; dx <= 1 && !found; dx++)
            {
                for (int dy = -1; dy <= 1 && !found; dy++)
                {
                    var key = (cx + dx, cy + dy);
                    if (grid.TryGetValue(key, out int candidate))
                    {
                        Point2D cv = merged[candidate];
                        double distSq = (v.X - cv.X) * (v.X - cv.X)
                                      + (v.Y - cv.Y) * (v.Y - cv.Y);
                        if (distSq <= tolerance * tolerance)
                        {
                            canonical[i] = candidate;
                            found = true;
                            removedCount++;
                        }
                    }
                }
            }

            if (!found)
            {
                canonical[i] = merged.Count;
                grid[(cx, cy)] = merged.Count;
                merged.Add(v);
            }
        }

        var newIndices = ImmutableArray.CreateBuilder<int>(indices.Length);
        for (int i = 0; i < indices.Length; i++)
            newIndices.Add(canonical[indices[i]]);

        return (merged.ToImmutable(), newIndices.ToImmutable(), removedCount);
    }

    private static (int, int, int) CellKey(Point3D v, double cellSize)
    {
        int cx = v.X >= 0 ? (int)System.Math.Floor(v.X / cellSize) : (int)System.Math.Ceiling(v.X / cellSize) - 1;
        int cy = v.Y >= 0 ? (int)System.Math.Floor(v.Y / cellSize) : (int)System.Math.Ceiling(v.Y / cellSize) - 1;
        int cz = v.Z >= 0 ? (int)System.Math.Floor(v.Z / cellSize) : (int)System.Math.Ceiling(v.Z / cellSize) - 1;
        return (cx, cy, cz);
    }

    private static (int, int) CellKey2D(Point2D v, double cellSize)
    {
        int cx = v.X >= 0 ? (int)System.Math.Floor(v.X / cellSize) : (int)System.Math.Ceiling(v.X / cellSize) - 1;
        int cy = v.Y >= 0 ? (int)System.Math.Floor(v.Y / cellSize) : (int)System.Math.Ceiling(v.Y / cellSize) - 1;
        return (cx, cy);
    }
}
