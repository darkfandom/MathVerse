using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Spatial;

/// <summary>A Bounding Volume Hierarchy (BVH) for triangle mesh spatial queries.</summary>
public sealed class BoundingVolumeHierarchy
{
    private readonly BVHNode? _root;
    private readonly IReadOnlyList<Triangle3D> _triangles;

    /// <summary>Gets the number of triangles in the BVH.</summary>
    public int Count { get; }

    /// <summary>Builds a BVH from triangles using a top-down median-split approach.</summary>
    public BoundingVolumeHierarchy(IReadOnlyList<Triangle3D> triangles)
    {
        _triangles = triangles;
        if (triangles.Count == 0) { _root = null; Count = 0; return; }
        var entries = new (int index, Point3D centroid, BoundingBox3D aabb)[triangles.Count];
        for (int i = 0; i < triangles.Count; i++)
        {
            BoundingBox3D aabb = triangles[i].ToBoundingBox();
            entries[i] = (i, triangles[i].Centroid, aabb);
        }
        _root = Build(entries, 0, entries.Length);
        Count = triangles.Count;
    }

    /// <summary>Tests ray intersection against all triangles, returning the closest hit.</summary>
    public (bool hit, Point3D point, double t, int triangleIndex) Raycast(Picking.Ray ray)
    {
        if (_root == null) return (false, Point3D.Origin, double.MaxValue, -1);
        Point3D best = Point3D.Origin;
        double bestT = double.MaxValue;
        int bestIdx = -1;
        Raycast(_root, ray, ref best, ref bestT, ref bestIdx);
        return (bestIdx >= 0, best, bestT, bestIdx);
    }

    /// <summary>Finds all triangles whose AABB intersects a query box.</summary>
    public ImmutableArray<Triangle3D> BoxQuery(BoundingBox3D query)
    {
        var result = ImmutableArray.CreateBuilder<Triangle3D>();
        if (_root != null) BoxQuery(_root, query, result);
        return result.ToImmutable();
    }

    private void Raycast(BVHNode node, Picking.Ray ray, ref Point3D best, ref double bestT, ref int bestIdx)
    {
        if (!node.Bounds.RayIntersects(ray)) return;

        if (node.TriangleIndex >= 0)
        {
            Triangle3D tri = _triangles[node.TriangleIndex];
            var (hit, point) = tri.Intersect(new Line3D(ray.Origin, ray.PointAt(1000)));
            if (hit)
            {
                double t = ray.Origin.DistanceTo(point);
                if (t < bestT) { bestT = t; best = point; bestIdx = node.TriangleIndex; }
            }
        }

        if (node.Left != null) Raycast(node.Left, ray, ref best, ref bestT, ref bestIdx);
        if (node.Right != null) Raycast(node.Right, ray, ref best, ref bestT, ref bestIdx);
    }

    private void BoxQuery(BVHNode node, BoundingBox3D query, ImmutableArray<Triangle3D>.Builder result)
    {
        if (!node.Bounds.Intersects(query)) return;
        if (node.TriangleIndex >= 0) result.Add(_triangles[node.TriangleIndex]);
        if (node.Left != null) BoxQuery(node.Left, query, result);
        if (node.Right != null) BoxQuery(node.Right, query, result);
    }

    private static BVHNode Build((int index, Point3D centroid, BoundingBox3D aabb)[] entries, int start, int end)
    {
        if (start >= end) return null!;

        BoundingBox3D bounds = entries[start].aabb;
        for (int i = start + 1; i < end; i++)
            bounds = bounds.Union(entries[i].aabb);

        if (end - start == 1)
            return new BVHNode { Bounds = bounds, TriangleIndex = entries[start].index };

        double dx = bounds.Width, dy = bounds.Height, dz = bounds.Depth;
        int axis = dx >= dy && dx >= dz ? 0 : dy >= dz ? 1 : 2;

        int mid = (start + end) / 2;
        System.Array.Sort(entries, start, end - start, Comparer<(int, Point3D, BoundingBox3D)>.Create(
            (a, b) => axis == 0 ? a.Item2.X.CompareTo(b.Item2.X) :
                       axis == 1 ? a.Item2.Y.CompareTo(b.Item2.Y) :
                                    a.Item2.Z.CompareTo(b.Item2.Z)));

        return new BVHNode
        {
            Bounds = bounds,
            TriangleIndex = -1,
            Left = Build(entries, start, mid),
            Right = Build(entries, mid, end)
        };
    }

    private sealed class BVHNode
    {
        public BoundingBox3D Bounds;
        public int TriangleIndex = -1;
        public BVHNode? Left;
        public BVHNode? Right;
    }
}
