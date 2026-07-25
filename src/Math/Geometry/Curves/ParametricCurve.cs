namespace MathVerse.Math.Geometry.Curves;

using Geometry2D;
using Geometry3D;

/// <summary>Evaluates a parametric curve at parameter t.</summary>
/// <param name="t">The parameter value.</param>
/// <returns>The point on the curve at t.</returns>
public delegate Point2D ParametricCurveFunc2D(double t);

/// <summary>Evaluates a parametric curve in 3D at parameter t.</summary>
/// <param name="t">The parameter value.</param>
/// <returns>The point on the curve at t.</returns>
public delegate Point3D ParametricCurveFunc3D(double t);

/// <summary>Represents a parametric 2D curve defined by a delegate and parameter range.</summary>
public sealed class ParametricCurve2D
{
    private readonly ParametricCurveFunc2D _evaluate;

    /// <summary>Initializes a new instance of the <see cref="ParametricCurve2D"/> class.</summary>
    /// <param name="evaluate">The delegate that evaluates the curve at a given parameter.</param>
    /// <param name="tMin">The minimum parameter value.</param>
    /// <param name="tMax">The maximum parameter value.</param>
    public ParametricCurve2D(ParametricCurveFunc2D evaluate, double tMin, double tMax)
    {
        _evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
        TMin = tMin;
        TMax = tMax;
    }

    /// <summary>Gets the minimum parameter value.</summary>
    public double TMin { get; }

    /// <summary>Gets the maximum parameter value.</summary>
    public double TMax { get; }

    /// <summary>Evaluates the curve at parameter t.</summary>
    /// <param name="t">The parameter value.</param>
    /// <returns>The point on the curve.</returns>
    public Point2D Evaluate(double t) => _evaluate(t);

    /// <summary>Samples the curve at n evenly-spaced points across the parameter range.</summary>
    /// <param name="n">The number of sample points (must be at least 2).</param>
    /// <returns>An immutable list of points on the curve.</returns>
    public IReadOnlyList<Point2D> Sample(int n)
    {
        var points = new List<Point2D>(n);
        double step = (TMax - TMin) / (n - 1);
        for (int i = 0; i < n; i++)
            points.Add(_evaluate(TMin + i * step));
        return points;
    }

    /// <summary>Computes the unit tangent vector at parameter t using central differences.</summary>
    /// <param name="t">The parameter value.</param>
    /// <param name="epsilon">The finite difference step size.</param>
    /// <returns>The normalized tangent vector.</returns>
    public Vector2D Tangent(double t, double epsilon = 1e-8)
    {
        Point2D p1 = _evaluate(t - epsilon);
        Point2D p2 = _evaluate(t + epsilon);
        return new Vector2D(p2.X - p1.X, p2.Y - p1.Y).Normalize();
    }

    /// <summary>Computes the signed curvature at parameter t using finite differences.</summary>
    /// <param name="t">The parameter value.</param>
    /// <param name="epsilon">The finite difference step size.</param>
    /// <returns>The curvature value.</returns>
    public double Curvature(double t, double epsilon = 1e-6)
    {
        double h = epsilon;
        Point2D p0 = _evaluate(t - h);
        Point2D p1 = _evaluate(t);
        Point2D p2 = _evaluate(t + h);

        double dx = (p2.X - p0.X) / (2.0 * h);
        double dy = (p2.Y - p0.Y) / (2.0 * h);
        double ddx = (p2.X - 2.0 * p1.X + p0.X) / (h * h);
        double ddy = (p2.Y - 2.0 * p1.Y + p0.Y) / (h * h);

        double num = System.Math.Abs(dx * ddy - dy * ddx);
        double denom = System.Math.Pow(dx * dx + dy * dy, 1.5);
        return denom > 1e-15 ? num / denom : 0.0;
    }
}

/// <summary>Represents a parametric 3D curve defined by a delegate and parameter range.</summary>
public sealed class ParametricCurve3D
{
    private readonly ParametricCurveFunc3D _evaluate;

    /// <summary>Initializes a new instance of the <see cref="ParametricCurve3D"/> class.</summary>
    /// <param name="evaluate">The delegate that evaluates the curve at a given parameter.</param>
    /// <param name="tMin">The minimum parameter value.</param>
    /// <param name="tMax">The maximum parameter value.</param>
    public ParametricCurve3D(ParametricCurveFunc3D evaluate, double tMin, double tMax)
    {
        _evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
        TMin = tMin;
        TMax = tMax;
    }

    /// <summary>Gets the minimum parameter value.</summary>
    public double TMin { get; }

    /// <summary>Gets the maximum parameter value.</summary>
    public double TMax { get; }

    /// <summary>Evaluates the curve at parameter t.</summary>
    /// <param name="t">The parameter value.</param>
    /// <returns>The point on the curve.</returns>
    public Point3D Evaluate(double t) => _evaluate(t);

    /// <summary>Samples the curve at n evenly-spaced points across the parameter range.</summary>
    /// <param name="n">The number of sample points (must be at least 2).</param>
    /// <returns>An immutable list of points on the curve.</returns>
    public IReadOnlyList<Point3D> Sample(int n)
    {
        var points = new List<Point3D>(n);
        double step = (TMax - TMin) / (n - 1);
        for (int i = 0; i < n; i++)
            points.Add(_evaluate(TMin + i * step));
        return points;
    }

    /// <summary>Computes the unit tangent vector at parameter t using central differences.</summary>
    /// <param name="t">The parameter value.</param>
    /// <param name="epsilon">The finite difference step size.</param>
    /// <returns>The normalized tangent vector.</returns>
    public Vector3D Tangent(double t, double epsilon = 1e-8)
    {
        Point3D p1 = _evaluate(t - epsilon);
        Point3D p2 = _evaluate(t + epsilon);
        return new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z).Normalize();
    }
}
