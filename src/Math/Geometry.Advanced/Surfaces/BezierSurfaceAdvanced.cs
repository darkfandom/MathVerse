using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Surfaces;

/// <summary>
/// Represents a point on a surface with both position and surface normal information.
/// </summary>
/// <param name="Position">The 3D position of the surface point.</param>
/// <param name="Normal">The outward-facing surface normal at this point.</param>
public readonly record struct SurfacePoint(Point3D Position, Vector3D Normal);

/// <summary>
/// Provides advanced evaluation and tessellation of Bézier surfaces defined by an n×m grid of control points.
/// </summary>
public sealed class BezierSurfaceAdvanced
{
    private readonly ImmutableArray<ImmutableArray<Point3D>> _controlPoints;
    private readonly int _degreeU;
    private readonly int _degreeV;

    /// <summary>
    /// Initializes a new instance of the <see cref="BezierSurfaceAdvanced"/> class with the specified control point grid.
    /// </summary>
    /// <param name="controlPoints">An n×m grid of control points defining the Bézier surface. Each inner array represents a row in the v-direction.</param>
    /// <exception cref="ArgumentException">Thrown when the control point grid is empty or not rectangular.</exception>
    public BezierSurfaceAdvanced(ImmutableArray<ImmutableArray<Point3D>> controlPoints)
    {
        if (controlPoints.Length == 0)
            throw new ArgumentException("Control point grid must not be empty.", nameof(controlPoints));

        for (int i = 0; i < controlPoints.Length; i++)
        {
            if (controlPoints[i].Length == 0)
                throw new ArgumentException("Each row of control points must not be empty.", nameof(controlPoints));
            if (controlPoints[i].Length != controlPoints[0].Length)
                throw new ArgumentException("All rows must have the same number of control points.", nameof(controlPoints));
        }

        _controlPoints = controlPoints;
        _degreeU = controlPoints.Length - 1;
        _degreeV = controlPoints[0].Length - 1;
    }

    /// <summary>
    /// Evaluates the surface position at the given parametric coordinates (u, v) using De Casteljau's algorithm for bivariate Bézier evaluation.
    /// </summary>
    /// <param name="u">The parametric coordinate in the u-direction, typically in [0, 1].</param>
    /// <param name="v">The parametric coordinate in the v-direction, typically in [0, 1].</param>
    /// <returns>The 3D point on the surface at parameters (u, v).</returns>
    public Point3D Evaluate(double u, double v)
    {
        ImmutableArray<Point3D> rowResults = DeCasteljauRows(u);
        return DeCasteljauColumn(rowResults, v);
    }

    /// <summary>
    /// Computes the surface normal vector at the given parametric coordinates (u, v) via cross product of partial derivatives.
    /// </summary>
    /// <param name="u">The parametric coordinate in the u-direction.</param>
    /// <param name="v">The parametric coordinate in the v-direction.</param>
    /// <returns>The normalized surface normal vector at (u, v).</returns>
    public Vector3D Normal(double u, double v)
    {
        const double Epsilon = 1e-10;
        Vector3D du = PartialDerivativeU(u, v, Epsilon);
        Vector3D dv = PartialDerivativeV(u, v, Epsilon);

        Vector3D cross = Cross(du, dv);
        double length = System.Math.Sqrt(cross.X * cross.X + cross.Y * cross.Y + cross.Z * cross.Z);

        if (length < Epsilon)
            return new Vector3D(0, 0, 1);

        return new Vector3D(cross.X / length, cross.Y / length, cross.Z / length);
    }

    /// <summary>
    /// Evaluates both the surface position and the surface normal at the given parametric coordinates (u, v).
    /// </summary>
    /// <param name="u">The parametric coordinate in the u-direction.</param>
    /// <param name="v">The parametric coordinate in the v-direction.</param>
    /// <returns>A <see cref="SurfacePoint"/> containing the position and normal at (u, v).</returns>
    public SurfacePoint EvaluateWithNormal(double u, double v)
    {
        Point3D position = Evaluate(u, v);
        Vector3D normal = Normal(u, v);
        return new SurfacePoint(position, normal);
    }

    /// <summary>
    /// Returns a copy of the control point grid that defines this Bézier surface.
    /// </summary>
    /// <returns>An immutable array of rows, where each row is an immutable array of <see cref="Point3D"/>.</returns>
    public ImmutableArray<ImmutableArray<Point3D>> GetControlPoints()
    {
        return _controlPoints;
    }

    /// <summary>
    /// Generates a tessellated mesh of the surface by evaluating positions and normals at a regular grid of parametric coordinates.
    /// </summary>
    /// <param name="uSegments">The number of subdivisions in the u-direction. Must be at least 1.</param>
    /// <param name="vSegments">The number of subdivisions in the v-direction. Must be at least 1.</param>
    /// <returns>An immutable array of <see cref="SurfacePoint"/> arranged in row-major order (u varies fastest).</returns>
    /// <exception cref="ArgumentException">Thrown when segment counts are less than 1.</exception>
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
                builder.Add(EvaluateWithNormal(u, v));
            }
        }

        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Evaluates the De Casteljau algorithm along rows of the control grid, reducing the v-dimension.
    /// </summary>
    private ImmutableArray<Point3D> DeCasteljauRows(double u)
    {
        int rowCount = _controlPoints.Length;
        var temp = ImmutableArray.CreateBuilder<Point3D>(rowCount);

        for (int i = 0; i < rowCount; i++)
        {
            temp.Add(DeCasteljau1D(_controlPoints[i], u));
        }

        return temp.MoveToImmutable();
    }

    /// <summary>
    /// Evaluates the De Casteljau algorithm in 1D on a single row of points.
    /// </summary>
    private static Point3D DeCasteljau1D(ImmutableArray<Point3D> points, double t)
    {
        int n = points.Length;
        var tmp = new Point3D[n];
        points.CopyTo(tmp, 0);

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                tmp[i] = Lerp(tmp[i], tmp[i + 1], t);
            }
        }

        return tmp[0];
    }

    /// <summary>
    /// Evaluates the De Casteljau algorithm on a column of intermediate results.
    /// </summary>
    private static Point3D DeCasteljauColumn(ImmutableArray<Point3D> points, double t)
    {
        return DeCasteljau1D(points, t);
    }

    /// <summary>
    /// Computes the partial derivative of the surface with respect to u using central finite differences.
    /// </summary>
    private Vector3D PartialDerivativeU(double u, double v, double epsilon)
    {
        double u0 = System.Math.Max(0.0, u - epsilon);
        double u1 = System.Math.Min(1.0, u + epsilon);
        double h = u1 - u0;
        if (h < epsilon)
            h = epsilon;

        Point3D p0 = Evaluate(u0, v);
        Point3D p1 = Evaluate(u1, v);
        return new Vector3D((p1.X - p0.X) / h, (p1.Y - p0.Y) / h, (p1.Z - p0.Z) / h);
    }

    /// <summary>
    /// Computes the partial derivative of the surface with respect to v using central finite differences.
    /// </summary>
    private Vector3D PartialDerivativeV(double u, double v, double epsilon)
    {
        double v0 = System.Math.Max(0.0, v - epsilon);
        double v1 = System.Math.Min(1.0, v + epsilon);
        double h = v1 - v0;
        if (h < epsilon)
            h = epsilon;

        Point3D p0 = Evaluate(u, v0);
        Point3D p1 = Evaluate(u, v1);
        return new Vector3D((p1.X - p0.X) / h, (p1.Y - p0.Y) / h, (p1.Z - p0.Z) / h);
    }

    /// <summary>
    /// Linearly interpolates between two points.
    /// </summary>
    private static Point3D Lerp(Point3D a, Point3D b, double t)
    {
        double mt = 1.0 - t;
        return new Point3D(
            mt * a.X + t * b.X,
            mt * a.Y + t * b.Y,
            mt * a.Z + t * b.Z);
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
}
