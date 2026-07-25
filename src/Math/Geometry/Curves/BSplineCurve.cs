namespace MathVerse.Math.Geometry.Curves;

using Geometry3D;

/// <summary>Represents a B-spline curve defined by a knot vector, control points, and degree.</summary>
public sealed class BSplineCurve
{
    /// <summary>Initializes a new instance of the <see cref="BSplineCurve"/> class.</summary>
    /// <param name="knots">The knot vector (non-decreasing sequence of doubles).</param>
    /// <param name="controlPoints">The control points.</param>
    /// <param name="degree">The degree of the B-spline basis functions.</param>
    public BSplineCurve(ImmutableArray<double> knots, ImmutableArray<Point3D> controlPoints, int degree)
    {
        Knots = knots.IsDefault ? ImmutableArray<double>.Empty : knots;
        ControlPoints = controlPoints.IsDefault ? ImmutableArray<Point3D>.Empty : controlPoints;
        Degree = degree;
    }

    /// <summary>Gets the knot vector.</summary>
    public ImmutableArray<double> Knots { get; }

    /// <summary>Gets the control points.</summary>
    public ImmutableArray<Point3D> ControlPoints { get; }

    /// <summary>Gets the degree of the B-spline basis functions.</summary>
    public int Degree { get; }

    /// <summary>Evaluates the B-spline curve at parameter t using the Cox-de Boor algorithm.</summary>
    /// <param name="t">The parameter value.</param>
    /// <returns>The point on the curve.</returns>
    public Point3D PointAt(double t)
    {
        int n = ControlPoints.Length;
        if (n == 0) return Point3D.Origin;
        if (n == 1) return ControlPoints[0];

        int p = Degree;
        double[] N = new double[n];

        for (int i = 0; i < n; i++)
        {
            bool inSpan = t >= Knots[i] && t < Knots[i + p + 1];
            bool isLast = i == n - 1 && System.Math.Abs(t - Knots[Knots.Length - 1]) < 1e-12;
            N[i] = (inSpan || isLast) ? 1.0 : 0.0;
        }

        for (int k = 1; k <= p; k++)
        {
            double[] Nprev = (double[])N.Clone();
            for (int i = 0; i < n; i++)
            {
                double left = 0.0;
                double right = 0.0;

                double denomLeft = Knots[i + k] - Knots[i];
                if (System.Math.Abs(denomLeft) > 1e-15)
                    left = ((t - Knots[i]) / denomLeft) * Nprev[i];

                double denomRight = Knots[i + k + 1] - Knots[i + 1];
                if (System.Math.Abs(denomRight) > 1e-15)
                    right = ((Knots[i + k + 1] - t) / denomRight) * Nprev[i + 1];

                N[i] = left + right;
            }
        }

        double px = 0.0, py = 0.0, pz = 0.0;
        for (int i = 0; i < n; i++)
        {
            px += N[i] * ControlPoints[i].X;
            py += N[i] * ControlPoints[i].Y;
            pz += N[i] * ControlPoints[i].Z;
        }

        return new Point3D(px, py, pz);
    }

    /// <summary>Samples the curve at n evenly-spaced parameter values.</summary>
    /// <param name="n">The number of sample points (must be at least 2).</param>
    /// <returns>An immutable list of points on the curve.</returns>
    public IReadOnlyList<Point3D> Sample(int n)
    {
        double tMin = Knots[Degree];
        double tMax = Knots[Knots.Length - Degree - 1];
        var points = new List<Point3D>(n);
        for (int i = 0; i < n; i++)
        {
            double t = tMin + (tMax - tMin) * i / (n - 1);
            points.Add(PointAt(t));
        }
        return points;
    }

    /// <summary>Inserts a new knot into the knot vector without changing the curve geometry.</summary>
    /// <param name="knot">The knot value to insert.</param>
    /// <returns>A new B-spline curve with the inserted knot.</returns>
    public BSplineCurve InsertKnot(double knot)
    {
        int p = Degree;
        int n = ControlPoints.Length;

        int k = 0;
        for (int i = 0; i < Knots.Length - 1; i++)
        {
            if (knot >= Knots[i] && knot < Knots[i + 1])
            {
                k = i;
                break;
            }
        }

        var newKnots = Knots.Add(knot);

        var newControlPoints = new List<Point3D>(n + 1);
        for (int i = 0; i <= n; i++)
        {
            double alpha;
            double denomLeft = Knots[i + p] - Knots[i];
            double denomRight = Knots[i + p + 1] - Knots[i + 1];

            if (System.Math.Abs(denomLeft) < 1e-15)
                alpha = 1.0;
            else if (System.Math.Abs(denomRight) < 1e-15)
                alpha = 0.0;
            else
                alpha = (knot - Knots[i]) / denomLeft;

            if (i <= k - p + 1)
                newControlPoints.Add(ControlPoints[i]);
            else if (i >= k + 1)
                newControlPoints.Add(ControlPoints[i - 1]);
            else
            {
                Point3D prev = ControlPoints[i - 1];
                Point3D curr = ControlPoints[i];
                newControlPoints.Add(new Point3D(
                    alpha * curr.X + (1.0 - alpha) * prev.X,
                    alpha * curr.Y + (1.0 - alpha) * prev.Y,
                    alpha * curr.Z + (1.0 - alpha) * prev.Z));
            }
        }

        return new BSplineCurve(newKnots, newControlPoints.ToImmutableArray(), p);
    }

    /// <summary>Computes the derivative B-spline curve.</summary>
    /// <returns>A new B-spline curve representing the first derivative.</returns>
    public BSplineCurve Derivative()
    {
        int p = Degree;
        int n = ControlPoints.Length;

        if (n < 2 || p < 1)
            return new BSplineCurve(ImmutableArray<double>.Empty, ImmutableArray<Point3D>.Empty, System.Math.Max(p - 1, 0));

        var derivControlPoints = new Point3D[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            double denom = Knots[i + p + 1] - Knots[i + 1];
            double factor = System.Math.Abs(denom) > 1e-15 ? p / denom : 0.0;
            derivControlPoints[i] = new Point3D(
                factor * (ControlPoints[i + 1].X - ControlPoints[i].X),
                factor * (ControlPoints[i + 1].Y - ControlPoints[i].Y),
                factor * (ControlPoints[i + 1].Z - ControlPoints[i].Z));
        }

        var derivKnots = ImmutableArray.CreateBuilder<double>(Knots.Length - 2);
        for (int i = 1; i < Knots.Length - 1; i++)
            derivKnots.Add(Knots[i]);

        return new BSplineCurve(derivKnots.ToImmutable(), derivControlPoints.ToImmutableArray(), p - 1);
    }
}
