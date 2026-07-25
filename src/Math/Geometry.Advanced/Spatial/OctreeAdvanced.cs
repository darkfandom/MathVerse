using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.Spatial;

/// <summary>Represents a single entry in an octree with a 3D point and associated identifier.</summary>
/// <param name="Point">The 3D point position.</param>
/// <param name="Id">The unique identifier associated with this entry.</param>
public readonly record struct OctEntry3D(Point3D Point, int Id);

/// <summary>A 3D octree for spatial indexing of <see cref="Point3D"/> data,
/// supporting insertion, AABB range search, and spherical radius search.</summary>
public class Octree3D
{
    private const double Tolerance = 1e-10;
    private readonly int _maxCapacity;
    private readonly OctNode _root;
    private int _nextId;

    /// <summary>Internal node of the octree storing bounds, 8 child octants, and entries.</summary>
    internal sealed class OctNode
    {
        /// <summary>The axis-aligned bounding box of this node.</summary>
        public BoundingBox3D Bounds { get; set; }

        /// <summary>The eight child octants.</summary>
        public OctNode?[] Children { get; set; } = new OctNode?[8];

        /// <summary>The entries stored at this node.</summary>
        public List<OctEntry3D> Entries { get; set; } = new();

        /// <summary>Whether this node has been subdivided.</summary>
        public bool IsDivided { get; set; }
    }

    /// <summary>Initializes a new <see cref="Octree3D"/> with the specified bounds and capacity threshold.</summary>
    /// <param name="bounds">The spatial bounds of the root node.</param>
    /// <param name="maxCapacity">The maximum number of entries per node before subdivision.</param>
    public Octree3D(BoundingBox3D bounds, int maxCapacity = 8)
    {
        _maxCapacity = maxCapacity;
        _root = new OctNode { Bounds = bounds };
        _nextId = 0;
    }

    /// <summary>Inserts a point into the octree, subdividing nodes as needed when capacity is exceeded.</summary>
    /// <param name="point">The point to insert.</param>
    public void Insert(Point3D point)
    {
        int id = _nextId++;
        InsertRecursive(_root, new OctEntry3D(point, id));
    }

    /// <summary>Finds all point indices within the given axis-aligned bounding box range.</summary>
    /// <param name="range">The query bounding box.</param>
    /// <returns>An immutable array of point identifiers that fall within the range.</returns>
    public ImmutableArray<int> RangeSearch(BoundingBox3D range)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        RangeSearchRecursive(_root, range, result);
        return result.ToImmutable();
    }

    /// <summary>Finds all point indices within a spherical radius of the given center point.</summary>
    /// <param name="center">The center of the spherical query region.</param>
    /// <param name="radius">The radius of the spherical query region.</param>
    /// <returns>An immutable array of point identifiers within the spherical region.</returns>
    public ImmutableArray<int> RadiusSearch(Point3D center, double radius)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        double radiusSq = radius * radius;
        RadiusSearchRecursive(_root, center, radiusSq, result);
        return result.ToImmutable();
    }

    /// <summary>Clears all entries from the octree, resetting it to an empty state.</summary>
    public void Clear()
    {
        _root.Entries.Clear();
        _root.IsDivided = false;
        for (int i = 0; i < 8; i++)
            _root.Children[i] = null;
        _nextId = 0;
    }

    private static int GetOctant(BoundingBox3D parentBounds, Point3D point)
    {
        Point3D center = parentBounds.Center;
        int index = 0;
        if (point.X >= center.X) index |= 1;
        if (point.Y >= center.Y) index |= 2;
        if (point.Z >= center.Z) index |= 4;
        return index;
    }

    private void InsertRecursive(OctNode node, OctEntry3D entry)
    {
        if (!node.IsDivided)
        {
            node.Entries.Add(entry);
            if (node.Entries.Count > _maxCapacity &&
                node.Bounds.Width > Tolerance && node.Bounds.Height > Tolerance && node.Bounds.Depth > Tolerance)
                Subdivide(node);
            return;
        }
        int octant = GetOctant(node.Bounds, entry.Point);
        if (node.Children[octant] != null)
            InsertRecursive(node.Children[octant]!, entry);
    }

    private void Subdivide(OctNode node)
    {
        Point3D center = node.Bounds.Center;
        Point3D min = node.Bounds.Min;
        Point3D max = node.Bounds.Max;
        for (int i = 0; i < 8; i++)
        {
            Point3D childMin = new Point3D(
                (i & 1) != 0 ? center.X : min.X,
                (i & 2) != 0 ? center.Y : min.Y,
                (i & 4) != 0 ? center.Z : min.Z);
            Point3D childMax = new Point3D(
                (i & 1) != 0 ? max.X : center.X,
                (i & 2) != 0 ? max.Y : center.Y,
                (i & 4) != 0 ? max.Z : center.Z);
            node.Children[i] = new OctNode { Bounds = new BoundingBox3D(childMin, childMax) };
        }
        node.IsDivided = true;
        List<OctEntry3D> entries = node.Entries;
        node.Entries = new List<OctEntry3D>();
        foreach (OctEntry3D e in entries)
        {
            int octant = GetOctant(node.Bounds, e.Point);
            node.Children[octant]!.Entries.Add(e);
        }
    }

    private void RangeSearchRecursive(OctNode node, BoundingBox3D range, ImmutableArray<int>.Builder result)
    {
        if (!node.Bounds.Intersects(range))
            return;
        if (!node.IsDivided)
        {
            foreach (OctEntry3D entry in node.Entries)
            {
                if (range.Contains(entry.Point))
                    result.Add(entry.Id);
            }
            return;
        }
        for (int i = 0; i < 8; i++)
        {
            if (node.Children[i] != null)
                RangeSearchRecursive(node.Children[i]!, range, result);
        }
    }

    private void RadiusSearchRecursive(OctNode node, Point3D center, double radiusSq, ImmutableArray<int>.Builder result)
    {
        Point3D closest = ClampToBox(center, node.Bounds);
        double dx = center.X - closest.X;
        double dy = center.Y - closest.Y;
        double dz = center.Z - closest.Z;
        if (dx * dx + dy * dy + dz * dz > radiusSq)
            return;
        if (!node.IsDivided)
        {
            foreach (OctEntry3D entry in node.Entries)
            {
                double ex = entry.Point.X - center.X;
                double ey = entry.Point.Y - center.Y;
                double ez = entry.Point.Z - center.Z;
                if (ex * ex + ey * ey + ez * ez <= radiusSq)
                    result.Add(entry.Id);
            }
            return;
        }
        for (int i = 0; i < 8; i++)
        {
            if (node.Children[i] != null)
                RadiusSearchRecursive(node.Children[i]!, center, radiusSq, result);
        }
    }

    private static Point3D ClampToBox(Point3D point, BoundingBox3D bounds)
    {
        double x = System.Math.Max(bounds.Min.X, System.Math.Min(bounds.Max.X, point.X));
        double y = System.Math.Max(bounds.Min.Y, System.Math.Min(bounds.Max.Y, point.Y));
        double z = System.Math.Max(bounds.Min.Z, System.Math.Min(bounds.Max.Z, point.Z));
        return new Point3D(x, y, z);
    }
}
