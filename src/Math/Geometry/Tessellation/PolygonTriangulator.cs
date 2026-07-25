using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Tessellation;

/// <summary>Provides static methods for ear-clipping polygon triangulation.</summary>
public static class PolygonTriangulator
{
    /// <summary>Triangulates a 2D polygon using the ear-clipping algorithm.</summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <returns>An immutable array of triangles covering the polygon.</returns>
    public static ImmutableArray<Triangle2D> Triangulate(IReadOnlyList<Point2D> polygon)
    {
        if (polygon.Count < 3)
            return ImmutableArray<Triangle2D>.Empty;

        List<int> indices = new(polygon.Count);
        for (int i = 0; i < polygon.Count; i++)
            indices.Add(i);

        double area = SignedArea(polygon);
        bool reverseNeeded = area < 0;

        if (reverseNeeded)
            indices.Reverse();

        ImmutableArray<Triangle2D>.Builder result = ImmutableArray.CreateBuilder<Triangle2D>();

        while (indices.Count > 2)
        {
            bool earFound = false;

            for (int i = 0; i < indices.Count; i++)
            {
                int prev = indices[(i - 1 + indices.Count) % indices.Count];
                int current = indices[i];
                int next = indices[(i + 1) % indices.Count];

                if (IsEar(polygon, prev, current, next))
                {
                    result.Add(new Triangle2D(polygon[prev], polygon[current], polygon[next]));
                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound)
                break;
        }

        return result.ToImmutable();
    }

    /// <summary>Triangulates a 3D polygon lying on a plane defined by its normal.</summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <param name="normal">The plane normal of the polygon.</param>
    /// <returns>An immutable array of 3D triangles covering the polygon.</returns>
    public static ImmutableArray<Triangle3D> Triangulate(IReadOnlyList<Point3D> polygon, Vector3D normal)
    {
        if (polygon.Count < 3)
            return ImmutableArray<Triangle3D>.Empty;

        Point3D origin = polygon[0];
        Vector3D n = normal.Normalize();

        Vector3D u;
        if (System.Math.Abs(n.X) < System.Math.Abs(n.Y))
            u = new Vector3D(0, -n.Z, n.Y).Normalize();
        else
            u = new Vector3D(-n.Z, 0, n.X).Normalize();

        Vector3D v = n.Cross(u);

        List<Point2D> projected = new(polygon.Count);
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3D diff = new(
                polygon[i].X - origin.X,
                polygon[i].Y - origin.Y,
                polygon[i].Z - origin.Z);
            projected.Add(new Point2D(diff.Dot(u), diff.Dot(v)));
        }

        ImmutableArray<Triangle2D> triangles2D = Triangulate(projected);
        ImmutableArray<Triangle3D>.Builder result = ImmutableArray.CreateBuilder<Triangle3D>(triangles2D.Length);

        for (int i = 0; i < triangles2D.Length; i++)
        {
            Triangle2D t2d = triangles2D[i];
            result.Add(new Triangle3D(
                Unproject(t2d.A, origin, u, v),
                Unproject(t2d.B, origin, u, v),
                Unproject(t2d.C, origin, u, v)));
        }

        return result.ToImmutable();
    }

    /// <summary>Determines whether the vertex at <paramref name="current"/> is an ear of the polygon.</summary>
    /// <param name="polygon">The polygon vertices.</param>
    /// <param name="prev">Index of the previous vertex.</param>
    /// <param name="current">Index of the current vertex.</param>
    /// <param name="next">Index of the next vertex.</param>
    /// <returns>True if the vertex is an ear; otherwise, false.</returns>
    public static bool IsEar(IReadOnlyList<Point2D> polygon, int prev, int current, int next)
    {
        Point2D a = polygon[prev];
        Point2D b = polygon[current];
        Point2D c = polygon[next];

        double cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        if (cross <= 0)
            return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            if (i == prev || i == current || i == next)
                continue;

            if (PointInTriangle(polygon[i], a, b, c))
                return false;
        }

        return true;
    }

    /// <summary>Computes the signed area of a 2D polygon using the shoelace formula.</summary>
    /// <param name="polygon">The polygon vertices.</param>
    /// <returns>The signed area (positive for counter-clockwise winding).</returns>
    public static double SignedArea(IReadOnlyList<Point2D> polygon)
    {
        double area = 0.0;
        int count = polygon.Count;

        for (int i = 0; i < count; i++)
        {
            Point2D current = polygon[i];
            Point2D next = polygon[(i + 1) % count];
            area += current.X * next.Y - next.X * current.Y;
        }

        return area * 0.5;
    }

    /// <summary>Ensures the polygon has the specified winding order.</summary>
    /// <param name="polygon">The polygon vertices.</param>
    /// <param name="order">The desired winding order.</param>
    /// <returns>The polygon with the correct winding order.</returns>
    public static IReadOnlyList<Point2D> EnsureWinding(IReadOnlyList<Point2D> polygon, WindingOrder order)
    {
        if (polygon.Count < 2)
            return polygon;

        double area = SignedArea(polygon);
        bool isCounterClockwise = area > 0;
        bool desiredCCW = order == WindingOrder.CounterClockwise;

        if (isCounterClockwise == desiredCCW)
            return polygon;

        List<Point2D> reversed = new(polygon.Count);
        for (int i = polygon.Count - 1; i >= 0; i--)
            reversed.Add(polygon[i]);

        return reversed;
    }

    private static bool PointInTriangle(Point2D p, Point2D a, Point2D b, Point2D c)
    {
        double d1 = CrossSign(a, b, p);
        double d2 = CrossSign(b, c, p);
        double d3 = CrossSign(c, a, p);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    private static double CrossSign(Point2D a, Point2D b, Point2D c)
    {
        return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    }

    private static Point3D Unproject(Point2D p, Point3D origin, Vector3D u, Vector3D v)
    {
        return new Point3D(
            origin.X + p.X * u.X + p.Y * v.X,
            origin.Y + p.X * u.Y + p.Y * v.Y,
            origin.Z + p.X * u.Z + p.Y * v.Z);
    }
}
