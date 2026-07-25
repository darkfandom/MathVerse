using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.ComputationalGeometry;

/// <summary>Implements the Sutherland-Hodgman polygon clipping algorithm.</summary>
public static class SutherlandHodgmanClipper
{
    /// <summary>Clips a subject polygon against a convex clip polygon.</summary>
    /// <param name="subject">The subject polygon to clip.</param>
    /// <param name="clip">The convex clip polygon.</param>
    /// <returns>The clipped polygon.</returns>
    public static Polygon2D Clip(Polygon2D subject, Polygon2D clip)
    {
        if (subject.VertexCount < 3 || clip.VertexCount < 3)
            return new Polygon2D(ImmutableArray<Point2D>.Empty);

        List<Point2D> output = new(subject.Vertices.AsSpan().ToArray());

        for (int i = 0; i < clip.VertexCount; i++)
        {
            if (output.Count == 0) break;

            Point2D edgeStart = clip.Vertices[i];
            Point2D edgeEnd = clip.Vertices[(i + 1) % clip.VertexCount];
            List<Point2D> input = new(output);
            output.Clear();

            for (int j = 0; j < input.Count; j++)
            {
                Point2D current = input[j];
                Point2D previous = input[(j + input.Count - 1) % input.Count];

                bool currentInside = IsInside(current, edgeStart, edgeEnd);
                bool previousInside = IsInside(previous, edgeStart, edgeEnd);

                if (currentInside)
                {
                    if (!previousInside)
                        output.Add(ComputeIntersection(previous, current, edgeStart, edgeEnd));
                    output.Add(current);
                }
                else if (previousInside)
                {
                    output.Add(ComputeIntersection(previous, current, edgeStart, edgeEnd));
                }
            }
        }

        return output.Count >= 3
            ? new Polygon2D(output.ToImmutableArray())
            : new Polygon2D(ImmutableArray<Point2D>.Empty);
    }

    /// <summary>Clips a line segment against a convex polygon, returning the clipped segment.</summary>
    public static (bool hit, Point2D p1, Point2D p2) ClipSegment(Segment2D segment, Polygon2D clip)
    {
        if (clip.VertexCount < 3) return (false, Point2D.Origin, Point2D.Origin);

        Point2D p1 = segment.P1, p2 = segment.P2;
        double tmin = 0, tmax = 1;

        for (int i = 0; i < clip.VertexCount; i++)
        {
            Point2D edgeStart = clip.Vertices[i];
            Point2D edgeEnd = clip.Vertices[(i + 1) % clip.VertexCount];
            Vector2D edgeNormal = new(edgeEnd.Y - edgeStart.Y, -(edgeEnd.X - edgeStart.X));

            double dx = p2.X - p1.X, dy = p2.Y - p1.Y;
            double denom = dx * edgeNormal.X + dy * edgeNormal.Y;
            double numer = (edgeStart.X - p1.X) * edgeNormal.X + (edgeStart.Y - p1.Y) * edgeNormal.Y;

            if (System.Math.Abs(denom) < 1e-15)
            {
                if (numer < 0) return (false, Point2D.Origin, Point2D.Origin);
                continue;
            }

            double t = numer / denom;
            if (denom < 0) tmin = System.Math.Max(tmin, t);
            else tmax = System.Math.Min(tmax, t);

            if (tmin > tmax) return (false, Point2D.Origin, Point2D.Origin);
        }

        Point2D c1 = new(p1.X + tmin * (p2.X - p1.X), p1.Y + tmin * (p2.Y - p1.Y));
        Point2D c2 = new(p1.X + tmax * (p2.X - p1.X), p1.Y + tmax * (p2.Y - p1.Y));
        return (true, c1, c2);
    }

    /// <summary>Tests whether a point is inside a convex polygon using the cross-product test.</summary>
    public static bool IsInsideConvex(Point2D p, Polygon2D convexClip)
    {
        if (convexClip.VertexCount < 3) return false;
        for (int i = 0; i < convexClip.VertexCount; i++)
        {
            Point2D a = convexClip.Vertices[i];
            Point2D b = convexClip.Vertices[(i + 1) % convexClip.VertexCount];
            double cross = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
            if (cross < -1e-10) return false;
        }
        return true;
    }

    private static bool IsInside(Point2D p, Point2D edgeStart, Point2D edgeEnd) =>
        (edgeEnd.X - edgeStart.X) * (p.Y - edgeStart.Y) - (edgeEnd.Y - edgeStart.Y) * (p.X - edgeStart.X) >= 0;

    private static Point2D ComputeIntersection(Point2D p1, Point2D p2, Point2D edgeStart, Point2D edgeEnd)
    {
        double x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
        double x3 = edgeStart.X, y3 = edgeStart.Y, x4 = edgeEnd.X, y4 = edgeEnd.Y;

        double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (System.Math.Abs(denom) < 1e-30) return p1;

        double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        return new Point2D(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
    }
}
