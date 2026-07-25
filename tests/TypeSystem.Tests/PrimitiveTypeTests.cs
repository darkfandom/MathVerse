namespace MathVerse.TypeSystem.Tests;

public class PrimitiveTypeTests
{
    [Fact]
    public void Integer_Singleton_IsNotNull()
    {
        IntegerType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Integer_Kind_IsInteger()
    {
        IntegerType.Instance.Kind.Should().Be(TypeKind.Integer);
    }

    [Fact]
    public void Integer_Name_IsInteger()
    {
        IntegerType.Instance.Name.Should().Be("Integer");
    }

    [Fact]
    public void Integer_IsNumeric()
    {
        IntegerType.Instance.IsNumeric.Should().BeTrue();
    }

    [Fact]
    public void Integer_IsIntegral()
    {
        IntegerType.Instance.IsIntegral.Should().BeTrue();
    }

    [Fact]
    public void Integer_IsOrdered()
    {
        ScalarType s = IntegerType.Instance;
        s.IsOrdered.Should().BeTrue();
    }

    [Fact]
    public void Integer_IsNotField()
    {
        IntegerType.Instance.IsField.Should().BeFalse();
    }

    [Fact]
    public void Integer_Supertype_IsRational()
    {
        ScalarType s = IntegerType.Instance;
        s.Supertype.Should().Be(RationalType.Instance);
    }

    [Fact]
    public void Integer_Equals_SameType()
    {
        IntegerType.Instance.Equals(IntegerType.Instance).Should().BeTrue();
    }

    [Fact]
    public void Integer_NotEquals_Real()
    {
        IntegerType.Instance.Equals(RealType.Instance).Should().BeFalse();
    }

    [Fact]
    public void Integer_NotEquals_Null()
    {
        IntegerType.Instance.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Integer_GetHashCode_IsStable()
    {
        var h1 = IntegerType.Instance.GetHashCode();
        var h2 = IntegerType.Instance.GetHashCode();
        h1.Should().Be(h2);
    }

    [Fact]
    public void Integer_IsNotGenericParameter()
    {
        IntegerType.Instance.IsGenericParameter.Should().BeFalse();
    }

    [Fact]
    public void Integer_IsNotError()
    {
        IntegerType.Instance.IsError.Should().BeFalse();
    }

    [Fact]
    public void Integer_IsNotUnknown()
    {
        IntegerType.Instance.IsUnknown.Should().BeFalse();
    }

    [Fact]
    public void Integer_IsNotUnit()
    {
        IntegerType.Instance.IsUnit.Should().BeFalse();
    }

    [Fact]
    public void TypedInteger_Create_StoresValue()
    {
        var ti = IntegerType.Create(42);
        ti.Value.Should().Be(42);
    }

    [Fact]
    public void TypedInteger_Kind_IsInteger()
    {
        var ti = IntegerType.Create(7);
        ti.Kind.Should().Be(TypeKind.Integer);
    }

    [Fact]
    public void TypedInteger_IsNumeric()
    {
        var ti = IntegerType.Create(5);
        ti.IsNumeric.Should().BeTrue();
    }

    [Fact]
    public void TypedInteger_IsIntegral()
    {
        var ti = IntegerType.Create(3);
        ti.IsIntegral.Should().BeTrue();
    }

    [Fact]
    public void TypedInteger_Supertype_IsRational()
    {
        ScalarType ti = IntegerType.Create(1);
        ti.Supertype.Should().Be(RationalType.Instance);
    }

    [Fact]
    public void TypedInteger_Equals_SameValue()
    {
        IntegerType.Create(10).Equals(IntegerType.Create(10)).Should().BeTrue();
    }

    [Fact]
    public void TypedInteger_NotEquals_DifferentValue()
    {
        IntegerType.Create(10).Equals(IntegerType.Create(20)).Should().BeFalse();
    }

    [Fact]
    public void TypedInteger_NotEquals_Singleton()
    {
        IntegerType.Create(5).Equals(IntegerType.Instance).Should().BeFalse();
    }

    [Fact]
    public void TypedInteger_GetHashCode_UsesValue()
    {
        IntegerType.Create(42).GetHashCode().Should().Be(42.GetHashCode());
    }

    [Fact]
    public void TypedInteger_Name_IsValue()
    {
        IntegerType.Create(99).Name.Should().Be("99");
    }

    [Fact]
    public void Rational_Singleton()
    {
        RationalType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Rational_Kind()
    {
        RationalType.Instance.Kind.Should().Be(TypeKind.Rational);
    }

    [Fact]
    public void Rational_Name()
    {
        RationalType.Instance.Name.Should().Be("Rational");
    }

    [Fact]
    public void Rational_IsNumeric()
    {
        RationalType.Instance.IsNumeric.Should().BeTrue();
    }

    [Fact]
    public void Rational_IsField()
    {
        RationalType.Instance.IsField.Should().BeTrue();
    }

    [Fact]
    public void Rational_IsNotIntegral()
    {
        RationalType.Instance.IsIntegral.Should().BeFalse();
    }

    [Fact]
    public void Rational_Supertype_IsReal()
    {
        ScalarType s = RationalType.Instance;
        s.Supertype.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Rational_IsOrdered()
    {
        ScalarType s = RationalType.Instance;
        s.IsOrdered.Should().BeTrue();
    }

    [Fact]
    public void Rational_IsNotAlgebraicallyClosed()
    {
        ScalarType s = RationalType.Instance;
        s.IsAlgebraicallyClosed.Should().BeFalse();
    }

    [Fact]
    public void Rational_Equals()
    {
        RationalType.Instance.Equals(RationalType.Instance).Should().BeTrue();
    }

    [Fact]
    public void Rational_NotEquals_Integer()
    {
        RationalType.Instance.Equals(IntegerType.Instance).Should().BeFalse();
    }

    [Fact]
    public void Rational_GetHashCode()
    {
        var h1 = RationalType.Instance.GetHashCode();
        var h2 = RationalType.Instance.GetHashCode();
        h1.Should().Be(h2);
    }

    [Fact]
    public void Real_Singleton()
    {
        RealType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Real_Kind()
    {
        RealType.Instance.Kind.Should().Be(TypeKind.Real);
    }

    [Fact]
    public void Real_Name()
    {
        RealType.Instance.Name.Should().Be("Real");
    }

    [Fact]
    public void Real_IsNumeric()
    {
        RealType.Instance.IsNumeric.Should().BeTrue();
    }

    [Fact]
    public void Real_IsField()
    {
        RealType.Instance.IsField.Should().BeTrue();
    }

    [Fact]
    public void Real_IsOrdered()
    {
        ScalarType s = RealType.Instance;
        s.IsOrdered.Should().BeTrue();
    }

    [Fact]
    public void Real_Supertype_IsComplex()
    {
        ScalarType s = RealType.Instance;
        s.Supertype.Should().Be(ComplexType.Instance);
    }

    [Fact]
    public void Real_IsNotAlgebraicallyClosed()
    {
        ScalarType s = RealType.Instance;
        s.IsAlgebraicallyClosed.Should().BeFalse();
    }

    [Fact]
    public void Real_Equals()
    {
        RealType.Instance.Equals(RealType.Instance).Should().BeTrue();
    }

    [Fact]
    public void Real_NotEquals_Rational()
    {
        RealType.Instance.Equals(RationalType.Instance).Should().BeFalse();
    }

    [Fact]
    public void Real_GetHashCode()
    {
        RealType.Instance.GetHashCode().Should().Be(RealType.Instance.GetHashCode());
    }

    [Fact]
    public void Complex_Singleton()
    {
        ComplexType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Complex_Kind()
    {
        ComplexType.Instance.Kind.Should().Be(TypeKind.Complex);
    }

    [Fact]
    public void Complex_Name()
    {
        ComplexType.Instance.Name.Should().Be("Complex");
    }

    [Fact]
    public void Complex_IsNumeric()
    {
        ComplexType.Instance.IsNumeric.Should().BeTrue();
    }

    [Fact]
    public void Complex_IsField()
    {
        ComplexType.Instance.IsField.Should().BeTrue();
    }

    [Fact]
    public void Complex_IsNotOrdered()
    {
        ScalarType s = ComplexType.Instance;
        s.IsOrdered.Should().BeFalse();
    }

    [Fact]
    public void Complex_IsAlgebraicallyClosed()
    {
        ScalarType s = ComplexType.Instance;
        s.IsAlgebraicallyClosed.Should().BeTrue();
    }

    [Fact]
    public void Complex_Supertype_IsNull()
    {
        ScalarType s = ComplexType.Instance;
        s.Supertype.Should().BeNull();
    }

    [Fact]
    public void Complex_Equals()
    {
        ComplexType.Instance.Equals(ComplexType.Instance).Should().BeTrue();
    }

    [Fact]
    public void Complex_NotEquals_Real()
    {
        ComplexType.Instance.Equals(RealType.Instance).Should().BeFalse();
    }

    [Fact]
    public void Complex_GetHashCode()
    {
        ComplexType.Instance.GetHashCode().Should().Be(ComplexType.Instance.GetHashCode());
    }

    [Fact]
    public void Boolean_Singleton()
    {
        BooleanType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Boolean_Kind()
    {
        BooleanType.Instance.Kind.Should().Be(TypeKind.Boolean);
    }

    [Fact]
    public void Boolean_Name()
    {
        BooleanType.Instance.Name.Should().Be("Boolean");
    }

    [Fact]
    public void Boolean_IsNotNumeric()
    {
        BooleanType.Instance.IsNumeric.Should().BeFalse();
    }

    [Fact]
    public void Boolean_Equals()
    {
        BooleanType.Instance.Equals(BooleanType.Instance).Should().BeTrue();
    }

    [Fact]
    public void Boolean_NotEquals_Integer()
    {
        BooleanType.Instance.Equals(IntegerType.Instance).Should().BeFalse();
    }

    [Fact]
    public void Boolean_GetHashCode()
    {
        BooleanType.Instance.GetHashCode().Should().Be(BooleanType.Instance.GetHashCode());
    }

    [Fact]
    public void String_Singleton()
    {
        StringType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void String_Kind()
    {
        StringType.Instance.Kind.Should().Be(TypeKind.String);
    }

    [Fact]
    public void String_Name()
    {
        StringType.Instance.Name.Should().Be("String");
    }

    [Fact]
    public void String_IsNotNumeric()
    {
        StringType.Instance.IsNumeric.Should().BeFalse();
    }

    [Fact]
    public void String_Equals()
    {
        StringType.Instance.Equals(StringType.Instance).Should().BeTrue();
    }

    [Fact]
    public void String_NotEquals_Boolean()
    {
        StringType.Instance.Equals(BooleanType.Instance).Should().BeFalse();
    }

    [Fact]
    public void String_GetHashCode()
    {
        StringType.Instance.GetHashCode().Should().Be(StringType.Instance.GetHashCode());
    }

    [Fact]
    public void Unit_Singleton()
    {
        UnitType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Unit_Kind()
    {
        UnitType.Instance.Kind.Should().Be(TypeKind.Unit);
    }

    [Fact]
    public void Unit_Name()
    {
        UnitType.Instance.Name.Should().Be("Unit");
    }

    [Fact]
    public void Unit_Equals()
    {
        UnitType.Instance.Equals(UnitType.Instance).Should().BeTrue();
    }

    [Fact]
    public void Unit_GetHashCode()
    {
        UnitType.Instance.GetHashCode().Should().Be(UnitType.Instance.GetHashCode());
    }

    [Fact]
    public void Unknown_Singleton()
    {
        UnknownType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Unknown_Kind()
    {
        UnknownType.Instance.Kind.Should().Be(TypeKind.Unknown);
    }

    [Fact]
    public void Unknown_Name()
    {
        UnknownType.Instance.Name.Should().Be("?");
    }

    [Fact]
    public void Unknown_IsUnknown()
    {
        UnknownType.Instance.IsUnknown.Should().BeTrue();
    }

    [Fact]
    public void Unknown_Equals()
    {
        UnknownType.Instance.Equals(UnknownType.Instance).Should().BeTrue();
    }

    [Fact]
    public void Unknown_GetHashCode()
    {
        UnknownType.Instance.GetHashCode().Should().Be(UnknownType.Instance.GetHashCode());
    }

    [Fact]
    public void Error_Singleton()
    {
        ErrorType.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Error_Kind()
    {
        ErrorType.Instance.Kind.Should().Be(TypeKind.Error);
    }

    [Fact]
    public void Error_Name()
    {
        ErrorType.Instance.Name.Should().Be("⊥");
    }

    [Fact]
    public void Error_IsError()
    {
        ErrorType.Instance.IsError.Should().BeTrue();
    }

    [Fact]
    public void Error_Equals()
    {
        ErrorType.Instance.Equals(ErrorType.Instance).Should().BeTrue();
    }

    [Fact]
    public void Error_GetHashCode()
    {
        ErrorType.Instance.GetHashCode().Should().Be(ErrorType.Instance.GetHashCode());
    }

    [Fact]
    public void MathType_ImplicitConversion_Int()
    {
        MathType t = 42;
        t.Should().BeOfType<TypedInteger>();
        ((TypedInteger)t).Value.Should().Be(42);
    }

    [Fact]
    public void MathType_ImplicitConversion_Double()
    {
        MathType t = 3.14;
        t.Should().BeOfType<RealType>();
    }

    [Fact]
    public void MathType_ToString_IsName()
    {
        IntegerType.Instance.ToString().Should().Be("Integer");
    }
}
