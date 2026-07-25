using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Collision;

/// <summary>
/// Provides spatial hashing for broad-phase collision detection.
/// </summary>
public class SpatialHashGrid
{
    private const double Tolerance = 1e-10;
    private readonly double _cellSize;
    private readonly double _inverseCellSize;
    private readonly Dictionary<(int, int, int), List<int>> _grid = new();
    private readonly Dictionary<int, BoundingBox3D> _objects = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SpatialHashGrid"/> class.
    /// </summary>
    /// <param name="cellSize">The size of each spatial hash cell.</param>
    public SpatialHashGrid(double cellSize)
    {
        _cellSize = cellSize;
        _inverseCellSize = 1.0 / cellSize;
    }

    /// <summary>
    /// Clears all objects from the spatial hash grid.
    /// </summary>
    public void Clear()
    {
        _grid.Clear();
        _objects.Clear();
    }

    /// <summary>
    /// Inserts an object with an ID and its bounding box into the grid.
    /// </summary>
    /// <param name="id">The unique identifier of the object.</param>
    /// <param name="bounds">The axis-aligned bounding box of the object.</param>
    public void Insert(int id, BoundingBox3D bounds)
    {
        _objects[id] = bounds;

        int minX = (int)System.Math.Floor(bounds.Min.X * _inverseCellSize);
        int minY = (int)System.Math.Floor(bounds.Min.Y * _inverseCellSize);
        int minZ = (int)System.Math.Floor(bounds.Min.Z * _inverseCellSize);
        int maxX = (int)System.Math.Floor(bounds.Max.X * _inverseCellSize);
        int maxY = (int)System.Math.Floor(bounds.Max.Y * _inverseCellSize);
        int maxZ = (int)System.Math.Floor(bounds.Max.Z * _inverseCellSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    var key = (x, y, z);

                    if (!_grid.TryGetValue(key, out var cell))
                    {
                        cell = new List<int>();
                        _grid[key] = cell;
                    }

                    if (!cell.Contains(id))
                    {
                        cell.Add(id);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Queries the grid for all potential collision pairs.
    /// </summary>
    /// <returns>An immutable array of <see cref="BroadPhasePair"/>.</returns>
    public ImmutableArray<BroadPhasePair> QueryPairs()
    {
        var pairs = new HashSet<(int, int)>();

        foreach (var cell in _grid.Values)
        {
            for (int i = 0; i < cell.Count; i++)
            {
                for (int j = i + 1; j < cell.Count; j++)
                {
                    int a = System.Math.Min(cell[i], cell[j]);
                    int b = System.Math.Max(cell[i], cell[j]);

                    if (a == b)
                    {
                        continue;
                    }

                    if (_objects.ContainsKey(a) && _objects.ContainsKey(b) && Overlaps(a, b))
                    {
                        pairs.Add((a, b));
                    }
                }
            }
        }

        var builder = ImmutableArray.CreateBuilder<BroadPhasePair>();

        foreach (var pair in pairs)
        {
            builder.Add(new BroadPhasePair(pair.Item1, pair.Item2));
        }

        return builder.ToImmutable();
    }

    private bool Overlaps(int idA, int idB)
    {
        BoundingBox3D a = _objects[idA];
        BoundingBox3D b = _objects[idB];

        return a.Min.X <= b.Max.X && a.Max.X >= b.Min.X &&
               a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y &&
               a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z;
    }
}