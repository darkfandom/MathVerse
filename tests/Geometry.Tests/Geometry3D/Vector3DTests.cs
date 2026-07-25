namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Vector3D"/> struct.</summary>
public class Vector3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies the Zero vector has all zero components.</summary>
    [Fact]
    public void Zero_HasAllZeroComponents()
    {
        Vector3D.Zero.X.Should().Be(0.0);
        Vector3D.Zero.Y.Should().Be(0.0);
        Vector3D.Zero.Z.Should().Be(0.0);
    }

    /// <summary>Verifies UnitX is (1,0,0).</summary>
    [Fact]
    public void UnitX_IsCorrect()
    {
        Vector3D.UnitX.X.Should().BeApproximately(1.0, Tolerance);
        Vector3D.UnitX.Y.Should().BeApproximately(0.0, Tolerance);
        Vector3D.UnitX.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies UnitY is (0,1,0).</summary>
    [Fact]
    public void UnitY_IsCorrect()
    {
        Vector3D.UnitY.X.Should().BeApproximately(0.0, Tolerance);
        Vector3D.UnitY.Y.Should().BeApproximately(1.0, Tolerance);
        Vector3D.UnitY.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies UnitZ is (0,0,1).</summary>
    [Fact]
    public void UnitZ_IsCorrect()
    {
        Vector3D.UnitZ.X.Should().BeApproximately(0.0, Tolerance);
        Vector3D.UnitZ.Y.Should().BeApproximately(0.0, Tolerance);
        Vector3D.UnitZ.Z.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Length of a known vector.</summary>
    [Fact]
    public void Length_KnownVector_ReturnsCorrectValue()
    {
        var v = new Vector3D(3, 4, 0);

        v.Length.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies Length of UnitX is 1.</summary>
    [Fact]
    public void Length_UnitVector_ReturnsOne()
    {
        Vector3D.UnitX.Length.Should().BeApproximately(1.0, Tolerance);
        Vector3D.UnitY.Length.Should().BeApproximately(1.0, Tolerance);
        Vector3D.UnitZ.Length.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies LengthSquared is the square of Length.</summary>
    [Fact]
    public void LengthSquared_IsSquareOfLength()
    {
        var v = new Vector3D(2, 3, 4);

        v.LengthSquared.Should().BeApproximately(v.Length * v.Length, Tolerance);
    }

    /// <summary>Verifies Normalize returns a unit vector.</summary>
    [Fact]
    public void Normalize_ReturnsUnitVector()
    {
        var v = new Vector3D(3, 4, 5);

        var n = v.Normalize();

        n.Length.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Normalize preserves direction.</summary>
    [Fact]
    public void Normalize_PreservesDirection()
    {
        var v = new Vector3D(2, 0, 0);

        var n = v.Normalize();

        n.X.Should().BeApproximately(1.0, Tolerance);
        n.Y.Should().BeApproximately(0.0, Tolerance);
        n.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Normalize of zero vector returns zero.</summary>
    [Fact]
    public void Normalize_ZeroVector_ReturnsZero()
    {
        var n = Vector3D.Zero.Normalize();

        n.Should().Be(Vector3D.Zero);
    }

    /// <summary>Verifies Dot product of perpendicular vectors is zero.</summary>
    [Fact]
    public void Dot_PerpendicularVectors_ReturnsZero()
    {
        Vector3D.UnitX.Dot(Vector3D.UnitY).Should().BeApproximately(0.0, Tolerance);
        Vector3D.UnitY.Dot(Vector3D.UnitZ).Should().BeApproximately(0.0, Tolerance);
        Vector3D.UnitX.Dot(Vector3D.UnitZ).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Dot product of parallel vectors equals product of lengths.</summary>
    [Fact]
    public void Dot_ParallelVectors_ReturnsProductOfLengths()
    {
        var a = new Vector3D(2, 0, 0);
        var b = new Vector3D(5, 0, 0);

        a.Dot(b).Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies Cross product of unit axes.</summary>
    [Fact]
    public void Cross_UnitAxes_ReturnsCorrectResult()
    {
        Vector3D.UnitX.Cross(Vector3D.UnitY).Should().Be(Vector3D.UnitZ);
        Vector3D.UnitY.Cross(Vector3D.UnitZ).Should().Be(Vector3D.UnitX);
        Vector3D.UnitZ.Cross(Vector3D.UnitX).Should().Be(Vector3D.UnitY);
    }

    /// <summary>Verifies Cross product of parallel vectors is zero.</summary>
    [Fact]
    public void Cross_ParallelVectors_ReturnsZero()
    {
        var a = new Vector3D(1, 2, 3);
        var b = new Vector3D(2, 4, 6);

        var result = a.Cross(b);

        result.Length.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Add returns the vector sum.</summary>
    [Fact]
    public void Add_ReturnsSum()
    {
        var a = new Vector3D(1, 2, 3);
        var b = new Vector3D(4, 5, 6);

        var result = a.Add(b);

        result.X.Should().BeApproximately(5.0, Tolerance);
        result.Y.Should().BeApproximately(7.0, Tolerance);
        result.Z.Should().BeApproximately(9.0, Tolerance);
    }

    /// <summary>Verifies Subtract returns the difference.</summary>
    [Fact]
    public void Subtract_ReturnsDifference()
    {
        var a = new Vector3D(4, 5, 6);
        var b = new Vector3D(1, 2, 3);

        var result = a.Subtract(b);

        result.X.Should().BeApproximately(3.0, Tolerance);
        result.Y.Should().BeApproximately(3.0, Tolerance);
        result.Z.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies Scale multiplies all components.</summary>
    [Fact]
    public void Scale_MultipliesAllComponents()
    {
        var v = new Vector3D(1, 2, 3);

        var result = v.Scale(3.0);

        result.X.Should().BeApproximately(3.0, Tolerance);
        result.Y.Should().BeApproximately(6.0, Tolerance);
        result.Z.Should().BeApproximately(9.0, Tolerance);
    }

    /// <summary>Verifies Negate reverses all components.</summary>
    [Fact]
    public void Negate_ReversesAllComponents()
    {
        var v = new Vector3D(1, -2, 3);

        var result = v.Negate();

        result.X.Should().BeApproximately(-1.0, Tolerance);
        result.Y.Should().BeApproximately(2.0, Tolerance);
        result.Z.Should().BeApproximately(-3.0, Tolerance);
    }

    /// <summary>Verifies AngleTo between perpendicular vectors is PI/2.</summary>
    [Fact]
    public void AngleTo_PerpendicularVectors_ReturnsPiOver2()
    {
        double angle = Vector3D.UnitX.AngleTo(Vector3D.UnitY);

        angle.Should().BeApproximately(System.Math.PI / 2.0, Tolerance);
    }

    /// <summary>Verifies AngleTo between identical vectors is 0.</summary>
    [Fact]
    public void AngleTo_SameVector_ReturnsZero()
    {
        var v = new Vector3D(1, 2, 3);

        v.AngleTo(v).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies AngleTo between opposite vectors is PI.</summary>
    [Fact]
    public void AngleTo_OppositeVectors_ReturnsPi()
    {
        double angle = Vector3D.UnitX.AngleTo(-Vector3D.UnitX);

        angle.Should().BeApproximately(System.Math.PI, Tolerance);
    }

    /// <summary>Verifies operator + matches Add.</summary>
    [Fact]
    public void OperatorAdd_MatchesAdd()
    {
        var a = new Vector3D(1, 2, 3);
        var b = new Vector3D(4, 5, 6);

        (a + b).Should().Be(a.Add(b));
    }

    /// <summary>Verifies operator - matches Subtract.</summary>
    [Fact]
    public void OperatorSubtract_MatchesSubtract()
    {
        var a = new Vector3D(4, 5, 6);
        var b = new Vector3D(1, 2, 3);

        (a - b).Should().Be(a.Subtract(b));
    }

    /// <summary>Verifies scalar multiplication operators.</summary>
    [Fact]
    public void OperatorScalarMultiply_WorksBothSides()
    {
        var v = new Vector3D(1, 2, 3);

        (v * 2.0).Should().Be(v.Scale(2.0));
        (2.0 * v).Should().Be(v.Scale(2.0));
    }

    /// <summary>Verifies unary negation operator matches Negate.</summary>
    [Fact]
    public void OperatorNegate_MatchesNegate()
    {
        var v = new Vector3D(1, -2, 3);

        (-v).Should().Be(v.Negate());
    }

    /// <summary>Verifies two non-parallel vectors produce a perpendicular cross product.</summary>
    [Fact]
    public void Cross_ProductIsOrthogonalToBothInputs()
    {
        var a = new Vector3D(1, 2, 3);
        var b = new Vector3D(4, 5, 6);

        var cross = a.Cross(b);

        cross.Dot(a).Should().BeApproximately(0.0, Tolerance);
        cross.Dot(b).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies parallel vectors have AngleTo of 0.</summary>
    [Fact]
    public void AngleTo_ParallelVectors_ReturnsZero()
    {
        var a = new Vector3D(1, 0, 0);
        var b = new Vector3D(5, 0, 0);

        a.AngleTo(b).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies ToString produces the expected format.</summary>
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var v = new Vector3D(1.5, -2.5, 3.5);

        v.ToString().Should().Be("(1.5, -2.5, 3.5)");
    }
}
