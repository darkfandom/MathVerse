namespace MathVerse.TypeSystem.Tests;

public class DomainTests
{
    [Fact]
    public void MathematicalDomain_Creates()
    {
        var d = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        d.Should().NotBeNull();
    }

    [Fact]
    public void MathematicalDomain_Symbol()
    {
        var d = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        d.Symbol.Should().Be("ℝ");
    }

    [Fact]
    public void MathematicalDomain_Name()
    {
        var d = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        d.Name.Should().Be("Reals");
    }

    [Fact]
    public void MathematicalDomain_IsCommutative()
    {
        var d = new MathematicalDomain("ℝ", "Reals", RealType.Instance, isCommutative: true);
        d.IsCommutative.Should().BeTrue();
    }

    [Fact]
    public void MathematicalDomain_IsOrdered()
    {
        var d = new MathematicalDomain("ℝ", "Reals", RealType.Instance, isOrdered: true);
        d.IsOrdered.Should().BeTrue();
    }

    [Fact]
    public void MathematicalDomain_IsAlgebraicallyClosed()
    {
        var d = new MathematicalDomain("ℂ", "Complex", ComplexType.Instance, isAlgebraicallyClosed: true);
        d.IsAlgebraicallyClosed.Should().BeTrue();
    }

    [Fact]
    public void MathematicalDomain_IsField()
    {
        var d = new MathematicalDomain("ℝ", "Reals", RealType.Instance, isField: true);
        d.IsField.Should().BeTrue();
    }

    [Fact]
    public void MathematicalDomain_IsFinite()
    {
        var d = new MathematicalDomain("Z2", "Z2", IntegerType.Instance, isFinite: true, cardinality: 2);
        d.IsFinite.Should().BeTrue();
        d.Cardinality.Should().Be(2);
    }

    [Fact]
    public void MathematicalDomain_Parent()
    {
        var parent = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        var child = new MathematicalDomain("ℂ", "Complex", ComplexType.Instance, parent: parent);
        child.Parent.Should().Be(parent);
    }

    [Fact]
    public void MathematicalDomain_Equals()
    {
        var d1 = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        var d2 = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        d1.Equals(d2).Should().BeTrue();
    }

    [Fact]
    public void MathematicalDomain_NotEquals()
    {
        var d1 = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        var d2 = new MathematicalDomain("ℂ", "Complex", ComplexType.Instance);
        d1.Equals(d2).Should().BeFalse();
    }

    [Fact]
    public void MathematicalDomain_GetHashCode()
    {
        var d = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        d.GetHashCode().Should().Be(d.GetHashCode());
    }

    [Fact]
    public void MathematicalDomain_ToString()
    {
        var d = new MathematicalDomain("ℝ", "Reals", RealType.Instance);
        d.ToString().Should().Be("ℝ (Reals)");
    }

    [Fact]
    public void DomainRegistry_HasNaturals()
    {
        var reg = new DomainRegistry();
        reg.Contains("ℕ").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_HasIntegers()
    {
        var reg = new DomainRegistry();
        reg.Contains("ℤ").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_HasRationals()
    {
        var reg = new DomainRegistry();
        reg.Contains("ℚ").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_HasReals()
    {
        var reg = new DomainRegistry();
        reg.Contains("ℝ").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_HasComplex()
    {
        var reg = new DomainRegistry();
        reg.Contains("ℂ").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_HasQuaternions()
    {
        var reg = new DomainRegistry();
        reg.Contains("ℍ").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_HasOctonions()
    {
        var reg = new DomainRegistry();
        reg.Contains("𝕆").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_Resolve_BySymbol()
    {
        var reg = new DomainRegistry();
        var d = reg.Resolve("ℝ");
        d.Should().NotBeNull();
        d!.Symbol.Should().Be("ℝ");
    }

    [Fact]
    public void DomainRegistry_Resolve_ByName()
    {
        var reg = new DomainRegistry();
        var d = reg.Resolve("Real Numbers");
        d.Should().NotBeNull();
        d!.Symbol.Should().Be("ℝ");
    }

    [Fact]
    public void DomainRegistry_Resolve_Unregistered()
    {
        var reg = new DomainRegistry();
        var d = reg.Resolve("X");
        d.Should().BeNull();
    }

    [Fact]
    public void DomainRegistry_ResolveByType()
    {
        var reg = new DomainRegistry();
        var d = reg.ResolveByType(RealType.Instance);
        d.Should().NotBeNull();
        d!.Symbol.Should().Be("ℝ");
    }

    [Fact]
    public void DomainRegistry_IsMemberOf()
    {
        var reg = new DomainRegistry();
        reg.IsMemberOf(RealType.Instance, "ℝ").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_IsMemberOf_False()
    {
        var reg = new DomainRegistry();
        reg.IsMemberOf(IntegerType.Instance, "ℝ").Should().BeFalse();
    }

    [Fact]
    public void DomainRegistry_FindCommonDomain_Same()
    {
        var reg = new DomainRegistry();
        var common = reg.FindCommonDomain(RealType.Instance, RealType.Instance);
        common.Should().NotBeNull();
        common!.Symbol.Should().Be("ℝ");
    }

    [Fact]
    public void DomainRegistry_FindCommonDomain_IntAndReal()
    {
        var reg = new DomainRegistry();
        var common = reg.FindCommonDomain(IntegerType.Instance, RealType.Instance);
        common.Should().NotBeNull();
    }

    [Fact]
    public void DomainRegistry_RegisterCustom()
    {
        var reg = new DomainRegistry();
        var custom = new MathematicalDomain("F2", "Field of 2", IntegerType.Instance,
            isFinite: true, cardinality: 2);
        reg.Register(custom);
        reg.Contains("F2").Should().BeTrue();
    }

    [Fact]
    public void DomainRegistry_Count()
    {
        var reg = new DomainRegistry();
        reg.Count.Should().BeGreaterThanOrEqualTo(7);
    }

    [Fact]
    public void DomainRegistry_Domains()
    {
        var reg = new DomainRegistry();
        reg.Domains.Should().NotBeEmpty();
    }

    [Fact]
    public void Naturals_IsNotField()
    {
        var reg = new DomainRegistry();
        var n = reg.Resolve("ℕ");
        n!.IsField.Should().BeFalse();
    }

    [Fact]
    public void Integers_IsNotField()
    {
        var reg = new DomainRegistry();
        var z = reg.Resolve("ℤ");
        z!.IsField.Should().BeFalse();
    }

    [Fact]
    public void Rationals_IsField()
    {
        var reg = new DomainRegistry();
        var q = reg.Resolve("ℚ");
        q!.IsField.Should().BeTrue();
    }

    [Fact]
    public void Complex_IsAlgebraicallyClosed()
    {
        var reg = new DomainRegistry();
        var c = reg.Resolve("ℂ");
        c!.IsAlgebraicallyClosed.Should().BeTrue();
    }

    [Fact]
    public void Complex_IsOrdered_False()
    {
        var reg = new DomainRegistry();
        var c = reg.Resolve("ℂ");
        c!.IsOrdered.Should().BeFalse();
    }

    [Fact]
    public void Quaternions_IsNotCommutative()
    {
        var reg = new DomainRegistry();
        var h = reg.Resolve("ℍ");
        h!.IsCommutative.Should().BeFalse();
    }

    [Fact]
    public void Octonions_IsNotCommutative()
    {
        var reg = new DomainRegistry();
        var o = reg.Resolve("𝕆");
        o!.IsCommutative.Should().BeFalse();
    }

    [Fact]
    public void Integers_ParentIsNaturals()
    {
        var reg = new DomainRegistry();
        var z = reg.Resolve("ℤ");
        z!.Parent.Should().NotBeNull();
        z!.Parent!.Symbol.Should().Be("ℕ");
    }

    [Fact]
    public void Rationals_ParentIsIntegers()
    {
        var reg = new DomainRegistry();
        var q = reg.Resolve("ℚ");
        q!.Parent.Should().NotBeNull();
        q!.Parent!.Symbol.Should().Be("ℤ");
    }

    [Fact]
    public void Reals_ParentIsRationals()
    {
        var reg = new DomainRegistry();
        var r = reg.Resolve("ℝ");
        r!.Parent.Should().NotBeNull();
        r!.Parent!.Symbol.Should().Be("ℚ");
    }

    [Fact]
    public void Complex_ParentIsReals()
    {
        var reg = new DomainRegistry();
        var c = reg.Resolve("ℂ");
        c!.Parent.Should().NotBeNull();
        c!.Parent!.Symbol.Should().Be("ℝ");
    }
}
