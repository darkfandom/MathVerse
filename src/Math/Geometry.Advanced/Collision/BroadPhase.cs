using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Collision;

/// <summary>
/// Represents a potential collision pair identified by the broad phase.
/// </summary>
/// <param name="IndexA">The index of the first bounding box.</param>
/// <param name="IndexB">The index of the second bounding box.</param>
public readonly record struct BroadPhasePair(int IndexA, int IndexB);

/// <summary>
/// Provides broad-phase collision detection using sweep-and-prune.
/// </summary>
public class BroadPhase
{
    private const double Tolerance = 1e-10;
    private readonly int _initialCapacity;
    private readonly List<(int index, double min, double max)> _endpointsX = new();
    private readonly List<(int index, double min, double max)> _endpointsY = new();
    private readonly List<(int index, double min, double max)> _endpointsZ = new();
    private readonly HashSet<(int, int)> _potentialPairs = new();
    private ImmutableArray<BoundingBox3D> _aabbs = ImmutableArray<BoundingBox3D>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BroadPhase"/> class.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity for the internal collections.</param>
    public BroadPhase(int initialCapacity = 64)
    {
        _initialCapacity = initialCapacity;
    }

    /// <summary>
    /// Updates the broad phase with a new set of bounding boxes and computes potential collision pairs.
    /// </summary>
    /// <param name="aabbs">The axis-aligned bounding boxes to test.</param>
    public void Update(ImmutableArray<BoundingBox3D> aabbs)
    {
        _aabbs = aabbs;
        _endpointsX.Clear();
        _endpointsY.Clear();
        _endpointsZ.Clear();
        _potentialPairs.Clear();

        for (int i = 0; i < aabbs.Length; i++)
        {
            _endpointsX.Add((i, aabbs[i].Min.X, aabbs[i].Max.X));
            _endpointsY.Add((i, aabbs[i].Min.Y, aabbs[i].Max.Y));
            _endpointsZ.Add((i, aabbs[i].Min.Z, aabbs[i].Max.Z));
        }

        _endpointsX.Sort((a, b) => a.min.CompareTo(b.min));
        _endpointsY.Sort((a, b) => a.min.CompareTo(b.min));
        _endpointsZ.Sort((a, b) => a.min.CompareTo(b.min));

        SweepAndPrune(_endpointsX);
        SweepAndPrune(_endpointsY);
        SweepAndPrune(_endpointsZ);
    }

    /// <summary>
    /// Gets the potential collision pairs identified during the last update.
    /// </summary>
    /// <returns>An immutable array of <see cref="BroadPhasePair"/>.</returns>
    public ImmutableArray<BroadPhasePair> GetPotentialPairs()
    {
        var builder = ImmutableArray.CreateBuilder<BroadPhasePair>();

        foreach (var pair in _potentialPairs)
        {
            builder.Add(new BroadPhasePair(pair.Item1, pair.Item2));
        }

        return builder.ToImmutable();
    }

    private void SweepAndPrune(List<(int index, double min, double max)> endpoints)
    {
        var active = new List<int>();

        for (int i = 0; i < endpoints.Count; i++)
        {
            var current = endpoints[i];

            for (int j = active.Count - 1; j >= 0; j--)
            {
                if (endpoints[active[j]].max < current.min)
                {
                    active.RemoveAt(j);
                }
            }

            for (int j = 0; j < active.Count; j++)
            {
                int a = System.Math.Min(current.index, endpoints[active[j]].index);
                int b = System.Math.Max(current.index, endpoints[active[j]].index);

                if (Overlaps(current.index, endpoints[active[j]].index))
                {
                    _potentialPairs.Add((a, b));
                }
            }

            active.Add(i);
        }
    }

    private bool Overlaps(int indexA, int indexB)
    {
        if (indexA >= _aabbs.Length || indexB >= _aabbs.Length)
        {
            return false;
        }

        BoundingBox3D a = _aabbs[indexA];
        BoundingBox3D b = _aabbs[indexB];

        return a.Min.X <= b.Max.X && a.Max.X >= b.Min.X &&
               a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y &&
               a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z;
    }
}