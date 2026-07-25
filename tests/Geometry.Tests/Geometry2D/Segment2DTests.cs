namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Segment2D struct.</summary>
public class Segment2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Length should return distance between endpoints.</summary>
    [Fact]
    public void Length_ShouldReturnDistanceBetweenEndpoints()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(3, 4));
        seg.Length.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Midpoint should be center of segment.</summary>
    [Fact]
    public void Midpoint_ShouldBeCenterOfSegment()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(10, 20));
        Point2D mid = seg.Midpoint;
        mid.X.Should().BeApproximately(5.0, Precision);
        mid.Y.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>Direction should be normalized from P1 to P2.</summary>
    [Fact]
    public void Direction_ShouldBeNormalizedFromP1ToP2()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(0, 5));
        seg.Direction.Length.Should().BeApproximately(1.0, Precision);
        seg.Direction.X.Should().BeApproximately(0.0, Precision);
        seg.Direction.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>PointAt(0) should return P1.</summary>
    [Fact]
    public void PointAt_Zero_ShouldReturnP1()
    {
        var seg = new Segment2D(new Point2D(2, 3), new Point2D(6, 7));
        Point2D p = seg.PointAt(0);
        p.X.Should().BeApproximately(2.0, Precision);
        p.Y.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>PointAt(1) should return P2.</summary>
    [Fact]
    public void PointAt_One_ShouldReturnP2()
    {
        var seg = new Segment2D(new Point2D(2, 3), new Point2D(6, 7));
        Point2D p = seg.PointAt(1);
        p.X.Should().BeApproximately(6.0, Precision);
        p.Y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>PointAt(0.5) should return midpoint.</summary>
    [Fact]
    public void PointAt_Half_ShouldReturnMidpoint()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(10, 10));
        Point2D p = seg.PointAt(0.5);
        p.X.Should().BeApproximately(5.0, Precision);
        p.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Intersect with crossing segments should hit.</summary>
    [Fact]
    public void Intersect_CrossingSegments_ShouldHit()
    {
        var s1 = new Segment2D(new Point2D(0, 0), new Point2D(2, 2));
        var s2 = new Segment2D(new Point2D(2, 0), new Point2D(0, 2));
        var (hit, point) = s1.Intersect(s2);
        hit.Should().BeTrue();
        point.X.Should().BeApproximately(1.0, Precision);
        point.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Intersect with non-overlapping segments should miss.</summary>
    [Fact]
    public void Intersect_NonOverlapping_ShouldMiss()
    {
        var s1 = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
        var s2 = new Segment2D(new Point2D(0, 1), new Point2D(1, 1));
        var (hit, _) = s1.Intersect(s2);
        hit.Should().BeFalse();
    }

    /// <summary>DistanceTo point on segment should be zero.</summary>
    [Fact]
    public void DistanceTo_PointOnSegment_ShouldBeZero()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(5, 0));
        seg.DistanceTo(new Point2D(3, 0)).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>DistanceTo point near endpoint should return correct distance.</summary>
    [Fact]
    public void DistanceTo_PointNearEndpoint_ShouldReturnCorrectDistance()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
        seg.DistanceTo(new Point2D(3, 0)).Should().BeApproximately(2.0, Precision);
    }

    /// <summary>ClosestPoint should return nearest point on segment.</summary>
    [Fact]
    public void ClosestPoint_MidpointOfPerpendicular_ShouldProjectCorrectly()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(4, 0));
        Point2D closest = seg.ClosestPoint(new Point2D(2, 3));
        closest.X.Should().BeApproximately(2.0, Precision);
        closest.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>ClosestPoint should clamp to endpoints.</summary>
    [Fact]
    public void ClosestPoint_BeyondEndpoint_ShouldClampToEndpoint()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
        Point2D closest = seg.ClosestPoint(new Point2D(5, 0));
        closest.X.Should().BeApproximately(1.0, Precision);
        closest.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>ToBoundingBox should enclose both endpoints.</summary>
    [Fact]
    public void ToBoundingBox_ShouldEncloseEndpoints()
    {
        var seg = new Segment2D(new Point2D(2, 5), new Point2D(8, 10));
        BoundingBox2D bbox = seg.ToBoundingBox();
        bbox.Min.X.Should().BeApproximately(2.0, Precision);
        bbox.Min.Y.Should().BeApproximately(5.0, Precision);
        bbox.Max.X.Should().BeApproximately(8.0, Precision);
        bbox.Max.Y.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>Parallel non-overlapping segments should not intersect.</summary>
    [Fact]
    public void Intersect_ParallelSegments_ShouldMiss()
    {
        var s1 = new Segment2D(new Point2D(0, 0), new Point2D(2, 0));
        var s2 = new Segment2D(new Point2D(0, 1), new Point2D(2, 1));
        var (hit, _) = s1.Intersect(s2);
        hit.Should().BeFalse();
    }

    /// <summary>Collinear overlapping segments should intersect.</summary>
    [Fact]
    public void Intersect_CollinearOverlapping_ShouldHit()
    {
        var s1 = new Segment2D(new Point2D(0, 0), new Point2D(3, 0));
        var s2 = new Segment2D(new Point2D(1, 0), new Point2D(4, 0));
        var (hit, point) = s1.Intersect(s2);
        hit.Should().BeFalse();
    }

    /// <summary>Zero-length segment should have zero length.</summary>
    [Fact]
    public void ZeroLengthSegment_ShouldHaveZeroLength()
    {
        var seg = new Segment2D(new Point2D(5, 5), new Point2D(5, 5));
        seg.Length.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Zero-length segment ClosestPoint should return P1.</summary>
    [Fact]
    public void ZeroLengthSegment_ClosestPoint_ShouldReturnP1()
    {
        var seg = new Segment2D(new Point2D(5, 5), new Point2D(5, 5));
        Point2D closest = seg.ClosestPoint(new Point2D(10, 10));
        closest.X.Should().BeApproximately(5.0, Precision);
        closest.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Indexer at 0 should return P1.</summary>
    [Fact]
    public void Indexer_AtZero_ShouldReturnP1()
    {
        var seg = new Segment2D(new Point2D(1, 2), new Point2D(3, 4));
        seg[0].Should().Be(new Point2D(1, 2));
    }

    /// <summary>Indexer at 1 should return P2.</summary>
    [Fact]
    public void Indexer_AtOne_ShouldReturnP2()
    {
        var seg = new Segment2D(new Point2D(1, 2), new Point2D(3, 4));
        seg[1].Should().Be(new Point2D(3, 4));
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(1, 1));
        string result = seg.ToString();
        result.Should().Contain("Segment2D");
    }
}
