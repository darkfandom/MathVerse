namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Vector2D struct.</summary>
public class Vector2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Zero vector should be (0, 0).</summary>
    [Fact]
    public void Zero_ShouldHaveZeroComponents()
    {
        Vector2D.Zero.X.Should().Be(0);
        Vector2D.Zero.Y.Should().Be(0);
    }

    /// <summary>UnitX should be (1, 0).</summary>
    [Fact]
    public void UnitX_ShouldBeAlongXAxis()
    {
        Vector2D.UnitX.X.Should().BeApproximately(1.0, Precision);
        Vector2D.UnitX.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>UnitY should be (0, 1).</summary>
    [Fact]
    public void UnitY_ShouldBeAlongYAxis()
    {
        Vector2D.UnitY.X.Should().BeApproximately(0.0, Precision);
        Vector2D.UnitY.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Length should return correct magnitude.</summary>
    [Fact]
    public void Length_ShouldReturnCorrectMagnitude()
    {
        var v = new Vector2D(3, 4);
        v.Length.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>LengthSquared should return squared magnitude.</summary>
    [Fact]
    public void LengthSquared_ShouldReturnSquaredMagnitude()
    {
        var v = new Vector2D(3, 4);
        v.LengthSquared.Should().BeApproximately(25.0, Precision);
    }

    /// <summary>Normalize should produce a unit vector.</summary>
    [Fact]
    public void Normalize_ShouldProduceUnitVector()
    {
        var v = new Vector2D(3, 4);
        Vector2D n = v.Normalize();
        n.Length.Should().BeApproximately(1.0, Precision);
        n.X.Should().BeApproximately(0.6, Precision);
        n.Y.Should().BeApproximately(0.8, Precision);
    }

    /// <summary>Normalize of zero vector should return zero.</summary>
    [Fact]
    public void Normalize_ZeroVector_ShouldReturnZero()
    {
        Vector2D n = Vector2D.Zero.Normalize();
        n.X.Should().BeApproximately(0.0, Precision);
        n.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Dot product of perpendicular vectors should be zero.</summary>
    [Fact]
    public void Dot_PerpendicularVectors_ShouldBeZero()
    {
        double result = Vector2D.UnitX.Dot(Vector2D.UnitY);
        result.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Dot product of parallel vectors should equal product of lengths.</summary>
    [Fact]
    public void Dot_ParallelVectors_ShouldBeProductOfLengths()
    {
        var a = new Vector2D(3, 0);
        var b = new Vector2D(5, 0);
        a.Dot(b).Should().BeApproximately(15.0, Precision);
    }

    /// <summary>Cross product of unit vectors should be 1.</summary>
    [Fact]
    public void Cross_UnitXY_ShouldBeOne()
    {
        double result = Vector2D.UnitX.Cross(Vector2D.UnitY);
        result.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Cross product of parallel vectors should be zero.</summary>
    [Fact]
    public void Cross_ParallelVectors_ShouldBeZero()
    {
        var a = new Vector2D(2, 3);
        var b = new Vector2D(4, 6);
        a.Cross(b).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Add should sum two vectors.</summary>
    [Fact]
    public void Add_ShouldSumVectors()
    {
        var a = new Vector2D(1, 2);
        var b = new Vector2D(3, 4);
        Vector2D result = a.Add(b);
        result.X.Should().BeApproximately(4.0, Precision);
        result.Y.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Subtract should difference two vectors.</summary>
    [Fact]
    public void Subtract_ShouldDifferenceVectors()
    {
        var a = new Vector2D(5, 7);
        var b = new Vector2D(2, 3);
        Vector2D result = a.Subtract(b);
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Scale should multiply components by scalar.</summary>
    [Fact]
    public void Scale_ShouldMultiplyByScalar()
    {
        var v = new Vector2D(2, 3);
        Vector2D result = v.Scale(2.5);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(7.5, Precision);
    }

    /// <summary>Negate should flip both components.</summary>
    [Fact]
    public void Negate_ShouldFlipComponents()
    {
        var v = new Vector2D(3, -4);
        Vector2D result = v.Negate();
        result.X.Should().BeApproximately(-3.0, Precision);
        result.Y.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Perpendicular should rotate 90 degrees CCW.</summary>
    [Fact]
    public void Perpendicular_ShouldRotate90DegreesCCW()
    {
        var v = new Vector2D(1, 0);
        Vector2D result = v.Perpendicular();
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>AngleTo between same direction vectors should be zero.</summary>
    [Fact]
    public void AngleTo_SameDirection_ShouldBeZero()
    {
        double angle = Vector2D.UnitX.AngleTo(Vector2D.UnitX);
        angle.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Angle of UnitX should be zero.</summary>
    [Fact]
    public void Angle_UnitX_ShouldBeZero()
    {
        Vector2D.UnitX.Angle.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Angle of UnitY should be PI/2.</summary>
    [Fact]
    public void Angle_UnitY_ShouldBePiOver2()
    {
        Vector2D.UnitY.Angle.Should().BeApproximately(System.Math.PI / 2.0, Precision);
    }

    /// <summary>Operator+ should add vectors.</summary>
    [Fact]
    public void OperatorPlus_ShouldAddVectors()
    {
        var a = new Vector2D(1, 2);
        var b = new Vector2D(3, 4);
        Vector2D result = a + b;
        result.X.Should().BeApproximately(4.0, Precision);
        result.Y.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Operator- should subtract vectors.</summary>
    [Fact]
    public void OperatorMinus_ShouldSubtractVectors()
    {
        var a = new Vector2D(5, 7);
        var b = new Vector2D(2, 3);
        Vector2D result = a - b;
        result.X.Should().BeApproximately(3.0, Precision);
        result.Y.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Operator* with scalar should scale vector.</summary>
    [Fact]
    public void OperatorMultiply_RightScalar_ShouldScaleVector()
    {
        var v = new Vector2D(2, 3);
        Vector2D result = v * 3.0;
        result.X.Should().BeApproximately(6.0, Precision);
        result.Y.Should().BeApproximately(9.0, Precision);
    }

    /// <summary>Operator* with left scalar should scale vector.</summary>
    [Fact]
    public void OperatorMultiply_LeftScalar_ShouldScaleVector()
    {
        var v = new Vector2D(2, 3);
        Vector2D result = 3.0 * v;
        result.X.Should().BeApproximately(6.0, Precision);
        result.Y.Should().BeApproximately(9.0, Precision);
    }

    /// <summary>Unary minus should negate vector.</summary>
    [Fact]
    public void UnaryMinus_ShouldNegateVector()
    {
        var v = new Vector2D(3, -4);
        Vector2D result = -v;
        result.X.Should().BeApproximately(-3.0, Precision);
        result.Y.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Equal vectors should use value equality.</summary>
    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var a = new Vector2D(1, 2);
        var b = new Vector2D(1, 2);
        a.Should().Be(b);
    }

    /// <summary>Zero vector should have zero length.</summary>
    [Fact]
    public void ZeroVector_ShouldHaveZeroLength()
    {
        Vector2D.Zero.Length.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Parallel vectors should have zero cross product.</summary>
    [Fact]
    public void ParallelVectors_ShouldHaveZeroCross()
    {
        var a = new Vector2D(1, 2);
        var b = new Vector2D(2, 4);
        a.Cross(b).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Perpendicular vectors should have zero dot product.</summary>
    [Fact]
    public void PerpendicularVectors_ShouldHaveZeroDot()
    {
        var a = new Vector2D(1, 0);
        var b = new Vector2D(0, 1);
        a.Dot(b).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Large vectors should compute length correctly.</summary>
    [Fact]
    public void LargeVector_ShouldComputeLengthCorrectly()
    {
        var v = new Vector2D(1e8, 1e8);
        v.Length.Should().BeApproximately(1e8 * System.Math.Sqrt(2.0), 1e-2);
    }

    /// <summary>Normalize of unit vector should return same vector.</summary>
    [Fact]
    public void Normalize_UnitVector_ShouldReturnSameVector()
    {
        Vector2D n = Vector2D.UnitX.Normalize();
        n.X.Should().BeApproximately(1.0, Precision);
        n.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var v = new Vector2D(1, 2);
        v.ToString().Should().Be("(1, 2)");
    }
}
