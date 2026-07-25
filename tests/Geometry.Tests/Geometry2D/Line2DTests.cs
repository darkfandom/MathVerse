namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Line2D struct.</summary>
public class Line2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Direction should be normalized.</summary>
    [Fact]
    public void Direction_ShouldBeNormalized()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(3, 4));
        line.Direction.Length.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Direction should point from P1 to P2.</summary>
    [Fact]
    public void Direction_ShouldPointFromP1ToP2()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(0, 5));
        line.Direction.X.Should().BeApproximately(0.0, Precision);
        line.Direction.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Length should return distance between defining points.</summary>
    [Fact]
    public void Length_ShouldReturnDistanceBetweenPoints()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(3, 4));
        line.Length.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>PointAt(0) should return P1.</summary>
    [Fact]
    public void PointAt_Zero_ShouldReturnP1()
    {
        var line = new Line2D(new Point2D(1, 2), new Point2D(5, 6));
        Point2D p = line.PointAt(0);
        p.X.Should().BeApproximately(1.0, Precision);
        p.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>PointAt(1) should return P2.</summary>
    [Fact]
    public void PointAt_One_ShouldReturnP2()
    {
        var line = new Line2D(new Point2D(1, 2), new Point2D(5, 6));
        Point2D p = line.PointAt(1);
        p.X.Should().BeApproximately(5.0, Precision);
        p.Y.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>DistanceTo for point on line should be zero.</summary>
    [Fact]
    public void DistanceTo_PointOnLine_ShouldBeZero()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(5, 0));
        line.DistanceTo(new Point2D(3, 0)).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>DistanceTo for point off line should be perpendicular distance.</summary>
    [Fact]
    public void DistanceTo_PointOffLine_ShouldBePerpendicularDistance()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(1, 0));
        line.DistanceTo(new Point2D(5, 3)).Should().BeApproximately(3.0, Precision);
    }

    /// <summary>ClosestPoint should return the projected point.</summary>
    [Fact]
    public void ClosestPoint_ShouldReturnProjectedPoint()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(1, 0));
        Point2D closest = line.ClosestPoint(new Point2D(5, 3));
        closest.X.Should().BeApproximately(5.0, Precision);
        closest.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Intersect with crossing line should return intersection point.</summary>
    [Fact]
    public void Intersect_CrossingLines_ShouldReturnIntersection()
    {
        var l1 = new Line2D(new Point2D(0, 0), new Point2D(2, 2));
        var l2 = new Line2D(new Point2D(2, 0), new Point2D(0, 2));
        var (hit, point) = l1.Intersect(l2);
        hit.Should().BeTrue();
        point.X.Should().BeApproximately(1.0, Precision);
        point.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Intersect with parallel lines should not hit.</summary>
    [Fact]
    public void Intersect_ParallelLines_ShouldNotHit()
    {
        var l1 = new Line2D(new Point2D(0, 0), new Point2D(1, 0));
        var l2 = new Line2D(new Point2D(0, 1), new Point2D(1, 1));
        var (hit, _) = l1.Intersect(l2);
        hit.Should().BeFalse();
    }

    /// <summary>Contains should return true for point on line.</summary>
    [Fact]
    public void Contains_PointOnLine_ShouldReturnTrue()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(2, 2));
        line.Contains(new Point2D(5, 5)).Should().BeTrue();
    }

    /// <summary>Contains should return false for point off line.</summary>
    [Fact]
    public void Contains_PointOffLine_ShouldReturnFalse()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(1, 0));
        line.Contains(new Point2D(1, 1)).Should().BeFalse();
    }

    /// <summary>ToBoundingBox should enclose both endpoints.</summary>
    [Fact]
    public void ToBoundingBox_ShouldEncloseEndpoints()
    {
        var line = new Line2D(new Point2D(1, 3), new Point2D(5, 7));
        BoundingBox2D bbox = line.ToBoundingBox();
        bbox.Min.X.Should().BeApproximately(1.0, Precision);
        bbox.Min.Y.Should().BeApproximately(3.0, Precision);
        bbox.Max.X.Should().BeApproximately(5.0, Precision);
        bbox.Max.Y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>Parallel lines should return no intersection.</summary>
    [Fact]
    public void Intersect_DiagonalParallel_ShouldNotHit()
    {
        var l1 = new Line2D(new Point2D(0, 0), new Point2D(1, 1));
        var l2 = new Line2D(new Point2D(0, 1), new Point2D(1, 2));
        var (hit, _) = l1.Intersect(l2);
        hit.Should().BeFalse();
    }

    /// <summary>Coincident lines should return no intersection.</summary>
    [Fact]
    public void Intersect_CoincidentLines_ShouldNotHit()
    {
        var l1 = new Line2D(new Point2D(0, 0), new Point2D(1, 1));
        var l2 = new Line2D(new Point2D(0, 0), new Point2D(2, 2));
        var (hit, _) = l1.Intersect(l2);
        hit.Should().BeFalse();
    }

    /// <summary>Perpendicular lines should intersect at origin of cross.</summary>
    [Fact]
    public void Intersect_PerpendicularLines_ShouldHitAtOrigin()
    {
        var l1 = new Line2D(new Point2D(-1, 0), new Point2D(1, 0));
        var l2 = new Line2D(new Point2D(0, -1), new Point2D(0, 1));
        var (hit, point) = l1.Intersect(l2);
        hit.Should().BeTrue();
        point.X.Should().BeApproximately(0.0, Precision);
        point.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt with t outside [0,1] should extrapolate.</summary>
    [Fact]
    public void PointAt_Extrapolation_ShouldExtendLine()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(1, 1));
        Point2D p = line.PointAt(2);
        p.X.Should().BeApproximately(2.0, Precision);
        p.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>Indexer at 0 should return P1.</summary>
    [Fact]
    public void Indexer_AtZero_ShouldReturnP1()
    {
        var line = new Line2D(new Point2D(1, 2), new Point2D(3, 4));
        line[0].Should().Be(new Point2D(1, 2));
    }

    /// <summary>Indexer at 1 should return P2.</summary>
    [Fact]
    public void Indexer_AtOne_ShouldReturnP2()
    {
        var line = new Line2D(new Point2D(1, 2), new Point2D(3, 4));
        line[1].Should().Be(new Point2D(3, 4));
    }

    /// <summary>Indexer at invalid index should throw.</summary>
    [Fact]
    public void Indexer_InvalidIndex_ShouldThrow()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(1, 1));
        Action act = () => _ = line[2];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(1, 1));
        string result = line.ToString();
        result.Should().Contain("Line2D");
    }
}
