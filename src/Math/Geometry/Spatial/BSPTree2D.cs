using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Spatial;

/// <summary>A BSP (Binary Space Partitioning) tree for 2D spatial queries.</summary>
public sealed class BSPTree2D
{
    private readonly BSPNode? _root;

    /// <summary>Gets the number of polygons stored in the tree.</summary>
    public int Count { get; }

    /// <summary>Builds a BSP tree from the given convex polygons.</summary>
    public BSPTree2D(IReadOnlyList<Polygon2D> polygons)
    {
        if (polygons.Count == 0) { _root = null; Count = 0; return; }
        List<Polygon2D> list = new(polygons);
        _root = Build(list, 0);
        Count = polygons.Count;
    }

    /// <summary>Determines which polygons contain the given point.</summary>
    public ImmutableArray<int> PointInPolygons(Point2D query)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        if (_root != null) PointQuery(_root, query, result);
        return result.ToImmutable();
    }

    /// <summary>Splits a polygon by a line, returning front and back halves.</summary>
    public static (List<Point2D> front, List<Point2D> back) SplitPolygon(IReadOnlyList<Point2D> polygon, Point2D linePoint, Vector2D lineNormal)
    {
        List<Point2D> front = new(), back = new();
        if (polygon.Count == 0) return (front, back);

        for (int i = 0; i < polygon.Count; i++)
        {
            Point2D curr = polygon[i];
            Point2D next = polygon[(i + 1) % polygon.Count];
            double currDist = (curr.X - linePoint.X) * lineNormal.X + (curr.Y - linePoint.Y) * lineNormal.Y;
            double nextDist = (next.X - linePoint.X) * lineNormal.X + (next.Y - linePoint.Y) * lineNormal.Y;

            if (currDist >= 0) front.Add(curr); else back.Add(curr);

            if ((currDist > 0 && nextDist < 0) || (currDist < 0 && nextDist > 0))
            {
                double t = currDist / (currDist - nextDist);
                Point2D intersection = new(curr.X + t * (next.X - curr.X), curr.Y + t * (next.Y - curr.Y));
                front.Add(intersection);
                back.Add(intersection);
            }
        }
        return (front, back);
    }

    private void PointQuery(BSPNode node, Point2D query, ImmutableArray<int>.Builder result)
    {
        if (node.PolygonIndices != null)
            foreach (int idx in node.PolygonIndices)
                result.Add(idx);

        if (node.SplitPoint == null || node.SplitNormal == null) return;

        double dist = (query.X - node.SplitPoint.Value.X) * node.SplitNormal.Value.X +
                      (query.Y - node.SplitPoint.Value.Y) * node.SplitNormal.Value.Y;

        if (dist >= 0 && node.Front != null) PointQuery(node.Front, query, result);
        if (dist < 0 && node.Back != null) PointQuery(node.Back, query, result);
    }

    private static BSPNode Build(List<Polygon2D> polygons, int depth)
    {
        if (polygons.Count == 0) return new BSPNode();

        int idx = depth % polygons.Count;
        Polygon2D splitter = polygons[idx];

        Point2D p0 = splitter.Vertices[0];
        Point2D p1 = splitter.Vertices[1];
        Vector2D normal = new(p1.Y - p0.Y, -(p1.X - p0.X));
        double len = normal.Length;
        if (len > 1e-15) normal = new Vector2D(normal.X / len, normal.Y / len);

        List<Polygon2D> frontPolys = new(), backPolys = new();
        for (int i = 0; i < polygons.Count; i++)
        {
            if (i == idx) continue;
            var (front, back) = SplitPolygon(polygons[i].Vertices, p0, normal);
            if (front.Count >= 3) frontPolys.Add(new Polygon2D(front.ToImmutableArray()));
            if (back.Count >= 3) backPolys.Add(new Polygon2D(back.ToImmutableArray()));
        }

        return new BSPNode
        {
            SplitPoint = p0,
            SplitNormal = normal,
            PolygonIndices = ImmutableArray.Create(idx),
            Front = frontPolys.Count > 0 ? Build(frontPolys, depth + 1) : null,
            Back = backPolys.Count > 0 ? Build(backPolys, depth + 1) : null
        };
    }

    private sealed class BSPNode
    {
        public Point2D? SplitPoint;
        public Vector2D? SplitNormal;
        public ImmutableArray<int>? PolygonIndices;
        public BSPNode? Front;
        public BSPNode? Back;
    }
}
