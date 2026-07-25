namespace MathVerse.Geometry.Tests.Curves;

/// <summary>Tests for BSplineCurve class.</summary>
public class BSplineCurveTests
{
    private const double Precision = 1e-8;

    private static BSplineCurve CreateSimpleBSpline()
    {
        var knots = ImmutableArray.Create(0.0, 0.0, 0.0, 1.0, 1.0, 1.0);
        var controlPoints = ImmutableArray.Create(
            new Point3D(0, 0, 0),
            new Point3D(1, 2, 0),
            new Point3D(3, 1, 0));
        return new BSplineCurve(knots, controlPoints, 2);
    }

    /// <summary>PointAt t=0 should return first control point for clamped B-spline.</summary>
    [Fact]
    public void PointAt_AtStart_ShouldReturnFirstControlPoint()
    {
        var bspline = CreateSimpleBSpline();
        Point3D result = bspline.PointAt(0);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt t=1 should return last control point for clamped B-spline.</summary>
    [Fact]
    public void PointAt_AtEnd_ShouldReturnLastControlPoint()
    {
        var bspline = CreateSimpleBSpline();
        Point3D result = bspline.PointAt(1);
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Sample should return the correct number of points.</summary>
    [Fact]
    public void Sample_ShouldReturnCorrectCount()
    {
        var bspline = CreateSimpleBSpline();
        IReadOnlyList<Point3D> points = bspline.Sample(10);
        points.Count.Should().Be(10);
    }

    /// <summary>Degree should match constructor parameter.</summary>
    [Fact]
    public void Degree_ShouldMatchConstructor()
    {
        var bspline = CreateSimpleBSpline();
        bspline.Degree.Should().Be(2);
    }

    /// <summary>KnotCount should equal knots array length.</summary>
    [Fact]
    public void KnotCount_ShouldMatchKnotsLength()
    {
        var bspline = CreateSimpleBSpline();
        bspline.Knots.Length.Should().Be(6);
    }

    /// <summary>ControlPointCount should match control points array length.</summary>
    [Fact]
    public void ControlPointCount_ShouldMatchControlPointsLength()
    {
        var bspline = CreateSimpleBSpline();
        bspline.ControlPoints.Length.Should().Be(3);
    }

    /// <summary>PointAt at midpoint should be a convex combination of control points.</summary>
    [Fact]
    public void PointAt_Midpoint_ShouldBeBetweenControlPoints()
    {
        var bspline = CreateSimpleBSpline();
        Point3D result = bspline.PointAt(0.5);
        result.X.Should().BeGreaterThanOrEqualTo(0.0);
        result.X.Should().BeLessThanOrEqualTo(3.0);
    }

    /// <summary>Sample first and last points should match endpoint evaluations.</summary>
    [Fact]
    public void Sample_Endpoints_ShouldMatchPointAt()
    {
        var bspline = CreateSimpleBSpline();
        IReadOnlyList<Point3D> points = bspline.Sample(5);
        Point3D first = bspline.PointAt(bspline.Knots[bspline.Degree]);
        points[0].X.Should().BeApproximately(first.X, Precision);
        points[0].Y.Should().BeApproximately(first.Y, Precision);
    }

    /// <summary>Linear B-spline should produce same results as linear Bezier.</summary>
    [Fact]
    public void LinearBSpline_ShouldInterpolateLinearly()
    {
        var knots = ImmutableArray.Create(0.0, 0.0, 1.0, 1.0);
        var controlPoints = ImmutableArray.Create(
            new Point3D(0, 0, 0),
            new Point3D(10, 10, 0));
        var bspline = new BSplineCurve(knots, controlPoints, 1);
        Point3D mid = bspline.PointAt(0.5);
        mid.X.Should().BeApproximately(5.0, Precision);
        mid.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Z coordinates should remain zero for planar control points.</summary>
    [Fact]
    public void PlanarControlPoints_ZShouldBeZero()
    {
        var bspline = CreateSimpleBSpline();
        Point3D result = bspline.PointAt(0.5);
        result.Z.Should().BeApproximately(0.0, Precision);
    }
}
