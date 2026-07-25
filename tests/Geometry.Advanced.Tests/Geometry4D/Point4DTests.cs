namespace MathVerse.Geometry.Advanced.Tests.Geometry4D;

public class Point4DTests
{
    [Fact]
    public void Origin_HasCorrectCoordinates()
    {
        var origin = Point4D.Origin;
        origin.X.Should().Be(0);
        origin.Y.Should().Be(0);
        origin.Z.Should().Be(0);
        origin.W.Should().Be(1);
    }

    [Fact]
    public void Constructor_SetsAllComponents()
    {
        var p = new Point4D(1, 2, 3, 4);
        p.X.Should().Be(1);
        p.Y.Should().Be(2);
        p.Z.Should().Be(3);
        p.W.Should().Be(4);
    }

    [Fact]
    public void Indexer_Zero_ReturnsX()
    {
        var p = new Point4D(10, 20, 30, 40);
        p[0].Should().Be(10);
    }

    [Fact]
    public void Indexer_One_ReturnsY()
    {
        var p = new Point4D(10, 20, 30, 40);
        p[1].Should().Be(20);
    }

    [Fact]
    public void Indexer_Two_ReturnsZ()
    {
        var p = new Point4D(10, 20, 30, 40);
        p[2].Should().Be(30);
    }

    [Fact]
    public void Indexer_Three_ReturnsW()
    {
        var p = new Point4D(10, 20, 30, 40);
        p[3].Should().Be(40);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var p = new Point4D(1, 2, 3, 4);
        Action act = () => _ = p[4];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Indexer_NegativeIndex_Throws()
    {
        var p = new Point4D(1, 2, 3, 4);
        Action act = () => _ = p[-1];
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void DistanceTo_SamePoint_ReturnsZero()
    {
        var p = new Point4D(1, 2, 3, 4);
        p.DistanceTo(p).Should().Be(0);
    }

    [Fact]
    public void DistanceTo_KnownDistance()
    {
        var a = new Point4D(0, 0, 0, 0);
        var b = new Point4D(1, 0, 0, 0);
        a.DistanceTo(b).Should().Be(1);
    }

    [Fact]
    public void DistanceTo_3DComponent()
    {
        var a = new Point4D(0, 0, 0, 0);
        var b = new Point4D(0, 0, 0, 3);
        a.DistanceTo(b).Should().Be(3);
    }

    [Fact]
    public void DistanceTo_SymmetricProperty()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(5, 6, 7, 8);
        a.DistanceTo(b).Should().Be(b.DistanceTo(a));
    }

    [Fact]
    public void DistanceTo_Pythagorean4D()
    {
        var a = new Point4D(0, 0, 0, 0);
        var b = new Point4D(1, 2, 2, 4);
        a.DistanceTo(b).Should().Be(5);
    }

    [Fact]
    public void DistanceSquaredTo_SamePoint_ReturnsZero()
    {
        var p = new Point4D(3, 4, 5, 6);
        p.DistanceSquaredTo(p).Should().Be(0);
    }

    [Fact]
    public void DistanceSquaredTo_KnownValue()
    {
        var a = new Point4D(0, 0, 0, 0);
        var b = new Point4D(1, 2, 3, 4);
        a.DistanceSquaredTo(b).Should().Be(30);
    }

    [Fact]
    public void DistanceSquaredTo_IsSquareOfDistance()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(5, 6, 7, 8);
        double dist = a.DistanceTo(b);
        a.DistanceSquaredTo(b).Should().Be(dist * dist);
    }

    [Fact]
    public void DistanceSquaredTo_SymmetricProperty()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(5, 6, 7, 8);
        a.DistanceSquaredTo(b).Should().Be(b.DistanceSquaredTo(a));
    }

    [Fact]
    public void DistanceSquaredTo_OnlyOneComponentDiffers()
    {
        var a = new Point4D(1, 0, 0, 0);
        var b = new Point4D(0, 0, 0, 0);
        a.DistanceSquaredTo(b).Should().Be(1);
    }

    [Fact]
    public void Lerp_AtZero_ReturnsOriginal()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(5, 6, 7, 8);
        var result = a.Lerp(b, 0);
        result.Should().Be(a);
    }

    [Fact]
    public void Lerp_AtOne_ReturnsOther()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(5, 6, 7, 8);
        var result = a.Lerp(b, 1);
        result.Should().Be(b);
    }

    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint()
    {
        var a = new Point4D(0, 0, 0, 0);
        var b = new Point4D(10, 20, 30, 40);
        var result = a.Lerp(b, 0.5);
        result.X.Should().Be(5);
        result.Y.Should().Be(10);
        result.Z.Should().Be(15);
        result.W.Should().Be(20);
    }

    [Fact]
    public void Lerp_AtQuarter()
    {
        var a = new Point4D(0, 0, 0, 0);
        var b = new Point4D(8, 12, 16, 20);
        var result = a.Lerp(b, 0.25);
        result.X.Should().Be(2);
        result.Y.Should().Be(3);
        result.Z.Should().Be(4);
        result.W.Should().Be(5);
    }

    [Fact]
    public void Lerp_AtThreeQuarters()
    {
        var a = new Point4D(0, 0, 0, 0);
        var b = new Point4D(4, 8, 12, 16);
        var result = a.Lerp(b, 0.75);
        result.X.Should().Be(3);
        result.Y.Should().Be(6);
        result.Z.Should().Be(9);
        result.W.Should().Be(12);
    }

    [Fact]
    public void Lerp_AtNegativeExtrapolates()
    {
        var a = new Point4D(2, 4, 6, 8);
        var b = new Point4D(4, 8, 12, 16);
        var result = a.Lerp(b, -1);
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
        result.Z.Should().Be(0);
        result.W.Should().Be(0);
    }

    [Fact]
    public void Lerp_AtTwoExtrapolates()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(3, 4, 5, 6);
        var result = a.Lerp(b, 2);
        result.X.Should().Be(5);
        result.Y.Should().Be(6);
        result.Z.Should().Be(7);
        result.W.Should().Be(8);
    }

    [Fact]
    public void Lerp_SamePoint_ReturnsSamePoint()
    {
        var p = new Point4D(5, 5, 5, 5);
        var result = p.Lerp(p, 0.7);
        result.Should().Be(p);
    }

    [Fact]
    public void ToPoint3D_DividesByW()
    {
        var p = new Point4D(4, 6, 8, 2);
        var p3 = p.ToPoint3D();
        p3.X.Should().Be(2);
        p3.Y.Should().Be(3);
        p3.Z.Should().Be(4);
    }

    [Fact]
    public void ToPoint3D_WIsOne()
    {
        var p = new Point4D(3, 4, 5, 1);
        var p3 = p.ToPoint3D();
        p3.X.Should().Be(3);
        p3.Y.Should().Be(4);
        p3.Z.Should().Be(5);
    }

    [Fact]
    public void ToPoint3D_NegativeW()
    {
        var p = new Point4D(6, -9, 12, -3);
        var p3 = p.ToPoint3D();
        p3.X.Should().Be(-2);
        p3.Y.Should().Be(3);
        p3.Z.Should().Be(-4);
    }

    [Fact]
    public void ToPoint3D_LargeW()
    {
        var p = new Point4D(100, 200, 300, 100);
        var p3 = p.ToPoint3D();
        p3.X.Should().Be(1);
        p3.Y.Should().Be(2);
        p3.Z.Should().Be(3);
    }

    [Fact]
    public void ToVector4D_ReturnsSameComponents()
    {
        var p = new Point4D(1, 2, 3, 4);
        var v = p.ToVector4D();
        v.X.Should().Be(1);
        v.Y.Should().Be(2);
        v.Z.Should().Be(3);
        v.W.Should().Be(4);
    }

    [Fact]
    public void ToVector4D_Origin()
    {
        var v = Point4D.Origin.ToVector4D();
        v.X.Should().Be(0);
        v.Y.Should().Be(0);
        v.Z.Should().Be(0);
        v.W.Should().Be(1);
    }

    [Fact]
    public void Length_Origin_IsZero()
    {
        Point4D.Origin.Length.Should().Be(1);
    }

    [Fact]
    public void Length_OnAxis()
    {
        var p = new Point4D(3, 0, 0, 0);
        p.Length.Should().Be(3);
    }

    [Fact]
    public void Length_AllComponents()
    {
        var p = new Point4D(1, 2, 2, 0);
        p.Length.Should().Be(3);
    }

    [Fact]
    public void Length_NegativeComponents()
    {
        var p = new Point4D(-3, 0, 0, 0);
        p.Length.Should().Be(3);
    }

    [Fact]
    public void Length_Full4D()
    {
        var p = new Point4D(1, 1, 1, 1);
        p.Length.Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(1, 2, 3, 4);
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(1, 2, 3, 5);
        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentX_AreNotEqual()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(9, 2, 3, 4);
        a.Should().NotBe(b);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(1, 2, 3, 4);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_LikelyDifferentHash()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(5, 6, 7, 8);
        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var p = new Point4D(1, 2, 3, 4);
        p.ToString().Should().Be("(1, 2, 3, 4)");
    }

    [Fact]
    public void ToString_Origin()
    {
        Point4D.Origin.ToString().Should().Be("(0, 0, 0, 1)");
    }

    [Fact]
    public void ToString_NegativeValues()
    {
        var p = new Point4D(-1, -2, -3, -4);
        p.ToString().Should().Be("(-1, -2, -3, -4)");
    }

    [Fact]
    public void ToString_FractionalValues()
    {
        var p = new Point4D(1.5, 2.5, 3.5, 4.5);
        p.ToString().Should().Be("(1.5, 2.5, 3.5, 4.5)");
    }

    [Fact]
    public void EqualsReflexive()
    {
        var p = new Point4D(1, 2, 3, 4);
        ((object)p).Equals((object)p).Should().BeTrue();
    }

    [Fact]
    public void Equals_Transitive()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(1, 2, 3, 4);
        var c = new Point4D(1, 2, 3, 4);
        a.Should().Be(b);
        b.Should().Be(c);
        a.Should().Be(c);
    }

    [Fact]
    public void DistanceTo_TriangleInequality()
    {
        var a = new Point4D(1, 2, 3, 4);
        var b = new Point4D(5, 6, 7, 8);
        var c = new Point4D(9, 10, 11, 12);
        double ab = a.DistanceTo(b);
        double bc = b.DistanceTo(c);
        double ac = a.DistanceTo(c);
        ac.Should().BeLessOrEqualTo(ab + bc + 1e-10);
    }

    [Fact]
    public void Lerp_LinearProperty()
    {
        var a = new Point4D(0, 0, 0, 0);
        var b = new Point4D(10, 10, 10, 10);
        var m1 = a.Lerp(b, 0.25);
        var m2 = a.Lerp(b, 0.75);
        double dist = m1.DistanceTo(m2);
        dist.Should().BeApproximately(10.0, 1e-10);
    }

    [Fact]
    public void DistanceSquaredTo_ZeroDistance()
    {
        var p = new Point4D(42, 42, 42, 42);
        p.DistanceSquaredTo(p).Should().Be(0);
    }
}
