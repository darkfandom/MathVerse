using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Tessellation;

/// <summary>Provides static methods for adaptive tessellation of curves and surfaces.</summary>
public static class AdaptiveTessellator
{
    /// <summary>Adaptively tessellates a parametric curve f(t) into a polyline.</summary>
    /// <param name="f">The parametric function mapping t to a 2D point.</param>
    /// <param name="tMin">The minimum parameter value.</param>
    /// <param name="tMax">The maximum parameter value.</param>
    /// <param name="minSegments">The minimum number of segments.</param>
    /// <param name="maxSegments">The maximum number of segments.</param>
    /// <param name="tolerance">The maximum deviation tolerance for subdivision.</param>
    /// <returns>An immutable array of points along the curve.</returns>
    public static ImmutableArray<Point2D> TessellateCurve(
        Func<double, Point2D> f,
        double tMin,
        double tMax,
        int minSegments,
        int maxSegments,
        double tolerance)
    {
        List<Point2D> points = new();
        points.Add(f(tMin));
        SubdivideRecursive2D(f, tMin, tMax, points, minSegments, maxSegments, tolerance, 0);
        points.Add(f(tMax));
        return points.ToImmutableArray();
    }

    /// <summary>Adaptively tessellates a parametric surface f(u,v) into a grid of 3D points.</summary>
    /// <param name="f">The parametric function mapping (u,v) to a 3D point.</param>
    /// <param name="uMin">The minimum U parameter value.</param>
    /// <param name="uMax">The maximum U parameter value.</param>
    /// <param name="vMin">The minimum V parameter value.</param>
    /// <param name="vMax">The maximum V parameter value.</param>
    /// <param name="minRes">The minimum resolution per axis.</param>
    /// <param name="maxRes">The maximum resolution per axis.</param>
    /// <param name="tolerance">The maximum deviation tolerance for subdivision.</param>
    /// <returns>An immutable array of rows, each containing 3D points along the U axis.</returns>
    public static ImmutableArray<ImmutableArray<Point3D>> TessellateSurface(
        Func<double, double, Point3D> f,
        double uMin,
        double uMax,
        double vMin,
        double vMax,
        int minRes,
        int maxRes,
        double tolerance)
    {
        int uRes = System.Math.Max(minRes, System.Math.Min(maxRes, minRes));
        int vRes = System.Math.Max(minRes, System.Math.Min(maxRes, minRes));

        double uStep = (uMax - uMin) / System.Math.Max(uRes, 1);
        double vStep = (vMax - vMin) / System.Math.Max(vRes, 1);

        ImmutableArray<ImmutableArray<Point3D>>.Builder rows =
            ImmutableArray.CreateBuilder<ImmutableArray<Point3D>>(vRes + 1);

        for (int j = 0; j <= vRes; j++)
        {
            double v = vMin + j * vStep;
            ImmutableArray<Point3D>.Builder row = ImmutableArray.CreateBuilder<Point3D>(uRes + 1);

            for (int i = 0; i <= uRes; i++)
            {
                double u = uMin + i * uStep;
                row.Add(f(u, v));
            }

            rows.Add(row.ToImmutable());
        }

        return rows.ToImmutable();
    }

    /// <summary>Recursively subdivides an edge based on a splitting predicate.</summary>
    /// <param name="a">The start point.</param>
    /// <param name="b">The end point.</param>
    /// <param name="shouldSplit">A predicate that determines whether the edge should be split.</param>
    /// <param name="maxDepth">The maximum recursion depth.</param>
    /// <returns>An immutable array of points along the subdivided edge.</returns>
    public static ImmutableArray<Point2D> SubdivideEdge(
        Point2D a,
        Point2D b,
        Func<double, bool> shouldSplit,
        int maxDepth)
    {
        List<Point2D> points = new();
        SubdivideEdgeRecursive(a, b, shouldSplit, maxDepth, 0, points);
        return points.ToImmutableArray();
    }

    private static void SubdivideRecursive2D(
        Func<double, Point2D> f,
        double tMin,
        double tMax,
        List<Point2D> points,
        int minSegments,
        int maxSegments,
        double tolerance,
        int depth)
    {
        if (depth >= maxSegments)
            return;

        double tMid = (tMin + tMax) * 0.5;
        Point2D pMin = f(tMin);
        Point2D pMax = f(tMax);
        Point2D pMid = f(tMid);

        Point2D midpoint = pMin.Lerp(pMax, 0.5);
        double deviation = pMid.DistanceTo(midpoint);

        if (deviation <= tolerance && depth >= minSegments)
            return;

        SubdivideRecursive2D(f, tMin, tMid, points, minSegments, maxSegments, tolerance, depth + 1);
        points.Add(pMid);
        SubdivideRecursive2D(f, tMid, tMax, points, minSegments, maxSegments, tolerance, depth + 1);
    }

    private static void SubdivideEdgeRecursive(
        Point2D a,
        Point2D b,
        Func<double, bool> shouldSplit,
        int maxDepth,
        int currentDepth,
        List<Point2D> points)
    {
        if (currentDepth >= maxDepth)
        {
            points.Add(a);
            return;
        }

        double t = 0.5;
        Point2D mid = a.Lerp(b, t);

        double param = (a.X + b.X) * 0.5;
        if (!shouldSplit(param))
        {
            points.Add(a);
            return;
        }

        SubdivideEdgeRecursive(a, mid, shouldSplit, maxDepth, currentDepth + 1, points);
        SubdivideEdgeRecursive(mid, b, shouldSplit, maxDepth, currentDepth + 1, points);
    }
}
