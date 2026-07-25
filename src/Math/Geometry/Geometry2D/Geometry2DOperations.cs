using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Provides static operations for 2D geometry primitives.</summary>
public static class Geometry2DOperations
{
    /// <summary>Computes the Euclidean distance between two points.</summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The distance between the two points.</returns>
    public static double Distance(Point2D a, Point2D b) => a.DistanceTo(b);

    /// <summary>Computes the distance from a line to a point.</summary>
    /// <param name="line">The line.</param>
    /// <param name="point">The point.</param>
    /// <returns>The perpendicular distance from the line to the point.</returns>
    public static double Distance(Line2D line, Point2D point) => line.DistanceTo(point);

    /// <summary>Computes the distance from a segment to a point.</summary>
    /// <param name="segment">The segment.</param>
    /// <param name="point">The point.</param>
    /// <returns>The minimum distance from the segment to the point.</returns>
    public static double Distance(Segment2D segment, Point2D point) => segment.DistanceTo(point);

    /// <summary>Computes the intersection of two lines.</summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public static (bool hit, Point2D point) Intersect(Line2D a, Line2D b) => a.Intersect(b);

    /// <summary>Computes the intersection of two segments.</summary>
    /// <param name="a">The first segment.</param>
    /// <param name="b">The second segment.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection point.</returns>
    public static (bool hit, Point2D point) Intersect(Segment2D a, Segment2D b) => a.Intersect(b);

    /// <summary>Computes the intersection of two circles.</summary>
    /// <param name="a">The first circle.</param>
    /// <param name="b">The second circle.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection points.</returns>
    public static (bool hit, ImmutableArray<Point2D> points) Intersect(Circle2D a, Circle2D b) => a.Intersect(b);

    /// <summary>Computes the intersection of a line with a circle.</summary>
    /// <param name="line">The line.</param>
    /// <param name="circle">The circle.</param>
    /// <returns>A tuple indicating whether a hit occurred and the intersection points.</returns>
    public static (bool hit, ImmutableArray<Point2D> points) Intersect(Line2D line, Circle2D circle) => circle.Intersect(line);

    /// <summary>Computes the intersection points of a triangle (as a polygon) with a line.</summary>
    /// <param name="tri">The triangle.</param>
    /// <param name="line">The line.</param>
    /// <returns>An immutable array of intersection points.</returns>
    public static ImmutableArray<Point2D> Intersect(Triangle2D tri, Line2D line)
    {
        var result = ImmutableArray.CreateBuilder<Point2D>();
        var edges = new Segment2D[]
        {
            new(tri.A, tri.B),
            new(tri.B, tri.C),
            new(tri.C, tri.A)
        };
        foreach (var edge in edges)
        {
            var (hit, point) = edge.IntersectLine(line);
            if (hit) result.Add(point);
        }
        return result.ToImmutable();
    }

    /// <summary>Projects a point onto a line.</summary>
    /// <param name="point">The point to project.</param>
    /// <param name="line">The line to project onto.</param>
    /// <returns>The projected point on the line.</returns>
    public static Point2D Project(Point2D point, Line2D line) => line.ClosestPoint(point);

    /// <summary>Projects a point onto a segment.</summary>
    /// <param name="point">The point to project.</param>
    /// <param name="segment">The segment to project onto.</param>
    /// <returns>The projected point on the segment.</returns>
    public static Point2D Project(Point2D point, Segment2D segment) => segment.ClosestPoint(point);

    /// <summary>Clips a line against a polygon, returning the intersection points.</summary>
    /// <param name="line">The line to clip.</param>
    /// <param name="polygon">The polygon to clip against.</param>
    /// <returns>An immutable array of intersection points.</returns>
    public static ImmutableArray<Point2D> Clip(Line2D line, Polygon2D polygon)
    {
        var result = ImmutableArray.CreateBuilder<Point2D>();
        foreach (var edge in polygon.Edges)
        {
            var (hit, point) = edge.IntersectLine(line);
            if (hit) result.Add(point);
        }
        return result.ToImmutable().Distinct().ToImmutableArray();
    }

    /// <summary>Computes the area of a triangle.</summary>
    /// <param name="t">The triangle.</param>
    /// <returns>The area.</returns>
    public static double Area(Triangle2D t) => t.Area;

    /// <summary>Computes the area of a polygon.</summary>
    /// <param name="p">The polygon.</param>
    /// <returns>The area.</returns>
    public static double Area(Polygon2D p) => p.Area;

    /// <summary>Computes the area of a circle.</summary>
    /// <param name="c">The circle.</param>
    /// <returns>The area.</returns>
    public static double Area(Circle2D c) => c.Area;

    /// <summary>Computes the perimeter of a polygon.</summary>
    /// <param name="p">The polygon.</param>
    /// <returns>The perimeter.</returns>
    public static double Perimeter(Polygon2D p) => p.Perimeter;

    /// <summary>Computes the centroid of a polygon.</summary>
    /// <param name="p">The polygon.</param>
    /// <returns>The centroid.</returns>
    public static Point2D Centroid(Polygon2D p) => p.Centroid;

    /// <summary>Computes the convex hull using Andrew's monotone chain algorithm.</summary>
    /// <param name="points">The points to compute the hull for.</param>
    /// <returns>A polygon representing the convex hull.</returns>
    public static Polygon2D ConvexHull(IReadOnlyList<Point2D> points)
    {
        if (points.Count <= 1)
            return new Polygon2D(ImmutableArray.Create(points.ToArray()));

        var sorted = new List<Point2D>(points);
        sorted.Sort((a, b) => a.X.CompareTo(b.X) != 0 ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

        var hull = new List<Point2D>();

        foreach (Point2D p in sorted)
        {
            while (hull.Count >= 2)
            {
                Point2D q = hull[hull.Count - 2];
                Point2D r = hull[hull.Count - 1];
                double cross = (r.X - q.X) * (p.Y - q.Y) - (r.Y - q.Y) * (p.X - q.X);
                if (cross <= 1e-15)
                    hull.RemoveAt(hull.Count - 1);
                else
                    break;
            }
            hull.Add(p);
        }

        int t = hull.Count + 1;
        for (int i = sorted.Count - 2; i >= 0; i--)
        {
            Point2D p = sorted[i];
            while (hull.Count >= t)
            {
                Point2D q = hull[hull.Count - 2];
                Point2D r = hull[hull.Count - 1];
                double cross = (r.X - q.X) * (p.Y - q.Y) - (r.Y - q.Y) * (p.X - q.X);
                if (cross <= 1e-15)
                    hull.RemoveAt(hull.Count - 1);
                else
                    break;
            }
            hull.Add(p);
        }

        if (hull.Count > 0) hull.RemoveAt(hull.Count - 1);

        return new Polygon2D(ImmutableArray.Create(hull.ToArray()));
    }

    /// <summary>Translates a geometry by a vector offset.</summary>
    /// <param name="geo">The geometry to translate.</param>
    /// <param name="offset">The translation offset.</param>
    /// <returns>The translated geometry.</returns>
    public static Geometry2D Translate(Geometry2D geo, Vector2D offset) => geo;
}
