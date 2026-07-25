namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Point2D struct.</summary>
public class Point2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Origin should be (0, 0).</summary>
    [Fact]
    public void Origin_ShouldBeZeroCoordinates()
    {
        Point2D.Origin.X.Should().Be(0);
        Point2D.Origin.Y.Should().Be(0);
    }

    /// <summary>DistanceTo should compute Euclidean distance.</summary>
    [Fact]
    public void DistanceTo_ShouldComputeEuclideanDistance()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(3, 4);
        a.DistanceTo(b).Should().BeApproximately(5.0, Precision);
    }

    /// <summary>DistanceSquaredTo should return squared distance.</summary>
    [Fact]
    public void DistanceSquaredTo_ShouldReturnSquaredDistance()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(4, 6);
        a.DistanceSquaredTo(b).Should().BeApproximately(25.0, Precision);
    }

    /// <summary>Lerp at t=0 should return the original point.</summary>
    [Fact]
    public void Lerp_AtZero_ShouldReturnStartPoint()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(5, 6);
        var result = a.Lerp(b, 0);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>Lerp at t=1 should return the target point.</summary>
    [Fact]
    public void Lerp_AtOne_ShouldReturnEndPoint()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(5, 6);
        var result = a.Lerp(b, 1);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Lerp at t=0.5 should return the midpoint.</summary>
    [Fact]
    public void Lerp_AtHalf_ShouldReturnMidpoint()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(10, 20);
        var result = a.Lerp(b, 0.5);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(10.0, Precision);
    }

    /// <summary>ToVector2D should create a vector from origin to point.</summary>
    [Fact]
    public void ToVector2D_ShouldCreateVectorFromOrigin()
    {
        var p = new Point2D(3, 4);
        Vector2D v = p.ToVector2D();
        v.X.Should().BeApproximately(3.0, Precision);
        v.Y.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Translate should offset the point by a vector.</summary>
    [Fact]
    public void Translate_ShouldOffsetPointByVector()
    {
        var p = new Point2D(1, 2);
        var v = new Vector2D(3, 4);
        Point2D result = p.Translate(v);
        result.X.Should().BeApproximately(4.0, Precision);
        result.Y.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Equal points should be equal.</summary>
    [Fact]
    public void Equals_SameCoordinates_ShouldBeEqual()
    {
        var a = new Point2D(1.5, 2.5);
        var b = new Point2D(1.5, 2.5);
        a.Should().Be(b);
    }

    /// <summary>Different points should not be equal.</summary>
    [Fact]
    public void Equals_DifferentCoordinates_ShouldNotBeEqual()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(3, 4);
        a.Should().NotBe(b);
    }

    /// <summary>GetHashCode should be same for equal points.</summary>
    [Fact]
    public void GetHashCode_SamePoints_ShouldReturnSameHash()
    {
        var a = new Point2D(7, 8);
        var b = new Point2D(7, 8);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var p = new Point2D(1, 2);
        p.ToString().Should().Be("(1, 2)");
    }

    /// <summary>NaN coordinates should propagate in DistanceTo.</summary>
    [Fact]
    public void DistanceTo_NaNCoordinates_ShouldReturnNaN()
    {
        var a = new Point2D(double.NaN, 0);
        var b = new Point2D(1, 1);
        double result = a.DistanceTo(b);
        double.IsNaN(result).Should().BeTrue();
    }

    /// <summary>Same point should have zero distance.</summary>
    [Fact]
    public void DistanceTo_SamePoint_ShouldBeZero()
    {
        var p = new Point2D(5, 5);
        p.DistanceTo(p).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Origin should have zero coordinates.</summary>
    [Fact]
    public void ZeroCoords_DistanceToOrigin_ShouldBeCorrect()
    {
        var p = new Point2D(0, 0);
        p.X.Should().Be(0);
        p.Y.Should().Be(0);
    }

    /// <summary>Large coordinates should work correctly.</summary>
    [Fact]
    public void LargeCoords_DistanceTo_ShouldBeCorrect()
    {
        var a = new Point2D(1e10, 1e10);
        var b = new Point2D(1e10 + 3, 1e10 + 4);
        a.DistanceTo(b).Should().BeApproximately(5.0, 1e-5);
    }

    /// <summary>operator== should return true for equal points.</summary>
    [Fact]
    public void OperatorEquals_SamePoints_ShouldBeTrue()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(1, 2);
        (a == b).Should().BeTrue();
    }

    /// <summary>operator== should return false for different points.</summary>
    [Fact]
    public void OperatorEquals_DifferentPoints_ShouldBeFalse()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(3, 4);
        (a == b).Should().BeFalse();
    }

    /// <summary>operator!= should return true for different points.</summary>
    [Fact]
    public void OperatorNotEquals_DifferentPoints_ShouldBeTrue()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(3, 4);
        (a != b).Should().BeTrue();
    }

    /// <summary>operator!= should return false for equal points.</summary>
    [Fact]
    public void OperatorNotEquals_SamePoints_ShouldBeFalse()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(1, 2);
        (a != b).Should().BeFalse();
    }

    /// <summary>Record struct deconstruction should work.</summary>
    [Fact]
    public void Deconstruction_ShouldReturnCorrectValues()
    {
        var p = new Point2D(3, 7);
        (double x, double y) = p;
        x.Should().BeApproximately(3.0, Precision);
        y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>Indexer should return correct coordinates.</summary>
    [Fact]
    public void Indexer_ValidIndices_ShouldReturnCoordinates()
    {
        var p = new Point2D(5, 10);
        p[0].Should().BeApproximately(5.0, Precision);
        p[1].Should().BeApproximately(10.0, Precision);
    }

    /// <summary>Indexer should throw for out of range index.</summary>
    [Fact]
    public void Indexer_InvalidIndex_ShouldThrow()
    {
        var p = new Point2D(1, 2);
        Action act = () => _ = p[2];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    /// <summary>Lerp with negative t should extrapolate.</summary>
    [Fact]
    public void Lerp_NegativeT_ShouldExtrapolate()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(10, 10);
        var result = a.Lerp(b, -0.5);
        result.X.Should().BeApproximately(-5.0, Precision);
        result.Y.Should().BeApproximately(-5.0, Precision);
    }

    /// <summary>DistanceSquaredTo for same point should be zero.</summary>
    [Fact]
    public void DistanceSquaredTo_SamePoint_ShouldBeZero()
    {
        var p = new Point2D(42, 42);
        p.DistanceSquaredTo(p).Should().BeApproximately(0.0, Precision);
    }
}
