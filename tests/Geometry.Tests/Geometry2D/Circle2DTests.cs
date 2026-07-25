namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Circle2D struct.</summary>
public class Circle2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Circumference should equal 2*PI*r.</summary>
    [Fact]
    public void Circumference_ShouldBeTwoPiR()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        circle.Circumference.Should().BeApproximately(2.0 * System.Math.PI * 5.0, Precision);
    }

    /// <summary>Area should equal PI*r^2.</summary>
    [Fact]
    public void Area_ShouldBePiRSquared()
    {
        var circle = new Circle2D(new Point2D(0, 0), 3);
        circle.Area.Should().BeApproximately(System.Math.PI * 9.0, Precision);
    }

    /// <summary>PointAt(0) should return rightmost point.</summary>
    [Fact]
    public void PointAt_Zero_ShouldReturnRightmostPoint()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        Point2D p = circle.PointAt(0);
        p.X.Should().BeApproximately(5.0, Precision);
        p.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt(PI/2) should return topmost point.</summary>
    [Fact]
    public void PointAt_PiOver2_ShouldReturnTopmostPoint()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        Point2D p = circle.PointAt(System.Math.PI / 2.0);
        p.X.Should().BeApproximately(0.0, Precision);
        p.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Contains point inside should return true.</summary>
    [Fact]
    public void Contains_PointInside_ShouldReturnTrue()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        circle.Contains(new Point2D(1, 1)).Should().BeTrue();
    }

    /// <summary>Contains point outside should return false.</summary>
    [Fact]
    public void Contains_PointOutside_ShouldReturnFalse()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        circle.Contains(new Point2D(10, 10)).Should().BeFalse();
    }

    /// <summary>Contains point on boundary should return true.</summary>
    [Fact]
    public void Contains_PointOnBoundary_ShouldReturnTrue()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        circle.Contains(new Point2D(5, 0)).Should().BeTrue();
    }

    /// <summary>DistanceTo point outside should be positive.</summary>
    [Fact]
    public void DistanceTo_PointOutside_ShouldBePositive()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        circle.DistanceTo(new Point2D(10, 0)).Should().BeApproximately(5.0, Precision);
    }

    /// <summary>DistanceTo point inside should be negative.</summary>
    [Fact]
    public void DistanceTo_PointInside_ShouldBeNegative()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        circle.DistanceTo(new Point2D(3, 0)).Should().BeApproximately(-2.0, Precision);
    }

    /// <summary>Intersect with overlapping circle should give two points.</summary>
    [Fact]
    public void Intersect_OverlappingCircle_ShouldGiveTwoPoints()
    {
        var c1 = new Circle2D(new Point2D(0, 0), 5);
        var c2 = new Circle2D(new Point2D(5, 0), 5);
        var (hit, points) = c1.Intersect(c2);
        hit.Should().BeTrue();
        points.Length.Should().Be(2);
    }

    /// <summary>Intersect tangent circles should give one point.</summary>
    [Fact]
    public void Intersect_TangentCircles_ShouldGiveOnePoint()
    {
        var c1 = new Circle2D(new Point2D(0, 0), 5);
        var c2 = new Circle2D(new Point2D(10, 0), 5);
        var (hit, points) = c1.Intersect(c2);
        hit.Should().BeTrue();
        points.Length.Should().Be(1);
    }

    /// <summary>Intersect separate circles should not hit.</summary>
    [Fact]
    public void Intersect_SeparateCircles_ShouldNotHit()
    {
        var c1 = new Circle2D(new Point2D(0, 0), 2);
        var c2 = new Circle2D(new Point2D(100, 0), 2);
        var (hit, _) = c1.Intersect(c2);
        hit.Should().BeFalse();
    }

    /// <summary>Intersect one circle inside another should not hit.</summary>
    [Fact]
    public void Intersect_OneInsideOther_ShouldNotHit()
    {
        var c1 = new Circle2D(new Point2D(0, 0), 10);
        var c2 = new Circle2D(new Point2D(0, 0), 3);
        var (hit, _) = c1.Intersect(c2);
        hit.Should().BeFalse();
    }

    /// <summary>Intersect with line crossing circle should give two points.</summary>
    [Fact]
    public void Intersect_LineCrossing_ShouldGiveTwoPoints()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var line = new Line2D(new Point2D(-10, 0), new Point2D(10, 0));
        var (hit, points) = circle.Intersect(line);
        hit.Should().BeTrue();
        points.Length.Should().Be(2);
    }

    /// <summary>Intersect with tangent line should give one point.</summary>
    [Fact]
    public void Intersect_TangentLine_ShouldGiveOnePoint()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var line = new Line2D(new Point2D(-10, 5), new Point2D(10, 5));
        var (hit, points) = circle.Intersect(line);
        hit.Should().BeTrue();
        points.Length.Should().Be(1);
    }

    /// <summary>Intersect with line not touching should not hit.</summary>
    [Fact]
    public void Intersect_LineNotTouching_ShouldNotHit()
    {
        var circle = new Circle2D(new Point2D(0, 0), 2);
        var line = new Line2D(new Point2D(-10, 10), new Point2D(10, 10));
        var (hit, _) = circle.Intersect(line);
        hit.Should().BeFalse();
    }

    /// <summary>TangentAt(0) should be (0, 1) for unit circle.</summary>
    [Fact]
    public void TangentAt_Zero_ShouldBeVerticalForUnitCircle()
    {
        var circle = new Circle2D(new Point2D(0, 0), 1);
        Vector2D tangent = circle.TangentAt(0);
        tangent.X.Should().BeApproximately(0.0, Precision);
        tangent.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>NormalAt(0) should be (1, 0) for unit circle.</summary>
    [Fact]
    public void NormalAt_Zero_ShouldBeHorizontalForUnitCircle()
    {
        var circle = new Circle2D(new Point2D(0, 0), 1);
        Vector2D normal = circle.NormalAt(0);
        normal.X.Should().BeApproximately(1.0, Precision);
        normal.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>ToBoundingBox should enclose the circle.</summary>
    [Fact]
    public void ToBoundingBox_ShouldEncloseCircle()
    {
        var circle = new Circle2D(new Point2D(2, 3), 5);
        BoundingBox2D bbox = circle.ToBoundingBox();
        bbox.Min.X.Should().BeApproximately(-3.0, Precision);
        bbox.Min.Y.Should().BeApproximately(-2.0, Precision);
        bbox.Max.X.Should().BeApproximately(7.0, Precision);
        bbox.Max.Y.Should().BeApproximately(8.0, Precision);
    }

    /// <summary>PointAt should return point on circle boundary.</summary>
    [Fact]
    public void PointAt_ShouldReturnPointOnBoundary()
    {
        var circle = new Circle2D(new Point2D(1, 2), 3);
        Point2D p = circle.PointAt(System.Math.PI / 4);
        double dist = circle.Center.DistanceTo(p);
        dist.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>Contains center point should return true.</summary>
    [Fact]
    public void Contains_Center_ShouldReturnTrue()
    {
        var circle = new Circle2D(new Point2D(5, 5), 3);
        circle.Contains(new Point2D(5, 5)).Should().BeTrue();
    }

    /// <summary>DistanceTo center should equal negative radius.</summary>
    [Fact]
    public void DistanceTo_Center_ShouldBeNegativeRadius()
    {
        var circle = new Circle2D(new Point2D(0, 0), 7);
        circle.DistanceTo(new Point2D(0, 0)).Should().BeApproximately(-7.0, Precision);
    }

    /// <summary>Tangent and normal should be perpendicular.</summary>
    [Fact]
    public void TangentAndNormal_ShouldBePerpendicular()
    {
        var circle = new Circle2D(new Point2D(0, 0), 5);
        Vector2D tangent = circle.TangentAt(System.Math.PI / 3);
        Vector2D normal = circle.NormalAt(System.Math.PI / 3);
        tangent.Dot(normal).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Area with radius 1 should be PI.</summary>
    [Fact]
    public void Area_UnitCircle_ShouldBePi()
    {
        var circle = new Circle2D(new Point2D(0, 0), 1);
        circle.Area.Should().BeApproximately(System.Math.PI, Precision);
    }
}
