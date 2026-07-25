namespace MathVerse.Geometry.Tests.Curves;

/// <summary>Tests for BezierCurve2D and BezierCurve3D structs.</summary>
public class BezierCurveTests
{
    private const double Precision = 1e-10;

    /// <summary>PointAt t=0 should return the first control point.</summary>
    [Fact]
    public void Bezier2D_PointAt_Zero_ShouldReturnFirstControlPoint()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(1, 2), new Point2D(3, 4), new Point2D(5, 6)));
        Point2D result = curve.PointAt(0);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>PointAt t=1 should return the last control point.</summary>
    [Fact]
    public void Bezier2D_PointAt_One_ShouldReturnLastControlPoint()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(1, 2), new Point2D(3, 4), new Point2D(5, 6)));
        Point2D result = curve.PointAt(1);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Degree of 4 control points should be 3.</summary>
    [Fact]
    public void Bezier2D_Degree_ShouldBeControlPointsMinusOne()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1), new Point2D(0, 1)));
        curve.Degree.Should().Be(3);
    }

    /// <summary>Sample should return the correct number of points.</summary>
    [Fact]
    public void Bezier2D_Sample_ShouldReturnCorrectCount()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 1)));
        ImmutableArray<Point2D> points = curve.Sample(5);
        points.Length.Should().Be(5);
    }

    /// <summary>Linear Bezier at t=0.5 should return midpoint.</summary>
    [Fact]
    public void LinearBezier2D_AtHalf_ShouldReturnMidpoint()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(10, 10)));
        Point2D result = curve.PointAt(0.5);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Quadratic Bezier at t=0.5 should be correct midpoint of midpoint construction.</summary>
    [Fact]
    public void QuadraticBezier2D_AtHalf_ShouldBeCorrect()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(5, 10), new Point2D(10, 0)));
        Point2D result = curve.PointAt(0.5);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Cubic Bezier at t=0.5 with symmetric controls should be at center.</summary>
    [Fact]
    public void CubicBezier2D_AtHalf_ShouldBeCorrect()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 2), new Point2D(3, 2), new Point2D(4, 0)));
        Point2D result = curve.PointAt(0.5);
        result.X.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>Derivative of a linear Bezier should be constant.</summary>
    [Fact]
    public void Derivative2D_Linear_ShouldBeConstant()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(10, 20)));
        Vector2D d1 = curve.Derivative(0);
        Vector2D d2 = curve.Derivative(1);
        d1.X.Should().BeApproximately(d2.X, Precision);
        d1.Y.Should().BeApproximately(d2.Y, Precision);
    }

    /// <summary>HermiteToBezier endpoints should match input endpoints.</summary>
    [Fact]
    public void HermiteToBezier_Endpoints_ShouldMatch()
    {
        var p0 = new Point2D(1, 2);
        var p1 = new Point2D(5, 6);
        var t0 = new Vector2D(3, 0);
        var t1 = new Vector2D(3, 0);
        var curve = BezierCurve2D.HermiteToBezier(p0, t0, p1, t1);
        Point2D start = curve.PointAt(0);
        Point2D end = curve.PointAt(1);
        start.X.Should().BeApproximately(p0.X, Precision);
        start.Y.Should().BeApproximately(p0.Y, Precision);
        end.X.Should().BeApproximately(p1.X, Precision);
        end.Y.Should().BeApproximately(p1.Y, Precision);
    }

    /// <summary>3D Bezier PointAt t=0 should return first control point.</summary>
    [Fact]
    public void Bezier3D_PointAt_Zero_ShouldReturnFirstControlPoint()
    {
        var curve = new BezierCurve3D(ImmutableArray.Create(
            new Point3D(1, 2, 3), new Point3D(4, 5, 6)));
        Point3D result = curve.PointAt(0);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
        result.Z.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>3D Bezier PointAt t=1 should return last control point.</summary>
    [Fact]
    public void Bezier3D_PointAt_One_ShouldReturnLastControlPoint()
    {
        var curve = new BezierCurve3D(ImmutableArray.Create(
            new Point3D(1, 2, 3), new Point3D(4, 5, 6)));
        Point3D result = curve.PointAt(1);
        result.X.Should().BeApproximately(4.0, Precision);
        result.Y.Should().BeApproximately(5.0, Precision);
        result.Z.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>3D Bezier Degree should be correct.</summary>
    [Fact]
    public void Bezier3D_Degree_ShouldBeControlPointsMinusOne()
    {
        var curve = new BezierCurve3D(ImmutableArray.Create(
            new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 1, 0)));
        curve.Degree.Should().Be(2);
    }

    /// <summary>3D Bezier Sample should return correct count.</summary>
    [Fact]
    public void Bezier3D_Sample_ShouldReturnCorrectCount()
    {
        var curve = new BezierCurve3D(ImmutableArray.Create(
            new Point3D(0, 0, 0), new Point3D(1, 1, 1)));
        ImmutableArray<Point3D> points = curve.Sample(10);
        points.Length.Should().Be(10);
    }

    /// <summary>3D HermiteToBezier endpoints should match input.</summary>
    [Fact]
    public void HermiteToBezier3D_Endpoints_ShouldMatch()
    {
        var p0 = new Point3D(1, 2, 3);
        var p1 = new Point3D(7, 8, 9);
        var t0 = new Vector3D(1, 0, 0);
        var t1 = new Vector3D(-1, 0, 0);
        var curve = BezierCurve3D.HermiteToBezier(p0, t0, p1, t1);
        Point3D start = curve.PointAt(0);
        Point3D end = curve.PointAt(1);
        start.X.Should().BeApproximately(p0.X, Precision);
        start.Y.Should().BeApproximately(p0.Y, Precision);
        start.Z.Should().BeApproximately(p0.Z, Precision);
        end.X.Should().BeApproximately(p1.X, Precision);
        end.Y.Should().BeApproximately(p1.Y, Precision);
        end.Z.Should().BeApproximately(p1.Z, Precision);
    }

    /// <summary>2D Derivative should give direction toward second control point at t=0.</summary>
    [Fact]
    public void Derivative2D_AtZero_ShouldPointTowardSecondCP()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(5, 10)));
        Vector2D d = curve.Derivative(0);
        d.X.Should().BeApproximately(5.0, Precision);
        d.Y.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>3D Derivative should be non-zero for non-trivial curve.</summary>
    [Fact]
    public void Derivative3D_NonTrivial_ShouldBeNonZero()
    {
        var curve = new BezierCurve3D(ImmutableArray.Create(
            new Point3D(0, 0, 0), new Point3D(1, 2, 3)));
        Vector3D d = curve.Derivative(0.5);
        d.Length.Should().BeGreaterThan(0.0);
    }

    /// <summary>Linear 2D Bezier along a line should stay on that line.</summary>
    [Fact]
    public void LinearBezier2D_ShouldStayOnLine()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(10, 5)));
        Point2D mid = curve.PointAt(0.5);
        double ratio = mid.Y / mid.X;
        ratio.Should().BeApproximately(0.5, Precision);
    }

    /// <summary>Sample first and last points should match control point endpoints.</summary>
    [Fact]
    public void Bezier2D_Sample_Endpoints_ShouldMatchControlPoints()
    {
        var curve = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(5, 10), new Point2D(10, 0)));
        ImmutableArray<Point2D> points = curve.Sample(11);
        points[0].X.Should().BeApproximately(0.0, Precision);
        points[^1].X.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>HermiteToBezier should produce a cubic curve (degree 3).</summary>
    [Fact]
    public void HermiteToBezier_ShouldProduceCubicCurve()
    {
        var curve = BezierCurve2D.HermiteToBezier(
            new Point2D(0, 0), new Vector2D(1, 0),
            new Point2D(1, 0), new Vector2D(1, 0));
        curve.Degree.Should().Be(3);
    }
}
