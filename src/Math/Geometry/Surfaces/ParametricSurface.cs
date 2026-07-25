namespace MathVerse.Math.Geometry.Surfaces;

using Geometry3D;

/// <summary>Represents a parametric surface defined by a function of two parameters.</summary>
public sealed class ParametricSurface
{
    private readonly Func<double, double, Point3D> _evaluate;

    /// <summary>Initializes a new instance of the <see cref="ParametricSurface"/> class.</summary>
    /// <param name="evaluate">The parametric function (u, v) -> Point3D.</param>
    /// <param name="uMin">The minimum u parameter value.</param>
    /// <param name="uMax">The maximum u parameter value.</param>
    /// <param name="vMin">The minimum v parameter value.</param>
    /// <param name="vMax">The maximum v parameter value.</param>
    public ParametricSurface(Func<double, double, Point3D> evaluate, double uMin, double uMax, double vMin, double vMax)
    {
        _evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
        UMin = uMin;
        UMax = uMax;
        VMin = vMin;
        VMax = vMax;
    }

    /// <summary>Gets the minimum u parameter value.</summary>
    public double UMin { get; }

    /// <summary>Gets the maximum u parameter value.</summary>
    public double UMax { get; }

    /// <summary>Gets the minimum v parameter value.</summary>
    public double VMin { get; }

    /// <summary>Gets the maximum v parameter value.</summary>
    public double VMax { get; }

    /// <summary>Evaluates the surface at parameters (u, v).</summary>
    /// <param name="u">The u parameter value.</param>
    /// <param name="v">The v parameter value.</param>
    /// <returns>The point on the surface.</returns>
    public Point3D Evaluate(double u, double v) => _evaluate(u, v);

    /// <summary>Computes the surface normal at (u, v) via the cross product of partial derivatives.</summary>
    /// <param name="u">The u parameter value.</param>
    /// <param name="v">The v parameter value.</param>
    /// <returns>The unit normal vector.</returns>
    public Vector3D Normal(double u, double v)
    {
        double eps = 1e-6;
        Point3D center = _evaluate(u, v);
        Point3D du = _evaluate(u + eps, v);
        Point3D dv = _evaluate(u, v + eps);

        Vector3D tangentU = new Vector3D(du.X - center.X, du.Y - center.Y, du.Z - center.Z);
        Vector3D tangentV = new Vector3D(dv.X - center.X, dv.Y - center.Y, dv.Z - center.Z);

        return tangentU.Cross(tangentV).Normalize();
    }

    /// <summary>Samples the surface on a regular grid.</summary>
    /// <param name="uCount">The number of samples along the u axis.</param>
    /// <param name="vCount">The number of samples along the v axis.</param>
    /// <returns>A 2D immutable array of sampled points.</returns>
    public ImmutableArray<ImmutableArray<Point3D>> Sample(int uCount, int vCount)
    {
        var rows = ImmutableArray.CreateBuilder<ImmutableArray<Point3D>>(uCount);
        for (int i = 0; i < uCount; i++)
        {
            double u = UMin + (UMax - UMin) * i / (uCount - 1);
            var row = ImmutableArray.CreateBuilder<Point3D>(vCount);
            for (int j = 0; j < vCount; j++)
            {
                double v = VMin + (VMax - VMin) * j / (vCount - 1);
                row.Add(_evaluate(u, v));
            }
            rows.Add(row.MoveToImmutable());
        }
        return rows.MoveToImmutable();
    }

    /// <summary>Computes the partial derivative with respect to u at (u, v).</summary>
    /// <param name="u">The u parameter value.</param>
    /// <param name="v">The v parameter value.</param>
    /// <returns>The tangent vector in the u direction.</returns>
    public Vector3D TangentU(double u, double v)
    {
        double eps = 1e-6;
        Point3D p0 = _evaluate(u - eps, v);
        Point3D p1 = _evaluate(u + eps, v);
        return new Vector3D(
            (p1.X - p0.X) / (2.0 * eps),
            (p1.Y - p0.Y) / (2.0 * eps),
            (p1.Z - p0.Z) / (2.0 * eps));
    }

    /// <summary>Computes the partial derivative with respect to v at (u, v).</summary>
    /// <param name="u">The u parameter value.</param>
    /// <param name="v">The v parameter value.</param>
    /// <returns>The tangent vector in the v direction.</returns>
    public Vector3D TangentV(double u, double v)
    {
        double eps = 1e-6;
        Point3D p0 = _evaluate(u, v - eps);
        Point3D p1 = _evaluate(u, v + eps);
        return new Vector3D(
            (p1.X - p0.X) / (2.0 * eps),
            (p1.Y - p0.Y) / (2.0 * eps),
            (p1.Z - p0.Z) / (2.0 * eps));
    }
}
