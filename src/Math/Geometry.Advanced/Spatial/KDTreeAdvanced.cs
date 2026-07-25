using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.Spatial;

/// <summary>Represents a single entry in a KD-tree with a 2D point and associated identifier.</summary>
/// <param name="Point">The 2D point position.</param>
/// <param name="Id">The unique identifier associated with this entry.</param>
public readonly record struct KDEntry2D(Point2D Point, int Id);

/// <summary>A balanced 2D KD-tree for efficient spatial queries including range search,
/// nearest neighbor, and k-nearest neighbor queries on <see cref="Point2D"/> data.</summary>
public class KDTree2D
{
    private const double Tolerance = 1e-10;
    private KDNode? _root;
    private int _nextId;

    /// <summary>Internal node of the KD-tree storing a split point, axis, and child references.</summary>
    internal sealed class KDNode
    {
        /// <summary>The point stored at this node.</summary>
        public Point2D Point { get; set; }

        /// <summary>The split axis (0 = X, 1 = Y).</summary>
        public int Axis { get; set; }

        /// <summary>The unique identifier for this point.</summary>
        public int Id { get; set; }

        /// <summary>The left child node (points with lesser coordinate on the split axis).</summary>
        public KDNode? Left { get; set; }

        /// <summary>The right child node (points with greater or equal coordinate on the split axis).</summary>
        public KDNode? Right { get; set; }
    }

    /// <summary>Initializes a new <see cref="KDTree2D"/> by building a balanced tree from the given points.</summary>
    /// <param name="points">The collection of 2D points to insert into the tree.</param>
    public KDTree2D(ImmutableArray<Point2D> points)
    {
        _nextId = 0;
        if (points.Length == 0) return;
        KDEntry2D[] entries = new KDEntry2D[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            entries[i] = new KDEntry2D(points[i], _nextId);
            _nextId++;
        }
        _root = BuildBalanced(entries, 0, entries.Length - 1, 0);
    }

    /// <summary>Finds all point indices within the given axis-aligned bounding box.</summary>
    /// <param name="bounds">The query bounding box.</param>
    /// <returns>An immutable array of point identifiers that fall within the bounds.</returns>
    public ImmutableArray<int> RangeSearch(BoundingBox2D bounds)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        if (_root != null)
            RangeSearchRecursive(_root, bounds, result);
        return result.ToImmutable();
    }

    /// <summary>Finds the index of the nearest point to the given query point.</summary>
    /// <param name="query">The query point.</param>
    /// <returns>The identifier of the nearest point, or -1 if the tree is empty.</returns>
    public int NearestNeighbor(Point2D query)
    {
        if (_root == null) return -1;
        int bestId = -1;
        double bestDistSq = double.MaxValue;
        NearestNeighborRecursive(_root, query, ref bestId, ref bestDistSq);
        return bestId;
    }

    /// <summary>Finds the k nearest neighbors to the given query point.</summary>
    /// <param name="query">The query point.</param>
    /// <param name="k">The number of nearest neighbors to find.</param>
    /// <returns>An immutable array of point identifiers of the k nearest points.</returns>
    public ImmutableArray<int> KNearestNeighbors(Point2D query, int k)
    {
        if (_root == null || k <= 0) return ImmutableArray<int>.Empty;
        var heap = new SortedSet<(double DistSq, int Id)>(Comparer<(double, int)>.Create((a, b) =>
        {
            int cmp = a.Item1.CompareTo(b.Item1);
            return cmp != 0 ? cmp : a.Item2.CompareTo(b.Item2);
        }));
        int count = 0;
        double worstDistSq = double.MaxValue;
        KNearestRecursive(_root, query, heap, ref count, k, ref worstDistSq);
        var result = ImmutableArray.CreateBuilder<int>(System.Math.Min(k, count));
        foreach (var entry in heap)
            result.Add(entry.Id);
        return result.ToImmutable();
    }

    /// <summary>Inserts a new point into the KD-tree.</summary>
    /// <param name="point">The point to insert.</param>
    public void Insert(Point2D point)
    {
        int id = _nextId++;
        var node = new KDNode { Point = point, Axis = 0, Id = id };
        if (_root == null)
        {
            _root = node;
            return;
        }
        KDNode current = _root;
        int depth = 0;
        while (true)
        {
            int axis = depth % 2;
            double cmp = point[axis] - current.Point[axis];
            if (cmp < 0)
            {
                if (current.Left == null)
                {
                    node.Axis = (axis + 1) % 2;
                    current.Left = node;
                    break;
                }
                current = current.Left;
            }
            else
            {
                if (current.Right == null)
                {
                    node.Axis = (axis + 1) % 2;
                    current.Right = node;
                    break;
                }
                current = current.Right;
            }
            depth++;
        }
    }

    /// <summary>Removes a point from the KD-tree matching the given coordinates within tolerance.</summary>
    /// <param name="point">The point to remove.</param>
    /// <returns><c>true</c> if the point was found and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(Point2D point)
    {
        if (_root == null) return false;
        var path = new List<KDNode>();
        KDNode? parent = null;
        KDNode? current = _root;
        int depth = 0;
        while (current != null)
        {
            double dx = System.Math.Abs(current.Point.X - point.X);
            double dy = System.Math.Abs(current.Point.Y - point.Y);
            if (dx < Tolerance && dy < Tolerance)
            {
                RemoveNode(parent, current, path, depth);
                return true;
            }
            parent = current;
            path.Add(current);
            int axis = depth % 2;
            current = point[axis] < current.Point[axis] ? current.Left : current.Right;
            depth++;
        }
        return false;
    }

    private static KDNode? BuildBalanced(KDEntry2D[] entries, int lo, int hi, int depth)
    {
        if (lo > hi) return null;
        int axis = depth % 2;
        int mid = (lo + hi) / 2;
        System.Array.Sort(entries, lo, hi - lo + 1, Comparer<KDEntry2D>.Create((a, b) =>
        {
            return a.Point[axis].CompareTo(b.Point[axis]);
        }));
        mid = (lo + hi) / 2;
        KDEntry2D median = entries[mid];
        return new KDNode
        {
            Point = median.Point,
            Id = median.Id,
            Axis = axis,
            Left = BuildBalanced(entries, lo, mid - 1, depth + 1),
            Right = BuildBalanced(entries, mid + 1, hi, depth + 1)
        };
    }

    private void RangeSearchRecursive(KDNode node, BoundingBox2D bounds, ImmutableArray<int>.Builder result)
    {
        if (bounds.Contains(node.Point))
            result.Add(node.Id);
        int axis = node.Axis;
        double splitCoord = node.Point[axis];
        if (node.Left != null)
        {
            double maxOnAxis = axis == 0 ? bounds.Max.X : bounds.Max.Y;
            if (splitCoord >= maxOnAxis || node.Point[axis] <= maxOnAxis + Tolerance)
            {
                double minOnAxis = axis == 0 ? bounds.Min.X : bounds.Min.Y;
                if (splitCoord >= minOnAxis - Tolerance)
                    RangeSearchRecursive(node.Left, bounds, result);
                else if (splitCoord < minOnAxis)
                    RangeSearchRecursive(node.Left, bounds, result);
            }
        }
        if (node.Right != null)
        {
            double minOnAxis = axis == 0 ? bounds.Min.X : bounds.Min.Y;
            if (splitCoord <= minOnAxis || splitCoord >= minOnAxis - Tolerance)
            {
                double maxOnAxis = axis == 0 ? bounds.Max.X : bounds.Max.Y;
                if (splitCoord <= maxOnAxis + Tolerance)
                    RangeSearchRecursive(node.Right, bounds, result);
                else if (splitCoord > maxOnAxis)
                    RangeSearchRecursive(node.Right, bounds, result);
            }
        }
    }

    private static void NearestNeighborRecursive(KDNode node, Point2D query, ref int bestId, ref double bestDistSq)
    {
        double dx = node.Point.X - query.X;
        double dy = node.Point.Y - query.Y;
        double distSq = dx * dx + dy * dy;
        if (distSq < bestDistSq)
        {
            bestDistSq = distSq;
            bestId = node.Id;
        }
        int axis = node.Axis;
        double diff = query[axis] - node.Point[axis];
        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;
        if (first != null)
            NearestNeighborRecursive(first, query, ref bestId, ref bestDistSq);
        if (second != null && diff * diff < bestDistSq)
            NearestNeighborRecursive(second, query, ref bestId, ref bestDistSq);
    }

    private static void KNearestRecursive(KDNode node, Point2D query, SortedSet<(double DistSq, int Id)> heap,
        ref int count, int k, ref double worstDistSq)
    {
        double dx = node.Point.X - query.X;
        double dy = node.Point.Y - query.Y;
        double distSq = dx * dx + dy * dy;
        if (count < k)
        {
            heap.Add((distSq, node.Id));
            count++;
            if (count == k)
                worstDistSq = heap.Max.DistSq;
        }
        else if (distSq < worstDistSq)
        {
            (double, int) last = heap.Max;
            heap.Remove(last);
            heap.Add((distSq, node.Id));
            worstDistSq = heap.Max.DistSq;
        }
        int axis = node.Axis;
        double diff = query[axis] - node.Point[axis];
        KDNode? first = diff < 0 ? node.Left : node.Right;
        KDNode? second = diff < 0 ? node.Right : node.Left;
        if (first != null)
            KNearestRecursive(first, query, heap, ref count, k, ref worstDistSq);
        if (second != null && diff * diff < worstDistSq)
            KNearestRecursive(second, query, heap, ref count, k, ref worstDistSq);
    }

    private void RemoveNode(KDNode? parent, KDNode target, List<KDNode> path, int depth)
    {
        if (target.Right != null)
        {
            KDNode successor = FindMin(target.Right, target.Axis);
            target.Point = successor.Point;
            target.Id = successor.Id;
            RemoveSuccessor(target, target.Right, successor);
        }
        else if (target.Left != null)
        {
            KDNode successor = FindMin(target.Left, target.Axis);
            target.Point = successor.Point;
            target.Id = successor.Id;
            target.Right = target.Left;
            target.Left = null;
            RemoveSuccessor(target, target.Right, successor);
        }
        else
        {
            if (parent == null)
                _root = null;
            else if (parent.Left == target)
                parent.Left = null;
            else
                parent.Right = null;
        }
    }

    private void RemoveSuccessor(KDNode parent, KDNode? current, KDNode target)
    {
        while (current != null && current != target)
        {
            if (current.Left == target)
            {
                current.Left = target.Right;
                return;
            }
            if (current.Right == target)
            {
                current.Right = target.Right;
                return;
            }
            current = target.Point[current.Axis] < current.Point[current.Axis] ? current.Left : current.Right;
        }
    }

    private static KDNode FindMin(KDNode node, int axis)
    {
        if (node.Left == null && node.Right == null) return node;
        KDNode min = node;
        if (node.Left != null)
        {
            KDNode leftMin = FindMin(node.Left, axis);
            if (leftMin.Point[axis] < min.Point[axis])
                min = leftMin;
        }
        if (node.Right != null && node.Axis != axis)
        {
            KDNode rightMin = FindMin(node.Right, axis);
            if (rightMin.Point[axis] < min.Point[axis])
                min = rightMin;
        }
        return min;
    }
}
