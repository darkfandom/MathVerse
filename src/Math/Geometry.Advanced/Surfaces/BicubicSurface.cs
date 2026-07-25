using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Surfaces;

/// <summary>
/// Represents a bicubic Hermite surface defined by a 4×4 grid of geometry data including positions, tangent vectors, and cross-tangent (twist) vectors.
/// </summary>
public sealed class BicubicSurface
{
    private readonly ImmutableArray<ImmutableArray<Point3D>> _points;
    private readonly ImmutableArray<ImmutableArray<Vector3D>> _tangentsU;
    private readonly ImmutableArray<ImmutableArray<Vector3D>> _tangentsV;
    private readonly ImmutableArray<ImmutableArray<Vector3D>> _crossTangents;

    /// <summary>
    /// Initializes a new instance of the <see cref="BicubicSurface"/> class with a 4×4 bicubic Hermite specification.
    /// </summary>
    /// <param name="points">A 4×4 grid of position values at the corners of the bicubic patch. Row index corresponds to v-parameter, column index to u-parameter.</param>
    /// <param name="tangentsU">A 4×4 grid of tangent vectors in the u-direction at each grid point.</param>
    /// <param name="tangentsV">A 4×4 grid of tangent vectors in the v-direction at each grid point.</param>
    /// <param name="crossTangents">A 4×4 grid of cross-tangent (twist) vectors at each grid point.</param>
    /// <exception cref="ArgumentException">Thrown when any input grid is not 4×4.</exception>
    public BicubicSurface(
        ImmutableArray<ImmutableArray<Point3D>> points,
        ImmutableArray<ImmutableArray<Vector3D>> tangentsU,
        ImmutableArray<ImmutableArray<Vector3D>> tangentsV,
        ImmutableArray<ImmutableArray<Vector3D>> crossTangents)
    {
        ValidateGrid(points, nameof(points));
        ValidateVectorGrid(tangentsU, nameof(tangentsU));
        ValidateVectorGrid(tangentsV, nameof(tangentsV));
        ValidateVectorGrid(crossTangents, nameof(crossTangents));

        _points = points;
        _tangentsU = tangentsU;
        _tangentsV = tangentsV;
        _crossTangents = crossTangents;
    }

    /// <summary>
    /// Evaluates the surface position at parametric coordinates (u, v) using bicubic Hermite interpolation.
    /// The evaluation applies the Hermite basis matrix M to the geometry matrix G: S(u,v) = U^T · M · G · M^T · V.
    /// </summary>
    /// <param name="u">The parametric coordinate in the u-direction, typically in [0, 1].</param>
    /// <param name="v">The parametric coordinate in the v-direction, typically in [0, 1].</param>
    /// <returns>The 3D point on the surface at parameters (u, v).</returns>
    public Point3D Evaluate(double u, double v)
    {
        double u2 = u * u;
        double u3 = u2 * u;
        double v2 = v * v;
        double v3 = v2 * v;

        double[] basisU = { 2.0 * u3 - 3.0 * u2 + 1.0, u3 - 2.0 * u2 + u, -2.0 * u3 + 3.0 * u2, u3 - u2 };
        double[] basisV = { 2.0 * v3 - 3.0 * v2 + 1.0, v3 - 2.0 * v2 + v, -2.0 * v3 + 3.0 * v2, v3 - v2 };

        double[] basisDerivU = { 6.0 * u2 - 6.0 * u, 3.0 * u2 - 4.0 * u + 1.0, -6.0 * u2 + 6.0 * u, 3.0 * u2 - 2.0 * u };
        double[] basisDerivV = { 6.0 * v2 - 6.0 * v, 3.0 * v2 - 4.0 * v + 1.0, -6.0 * v2 + 6.0 * v, 3.0 * v2 - 2.0 * v };

        double hx = 0.0, hy = 0.0, hz = 0.0;
        for (int i = 0; i < 4; i++)
        {
            double bu = basisU[i];
            double bdu = basisDerivU[i];
            for (int j = 0; j < 4; j++)
            {
                double bv = basisV[j];
                double bdv = basisDerivV[j];

                double px = _points[i][j].X;
                double py = _points[i][j].Y;
                double pz = _points[i][j].Z;

                double tux = _tangentsU[i][j].X;
                double tuy = _tangentsU[i][j].Y;
                double tuz = _tangentsU[i][j].Z;

                double tvx = _tangentsV[i][j].X;
                double tvy = _tangentsV[i][j].Y;
                double tvz = _tangentsV[i][j].Z;

                double twx = _crossTangents[i][j].X;
                double twy = _crossTangents[i][j].Y;
                double twz = _crossTangents[i][j].Z;

                double coeff = bu * bv;
                double coeffU = bdu * bv;
                double coeffV = bu * bdv;
                double coeffUV = bdu * bdv;

                hx += px * coeff + tux * coeffU + tvx * coeffV + twx * coeffUV;
                hy += py * coeff + tuy * coeffU + tvy * coeffV + twy * coeffUV;
                hz += pz * coeff + tuz * coeffU + tvz * coeffV + twz * coeffUV;
            }
        }

        return new Point3D(hx, hy, hz);
    }

    /// <summary>
    /// Computes the surface normal vector at parametric coordinates (u, v) via the cross product of partial derivatives ∂S/∂u × ∂S/∂v.
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
    /// Generates a tessellated mesh of the surface by evaluating positions and normals at a regular parametric grid.
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
    /// Validates that a point grid is exactly 4×4.
    /// </summary>
    private static void ValidateGrid(ImmutableArray<ImmutableArray<Point3D>> grid, string paramName)
    {
        if (grid.Length != 4)
            throw new ArgumentException($"{paramName} must have exactly 4 rows.", paramName);
        for (int i = 0; i < 4; i++)
            if (grid[i].Length != 4)
                throw new ArgumentException($"{paramName} row {i} must have exactly 4 columns.", paramName);
    }

    /// <summary>
    /// Validates that a vector grid is exactly 4×4.
    /// </summary>
    private static void ValidateVectorGrid(ImmutableArray<ImmutableArray<Vector3D>> grid, string paramName)
    {
        if (grid.Length != 4)
            throw new ArgumentException($"{paramName} must have exactly 4 rows.", paramName);
        for (int i = 0; i < 4; i++)
            if (grid[i].Length != 4)
                throw new ArgumentException($"{paramName} row {i} must have exactly 4 columns.", paramName);
    }
}
