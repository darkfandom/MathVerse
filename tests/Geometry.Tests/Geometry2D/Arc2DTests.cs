namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Arc2D struct.</summary>
public class Arc2DTests
{
    private const double Precision = 1e-10;

    /// <summary>PointAt(0) should return start point.</summary>
    [Fact]
    public void PointAt_Zero_ShouldReturnStartPoint()
    {
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, System.Math.PI);
        Point2D p = arc.PointAt(0);
        p.X.Should().BeApproximately(5.0, Precision);
        p.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt(1) should return end point.</summary>
    [Fact]
    public void PointAt_One_ShouldReturnEndPoint()
    {
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, System.Math.PI);
        Point2D p = arc.PointAt(1);
        p.X.Should().BeApproximately(-5.0, Precision);
        p.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Quarter circle arc length should be PI*r/2.</summary>
    [Fact]
    public void Length_QuarterCircle_ShouldBePiRTimesHalf()
    {
        var arc = new Arc2D(new Point2D(0, 0), 4, 0, System.Math.PI / 2.0);
        arc.Length.Should().BeApproximately(System.Math.PI * 2.0, Precision);
    }

    /// <summary>Half circle arc length should be PI*r.</summary>
    [Fact]
    public void Length_HalfCircle_ShouldBePiR()
    {
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, System.Math.PI);
        arc.Length.Should().BeApproximately(System.Math.PI * 5.0, Precision);
    }

    /// <summary>Full circle arc length should be 2*PI*r.</summary>
    [Fact]
    public void Length_FullCircle_ShouldBeTwoPiR()
    {
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, 2.0 * System.Math.PI);
        arc.Length.Should().BeApproximately(2.0 * System.Math.PI * 5.0, Precision);
    }

    /// <summary>ToBoundingBox should enclose the arc.</summary>
    [Fact]
    public void ToBoundingBox_ShouldEncloseArc()
    {
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, System.Math.PI / 2.0);
        BoundingBox2D bbox = arc.ToBoundingBox();
        bbox.Min.X.Should().BeLessThanOrEqualTo(0.0 + 1e-6);
        bbox.Min.Y.Should().BeGreaterThanOrEqualTo(-1e-6);
        bbox.Max.X.Should().BeGreaterThanOrEqualTo(5.0 - 1e-6);
        bbox.Max.Y.Should().BeGreaterThanOrEqualTo(5.0 - 1e-6);
    }

    /// <summary>Start point should lie on the arc circle.</summary>
    [Fact]
    public void StartPoint_ShouldLieOnCircle()
    {
        var arc = new Arc2D(new Point2D(1, 2), 3, 0.5, 2.5);
        Point2D start = arc.PointAt(0);
        double dist = arc.Center.DistanceTo(start);
        dist.Should().BeApproximately(arc.Radius, Precision);
    }

    /// <summary>End point should lie on the arc circle.</summary>
    [Fact]
    public void EndPoint_ShouldLieOnCircle()
    {
        var arc = new Arc2D(new Point2D(1, 2), 3, 0.5, 2.5);
        Point2D end = arc.PointAt(1);
        double dist = arc.Center.DistanceTo(end);
        dist.Should().BeApproximately(arc.Radius, Precision);
    }

    /// <summary>Quarter arc from 0 to PI/2 should have bounding box from (0,0) to (r,r).</summary>
    [Fact]
    public void ToBoundingBox_QuarterArc_ShouldHaveCorrectBounds()
    {
        var arc = new Arc2D(new Point2D(0, 0), 4, 0, System.Math.PI / 2.0);
        BoundingBox2D bbox = arc.ToBoundingBox();
        bbox.Min.X.Should().BeApproximately(0.0, Precision);
        bbox.Min.Y.Should().BeApproximately(0.0, Precision);
        bbox.Max.X.Should().BeApproximately(4.0, Precision);
        bbox.Max.Y.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>PointAt with reversed angles should still produce points on circle.</summary>
    [Fact]
    public void PointAt_ReversedAngles_ShouldStillBeOnCircle()
    {
        var arc = new Arc2D(new Point2D(0, 0), 6, System.Math.PI, 0);
        Point2D mid = arc.PointAt(0.5);
        double dist = arc.Center.DistanceTo(mid);
        dist.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, System.Math.PI);
        string result = arc.ToString();
        result.Should().Contain("Arc2D");
    }
}
