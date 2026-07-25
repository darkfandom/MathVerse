using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Surfaces;

/// <summary>
/// Represents a control point for a NURBS surface, consisting of a 3D position and an associated weight.
/// </summary>
/// <param name="Position">The 3D position of the control point.</param>
/// <param name="Weight">The weight associated with this control point. Must be positive for proper surface behavior.</param>
public readonly record struct NURBSControlPoint(Point3D Position, double Weight);

/// <summary>
/// Provides evaluation and tessellation of Non-Uniform Rational B-Spline (NURBS) surfaces.
/// </summary>
public sealed class NURBSSurface
{
    private readonly ImmutableArray<ImmutableArray<NURBSControlPoint>> _controlPoints;
    private readonly ImmutableArray<double> _knotsU;
    private readonly ImmutableArray<double> _knotsV;
    private readonly int _degreeU;
    private readonly int _degreeV;

    /// <summary>
    /// Initializes a new instance of the <see cref="NURBSSurface"/> class with weighted control points, knot vectors, and degrees.
    /// </summary>
    /// <param name="controlPoints">An n×m grid of weighted control points defining the NURBS surface.</param>
    /// <param name="knotsU">The knot vector in the u-direction. Must have length n + degreeU + 1.</param>
    /// <param name="knotsV">The knot vector in the v-direction. Must have length m + degreeV + 1.</param>
    /// <param name="degreeU">The polynomial degree in the u-direction.</param>
    /// <param name="degreeV">The polynomial degree in the v-direction.</param>
    /// <exception cref="ArgumentException">Thrown when dimensions are inconsistent or degrees are invalid.</exception>
    public NURBSSurface(
        ImmutableArray<ImmutableArray<NURBSControlPoint>> controlPoints,
        ImmutableArray<double> knotsU,
        ImmutableArray<double> knotsV,
        int degreeU,
        int degreeV)
    {
        if (controlPoints.Length == 0)
            throw new ArgumentException("Control points must not be empty.", nameof(controlPoints));
        if (degreeU < 1)
            throw new ArgumentException("Degree in u must be at least 1.", nameof(degreeU));
        if (degreeV < 1)
            throw new ArgumentException("Degree in v must be at least 1.", nameof(degreeV));

        int n = controlPoints.Length;
        int m = controlPoints[0].Length;

        for (int i = 0; i < n; i++)
            if (controlPoints[i].Length != m)
                throw new ArgumentException("All rows must have the same number of control points.", nameof(controlPoints));

        if (knotsU.Length != n + degreeU + 1)
            throw new ArgumentException($"Knot vector u must have length {n + degreeU + 1}, got {knotsU.Length}.", nameof(knotsU));
        if (knotsV.Length != m + degreeV + 1)
            throw new ArgumentException($"Knot vector v must have length {m + degreeV + 1}, got {knotsV.Length}.", nameof(knotsV));

        for (int i = 1; i < knotsU.Length; i++)
            if (knotsU[i] < knotsU[i - 1] - 1e-10)
                throw new ArgumentException("Knot vector u must be non-decreasing.", nameof(knotsU));
        for (int i = 1; i < knotsV.Length; i++)
            if (knotsV[i] < knotsV[i - 1] - 1e-10)
                throw new ArgumentException("Knot vector v must be non-decreasing.", nameof(knotsV));

        _controlPoints = controlPoints;
        _knotsU = knotsU;
        _knotsV = knotsV;
        _degreeU = degreeU;
        _degreeV = degreeV;
    }

    /// <summary>
    /// Evaluates the NURBS surface position at parametric coordinates (u, v) by projecting the homogeneous result to 3D.
    /// </summary>
    /// <param name="u">The parametric coordinate in the u-direction.</param>
    /// <param name="v">The parametric coordinate in the v-direction.</param>
    /// <returns>The 3D point on the NURBS surface at parameters (u, v).</returns>
    public Point3D Evaluate(double u, double v)
    {
        double clampedU = Clamp(u, _knotsU[_degreeU], _knotsU[_knotsU.Length - _degreeU - 1]);
        double clampedV = Clamp(v, _knotsV[_degreeV], _knotsV[_knotsV.Length - _degreeV - 1]);

        double sumX = 0.0, sumY = 0.0, sumZ = 0.0, sumW = 0.0;

        int n = _controlPoints.Length;
        int m = _controlPoints[0].Length;

        for (int i = 0; i < n; i++)
        {
            double bu = BSplineSurfaceAdvanced.BasisFunction(_knotsU, i, _degreeU, clampedU);
            if (System.Math.Abs(bu) < 1e-15) continue;

            for (int j = 0; j < m; j++)
            {
                double bv = BSplineSurfaceAdvanced.BasisFunction(_knotsV, j, _degreeV, clampedV);
                if (System.Math.Abs(bv) < 1e-15) continue;

                NURBSControlPoint cp = _controlPoints[i][j];
                double w = bu * bv * cp.Weight;

                sumX += w * cp.Position.X;
                sumY += w * cp.Position.Y;
                sumZ += w * cp.Position.Z;
                sumW += w;
            }
        }

        if (System.Math.Abs(sumW) < 1e-15)
            return new Point3D(0, 0, 0);

        return new Point3D(sumX / sumW, sumY / sumW, sumZ / sumW);
    }

    /// <summary>
    /// Evaluates the NURBS surface in homogeneous (4D) coordinates before perspective division.
    /// </summary>
    /// <param name="u">The parametric coordinate in the u-direction.</param>
    /// <param name="v">The parametric coordinate in the v-direction.</param>
    /// <returns>A <see cref="Point3D"/> representing the homogeneous coordinates as (wx, wy, wz) where w is the homogeneous weight. Use the fourth component for rational division.</returns>
    public Point3D EvaluateHomogeneous(double u, double v)
    {
        double clampedU = Clamp(u, _knotsU[_degreeU], _knotsU[_knotsU.Length - _degreeU - 1]);
        double clampedV = Clamp(v, _knotsV[_degreeV], _knotsV[_knotsV.Length - _degreeV - 1]);

        double sumX = 0.0, sumY = 0.0, sumZ = 0.0, sumW = 0.0;

        int n = _controlPoints.Length;
        int m = _controlPoints[0].Length;

        for (int i = 0; i < n; i++)
        {
            double bu = BSplineSurfaceAdvanced.BasisFunction(_knotsU, i, _degreeU, clampedU);
            if (System.Math.Abs(bu) < 1e-15) continue;

            for (int j = 0; j < m; j++)
            {
                double bv = BSplineSurfaceAdvanced.BasisFunction(_knotsV, j, _degreeV, clampedV);
                if (System.Math.Abs(bv) < 1e-15) continue;

                NURBSControlPoint cp = _controlPoints[i][j];
                double w = bu * bv * cp.Weight;

                sumX += w * cp.Position.X;
                sumY += w * cp.Position.Y;
                sumZ += w * cp.Position.Z;
                sumW += w;
            }
        }

        return new Point3D(sumX, sumY, sumZ);
    }

    /// <summary>
    /// Computes the surface normal vector at parametric coordinates (u, v) via the cross product of partial derivatives.
    /// </summary>
    /// <param name="u">The parametric coordinate in the u-direction.</param>
    /// <param name="v">The parametric coordinate in the v-direction.</param>
    /// <returns>The normalized surface normal vector at (u, v).</returns>
    public Vector3D Normal(double u, double v)
    {
        const double Epsilon = 1e-10;
        double u0 = System.Math.Max(0.0, u - Epsilon);
        double u1 = System.Math.Min(1.0, u + Epsilon);
        double hu = u1 - u0;
        if (hu < Epsilon) hu = Epsilon;

        Point3D p0 = Evaluate(u0, v);
        Point3D p1 = Evaluate(u1, v);
        Vector3D dU = new Vector3D((p1.X - p0.X) / hu, (p1.Y - p0.Y) / hu, (p1.Z - p0.Z) / hu);

        double v0 = System.Math.Max(0.0, v - Epsilon);
        double v1 = System.Math.Min(1.0, v + Epsilon);
        double hv = v1 - v0;
        if (hv < Epsilon) hv = Epsilon;

        Point3D p2 = Evaluate(u, v0);
        Point3D p3 = Evaluate(u, v1);
        Vector3D dV = new Vector3D((p3.X - p2.X) / hv, (p3.Y - p2.Y) / hv, (p3.Z - p2.Z) / hv);

        Vector3D cross = new Vector3D(
            dU.Y * dV.Z - dU.Z * dV.Y,
            dU.Z * dV.X - dU.X * dV.Z,
            dU.X * dV.Y - dU.Y * dV.X);

        double length = System.Math.Sqrt(cross.X * cross.X + cross.Y * cross.Y + cross.Z * cross.Z);
        if (length < Epsilon)
            return new Vector3D(0, 0, 1);

        return new Vector3D(cross.X / length, cross.Y / length, cross.Z / length);
    }

    /// <summary>
    /// Generates a tessellated mesh of the NURBS surface by evaluating positions and normals at a regular parametric grid.
    /// </summary>
    /// <param name="uSegments">The number of subdivisions in the u-direction. Must be at least 1.</param>
    /// <param name="vSegments">The number of subdivisions in the v-direction. Must be at least 1.</param>
    /// <returns>An immutable array of <see cref="SurfacePoint"/> in row-major order (u varies fastest).</returns>
    public ImmutableArray<SurfacePoint> Tessellate(int uSegments, int vSegments)
    {
        if (uSegments < 1)
            throw new ArgumentException("uSegments must be at least 1.", nameof(uSegments));
        if (vSegments < 1)
            throw new ArgumentException("vSegments must be at least 1.", nameof(vSegments));

        var builder = ImmutableArray.CreateBuilder<SurfacePoint>((uSegments + 1) * (vSegments + 1));

        for (int j = 0; j <= vSegments; j++)
        {
            double v = (double)j / vSegments;
            for (int i = 0; i <= uSegments; i++)
            {
                double u = (double)i / uSegments;
                builder.Add(new SurfacePoint(Evaluate(u, v), Normal(u, v)));
            }
        }

        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Clamps a value between a minimum and maximum.
    /// </summary>
    private static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
