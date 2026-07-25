namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Point3D"/> struct.</summary>
public class Point3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that the Origin constant has all zero coordinates.</summary>
    [Fact]
    public void Origin_ShouldHaveZeroCoordinates()
    {
        Point3D.Origin.X.Should().Be(0.0);
        Point3D.Origin.Y.Should().Be(0.0);
        Point3D.Origin.Z.Should().Be(0.0);
    }

    /// <summary>Verifies the Euclidean distance between two known points.</summary>
    [Fact]
    public void DistanceTo_KnownPoints_ReturnsCorrectDistance()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(4, 6, 3);

        double dist = a.DistanceTo(b);

        dist.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies distance from a point to itself is zero.</summary>
    [Fact]
    public void DistanceTo_SamePoint_ReturnsZero()
    {
        var p = new Point3D(3, 7, -2);

        p.DistanceTo(p).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies the squared distance matches the expected value.</summary>
    [Fact]
    public void DistanceSquaredTo_KnownPoints_ReturnsCorrectValue()
    {
        var a = new Point3D(1, 0, 0);
        var b = new Point3D(4, 0, 0);

        a.DistanceSquaredTo(b).Should().BeApproximately(9.0, Tolerance);
    }

    /// <summary>Verifies squared distance is the square of the Euclidean distance.</summary>
    [Fact]
    public void DistanceSquaredTo_EqualsDistanceSquared()
    {
        var a = new Point3D(2, 3, 5);
        var b = new Point3D(7, 11, 13);

        double d = a.DistanceTo(b);
        a.DistanceSquaredTo(b).Should().BeApproximately(d * d, Tolerance);
    }

    /// <summary>Verifies Lerp at t=0 returns the start point.</summary>
    [Fact]
    public void Lerp_AtZero_ReturnsStartPoint()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(4, 6, 8);

        var result = a.Lerp(b, 0.0);

        result.Should().Be(a);
    }

    /// <summary>Verifies Lerp at t=1 returns the end point.</summary>
    [Fact]
    public void Lerp_AtOne_ReturnsEndPoint()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(4, 6, 8);

        var result = a.Lerp(b, 1.0);

        result.Should().Be(b);
    }

    /// <summary>Verifies Lerp at t=0.5 returns the midpoint.</summary>
    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint()
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(10, 20, 30);

        var result = a.Lerp(b, 0.5);

        result.X.Should().BeApproximately(5.0, Tolerance);
        result.Y.Should().BeApproximately(10.0, Tolerance);
        result.Z.Should().BeApproximately(15.0, Tolerance);
    }

    /// <summary>Verifies Lerp at t=0.5 returns the midpoint of two arbitrary points.</summary>
    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint_ArbitraryPoints()
    {
        var a = new Point3D(3, 7, 1);
        var b = new Point3D(9, 5, 11);

        var result = a.Lerp(b, 0.5);

        result.X.Should().BeApproximately(6.0, Tolerance);
        result.Y.Should().BeApproximately(6.0, Tolerance);
        result.Z.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies ToVector3D converts correctly.</summary>
    [Fact]
    public void ToVector3D_ReturnsCorrectVector()
    {
        var p = new Point3D(3, -4, 5);

        var v = p.ToVector3D();

        v.X.Should().BeApproximately(3.0, Tolerance);
        v.Y.Should().BeApproximately(-4.0, Tolerance);
        v.Z.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies Translate moves the point by the given vector.</summary>
    [Fact]
    public void Translate_MovesPoint()
    {
        var p = new Point3D(1, 2, 3);
        var v = new Vector3D(4, 5, 6);

        var result = p.Translate(v);

        result.X.Should().BeApproximately(5.0, Tolerance);
        result.Y.Should().BeApproximately(7.0, Tolerance);
        result.Z.Should().BeApproximately(9.0, Tolerance);
    }

    /// <summary>Verifies Translate by zero vector returns the same point.</summary>
    [Fact]
    public void Translate_ZeroVector_ReturnsSamePoint()
    {
        var p = new Point3D(7, 8, 9);

        var result = p.Translate(Vector3D.Zero);

        result.Should().Be(p);
    }

    /// <summary>Verifies two points with same coordinates are equal.</summary>
    [Fact]
    public void Equals_SameCoordinates_ReturnsTrue()
    {
        var a = new Point3D(1.0, 2.0, 3.0);
        var b = new Point3D(1.0, 2.0, 3.0);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    /// <summary>Verifies two points with different coordinates are not equal.</summary>
    [Fact]
    public void Equals_DifferentCoordinates_ReturnsFalse()
    {
        var a = new Point3D(1.0, 2.0, 3.0);
        var b = new Point3D(1.0, 2.0, 3.001);

        a.Equals(b).Should().BeFalse();
    }

    /// <summary>Verifies ToString produces the expected format.</summary>
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var p = new Point3D(1.5, -2.5, 3.5);

        p.ToString().Should().Be("(1.5, -2.5, 3.5)");
    }

    /// <summary>Verifies indexer returns correct components.</summary>
    [Fact]
    public void Indexer_ReturnsCorrectComponents()
    {
        var p = new Point3D(10, 20, 30);

        p[0].Should().BeApproximately(10.0, Tolerance);
        p[1].Should().BeApproximately(20.0, Tolerance);
        p[2].Should().BeApproximately(30.0, Tolerance);
    }

    /// <summary>Verifies indexer throws for out of range index.</summary>
    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var p = new Point3D(1, 2, 3);

        var act = () => p[3];

        act.Should().Throw<IndexOutOfRangeException>();
    }

    /// <summary>Verifies DistanceTo with NaN coordinates returns NaN.</summary>
    [Fact]
    public void DistanceTo_NaN_ReturnsNaN()
    {
        var a = new Point3D(double.NaN, 0, 0);
        var b = new Point3D(1, 0, 0);

        double result = a.DistanceTo(b);

        double.IsNaN(result).Should().BeTrue();
    }

    /// <summary>Verifies operations with Infinite coordinates.</summary>
    [Fact]
    public void DistanceTo_Infinite_ReturnsInfinity()
    {
        var a = new Point3D(double.PositiveInfinity, 0, 0);
        var b = new Point3D(1, 0, 0);

        double result = a.DistanceTo(b);

        double.IsPositiveInfinity(result).Should().BeTrue();
    }

    /// <summary>Verifies a point with very small coordinates behaves correctly.</summary>
    [Fact]
    public void ZeroCoords_VerySmallCoordinates_ComputeCorrectly()
    {
        var a = new Point3D(1e-15, 1e-15, 1e-15);
        var b = new Point3D(2e-15, 2e-15, 2e-15);

        double dist = a.DistanceTo(b);

        dist.Should().BeApproximately(System.Math.Sqrt(3e-30), Tolerance);
    }

    /// <summary>Verifies distance with large coordinates.</summary>
    [Fact]
    public void LargeCoords_DistanceTo_ComputesCorrectly()
    {
        var a = new Point3D(1e10, 2e10, 3e10);
        var b = new Point3D(4e10, 6e10, 3e10);

        double dist = a.DistanceTo(b);

        dist.Should().BeApproximately(5e10, 1.0);
    }

    /// <summary>Verifies distance symmetry: a to b equals b to a.</summary>
    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(4, 5, 6);

        a.DistanceTo(b).Should().BeApproximately(b.DistanceTo(a), Tolerance);
    }

    /// <summary>Verifies Lerp symmetry: t on AB equals (1-t) on BA.</summary>
    [Fact]
    public void Lerp_IsSymmetric()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(7, 8, 9);

        var ab = a.Lerp(b, 0.3);
        var ba = b.Lerp(a, 0.7);

        ab.X.Should().BeApproximately(ba.X, Tolerance);
        ab.Y.Should().BeApproximately(ba.Y, Tolerance);
        ab.Z.Should().BeApproximately(ba.Z, Tolerance);
    }

    /// <summary>Verifies the record struct equality semantics.</summary>
    [Fact]
    public void Equality_RecordStructSemantics()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(1, 2, 3);
        var c = new Point3D(4, 5, 6);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
        a.Equals(c).Should().BeFalse();
    }
}
