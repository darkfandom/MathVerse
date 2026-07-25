namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for BoundingBox2D struct.</summary>
public class BoundingBox2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Width should equal max X minus min X.</summary>
    [Fact]
    public void Width_ShouldBeMaxXMinusMinX()
    {
        var bbox = new BoundingBox2D(new Point2D(1, 2), new Point2D(5, 8));
        bbox.Width.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Height should equal max Y minus min Y.</summary>
    [Fact]
    public void Height_ShouldBeMaxYMinusMinY()
    {
        var bbox = new BoundingBox2D(new Point2D(1, 2), new Point2D(5, 8));
        bbox.Height.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Center should be midpoint of the bounding box.</summary>
    [Fact]
    public void Center_ShouldBeMidpoint()
    {
        var bbox = new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 20));
        Point2D center = bbox.Center;
        center.X.Should().BeApproximately(5.0, Precision);
        center.Y.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>Area should be width times height.</summary>
    [Fact]
    public void Area_ShouldBeWidthTimesHeight()
    {
        var bbox = new BoundingBox2D(new Point2D(0, 0), new Point2D(4, 5));
        bbox.Area.Should().BeApproximately(20.0, Precision);
    }

    /// <summary>Contains point inside should return true.</summary>
    [Fact]
    public void Contains_PointInside_ShouldReturnTrue()
    {
        var bbox = new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 10));
        bbox.Contains(new Point2D(5, 5)).Should().BeTrue();
    }

    /// <summary>Contains point outside should return false.</summary>
    [Fact]
    public void Contains_PointOutside_ShouldReturnFalse()
    {
        var bbox = new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 10));
        bbox.Contains(new Point2D(15, 15)).Should().BeFalse();
    }

    /// <summary>Contains bbox inside should return true.</summary>
    [Fact]
    public void Contains_BBoxInside_ShouldReturnTrue()
    {
        var outer = new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 10));
        var inner = new BoundingBox2D(new Point2D(2, 2), new Point2D(8, 8));
        outer.Contains(inner).Should().BeTrue();
    }

    /// <summary>Contains bbox outside should return false.</summary>
    [Fact]
    public void Contains_BBoxOutside_ShouldReturnFalse()
    {
        var outer = new BoundingBox2D(new Point2D(0, 0), new Point2D(5, 5));
        var other = new BoundingBox2D(new Point2D(3, 3), new Point2D(8, 8));
        outer.Contains(other).Should().BeFalse();
    }

    /// <summary>Intersects with overlapping bbox should return true.</summary>
    [Fact]
    public void Intersects_Overlapping_ShouldReturnTrue()
    {
        var b1 = new BoundingBox2D(new Point2D(0, 0), new Point2D(5, 5));
        var b2 = new BoundingBox2D(new Point2D(3, 3), new Point2D(8, 8));
        b1.Intersects(b2).Should().BeTrue();
    }

    /// <summary>Intersects with non-overlapping bbox should return false.</summary>
    [Fact]
    public void Intersects_NonOverlapping_ShouldReturnFalse()
    {
        var b1 = new BoundingBox2D(new Point2D(0, 0), new Point2D(2, 2));
        var b2 = new BoundingBox2D(new Point2D(5, 5), new Point2D(8, 8));
        b1.Intersects(b2).Should().BeFalse();
    }

    /// <summary>Union should produce enclosing bounding box.</summary>
    [Fact]
    public void Union_ShouldProduceEnclosingBBox()
    {
        var b1 = new BoundingBox2D(new Point2D(0, 0), new Point2D(3, 3));
        var b2 = new BoundingBox2D(new Point2D(2, 2), new Point2D(5, 5));
        BoundingBox2D u = b1.Union(b2);
        u.Min.X.Should().BeApproximately(0.0, Precision);
        u.Min.Y.Should().BeApproximately(0.0, Precision);
        u.Max.X.Should().BeApproximately(5.0, Precision);
        u.Max.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Inflate should expand bbox equally on all sides.</summary>
    [Fact]
    public void Inflate_ShouldExpandEqually()
    {
        var bbox = new BoundingBox2D(new Point2D(2, 2), new Point2D(6, 6));
        BoundingBox2D inflated = bbox.Inflate(1);
        inflated.Min.X.Should().BeApproximately(1.0, Precision);
        inflated.Min.Y.Should().BeApproximately(1.0, Precision);
        inflated.Max.X.Should().BeApproximately(7.0, Precision);
        inflated.Max.Y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>FromPoints should create enclosing bbox.</summary>
    [Fact]
    public void FromPoints_ShouldCreateEnclosingBBox()
    {
        var points = new[] { new Point2D(1, 5), new Point2D(3, 2), new Point2D(7, 8), new Point2D(0, 0) };
        BoundingBox2D bbox = BoundingBox2D.FromPoints(points);
        bbox.Min.X.Should().BeApproximately(0.0, Precision);
        bbox.Min.Y.Should().BeApproximately(0.0, Precision);
        bbox.Max.X.Should().BeApproximately(7.0, Precision);
        bbox.Max.Y.Should().BeApproximately(8.0, Precision);
    }

    /// <summary>Zero bbox should have zero area.</summary>
    [Fact]
    public void ZeroBBox_ShouldHaveZeroArea()
    {
        var bbox = new BoundingBox2D(new Point2D(5, 5), new Point2D(5, 5));
        bbox.Area.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Square bbox should have equal width and height.</summary>
    [Fact]
    public void SquareBBox_ShouldHaveEqualWidthAndHeight()
    {
        var bbox = new BoundingBox2D(new Point2D(0, 0), new Point2D(7, 7));
        bbox.Width.Should().BeApproximately(bbox.Height, Precision);
    }

    /// <summary>Non-overlapping bboxes should not intersect.</summary>
    [Fact]
    public void NonOverlapping_ShouldNotIntersect()
    {
        var b1 = new BoundingBox2D(new Point2D(0, 0), new Point2D(1, 1));
        var b2 = new BoundingBox2D(new Point2D(10, 10), new Point2D(11, 11));
        b1.Intersects(b2).Should().BeFalse();
    }

    /// <summary>FromPoints with single point should create zero-area bbox.</summary>
    [Fact]
    public void FromPoints_SinglePoint_ShouldCreateZeroAreaBBox()
    {
        var points = new[] { new Point2D(3, 4) };
        BoundingBox2D bbox = BoundingBox2D.FromPoints(points);
        bbox.Area.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>FromPoints with empty collection should return origin bbox.</summary>
    [Fact]
    public void FromPoints_Empty_ShouldReturnOriginBBox()
    {
        var points = Array.Empty<Point2D>();
        BoundingBox2D bbox = BoundingBox2D.FromPoints(points);
        bbox.Min.Should().Be(Point2D.Origin);
        bbox.Max.Should().Be(Point2D.Origin);
    }

    /// <summary>Contains point on boundary should return true.</summary>
    [Fact]
    public void Contains_PointOnBoundary_ShouldReturnTrue()
    {
        var bbox = new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 10));
        bbox.Contains(new Point2D(0, 5)).Should().BeTrue();
        bbox.Contains(new Point2D(10, 5)).Should().BeTrue();
    }

    /// <summary>Union with self should return same bbox.</summary>
    [Fact]
    public void Union_WithSelf_ShouldReturnSame()
    {
        var bbox = new BoundingBox2D(new Point2D(1, 2), new Point2D(5, 6));
        BoundingBox2D u = bbox.Union(bbox);
        u.Min.Should().Be(bbox.Min);
        u.Max.Should().Be(bbox.Max);
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var bbox = new BoundingBox2D(new Point2D(0, 0), new Point2D(1, 1));
        string result = bbox.ToString();
        result.Should().Contain("BoundingBox2D");
    }
}
