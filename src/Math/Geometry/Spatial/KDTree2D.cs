using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Spatial;

/// <summary>A KD-Tree for 2D spatial indexing of points.</summary>
public sealed class KDTree2D
{
    private readonly KDNode? _root;

    /// <summary>Gets the number of points in the tree.</summary>
    public int Count { get; }

    /// <summary>Builds a KD-Tree from the given points.</summary>
    public KDTree2D(IReadOnlyList<Point2D> points)
    {
        if (points.Count == 0) { _root = null; Count = 0; return; }
        Point2D[] pts = new Point2D[points.Count];
        for (int i = 0; i < points.Count; i++) pts[i] = points[i];
        _root = Build(pts, 0, pts.Length, 0);
        Count = pts.Length;
    }

    /// <summary>Finds the nearest neighbor to the query point.</summary>
    public Point2D NearestNeighbor(Point2D query)
    {
        if (_root == null) return Point2D.Origin;
        Point2D best = _root.Point;
        double bestDist = query.DistanceSquaredTo(best);
        Nearest(_root, query, 0, ref best, ref bestDist);
        return best;
    }

    /// <summary>Finds the k nearest neighbors.</summary>
    public ImmutableArray<Point2D> KNearest(Point2D query, int k)
    {
        if (_root == null || k <= 0) return ImmutableArray<Point2D>.Empty;
        var pq = new SortedSet<(double dist, int idx, Point2D p)>(Comparer<(double, int, Point2D)>.Create(
            (a, b) => a.Item1.CompareTo(b.Item1) != 0 ? a.Item1.CompareTo(b.Item1) : a.Item2.CompareTo(b.Item2)));
        int counter = 0;
        KNearestHelper(_root, query, 0, pq, k, ref counter);
        var result = ImmutableArray.CreateBuilder<Point2D>(System.Math.Min(k, pq.Count));
        foreach (var item in pq) result.Add(item.p);
        return result.ToImmutable();
    }

    /// <summary>Performs a range query, returning all points within the given radius.</summary>
    public ImmutableArray<Point2D> RangeQuery(Point2D center, double radius)
    {
        var result = ImmutableArray.CreateBuilder<Point2D>();
        if (_root == null) return result.ToImmutable();
        double rSq = radius * radius;
        RangeQuery(_root, center, rSq, 0, result);
        return result.ToImmutable();
    }

    /// <summary>Performs a range query returning all points within a bounding box.</summary>
    public ImmutableArray<Point2D> RangeQuery(BoundingBox2D box)
    {
        var result = ImmutableArray.CreateBuilder<Point2D>();
        if (_root == null) return result.ToImmutable();
        RangeQueryBox(_root, box, 0, result);
        return result.ToImmutable();
    }

    private void Nearest(KDNode node, Point2D query, int depth, ref Point2D best, ref double bestDist)
    {
        double d = query.DistanceSquaredTo(node.Point);
        if (d < bestDist) { bestDist = d; best = node.Point; }

        int axis = depth % 2;
        double diff = axis == 0 ? query.X - node.Point.X : query.Y - node.Point.Y;

        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;

        if (first != null) Nearest(first, query, depth + 1, ref best, ref bestDist);
        if (second != null && diff * diff < bestDist)
            Nearest(second, query, depth + 1, ref best, ref bestDist);
    }

    private void KNearestHelper(KDNode node, Point2D query, int depth,
        SortedSet<(double, int, Point2D)> pq, int k, ref int counter)
    {
        double d = query.DistanceSquaredTo(node.Point);
        pq.Add((d, counter++, node.Point));
        if (pq.Count > k) pq.Remove(pq.Max);

        int axis = depth % 2;
        double diff = axis == 0 ? query.X - node.Point.X : query.Y - node.Point.Y;

        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;

        if (first != null) KNearestHelper(first, query, depth + 1, pq, k, ref counter);
        double worstDist = pq.Count >= k ? pq.Max.Item1 : double.MaxValue;
        if (second != null && diff * diff < worstDist)
            KNearestHelper(second, query, depth + 1, pq, k, ref counter);
    }

    private void RangeQuery(KDNode node, Point2D center, double rSq, int depth, ImmutableArray<Point2D>.Builder result)
    {
        double d = node.Point.DistanceSquaredTo(center);
        if (d <= rSq) result.Add(node.Point);

        int axis = depth % 2;
        double diff = axis == 0 ? center.X - node.Point.X : center.Y - node.Point.Y;

        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;

        if (first != null) RangeQuery(first, center, rSq, depth + 1, result);
        if (second != null && diff * diff <= rSq)
            RangeQuery(second, center, rSq, depth + 1, result);
    }

    private void RangeQueryBox(KDNode node, BoundingBox2D box, int depth, ImmutableArray<Point2D>.Builder result)
    {
        if (box.Contains(node.Point)) result.Add(node.Point);

        int axis = depth % 2;
        double diff = axis == 0 ? node.Point.X - (box.Min.X + box.Width * 0.5) : node.Point.Y - (box.Min.Y + box.Height * 0.5);

        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;

        if (first != null) RangeQueryBox(first, box, depth + 1, result);
        if (second != null)
        {
            double halfExtent = axis == 0 ? box.Width * 0.5 : box.Height * 0.5;
            if (System.Math.Abs(diff) <= halfExtent)
                RangeQueryBox(second, box, depth + 1, result);
        }
    }

    private static KDNode Build(Point2D[] pts, int start, int end, int depth)
    {
        if (start >= end) return null!;
        int mid = (start + end) / 2;
        int axis = depth % 2;
        System.Array.Sort(pts, start, end - start, Comparer<Point2D>.Create(
            (a, b) => axis == 0 ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y)));
        return new KDNode(pts[mid], Build(pts, start, mid, depth + 1), Build(pts, mid + 1, end, depth + 1));
    }

    private sealed class KDNode
    {
        public Point2D Point;
        public KDNode? Left;
        public KDNode? Right;
        public KDNode(Point2D p, KDNode? l, KDNode? r) { Point = p; Left = l; Right = r; }
    }
}
