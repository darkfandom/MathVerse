using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Optimization;

/// <summary>
/// Provides polygon simplification algorithms for reducing vertex count while preserving shape.
/// </summary>
public static class PolygonSimplifier
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Simplifies a polygon using the Douglas-Peucker algorithm.
    /// Recursively removes points that are within the tolerance distance
    /// of the line segment connecting their neighboring retained points.
    /// </summary>
    /// <param name="polygon">The input polygon vertices in order.</param>
    /// <param name="tolerance">Maximum perpendicular distance from the line for a point to be removed.</param>
    /// <returns>The simplified polygon vertices.</returns>
    public static ImmutableArray<Point2D> DouglasPeucker(ImmutableArray<Point2D> polygon, double tolerance)
    {
        if (polygon.Length <= 2)
            return polygon;

        bool[] keep = new bool[polygon.Length];
        keep[0] = true;
        keep[polygon.Length - 1] = true;

        DouglasPeuckerRecurse(polygon, 0, polygon.Length - 1, tolerance, keep);

        var result = ImmutableArray.CreateBuilder<Point2D>();
        for (int i = 0; i < polygon.Length; i++)
        {
            if (keep[i])
                result.Add(polygon[i]);
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Simplifies a polygon using the Visvalingam-Whyatt algorithm.
    /// Iteratively removes the vertex with the smallest effective area
    /// until the target vertex count is reached.
    /// </summary>
    /// <param name="polygon">The input polygon vertices in order.</param>
    /// <param name="targetCount">Desired number of vertices after simplification.</param>
    /// <returns>The simplified polygon vertices.</returns>
    public static ImmutableArray<Point2D> VisvalingamWhyatt(ImmutableArray<Point2D> polygon, int targetCount)
    {
        if (polygon.Length <= targetCount)
            return polygon;

        var points = new LinkedList<Point2D>();
        for (int i = 0; i < polygon.Length; i++)
            points.AddLast(polygon[i]);

        int toRemove = polygon.Length - targetCount;
        var areas = new double[polygon.Length];
        var nodes = new LinkedListNode<Point2D>?[polygon.Length];
        var areaHeap = new SortedSet<(double Area, int Index)>(Comparer<(double Area, int Index)>.Create(
            (x, y) => x.Area.CompareTo(y.Area) != 0 ? x.Area.CompareTo(y.Area) : x.Index.CompareTo(y.Index)));

        var node = points.First;
        int idx = 0;
        while (node != null)
        {
            nodes[idx] = node;
            idx++;
            node = node.Next;
        }

        for (int i = 0; i < polygon.Length; i++)
        {
            int prev = (i - 1 + polygon.Length) % polygon.Length;
            int next = (i + 1) % polygon.Length;
            if (prev == next) continue;
            areas[i] = ComputeTriangleArea(nodes[prev]!.Value, nodes[i]!.Value, nodes[next]!.Value);
            if (areas[i] >= 0)
                areaHeap.Add((areas[i], i));
        }

        int removed = 0;
        while (removed < toRemove && areaHeap.Count > 0)
        {
            var smallest = areaHeap.Min;
            areaHeap.Remove(smallest);
            int ci = smallest.Index;

            if (areas[ci] < -0.5) continue;

            LinkedListNode<Point2D>? current = nodes[ci];
            if (current == null) continue;

            LinkedListNode<Point2D> prevNode = current.Previous ?? points.Last!;
            LinkedListNode<Point2D> nextNode = current.Next ?? points.First!;

            points.Remove(current);
            nodes[ci] = null;
            areas[ci] = -1.0;
            removed++;

            if (prevNode != nextNode)
            {
                if (prevNode != null)
                    ReevaluateNode(prevNode, nodes, areas, areaHeap, points);
                if (nextNode != null)
                    ReevaluateNode(nextNode, nodes, areas, areaHeap, points);
            }
        }

        var result = ImmutableArray.CreateBuilder<Point2D>(points.Count);
        var cur = points.First;
        while (cur != null)
        {
            result.Add(cur.Value);
            cur = cur.Next;
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Simplifies a polygon by removing vertices that are within the tolerance
    /// distance from the line connecting their neighbors (radial distance method).
    /// This is a single-pass algorithm that processes vertices sequentially.
    /// </summary>
    /// <param name="polygon">The input polygon vertices in order.</param>
    /// <param name="tolerance">Maximum distance from the line for a point to be removed.</param>
    /// <returns>The simplified polygon vertices.</returns>
    public static ImmutableArray<Point2D> RadialDistance(ImmutableArray<Point2D> polygon, double tolerance)
    {
        if (polygon.Length <= 2)
            return polygon;

        var result = ImmutableArray.CreateBuilder<Point2D>(polygon.Length);
        result.Add(polygon[0]);

        double toleranceSq = tolerance * tolerance;
        int lastIndex = 0;

        for (int i = 1; i < polygon.Length - 1; i++)
        {
            double dx = polygon[i].X - polygon[lastIndex].X;
            double dy = polygon[i].Y - polygon[lastIndex].Y;
            double distSq = dx * dx + dy * dy;

            double nx = polygon[i + 1].X - polygon[lastIndex].X;
            double ny = polygon[i + 1].Y - polygon[lastIndex].Y;
            double lineLenSq = nx * nx + ny * ny;

            if (lineLenSq < Tolerance * Tolerance)
            {
                if (distSq > toleranceSq)
                {
                    result.Add(polygon[i]);
                    lastIndex = i;
                }
                continue;
            }

            double t = ((polygon[i].X - polygon[lastIndex].X) * nx + (polygon[i].Y - polygon[lastIndex].Y) * ny) / lineLenSq;
            t = System.Math.Max(0.0, System.Math.Min(1.0, t));

            double closestX = polygon[lastIndex].X + t * nx;
            double closestY = polygon[lastIndex].Y + t * ny;
            double perpDistSq = (polygon[i].X - closestX) * (polygon[i].X - closestX)
                              + (polygon[i].Y - closestY) * (polygon[i].Y - closestY);

            if (perpDistSq > toleranceSq)
            {
                result.Add(polygon[i]);
                lastIndex = i;
            }
        }

        result.Add(polygon[polygon.Length - 1]);

        return result.ToImmutable();
    }

    private static void DouglasPeuckerRecurse(
        ImmutableArray<Point2D> polygon, int start, int end, double tolerance, bool[] keep)
    {
        if (end - start < 2) return;

        double maxDist = 0;
        int maxIndex = start;
        double toleranceSq = tolerance * tolerance;

        Point2D a = polygon[start];
        Point2D b = polygon[end];
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double lenSq = dx * dx + dy * dy;

        for (int i = start + 1; i < end; i++)
        {
            double distSq;
            if (lenSq < Tolerance * Tolerance)
            {
                double pdx = polygon[i].X - a.X;
                double pdy = polygon[i].Y - a.Y;
                distSq = pdx * pdx + pdy * pdy;
            }
            else
            {
                double t = ((polygon[i].X - a.X) * dx + (polygon[i].Y - a.Y) * dy) / lenSq;
                double projX = a.X + t * dx;
                double projY = a.Y + t * dy;
                double ex = polygon[i].X - projX;
                double ey = polygon[i].Y - projY;
                distSq = ex * ex + ey * ey;
            }

            if (distSq > maxDist)
            {
                maxDist = distSq;
                maxIndex = i;
            }
        }

        if (maxDist > toleranceSq)
        {
            keep[maxIndex] = true;
            DouglasPeuckerRecurse(polygon, start, maxIndex, tolerance, keep);
            DouglasPeuckerRecurse(polygon, maxIndex, end, tolerance, keep);
        }
    }

    private static void ReevaluateNode(
        LinkedListNode<Point2D> node,
        LinkedListNode<Point2D>?[] nodes,
        double[] areas,
        SortedSet<(double Area, int Index)> heap,
        LinkedList<Point2D> list)
    {
        int nodeIdx = -1;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] == node)
            {
                nodeIdx = i;
                break;
            }
        }

        if (nodeIdx < 0) return;

        LinkedListNode<Point2D> prev = node.Previous ?? list.Last!;
        LinkedListNode<Point2D> next = node.Next ?? list.First!;

        if (prev == null || next == null || prev == next) return;

        if (areas[nodeIdx] >= 0)
            heap.Remove((areas[nodeIdx], nodeIdx));

        areas[nodeIdx] = ComputeTriangleArea(prev.Value, node.Value, next.Value);
        if (areas[nodeIdx] >= 0)
            heap.Add((areas[nodeIdx], nodeIdx));
    }

    private static double ComputeTriangleArea(Point2D a, Point2D b, Point2D c)
    {
        return System.Math.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) * 0.5;
    }
}
