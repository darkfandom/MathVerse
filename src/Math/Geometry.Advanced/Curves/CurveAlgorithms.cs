using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Curves;

/// <summary>
/// Provides algorithms for analyzing and manipulating 3D curves represented as polylines.
/// </summary>
public static class CurveAlgorithms
{
    /// <summary>
    /// Computes the arc length of a polyline curve using adaptive subdivision with Simpson's rule integration.
    /// </summary>
    /// <param name="curve">The polyline curve defined by an ordered sequence of 3D points.</param>
    /// <param name="subdivisions">The number of subdivisions for numerical integration. Higher values yield greater accuracy. Default is 100.</param>
    /// <returns>The total arc length of the curve.</returns>
    /// <exception cref="ArgumentException">Thrown when the curve has fewer than 2 points.</exception>
    public static double ArcLength(ImmutableArray<Point3D> curve, int subdivisions = 100)
    {
        if (curve.Length < 2)
            throw new ArgumentException("Curve must have at least 2 points.", nameof(curve));
        if (subdivisions < 1)
            throw new ArgumentException("Subdivisions must be at least 1.", nameof(subdivisions));

        double totalLength = 0.0;

        for (int seg = 0; seg < curve.Length - 1; seg++)
        {
            Point3D p0 = curve[seg];
            Point3D p1 = curve[seg + 1];

            double h = 1.0 / subdivisions;
            double sum = SegmentLength(p0, p1, 0.0) + SegmentLength(p0, p1, 1.0);

            for (int i = 1; i < subdivisions; i++)
            {
                double t = (double)i / subdivisions;
                double weight = (i % 2 == 0) ? 2.0 : 4.0;
                sum += weight * SegmentLength(p0, p1, t);
            }

            totalLength += sum * h / 3.0;
        }

        return totalLength;
    }

    /// <summary>
    /// Computes the discrete curvature at a vertex using Menger curvature (inverse of the circumradius of 3 consecutive points).
    /// </summary>
    /// <param name="curve">The polyline curve.</param>
    /// <param name="index">The index of the vertex at which to compute curvature. Must be in [1, curve.Length - 2].</param>
    /// <returns>The curvature value (1/circumradius). Returns 0 if the points are collinear.</returns>
    /// <exception cref="ArgumentException">Thrown when the index is out of the valid range or the curve has fewer than 3 points.</exception>
    public static double Curvature(ImmutableArray<Point3D> curve, int index)
    {
        if (curve.Length < 3)
            throw new ArgumentException("Curve must have at least 3 points for curvature computation.", nameof(curve));
        if (index < 1 || index >= curve.Length - 1)
            throw new ArgumentException($"Index must be in [1, {curve.Length - 2}].", nameof(index));

        Point3D p0 = curve[index - 1];
        Point3D p1 = curve[index];
        Point3D p2 = curve[index + 1];

        double a = Distance(p0, p1);
        double b = Distance(p1, p2);
        double c = Distance(p0, p2);

        if (a < 1e-15 || b < 1e-15 || c < 1e-15)
            return 0.0;

        double s = (a + b + c) / 2.0;
        double areaArg = s * (s - a) * (s - b) * (s - c);
        if (areaArg < 1e-30)
            return 0.0;

        double area = System.Math.Sqrt(areaArg);
        double circumradius = (a * b * c) / (4.0 * area);

        if (circumradius < 1e-15)
            return 0.0;

        return 1.0 / circumradius;
    }

    /// <summary>
    /// Computes the discrete torsion at a vertex using 4 consecutive points on the curve.
    /// Torsion measures the rate at which the curve deviates from its osculating plane.
    /// </summary>
    /// <param name="curve">The polyline curve.</param>
    /// <param name="index">The index of the vertex at which to compute torsion. Must be in [1, curve.Length - 2].</param>
    /// <returns>The torsion value. Returns 0 if the 4 points are coplanar.</returns>
    /// <exception cref="ArgumentException">Thrown when the index is out of the valid range or the curve has fewer than 4 points.</exception>
    public static double Torsion(ImmutableArray<Point3D> curve, int index)
    {
        if (curve.Length < 4)
            throw new ArgumentException("Curve must have at least 4 points for torsion computation.", nameof(curve));
        if (index < 1 || index >= curve.Length - 2)
            throw new ArgumentException($"Index must be in [1, {curve.Length - 2}] for torsion.", nameof(index));

        Point3D p0 = curve[index - 1];
        Point3D p1 = curve[index];
        Point3D p2 = curve[index + 1];
        Point3D p3 = curve[index + 2];

        Vector3D d1 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
        Vector3D d2 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        Vector3D d3 = new Vector3D(p3.X - p2.X, p3.Y - p2.Y, p3.Z - p2.Z);

        Vector3D cross12 = Cross(d1, d2);
        double numerator = cross12.X * d3.X + cross12.Y * d3.Y + cross12.Z * d3.Z;

        double crossLen = System.Math.Sqrt(cross12.X * cross12.X + cross12.Y * cross12.Y + cross12.Z * cross12.Z);
        double denominator = crossLen * crossLen;

        if (denominator < 1e-30)
            return 0.0;

        return numerator / denominator;
    }

    /// <summary>
    /// Finds the closest point on the polyline curve to a given query point using brute-force segment testing.
    /// </summary>
    /// <param name="curve">The polyline curve.</param>
    /// <param name="query">The query point to find the closest point to.</param>
    /// <returns>The point on the polyline closest to the query point.</returns>
    /// <exception cref="ArgumentException">Thrown when the curve has fewer than 2 points.</exception>
    public static Point3D ClosestPoint(ImmutableArray<Point3D> curve, Point3D query)
    {
        if (curve.Length < 2)
            throw new ArgumentException("Curve must have at least 2 points.", nameof(curve));

        double bestDistSq = double.MaxValue;
        Point3D bestPoint = curve[0];

        for (int i = 0; i < curve.Length - 1; i++)
        {
            Point3D projected = ClosestPointOnSegment(curve[i], curve[i + 1], query);
            double dx = projected.X - query.X;
            double dy = projected.Y - query.Y;
            double dz = projected.Z - query.Z;
            double distSq = dx * dx + dy * dy + dz * dz;

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                bestPoint = projected;
            }
        }

        return bestPoint;
    }

    /// <summary>
    /// Adaptively subdivides a polyline curve to approximate the true curve within the specified error tolerance.
    /// Segments where the midpoint deviates from the chord by more than the tolerance are recursively subdivided.
    /// </summary>
    /// <param name="curve">The polyline curve to subdivide.</param>
    /// <param name="maxError">The maximum allowed deviation between the midpoint and the chord.</param>
    /// <returns>A new polyline with additional points where needed to meet the error tolerance.</returns>
    /// <exception cref="ArgumentException">Thrown when the curve has fewer than 2 points.</exception>
    public static ImmutableArray<Point3D> AdaptiveSubdivide(ImmutableArray<Point3D> curve, double maxError)
    {
        if (curve.Length < 2)
            throw new ArgumentException("Curve must have at least 2 points.", nameof(curve));

        var result = ImmutableArray.CreateBuilder<Point3D>();
        result.Add(curve[0]);

        for (int i = 0; i < curve.Length - 1; i++)
        {
            AdaptiveSubdivideSegment(curve[i], curve[i + 1], maxError, result);
        }

        result.Add(curve[curve.Length - 1]);
        return result.MoveToImmutable();
    }

    /// <summary>
    /// Subdivides a polyline curve by inserting linearly interpolated points between existing vertices.
    /// </summary>
    /// <param name="curve">The polyline curve to subdivide.</param>
    /// <param name="segments">The number of subdivisions to insert between each pair of consecutive points. Must be at least 1.</param>
    /// <returns>A new polyline with additional linearly interpolated points.</returns>
    /// <exception cref="ArgumentException">Thrown when the curve has fewer than 2 points or segments is less than 1.</exception>
    public static ImmutableArray<Point3D> SubdivideLinear(ImmutableArray<Point3D> curve, int segments)
    {
        if (curve.Length < 2)
            throw new ArgumentException("Curve must have at least 2 points.", nameof(curve));
        if (segments < 1)
            throw new ArgumentException("Segments must be at least 1.", nameof(segments));

        var result = ImmutableArray.CreateBuilder<Point3D>();

        for (int i = 0; i < curve.Length - 1; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                double t = (double)j / segments;
                result.Add(Lerp(curve[i], curve[i + 1], t));
            }
        }

        result.Add(curve[curve.Length - 1]);
        return result.MoveToImmutable();
    }

    /// <summary>
    /// Computes the arc length contribution of a segment at a given parameter using Simpson's rule sample.
    /// </summary>
    private static double SegmentLength(Point3D p0, Point3D p1, double t)
    {
        Point3D p = Lerp(p0, p1, t);
        Point3D q = Lerp(p0, p1, t + 1e-8);
        double dt = 1e-8;
        double dx = (q.X - p.X) / dt;
        double dy = (q.Y - p.Y) / dt;
        double dz = (q.Z - p.Z) / dt;
        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Computes the closest point on a line segment to a query point.
    /// </summary>
    private static Point3D ClosestPointOnSegment(Point3D a, Point3D b, Point3D query)
    {
        Vector3D ab = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        Vector3D aq = new Vector3D(query.X - a.X, query.Y - a.Y, query.Z - a.Z);

        double abLenSq = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
        if (abLenSq < 1e-30)
            return a;

        double t = (aq.X * ab.X + aq.Y * ab.Y + aq.Z * ab.Z) / abLenSq;
        t = Clamp01(t);

        return Lerp(a, b, t);
    }

    /// <summary>
    /// Recursively subdivides a segment where the midpoint deviation exceeds the tolerance.
    /// </summary>
    private static void AdaptiveSubdivideSegment(Point3D a, Point3D b, double maxError, ImmutableArray<Point3D>.Builder result)
    {
        Point3D mid = Lerp(a, b, 0.5);
        double deviation = PointToSegmentDistance(mid, a, b);

        if (deviation <= maxError)
            return;

        AdaptiveSubdivideSegment(a, mid, maxError, result);
        result.Add(mid);
        AdaptiveSubdivideSegment(mid, b, maxError, result);
    }

    /// <summary>
    /// Computes the distance from a point to the line segment defined by two endpoints.
    /// </summary>
    private static double PointToSegmentDistance(Point3D p, Point3D a, Point3D b)
    {
        Vector3D ab = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        Vector3D ap = new Vector3D(p.X - a.X, p.Y - a.Y, p.Z - a.Z);

        double abLenSq = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
        if (abLenSq < 1e-30)
            return System.Math.Sqrt(ap.X * ap.X + ap.Y * ap.Y + ap.Z * ap.Z);

        double t = Clamp01((ap.X * ab.X + ap.Y * ab.Y + ap.Z * ab.Z) / abLenSq);

        Point3D proj = Lerp(a, b, t);
        double dx = p.X - proj.X;
        double dy = p.Y - proj.Y;
        double dz = p.Z - proj.Z;
        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Linearly interpolates between two points.
    /// </summary>
    private static Point3D Lerp(Point3D a, Point3D b, double t)
    {
        double mt = 1.0 - t;
        return new Point3D(mt * a.X + t * b.X, mt * a.Y + t * b.Y, mt * a.Z + t * b.Z);
    }

    /// <summary>
    /// Computes the Euclidean distance between two points.
    /// </summary>
    private static double Distance(Point3D a, Point3D b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double dz = b.Z - a.Z;
        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    private static Vector3D Cross(Vector3D a, Vector3D b)
    {
        return new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);
    }

    /// <summary>
    /// Clamps a value to the range [0, 1].
    /// </summary>
    private static double Clamp01(double value)
    {
        if (value < 0.0) return 0.0;
        if (value > 1.0) return 1.0;
        return value;
    }
}
