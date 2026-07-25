namespace MathVerse.Geometry.Tests.Curves;

/// <summary>Tests for ParametricCurve2D and ParametricCurve3D.</summary>
public class ParametricCurveTests
{
    private const double Precision = 1e-6;

    /// <summary>Line curve evaluate at t=0 should return start point.</summary>
    [Fact]
    public void Line2D_Evaluate_AtStart_ShouldReturnStartPoint()
    {
        var curve = new ParametricCurve2D(t => new Point2D(t, t * 2), 0, 10);
        Point2D result = curve.Evaluate(0);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Line curve evaluate at t=5 should return midpoint.</summary>
    [Fact]
    public void Line2D_Evaluate_AtMiddle_ShouldReturnCorrectPoint()
    {
        var curve = new ParametricCurve2D(t => new Point2D(t, t * 2), 0, 10);
        Point2D result = curve.Evaluate(5);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>Sample should return the correct number of points.</summary>
    [Fact]
    public void Sample2D_ShouldReturnCorrectCount()
    {
        var curve = new ParametricCurve2D(t => new Point2D(t, 0), 0, 1);
        IReadOnlyList<Point2D> points = curve.Sample(5);
        points.Count.Should().Be(5);
    }

    /// <summary>Sample first point should equal TMin evaluation.</summary>
    [Fact]
    public void Sample2D_FirstPoint_ShouldEqualTMin()
    {
        var curve = new ParametricCurve2D(t => new Point2D(t * 2, t), 0, 10);
        IReadOnlyList<Point2D> points = curve.Sample(11);
        points[0].X.Should().BeApproximately(0.0, Precision);
        points[0].Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Tangent of a horizontal line should point along X axis.</summary>
    [Fact]
    public void Tangent2D_HorizontalLine_ShouldPointAlongX()
    {
        var curve = new ParametricCurve2D(t => new Point2D(t, 0), 0, 10);
        Vector2D tangent = curve.Tangent(5);
        tangent.X.Should().BeApproximately(1.0, Precision);
        tangent.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Curvature of a circle should be approximately 1/r.</summary>
    [Fact]
    public void Curvature2D_Circle_ShouldBeApproximatelyOneOverR()
    {
        double r = 5.0;
        var curve = new ParametricCurve2D(
            t => new Point2D(r * System.Math.Cos(t), r * System.Math.Sin(t)),
            0,
            2 * System.Math.PI);
        double curvature = curve.Curvature(System.Math.PI / 4);
        curvature.Should().BeApproximately(1.0 / r, 1e-3);
    }

    /// <summary>Curvature of a line should be zero.</summary>
    [Fact]
    public void Curvature2D_Line_ShouldBeZero()
    {
        var curve = new ParametricCurve2D(t => new Point2D(t, t), 0, 10);
        double curvature = curve.Curvature(5);
        curvature.Should().BeApproximately(0.0, 1e-4);
    }

    /// <summary>3D curve evaluate at t=0 should return start point.</summary>
    [Fact]
    public void Curve3D_Evaluate_AtStart_ShouldReturnStartPoint()
    {
        var curve = new ParametricCurve3D(t => new Point3D(t, t * 2, t * 3), 0, 10);
        Point3D result = curve.Evaluate(0);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>3D Sample should return the correct number of points.</summary>
    [Fact]
    public void Sample3D_ShouldReturnCorrectCount()
    {
        var curve = new ParametricCurve3D(t => new Point3D(t, 0, 0), 0, 1);
        IReadOnlyList<Point3D> points = curve.Sample(7);
        points.Count.Should().Be(7);
    }

    /// <summary>3D Tangent of helix should be unit length.</summary>
    [Fact]
    public void Tangent3D_Helix_ShouldBeUnitLength()
    {
        var curve = new ParametricCurve3D(
            t => new Point3D(System.Math.Cos(t), System.Math.Sin(t), t),
            0,
            2 * System.Math.PI);
        Vector3D tangent = curve.Tangent(System.Math.PI);
        tangent.Length.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Tangent2D of a circle should be perpendicular to radius.</summary>
    [Fact]
    public void Tangent2D_Circle_ShouldBePerpendicularToRadius()
    {
        double r = 3.0;
        var curve = new ParametricCurve2D(
            t => new Point2D(r * System.Math.Cos(t), r * System.Math.Sin(t)),
            0,
            2 * System.Math.PI);
        double angle = System.Math.PI / 3;
        Point2D p = curve.Evaluate(angle);
        Vector2D tangent = curve.Tangent(angle);
        double dot = p.X * tangent.X + p.Y * tangent.Y;
        dot.Should().BeApproximately(0.0, 1e-4);
    }

    /// <summary>3D line has zero curvature equivalent (tangent is constant).</summary>
    [Fact]
    public void Tangent3D_Line_ShouldBeConstant()
    {
        var curve = new ParametricCurve3D(t => new Point3D(t, t, t), 0, 10);
        Vector3D t1 = curve.Tangent(2);
        Vector3D t2 = curve.Tangent(8);
        t1.X.Should().BeApproximately(t2.X, Precision);
        t1.Y.Should().BeApproximately(t2.Y, Precision);
        t1.Z.Should().BeApproximately(t2.Z, Precision);
    }

    /// <summary>2D tangent should be normalized.</summary>
    [Fact]
    public void Tangent2D_ShouldBeNormalized()
    {
        var curve = new ParametricCurve2D(
            t => new Point2D(System.Math.Cos(t) * 3, System.Math.Sin(t) * 3),
            0,
            2 * System.Math.PI);
        Vector2D tangent = curve.Tangent(1.0);
        tangent.Length.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Sample endpoints should match TMin and TMax evaluations.</summary>
    [Fact]
    public void Sample_Endpoints_ShouldMatchBounds()
    {
        var curve = new ParametricCurve2D(t => new Point2D(t, t * t), 0, 4);
        IReadOnlyList<Point2D> points = curve.Sample(5);
        points[0].X.Should().BeApproximately(0.0, Precision);
        points[^1].X.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Parametric curve TMin and TMax should be set correctly.</summary>
    [Fact]
    public void ParametricCurve_TMinTMax_ShouldBeSetCorrectly()
    {
        var curve = new ParametricCurve2D(t => new Point2D(t, 0), -5, 15);
        curve.TMin.Should().BeApproximately(-5.0, Precision);
        curve.TMax.Should().BeApproximately(15.0, Precision);
    }
}
