using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Spatial;

/// <summary>A KD-Tree for 3D spatial indexing of points.</summary>
public sealed class KDTree3D
{
    private readonly KDNode? _root;

    /// <summary>Gets the number of points in the tree.</summary>
    public int Count { get; }

    /// <summary>Builds a KD-Tree from the given points.</summary>
    public KDTree3D(IReadOnlyList<Point3D> points)
    {
        if (points.Count == 0) { _root = null; Count = 0; return; }
        Point3D[] pts = new Point3D[points.Count];
        for (int i = 0; i < points.Count; i++) pts[i] = points[i];
        _root = Build(pts, 0, pts.Length, 0);
        Count = pts.Length;
    }

    /// <summary>Finds the nearest neighbor to the query point.</summary>
    public Point3D NearestNeighbor(Point3D query)
    {
        if (_root == null) return Point3D.Origin;
        Point3D best = _root.Point;
        double bestDist = query.DistanceSquaredTo(best);
        Nearest(_root, query, 0, ref best, ref bestDist);
        return best;
    }

    /// <summary>Performs a range query returning all points within the given radius.</summary>
    public ImmutableArray<Point3D> RangeQuery(Point3D center, double radius)
    {
        var result = ImmutableArray.CreateBuilder<Point3D>();
        if (_root == null) return result.ToImmutable();
        double rSq = radius * radius;
        RangeQuery(_root, center, rSq, 0, result);
        return result.ToImmutable();
    }

    /// <summary>Performs a range query returning all points within a bounding box.</summary>
    public ImmutableArray<Point3D> RangeQuery(BoundingBox3D box)
    {
        var result = ImmutableArray.CreateBuilder<Point3D>();
        if (_root == null) return result.ToImmutable();
        RangeQueryBox(_root, box, 0, result);
        return result.ToImmutable();
    }

    private void Nearest(KDNode node, Point3D query, int depth, ref Point3D best, ref double bestDist)
    {
        double d = query.DistanceSquaredTo(node.Point);
        if (d < bestDist) { bestDist = d; best = node.Point; }

        int axis = depth % 3;
        double diff = axis == 0 ? query.X - node.Point.X : axis == 1 ? query.Y - node.Point.Y : query.Z - node.Point.Z;

        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;

        if (first != null) Nearest(first, query, depth + 1, ref best, ref bestDist);
        if (second != null && diff * diff < bestDist)
            Nearest(second, query, depth + 1, ref best, ref bestDist);
    }

    private void RangeQuery(KDNode node, Point3D center, double rSq, int depth, ImmutableArray<Point3D>.Builder result)
    {
        double d = node.Point.DistanceSquaredTo(center);
        if (d <= rSq) result.Add(node.Point);

        int axis = depth % 3;
        double diff = axis == 0 ? center.X - node.Point.X : axis == 1 ? center.Y - node.Point.Y : center.Z - node.Point.Z;

        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;

        if (first != null) RangeQuery(first, center, rSq, depth + 1, result);
        if (second != null && diff * diff <= rSq)
            RangeQuery(second, center, rSq, depth + 1, result);
    }

    private void RangeQueryBox(KDNode node, BoundingBox3D box, int depth, ImmutableArray<Point3D>.Builder result)
    {
        if (box.Contains(node.Point)) result.Add(node.Point);

        int axis = depth % 3;
        double nodeCoord = axis == 0 ? node.Point.X : axis == 1 ? node.Point.Y : node.Point.Z;
        double boxCenter = axis == 0 ? (box.Min.X + box.Max.X) * 0.5 : axis == 1 ? (box.Min.Y + box.Max.Y) * 0.5 : (box.Min.Z + box.Max.Z) * 0.5;
        double diff = nodeCoord - boxCenter;
        double halfExtent = axis == 0 ? box.Width * 0.5 : axis == 1 ? box.Height * 0.5 : box.Depth * 0.5;

        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;

        if (first != null) RangeQueryBox(first, box, depth + 1, result);
        if (second != null && System.Math.Abs(diff) <= halfExtent)
            RangeQueryBox(second, box, depth + 1, result);
    }

    private static KDNode Build(Point3D[] pts, int start, int end, int depth)
    {
        if (start >= end) return null!;
        int mid = (start + end) / 2;
        int axis = depth % 3;
        System.Array.Sort(pts, start, end - start, Comparer<Point3D>.Create(
            (a, b) => axis == 0 ? a.X.CompareTo(b.X) : axis == 1 ? a.Y.CompareTo(b.Y) : a.Z.CompareTo(b.Z)));
        return new KDNode(pts[mid], Build(pts, start, mid, depth + 1), Build(pts, mid + 1, end, depth + 1));
    }

    private sealed class KDNode
    {
        public Point3D Point;
        public KDNode? Left;
        public KDNode? Right;
        public KDNode(Point3D p, KDNode? l, KDNode? r) { Point = p; Left = l; Right = r; }
    }
}
