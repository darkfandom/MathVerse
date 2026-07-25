namespace MathVerse.Geometry.Advanced.Tests.Geometry4D;

public class Vector4DTests
{
    [Fact]
    public void Zero_IsAllZeros()
    {
        Vector4D.Zero.X.Should().Be(0);
        Vector4D.Zero.Y.Should().Be(0);
        Vector4D.Zero.Z.Should().Be(0);
        Vector4D.Zero.W.Should().Be(0);
    }

    [Fact]
    public void UnitX_IsCorrect()
    {
        Vector4D.UnitX.X.Should().Be(1);
        Vector4D.UnitX.Y.Should().Be(0);
        Vector4D.UnitX.Z.Should().Be(0);
        Vector4D.UnitX.W.Should().Be(0);
    }

    [Fact]
    public void UnitY_IsCorrect()
    {
        Vector4D.UnitY.X.Should().Be(0);
        Vector4D.UnitY.Y.Should().Be(1);
        Vector4D.UnitY.Z.Should().Be(0);
        Vector4D.UnitY.W.Should().Be(0);
    }

    [Fact]
    public void UnitZ_IsCorrect()
    {
        Vector4D.UnitZ.X.Should().Be(0);
        Vector4D.UnitZ.Y.Should().Be(0);
        Vector4D.UnitZ.Z.Should().Be(1);
        Vector4D.UnitZ.W.Should().Be(0);
    }

    [Fact]
    public void UnitW_IsCorrect()
    {
        Vector4D.UnitW.X.Should().Be(0);
        Vector4D.UnitW.Y.Should().Be(0);
        Vector4D.UnitW.Z.Should().Be(0);
        Vector4D.UnitW.W.Should().Be(1);
    }

    [Fact]
    public void Constructor_SetsAllComponents()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.X.Should().Be(1);
        v.Y.Should().Be(2);
        v.Z.Should().Be(3);
        v.W.Should().Be(4);
    }

    [Fact]
    public void Length_ZeroVector_IsZero()
    {
        Vector4D.Zero.Length.Should().Be(0);
    }

    [Fact]
    public void Length_UnitX_IsOne()
    {
        Vector4D.UnitX.Length.Should().Be(1);
    }

    [Fact]
    public void Length_UnitY_IsOne()
    {
        Vector4D.UnitY.Length.Should().Be(1);
    }

    [Fact]
    public void Length_UnitZ_IsOne()
    {
        Vector4D.UnitZ.Length.Should().Be(1);
    }

    [Fact]
    public void Length_UnitW_IsOne()
    {
        Vector4D.UnitW.Length.Should().Be(1);
    }

    [Fact]
    public void Length_KnownVector()
    {
        var v = new Vector4D(1, 2, 2, 0);
        v.Length.Should().Be(3);
    }

    [Fact]
    public void Length_Full4D()
    {
        var v = new Vector4D(1, 1, 1, 1);
        v.Length.Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void LengthSquared_ZeroVector_IsZero()
    {
        Vector4D.Zero.LengthSquared.Should().Be(0);
    }

    [Fact]
    public void LengthSquared_KnownVector()
    {
        var v = new Vector4D(1, 2, 2, 0);
        v.LengthSquared.Should().Be(9);
    }

    [Fact]
    public void LengthSquared_IsSquareOfLength()
    {
        var v = new Vector4D(3, 4, 5, 6);
        v.LengthSquared.Should().BeApproximately(v.Length * v.Length, 1e-10);
    }

    [Fact]
    public void LengthSquared_AllOnes()
    {
        var v = new Vector4D(1, 1, 1, 1);
        v.LengthSquared.Should().Be(4);
    }

    [Fact]
    public void Normalize_UnitVector_StaysUnit()
    {
        var v = Vector4D.UnitX.Normalize();
        v.Should().Be(Vector4D.UnitX);
    }

    [Fact]
    public void Normalize_KnownVector()
    {
        var v = new Vector4D(3, 0, 0, 0);
        var n = v.Normalize();
        n.X.Should().BeApproximately(1, 1e-10);
        n.Y.Should().BeApproximately(0, 1e-10);
        n.Z.Should().BeApproximately(0, 1e-10);
        n.W.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Normalize_ResultIsUnit()
    {
        var v = new Vector4D(1, 2, 3, 4);
        var n = v.Normalize();
        n.Length.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Normalize_NegativeVector()
    {
        var v = new Vector4D(-3, 0, 0, 0);
        var n = v.Normalize();
        n.X.Should().BeApproximately(-1, 1e-10);
    }

    [Fact]
    public void Normalize_ZeroVector_ReturnsZero()
    {
        var n = Vector4D.Zero.Normalize();
        n.Should().Be(Vector4D.Zero);
    }

    [Fact]
    public void Normalize_AllComponents()
    {
        var v = new Vector4D(1, 1, 1, 1);
        var n = v.Normalize();
        n.Length.Should().BeApproximately(1, 1e-10);
        n.X.Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void Dot_WithSelf_ReturnsLengthSquared()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.Dot(v).Should().BeApproximately(v.LengthSquared, 1e-10);
    }

    [Fact]
    public void Dot_PerpendicularVectors_ReturnsZero()
    {
        Vector4D.UnitX.Dot(Vector4D.UnitY).Should().Be(0);
    }

    [Fact]
    public void Dot_UnitX_UnitZ_ReturnsZero()
    {
        Vector4D.UnitX.Dot(Vector4D.UnitZ).Should().Be(0);
    }

    [Fact]
    public void Dot_KnownValue()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        a.Dot(b).Should().Be(70);
    }

    [Fact]
    public void Dot_SymmetricProperty()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        a.Dot(b).Should().Be(b.Dot(a));
    }

    [Fact]
    public void Dot_WithZero_ReturnsZero()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.Dot(Vector4D.Zero).Should().Be(0);
    }

    [Fact]
    public void Dot_NegativeComponents()
    {
        var a = new Vector4D(1, -2, 3, -4);
        var b = new Vector4D(-1, 2, -3, 4);
        a.Dot(b).Should().Be(-30);
    }

    [Fact]
    public void Add_TwoVectors()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        var result = a.Add(b);
        result.X.Should().Be(6);
        result.Y.Should().Be(8);
        result.Z.Should().Be(10);
        result.W.Should().Be(12);
    }

    [Fact]
    public void Add_WithZero_ReturnsOriginal()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.Add(Vector4D.Zero).Should().Be(v);
    }

    [Fact]
    public void Add_Commutative()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        a.Add(b).Should().Be(b.Add(a));
    }

    [Fact]
    public void Subtract_TwoVectors()
    {
        var a = new Vector4D(5, 6, 7, 8);
        var b = new Vector4D(1, 2, 3, 4);
        var result = a.Subtract(b);
        result.X.Should().Be(4);
        result.Y.Should().Be(4);
        result.Z.Should().Be(4);
        result.W.Should().Be(4);
    }

    [Fact]
    public void Subtract_SameVector_ReturnsZero()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.Subtract(v).Should().Be(Vector4D.Zero);
    }

    [Fact]
    public void Subtract_IsAntiCommutative()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        a.Subtract(b).Should().Be(b.Subtract(a).Negate());
    }

    [Fact]
    public void Scale_KnownValue()
    {
        var v = new Vector4D(1, 2, 3, 4);
        var result = v.Scale(2);
        result.X.Should().Be(2);
        result.Y.Should().Be(4);
        result.Z.Should().Be(6);
        result.W.Should().Be(8);
    }

    [Fact]
    public void Scale_ByZero_ReturnsZero()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.Scale(0).Should().Be(Vector4D.Zero);
    }

    [Fact]
    public void Scale_ByNegative()
    {
        var v = new Vector4D(1, 2, 3, 4);
        var result = v.Scale(-1);
        result.X.Should().Be(-1);
        result.Y.Should().Be(-2);
        result.Z.Should().Be(-3);
        result.W.Should().Be(-4);
    }

    [Fact]
    public void Scale_ByOne_ReturnsOriginal()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.Scale(1).Should().Be(v);
    }

    [Fact]
    public void Negate_KnownVector()
    {
        var v = new Vector4D(1, -2, 3, -4);
        var result = v.Negate();
        result.X.Should().Be(-1);
        result.Y.Should().Be(2);
        result.Z.Should().Be(-3);
        result.W.Should().Be(4);
    }

    [Fact]
    public void Negate_Twice_ReturnsOriginal()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.Negate().Negate().Should().Be(v);
    }

    [Fact]
    public void Negate_ZeroVector_ReturnsZero()
    {
        Vector4D.Zero.Negate().Should().Be(Vector4D.Zero);
    }

    [Fact]
    public void OperatorAdd()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        var result = a + b;
        result.X.Should().Be(6);
        result.Y.Should().Be(8);
        result.Z.Should().Be(10);
        result.W.Should().Be(12);
    }

    [Fact]
    public void OperatorSubtract()
    {
        var a = new Vector4D(10, 20, 30, 40);
        var b = new Vector4D(1, 2, 3, 4);
        var result = a - b;
        result.X.Should().Be(9);
        result.Y.Should().Be(18);
        result.Z.Should().Be(27);
        result.W.Should().Be(36);
    }

    [Fact]
    public void OperatorScalarMultiply_Right()
    {
        var v = new Vector4D(1, 2, 3, 4);
        var result = v * 3;
        result.X.Should().Be(3);
        result.Y.Should().Be(6);
        result.Z.Should().Be(9);
        result.W.Should().Be(12);
    }

    [Fact]
    public void OperatorScalarMultiply_Left()
    {
        var v = new Vector4D(1, 2, 3, 4);
        var result = 3 * v;
        result.X.Should().Be(3);
        result.Y.Should().Be(6);
        result.Z.Should().Be(9);
        result.W.Should().Be(12);
    }

    [Fact]
    public void OperatorNegate()
    {
        var v = new Vector4D(1, -2, 3, -4);
        var result = -v;
        result.X.Should().Be(-1);
        result.Y.Should().Be(2);
        result.Z.Should().Be(-3);
        result.W.Should().Be(4);
    }

    [Fact]
    public void OperatorAdd_Commutative()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        (a + b).Should().Be(b + a);
    }

    [Fact]
    public void ToVector3D_DividesByW()
    {
        var v = new Vector4D(4, 6, 8, 2);
        var v3 = v.ToVector3D();
        v3.X.Should().Be(2);
        v3.Y.Should().Be(3);
        v3.Z.Should().Be(4);
    }

    [Fact]
    public void ToVector3D_WIsOne()
    {
        var v = new Vector4D(3, 4, 5, 1);
        var v3 = v.ToVector3D();
        v3.X.Should().Be(3);
        v3.Y.Should().Be(4);
        v3.Z.Should().Be(5);
    }

    [Fact]
    public void ToVector3D_NegativeW()
    {
        var v = new Vector4D(6, -9, 12, -3);
        var v3 = v.ToVector3D();
        v3.X.Should().Be(-2);
        v3.Y.Should().Be(3);
        v3.Z.Should().Be(-4);
    }

    [Fact]
    public void Indexer_AllComponents()
    {
        var v = new Vector4D(10, 20, 30, 40);
        v[0].Should().Be(10);
        v[1].Should().Be(20);
        v[2].Should().Be(30);
        v[3].Should().Be(40);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var v = new Vector4D(1, 2, 3, 4);
        Action act = () => _ = v[4];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Indexer_NegativeIndex_Throws()
    {
        var v = new Vector4D(1, 2, 3, 4);
        Action act = () => _ = v[-1];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(1, 2, 3, 4);
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(1, 2, 3, 5);
        a.Should().NotBe(b);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(1, 2, 3, 4);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var v = new Vector4D(1, 2, 3, 4);
        v.ToString().Should().Be("(1, 2, 3, 4)");
    }

    [Fact]
    public void ToString_NegativeValues()
    {
        var v = new Vector4D(-1, -2, -3, -4);
        v.ToString().Should().Be("(-1, -2, -3, -4)");
    }

    [Fact]
    public void ToString_ZeroVector()
    {
        Vector4D.Zero.ToString().Should().Be("(0, 0, 0, 0)");
    }

    [Fact]
    public void ToString_FractionalValues()
    {
        var v = new Vector4D(1.5, 2.5, 3.5, 4.5);
        v.ToString().Should().Be("(1.5, 2.5, 3.5, 4.5)");
    }

    [Fact]
    public void Dot_DistributesOverAdd()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        var c = new Vector4D(9, 10, 11, 12);
        double left = a.Dot(b + c);
        double right = a.Dot(b) + a.Dot(c);
        left.Should().BeApproximately(right, 1e-10);
    }

    [Fact]
    public void Scale_DistributesOverAdd()
    {
        var a = new Vector4D(1, 2, 3, 4);
        var b = new Vector4D(5, 6, 7, 8);
        (a + b).Scale(2).Should().Be(a.Scale(2) + b.Scale(2));
    }
}
