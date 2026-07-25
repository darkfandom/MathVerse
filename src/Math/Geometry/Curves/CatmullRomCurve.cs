namespace MathVerse.Math.Geometry.Curves;

using Geometry3D;

/// <summary>Represents a Catmull-Rom spline curve through a set of points with configurable tension.</summary>
public sealed class CatmullRomCurve
{
    /// <summary>Initializes a new instance of the <see cref="CatmullRomCurve"/> class.</summary>
    /// <param name="points">The interpolation points (at least 4).</param>
    /// <param name="tension">The tension parameter (0 = uniform, 0.5 = centripetal, 1.0 = chord).</param>
    public CatmullRomCurve(ImmutableArray<Point3D> points, double tension = 0.5)
    {
        Points = points.IsDefault ? ImmutableArray<Point3D>.Empty : points;
        Tension = tension;
    }

    /// <summary>Gets the interpolation points.</summary>
    public ImmutableArray<Point3D> Points { get; }

    /// <summary>Gets the tension parameter.</summary>
    public double Tension { get; }

    /// <summary>Evaluates the curve at parameter t, where t ranges from 0 to Points.Length - 1.</summary>
    /// <param name="t">The parameter value.</param>
    /// <returns>The point on the curve.</returns>
    public Point3D PointAt(double t)
    {
        int count = Points.Length;
        if (count == 0) return Point3D.Origin;
        if (count == 1) return Points[0];
        if (count == 2) return Points[0].Lerp(Points[1], System.Math.Clamp(t, 0.0, 1.0));

        double scaledT = System.Math.Clamp(t, 0.0, count - 1);
        int segment = (int)System.Math.Floor(scaledT);
        if (segment >= count - 1) segment = count - 2;
        double localT = scaledT - segment;

        Point3D p0 = Points[System.Math.Max(segment - 1, 0)];
        Point3D p1 = Points[segment];
        Point3D p2 = Points[System.Math.Min(segment + 1, count - 1)];
        Point3D p3 = Points[System.Math.Min(segment + 2, count - 1)];

        double tt = localT;
        double t2 = tt * tt;
        double t3 = t2 * tt;

        double s = Tension;

        double m1x = (1.0 - s) * (p2.X - p0.X) * 0.5;
        double m1y = (1.0 - s) * (p2.Y - p0.Y) * 0.5;
        double m1z = (1.0 - s) * (p2.Z - p0.Z) * 0.5;
        double m2x = (1.0 - s) * (p3.X - p1.X) * 0.5;
        double m2y = (1.0 - s) * (p3.Y - p1.Y) * 0.5;
        double m2z = (1.0 - s) * (p3.Z - p1.Z) * 0.5;

        double h00 = 2.0 * t3 - 3.0 * t2 + 1.0;
        double h10 = t3 - 2.0 * t2 + tt;
        double h01 = -2.0 * t3 + 3.0 * t2;
        double h11 = t3 - t2;

        return new Point3D(
            h00 * p1.X + h10 * m1x + h01 * p2.X + h11 * m2x,
            h00 * p1.Y + h10 * m1y + h01 * p2.Y + h11 * m2y,
            h00 * p1.Z + h10 * m1z + h01 * p2.Z + h11 * m2z);
    }

    /// <summary>Samples the curve at n evenly-spaced parameter values.</summary>
    /// <param name="n">The number of sample points (must be at least 2).</param>
    /// <returns>An immutable list of points on the curve.</returns>
    public IReadOnlyList<Point3D> Sample(int n)
    {
        int count = Points.Length;
        if (count < 2) return new List<Point3D>(Points);

        var points = new List<Point3D>(n);
        double maxT = count - 1;
        for (int i = 0; i < n; i++)
        {
            double t = maxT * i / (n - 1);
            points.Add(PointAt(t));
        }
        return points;
    }
}
