namespace MathVerse.Foundation.Tests;

public sealed class QuantityTests
{
    private static Unit Meter => new() { Symbol = "m", Name = "Meter", Dimension = Dimension.FromBaseDimensions(length: 1), Category = UnitCategory.Length, ScaleFactor = 1.0 };
    private static Unit Kilogram => new() { Symbol = "kg", Name = "Kilogram", Dimension = Dimension.FromBaseDimensions(mass: 1), Category = UnitCategory.Mass, ScaleFactor = 1.0 };
    private static Unit Second => new() { Symbol = "s", Name = "Second", Dimension = Dimension.FromBaseDimensions(time: 1), Category = UnitCategory.Time, ScaleFactor = 1.0 };
    private static Unit Centimeter => new() { Symbol = "cm", Name = "Centimeter", Dimension = Dimension.FromBaseDimensions(length: 1), Category = UnitCategory.Length, ScaleFactor = 0.01 };
    private static Unit Kilometer => new() { Symbol = "km", Name = "Kilometer", Dimension = Dimension.FromBaseDimensions(length: 1), Category = UnitCategory.Length, ScaleFactor = 1000.0 };
    private static Unit Newton => new() { Symbol = "N", Name = "Newton", Dimension = DerivedDimension.Force, Category = UnitCategory.Force, ScaleFactor = 1.0 };

    // ── PhysicalQuantity ──────────────────────────────────────────

    [Fact]
    public void PhysicalQuantity_Create_WithValues()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        q.Value.Should().Be(5.0);
        q.Unit.Should().Be(Meter);
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_MetersToKilometers()
    {
        var q = new PhysicalQuantity { Value = 1000.0, Unit = Meter, Dimension = Meter.Dimension };
        var converted = q.ConvertTo(Kilometer);
        converted.Value.Should().Be(1.0);
        converted.Unit.Should().Be(Kilometer);
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_KilometersToMeters()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var converted = q.ConvertTo(Meter);
        converted.Value.Should().Be(5000.0);
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_MetersToCentimeters()
    {
        var q = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var converted = q.ConvertTo(Centimeter);
        converted.Value.Should().Be(100.0);
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_Incompatible_Throws()
    {
        var length = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var mass = new PhysicalQuantity { Value = 2.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        Action act = () => length.ConvertTo(Kilogram);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_Null_Throws()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        Action act = () => q.ConvertTo(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PhysicalQuantity_ToBase_Meter()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter };
        var b = q.ToBase();
        b.Value.Should().Be(5.0);
    }

    [Fact]
    public void PhysicalQuantity_ToBase_Centimeter()
    {
        var q = new PhysicalQuantity { Value = 100.0, Unit = Centimeter };
        var b = q.ToBase();
        b.Value.Should().Be(1.0);
    }

    [Fact]
    public void PhysicalQuantity_ToBase_Kilometer()
    {
        var q = new PhysicalQuantity { Value = 3.0, Unit = Kilometer };
        var b = q.ToBase();
        b.Value.Should().Be(3000.0);
    }

    [Fact]
    public void PhysicalQuantity_IsDimensionallyCompatible_WithUnit()
    {
        var q = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        q.IsDimensionallyCompatible(Kilometer).Should().BeTrue();
    }

    [Fact]
    public void PhysicalQuantity_IsDimensionallyCompatible_NullQuantity_Throws()
    {
        var q = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        Action act = () => q.IsDimensionallyCompatible((PhysicalQuantity)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PhysicalQuantity_IsDimensionallyCompatible_NullUnit_Throws()
    {
        var q = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        Action act = () => q.IsDimensionallyCompatible((Unit)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PhysicalQuantity_Add_SameDimension()
    {
        var a = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = a + b;
        result.Value.Should().Be(5.0);
    }

    [Fact]
    public void PhysicalQuantity_Add_DifferentScale()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var b = new PhysicalQuantity { Value = 500.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = a + b;
        result.Value.Should().Be(1.5);
        result.Unit.Should().Be(Kilometer);
    }

    [Fact]
    public void PhysicalQuantity_Add_Incompatible_Throws()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        Action act = () => { _ = a + b; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PhysicalQuantity_Subtract_SameDimension()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = a - b;
        result.Value.Should().Be(3.0);
    }

    [Fact]
    public void PhysicalQuantity_Subtract_Incompatible_Throws()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Second, Dimension = Second.Dimension };
        Action act = () => { _ = a - b; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PhysicalQuantity_Multiply()
    {
        var a = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = a * b;
        result.Value.Should().Be(6.0);
    }

    [Fact]
    public void PhysicalQuantity_Divide()
    {
        var a = new PhysicalQuantity { Value = 6.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = a / b;
        result.Value.Should().Be(3.0);
    }

    [Fact]
    public void PhysicalQuantity_ScaleByScalar()
    {
        var q = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = q * 2.0;
        result.Value.Should().Be(6.0);
    }

    [Fact]
    public void PhysicalQuantity_ScalarTimesQuantity()
    {
        var q = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = 2.0 * q;
        result.Value.Should().Be(6.0);
    }

    [Fact]
    public void PhysicalQuantity_DivideByScalar()
    {
        var q = new PhysicalQuantity { Value = 6.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = q / 2.0;
        result.Value.Should().Be(3.0);
    }

    [Fact]
    public void PhysicalQuantity_Negate()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = -q;
        result.Value.Should().Be(-5.0);
    }

    [Fact]
    public void PhysicalQuantity_CompareTo_SameUnit()
    {
        var a = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        a.CompareTo(b).Should().BeNegative();
    }

    [Fact]
    public void PhysicalQuantity_CompareTo_Equal()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        a.CompareTo(b).Should().Be(0);
    }

    [Fact]
    public void PhysicalQuantity_CompareTo_Null_ReturnsPositive()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        a.CompareTo(null).Should().Be(1);
    }

    [Fact]
    public void PhysicalQuantity_CompareTo_Incompatible_Throws()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        Action act = () => a.CompareTo(b);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PhysicalQuantity_ToString_FormatsCorrectly()
    {
        var q = new PhysicalQuantity { Value = 42.0, Unit = Meter, Dimension = Meter.Dimension };
        q.ToString().Should().Be("42 m");
    }

    [Fact]
    public void PhysicalQuantity_Equality_SameValues()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        a.Should().Be(b);
    }

    // ── QuantityFactory ───────────────────────────────────────────

    [Fact]
    public void QuantityFactory_Instance_IsNotNull()
    {
        QuantityFactory.Instance.Should().NotBeNull();
    }

    [Fact]
    public void QuantityFactory_Instance_IsSingleton()
    {
        QuantityFactory.Instance.Should().BeSameAs(QuantityFactory.Instance);
    }

    [Fact]
    public void QuantityFactory_Create_NullUnit_Throws()
    {
        Action act = () => QuantityFactory.Instance.Create(1.0, (Unit)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void QuantityFactory_Create_WithSymbol()
    {
        var q = QuantityFactory.Instance.Create(9.8, "m");
        q.Value.Should().Be(9.8);
        q.Unit!.Symbol.Should().Be("m");
    }

    [Fact]
    public void QuantityFactory_Create_WithUnknownSymbol_Throws()
    {
        Action act = () => QuantityFactory.Instance.Create(1.0, "nonexistent");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void QuantityFactory_Zero_ReturnsZero()
    {
        var q = QuantityFactory.Instance.Zero(Meter);
        q.Value.Should().Be(0.0);
    }

    [Fact]
    public void QuantityFactory_One_ReturnsOne()
    {
        var q = QuantityFactory.Instance.One(Meter);
        q.Value.Should().Be(1.0);
    }

    [Fact]
    public void QuantityFactory_Zero_WithSymbol()
    {
        var q = QuantityFactory.Instance.Zero("m");
        q.Value.Should().Be(0.0);
        q.Unit!.Symbol.Should().Be("m");
    }

    [Fact]
    public void QuantityFactory_One_WithSymbol()
    {
        var q = QuantityFactory.Instance.One("m");
        q.Value.Should().Be(1.0);
        q.Unit!.Symbol.Should().Be("m");
    }

    // ── QuantityFormatter ─────────────────────────────────────────

    [Fact]
    public void QuantityFormatter_Instance_IsNotNull()
    {
        QuantityFormatter.Instance.Should().NotBeNull();
    }

    [Fact]
    public void QuantityFormatter_Instance_IsSingleton()
    {
        QuantityFormatter.Instance.Should().BeSameAs(QuantityFormatter.Instance);
    }

    [Fact]
    public void QuantityFormatter_Format()
    {
        var q = new PhysicalQuantity { Value = 42.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityFormatter.Instance.Format(q).Should().Be("42 m");
    }

    [Fact]
    public void QuantityFormatter_Format_Null_Throws()
    {
        Action act = () => QuantityFormatter.Instance.Format(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void QuantityFormatter_FormatWithPrecision_Null_Throws()
    {
        Action act = () => QuantityFormatter.Instance.FormatWithPrecision(null!, 2);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void QuantityFormatter_FormatScientific_Null_Throws()
    {
        Action act = () => QuantityFormatter.Instance.FormatScientific(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── QuantityParser ────────────────────────────────────────────

    [Fact]
    public void QuantityParser_Instance_IsNotNull()
    {
        QuantityParser.Instance.Should().NotBeNull();
    }

    [Fact]
    public void QuantityParser_Instance_IsSingleton()
    {
        QuantityParser.Instance.Should().BeSameAs(QuantityParser.Instance);
    }

    [Fact]
    public void QuantityParser_Parse_ValidQuantity()
    {
        var q = QuantityParser.Instance.Parse("5 m");
        q.Value.Should().Be(5.0);
        q.Unit!.Symbol.Should().Be("m");
    }

    [Fact]
    public void QuantityParser_Parse_NullInput_Throws()
    {
        Action act = () => QuantityParser.Instance.Parse(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void QuantityParser_Parse_InvalidInput_Throws()
    {
        Action act = () => QuantityParser.Instance.Parse("invalid");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void QuantityParser_Parse_ValueOnly()
    {
        var q = QuantityParser.Instance.Parse("42");
        q.Value.Should().Be(42.0);
    }

    [Fact]
    public void QuantityParser_TryParse_Valid()
    {
        var result = QuantityParser.Instance.TryParse("5 m", out var q);
        result.Should().BeTrue();
        q!.Value.Should().Be(5.0);
        q.Unit!.Symbol.Should().Be("m");
    }

    [Fact]
    public void QuantityParser_TryParse_Invalid_ReturnsFalse()
    {
        var result = QuantityParser.Instance.TryParse("invalid", out var q);
        result.Should().BeFalse();
        q.Should().BeNull();
    }

    [Fact]
    public void QuantityParser_TryParse_Empty_ReturnsFalse()
    {
        var result = QuantityParser.Instance.TryParse("", out var q);
        result.Should().BeFalse();
    }

    [Fact]
    public void QuantityParser_TryParse_Whitespace_ReturnsFalse()
    {
        var result = QuantityParser.Instance.TryParse("   ", out var q);
        result.Should().BeFalse();
    }

    [Fact]
    public void QuantityParser_TryParse_Null_ReturnsFalse()
    {
        var result = QuantityParser.Instance.TryParse(null!, out var q);
        result.Should().BeFalse();
    }

    // ── QuantityComparer ──────────────────────────────────────────

    [Fact]
    public void QuantityComparer_Instance_IsNotNull()
    {
        QuantityComparer.Instance.Should().NotBeNull();
    }

    [Fact]
    public void QuantityComparer_Instance_IsSingleton()
    {
        QuantityComparer.Instance.Should().BeSameAs(QuantityComparer.Instance);
    }

    [Fact]
    public void QuantityComparer_Compare_LessThan()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityComparer.Instance.Compare(a, b).Should().BeNegative();
    }

    [Fact]
    public void QuantityComparer_Compare_Equal()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityComparer.Instance.Compare(a, b).Should().Be(0);
    }

    [Fact]
    public void QuantityComparer_Compare_GreaterThan()
    {
        var a = new PhysicalQuantity { Value = 10.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityComparer.Instance.Compare(a, b).Should().BePositive();
    }

    [Fact]
    public void QuantityComparer_Compare_NullX_ReturnsNegative()
    {
        var b = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityComparer.Instance.Compare(null, b).Should().Be(-1);
    }

    [Fact]
    public void QuantityComparer_Compare_NullY_ReturnsPositive()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityComparer.Instance.Compare(a, null).Should().Be(1);
    }

    [Fact]
    public void QuantityComparer_Compare_BothNull_ReturnsZero()
    {
        QuantityComparer.Instance.Compare(null, null).Should().Be(0);
    }

    [Fact]
    public void QuantityComparer_Equals_SameBaseValue()
    {
        var a = new PhysicalQuantity { Value = 100.0, Unit = Centimeter, Dimension = Centimeter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityComparer.Instance.Equals(a, b).Should().BeTrue();
    }

[Fact]
    public void QuantityComparer_Equals_DifferentDimensions_LengthVsMass()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        QuantityComparer.Instance.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void QuantityComparer_Equals_DifferentDimensions()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        QuantityComparer.Instance.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void QuantityComparer_Equals_BothNull()
    {
        QuantityComparer.Instance.Equals(null, null).Should().BeTrue();
    }

    [Fact]
    public void QuantityComparer_Equals_OneNull()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityComparer.Instance.Equals(a, null).Should().BeFalse();
        QuantityComparer.Instance.Equals(null, a).Should().BeFalse();
    }

    [Fact]
    public void QuantityComparer_GetHashCode_Null_ReturnsZero()
    {
        QuantityComparer.Instance.GetHashCode(null).Should().Be(0);
    }

    // ── QuantityOperations ────────────────────────────────────────

    [Fact]
    public void QuantityOperations_Add()
    {
        var a = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Add(a, b).Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Subtract()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Subtract(a, b).Value.Should().Be(3.0);
    }

    [Fact]
    public void QuantityOperations_Multiply()
    {
        var a = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Multiply(a, b).Value.Should().Be(6.0);
    }

    [Fact]
    public void QuantityOperations_Divide()
    {
        var a = new PhysicalQuantity { Value = 6.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Divide(a, b).Value.Should().Be(3.0);
    }

    [Fact]
    public void QuantityOperations_Scale()
    {
        var q = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Scale(q, 2.0).Value.Should().Be(6.0);
    }

    [Fact]
    public void QuantityOperations_Negate()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Negate(q).Value.Should().Be(-5.0);
    }

    [Fact]
    public void QuantityOperations_Abs()
    {
        var q = new PhysicalQuantity { Value = -5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Abs(q).Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Abs_AlreadyPositive()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Abs(q).Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Pow()
    {
        var q = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityOperations.Pow(q, 2.0);
        result.Value.Should().Be(9.0);
    }

    [Fact]
    public void QuantityOperations_Sqrt()
    {
        var q = new PhysicalQuantity { Value = 9.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityOperations.Sqrt(q);
        result.Value.Should().Be(3.0);
    }

    [Fact]
    public void QuantityOperations_Max()
    {
        var a = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Max(a, b).Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Min()
    {
        var a = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Min(a, b).Value.Should().Be(3.0);
    }

    [Fact]
    public void QuantityOperations_Max_Equal()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Max(a, b).Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Min_Equal()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Min(a, b).Value.Should().Be(5.0);
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_SameUnit()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var converted = q.ConvertTo(Meter);
        converted.Value.Should().Be(5.0);
        converted.Unit.Should().Be(Meter);
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_ChainedConversions()
    {
        var q = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var cm = new Unit { Symbol = "cm", Name = "Centimeter", Dimension = Dimension.FromBaseDimensions(length: 1), Category = UnitCategory.Length, ScaleFactor = 0.01 };
        var converted = q.ConvertTo(cm);
        converted.Value.Should().Be(100000.0);
    }

    [Fact]
    public void PhysicalQuantity_ToBase_AlreadyBaseUnit()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var baseQ = q.ToBase();
        baseQ.Value.Should().Be(5.0);
        baseQ.Unit.ScaleFactor.Should().Be(1.0);
    }

    [Fact]
    public void PhysicalQuantity_IsDimensionallyCompatible_Self()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        q.IsDimensionallyCompatible(q).Should().BeTrue();
    }

    [Fact]
    public void PhysicalQuantity_IsDimensionallyCompatible_NullQuantity()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        Action act = () => q.IsDimensionallyCompatible((PhysicalQuantity)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PhysicalQuantity_IsDimensionallyCompatible_NullUnit()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        Action act = () => q.IsDimensionallyCompatible((Unit)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PhysicalQuantity_Add_MultipleDifferentUnits()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var b = new PhysicalQuantity { Value = 500.0, Unit = Meter, Dimension = Meter.Dimension };
        var c = new PhysicalQuantity { Value = 20000.0, Unit = Centimeter, Dimension = Centimeter.Dimension };
        var result = a + b + c;
        result.Value.Should().BeApproximately(1.5 + 0.2, 1e-10);
        result.Unit.Should().Be(Kilometer);
    }

    [Fact]
    public void PhysicalQuantity_Subtract_DifferentUnits()
    {
        var a = new PhysicalQuantity { Value = 2.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var b = new PhysicalQuantity { Value = 500.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = a - b;
        result.Value.Should().Be(1.5);
    }

    [Fact]
    public void PhysicalQuantity_ScaleByScalar_Zero()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = q * 0.0;
        result.Value.Should().Be(0.0);
    }

    [Fact]
    public void PhysicalQuantity_ScaleByScalar_Negative()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = q * -2.0;
        result.Value.Should().Be(-10.0);
    }

    [Fact]
    public void PhysicalQuantity_ScalarTimesQuantity_Zero()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = 0.0 * q;
        result.Value.Should().Be(0.0);
    }

    [Fact]
    public void PhysicalQuantity_DivideByScalar_Zero_Throws()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        Action act = () => q / 0.0;
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void PhysicalQuantity_Negate_Zero()
    {
        var q = new PhysicalQuantity { Value = 0.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = -q;
        result.Value.Should().Be(0.0);
    }

    [Fact]
    public void PhysicalQuantity_CompareTo_SameValueDifferentUnits()
    {
        var a = new PhysicalQuantity { Value = 1000.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        a.CompareTo(b).Should().Be(0);
    }

    [Fact]
    public void PhysicalQuantity_CompareTo_Incompatible_Throws_Added()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        Action act = () => a.CompareTo(b);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PhysicalQuantity_ToString_WithUnit()
    {
        var q = new PhysicalQuantity { Value = 42.0, Unit = Meter, Dimension = Meter.Dimension };
        q.ToString().Should().Be("42 m");
    }

    [Fact]
    public void PhysicalQuantity_ToString_NoUnit()
    {
        var q = new PhysicalQuantity { Value = 42.0, Dimension = Dimension.None };
        q.ToString().Should().Be("42 ");
    }

    [Fact]
    public void PhysicalQuantity_Equality_DifferentUnitsSameValue()
    {
        var a = new PhysicalQuantity { Value = 1000.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        a.Should().NotBe(b);
    }

    [Fact]
    public void PhysicalQuantity_Equality_Null()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        q.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void PhysicalQuantity_GetHashCode_SameValues()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void QuantityFactory_CreateWithDimension()
    {
        var q = QuantityFactory.Instance.FromValue(5.0, Dimension.FromBaseDimensions(length: 1));
        q.Value.Should().Be(5.0);
        q.Dimension.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void QuantityFactory_ZeroWithDimension()
    {
        var q = QuantityFactory.Instance.Zero("m");
        q.Value.Should().Be(0.0);
        q.Unit.Should().NotBeNull();
    }

    [Fact]
    public void QuantityFactory_OneWithDimension()
    {
        var q = QuantityFactory.Instance.One("kg");
        q.Value.Should().Be(1.0);
        q.Unit.Should().NotBeNull();
    }

    [Fact]
    public void QuantityFormatter_FormatWithPrecision_VariousPrecisions()
    {
        var q = new PhysicalQuantity { Value = Math.PI, Unit = Meter, Dimension = Meter.Dimension };
        QuantityFormatter.Instance.FormatWithPrecision(q, 0).Should().Be("3 m");
        QuantityFormatter.Instance.FormatWithPrecision(q, 5).Should().Be("3.14159 m");
    }

    [Fact]
    public void QuantityFormatter_FormatScientific_VariousValues()
    {
        var q = new PhysicalQuantity { Value = 0.00012345, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityFormatter.Instance.FormatScientific(q, 3);
        result.Should().MatchRegex(@"1\.234E[+-]004 m");
    }

    [Fact]
    public void QuantityFormatter_FormatScientific_LargeValue()
    {
        var q = new PhysicalQuantity { Value = 123456789.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityFormatter.Instance.FormatScientific(q, 4);
        result.Should().MatchRegex(@"1\.2345E\+008 m");
    }

    [Fact]
    public void QuantityParser_Parse_ValueOnly_Added()
    {
        var q = QuantityParser.Instance.Parse("42");
        q.Value.Should().Be(42.0);
        q.Dimension.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void QuantityParser_Parse_NegativeValue()
    {
        var q = QuantityParser.Instance.Parse("-5 m");
        q.Value.Should().Be(-5.0);
    }

    [Fact]
    public void QuantityParser_Parse_DecimalValue()
    {
        var q = QuantityParser.Instance.Parse("3.14159 m");
        q.Value.Should().Be(3.14159);
    }

    [Fact]
    public void QuantityParser_Parse_ScientificNotation()
    {
        var q = QuantityParser.Instance.Parse("1.5e3 m");
        q.Value.Should().Be(1500.0);
    }

    [Fact]
    public void QuantityParser_TryParse_VariousFormats()
    {
        QuantityParser.Instance.TryParse("5 m", out var q1).Should().BeTrue();
        q1!.Value.Should().Be(5.0);

        QuantityParser.Instance.TryParse("10", out var q2).Should().BeTrue();
        q2!.Value.Should().Be(10.0);

        QuantityParser.Instance.TryParse("-3.5 kg", out var q3).Should().BeTrue();
        q3!.Value.Should().Be(-3.5);
    }

    [Fact]
    public void QuantityParser_TryParse_InvalidFormats()
    {
        QuantityParser.Instance.TryParse("abc", out _).Should().BeFalse();
        QuantityParser.Instance.TryParse("5 unknownunit", out _).Should().BeFalse();
        QuantityParser.Instance.TryParse("", out _).Should().BeFalse();
        QuantityParser.Instance.TryParse("   ", out _).Should().BeFalse();
        QuantityParser.Instance.TryParse(null!, out _).Should().BeFalse();
    }

    [Fact]
    public void QuantityComparer_Compare_DifferentUnits()
    {
        var a = new PhysicalQuantity { Value = 1000.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        QuantityComparer.Instance.Compare(a, b).Should().Be(0);
    }

    [Fact]
    public void QuantityComparer_Equals_DifferentUnitsSameValue()
    {
        var a = new PhysicalQuantity { Value = 1000.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        QuantityComparer.Instance.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void QuantityComparer_GetHashCode_DifferentUnitsSameBaseValue()
    {
        var a = new PhysicalQuantity { Value = 1000.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        QuantityComparer.Instance.GetHashCode(a).Should().Be(QuantityComparer.Instance.GetHashCode(b));
    }

    [Fact]
    public void QuantityOperations_Add_CompatibleUnits()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var b = new PhysicalQuantity { Value = 500.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityOperations.Add(a, b);
        result.Value.Should().Be(1.5);
        result.Unit.Should().Be(Kilometer);
    }

    [Fact]
    public void QuantityOperations_Subtract_CompatibleUnits()
    {
        var a = new PhysicalQuantity { Value = 2.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var b = new PhysicalQuantity { Value = 1500.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityOperations.Subtract(a, b);
        result.Value.Should().Be(0.5);
    }

    [Fact]
    public void QuantityOperations_Multiply_DifferentDimensions()
    {
        var a = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Kilogram, Dimension = Kilogram.Dimension };
        var result = QuantityOperations.Multiply(a, b);
        result.Value.Should().Be(6.0);
        result.Dimension.Exponents["L"].Should().Be(1);
        result.Dimension.Exponents["M"].Should().Be(1);
    }

    [Fact]
    public void QuantityOperations_Divide_DifferentDimensions()
    {
        var a = new PhysicalQuantity { Value = 10.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Second, Dimension = Second.Dimension };
        var result = QuantityOperations.Divide(a, b);
        result.Value.Should().Be(5.0);
        result.Dimension.Exponents["L"].Should().Be(1);
        result.Dimension.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void QuantityOperations_Scale_Zero()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Scale(q, 0.0).Value.Should().Be(0.0);
    }

    [Fact]
    public void QuantityOperations_Scale_Negative()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Scale(q, -2.0).Value.Should().Be(-10.0);
    }

    [Fact]
    public void QuantityOperations_Negate_Zero()
    {
        var q = new PhysicalQuantity { Value = 0.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Negate(q).Value.Should().Be(0.0);
    }

    [Fact]
    public void QuantityOperations_Abs_Negative()
    {
        var q = new PhysicalQuantity { Value = -5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Abs(q).Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Abs_Positive()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Abs(q).Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Pow_IntegerExponent()
    {
        var q = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityOperations.Pow(q, 3);
        result.Value.Should().Be(27.0);
        result.Dimension.Exponents["L"].Should().Be(3);
    }

    [Fact]
    public void QuantityOperations_Pow_FractionalExponent()
    {
        var q = new PhysicalQuantity { Value = 16.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityOperations.Pow(q, 0.5);
        result.Value.Should().Be(4.0);
        result.Dimension.Exponents["L"].Should().Be(0.5);
    }

    [Fact]
    public void QuantityOperations_Sqrt_PerfectSquare()
    {
        var q = new PhysicalQuantity { Value = 25.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Sqrt(q).Value.Should().Be(5.0);
        QuantityOperations.Sqrt(q).Dimension.Exponents["L"].Should().Be(0.5);
    }

    [Fact]
    public void QuantityOperations_Max_DifferentUnits()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var b = new PhysicalQuantity { Value = 500.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Max(a, b).Value.Should().Be(1.0);
    }

    [Fact]
    public void QuantityOperations_Min_DifferentUnits()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Kilometer, Dimension = Kilometer.Dimension };
        var b = new PhysicalQuantity { Value = 500.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Min(a, b).Value.Should().Be(0.5);
    }

    [Fact]
    public void PhysicalQuantity_Equality_SameValueAndUnit()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void PhysicalQuantity_GetHashCode_SameValueSameHash()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void PhysicalQuantity_ToString_Format()
    {
        var q = new PhysicalQuantity { Value = 1.5, Unit = Meter, Dimension = Meter.Dimension };
        q.ToString().Should().Contain("1.5");
        q.ToString().Should().Contain("m");
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_SameUnit_ReturnsSame()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = q.ConvertTo(Meter);
        result.Should().Be(q);
    }

    [Fact]
    public void QuantityFactory_FromBase_CreatesFromBaseValue()
    {
        var q = QuantityFactory.FromBase(1000.0, Meter);
        q.Value.Should().Be(1000.0);
    }

    [Fact]
    public void QuantityFactory_FromValue_ParsesSymbol()
    {
        var q = QuantityFactory.FromValue(10.0, "m");
        q.Value.Should().Be(10.0);
        q.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void QuantityFactory_Create_WithNullUnit_Throws()
    {
        Action act = () => QuantityFactory.Create(1.0, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void QuantityParser_TryParse_ValidString()
    {
        QuantityParser.TryParse("5.0 m", out var q).Should().BeTrue();
        q!.Value.Should().Be(5.0);
        q.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void QuantityParser_TryParse_InvalidString()
    {
        QuantityParser.TryParse("invalid", out _).Should().BeFalse();
    }

    [Fact]
    public void QuantityParser_TryParse_WithPrefix()
    {
        QuantityParser.TryParse("2.5 km", out var q).Should().BeTrue();
        q!.Value.Should().Be(2.5);
        q.Unit.Symbol.Should().Be("km");
    }

    [Fact]
    public void QuantityParser_TryParse_CompoundUnit()
    {
        QuantityParser.TryParse("10 m/s", out var q).Should().BeTrue();
        q!.Unit.Symbol.Should().Be("m/s");
    }

    [Fact]
    public void QuantityParser_Parse_ThrowsOnInvalid()
    {
        Action act = () => QuantityParser.Parse("invalid");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void QuantityComparer_Equals_Tolerance()
    {
        var a = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 1.0000001, Unit = Meter, Dimension = Meter.Dimension };
        QuantityComparer.Instance.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void QuantityOperations_Abs_NegativeValue()
    {
        var q = new PhysicalQuantity { Value = -5.0, Unit = Meter, Dimension = Meter.Dimension };
        QuantityOperations.Abs(q).Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Pow_ZeroExponent()
    {
        var q = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityOperations.Pow(q, 0);
        result.Value.Should().Be(1.0);
        result.Dimension.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void QuantityOperations_Pow_NegativeExponent()
    {
        var q = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        var result = QuantityOperations.Pow(q, -1);
        result.Value.Should().Be(0.5);
        result.Dimension.Exponents["L"].Should().Be(-1);
    }

    [Fact]
    public void PhysicalQuantity_Multiply_ChangesDimension()
    {
        var a = new PhysicalQuantity { Value = 2.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 3.0, Unit = Second, Dimension = Second.Dimension };
        var result = a * b;
        result.Value.Should().Be(6.0);
        result.Dimension.Exponents["L"].Should().Be(1);
        result.Dimension.Exponents["T"].Should().Be(1);
    }

    [Fact]
    public void PhysicalQuantity_Divide_ChangesDimension()
    {
        var a = new PhysicalQuantity { Value = 10.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = Second, Dimension = Second.Dimension };
        var result = a / b;
        result.Value.Should().Be(5.0);
        result.Dimension.Exponents["L"].Should().Be(1);
        result.Dimension.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void PhysicalQuantity_Comparison_EqualValues()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void PhysicalQuantity_Comparison_DifferentValues()
    {
        var a = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension };
        var b = new PhysicalQuantity { Value = 3.0, Unit = Meter, Dimension = Meter.Dimension };
        (a > b).Should().BeTrue();
        (a < b).Should().BeFalse();
    }

    [Fact]
    public void PhysicalQuantity_Serialization_RoundTrip()
    {
        var q = new PhysicalQuantity { Value = 42.0, Unit = Meter, Dimension = Meter.Dimension };
        var json = System.Text.Json.JsonSerializer.Serialize(q);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<PhysicalQuantity>(json);
        deserialized.Should().BeEquivalentTo(q);
    }

    [Fact]
    public void QuantityOperations_Concurrent_ThreadSafe()
    {
        Parallel.For(0, 100, _ => {
            var q = new PhysicalQuantity { Value = 1.0, Unit = Meter, Dimension = Meter.Dimension };
            QuantityOperations.Add(q, q).Value.Should().Be(2.0);
        });
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_KelvinFromCelsius()
    {
        var celsius = new Unit { Symbol = "\u00B0C", Name = "Celsius", Dimension = Dimension.FromBaseDimensions(temperature: 1), Category = UnitCategory.Temperature, ScaleFactor = 1.0 };
        var kelvin = new Unit { Symbol = "K", Name = "Kelvin", Dimension = Dimension.FromBaseDimensions(temperature: 1), Category = UnitCategory.Temperature, ScaleFactor = 1.0 };
        var q = new PhysicalQuantity { Value = 0.0, Unit = celsius, Dimension = celsius.Dimension };
        var converted = q.ConvertTo(kelvin);
        converted.Value.Should().BeApproximately(273.15, 0.01);
    }

    [Fact]
    public void PhysicalQuantity_ConvertTo_FahrenheitFromKelvin()
    {
        var k = new Unit { Symbol = "K", Name = "Kelvin", Dimension = Dimension.FromBaseDimensions(temperature: 1), Category = UnitCategory.Temperature, ScaleFactor = 1.0 };
        var f = new Unit { Symbol = "\u00B0F", Name = "Fahrenheit", Dimension = Dimension.FromBaseDimensions(temperature: 1), Category = UnitCategory.Temperature, ScaleFactor = 1.0 };
        var q = new PhysicalQuantity { Value = 273.15, Unit = k, Dimension = k.Dimension };
        var converted = q.ConvertTo(f);
        converted.Value.Should().BeApproximately(32.0, 0.1);
    }

    [Fact]
    public void PhysicalQuantity_Add_SameUnit()
    {
        var m = new Unit { Symbol = "m", Dimension = Dimension.FromBaseDimensions(length: 1) };
        var a = new PhysicalQuantity { Value = 1.0, Unit = m, Dimension = m.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = m, Dimension = m.Dimension };
        var sum = a + b;
        sum.Value.Should().Be(3.0);
        sum.Unit.Should().Be(m);
    }

    [Fact]
    public void PhysicalQuantity_Subtract_SameUnit()
    {
        var m = new Unit { Symbol = "m", Dimension = Dimension.FromBaseDimensions(length: 1) };
        var a = new PhysicalQuantity { Value = 5.0, Unit = m, Dimension = m.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = m, Dimension = m.Dimension };
        var diff = a - b;
        diff.Value.Should().Be(3.0);
        diff.Unit.Should().Be(m);
    }

    [Fact]
    public void PhysicalQuantity_Negation()
    {
        var m = new Unit { Symbol = "m", Dimension = Dimension.FromBaseDimensions(length: 1) };
        var a = new PhysicalQuantity { Value = 5.0, Unit = m, Dimension = m.Dimension };
        var neg = -a;
        neg.Value.Should().Be(-5.0);
        neg.Unit.Should().Be(m);
    }

    [Fact]
    public void PhysicalQuantity_CompareTo_SameDimension()
    {
        var m = new Unit { Symbol = "m", Dimension = Dimension.FromBaseDimensions(length: 1) };
        var a = new PhysicalQuantity { Value = 1.0, Unit = m, Dimension = m.Dimension };
        var b = new PhysicalQuantity { Value = 2.0, Unit = m, Dimension = m.Dimension };
        a.CompareTo(b).Should().BeLessThan(0);
    }

    [Fact]
    public void PhysicalQuantity_Equality_WithTolerance()
    {
        var m = new Unit { Symbol = "m", Dimension = Dimension.FromBaseDimensions(length: 1) };
        var a = new PhysicalQuantity { Value = 1.0, Unit = m, Dimension = m.Dimension };
        var b = new PhysicalQuantity { Value = 1.0 + 1e-10, Unit = m, Dimension = m.Dimension };
        var comparer = new QuantityComparer(1e-9);
        comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void QuantityFactory_FromValue_WithSymbol()
    {
        var q = QuantityFactory.FromValue(10.0, "m");
        q.Value.Should().Be(10.0);
        q.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void QuantityFactory_FromBase_ConvertsFromBase()
    {
        var q = QuantityFactory.FromBase(1000.0, "m");
        q.Value.Should().Be(1.0);
        q.Unit.Symbol.Should().Be("km");
    }

    [Fact]
    public void QuantityFormatter_Format_Various()
    {
        var q = new PhysicalQuantity { Value = 123.456, Unit = UnitRegistry.Instance.Get("m")! };
        QuantityFormatter.Instance.Format(q).Should().Contain("123.456");
    }

    [Fact]
    public void QuantityParser_Parse_WithUnitSymbol()
    {
        var q = QuantityParser.Instance.Parse("5.0 m");
        q.Should().NotBeNull();
        q!.Value.Should().Be(5.0);
        q.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void QuantityParser_Parse_WithCompoundUnit()
    {
        var q = QuantityParser.Instance.Parse("10 m/s");
        q.Should().NotBeNull();
        q!.Value.Should().Be(10.0);
    }

    [Fact]
    public void QuantityParser_TryParse_Success()
    {
        QuantityParser.Instance.TryParse("3.14 kg", out var q).Should().BeTrue();
        q!.Value.Should().Be(3.14);
    }

    [Fact]
    public void QuantityParser_TryParse_Failure()
    {
        QuantityParser.Instance.TryParse("invalid", out _).Should().BeFalse();
    }

    [Fact]
    public void QuantityComparer_Compare_SameDimension()
    {
        var m = UnitRegistry.Instance.Get("m")!;
        var cm = UnitRegistry.Instance.Get("cm")!;
        var a = new PhysicalQuantity { Value = 1.0, Unit = m, Dimension = m.Dimension };
        var b = new PhysicalQuantity { Value = 100.0, Unit = cm, Dimension = cm.Dimension };
        QuantityComparer.Instance.Compare(a, b).Should().Be(0);
    }

    [Fact]
    public void QuantityOperations_Abs_NegativeValue_UnitOps()
    {
        var m = UnitRegistry.Instance.Get("m")!;
        var a = new PhysicalQuantity { Value = -5.0, Unit = m, Dimension = m.Dimension };
        var abs = QuantityOperations.Abs(a);
        abs.Value.Should().Be(5.0);
    }

    [Fact]
    public void QuantityOperations_Power_Quantity()
    {
        var m = UnitRegistry.Instance.Get("m")!;
        var a = new PhysicalQuantity { Value = 2.0, Unit = m, Dimension = m.Dimension };
        var powered = QuantityOperations.Power(a, 3);
        powered.Value.Should().Be(8.0);
        powered.Dimension.Exponents["L"].Should().Be(3);
    }

    [Fact]
    public void QuantityOperations_Sqrt_Quantity()
    {
        var m2 = new Unit { Symbol = "m2", Dimension = Dimension.FromBaseDimensions(length: 2), Category = UnitCategory.Area, ScaleFactor = 1.0 };
        var a = new PhysicalQuantity { Value = 16.0, Unit = m2, Dimension = m2.Dimension };
        var sqrt = QuantityOperations.Sqrt(a);
        sqrt.Value.Should().Be(4.0);
        sqrt.Dimension.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void QuantityOperations_Min_Max()
    {
        var m = UnitRegistry.Instance.Get("m")!;
        var a = new PhysicalQuantity { Value = 5.0, Unit = m, Dimension = m.Dimension };
        var b = new PhysicalQuantity { Value = 10.0, Unit = m, Dimension = m.Dimension };
        QuantityOperations.Min(a, b).Value.Should().Be(5.0);
        QuantityOperations.Max(a, b).Value.Should().Be(10.0);
    }

    [Fact]
    public void PhysicalQuantity_DimensionProperty()
    {
        var m = UnitRegistry.Instance.Get("m")!;
        var q = new PhysicalQuantity { Value = 1.0, Unit = m };
        q.Dimension.Should().Be(m.Dimension);
    }

    [Fact]
    public void PhysicalQuantity_ValueInBase_Scaled()
    {
        var km = new Unit { Symbol = "km", Dimension = Dimension.FromBaseDimensions(length: 1), Category = UnitCategory.Length, ScaleFactor = 1000.0 };
        var q = new PhysicalQuantity { Value = 2.0, Unit = km };
        q.ValueInBase.Should().Be(2000.0);
    }

    [Fact]
    public void PhysicalQuantity_ToString_ContainsValueAndUnit()
    {
        var q = new PhysicalQuantity { Value = 42.0, Unit = UnitRegistry.Instance.Get("m")! };
        q.ToString().Should().Contain("42");
        q.ToString().Should().Contain("m");
    }

    [Fact]
    public void QuantityOperations_Subtract_ThreadSafe()
    {
        Parallel.For(0, 100, _ => {
            var m = UnitRegistry.Instance.Get("m")!;
            var a = new PhysicalQuantity { Value = 10.0, Unit = m, Dimension = m.Dimension };
            var b = new PhysicalQuantity { Value = 3.0, Unit = m, Dimension = m.Dimension };
            var diff = QuantityOperations.Subtract(a, b);
            diff.Value.Should().Be(7.0);
        });
    }
}

