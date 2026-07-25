namespace MathVerse.TypeSystem.Tests;

public class TypeCoercionTests
{
    [Fact]
    public void ConversionCost_Zero()
    {
        ConversionCost.Zero.Value.Should().Be(0);
    }

    [Fact]
    public void ConversionCost_IsZero()
    {
        ConversionCost.Zero.IsZero.Should().BeTrue();
    }

    [Fact]
    public void ConversionCost_Widening()
    {
        ConversionCost.Widening.Value.Should().Be(1);
    }

    [Fact]
    public void ConversionCost_Narrowing()
    {
        ConversionCost.Narrowing.Value.Should().Be(10);
    }

    [Fact]
    public void ConversionCost_Explicit()
    {
        ConversionCost.Explicit.Value.Should().Be(100);
    }

    [Fact]
    public void ConversionCost_Impossible()
    {
        ConversionCost.Impossible.Value.Should().Be(int.MaxValue);
    }

    [Fact]
    public void ConversionCost_IsPossible_True()
    {
        ConversionCost.Zero.IsPossible.Should().BeTrue();
    }

    [Fact]
    public void ConversionCost_IsPossible_False()
    {
        ConversionCost.Impossible.IsPossible.Should().BeFalse();
    }

    [Fact]
    public void ConversionCost_Equals()
    {
        var c1 = new ConversionCost(5);
        var c2 = new ConversionCost(5);
        c1.Equals(c2).Should().BeTrue();
    }

    [Fact]
    public void ConversionCost_NotEquals()
    {
        var c1 = new ConversionCost(5);
        var c2 = new ConversionCost(10);
        c1.Equals(c2).Should().BeFalse();
    }

    [Fact]
    public void ConversionCost_GetHashCode()
    {
        var c = new ConversionCost(5);
        c.GetHashCode().Should().Be(c.GetHashCode());
    }

    [Fact]
    public void ConversionCost_CompareTo_Less()
    {
        var c1 = new ConversionCost(5);
        var c2 = new ConversionCost(10);
        c1.CompareTo(c2).Should().BeLessThan(0);
    }

    [Fact]
    public void ConversionCost_CompareTo_Greater()
    {
        var c1 = new ConversionCost(10);
        var c2 = new ConversionCost(5);
        c1.CompareTo(c2).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ConversionCost_CompareTo_Equal()
    {
        var c1 = new ConversionCost(5);
        var c2 = new ConversionCost(5);
        c1.CompareTo(c2).Should().Be(0);
    }

    [Fact]
    public void ConversionCost_Addition()
    {
        var c1 = new ConversionCost(3);
        var c2 = new ConversionCost(4);
        (c1 + c2).Value.Should().Be(7);
    }

    [Fact]
    public void ConversionCost_LessThan()
    {
        var c1 = new ConversionCost(3);
        var c2 = new ConversionCost(4);
        (c1 < c2).Should().BeTrue();
    }

    [Fact]
    public void ConversionCost_GreaterThan()
    {
        var c1 = new ConversionCost(4);
        var c2 = new ConversionCost(3);
        (c1 > c2).Should().BeTrue();
    }

    [Fact]
    public void ConversionCost_ToString_Zero()
    {
        ConversionCost.Zero.ToString().Should().Be("zero");
    }

    [Fact]
    public void ConversionCost_ToString_Widening()
    {
        ConversionCost.Widening.ToString().Should().Be("widening");
    }

    [Fact]
    public void ConversionCost_ToString_Narrowing()
    {
        ConversionCost.Narrowing.ToString().Should().Be("narrowing");
    }

    [Fact]
    public void ConversionCost_ToString_Explicit()
    {
        ConversionCost.Explicit.ToString().Should().Be("explicit");
    }

    [Fact]
    public void ConversionCost_ToString_Impossible()
    {
        ConversionCost.Impossible.ToString().Should().Be("impossible");
    }

    [Fact]
    public void ConversionCost_ToString_Other()
    {
        var c = new ConversionCost(7);
        c.ToString().Should().Be("cost(7)");
    }

    [Fact]
    public void CoercionRule_Creates()
    {
        var rule = new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, ConversionCost.Widening);
        rule.Should().NotBeNull();
    }

    [Fact]
    public void CoercionRule_From()
    {
        var rule = new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, ConversionCost.Widening);
        rule.From.Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void CoercionRule_To()
    {
        var rule = new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, ConversionCost.Widening);
        rule.To.Should().Be(RealType.Instance);
    }

    [Fact]
    public void CoercionRule_IsImplicit()
    {
        var rule = new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, ConversionCost.Widening);
        rule.IsImplicit.Should().BeTrue();
    }

    [Fact]
    public void CoercionRule_IsNotImplicit_Explicit()
    {
        var rule = new CoercionRule(RealType.Instance, IntegerType.Instance,
            CoercionKind.Explicit, ConversionCost.Explicit);
        rule.IsImplicit.Should().BeFalse();
    }

    [Fact]
    public void CoercionRule_Equals()
    {
        var r1 = new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, ConversionCost.Widening);
        var r2 = new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitNarrowing, ConversionCost.Narrowing);
        r1.Equals(r2).Should().BeTrue();
    }

    [Fact]
    public void CoercionRule_GetHashCode()
    {
        var r = new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, ConversionCost.Widening);
        r.GetHashCode().Should().Be(r.GetHashCode());
    }

    [Fact]
    public void CoercionRule_ToString()
    {
        var r = new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, ConversionCost.Widening);
        r.ToString().Should().Contain("Integer");
        r.ToString().Should().Contain("Real");
    }

    [Fact]
    public void ImplicitConversion_Creates()
    {
        var conv = new ImplicitConversion(IntegerType.Instance, RealType.Instance,
            ConversionCost.Widening);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void ImplicitConversion_Equals()
    {
        var c1 = new ImplicitConversion(IntegerType.Instance, RealType.Instance,
            ConversionCost.Widening);
        var c2 = new ImplicitConversion(IntegerType.Instance, RealType.Instance,
            ConversionCost.Widening);
        c1.Equals(c2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_GetHashCode()
    {
        var c = new ImplicitConversion(IntegerType.Instance, RealType.Instance,
            ConversionCost.Widening);
        c.GetHashCode().Should().Be(c.GetHashCode());
    }

    [Fact]
    public void ExplicitConversion_Creates()
    {
        var conv = new ExplicitConversion(RealType.Instance, IntegerType.Instance);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void ExplicitConversion_MayLoseData()
    {
        var conv = new ExplicitConversion(RealType.Instance, IntegerType.Instance, true);
        conv.MayLoseData.Should().BeTrue();
    }

    [Fact]
    public void ExplicitConversion_Equals()
    {
        var c1 = new ExplicitConversion(RealType.Instance, IntegerType.Instance);
        var c2 = new ExplicitConversion(RealType.Instance, IntegerType.Instance);
        c1.Equals(c2).Should().BeTrue();
    }

    [Fact]
    public void ExplicitConversion_GetHashCode()
    {
        var c = new ExplicitConversion(RealType.Instance, IntegerType.Instance);
        c.GetHashCode().Should().Be(c.GetHashCode());
    }

    [Fact]
    public void TypeCoercion_DefaultRules()
    {
        var tc = new TypeCoercion();
        tc.Rules.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void TypeCoercion_ImplicitConversion_IntToReal()
    {
        var tc = new TypeCoercion();
        var conv = tc.FindImplicitConversion(IntegerType.Instance, RealType.Instance);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void TypeCoercion_ImplicitConversion_IntToComplex()
    {
        var tc = new TypeCoercion();
        var conv = tc.FindImplicitConversion(IntegerType.Instance, ComplexType.Instance);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void TypeCoercion_ImplicitConversion_RationalToReal()
    {
        var tc = new TypeCoercion();
        var conv = tc.FindImplicitConversion(RationalType.Instance, RealType.Instance);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void TypeCoercion_ImplicitConversion_RealToComplex()
    {
        var tc = new TypeCoercion();
        var conv = tc.FindImplicitConversion(RealType.Instance, ComplexType.Instance);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void TypeCoercion_ImplicitConversion_SameType()
    {
        var tc = new TypeCoercion();
        var conv = tc.FindImplicitConversion(RealType.Instance, RealType.Instance);
        conv.Should().NotBeNull();
        conv!.Cost.Should().Be(ConversionCost.Zero);
    }

    [Fact]
    public void TypeCoercion_NoImplicitConversion_ComplexToInt()
    {
        var tc = new TypeCoercion();
        var conv = tc.FindImplicitConversion(ComplexType.Instance, IntegerType.Instance);
        conv.Should().BeNull();
    }

    [Fact]
    public void TypeCoercion_ExplicitConversion_ComplexToInt()
    {
        var tc = new TypeCoercion();
        var conv = tc.FindExplicitConversion(ComplexType.Instance, IntegerType.Instance);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void TypeCoercion_CanImplicitlyConvert_True()
    {
        var tc = new TypeCoercion();
        tc.CanImplicitlyConvert(IntegerType.Instance, RealType.Instance).Should().BeTrue();
    }

    [Fact]
    public void TypeCoercion_CanImplicitlyConvert_False()
    {
        var tc = new TypeCoercion();
        tc.CanImplicitlyConvert(ComplexType.Instance, IntegerType.Instance).Should().BeFalse();
    }

    [Fact]
    public void TypeCoercion_CanExplicitlyConvert()
    {
        var tc = new TypeCoercion();
        tc.CanExplicitlyConvert(ComplexType.Instance, IntegerType.Instance).Should().BeTrue();
    }

    [Fact]
    public void TypeCoercion_GetConversionCost_Same()
    {
        var tc = new TypeCoercion();
        var cost = tc.GetConversionCost(RealType.Instance, RealType.Instance);
        cost.Should().Be(ConversionCost.Zero);
    }

    [Fact]
    public void TypeCoercion_GetConversionCost_Widening()
    {
        var tc = new TypeCoercion();
        var cost = tc.GetConversionCost(IntegerType.Instance, RealType.Instance);
        cost.IsPossible.Should().BeTrue();
    }

    [Fact]
    public void TypeCoercion_GetConversionCost_Impossible()
    {
        var tc = new TypeCoercion();
        var cost = tc.GetConversionCost(ComplexType.Instance, BooleanType.Instance);
        cost.Should().Be(ConversionCost.Impossible);
    }

    [Fact]
    public void TypeCoercion_Vector_ImplicitConversion()
    {
        var tc = new TypeCoercion();
        var from = new VectorType(IntegerType.Instance, 3);
        var to = new VectorType(RealType.Instance, 3);
        var conv = tc.FindImplicitConversion(from, to);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void TypeCoercion_Matrix_ImplicitConversion()
    {
        var tc = new TypeCoercion();
        var from = new MatrixType(IntegerType.Instance, 2, 2);
        var to = new MatrixType(RealType.Instance, 2, 2);
        var conv = tc.FindImplicitConversion(from, to);
        conv.Should().NotBeNull();
    }

    [Fact]
    public void TypeCoercion_Vector_DimensionMismatch()
    {
        var tc = new TypeCoercion();
        var from = new VectorType(IntegerType.Instance, 3);
        var to = new VectorType(RealType.Instance, 4);
        var conv = tc.FindImplicitConversion(from, to);
        conv.Should().BeNull();
    }

    [Fact]
    public void TypeCoercion_RegisterCustomRule()
    {
        var tc = new TypeCoercion();
        tc.RegisterRule(new CoercionRule(BooleanType.Instance, IntegerType.Instance,
            CoercionKind.ImplicitWidening, new ConversionCost(1)));
        var conv = tc.FindImplicitConversion(BooleanType.Instance, IntegerType.Instance);
        conv.Should().NotBeNull();
    }
}
