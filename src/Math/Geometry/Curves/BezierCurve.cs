namespace MathVerse.Math.Geometry.Curves;

using Geometry2D;
using Geometry3D;

/// <summary>Represents a Bezier curve defined by control points in 2D.</summary>
public readonly record struct BezierCurve2D
{
    /// <summary>The control points defining this Bezier curve.</summary>
    public ImmutableArray<Point2D> ControlPoints { get; }

    /// <summary>Initializes a new instance of the <see cref="BezierCurve2D"/> struct.</summary>
    /// <param name="controlPoints">The control points (at least 2).</param>
    public BezierCurve2D(ImmutableArray<Point2D> controlPoints)
    {
        ControlPoints = controlPoints.IsDefault ? ImmutableArray<Point2D>.Empty : controlPoints;
    }

    /// <summary>Gets the degree of this Bezier curve (control points - 1).</summary>
    public int Degree => ControlPoints.Length - 1;

    /// <summary>Evaluates the curve at parameter t using De Casteljau's algorithm.</summary>
    /// <param name="t">The parameter value in [0, 1].</param>
    /// <returns>The point on the curve.</returns>
    public Point2D PointAt(double t)
    {
        if (ControlPoints.Length == 0) return Point2D.Origin;
        if (ControlPoints.Length == 1) return ControlPoints[0];

        int n = ControlPoints.Length;
        var temp = new Point2D[n];
        ControlPoints.CopyTo(temp, 0);

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                temp[i] = temp[i].Lerp(temp[i + 1], t);
            }
        }

        return temp[0];
    }

    /// <summary>Computes the derivative (tangent) curve at parameter t.</summary>
    /// <param name="t">The parameter value in [0, 1].</param>
    /// <returns>The derivative vector.</returns>
    public Vector2D Derivative(double t)
    {
        if (ControlPoints.Length < 2) return Vector2D.Zero;

        int n = ControlPoints.Length - 1;
        var temp = new Point2D[ControlPoints.Length];
        ControlPoints.CopyTo(temp, 0);

        for (int k = 1; k < ControlPoints.Length; k++)
        {
            for (int i = 0; i < ControlPoints.Length - k; i++)
            {
                temp[i] = temp[i].Lerp(temp[i + 1], t);
            }
        }

        double dx = n * (ControlPoints[1].X - ControlPoints[0].X);
        double dy = n * (ControlPoints[1].Y - ControlPoints[0].Y);
        return new Vector2D(dx, dy);
    }

    /// <summary>Samples the curve at n evenly-spaced parameter values.</summary>
    /// <param name="n">The number of sample points (must be at least 2).</param>
    /// <returns>An immutable array of points on the curve.</returns>
    public ImmutableArray<Point2D> Sample(int n)
    {
        var builder = ImmutableArray.CreateBuilder<Point2D>(n);
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / (n - 1);
            builder.Add(PointAt(t));
        }
        return builder.MoveToImmutable();
    }

    /// <summary>Creates a cubic Bezier curve from Hermite endpoints and tangents.</summary>
    /// <param name="p0">The starting point.</param>
    /// <param name="t0">The starting tangent.</param>
    /// <param name="p1">The ending point.</param>
    /// <param name="t1">The ending tangent.</param>
    /// <returns>A cubic Bezier curve.</returns>
    public static BezierCurve2D HermiteToBezier(Point2D p0, Vector2D t0, Point2D p1, Vector2D t1)
    {
        var cp0 = p0;
        var cp1 = new Point2D(p0.X + t0.X / 3.0, p0.Y + t0.Y / 3.0);
        var cp2 = new Point2D(p1.X - t1.X / 3.0, p1.Y - t1.Y / 3.0);
        var cp3 = p1;
        return new BezierCurve2D(ImmutableArray.Create(cp0, cp1, cp2, cp3));
    }
}

/// <summary>Represents a Bezier curve defined by control points in 3D.</summary>
public readonly record struct BezierCurve3D
{
    /// <summary>The control points defining this Bezier curve.</summary>
    public ImmutableArray<Point3D> ControlPoints { get; }

    /// <summary>Initializes a new instance of the <see cref="BezierCurve3D"/> struct.</summary>
    /// <param name="controlPoints">The control points (at least 2).</param>
    public BezierCurve3D(ImmutableArray<Point3D> controlPoints)
    {
        ControlPoints = controlPoints.IsDefault ? ImmutableArray<Point3D>.Empty : controlPoints;
    }

    /// <summary>Gets the degree of this Bezier curve (control points - 1).</summary>
    public int Degree => ControlPoints.Length - 1;

    /// <summary>Evaluates the curve at parameter t using De Casteljau's algorithm.</summary>
    /// <param name="t">The parameter value in [0, 1].</param>
    /// <returns>The point on the curve.</returns>
    public Point3D PointAt(double t)
    {
        if (ControlPoints.Length == 0) return Point3D.Origin;
        if (ControlPoints.Length == 1) return ControlPoints[0];

        int n = ControlPoints.Length;
        var temp = new Point3D[n];
        ControlPoints.CopyTo(temp, 0);

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                temp[i] = temp[i].Lerp(temp[i + 1], t);
            }
        }

        return temp[0];
    }

    /// <summary>Computes the derivative (tangent) vector at parameter t.</summary>
    /// <param name="t">The parameter value in [0, 1].</param>
    /// <returns>The derivative vector.</returns>
    public Vector3D Derivative(double t)
    {
        if (ControlPoints.Length < 2) return Vector3D.Zero;

        int n = ControlPoints.Length - 1;
        var temp = new Point3D[ControlPoints.Length];
        ControlPoints.CopyTo(temp, 0);

        for (int k = 1; k < ControlPoints.Length; k++)
        {
            for (int i = 0; i < ControlPoints.Length - k; i++)
            {
                temp[i] = temp[i].Lerp(temp[i + 1], t);
            }
        }

        double dx = n * (ControlPoints[1].X - ControlPoints[0].X);
        double dy = n * (ControlPoints[1].Y - ControlPoints[0].Y);
        double dz = n * (ControlPoints[1].Z - ControlPoints[0].Z);
        return new Vector3D(dx, dy, dz);
    }

    /// <summary>Samples the curve at n evenly-spaced parameter values.</summary>
    /// <param name="n">The number of sample points (must be at least 2).</param>
    /// <returns>An immutable array of points on the curve.</returns>
    public ImmutableArray<Point3D> Sample(int n)
    {
        var builder = ImmutableArray.CreateBuilder<Point3D>(n);
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / (n - 1);
            builder.Add(PointAt(t));
        }
        return builder.MoveToImmutable();
    }

    /// <summary>Creates a cubic Bezier curve from Hermite endpoints and tangents.</summary>
    /// <param name="p0">The starting point.</param>
    /// <param name="t0">The starting tangent.</param>
    /// <param name="p1">The ending point.</param>
    /// <param name="t1">The ending tangent.</param>
    /// <returns>A cubic Bezier curve.</returns>
    public static BezierCurve3D HermiteToBezier(Point3D p0, Vector3D t0, Point3D p1, Vector3D t1)
    {
        var cp0 = p0;
        var cp1 = new Point3D(p0.X + t0.X / 3.0, p0.Y + t0.Y / 3.0, p0.Z + t0.Z / 3.0);
        var cp2 = new Point3D(p1.X - t1.X / 3.0, p1.Y - t1.Y / 3.0, p1.Z - t1.Z / 3.0);
        var cp3 = p1;
        return new BezierCurve3D(ImmutableArray.Create(cp0, cp1, cp2, cp3));
    }
}
