using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.ConvexHull;

/// <summary>
/// Computes the convex hull of a set of 2D or 3D points using the Graham scan algorithm.
/// </summary>
public static class GrahamScan
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the convex hull of a set of 2D points using the Graham scan algorithm.
    /// </summary>
    /// <param name="points">The input point set.</param>
    /// <returns>
    /// An immutable array of points forming the convex hull in counter-clockwise order.
    /// Returns an empty array if fewer than 3 non-collinear points exist.
    /// </returns>
    public static ImmutableArray<Point2D> Compute(ImmutableArray<Point2D> points)
    {
        if (points.Length <= 2)
            return ImmutableArray.CreateRange(points);

        Point2D[] pts = points.ToArray();
        FindPivot(pts, out int pivotIdx, out Point2D pivot);

        int effectiveCount = SortByAngle(pts, pivotIdx, pivot);

        if (effectiveCount <= 2)
            return ImmutableArray.CreateRange(pts.Take(effectiveCount));

        var stack = new Stack<Point2D>();
        stack.Push(pts[0]);
        stack.Push(pts[1]);

        for (int i = 2; i < effectiveCount; i++)
        {
            while (stack.Count > 1)
            {
                Point2D top = stack.Pop();
                Point2D second = stack.Peek();
                if (Cross2D(second, top, pts[i]) > -Tolerance)
                {
                    stack.Push(top);
                    break;
                }
            }

            stack.Push(pts[i]);
        }

        var result = ImmutableArray.CreateBuilder<Point2D>(stack.Count);
        foreach (Point2D p in stack)
            result.Add(p);

        return result.ToImmutable();
    }

    /// <summary>
    /// Computes the convex hull of a set of 3D points by projecting onto the best-fit plane,
    /// computing the 2D hull, then lifting the result back to 3D.
    /// </summary>
    /// <param name="points">The input 3D point set.</param>
    /// <returns>
    /// An immutable array of 3D points forming the convex hull on the estimated plane.
    /// Returns the input directly if fewer than 3 points exist.
    /// </returns>
    public static ImmutableArray<Point3D> Compute3D(ImmutableArray<Point3D> points)
    {
        if (points.Length <= 3)
            return ImmutableArray.CreateRange(points);

        Point3D centroid = ComputeCentroid(points);
        Vector3D normal = EstimateNormal(points, centroid);

        if (normal.Length < Tolerance)
            return ImmutableArray.CreateRange(points);

        (Vector3D u, Vector3D v) = BuildBasis(normal);

        var projected2D = ImmutableArray.CreateBuilder<Point2D>(points.Length);
        for (int i = 0; i < points.Length; i++)
        {
            Vector3D diff = new Vector3D(points[i].X - centroid.X, points[i].Y - centroid.Y, points[i].Z - centroid.Z);
            double px = diff.Dot(u);
            double py = diff.Dot(v);
            projected2D.Add(new Point2D(px, py));
        }

        ImmutableArray<Point2D> hull2D = Compute(projected2D.ToImmutable());

        var result = ImmutableArray.CreateBuilder<Point3D>(hull2D.Length);
        for (int i = 0; i < hull2D.Length; i++)
        {
            Point2D p2d = hull2D[i];
            Point3D p3d = new Point3D(
                centroid.X + p2d.X * u.X + p2d.Y * v.X,
                centroid.Y + p2d.X * u.Y + p2d.Y * v.Y,
                centroid.Z + p2d.X * u.Z + p2d.Y * v.Z);
            result.Add(p3d);
        }

        return result.ToImmutable();
    }

    private static void FindPivot(Point2D[] pts, out int pivotIdx, out Point2D pivot)
    {
        pivotIdx = 0;
        pivot = pts[0];
        for (int i = 1; i < pts.Length; i++)
        {
            if (pts[i].Y < pivot.Y || (System.Math.Abs(pts[i].Y - pivot.Y) < Tolerance && pts[i].X < pivot.X))
            {
                pivotIdx = i;
                pivot = pts[i];
            }
        }
    }

    private static int SortByAngle(Point2D[] pts, int pivotIdx, Point2D pivot)
    {
        if (pivotIdx != 0)
            (pts[0], pts[pivotIdx]) = (pts[pivotIdx], pts[0]);

        Point2D base_ = pts[0];
        int count = pts.Length;

        System.Array.Sort(pts, 1, count - 1, Comparer<Point2D>.Create((a, b) =>
        {
            double cross = Cross2D(base_, a, b);
            if (System.Math.Abs(cross) < Tolerance)
            {
                double distA = a.DistanceSquaredTo(base_);
                double distB = b.DistanceSquaredTo(base_);
                return distA.CompareTo(distB);
            }
            return cross > 0 ? -1 : 1;
        }));

        int writeIdx = 1;
        for (int i = 1; i < count; i++)
        {
            if (System.Math.Abs(Cross2D(base_, pts[writeIdx - 1], pts[i])) < Tolerance)
            {
                pts[writeIdx - 1] = pts[i];
            }
            else
            {
                writeIdx++;
                pts[writeIdx - 1] = pts[i];
            }
        }

        int newCount = writeIdx;
        for (int i = newCount; i < count; i++)
            pts[i] = default;

        return newCount;
    }

    private static double Cross2D(Point2D o, Point2D a, Point2D b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }

    private static Point3D ComputeCentroid(ImmutableArray<Point3D> points)
    {
        double cx = 0, cy = 0, cz = 0;
        for (int i = 0; i < points.Length; i++)
        {
            cx += points[i].X;
            cy += points[i].Y;
            cz += points[i].Z;
        }
        double inv = 1.0 / points.Length;
        return new Point3D(cx * inv, cy * inv, cz * inv);
    }

    private static Vector3D EstimateNormal(ImmutableArray<Point3D> points, Point3D centroid)
    {
        if (points.Length < 3)
            return Vector3D.Zero;

        Vector3D n = Vector3D.Zero;
        for (int i = 0; i < points.Length; i++)
        {
            int j = (i + 1) % points.Length;
            Vector3D vi = new Vector3D(points[i].X - centroid.X, points[i].Y - centroid.Y, points[i].Z - centroid.Z);
            Vector3D vj = new Vector3D(points[j].X - centroid.X, points[j].Y - centroid.Y, points[j].Z - centroid.Z);
            n = n + vi.Cross(vj);
        }
        return n.Normalize();
    }

    private static (Vector3D u, Vector3D v) BuildBasis(Vector3D normal)
    {
        Vector3D arbitrary;
        if (System.Math.Abs(normal.X) < System.Math.Abs(normal.Y))
            arbitrary = Vector3D.UnitX;
        else
            arbitrary = Vector3D.UnitY;

        Vector3D u = normal.Cross(arbitrary).Normalize();
        Vector3D v = normal.Cross(u).Normalize();
        return (u, v);
    }
}
