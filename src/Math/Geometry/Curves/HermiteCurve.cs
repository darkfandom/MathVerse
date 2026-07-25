namespace MathVerse.Math.Geometry.Curves;

using Geometry3D;

/// <summary>Represents a cubic Hermite curve defined by start and end positions and tangent vectors.</summary>
public readonly record struct HermiteCurve
{
    /// <summary>Gets the starting point.</summary>
    public Point3D P0 { get; }

    /// <summary>Gets the starting tangent vector.</summary>
    public Vector3D T0 { get; }

    /// <summary>Gets the ending point.</summary>
    public Point3D P1 { get; }

    /// <summary>Gets the ending tangent vector.</summary>
    public Vector3D T1 { get; }

    /// <summary>Initializes a new instance of the <see cref="HermiteCurve"/> struct.</summary>
    /// <param name="p0">The starting point.</param>
    /// <param name="t0">The starting tangent vector.</param>
    /// <param name="p1">The ending point.</param>
    /// <param name="t1">The ending tangent vector.</param>
    public HermiteCurve(Point3D p0, Vector3D t0, Point3D p1, Vector3D t1)
    {
        P0 = p0;
        T0 = t0;
        P1 = p1;
        T1 = t1;
    }

    /// <summary>Evaluates the curve at parameter t in [0, 1].</summary>
    /// <param name="t">The parameter value.</param>
    /// <returns>The point on the curve.</returns>
    public Point3D PointAt(double t)
    {
        double t2 = t * t;
        double t3 = t2 * t;

        double h00 = 2.0 * t3 - 3.0 * t2 + 1.0;
        double h10 = t3 - 2.0 * t2 + t;
        double h01 = -2.0 * t3 + 3.0 * t2;
        double h11 = t3 - t2;

        return new Point3D(
            h00 * P0.X + h10 * T0.X + h01 * P1.X + h11 * T1.X,
            h00 * P0.Y + h10 * T0.Y + h01 * P1.Y + h11 * T1.Y,
            h00 * P0.Z + h10 * T0.Z + h01 * P1.Z + h11 * T1.Z);
    }

    /// <summary>Converts this Hermite curve to a cubic Bezier curve.</summary>
    /// <returns>An equivalent cubic Bezier curve.</returns>
    public BezierCurve3D ToBezier()
    {
        var cp0 = P0;
        var cp1 = new Point3D(P0.X + T0.X / 3.0, P0.Y + T0.Y / 3.0, P0.Z + T0.Z / 3.0);
        var cp2 = new Point3D(P1.X - T1.X / 3.0, P1.Y - T1.Y / 3.0, P1.Z - T1.Z / 3.0);
        var cp3 = P1;
        return new BezierCurve3D(ImmutableArray.Create(cp0, cp1, cp2, cp3));
    }
}
