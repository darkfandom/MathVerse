using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Voronoi;

/// <summary>
/// Clips Voronoi cells and edges to a bounding box using the Sutherland-Hodgman algorithm.
/// </summary>
public static class VoronoiClipper
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Clips each Voronoi cell polygon to the specified bounding box using Sutherland-Hodgman clipping.
    /// </summary>
    /// <param name="cells">The input Voronoi cells.</param>
    /// <param name="bounds">The clipping bounding box.</param>
    /// <returns>An immutable array of clipped Voronoi cells.</returns>
    public static ImmutableArray<VoronoiCell> ClipToBounds(ImmutableArray<VoronoiCell> cells, BoundingBox2D bounds)
    {
        var result = ImmutableArray.CreateBuilder<VoronoiCell>(cells.Length);

        for (int i = 0; i < cells.Length; i++)
        {
            VoronoiCell cell = cells[i];
            if (cell.Vertices.Length == 0)
            {
                result.Add(cell);
                continue;
            }

            ImmutableArray<Point2D> clipped = ClipPolygon(cell.Vertices, bounds);
            result.Add(new VoronoiCell(cell.SiteId, clipped));
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Clips each Voronoi edge segment to the specified bounding box, discarding edges outside the bounds.
    /// </summary>
    /// <param name="edges">The input Voronoi edges.</param>
    /// <param name="bounds">The clipping bounding box.</param>
    /// <returns>An immutable array of clipped Voronoi edges that lie within the bounds.</returns>
    public static ImmutableArray<VoronoiEdge> ClipEdges(ImmutableArray<VoronoiEdge> edges, BoundingBox2D bounds)
    {
        var result = ImmutableArray.CreateBuilder<VoronoiEdge>(edges.Length);

        for (int i = 0; i < edges.Length; i++)
        {
            VoronoiEdge edge = edges[i];
            Point2D clipped1 = ClipPoint(edge.P1, bounds);
            Point2D clipped2 = ClipPoint(edge.P2, bounds);

            bool p1Inside = bounds.Contains(edge.P1);
            bool p2Inside = bounds.Contains(edge.P2);

            if (p1Inside && p2Inside)
            {
                result.Add(edge);
            }
            else
            {
                var seg = new Segment2D(edge.P1, edge.P2);
                var clippedPoints = ClipSegmentToBounds(seg, bounds);
                if (clippedPoints.Length >= 2)
                {
                    result.Add(new VoronoiEdge(
                        clippedPoints[0], clippedPoints[clippedPoints.Length - 1],
                        edge.Site1, edge.Site2));
                }
            }
        }

        return result.ToImmutable();
    }

    private static ImmutableArray<Point2D> ClipPolygon(ImmutableArray<Point2D> polygon, BoundingBox2D bounds)
    {
        Point2D[] output = polygon.ToArray();
        int count = output.Length;

        double[] clipEdgesX = { bounds.Min.X, bounds.Max.X, bounds.Max.X, bounds.Min.X };
        double[] clipEdgesY = { bounds.Min.Y, bounds.Min.Y, bounds.Max.Y, bounds.Max.Y };

        for (int edge = 0; edge < 4; edge++)
        {
            if (count == 0) break;

            var input = new Point2D[count];
            System.Array.Copy(output, input, count);
            int inputCount = count;
            count = 0;

            for (int i = 0; i < inputCount; i++)
            {
                Point2D current = input[i];
                Point2D previous = input[(i + inputCount - 1) % inputCount];

                bool currentInside = IsInsideClipEdge(current, edge, bounds);
                bool previousInside = IsInsideClipEdge(previous, edge, bounds);

                if (currentInside)
                {
                    if (!previousInside)
                    {
                        Point2D? intersection = ClipLineIntersect(previous, current, edge, bounds);
                        if (intersection.HasValue)
                        {
                            output[count++] = intersection.Value;
                        }
                    }
                    output[count++] = current;
                }
                else if (previousInside)
                {
                    Point2D? intersection = ClipLineIntersect(previous, current, edge, bounds);
                    if (intersection.HasValue)
                    {
                        output[count++] = intersection.Value;
                    }
                }
            }
        }

        if (count < 3)
            return ImmutableArray<Point2D>.Empty;

        var result = ImmutableArray.CreateBuilder<Point2D>(count);
        for (int i = 0; i < count; i++)
            result.Add(output[i]);
        return result.ToImmutable();
    }

    private static bool IsInsideClipEdge(Point2D p, int edge, BoundingBox2D bounds)
    {
        return edge switch
        {
            0 => p.X >= bounds.Min.X - Tolerance,
            1 => p.X <= bounds.Max.X + Tolerance,
            2 => p.Y <= bounds.Max.Y + Tolerance,
            3 => p.Y >= bounds.Min.Y - Tolerance,
            _ => false
        };
    }

    private static Point2D? ClipLineIntersect(Point2D p1, Point2D p2, int edge, BoundingBox2D bounds)
    {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;
        double t = 0;

        switch (edge)
        {
            case 0:
                if (System.Math.Abs(dx) < Tolerance) return null;
                t = (bounds.Min.X - p1.X) / dx;
                break;
            case 1:
                if (System.Math.Abs(dx) < Tolerance) return null;
                t = (bounds.Max.X - p1.X) / dx;
                break;
            case 2:
                if (System.Math.Abs(dy) < Tolerance) return null;
                t = (bounds.Max.Y - p1.Y) / dy;
                break;
            case 3:
                if (System.Math.Abs(dy) < Tolerance) return null;
                t = (bounds.Min.Y - p1.Y) / dy;
                break;
        }

        if (t < 0 || t > 1) return null;
        return new Point2D(p1.X + t * dx, p1.Y + t * dy);
    }

    private static Point2D ClipPoint(Point2D p, BoundingBox2D bounds)
    {
        return new Point2D(
            System.Math.Clamp(p.X, bounds.Min.X, bounds.Max.X),
            System.Math.Clamp(p.Y, bounds.Min.Y, bounds.Max.Y));
    }

    private static ImmutableArray<Point2D> ClipSegmentToBounds(Segment2D seg, BoundingBox2D bounds)
    {
        double dx = seg.P2.X - seg.P1.X;
        double dy = seg.P2.Y - seg.P1.Y;

        double tMin = 0, tMax = 1;

        if (System.Math.Abs(dx) > Tolerance)
        {
            double t1 = (bounds.Min.X - seg.P1.X) / dx;
            double t2 = (bounds.Max.X - seg.P1.X) / dx;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tMin = System.Math.Max(tMin, t1);
            tMax = System.Math.Min(tMax, t2);
        }
        else if (seg.P1.X < bounds.Min.X || seg.P1.X > bounds.Max.X)
        {
            return ImmutableArray<Point2D>.Empty;
        }

        if (System.Math.Abs(dy) > Tolerance)
        {
            double t1 = (bounds.Min.Y - seg.P1.Y) / dy;
            double t2 = (bounds.Max.Y - seg.P1.Y) / dy;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tMin = System.Math.Max(tMin, t1);
            tMax = System.Math.Min(tMax, t2);
        }
        else if (seg.P1.Y < bounds.Min.Y || seg.P1.Y > bounds.Max.Y)
        {
            return ImmutableArray<Point2D>.Empty;
        }

        if (tMin > tMax + Tolerance)
            return ImmutableArray<Point2D>.Empty;

        var result = ImmutableArray.CreateBuilder<Point2D>(2);
        result.Add(new Point2D(seg.P1.X + tMin * dx, seg.P1.Y + tMin * dy));
        if (tMax > tMin + Tolerance)
            result.Add(new Point2D(seg.P1.X + tMax * dx, seg.P1.Y + tMax * dy));

        return result.ToImmutable();
    }
}
