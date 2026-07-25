namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Ellipse2D struct.</summary>
public class Ellipse2DTests
{
    private const double Precision = 1e-10;

    /// <summary>PointAt should return point at semi-major distance along x-axis for zero rotation.</summary>
    [Fact]
    public void PointAt_ZeroAngle_ShouldReturnSemiMajorPoint()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        Point2D p = ellipse.PointAt(0);
        p.X.Should().BeApproximately(5.0, Precision);
        p.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt(PI/2) should return point at semi-minor distance for zero rotation.</summary>
    [Fact]
    public void PointAt_PiOver2_ShouldReturnSemiMinorPoint()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        Point2D p = ellipse.PointAt(System.Math.PI / 2.0);
        p.X.Should().BeApproximately(0.0, Precision);
        p.Y.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>TangentAt should return normalized vector.</summary>
    [Fact]
    public void TangentAt_ShouldReturnNormalizedVector()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        Vector2D t = ellipse.TangentAt(System.Math.PI / 4);
        t.Length.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Area should equal PI * semiMajor * semiMinor.</summary>
    [Fact]
    public void Area_ShouldBePiTimesSemiMajorTimesSemiMinor()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        ellipse.Area.Should().BeApproximately(System.Math.PI * 5.0 * 3.0, Precision);
    }

    /// <summary>Perimeter should return positive value.</summary>
    [Fact]
    public void Perimeter_ShouldReturnPositiveValue()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        ellipse.Perimeter().Should().BeGreaterThan(0);
    }

    /// <summary>Contains center should return true.</summary>
    [Fact]
    public void Contains_Center_ShouldReturnTrue()
    {
        var ellipse = new Ellipse2D(new Point2D(2, 3), 5, 3, 0);
        ellipse.Contains(new Point2D(2, 3)).Should().BeTrue();
    }

    /// <summary>Contains point well inside should return true.</summary>
    [Fact]
    public void Contains_PointInside_ShouldReturnTrue()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        ellipse.Contains(new Point2D(1, 1)).Should().BeTrue();
    }

    /// <summary>Contains point far outside should return false.</summary>
    [Fact]
    public void Contains_PointOutside_ShouldReturnFalse()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        ellipse.Contains(new Point2D(100, 100)).Should().BeFalse();
    }

    /// <summary>ToBoundingBox should enclose the ellipse.</summary>
    [Fact]
    public void ToBoundingBox_ShouldEncloseEllipse()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        BoundingBox2D bbox = ellipse.ToBoundingBox();
        bbox.Center.X.Should().BeApproximately(0.0, Precision);
        bbox.Center.Y.Should().BeApproximately(0.0, Precision);
        bbox.Width.Should().BeGreaterThanOrEqualTo(10.0 - 1e-6);
        bbox.Height.Should().BeGreaterThanOrEqualTo(6.0 - 1e-6);
    }

    /// <summary>Circular ellipse should have equal semi-major and semi-minor.</summary>
    [Fact]
    public void CircularEllipse_ShouldHaveEqualAxes()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 5, 0);
        ellipse.SemiMajor.Should().BeApproximately(ellipse.SemiMinor, Precision);
    }

    /// <summary>Circular ellipse area should equal circle area.</summary>
    [Fact]
    public void CircularEllipse_Area_ShouldEqualCircleArea()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 5, 0);
        double circleArea = System.Math.PI * 25.0;
        ellipse.Area.Should().BeApproximately(circleArea, Precision);
    }

    /// <summary>Rotated ellipse should compute point correctly.</summary>
    [Fact]
    public void RotatedEllipse_ShouldComputePointCorrectly()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, System.Math.PI / 2.0);
        Point2D p = ellipse.PointAt(0);
        p.X.Should().BeApproximately(0.0, Precision);
        p.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>PointAt should lie on the ellipse boundary.</summary>
    [Fact]
    public void PointAt_ShouldLieOnBoundary()
    {
        var ellipse = new Ellipse2D(new Point2D(1, 2), 4, 3, 0.3);
        Point2D p = ellipse.PointAt(System.Math.PI / 3);
        ellipse.Contains(p).Should().BeTrue();
    }

    /// <summary>TangentAt should be perpendicular to the radial direction approximately.</summary>
    [Fact]
    public void TangentAt_ShouldBePerpendicularToNormal()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        Point2D p = ellipse.PointAt(1.0);
        Vector2D normal = new Vector2D(System.Math.Cos(1.0) / 5.0, System.Math.Sin(1.0) / 3.0).Normalize();
        Vector2D tangent = ellipse.TangentAt(1.0);
        double dot = normal.Dot(tangent);
        dot.Should().BeApproximately(0.0, 1e-3);
    }

    /// <summary>Non-rotated ellipse at PI should be at negative semi-major.</summary>
    [Fact]
    public void PointAt_Pi_ShouldBeAtNegativeSemiMajor()
    {
        var ellipse = new Ellipse2D(new Point2D(0, 0), 5, 3, 0);
        Point2D p = ellipse.PointAt(System.Math.PI);
        p.X.Should().BeApproximately(-5.0, Precision);
        p.Y.Should().BeApproximately(0.0, Precision);
    }
}
