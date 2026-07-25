namespace MathVerse.TypeSystem.Tests;

public class AlgebraicStructureTests
{
    [Fact]
    public void Magma_Creates()
    {
        var m = new Magma(RealType.Instance);
        m.Kind.Should().Be(AlgebraicStructureKind.Magma);
    }

    [Fact]
    public void Magma_IsNotAssociative()
    {
        var m = new Magma(RealType.Instance);
        m.IsAssociative.Should().BeFalse();
    }

    [Fact]
    public void Magma_IsNotCommutative()
    {
        var m = new Magma(RealType.Instance);
        m.IsCommutative.Should().BeFalse();
    }

    [Fact]
    public void Magma_HasNoIdentity()
    {
        var m = new Magma(RealType.Instance);
        m.HasIdentity.Should().BeFalse();
    }

    [Fact]
    public void Magma_ElementType()
    {
        var m = new Magma(RealType.Instance);
        m.ElementType.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Magma_Equals()
    {
        var m1 = new Magma(RealType.Instance);
        var m2 = new Magma(RealType.Instance);
        m1.Equals(m2).Should().BeTrue();
    }

    [Fact]
    public void Magma_NotEquals_DifferentType()
    {
        var m1 = new Magma(RealType.Instance);
        var m2 = new Magma(IntegerType.Instance);
        m1.Equals(m2).Should().BeFalse();
    }

    [Fact]
    public void Magma_GetHashCode()
    {
        var m = new Magma(RealType.Instance);
        m.GetHashCode().Should().Be(m.GetHashCode());
    }

    [Fact]
    public void Magma_ToString()
    {
        var m = new Magma(RealType.Instance);
        m.ToString().Should().Be("Magma(Real)");
    }

    [Fact]
    public void Semigroup_Creates()
    {
        var s = new Semigroup(RealType.Instance);
        s.Kind.Should().Be(AlgebraicStructureKind.Semigroup);
    }

    [Fact]
    public void Semigroup_IsAssociative()
    {
        var s = new Semigroup(RealType.Instance);
        s.IsAssociative.Should().BeTrue();
    }

    [Fact]
    public void Semigroup_Equals()
    {
        var s1 = new Semigroup(RealType.Instance);
        var s2 = new Semigroup(RealType.Instance);
        s1.Equals(s2).Should().BeTrue();
    }

    [Fact]
    public void Semigroup_GetHashCode()
    {
        var s = new Semigroup(RealType.Instance);
        s.GetHashCode().Should().Be(s.GetHashCode());
    }

    [Fact]
    public void Monoid_Creates()
    {
        var m = new Monoid(RealType.Instance);
        m.Kind.Should().Be(AlgebraicStructureKind.Monoid);
    }

    [Fact]
    public void Monoid_HasIdentity()
    {
        var m = new Monoid(RealType.Instance);
        m.HasIdentity.Should().BeTrue();
    }

    [Fact]
    public void Monoid_IsAssociative()
    {
        var m = new Monoid(RealType.Instance);
        m.IsAssociative.Should().BeTrue();
    }

    [Fact]
    public void Monoid_Equals()
    {
        var m1 = new Monoid(RealType.Instance);
        var m2 = new Monoid(RealType.Instance);
        m1.Equals(m2).Should().BeTrue();
    }

    [Fact]
    public void Monoid_GetHashCode()
    {
        var m = new Monoid(RealType.Instance);
        m.GetHashCode().Should().Be(m.GetHashCode());
    }

    [Fact]
    public void Group_Creates()
    {
        var g = new Group(RealType.Instance);
        g.Kind.Should().Be(AlgebraicStructureKind.Group);
    }

    [Fact]
    public void Group_HasInverses()
    {
        var g = new Group(RealType.Instance);
        g.HasInverses.Should().BeTrue();
    }

    [Fact]
    public void Group_HasIdentity()
    {
        var g = new Group(RealType.Instance);
        g.HasIdentity.Should().BeTrue();
    }

    [Fact]
    public void Group_IsNotCommutative()
    {
        var g = new Group(RealType.Instance);
        g.IsCommutative.Should().BeFalse();
    }

    [Fact]
    public void Group_IsAbelian()
    {
        var g = new Group(RealType.Instance, isAbelian: true);
        g.IsAbelianGroup.Should().BeTrue();
        g.Kind.Should().Be(AlgebraicStructureKind.AbelianGroup);
        g.IsCommutative.Should().BeTrue();
    }

    [Fact]
    public void Group_Equals()
    {
        var g1 = new Group(RealType.Instance);
        var g2 = new Group(RealType.Instance);
        g1.Equals(g2).Should().BeTrue();
    }

    [Fact]
    public void Group_Equals_Abelian()
    {
        var g1 = new Group(RealType.Instance, isAbelian: true);
        var g2 = new Group(RealType.Instance, isAbelian: true);
        g1.Equals(g2).Should().BeTrue();
    }

    [Fact]
    public void Group_NotEquals_AbelianVsNot()
    {
        var g1 = new Group(RealType.Instance, isAbelian: true);
        var g2 = new Group(RealType.Instance, isAbelian: false);
        g1.Equals(g2).Should().BeFalse();
    }

    [Fact]
    public void Group_GetHashCode()
    {
        var g = new Group(RealType.Instance);
        g.GetHashCode().Should().Be(g.GetHashCode());
    }

    [Fact]
    public void AbelianGroup_Creates()
    {
        var ag = new AbelianGroup(RealType.Instance);
        ag.Kind.Should().Be(AlgebraicStructureKind.AbelianGroup);
    }

    [Fact]
    public void AbelianGroup_IsCommutative()
    {
        var ag = new AbelianGroup(RealType.Instance);
        ag.IsCommutative.Should().BeTrue();
    }

    [Fact]
    public void AbelianGroup_HasInverses()
    {
        var ag = new AbelianGroup(RealType.Instance);
        ag.HasInverses.Should().BeTrue();
    }

    [Fact]
    public void AbelianGroup_Equals()
    {
        var ag1 = new AbelianGroup(RealType.Instance);
        var ag2 = new AbelianGroup(RealType.Instance);
        ag1.Equals(ag2).Should().BeTrue();
    }

    [Fact]
    public void AbelianGroup_GetHashCode()
    {
        var ag = new AbelianGroup(RealType.Instance);
        ag.GetHashCode().Should().Be(ag.GetHashCode());
    }

    [Fact]
    public void Ring_Creates()
    {
        var r = new Ring(IntegerType.Instance);
        r.Kind.Should().Be(AlgebraicStructureKind.Ring);
    }

    [Fact]
    public void Ring_IsDistributive()
    {
        var r = new Ring(IntegerType.Instance);
        r.IsDistributive.Should().BeTrue();
    }

    [Fact]
    public void Ring_IsAssociative()
    {
        var r = new Ring(IntegerType.Instance);
        r.IsAssociative.Should().BeTrue();
    }

    [Fact]
    public void Ring_Commutative()
    {
        var r = new Ring(IntegerType.Instance, isCommutative: true);
        r.IsCommutativeRing.Should().BeTrue();
    }

    [Fact]
    public void Ring_Unital()
    {
        var r = new Ring(IntegerType.Instance, isUnital: true);
        r.IsUnital.Should().BeTrue();
    }

    [Fact]
    public void Ring_Equals()
    {
        var r1 = new Ring(IntegerType.Instance, true, true);
        var r2 = new Ring(IntegerType.Instance, true, true);
        r1.Equals(r2).Should().BeTrue();
    }

    [Fact]
    public void Ring_GetHashCode()
    {
        var r = new Ring(IntegerType.Instance);
        r.GetHashCode().Should().Be(r.GetHashCode());
    }

    [Fact]
    public void IntegralDomain_Creates()
    {
        var id = new IntegralDomain(IntegerType.Instance);
        id.Kind.Should().Be(AlgebraicStructureKind.IntegralDomain);
    }

    [Fact]
    public void IntegralDomain_NoZeroDivisors()
    {
        var id = new IntegralDomain(IntegerType.Instance);
        id.HasZeroDivisors.Should().BeFalse();
    }

    [Fact]
    public void IntegralDomain_IsDistributive()
    {
        var id = new IntegralDomain(IntegerType.Instance);
        id.IsDistributive.Should().BeTrue();
    }

    [Fact]
    public void IntegralDomain_Equals()
    {
        var id1 = new IntegralDomain(IntegerType.Instance);
        var id2 = new IntegralDomain(IntegerType.Instance);
        id1.Equals(id2).Should().BeTrue();
    }

    [Fact]
    public void IntegralDomain_GetHashCode()
    {
        var id = new IntegralDomain(IntegerType.Instance);
        id.GetHashCode().Should().Be(id.GetHashCode());
    }

    [Fact]
    public void Field_Creates()
    {
        var f = new Field(RealType.Instance);
        f.Kind.Should().Be(AlgebraicStructureKind.Field);
    }

    [Fact]
    public void Field_HasInverses()
    {
        var f = new Field(RealType.Instance);
        f.HasInverses.Should().BeTrue();
    }

    [Fact]
    public void Field_IsCommutative()
    {
        var f = new Field(RealType.Instance);
        f.IsCommutative.Should().BeTrue();
    }

    [Fact]
    public void Field_NoZeroDivisors()
    {
        var f = new Field(RealType.Instance);
        f.HasZeroDivisors.Should().BeFalse();
    }

    [Fact]
    public void Field_Equals()
    {
        var f1 = new Field(RealType.Instance);
        var f2 = new Field(RealType.Instance);
        f1.Equals(f2).Should().BeTrue();
    }

    [Fact]
    public void Field_GetHashCode()
    {
        var f = new Field(RealType.Instance);
        f.GetHashCode().Should().Be(f.GetHashCode());
    }

    [Fact]
    public void VectorSpace_Creates()
    {
        var f = new Field(RealType.Instance);
        var vs = new VectorSpace(new VectorType(RealType.Instance), f);
        vs.Kind.Should().Be(AlgebraicStructureKind.VectorSpace);
    }

    [Fact]
    public void VectorSpace_IsDistributive()
    {
        var f = new Field(RealType.Instance);
        var vs = new VectorSpace(new VectorType(RealType.Instance), f);
        vs.IsDistributive.Should().BeTrue();
    }

    [Fact]
    public void VectorSpace_Equals()
    {
        var f = new Field(RealType.Instance);
        var vs1 = new VectorSpace(new VectorType(RealType.Instance), f);
        var vs2 = new VectorSpace(new VectorType(RealType.Instance), f);
        vs1.Equals(vs2).Should().BeTrue();
    }

    [Fact]
    public void VectorSpace_GetHashCode()
    {
        var f = new Field(RealType.Instance);
        var vs = new VectorSpace(new VectorType(RealType.Instance), f);
        vs.GetHashCode().Should().Be(vs.GetHashCode());
    }

    [Fact]
    public void Module_Creates()
    {
        var r = new Ring(IntegerType.Instance);
        var m = new Module(new VectorType(IntegerType.Instance), r);
        m.Kind.Should().Be(AlgebraicStructureKind.Module);
    }

    [Fact]
    public void Module_IsDistributive()
    {
        var r = new Ring(IntegerType.Instance);
        var m = new Module(new VectorType(IntegerType.Instance), r);
        m.IsDistributive.Should().BeTrue();
    }

    [Fact]
    public void Module_Equals()
    {
        var r = new Ring(IntegerType.Instance);
        var m1 = new Module(new VectorType(IntegerType.Instance), r);
        var m2 = new Module(new VectorType(IntegerType.Instance), r);
        m1.Equals(m2).Should().BeTrue();
    }

    [Fact]
    public void Module_GetHashCode()
    {
        var r = new Ring(IntegerType.Instance);
        var m = new Module(new VectorType(IntegerType.Instance), r);
        m.GetHashCode().Should().Be(m.GetHashCode());
    }

    [Fact]
    public void InnerProductSpace_Creates()
    {
        var f = new Field(RealType.Instance);
        var vs = new VectorSpace(new VectorType(RealType.Instance), f);
        var ips = new InnerProductSpace(new VectorType(RealType.Instance), vs);
        ips.Kind.Should().Be(AlgebraicStructureKind.InnerProductSpace);
    }

    [Fact]
    public void InnerProductSpace_IsSymmetric()
    {
        var f = new Field(RealType.Instance);
        var vs = new VectorSpace(new VectorType(RealType.Instance), f);
        var ips = new InnerProductSpace(new VectorType(RealType.Instance), vs, isSymmetric: true);
        ips.IsSymmetric.Should().BeTrue();
    }

    [Fact]
    public void InnerProductSpace_IsPositiveDefinite()
    {
        var f = new Field(RealType.Instance);
        var vs = new VectorSpace(new VectorType(RealType.Instance), f);
        var ips = new InnerProductSpace(new VectorType(RealType.Instance), vs, isPositiveDefinite: true);
        ips.IsPositiveDefinite.Should().BeTrue();
    }

    [Fact]
    public void InnerProductSpace_Equals()
    {
        var f = new Field(RealType.Instance);
        var vs = new VectorSpace(new VectorType(RealType.Instance), f);
        var ips1 = new InnerProductSpace(new VectorType(RealType.Instance), vs);
        var ips2 = new InnerProductSpace(new VectorType(RealType.Instance), vs);
        ips1.Equals(ips2).Should().BeTrue();
    }

    [Fact]
    public void InnerProductSpace_GetHashCode()
    {
        var f = new Field(RealType.Instance);
        var vs = new VectorSpace(new VectorType(RealType.Instance), f);
        var ips = new InnerProductSpace(new VectorType(RealType.Instance), vs);
        ips.GetHashCode().Should().Be(ips.GetHashCode());
    }

    [Fact]
    public void MetricSpace_Creates()
    {
        var ms = new MetricSpace(RealType.Instance);
        ms.Kind.Should().Be(AlgebraicStructureKind.MetricSpace);
    }

    [Fact]
    public void MetricSpace_IsNormed()
    {
        var ms = new MetricSpace(RealType.Instance, isNormed: true);
        ms.IsNormed.Should().BeTrue();
    }

    [Fact]
    public void MetricSpace_Equals()
    {
        var ms1 = new MetricSpace(RealType.Instance);
        var ms2 = new MetricSpace(RealType.Instance);
        ms1.Equals(ms2).Should().BeTrue();
    }

    [Fact]
    public void MetricSpace_GetHashCode()
    {
        var ms = new MetricSpace(RealType.Instance);
        ms.GetHashCode().Should().Be(ms.GetHashCode());
    }

    [Fact]
    public void OrderedField_Creates()
    {
        var of = new OrderedField(RealType.Instance);
        of.Kind.Should().Be(AlgebraicStructureKind.OrderedField);
    }

    [Fact]
    public void OrderedField_Equals()
    {
        var of1 = new OrderedField(RealType.Instance);
        var of2 = new OrderedField(RealType.Instance);
        of1.Equals(of2).Should().BeTrue();
    }

    [Fact]
    public void MatrixSpace_Creates()
    {
        var f = new Field(RealType.Instance);
        var ms = new MatrixSpace(new MatrixType(RealType.Instance, 2, 2), f);
        ms.Kind.Should().Be(AlgebraicStructureKind.MatrixSpace);
    }

    [Fact]
    public void MatrixSpace_Equals()
    {
        var f = new Field(RealType.Instance);
        var ms1 = new MatrixSpace(new MatrixType(RealType.Instance, 2, 2), f);
        var ms2 = new MatrixSpace(new MatrixType(RealType.Instance, 2, 2), f);
        ms1.Equals(ms2).Should().BeTrue();
    }
}
