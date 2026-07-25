using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.Spatial;

/// <summary>Represents a single entry in a quad-tree with a 2D point and associated identifier.</summary>
/// <param name="Point">The 2D point position.</param>
/// <param name="Id">The unique identifier associated with this entry.</param>
public readonly record struct QuadEntry2D(Point2D Point, int Id);

/// <summary>A 2D quad-tree for spatial indexing of <see cref="Point2D"/> data,
/// supporting insertion, range search, and circular radius search.</summary>
public class QuadTree2D
{
    private const double Tolerance = 1e-10;
    private readonly int _maxCapacity;
    private readonly QuadNode _root;
    private int _nextId;

    /// <summary>Internal node of the quad-tree storing bounds, child quadrants, and entries.</summary>
    internal sealed class QuadNode
    {
        /// <summary>The axis-aligned bounding box of this node.</summary>
        public BoundingBox2D Bounds { get; set; }

        /// <summary>The four child quadrants (NW, NE, SW, SE).</summary>
        public QuadNode?[] Children { get; set; } = new QuadNode?[4];

        /// <summary>The entries stored at this node.</summary>
        public List<QuadEntry2D> Entries { get; set; } = new();

        /// <summary>Whether this node has been subdivided.</summary>
        public bool IsDivided { get; set; }
    }

    /// <summary>Initializes a new <see cref="QuadTree2D"/> with the specified bounds and capacity threshold.</summary>
    /// <param name="bounds">The spatial bounds of the root node.</param>
    /// <param name="maxCapacity">The maximum number of entries per node before subdivision.</param>
    public QuadTree2D(BoundingBox2D bounds, int maxCapacity = 4)
    {
        _maxCapacity = maxCapacity;
        _root = new QuadNode { Bounds = bounds };
        _nextId = 0;
    }

    /// <summary>Inserts a point into the quad-tree, subdividing nodes as needed when capacity is exceeded.</summary>
    /// <param name="point">The point to insert.</param>
    public void Insert(Point2D point)
    {
        int id = _nextId++;
        InsertRecursive(_root, new QuadEntry2D(point, id));
    }

    /// <summary>Finds all point indices within the given axis-aligned bounding box range.</summary>
    /// <param name="range">The query bounding box.</param>
    /// <returns>An immutable array of point identifiers that fall within the range.</returns>
    public ImmutableArray<int> RangeSearch(BoundingBox2D range)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        RangeSearchRecursive(_root, range, result);
        return result.ToImmutable();
    }

    /// <summary>Finds all point indices within a circular radius of the given center point.</summary>
    /// <param name="center">The center of the circular query region.</param>
    /// <param name="radius">The radius of the circular query region.</param>
    /// <returns>An immutable array of point identifiers within the circular region.</returns>
    public ImmutableArray<int> RadiusSearch(Point2D center, double radius)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        double radiusSq = radius * radius;
        RadiusSearchRecursive(_root, center, radiusSq, result);
        return result.ToImmutable();
    }

    /// <summary>Clears all entries from the quad-tree, resetting it to an empty state.</summary>
    public void Clear()
    {
        _root.Entries.Clear();
        _root.IsDivided = false;
        for (int i = 0; i < 4; i++)
            _root.Children[i] = null;
        _nextId = 0;
    }

    private static int GetQuadrant(BoundingBox2D parentBounds, Point2D point)
    {
        Point2D center = parentBounds.Center;
        bool top = point.Y >= center.Y;
        bool right = point.X >= center.X;
        return (top ? 0 : 2) + (right ? 1 : 0);
    }

    private void InsertRecursive(QuadNode node, QuadEntry2D entry)
    {
        if (!node.IsDivided)
        {
            node.Entries.Add(entry);
            if (node.Entries.Count > _maxCapacity && node.Bounds.Width > Tolerance && node.Bounds.Height > Tolerance)
                Subdivide(node);
            return;
        }
        int quadrant = GetQuadrant(node.Bounds, entry.Point);
        if (node.Children[quadrant] != null)
            InsertRecursive(node.Children[quadrant]!, entry);
    }

    private void Subdivide(QuadNode node)
    {
        Point2D center = node.Bounds.Center;
        BoundingBox2D[] childBounds = new BoundingBox2D[4];
        childBounds[0] = new BoundingBox2D(new Point2D(node.Bounds.Min.X, center.Y), new Point2D(center.X, node.Bounds.Max.Y));
        childBounds[1] = new BoundingBox2D(center, node.Bounds.Max);
        childBounds[2] = new BoundingBox2D(node.Bounds.Min, center);
        childBounds[3] = new BoundingBox2D(new Point2D(center.X, node.Bounds.Min.Y), new Point2D(node.Bounds.Max.X, center.Y));

        for (int i = 0; i < 4; i++)
            node.Children[i] = new QuadNode { Bounds = childBounds[i] };

        node.IsDivided = true;
        List<QuadEntry2D> entries = node.Entries;
        node.Entries = new List<QuadEntry2D>();
        foreach (QuadEntry2D e in entries)
        {
            int quadrant = GetQuadrant(node.Bounds, e.Point);
            node.Children[quadrant]!.Entries.Add(e);
        }
    }

    private void RangeSearchRecursive(QuadNode node, BoundingBox2D range, ImmutableArray<int>.Builder result)
    {
        if (!node.Bounds.Intersects(range))
            return;
        if (!node.IsDivided)
        {
            foreach (QuadEntry2D entry in node.Entries)
            {
                if (range.Contains(entry.Point))
                    result.Add(entry.Id);
            }
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            if (node.Children[i] != null)
                RangeSearchRecursive(node.Children[i]!, range, result);
        }
    }

    private void RadiusSearchRecursive(QuadNode node, Point2D center, double radiusSq, ImmutableArray<int>.Builder result)
    {
        Point2D closest = ClampToBox(center, node.Bounds);
        double dx = center.X - closest.X;
        double dy = center.Y - closest.Y;
        if (dx * dx + dy * dy > radiusSq)
            return;
        if (!node.IsDivided)
        {
            foreach (QuadEntry2D entry in node.Entries)
            {
                double ex = entry.Point.X - center.X;
                double ey = entry.Point.Y - center.Y;
                if (ex * ex + ey * ey <= radiusSq)
                    result.Add(entry.Id);
            }
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            if (node.Children[i] != null)
                RadiusSearchRecursive(node.Children[i]!, center, radiusSq, result);
        }
    }

    private static Point2D ClampToBox(Point2D point, BoundingBox2D bounds)
    {
        double x = System.Math.Max(bounds.Min.X, System.Math.Min(bounds.Max.X, point.X));
        double y = System.Math.Max(bounds.Min.Y, System.Math.Min(bounds.Max.Y, point.Y));
        return new Point2D(x, y);
    }
}
