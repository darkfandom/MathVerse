using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Spatial;

/// <summary>A loose octree for 3D spatial indexing of points.</summary>
public sealed class Octree
{
    private readonly OctreeNode _root;
    private const int MaxDepth = 20;
    private const int MaxPointsPerNode = 8;

    /// <summary>Gets the number of points in the octree.</summary>
    public int Count { get; }

    /// <summary>Builds an octree from the given points and bounding volume.</summary>
    public Octree(BoundingBox3D bounds, IReadOnlyList<Point3D> points)
    {
        _root = new OctreeNode(bounds);
        Count = points.Count;
        for (int i = 0; i < points.Count; i++)
            Insert(points[i], _root, 0);
    }

    /// <summary>Finds all points within the given radius of the center.</summary>
    public ImmutableArray<Point3D> RangeQuery(Point3D center, double radius)
    {
        var result = ImmutableArray.CreateBuilder<Point3D>();
        double rSq = radius * radius;
        RangeQuery(_root, center, rSq, result);
        return result.ToImmutable();
    }

    /// <summary>Finds all points within the given bounding box.</summary>
    public ImmutableArray<Point3D> RangeQuery(BoundingBox3D box)
    {
        var result = ImmutableArray.CreateBuilder<Point3D>();
        RangeQueryBox(_root, box, result);
        return result.ToImmutable();
    }

    /// <summary>Finds the nearest neighbor to the query point.</summary>
    public Point3D NearestNeighbor(Point3D query)
    {
        Point3D best = Point3D.Origin;
        double bestDist = double.MaxValue;
        NearestNeighbor(_root, query, ref best, ref bestDist);
        return best;
    }

    /// <summary>Gets the leaf nodes of the octree for visualization.</summary>
    public ImmutableArray<BoundingBox3D> GetLeaves()
    {
        var result = ImmutableArray.CreateBuilder<BoundingBox3D>();
        CollectLeaves(_root, result);
        return result.ToImmutable();
    }

    private static void Insert(Point3D point, OctreeNode node, int depth)
    {
        if (node.Points == null) node.Points = new List<Point3D>();

        if (node.Children == null || depth >= MaxDepth)
        {
            node.Points.Add(point);
            if (node.Points.Count > MaxPointsPerNode && depth < MaxDepth && node.Children == null)
                Subdivide(node, depth);
            return;
        }

        int octant = GetOctant(node.Bounds.Center, point);
        Insert(point, node.Children[octant], depth + 1);
    }

    private static void Subdivide(OctreeNode node, int depth)
    {
        Point3D c = node.Bounds.Center;
        Point3D min = node.Bounds.Min;
        Point3D max = node.Bounds.Max;
        node.Children = new OctreeNode[8];
        var children = node.Children;

        for (int i = 0; i < 8; i++)
        {
            Point3D nMin = new(
                (i & 1) == 0 ? min.X : c.X,
                (i & 2) == 0 ? min.Y : c.Y,
                (i & 4) == 0 ? min.Z : c.Z);
            Point3D nMax = new(
                (i & 1) == 0 ? c.X : max.X,
                (i & 2) == 0 ? c.Y : max.Y,
                (i & 4) == 0 ? c.Z : max.Z);
            children[i] = new OctreeNode(new BoundingBox3D(nMin, nMax));
        }

        if (node.Points != null)
        {
            foreach (Point3D p in node.Points)
            {
                int octant = GetOctant(c, p);
                List<Point3D> pts = children[octant].Points ??= new List<Point3D>();
                pts.Add(p);
            }
            node.Points = null;
        }
    }

    private static int GetOctant(Point3D center, Point3D point) =>
        (point.X >= center.X ? 1 : 0) | (point.Y >= center.Y ? 2 : 0) | (point.Z >= center.Z ? 4 : 0);

    private void RangeQuery(OctreeNode node, Point3D center, double rSq, ImmutableArray<Point3D>.Builder result)
    {
        if (!SphereIntersectsBox(center, System.Math.Sqrt(rSq), node.Bounds)) return;

        if (node.Points != null)
            foreach (Point3D p in node.Points)
                if (p.DistanceSquaredTo(center) <= rSq) result.Add(p);

        if (node.Children != null)
            for (int i = 0; i < 8; i++)
                RangeQuery(node.Children[i], center, rSq, result);
    }

    private void RangeQueryBox(OctreeNode node, BoundingBox3D box, ImmutableArray<Point3D>.Builder result)
    {
        if (!node.Bounds.Intersects(box)) return;

        if (node.Points != null)
            foreach (Point3D p in node.Points)
                if (box.Contains(p)) result.Add(p);

        if (node.Children != null)
            for (int i = 0; i < 8; i++)
                RangeQueryBox(node.Children[i], box, result);
    }

    private void NearestNeighbor(OctreeNode node, Point3D query, ref Point3D best, ref double bestDist)
    {
        if (ClosestPointToBounds(query, node.Bounds).DistanceSquaredTo(query) >= bestDist) return;

        if (node.Points != null)
            foreach (Point3D p in node.Points)
            {
                double d = p.DistanceSquaredTo(query);
                if (d < bestDist) { bestDist = d; best = p; }
            }

        if (node.Children != null)
            for (int i = 0; i < 8; i++)
                NearestNeighbor(node.Children[i], query, ref best, ref bestDist);
    }

    private static void CollectLeaves(OctreeNode node, ImmutableArray<BoundingBox3D>.Builder result)
    {
        if (node.Children == null) { result.Add(node.Bounds); return; }
        for (int i = 0; i < 8; i++) CollectLeaves(node.Children[i], result);
    }

    private static bool SphereIntersectsBox(Point3D center, double radius, BoundingBox3D box)
    {
        double rSq = radius * radius;
        double closestX = System.Math.Max(box.Min.X, System.Math.Min(center.X, box.Max.X));
        double closestY = System.Math.Max(box.Min.Y, System.Math.Min(center.Y, box.Max.Y));
        double closestZ = System.Math.Max(box.Min.Z, System.Math.Min(center.Z, box.Max.Z));
        double dx = center.X - closestX, dy = center.Y - closestY, dz = center.Z - closestZ;
        return dx * dx + dy * dy + dz * dz <= rSq;
    }

    private static Point3D ClosestPointToBounds(Point3D p, BoundingBox3D box) => new(
        System.Math.Max(box.Min.X, System.Math.Min(p.X, box.Max.X)),
        System.Math.Max(box.Min.Y, System.Math.Min(p.Y, box.Max.Y)),
        System.Math.Max(box.Min.Z, System.Math.Min(p.Z, box.Max.Z)));

    private sealed class OctreeNode
    {
        public BoundingBox3D Bounds;
        public List<Point3D>? Points;
        public OctreeNode[]? Children;
        public OctreeNode(BoundingBox3D b) { Bounds = b; }
    }
}
