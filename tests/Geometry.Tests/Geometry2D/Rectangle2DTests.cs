namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Rectangle2D struct.</summary>
public class Rectangle2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Width should equal max X minus min X.</summary>
    [Fact]
    public void Width_ShouldBeMaxXMinusMinX()
    {
        var rect = new Rectangle2D(new Point2D(1, 2), new Point2D(5, 8));
        rect.Width.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Height should equal max Y minus min Y.</summary>
    [Fact]
    public void Height_ShouldBeMaxYMinusMinY()
    {
        var rect = new Rectangle2D(new Point2D(1, 2), new Point2D(5, 8));
        rect.Height.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Center should be midpoint of the rectangle.</summary>
    [Fact]
    public void Center_ShouldBeMidpoint()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(10, 20));
        Point2D center = rect.Center;
        center.X.Should().BeApproximately(5.0, Precision);
        center.Y.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>Area should be width times height.</summary>
    [Fact]
    public void Area_ShouldBeWidthTimesHeight()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(4, 5));
        rect.Area.Should().BeApproximately(20.0, Precision);
    }

    /// <summary>Perimeter should be 2 times width plus height.</summary>
    [Fact]
    public void Perimeter_ShouldBeTwoTimesWidthPlusHeight()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(4, 5));
        rect.Perimeter.Should().BeApproximately(18.0, Precision);
    }

    /// <summary>Contains point inside should return true.</summary>
    [Fact]
    public void Contains_PointInside_ShouldReturnTrue()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(10, 10));
        rect.Contains(new Point2D(5, 5)).Should().BeTrue();
    }

    /// <summary>Contains point outside should return false.</summary>
    [Fact]
    public void Contains_PointOutside_ShouldReturnFalse()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(10, 10));
        rect.Contains(new Point2D(15, 15)).Should().BeFalse();
    }

    /// <summary>Contains rectangle inside should return true.</summary>
    [Fact]
    public void Contains_RectInside_ShouldReturnTrue()
    {
        var outer = new Rectangle2D(new Point2D(0, 0), new Point2D(10, 10));
        var inner = new Rectangle2D(new Point2D(2, 2), new Point2D(8, 8));
        outer.Contains(inner).Should().BeTrue();
    }

    /// <summary>Contains rectangle outside should return false.</summary>
    [Fact]
    public void Contains_RectOutside_ShouldReturnFalse()
    {
        var outer = new Rectangle2D(new Point2D(0, 0), new Point2D(5, 5));
        var other = new Rectangle2D(new Point2D(3, 3), new Point2D(8, 8));
        outer.Contains(other).Should().BeFalse();
    }

    /// <summary>Intersects with overlapping rectangle should return true.</summary>
    [Fact]
    public void Intersects_Overlapping_ShouldReturnTrue()
    {
        var r1 = new Rectangle2D(new Point2D(0, 0), new Point2D(5, 5));
        var r2 = new Rectangle2D(new Point2D(3, 3), new Point2D(8, 8));
        r1.Intersects(r2).Should().BeTrue();
    }

    /// <summary>Intersects with non-overlapping rectangle should return false.</summary>
    [Fact]
    public void Intersects_NonOverlapping_ShouldReturnFalse()
    {
        var r1 = new Rectangle2D(new Point2D(0, 0), new Point2D(2, 2));
        var r2 = new Rectangle2D(new Point2D(5, 5), new Point2D(8, 8));
        r1.Intersects(r2).Should().BeFalse();
    }

    /// <summary>Intersection of overlapping rectangles should return overlap rect.</summary>
    [Fact]
    public void Intersection_Overlapping_ShouldReturnOverlapRect()
    {
        var r1 = new Rectangle2D(new Point2D(0, 0), new Point2D(5, 5));
        var r2 = new Rectangle2D(new Point2D(3, 3), new Point2D(8, 8));
        Rectangle2D? result = r1.Intersect(r2);
        result.Should().NotBeNull();
        result!.Value.Min.X.Should().BeApproximately(3.0, Precision);
        result.Value.Min.Y.Should().BeApproximately(3.0, Precision);
        result.Value.Max.X.Should().BeApproximately(5.0, Precision);
        result.Value.Max.Y.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Intersection of non-overlapping rectangles should be null.</summary>
    [Fact]
    public void Intersection_NonOverlapping_ShouldBeNull()
    {
        var r1 = new Rectangle2D(new Point2D(0, 0), new Point2D(2, 2));
        var r2 = new Rectangle2D(new Point2D(5, 5), new Point2D(8, 8));
        r1.Intersect(r2).Should().BeNull();
    }

    /// <summary>Inflate should expand rectangle equally on all sides.</summary>
    [Fact]
    public void Inflate_ShouldExpandEqually()
    {
        var rect = new Rectangle2D(new Point2D(2, 2), new Point2D(6, 6));
        Rectangle2D inflated = rect.Inflate(1);
        inflated.Min.X.Should().BeApproximately(1.0, Precision);
        inflated.Min.Y.Should().BeApproximately(1.0, Precision);
        inflated.Max.X.Should().BeApproximately(7.0, Precision);
        inflated.Max.Y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>Translate should offset the rectangle.</summary>
    [Fact]
    public void Translate_ShouldOffsetRectangle()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(4, 4));
        Rectangle2D translated = rect.Translate(new Vector2D(3, 5));
        translated.Min.X.Should().BeApproximately(3.0, Precision);
        translated.Min.Y.Should().BeApproximately(5.0, Precision);
        translated.Max.X.Should().BeApproximately(7.0, Precision);
        translated.Max.Y.Should().BeApproximately(9.0, Precision);
    }

    /// <summary>Points should return four corners.</summary>
    [Fact]
    public void Points_ShouldReturnFourCorners()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(4, 3));
        var corners = rect.Points;
        corners.Length.Should().Be(4);
        corners[0].Should().Be(new Point2D(0, 0));
        corners[1].Should().Be(new Point2D(4, 0));
        corners[2].Should().Be(new Point2D(4, 3));
        corners[3].Should().Be(new Point2D(0, 3));
    }

    /// <summary>Zero rect should have zero area.</summary>
    [Fact]
    public void ZeroRect_ShouldHaveZeroArea()
    {
        var rect = new Rectangle2D(new Point2D(5, 5), new Point2D(5, 5));
        rect.Area.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Square should have equal width and height.</summary>
    [Fact]
    public void Square_ShouldHaveEqualWidthAndHeight()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(7, 7));
        rect.Width.Should().BeApproximately(rect.Height, Precision);
    }

    /// <summary>Large rect should compute area correctly.</summary>
    [Fact]
    public void LargeRect_ShouldComputeAreaCorrectly()
    {
        var rect = new Rectangle2D(new Point2D(-1e8, -1e8), new Point2D(1e8, 1e8));
        rect.Area.Should().BeApproximately(4e16, 1e2);
    }

    /// <summary>Contains point on boundary should return true.</summary>
    [Fact]
    public void Contains_PointOnBoundary_ShouldReturnTrue()
    {
        var rect = new Rectangle2D(new Point2D(0, 0), new Point2D(10, 10));
        rect.Contains(new Point2D(0, 5)).Should().BeTrue();
        rect.Contains(new Point2D(10, 5)).Should().BeTrue();
    }
}
