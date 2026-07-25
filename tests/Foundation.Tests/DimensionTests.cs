namespace MathVerse.Foundation.Tests;

public class DimensionTests
{

    [Fact]
    public void Dimension_FromBaseDimensions_LengthOnly()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        d.IsDimensionless.Should().BeFalse();
        d.IsBaseDimension.Should().BeTrue();
    }

    [Fact]
    public void Dimension_FromBaseDimensions_Velocity()
    {
        var d = Dimension.FromBaseDimensions(length: 1, time: -1);
        d.IsBaseDimension.Should().BeFalse();
        d.Exponents.Should().HaveCount(2);
    }

    [Fact]
    public void Dimension_Multiply_CombinesExponents()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: 1);
        var result = length.Multiply(time);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(1);
    }

    [Fact]
    public void Dimension_Multiply_SameDimensionSquaresExponent()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = length.Multiply(length);
        result.Exponents["L"].Should().Be(2);
    }

    [Fact]
    public void Dimension_Multiply_OppositeDimensionsCancel()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var invLength = Dimension.FromBaseDimensions(length: -1);
        var result = length.Multiply(invLength);
        result.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void Dimension_Divide_SubtractsExponents()
    {
        var energy = Dimension.FromBaseDimensions(mass: 1, length: 2, time: -2);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        var result = energy.Divide(mass);
        result.Exponents["L"].Should().Be(2);
        result.Exponents["T"].Should().Be(-2);
        result.Exponents.Should().NotContainKey("M");
    }

    [Fact]
    public void Dimension_Divide_SameDimensionReturnsNone()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = length.Divide(length);
        result.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void Dimension_Power_ScalesExponents()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = length.Power(3);
        result.Exponents["L"].Should().Be(3);
    }

    [Fact]
    public void Dimension_Power_ZeroReturnsNone()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var result = length.Power(0);
        result.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void Dimension_Root_DividesExponent()
    {
        var area = Dimension.FromBaseDimensions(length: 2);
        var result = area.Root(2);
        result.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void Dimension_Root_ThrowsForZero()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        Action act = () => d.Root(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Dimension_IsCompatibleWith_SameDimension()
    {
        var d1 = Dimension.FromBaseDimensions(length: 1);
        var d2 = Dimension.FromBaseDimensions(length: 1);
        d1.IsCompatibleWith(d2).Should().BeTrue();
    }

    [Fact]
    public void Dimension_IsCompatibleWith_DifferentDimensions()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        length.IsCompatibleWith(mass).Should().BeFalse();
    }

    [Fact]
    public void Dimension_IsCompatibleWith_Dimensionless()
    {
        Dimension.None.IsCompatibleWith(Dimension.None).Should().BeTrue();
    }

    [Fact]
    public void Dimension_IsCompatibleWith_DimensionlessVsBase()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        Dimension.None.IsCompatibleWith(length).Should().BeFalse();
    }

    [Fact]
    public void Dimension_Equals_SameExponents()
    {
        var d1 = Dimension.FromBaseDimensions(length: 1, time: -1);
        var d2 = Dimension.FromBaseDimensions(length: 1, time: -1);
        d1.Equals(d2).Should().BeTrue();
    }

    [Fact]
    public void Dimension_Equals_DifferentExponents()
    {
        var d1 = Dimension.FromBaseDimensions(length: 1);
        var d2 = Dimension.FromBaseDimensions(mass: 1);
        d1.Equals(d2).Should().BeFalse();
    }

    [Fact]
    public void Dimension_Equals_NullReturnsFalse()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        d.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Dimension_GetHashCode_SameForEqualDimensions()
    {
        var d1 = Dimension.FromBaseDimensions(length: 1, time: -1);
        var d2 = Dimension.FromBaseDimensions(length: 1, time: -1);
        d1.GetHashCode().Should().Be(d2.GetHashCode());
    }

    [Fact]
    public void Dimension_ToString_DimensionlessReturns1()
    {
        Dimension.None.ToString().Should().Be("1");
    }

    [Fact]
    public void Dimension_ToString_SingleExponent()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        d.ToString().Should().Be("L");
    }

    [Fact]
    public void Dimension_ToString_MultipleExponents()
    {
        var d = Dimension.FromBaseDimensions(mass: 1, length: 1, time: -2);
        var s = d.ToString();
        s.Should().Contain("M");
        s.Should().Contain("L");
        s.Should().Contain("T^-2");
    }

    [Fact]
    public void BaseDimension_Length_SymbolIsL()
    {
        BaseDimension.Length.Symbol().Should().Be("L");
    }

    [Fact]
    public void BaseDimension_Mass_SymbolIsM()
    {
        BaseDimension.Mass.Symbol().Should().Be("M");
    }

    [Fact]
    public void BaseDimension_Time_SymbolIsT()
    {
        BaseDimension.Time.Symbol().Should().Be("T");
    }

    [Fact]
    public void BaseDimension_ElectricCurrent_SymbolIsI()
    {
        BaseDimension.ElectricCurrent.Symbol().Should().Be("I");
    }

    [Fact]
    public void BaseDimension_Temperature_SymbolIsK()
    {
        BaseDimension.Temperature.Symbol().Should().Be("K");
    }

    [Fact]
    public void BaseDimension_AmountOfSubstance_SymbolIsN()
    {
        BaseDimension.AmountOfSubstance.Symbol().Should().Be("N");
    }

    [Fact]
    public void BaseDimension_LuminousIntensity_SymbolIsJ()
    {
        BaseDimension.LuminousIntensity.Symbol().Should().Be("J");
    }

    [Fact]
    public void BaseDimension_Length_DisplayName()
    {
        BaseDimension.Length.DisplayName().Should().Be("Length");
    }

    [Fact]
    public void BaseDimension_ElectricCurrent_DisplayName()
    {
        BaseDimension.ElectricCurrent.DisplayName().Should().Be("Electric Current");
    }

    [Fact]
    public void BaseDimension_FromSymbol_L()
    {
        BaseDimensionExtensions.FromSymbol("L").Should().Be(BaseDimension.Length);
    }

    [Fact]
    public void BaseDimension_FromSymbol_T()
    {
        BaseDimensionExtensions.FromSymbol("T").Should().Be(BaseDimension.Time);
    }

    [Fact]
    public void BaseDimension_FromSymbol_InvalidThrows()
    {
        Action act = () => BaseDimensionExtensions.FromSymbol("X");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DerivedDimension_Velocity_HasCorrectExponents()
    {
        DerivedDimension.Velocity.Exponents["L"].Should().Be(1);
        DerivedDimension.Velocity.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DerivedDimension_Acceleration_HasCorrectExponents()
    {
        DerivedDimension.Acceleration.Exponents["L"].Should().Be(1);
        DerivedDimension.Acceleration.Exponents["T"].Should().Be(-2);
    }

    [Fact]
    public void DerivedDimension_Force_HasCorrectExponents()
    {
        DerivedDimension.Force.Exponents["M"].Should().Be(1);
        DerivedDimension.Force.Exponents["L"].Should().Be(1);
        DerivedDimension.Force.Exponents["T"].Should().Be(-2);
    }

    [Fact]
    public void DerivedDimension_Energy_HasCorrectExponents()
    {
        DerivedDimension.Energy.Exponents["M"].Should().Be(1);
        DerivedDimension.Energy.Exponents["L"].Should().Be(2);
        DerivedDimension.Energy.Exponents["T"].Should().Be(-2);
    }

    [Fact]
    public void DerivedDimension_Power_IsEnergyDividedByTime()
    {
        var computed = DerivedDimension.Energy.Divide(DerivedDimension.Force);
        DerivedDimension.Power.Exponents["L"].Should().Be(2);
        DerivedDimension.Power.Exponents["M"].Should().Be(1);
        DerivedDimension.Power.Exponents["T"].Should().Be(-3);
    }

    [Fact]
    public void DerivedDimension_Frequency_IsTimeInverse()
    {
        DerivedDimension.Frequency.Exponents["T"].Should().Be(-1);
        DerivedDimension.Frequency.Exponents.Should().HaveCount(1);
    }

    [Fact]
    public void DerivedDimension_Area_IsLengthSquared()
    {
        DerivedDimension.Area.Exponents["L"].Should().Be(2);
    }

    [Fact]
    public void DerivedDimension_Volume_IsLengthCubed()
    {
        DerivedDimension.Volume.Exponents["L"].Should().Be(3);
    }

    [Fact]
    public void DerivedDimension_Density_IsMassOverVolume()
    {
        DerivedDimension.Density.Exponents["M"].Should().Be(1);
        DerivedDimension.Density.Exponents["L"].Should().Be(-3);
    }

    [Fact]
    public void DerivedDimension_Create_PowersDimension()
    {
        var area = DerivedDimension.Create(Dimension.FromBaseDimensions(length: 1), 2);
        area.Exponents["L"].Should().Be(2);
    }

    [Fact]
    public void DerivedDimension_Multiply_CombinesDimensions()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var time = Dimension.FromBaseDimensions(time: -1);
        var result = DerivedDimension.Multiply(length, time);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DerivedDimension_Divide_CombinesDimensions()
    {
        var result = DerivedDimension.Divide(DerivedDimension.Energy, DerivedDimension.Power);
        result.Exponents["T"].Should().Be(1);
    }

    [Fact]
    public void DerivedDimension_Voltage_HasCorrectExponents()
    {
        DerivedDimension.Voltage.Exponents["M"].Should().Be(1);
        DerivedDimension.Voltage.Exponents["L"].Should().Be(2);
        DerivedDimension.Voltage.Exponents["T"].Should().Be(-3);
        DerivedDimension.Voltage.Exponents["I"].Should().Be(-1);
    }

    [Fact]
    public void DerivedDimension_Resistance_HasCorrectExponents()
    {
        DerivedDimension.Resistance.Exponents["M"].Should().Be(1);
        DerivedDimension.Resistance.Exponents["L"].Should().Be(2);
        DerivedDimension.Resistance.Exponents["T"].Should().Be(-3);
        DerivedDimension.Resistance.Exponents["I"].Should().Be(-2);
    }

    [Fact]
    public void DerivedDimension_ElectricCharge_HasCorrectExponents()
    {
        DerivedDimension.ElectricCharge.Exponents["I"].Should().Be(1);
        DerivedDimension.ElectricCharge.Exponents["T"].Should().Be(1);
    }

    [Fact]
    public void DimensionBuilder_BuildLength()
    {
        var d = new DimensionBuilder().Length().Build();
        d.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void DimensionBuilder_BuildMass()
    {
        var d = new DimensionBuilder().Mass().Build();
        d.Exponents["M"].Should().Be(1);
    }

    [Fact]
    public void DimensionBuilder_BuildTime()
    {
        var d = new DimensionBuilder().Time().Build();
        d.Exponents["T"].Should().Be(1);
    }

    [Fact]
    public void DimensionBuilder_BuildMultiple()
    {
        var d = new DimensionBuilder()
            .Mass()
            .Length(2)
            .Time(-2)
            .Build();
        d.Exponents["M"].Should().Be(1);
        d.Exponents["L"].Should().Be(2);
        d.Exponents["T"].Should().Be(-2);
    }

    [Fact]
    public void DimensionBuilder_CancelingExponentsRemoves()
    {
        var d = new DimensionBuilder()
            .Length(1)
            .Length(-1)
            .Build();
        d.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void DimensionBuilder_WithCustomExponent()
    {
        var d = new DimensionBuilder().Length(3).Build();
        d.Exponents["L"].Should().Be(3);
    }

    [Fact]
    public void DimensionVector_DefaultConstructor_HasSevenComponents()
    {
        var v = new DimensionVector();
        v.Components.Should().HaveCount(7);
    }

    [Fact]
    public void DimensionVector_Indexer_Length()
    {
        var v = new DimensionVector { Components = new double[] { 1, 0, 0, 0, 0, 0, 0 } };
        v[BaseDimension.Length].Should().Be(1);
    }

    [Fact]
    public void DimensionVector_Indexer_Mass()
    {
        var v = new DimensionVector { Components = new double[] { 0, 2, 0, 0, 0, 0, 0 } };
        v[BaseDimension.Mass].Should().Be(2);
    }

    [Fact]
    public void DimensionVector_Multiply_AddsExponents()
    {
        var v1 = new DimensionVector { Components = new double[] { 1, 0, 0, 0, 0, 0, 0 } };
        var v2 = new DimensionVector { Components = new double[] { 0, 0, -1, 0, 0, 0, 0 } };
        var result = v1.Multiply(v2);
        result.Components[0].Should().Be(1);
        result.Components[2].Should().Be(-1);
    }

    [Fact]
    public void DimensionVector_Scale_MultipliesComponents()
    {
        var v = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        var result = v.Scale(2);
        result.Components[0].Should().Be(2);
        result.Components[1].Should().Be(4);
        result.Components[2].Should().Be(6);
    }

    [Fact]
    public void DimensionVector_Power_ScalesComponents()
    {
        var v = new DimensionVector { Components = new double[] { 1, 2, 0, 0, 0, 0, 0 } };
        var result = v.Power(3);
        result.Components[0].Should().Be(3);
        result.Components[1].Should().Be(6);
    }

    [Fact]
    public void DimensionVector_ToDimension_Converts()
    {
        var v = new DimensionVector { Components = new double[] { 1, 0, -2, 0, 0, 0, 0 } };
        var d = v.ToDimension();
        d.Exponents["L"].Should().Be(1);
        d.Exponents["T"].Should().Be(-2);
    }

    [Fact]
    public void DimensionVector_FromDimension_Converts()
    {
        var d = Dimension.FromBaseDimensions(mass: 1, length: 2);
        var v = DimensionVector.FromDimension(d);
        v[BaseDimension.Mass].Should().Be(1);
        v[BaseDimension.Length].Should().Be(2);
    }

    [Fact]
    public void DimensionVector_RoundTrip()
    {
        var original = Dimension.FromBaseDimensions(length: 1, time: -2, current: 3);
        var v = DimensionVector.FromDimension(original);
        var d = v.ToDimension();
        d.Exponents.Should().Equal(original.Exponents);
    }

    [Fact]
    public void DimensionOperations_Multiply()
    {
        var a = Dimension.FromBaseDimensions(length: 1);
        var b = Dimension.FromBaseDimensions(time: -1);
        var result = DimensionOperations.Multiply(a, b);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DimensionOperations_Divide()
    {
        var a = Dimension.FromBaseDimensions(mass: 1, length: 2, time: -2);
        var b = Dimension.FromBaseDimensions(time: -1);
        var result = DimensionOperations.Divide(a, b);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DimensionOperations_Power()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionOperations.Power(d, 2);
        result.Exponents["L"].Should().Be(2);
    }

    [Fact]
    public void DimensionOperations_Root()
    {
        var d = Dimension.FromBaseDimensions(length: 2);
        var result = DimensionOperations.Root(d, 2);
        result.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void DimensionOperations_AreCompatible_Same()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        DimensionOperations.AreCompatible(d, d).Should().BeTrue();
    }

    [Fact]
    public void DimensionOperations_AreCompatible_Different()
    {
        var a = Dimension.FromBaseDimensions(length: 1);
        var b = Dimension.FromBaseDimensions(mass: 1);
        DimensionOperations.AreCompatible(a, b).Should().BeFalse();
    }

    [Fact]
    public void DimensionOperations_ComputeFromProduct()
    {
        var dims = new[] {
            Dimension.FromBaseDimensions(mass: 1),
            Dimension.FromBaseDimensions(length: 1),
            Dimension.FromBaseDimensions(time: -2)
        };
        var exponents = new[] { 1.0, 1.0, -2.0 };
        var result = DimensionOperations.ComputeFromProduct(dims, exponents);
        result.Exponents["M"].Should().Be(1);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(4);
    }

    [Fact]
    public void DimensionOperations_ComputeFromProduct_ThrowsForMismatchedLengths()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1) };
        var exponents = new[] { 1.0, 2.0 };
        Action act = () => DimensionOperations.ComputeFromProduct(dims, exponents);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DimensionComparer_Instance_IsSingleton()
    {
        DimensionComparer.Instance.Should().BeSameAs(DimensionComparer.Instance);
    }

    [Fact]
    public void DimensionComparer_Compare_NullBoth_ReturnsZero()
    {
        DimensionComparer.Instance.Compare(null, null).Should().Be(0);
    }

    [Fact]
    public void DimensionComparer_Compare_NullFirst_ReturnsNegative()
    {
        DimensionComparer.Instance.Compare(null, Dimension.FromBaseDimensions(length: 1)).Should().BeLessThan(0);
    }

    [Fact]
    public void DimensionComparer_Compare_NullSecond_ReturnsPositive()
    {
        DimensionComparer.Instance.Compare(Dimension.FromBaseDimensions(length: 1), null).Should().BeGreaterThan(0);
    }

    [Fact]
    public void DimensionComparer_Compare_SameDimension_ReturnsZero()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        DimensionComparer.Instance.Compare(d, d).Should().Be(0);
    }

    [Fact]
    public void DimensionComparer_Equals_NullBoth_ReturnsTrue()
    {
        DimensionComparer.Instance.Equals(null, null).Should().BeTrue();
    }

    [Fact]
    public void DimensionComparer_Equals_OneNull_ReturnsFalse()
    {
        DimensionComparer.Instance.Equals(null, Dimension.None).Should().BeFalse();
    }

    [Fact]
    public void DimensionComparer_Equals_SameDimension_ReturnsTrue()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        DimensionComparer.Instance.Equals(d, d).Should().BeTrue();
    }

    [Fact]
    public void DimensionComparer_GetHashCode_EqualForEqualDimensions()
    {
        var d1 = Dimension.FromBaseDimensions(length: 1);
        var d2 = Dimension.FromBaseDimensions(length: 1);
        DimensionComparer.Instance.GetHashCode(d1).Should().Be(DimensionComparer.Instance.GetHashCode(d2));
    }

    [Fact]
    public void DerivedDimension_Capacitance_HasCorrectExponents()
    {
        DerivedDimension.Capacitance.Exponents["M"].Should().Be(-1);
        DerivedDimension.Capacitance.Exponents["L"].Should().Be(-2);
        DerivedDimension.Capacitance.Exponents["T"].Should().Be(4);
        DerivedDimension.Capacitance.Exponents["I"].Should().Be(2);
    }

    [Fact]
    public void DerivedDimension_Pressure_HasCorrectExponents()
    {
        DerivedDimension.Pressure.Exponents["M"].Should().Be(1);
        DerivedDimension.Pressure.Exponents["L"].Should().Be(-1);
        DerivedDimension.Pressure.Exponents["T"].Should().Be(-2);
    }

    [Fact]
    public void DimensionVector_Equals_SameComponents()
    {
        var v1 = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        var v2 = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        v1.Equals(v2).Should().BeTrue();
    }

    [Fact]
    public void DimensionVector_Equals_DifferentComponents()
    {
        var v1 = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        var v2 = new DimensionVector { Components = new double[] { 1, 2, 4, 0, 0, 0, 0 } };
        v1.Equals(v2).Should().BeFalse();
    }

    [Fact]
    public void DimensionVector_GetHashCode_SameForEqualVectors()
    {
        var v1 = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        var v2 = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        v1.GetHashCode().Should().Be(v2.GetHashCode());
    }

    [Fact]
    public void DimensionVector_Multiply_NullComponents()
    {
        var v1 = new DimensionVector();
        var v2 = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        var result = v1.Multiply(v2);
        result.Components[0].Should().Be(1);
        result.Components[1].Should().Be(2);
    }

    [Fact]
    public void DimensionVector_Scale_ZeroFactor()
    {
        var v = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        var result = v.Scale(0);
        result.Components.Should().AllSatisfy(x => x.Should().Be(0));
    }

    [Fact]
    public void DimensionVector_Scale_NegativeFactor()
    {
        var v = new DimensionVector { Components = new double[] { 1, 2, 3, 0, 0, 0, 0 } };
        var result = v.Scale(-2);
        result.Components[0].Should().Be(-2);
        result.Components[1].Should().Be(-4);
        result.Components[2].Should().Be(-6);
    }

    [Fact]
    public void DimensionVector_Power_FractionalExponent()
    {
        var v = new DimensionVector { Components = new double[] { 2, 4, 0, 0, 0, 0, 0 } };
        var result = v.Power(0.5);
        result.Components[0].Should().Be(1);
        result.Components[1].Should().Be(2);
    }

    [Fact]
    public void DimensionVector_ToDimension_EmptyVector()
    {
        var v = new DimensionVector();
        var d = v.ToDimension();
        d.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void DimensionVector_FromDimension_AllBaseDimensions()
    {
        var d = Dimension.FromBaseDimensions(length: 1, mass: 1, time: -2, current: 1, temperature: -1, substance: 1, luminous: 1);
        var v = DimensionVector.FromDimension(d);
        v[BaseDimension.Length].Should().Be(1);
        v[BaseDimension.Mass].Should().Be(1);
        v[BaseDimension.Time].Should().Be(-2);
        v[BaseDimension.ElectricCurrent].Should().Be(1);
        v[BaseDimension.Temperature].Should().Be(-1);
        v[BaseDimension.AmountOfSubstance].Should().Be(1);
        v[BaseDimension.LuminousIntensity].Should().Be(1);
    }

    [Fact]
    public void DimensionVector_RoundTrip_MultipleTimes()
    {
        var original = Dimension.FromBaseDimensions(length: 1, time: -2, current: 3);
        var v1 = DimensionVector.FromDimension(original);
        var d1 = v1.ToDimension();
        var v2 = DimensionVector.FromDimension(d1);
        var d2 = v2.ToDimension();
        d2.Exponents.Should().Equal(original.Exponents);
    }

    [Fact]
    public void DimensionOperations_Multiply_Dimensionless()
    {
        var result = DimensionOperations.Multiply(Dimension.None, Dimension.FromBaseDimensions(length: 1));
        result.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void DimensionOperations_Divide_ByDimensionless()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionOperations.Divide(d, Dimension.None);
        result.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void DimensionOperations_Power_NegativeExponent()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        var result = DimensionOperations.Power(d, -2);
        result.Exponents["L"].Should().Be(-2);
    }

    [Fact]
    public void DimensionOperations_Power_FractionalExponent()
    {
        var d = Dimension.FromBaseDimensions(length: 2);
        var result = DimensionOperations.Power(d, 0.5);
        result.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void DimensionOperations_Root_OfPower()
    {
        var d = Dimension.FromBaseDimensions(length: 6);
        var result = DimensionOperations.Root(d, 3);
        result.Exponents["L"].Should().Be(2);
    }

    [Fact]
    public void DimensionOperations_Root_NonIntegerResult()
    {
        var d = Dimension.FromBaseDimensions(length: 2);
        var result = DimensionOperations.Root(d, 3);
        result.Exponents["L"].Should().BeApproximately(2.0 / 3.0, 1e-10);
    }

    [Fact]
    public void DimensionOperations_Simplify_MultipleZeroExponents()
    {
        var d = new Dimension(System.Collections.Immutable.ImmutableDictionary<string, double>.Empty
            .Add("L", 1.0).Add("M", 0.0).Add("T", 0.0).Add("I", 0.0));
        var simplified = DimensionOperations.Simplify(d);
        simplified.Exponents.Should().HaveCount(1);
        simplified.Exponents.Should().ContainKey("L");
    }

    [Fact]
    public void DimensionOperations_AreCompatible_DerivedDimensions()
    {
        DimensionOperations.AreCompatible(DerivedDimension.Force, DerivedDimension.Force).Should().BeTrue();
    }

    [Fact]
    public void DimensionOperations_AreCompatible_DerivedVsBase()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        DimensionOperations.AreCompatible(length, DerivedDimension.Velocity).Should().BeFalse();
    }

    [Fact]
    public void DimensionOperations_ComputeFromProduct_SingleDimension()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1) };
        var exponents = new[] { 3.0 };
        var result = DimensionOperations.ComputeFromProduct(dims, exponents);
        result.Exponents["L"].Should().Be(3);
    }

    [Fact]
    public void DimensionOperations_ComputeFromProduct_ZeroExponent()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1), Dimension.FromBaseDimensions(mass: 1) };
        var exponents = new[] { 1.0, 0.0 };
        var result = DimensionOperations.ComputeFromProduct(dims, exponents);
        result.Exponents.Should().ContainKey("L");
        result.Exponents.Should().NotContainKey("M");
    }

    [Fact]
    public void DimensionOperations_ComputeFromProduct_NegativeExponents()
    {
        var dims = new[] { Dimension.FromBaseDimensions(length: 1), Dimension.FromBaseDimensions(time: 1) };
        var exponents = new[] { 1.0, -1.0 };
        var result = DimensionOperations.ComputeFromProduct(dims, exponents);
        result.Exponents["L"].Should().Be(1);
        result.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DimensionComparer_GetHashCode_NullReturnsZero()
    {
        DimensionComparer.Instance.GetHashCode(null).Should().Be(0);
    }

    [Fact]
    public void DerivedDimension_MagneticFlux_HasCorrectExponents()
    {
        DerivedDimension.MagneticFlux.Exponents["M"].Should().Be(1);
        DerivedDimension.MagneticFlux.Exponents["L"].Should().Be(2);
        DerivedDimension.MagneticFlux.Exponents["T"].Should().Be(-2);
        DerivedDimension.MagneticFlux.Exponents["I"].Should().Be(-1);
    }

    [Fact]
    public void DerivedDimension_MagneticFluxDensity_HasCorrectExponents()
    {
        DerivedDimension.MagneticFluxDensity.Exponents["M"].Should().Be(1);
        DerivedDimension.MagneticFluxDensity.Exponents["T"].Should().Be(-2);
        DerivedDimension.MagneticFluxDensity.Exponents["I"].Should().Be(-1);
    }

    [Fact]
    public void DerivedDimension_Inductance_HasCorrectExponents()
    {
        DerivedDimension.Inductance.Exponents["M"].Should().Be(1);
        DerivedDimension.Inductance.Exponents["L"].Should().Be(2);
        DerivedDimension.Inductance.Exponents["T"].Should().Be(-2);
        DerivedDimension.Inductance.Exponents["I"].Should().Be(-2);
    }

    [Fact]
    public void DerivedDimension_Conductance_HasCorrectExponents()
    {
        DerivedDimension.Conductance.Exponents["M"].Should().Be(-1);
        DerivedDimension.Conductance.Exponents["L"].Should().Be(-2);
        DerivedDimension.Conductance.Exponents["T"].Should().Be(3);
        DerivedDimension.Conductance.Exponents["I"].Should().Be(2);
    }

    [Fact]
    public void DimensionBuilder_AllBaseDimensions()
    {
        var d = new DimensionBuilder()
            .Length()
            .Mass()
            .Time()
            .Current()
            .Temperature()
            .Substance()
            .Luminous()
            .Build();
        d.Exponents.Should().HaveCount(7);
        d.Exponents["L"].Should().Be(1);
        d.Exponents["M"].Should().Be(1);
        d.Exponents["T"].Should().Be(1);
        d.Exponents["I"].Should().Be(1);
        d.Exponents["K"].Should().Be(1);
        d.Exponents["N"].Should().Be(1);
        d.Exponents["J"].Should().Be(1);
    }

    [Fact]
    public void DimensionBuilder_WithNegativeExponents()
    {
        var d = new DimensionBuilder()
            .Length(-1)
            .Time(-2)
            .Build();
        d.Exponents["L"].Should().Be(-1);
        d.Exponents["T"].Should().Be(-2);
    }

    [Fact]
    public void DimensionBuilder_FractionalExponents()
    {
        var d = new DimensionBuilder()
            .Length(0.5)
            .Time(-1.5)
            .Build();
        d.Exponents["L"].Should().Be(0.5);
        d.Exponents["T"].Should().Be(-1.5);
    }

    [Fact]
    public void BaseDimension_FromSymbol_AllValid()
    {
        BaseDimensionExtensions.FromSymbol("L").Should().Be(BaseDimension.Length);
        BaseDimensionExtensions.FromSymbol("M").Should().Be(BaseDimension.Mass);
        BaseDimensionExtensions.FromSymbol("T").Should().Be(BaseDimension.Time);
        BaseDimensionExtensions.FromSymbol("I").Should().Be(BaseDimension.ElectricCurrent);
        BaseDimensionExtensions.FromSymbol("K").Should().Be(BaseDimension.Temperature);
        BaseDimensionExtensions.FromSymbol("N").Should().Be(BaseDimension.AmountOfSubstance);
        BaseDimensionExtensions.FromSymbol("J").Should().Be(BaseDimension.LuminousIntensity);
    }

    [Fact]
    public void BaseDimension_FromSymbol_CaseSensitive()
    {
        Action act = () => BaseDimensionExtensions.FromSymbol("l");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Dimension_Equality_WithDifferentDictionaryInstances()
    {
        var d1 = Dimension.FromBaseDimensions(length: 1, time: -1);
        var d2 = new Dimension(System.Collections.Immutable.ImmutableDictionary<string, double>.Empty
            .Add("L", 1.0).Add("T", -1.0));
        d1.Equals(d2).Should().BeTrue();
    }

    [Fact]
    public void Dimension_GetHashCode_SameForEqualDimensions_MultipleExponents()
    {
        var d1 = Dimension.FromBaseDimensions(length: 1, mass: 2, time: -3);
        var d2 = Dimension.FromBaseDimensions(length: 1, mass: 2, time: -3);
        d1.GetHashCode().Should().Be(d2.GetHashCode());
    }

    [Fact]
    public void Dimension_ToString_ComplexDimension()
    {
        var d = DerivedDimension.Force;
        var s = d.ToString();
        s.Should().Contain("M");
        s.Should().Contain("L");
        s.Should().Contain("T^-2");
    }

    [Fact]
    public void Dimension_IsCompatibleWith_Symmetric()
    {
        var a = Dimension.FromBaseDimensions(length: 1);
        var b = Dimension.FromBaseDimensions(length: 1);
        a.IsCompatibleWith(b).Should().BeTrue();
        b.IsCompatibleWith(a).Should().BeTrue();
    }

    [Fact]
    public void Dimension_Multiply_Associative()
    {
        var a = Dimension.FromBaseDimensions(length: 1);
        var b = Dimension.FromBaseDimensions(mass: 1);
        var c = Dimension.FromBaseDimensions(time: -2);
        var left = a.Multiply(b).Multiply(c);
        var right = a.Multiply(b.Multiply(c));
        left.Equals(right).Should().BeTrue();
    }

    [Fact]
    public void Dimension_Divide_MultiplyInverse()
    {
        var a = Dimension.FromBaseDimensions(length: 2);
        var b = Dimension.FromBaseDimensions(length: 1);
        var result = a.Divide(b);
        result.Exponents["L"].Should().Be(1);
    }

    [Fact]
    public void Dimension_Power_ThenRoot_ReturnsOriginal()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        var powered = d.Power(4);
        var rooted = powered.Root(2);
        rooted.Exponents["L"].Should().Be(2);
    }

    [Fact]
    public void DimensionBuilder_CancelMultipleExponents()
    {
        var d = new DimensionBuilder()
            .Length(2)
            .Length(-1)
            .Length(-1)
            .Build();
        d.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void Dimension_FromBaseDimensions_EmptyDictionary()
    {
        var d = Dimension.FromBaseDimensions();
        d.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void DerivedDimension_Action_HasCorrectExponents()
    {
        DerivedDimension.Action.Exponents["M"].Should().Be(1);
        DerivedDimension.Action.Exponents["L"].Should().Be(2);
        DerivedDimension.Action.Exponents["T"].Should().Be(-1);
    }

    [Fact]
    public void DimensionVector_Indexer_GetSet()
    {
        var dv = new DimensionVector();
        dv[BaseDimension.Length] = 2.5;
        dv[BaseDimension.Length].Should().Be(2.5);
    }

    [Fact]
    public void DimensionVector_Multiply_CombinesComponents()
    {
        var dv1 = DimensionVector.FromDimension(Dimension.FromBaseDimensions(length: 1, mass: 1));
        var dv2 = DimensionVector.FromDimension(Dimension.FromBaseDimensions(time: -2));
        var result = dv1.Multiply(dv2);
        result[BaseDimension.Length].Should().Be(1);
        result[BaseDimension.Mass].Should().Be(1);
        result[BaseDimension.Time].Should().Be(-2);
    }

    [Fact]
    public void DimensionVector_Scale_MultipliesAllComponents()
    {
        var dv = DimensionVector.FromDimension(Dimension.FromBaseDimensions(length: 2, mass: 1));
        var scaled = dv.Scale(3.0);
        scaled[BaseDimension.Length].Should().Be(6);
        scaled[BaseDimension.Mass].Should().Be(3);
    }

    [Fact]
    public void DimensionVector_Power_RaisesEachComponent()
    {
        var dv = DimensionVector.FromDimension(Dimension.FromBaseDimensions(length: 2));
        var powered = dv.Power(3);
        powered[BaseDimension.Length].Should().Be(6);
    }

    [Fact]
    public void DimensionOperations_AreCompatible_SameBaseDimensions()
    {
        var a = Dimension.FromBaseDimensions(length: 1);
        var b = Dimension.FromBaseDimensions(length: 2);
        DimensionOperations.AreCompatible(a, b).Should().BeTrue();
    }

    [Fact]
    public void DimensionOperations_ComputeFromProduct_EmptyArrays()
    {
        var result = DimensionOperations.ComputeFromProduct(System.Array.Empty<Dimension>(), System.Array.Empty<double>());
        result.Should().Be(Dimension.None);
    }

    [Fact]
    public void DimensionComparer_Equals_ForEqualDimensions()
    {
        var a = Dimension.FromBaseDimensions(length: 1);
        var b = Dimension.FromBaseDimensions(length: 1);
        DimensionComparer.Instance.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void DimensionComparer_GetHashCode_EqualDimensionsSameHash()
    {
        var a = Dimension.FromBaseDimensions(length: 1);
        var b = Dimension.FromBaseDimensions(length: 1);
        DimensionComparer.Instance.GetHashCode(a).Should().Be(DimensionComparer.Instance.GetHashCode(b));
    }

    [Fact]
    public void Dimension_Serialization_RoundTrip()
    {
        var d = Dimension.FromBaseDimensions(length: 2, mass: 1, time: -3);
        var json = System.Text.Json.JsonSerializer.Serialize(d);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<Dimension>(json);
        deserialized.Should().BeEquivalentTo(d);
    }

    [Fact]
    public void BaseDimension_Symbol_ReturnsCorrectValues()
    {
        BaseDimension.Length.GetSymbol().Should().Be("L");
        BaseDimension.Mass.GetSymbol().Should().Be("M");
        BaseDimension.Time.GetSymbol().Should().Be("T");
        BaseDimension.ElectricCurrent.GetSymbol().Should().Be("I");
        BaseDimension.Temperature.GetSymbol().Should().Be("\u0398");
        BaseDimension.AmountOfSubstance.GetSymbol().Should().Be("N");
        BaseDimension.LuminousIntensity.GetSymbol().Should().Be("J");
    }

    [Fact]
    public void BaseDimension_DisplayName_ReturnsCorrectValues()
    {
        BaseDimension.Length.GetDisplayName().Should().Be("Length");
        BaseDimension.Mass.GetDisplayName().Should().Be("Mass");
    }

    [Fact]
    public void DerivedDimension_AllProperties_Accessible()
    {
        DerivedDimension.Velocity.Should().NotBeNull();
        DerivedDimension.Acceleration.Should().NotBeNull();
        DerivedDimension.Force.Should().NotBeNull();
        DerivedDimension.Energy.Should().NotBeNull();
        DerivedDimension.Power.Should().NotBeNull();
        DerivedDimension.Pressure.Should().NotBeNull();
        DerivedDimension.Frequency.Should().NotBeNull();
        DerivedDimension.ElectricCharge.Should().NotBeNull();
        DerivedDimension.Voltage.Should().NotBeNull();
        DerivedDimension.Resistance.Should().NotBeNull();
        DerivedDimension.Capacitance.Should().NotBeNull();
        DerivedDimension.MagneticFlux.Should().NotBeNull();
        DerivedDimension.MagneticField.Should().NotBeNull();
        DerivedDimension.Area.Should().NotBeNull();
        DerivedDimension.Volume.Should().NotBeNull();
        DerivedDimension.Density.Should().NotBeNull();
        DerivedDimension.MomentOfForce.Should().NotBeNull();
        DerivedDimension.Action.Should().NotBeNull();
    }

    [Fact]
    public void DimensionBuilder_WithAllBaseDimensions()
    {
        var d = new DimensionBuilder()
            .Length(1)
            .Mass(1)
            .Time(-2)
            .Current(1)
            .Temperature(-1)
            .Substance(1)
            .Luminous(-1)
            .Build();
        d.Exponents["L"].Should().Be(1);
        d.Exponents["M"].Should().Be(1);
        d.Exponents["T"].Should().Be(-2);
        d.Exponents["I"].Should().Be(1);
        d.Exponents["\u0398"].Should().Be(-1);
        d.Exponents["N"].Should().Be(1);
        d.Exponents["J"].Should().Be(-1);
    }

    [Fact]
    public void DimensionVector_FromDimension_HandlesMissingDimensions()
    {
        var d = Dimension.FromBaseDimensions(length: 1);
        var dv = DimensionVector.FromDimension(d);
        dv[BaseDimension.Mass].Should().Be(0);
    }

    [Fact]
    public void Dimension_Equals_ForEquivalentDimensions()
    {
        var a = Dimension.FromBaseDimensions(length: 1, mass: 1);
        var b = Dimension.FromBaseDimensions(mass: 1, length: 1);
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Dimension_GetHashCode_EqualDimensionsSameHash()
    {
        var a = Dimension.FromBaseDimensions(length: 1);
        var b = Dimension.FromBaseDimensions(length: 1);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Dimension_None_IsBaseDimension_False()
    {
        Dimension.None.IsBaseDimension.Should().BeFalse();
    }
}

