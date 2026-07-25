using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Advanced.Spatial;

/// <summary>Represents a node in a 2D BSP (Binary Space Partitioning) tree.</summary>
public sealed class BSPNode2D
{
    /// <summary>The front child node (points on the positive side of the splitting line).</summary>
    public BSPNode2D? Front { get; }

    /// <summary>The back child node (points on the negative side of the splitting line).</summary>
    public BSPNode2D? Back { get; }

    /// <summary>The line used to partition the space at this node.</summary>
    public Line2D SplitLine { get; }

    /// <summary>The points stored at this node (leaf nodes only).</summary>
    public ImmutableArray<Point2D> Points { get; }

    /// <summary>Initializes a new BSP tree node.</summary>
    /// <param name="front">The front child node.</param>
    /// <param name="back">The back child node.</param>
    /// <param name="splitLine">The splitting line for this node.</param>
    /// <param name="points">The points stored at this leaf node.</param>
    public BSPNode2D(BSPNode2D? front, BSPNode2D? back, Line2D splitLine, ImmutableArray<Point2D> points)
    {
        Front = front;
        Back = back;
        SplitLine = splitLine;
        Points = points;
    }
}

/// <summary>A 2D BSP (Binary Space Partitioning) tree for spatial partitioning and queries on 2D points.</summary>
public static class BSPTree2D
{
    private const double Tolerance = 1e-10;

    /// <summary>Builds a balanced BSP tree from the given collection of points.</summary>
    /// <param name="points">The points to partition into the tree.</param>
    /// <returns>The root node of the constructed BSP tree.</returns>
    public static BSPNode2D Build(ImmutableArray<Point2D> points)
    {
        if (points.Length == 0)
            return new BSPNode2D(null, null, new Line2D(Point2D.Origin, new Point2D(1, 0)), ImmutableArray<Point2D>.Empty);
        if (points.Length <= 3)
            return new BSPNode2D(null, null, new Line2D(points[0], points.Length > 1 ? points[1] : new Point2D(points[0].X + 1, points[0].Y)), points);
        return BuildRecursive(points);
    }

    /// <summary>Performs a range search using the BSP tree, finding all points within the given bounding box.</summary>
    /// <param name="node">The current BSP tree node to search.</param>
    /// <param name="bounds">The query bounding box.</param>
    /// <returns>An immutable array of points that fall within the bounding box.</returns>
    public static ImmutableArray<Point2D> RangeSearch(BSPNode2D node, BoundingBox2D bounds)
    {
        var result = ImmutableArray.CreateBuilder<Point2D>();
        RangeSearchRecursive(node, bounds, result);
        return result.ToImmutable();
    }

    /// <summary>Finds the nearest point to the query point using the BSP tree for acceleration.</summary>
    /// <param name="node">The current BSP tree node.</param>
    /// <param name="query">The query point to find the nearest neighbor for.</param>
    /// <returns>The nearest point, or <c>null</c> if the tree is empty.</returns>
    public static Point2D? FindNearest(BSPNode2D node, Point2D query)
    {
        Point2D? best = null;
        double bestDistSq = double.MaxValue;
        FindNearestRecursive(node, query, ref best, ref bestDistSq);
        return best;
    }

    private static BSPNode2D BuildRecursive(ImmutableArray<Point2D> points)
    {
        if (points.Length <= 3)
        {
            Point2D p1 = points[0];
            Point2D p2 = points.Length > 1 ? points[1] : new Point2D(p1.X + 1, p1.Y);
            return new BSPNode2D(null, null, new Line2D(p1, p2), points);
        }

        Point2D splitPoint = points[points.Length / 2];
        Vector2D dir = new Vector2D(splitPoint.Y * 0.371 + 0.1, splitPoint.X * 0.293 + 0.7).Normalize();
        Line2D splitLine = new Line2D(splitPoint, new Point2D(splitPoint.X + dir.X, splitPoint.Y + dir.Y));

        var front = ImmutableArray.CreateBuilder<Point2D>();
        var back = ImmutableArray.CreateBuilder<Point2D>();

        for (int i = 0; i < points.Length; i++)
        {
            double side = SideOfLine(splitLine, points[i]);
            if (side >= -Tolerance)
                front.Add(points[i]);
            else
                back.Add(points[i]);
        }

        if (front.Count == 0 || back.Count == 0)
        {
            int half = points.Length / 2;
            front.Clear();
            back.Clear();
            for (int i = 0; i < half; i++)
                front.Add(points[i]);
            for (int i = half; i < points.Length; i++)
                back.Add(points[i]);
        }

        BSPNode2D frontNode = BuildRecursive(front.ToImmutable());
        BSPNode2D backNode = BuildRecursive(back.ToImmutable());

        return new BSPNode2D(frontNode, backNode, splitLine, ImmutableArray<Point2D>.Empty);
    }

    private static double SideOfLine(Line2D line, Point2D point)
    {
        double dx = line.P2.X - line.P1.X;
        double dy = line.P2.Y - line.P1.Y;
        return (point.X - line.P1.X) * dy - (point.Y - line.P1.Y) * dx;
    }

    private static void RangeSearchRecursive(BSPNode2D node, BoundingBox2D bounds, ImmutableArray<Point2D>.Builder result)
    {
        foreach (Point2D p in node.Points)
        {
            if (bounds.Contains(p))
                result.Add(p);
        }
        if (node.Front == null && node.Back == null)
            return;

        double d0 = SideOfLine(node.SplitLine, new Point2D(bounds.Min.X, bounds.Min.Y));
        double d1 = SideOfLine(node.SplitLine, new Point2D(bounds.Max.X, bounds.Min.Y));
        double d2 = SideOfLine(node.SplitLine, new Point2D(bounds.Max.X, bounds.Max.Y));
        double d3 = SideOfLine(node.SplitLine, new Point2D(bounds.Min.X, bounds.Max.Y));

        double minSide = System.Math.Min(d0, System.Math.Min(d1, System.Math.Min(d2, d3)));
        double maxSide = System.Math.Max(d0, System.Math.Max(d1, System.Math.Max(d2, d3)));

        bool intersectsFront = maxSide >= -Tolerance;
        bool intersectsBack = minSide <= Tolerance;

        if (intersectsFront && node.Front != null)
            RangeSearchRecursive(node.Front, bounds, result);
        if (intersectsBack && node.Back != null)
            RangeSearchRecursive(node.Back, bounds, result);
    }

    private static void FindNearestRecursive(BSPNode2D node, Point2D query, ref Point2D? best, ref double bestDistSq)
    {
        foreach (Point2D p in node.Points)
        {
            double dx = p.X - query.X;
            double dy = p.Y - query.Y;
            double distSq = dx * dx + dy * dy;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = p;
            }
        }
        if (node.Front == null && node.Back == null)
            return;

        double side = SideOfLine(node.SplitLine, query);
        BSPNode2D? first = side >= 0 ? node.Front : node.Back;
        BSPNode2D? second = side >= 0 ? node.Back : node.Front;

        if (first != null)
            FindNearestRecursive(first, query, ref best, ref bestDistSq);

        if (second != null)
        {
            double perpDistSq = System.Math.Abs(SideOfLine(node.SplitLine, query));
            double lenSq = node.SplitLine.Length * node.SplitLine.Length;
            if (lenSq > Tolerance)
                perpDistSq = (perpDistSq * perpDistSq) / lenSq;
            if (perpDistSq < bestDistSq)
                FindNearestRecursive(second, query, ref best, ref bestDistSq);
        }
    }
}
