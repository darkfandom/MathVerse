namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Ray2D struct.</summary>
public class Ray2DTests
{
    private const double Precision = 1e-10;

    /// <summary>PointAt(0) should return the origin.</summary>
    [Fact]
    public void PointAt_Zero_ShouldReturnOrigin()
    {
        var ray = new Ray2D(new Point2D(3, 4), Vector2D.UnitX);
        Point2D p = ray.PointAt(0);
        p.X.Should().BeApproximately(3.0, Precision);
        p.Y.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>PointAt with positive t should move along direction.</summary>
    [Fact]
    public void PointAt_PositiveT_ShouldMoveAlongDirection()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        Point2D p = ray.PointAt(5);
        p.X.Should().BeApproximately(5.0, Precision);
        p.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Intersect with perpendicular line should hit.</summary>
    [Fact]
    public void Intersect_Line_ShouldHit()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        var line = new Line2D(new Point2D(5, -1), new Point2D(5, 1));
        var (hit, t) = ray.Intersect(line);
        hit.Should().BeTrue();
        t.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Intersect with parallel line should not hit.</summary>
    [Fact]
    public void Intersect_ParallelLine_ShouldNotHit()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        var line = new Line2D(new Point2D(0, 1), new Point2D(1, 1));
        var (hit, _) = ray.Intersect(line);
        hit.Should().BeFalse();
    }

    /// <summary>Intersect with line behind ray should not hit.</summary>
    [Fact]
    public void Intersect_LineBehind_ShouldNotHit()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        var line = new Line2D(new Point2D(-5, -1), new Point2D(-5, 1));
        var (hit, _) = ray.Intersect(line);
        hit.Should().BeFalse();
    }

    /// <summary>Intersect with circle should give two points.</summary>
    [Fact]
    public void Intersect_Circle_ShouldGiveTwoPoints()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        var circle = new Circle2D(new Point2D(5, 0), 3);
        var (hit, points) = ray.Intersect(circle);
        hit.Should().BeTrue();
        points.Length.Should().Be(2);
    }

    /// <summary>Intersect with tangent circle should give one point.</summary>
    [Fact]
    public void Intersect_TangentCircle_ShouldGiveOnePoint()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        var circle = new Circle2D(new Point2D(5, 3), 3);
        var (hit, points) = ray.Intersect(circle);
        hit.Should().BeTrue();
        points.Length.Should().Be(1);
    }

    /// <summary>Intersect with distant circle should not hit.</summary>
    [Fact]
    public void Intersect_DistantCircle_ShouldNotHit()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        var circle = new Circle2D(new Point2D(100, 100), 2);
        var (hit, _) = ray.Intersect(circle);
        hit.Should().BeFalse();
    }

    /// <summary>DistanceTo point along ray direction should be correct.</summary>
    [Fact]
    public void DistanceTo_PointAlongRay_ShouldBeCorrect()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        double dist = ray.DistanceTo(new Point2D(5, 0));
        dist.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>DistanceTo point perpendicular to ray should be perpendicular distance.</summary>
    [Fact]
    public void DistanceTo_PointPerpendicular_ShouldBeCorrect()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        double dist = ray.DistanceTo(new Point2D(3, 4));
        dist.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>DistanceTo point behind ray should be distance to origin.</summary>
    [Fact]
    public void DistanceTo_PointBehind_ShouldBeDistToOrigin()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        double dist = ray.DistanceTo(new Point2D(-5, 0));
        dist.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Intersect circle behind ray should not hit.</summary>
    [Fact]
    public void Intersect_CircleBehind_ShouldNotHit()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        var circle = new Circle2D(new Point2D(-10, 0), 2);
        var (hit, _) = ray.Intersect(circle);
        hit.Should().BeFalse();
    }

    /// <summary>PointAt should be on the ray line.</summary>
    [Fact]
    public void PointAt_ShouldBeOnRayLine()
    {
        var ray = new Ray2D(new Point2D(1, 1), new Vector2D(1, 1).Normalize());
        Point2D p = ray.PointAt(3);
        double dist = new Line2D(ray.Origin, ray.Origin.Translate(ray.Direction)).DistanceTo(p);
        dist.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Intersect with line at origin should hit at t=0.</summary>
    [Fact]
    public void Intersect_LineAtOrigin_ShouldHitAtZeroT()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        var line = new Line2D(new Point2D(0, -1), new Point2D(0, 1));
        var (hit, t) = ray.Intersect(line);
        hit.Should().BeTrue();
        t.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var ray = new Ray2D(new Point2D(0, 0), Vector2D.UnitX);
        string result = ray.ToString();
        result.Should().Contain("Ray2D");
    }
}
