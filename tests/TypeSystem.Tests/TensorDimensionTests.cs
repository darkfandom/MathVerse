namespace MathVerse.TypeSystem.Tests;

public class TensorDimensionTests
{
    [Fact]
    public void Dimension_Creates()
    {
        var d = new Dimension(3);
        d.Should().NotBeNull();
    }

    [Fact]
    public void Dimension_Size()
    {
        var d = new Dimension(5);
        d.Size.Should().Be(5);
    }

    [Fact]
    public void Dimension_Dynamic()
    {
        var d = new Dimension();
        d.Size.Should().BeNull();
    }

    [Fact]
    public void Dimension_IsFixed()
    {
        var d = new Dimension(3);
        d.IsFixed.Should().BeTrue();
    }

    [Fact]
    public void Dimension_IsNotFixed()
    {
        var d = new Dimension();
        d.IsFixed.Should().BeFalse();
    }

    [Fact]
    public void Dimension_IsScalar()
    {
        var d = new Dimension(1);
        d.IsScalar.Should().BeTrue();
    }

    [Fact]
    public void Dimension_IsNotScalar()
    {
        var d = new Dimension(5);
        d.IsScalar.Should().BeFalse();
    }

    [Fact]
    public void Dimension_WithName()
    {
        var d = new Dimension(3, "batch");
        d.Name.Should().Be("batch");
    }

    [Fact]
    public void Dimension_Equals()
    {
        var d1 = new Dimension(3);
        var d2 = new Dimension(3);
        d1.Equals(d2).Should().BeTrue();
    }

    [Fact]
    public void Dimension_NotEquals_DifferentSize()
    {
        var d1 = new Dimension(3);
        var d2 = new Dimension(4);
        d1.Equals(d2).Should().BeFalse();
    }

    [Fact]
    public void Dimension_NotEquals_DifferentName()
    {
        var d1 = new Dimension(3, "a");
        var d2 = new Dimension(3, "b");
        d1.Equals(d2).Should().BeFalse();
    }

    [Fact]
    public void Dimension_GetHashCode()
    {
        var d = new Dimension(3);
        d.GetHashCode().Should().Be(d.GetHashCode());
    }

    [Fact]
    public void Dimension_ToString_WithName()
    {
        var d = new Dimension(3, "batch");
        d.ToString().Should().Be("batch");
    }

    [Fact]
    public void Dimension_ToString_WithoutName()
    {
        var d = new Dimension(3);
        d.ToString().Should().Be("3");
    }

    [Fact]
    public void Dimension_ToString_Dynamic()
    {
        var d = new Dimension();
        d.ToString().Should().Be("?");
    }

    [Fact]
    public void TensorShape_Creates()
    {
        var ts = new TensorShape(2, 3, 4);
        ts.Should().NotBeNull();
    }

    [Fact]
    public void TensorShape_Rank()
    {
        var ts = new TensorShape(2, 3, 4);
        ts.Rank.Should().Be(3);
    }

    [Fact]
    public void TensorShape_TotalSize()
    {
        var ts = new TensorShape(2, 3, 4);
        ts.TotalSize.Should().Be(24);
    }

    [Fact]
    public void TensorShape_IsFullyStatic()
    {
        var ts = new TensorShape(2, 3);
        ts.IsFullyStatic.Should().BeTrue();
    }

    [Fact]
    public void TensorShape_IsScalar()
    {
        var ts = new TensorShape();
        ts.IsScalar.Should().BeTrue();
    }

    [Fact]
    public void TensorShape_IsVector()
    {
        var ts = new TensorShape(5);
        ts.IsVector.Should().BeTrue();
    }

    [Fact]
    public void TensorShape_IsMatrix()
    {
        var ts = new TensorShape(3, 4);
        ts.IsMatrix.Should().BeTrue();
    }

    [Fact]
    public void TensorShape_IsBroadcastableWith_Same()
    {
        var ts1 = new TensorShape(3, 4);
        var ts2 = new TensorShape(3, 4);
        ts1.IsBroadcastableWith(ts2).Should().BeTrue();
    }

    [Fact]
    public void TensorShape_IsBroadcastableWith_Different()
    {
        var ts1 = new TensorShape(3, 4);
        var ts2 = new TensorShape(3, 5);
        ts1.IsBroadcastableWith(ts2).Should().BeFalse();
    }

    [Fact]
    public void TensorShape_Equals()
    {
        var ts1 = new TensorShape(2, 3);
        var ts2 = new TensorShape(2, 3);
        ts1.Equals(ts2).Should().BeTrue();
    }

    [Fact]
    public void TensorShape_NotEquals()
    {
        var ts1 = new TensorShape(2, 3);
        var ts2 = new TensorShape(2, 4);
        ts1.Equals(ts2).Should().BeFalse();
    }

    [Fact]
    public void TensorShape_GetHashCode()
    {
        var ts = new TensorShape(2, 3);
        ts.GetHashCode().Should().Be(ts.GetHashCode());
    }

    [Fact]
    public void TensorShape_ToString_Scalar()
    {
        var ts = new TensorShape();
        ts.ToString().Should().Be("scalar");
    }

    [Fact]
    public void TensorShape_ToString_Vector()
    {
        var ts = new TensorShape(5);
        ts.ToString().Should().Be("[5]");
    }

    [Fact]
    public void TensorShape_ToString_Matrix()
    {
        var ts = new TensorShape(3, 4);
        ts.ToString().Should().Be("[3×4]");
    }

    [Fact]
    public void TensorShape_FromDimensionList()
    {
        var dims = new List<Dimension> { new(2), new(3) };
        var ts = new TensorShape(dims);
        ts.Rank.Should().Be(2);
    }

    [Fact]
    public void TensorRank_Scalar()
    {
        TensorRank.Scalar.Value.Should().Be(0);
    }

    [Fact]
    public void TensorRank_Vector()
    {
        TensorRank.Vector.Value.Should().Be(1);
    }

    [Fact]
    public void TensorRank_Matrix()
    {
        TensorRank.Matrix.Value.Should().Be(2);
    }

    [Fact]
    public void TensorRank_Equals()
    {
        var r1 = new TensorRank(3);
        var r2 = new TensorRank(3);
        r1.Equals(r2).Should().BeTrue();
    }

    [Fact]
    public void TensorRank_NotEquals()
    {
        var r1 = new TensorRank(3);
        var r2 = new TensorRank(4);
        r1.Equals(r2).Should().BeFalse();
    }

    [Fact]
    public void TensorRank_GetHashCode()
    {
        var r = new TensorRank(3);
        r.GetHashCode().Should().Be(r.GetHashCode());
    }

    [Fact]
    public void TensorRank_Addition()
    {
        var r1 = new TensorRank(1);
        var r2 = new TensorRank(2);
        (r1 + r2).Value.Should().Be(3);
    }

    [Fact]
    public void TensorRank_Subtraction()
    {
        var r1 = new TensorRank(3);
        var r2 = new TensorRank(1);
        (r1 - r2).Value.Should().Be(2);
    }

    [Fact]
    public void TensorRank_ToString_Scalar()
    {
        TensorRank.Scalar.ToString().Should().Be("scalar");
    }

    [Fact]
    public void TensorRank_ToString_Vector()
    {
        TensorRank.Vector.ToString().Should().Be("vector");
    }

    [Fact]
    public void TensorRank_ToString_Matrix()
    {
        TensorRank.Matrix.ToString().Should().Be("matrix");
    }

    [Fact]
    public void TensorRank_ToString_Higher()
    {
        var r = new TensorRank(5);
        r.ToString().Should().Be("rank-5");
    }

    [Fact]
    public void DimensionVector_Creates()
    {
        var dv = new DimensionVector(2, 3, 4);
        dv.Should().NotBeNull();
    }

    [Fact]
    public void DimensionVector_Rank()
    {
        var dv = new DimensionVector(2, 3, 4);
        dv.Rank.Should().Be(3);
    }

    [Fact]
    public void DimensionVector_TotalSize()
    {
        var dv = new DimensionVector(2, 3, 4);
        dv.TotalSize.Should().Be(24);
    }

    [Fact]
    public void DimensionVector_Matches_Same()
    {
        var dv1 = new DimensionVector(3, 4);
        var dv2 = new DimensionVector(3, 4);
        dv1.Matches(dv2).Should().BeTrue();
    }

    [Fact]
    public void DimensionVector_Matches_Different()
    {
        var dv1 = new DimensionVector(3, 4);
        var dv2 = new DimensionVector(3, 5);
        dv1.Matches(dv2).Should().BeFalse();
    }

    [Fact]
    public void DimensionVector_Matches_WithOne()
    {
        var dv1 = new DimensionVector(1, 4);
        var dv2 = new DimensionVector(3, 4);
        dv1.Matches(dv2).Should().BeTrue();
    }

    [Fact]
    public void DimensionVector_Equals()
    {
        var dv1 = new DimensionVector(2, 3);
        var dv2 = new DimensionVector(2, 3);
        dv1.Equals(dv2).Should().BeTrue();
    }

    [Fact]
    public void DimensionVector_NotEquals()
    {
        var dv1 = new DimensionVector(2, 3);
        var dv2 = new DimensionVector(2, 4);
        dv1.Equals(dv2).Should().BeFalse();
    }

    [Fact]
    public void DimensionVector_NotEquals_DifferentRank()
    {
        var dv1 = new DimensionVector(2, 3);
        var dv2 = new DimensionVector(2, 3, 4);
        dv1.Equals(dv2).Should().BeFalse();
    }

    [Fact]
    public void DimensionVector_GetHashCode()
    {
        var dv = new DimensionVector(2, 3);
        dv.GetHashCode().Should().Be(dv.GetHashCode());
    }

    [Fact]
    public void DimensionVector_ToString()
    {
        var dv = new DimensionVector(2, 3, 4);
        dv.ToString().Should().Be("[2×3×4]");
    }

    [Fact]
    public void DimensionVector_FromList()
    {
        var dv = new DimensionVector(new List<int> { 5, 6 });
        dv.Rank.Should().Be(2);
    }
}
