namespace MathVerse.Foundation.Tests;

public sealed class UnitTests
{
    // ── Unit ──────────────────────────────────────────────────────

    [Fact]
    public void Unit_DefaultProperties_AreCorrect()
    {
        var unit = new Unit();
        unit.Symbol.Should().BeEmpty();
        unit.Name.Should().BeEmpty();
        unit.Dimension.Should().Be(Dimension.None);
        unit.Category.Should().Be(UnitCategory.Other);
        unit.ScaleFactor.Should().Be(1.0);
        unit.Aliases.Should().BeEmpty();
    }

    [Fact]
    public void Unit_WithProperties_SetsCorrectly()
    {
        var unit = new Unit
        {
            Symbol = "m",
            Name = "Meter",
            Dimension = Dimension.FromBaseDimensions(length: 1),
            Category = UnitCategory.Length,
            ScaleFactor = 1.0
        };
        unit.Symbol.Should().Be("m");
        unit.Name.Should().Be("Meter");
        unit.Category.Should().Be(UnitCategory.Length);
    }

    [Fact]
    public void Unit_IsBaseDimension_ReturnsTrue()
    {
        var unit = new Unit
        {
            Symbol = "m",
            Dimension = Dimension.FromBaseDimensions(length: 1)
        };
        unit.IsBaseUnit.Should().BeTrue();
    }

    [Fact]
    public void Unit_IsDerivedUnit_ReturnsTrue()
    {
        var unit = new Unit
        {
            Symbol = "N",
            Dimension = DerivedDimension.Force,
            Category = UnitCategory.Force
        };
        unit.IsDerivedUnit.Should().BeTrue();
    }

    [Fact]
    public void Unit_IsDimensionless_NotBaseOrDerived()
    {
        var unit = new Unit
        {
            Symbol = "rad",
            Dimension = Dimension.None,
            Category = UnitCategory.Dimensionless
        };
        unit.IsBaseUnit.Should().BeFalse();
        unit.IsDerivedUnit.Should().BeFalse();
    }

    [Fact]
    public void Unit_WithPrefix_AppendsSymbolAndName()
    {
        var meter = new Unit
        {
            Symbol = "m",
            Name = "Meter",
            ScaleFactor = 1.0
        };
        var kilometer = meter.WithPrefix(UnitPrefixes.Kilo);
        kilometer.Symbol.Should().Be("km");
        kilometer.Name.Should().Be("kiloMeter");
        kilometer.ScaleFactor.Should().Be(1000.0);
    }

    [Fact]
    public void Unit_WithPrefix_Centi_CorrectScale()
    {
        var meter = new Unit { Symbol = "m", Name = "Meter", ScaleFactor = 1.0 };
        var centimeter = meter.WithPrefix(UnitPrefixes.Centi);
        centimeter.Symbol.Should().Be("cm");
        centimeter.ScaleFactor.Should().Be(0.01);
    }

    [Fact]
    public void Unit_WithPrefix_Nano_CorrectScale()
    {
        var second = new Unit { Symbol = "s", Name = "Second", ScaleFactor = 1.0 };
        var nanosecond = second.WithPrefix(UnitPrefixes.Nano);
        nanosecond.Symbol.Should().Be("ns");
        nanosecond.ScaleFactor.Should().Be(1e-9);
    }

    [Fact]
    public void Unit_ToString_ReturnsSymbol()
    {
        var unit = new Unit { Symbol = "kg" };
        unit.ToString().Should().Be("kg");
    }

    [Fact]
    public void Unit_Equality_SameValues_AreEqual()
    {
        var a = new Unit { Symbol = "m", Name = "Meter", ScaleFactor = 1.0 };
        var b = new Unit { Symbol = "m", Name = "Meter", ScaleFactor = 1.0 };
        a.Should().Be(b);
    }

    [Fact]
    public void Unit_Equality_DifferentSymbol_NotEqual()
    {
        var a = new Unit { Symbol = "m", Name = "Meter" };
        var b = new Unit { Symbol = "kg", Name = "Kilogram" };
        a.Should().NotBe(b);
    }

    [Fact]
    public void Unit_Equality_Null_IsNotEqual()
    {
        var unit = new Unit { Symbol = "m" };
        unit.Equals(null!).Should().BeFalse();
    }

    [Fact]
    public void Unit_Equality_DifferentScaleFactor_NotEqual()
    {
        var a = new Unit { Symbol = "cm", ScaleFactor = 0.01 };
        var b = new Unit { Symbol = "cm", ScaleFactor = 1.0 };
        a.Should().NotBe(b);
    }

    [Fact]
    public void Unit_Equality_DifferentAliases_NotEqual()
    {
        var a = new Unit { Symbol = "Ω", Aliases = ImmutableArray.Create("Ohm") };
        var b = new Unit { Symbol = "Ω", Aliases = ImmutableArray<string>.Empty };
        a.Should().NotBe(b);
    }

    [Fact]
    public void Unit_GetHashCode_SameForEqualUnits()
    {
        var a = new Unit { Symbol = "m", Name = "Meter", ScaleFactor = 1.0 };
        var b = new Unit { Symbol = "m", Name = "Meter", ScaleFactor = 1.0 };
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Unit_Immutability_WithDoesNotMutateOriginal()
    {
        var original = new Unit { Symbol = "m", ScaleFactor = 1.0 };
        var prefixed = original.WithPrefix(UnitPrefixes.Kilo);
        original.Symbol.Should().Be("m");
        original.ScaleFactor.Should().Be(1.0);
        prefixed.Symbol.Should().Be("km");
    }

    [Fact]
    public void Unit_WithAliases_AreStoredCorrectly()
    {
        var unit = new Unit
        {
            Symbol = "Ω",
            Name = "Ohm",
            Aliases = ImmutableArray.Create("Ohm", "ohm")
        };
        unit.Aliases.Should().HaveCount(2);
        unit.Aliases.Should().Contain("Ohm");
        unit.Aliases.Should().Contain("ohm");
    }

    // ── UnitPrefix ────────────────────────────────────────────────

    [Fact]
    public void UnitPrefix_DefaultValues()
    {
        var prefix = new UnitPrefix();
        prefix.Symbol.Should().BeEmpty();
        prefix.Name.Should().BeEmpty();
        prefix.Factor.Should().Be(1.0);
    }

    [Fact]
    public void UnitPrefix_Kilo_HasCorrectValues()
    {
        UnitPrefixes.Kilo.Symbol.Should().Be("k");
        UnitPrefixes.Kilo.Name.Should().Be("kilo");
        UnitPrefixes.Kilo.Factor.Should().Be(1e3);
    }

    [Fact]
    public void UnitPrefix_Mega_HasCorrectValues()
    {
        UnitPrefixes.Mega.Symbol.Should().Be("M");
        UnitPrefixes.Mega.Factor.Should().Be(1e6);
    }

    [Fact]
    public void UnitPrefix_Micro_HasCorrectValues()
    {
        UnitPrefixes.Micro.Symbol.Should().Be("μ");
        UnitPrefixes.Micro.Factor.Should().Be(1e-6);
    }

    [Fact]
    public void UnitPrefix_FromSymbol_Kilo()
    {
        var prefix = UnitPrefixes.FromSymbol("k");
        prefix.Should().NotBeNull();
        prefix!.Name.Should().Be("kilo");
    }

    [Fact]
    public void UnitPrefix_FromSymbol_Unknown_ReturnsNull()
    {
        var prefix = UnitPrefixes.FromSymbol("xx");
        prefix.Should().BeNull();
    }

    [Fact]
    public void UnitPrefix_FromName_CaseInsensitive()
    {
        var prefix = UnitPrefixes.FromName("MEGA");
        prefix.Should().NotBeNull();
        prefix!.Symbol.Should().Be("M");
    }

    [Fact]
    public void UnitPrefix_FromName_Unknown_ReturnsNull()
    {
        var prefix = UnitPrefixes.FromName("nonexistent");
        prefix.Should().BeNull();
    }

    [Fact]
    public void UnitPrefix_FromFactor_FindsPrefix()
    {
        var prefix = UnitPrefixes.FromFactor(1e-3);
        prefix.Should().NotBeNull();
        prefix!.Symbol.Should().Be("m");
    }

    [Fact]
    public void UnitPrefix_All_ReturnsAllPrefixes()
    {
        var all = UnitPrefixes.All();
        all.Should().HaveCount(20);
    }

    [Fact]
    public void UnitPrefix_Yotta_HasCorrectFactor()
    {
        UnitPrefixes.Yotta.Factor.Should().Be(1e24);
        UnitPrefixes.Yotta.Symbol.Should().Be("Y");
    }

    [Fact]
    public void UnitPrefix_Yocto_HasCorrectFactor()
    {
        UnitPrefixes.Yocto.Factor.Should().Be(1e-24);
        UnitPrefixes.Yocto.Symbol.Should().Be("y");
    }

    // ── UnitCategory ──────────────────────────────────────────────

    [Fact]
    public void UnitCategory_HasExpectedMembers()
    {
        var values = Enum.GetValues<UnitCategory>();
        values.Should().Contain(UnitCategory.Length);
        values.Should().Contain(UnitCategory.Mass);
        values.Should().Contain(UnitCategory.Time);
        values.Should().Contain(UnitCategory.Force);
        values.Should().Contain(UnitCategory.Energy);
        values.Should().Contain(UnitCategory.Power);
        values.Should().Contain(UnitCategory.Pressure);
        values.Should().Contain(UnitCategory.Dimensionless);
        values.Should().Contain(UnitCategory.Other);
    }

    [Fact]
    public void UnitCategory_HasAllBaseQuantities()
    {
        var values = Enum.GetValues<UnitCategory>();
        values.Should().Contain(UnitCategory.ElectricCurrent);
        values.Should().Contain(UnitCategory.Temperature);
        values.Should().Contain(UnitCategory.AmountOfSubstance);
        values.Should().Contain(UnitCategory.LuminousIntensity);
    }

    [Fact]
    public void UnitCategory_HasDerivedCategories()
    {
        var values = Enum.GetValues<UnitCategory>();
        values.Should().Contain(UnitCategory.Frequency);
        values.Should().Contain(UnitCategory.Voltage);
        values.Should().Contain(UnitCategory.Resistance);
        values.Should().Contain(UnitCategory.Capacitance);
        values.Should().Contain(UnitCategory.Area);
        values.Should().Contain(UnitCategory.Volume);
        values.Should().Contain(UnitCategory.Density);
        values.Should().Contain(UnitCategory.Speed);
        values.Should().Contain(UnitCategory.Acceleration);
    }

    // ── UnitRegistry ──────────────────────────────────────────────

    [Fact]
    public void UnitRegistry_Instance_IsNotNull()
    {
        UnitRegistry.Instance.Should().NotBeNull();
    }

    [Fact]
    public void UnitRegistry_Instance_IsSingleton()
    {
        var a = UnitRegistry.Instance;
        var b = UnitRegistry.Instance;
        a.Should().BeSameAs(b);
    }

    [Fact]
    public void UnitRegistry_Get_Meter()
    {
        var unit = UnitRegistry.Instance.Get("m");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Meter");
    }

    [Fact]
    public void UnitRegistry_Get_Kilogram()
    {
        var unit = UnitRegistry.Instance.Get("kg");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Kilogram");
    }

    [Fact]
    public void UnitRegistry_Get_Unknown_ReturnsNull()
    {
        var unit = UnitRegistry.Instance.Get("zzz");
        unit.Should().BeNull();
    }

    [Fact]
    public void UnitRegistry_Get_Null_Throws()
    {
        Action act = () => UnitRegistry.Instance.Get(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnitRegistry_GetByCategory_Length()
    {
        var units = UnitRegistry.Instance.GetByCategory(UnitCategory.Length);
        units.Should().NotBeEmpty();
        units.Should().Contain(u => u.Symbol == "m");
    }

    [Fact]
    public void UnitRegistry_GetByCategory_Empty()
    {
        var units = UnitRegistry.Instance.GetByCategory(UnitCategory.Information);
        units.Should().BeEmpty();
    }

    [Fact]
    public void UnitRegistry_GetAll_ReturnsRegisteredUnits()
    {
        var all = UnitRegistry.Instance.GetAll();
        all.Should().NotBeEmpty();
        all.Should().Contain(u => u.Symbol == "m");
        all.Should().Contain(u => u.Symbol == "kg");
        all.Should().Contain(u => u.Symbol == "s");
    }

    [Fact]
    public void UnitRegistry_Register_CustomUnit()
    {
        var registry = UnitRegistry.Instance;
        var custom = new Unit
        {
            Symbol = "parsectest",
            Name = "Parsec Test",
            Category = UnitCategory.Length,
            Dimension = Dimension.FromBaseDimensions(length: 1),
            ScaleFactor = 3.086e16
        };
        registry.Register(custom);
        var retrieved = registry.Get("parsectest");
        retrieved.Should().NotBeNull();
        retrieved!.ScaleFactor.Should().Be(3.086e16);
    }

    [Fact]
    public void UnitRegistry_Register_Null_Throws()
    {
        Action act = () => UnitRegistry.Instance.Register(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnitRegistry_Register_WithAlias_LookupByAlias()
    {
        var registry = UnitRegistry.Instance;
        var unit = new Unit
        {
            Symbol = "aliastest",
            Name = "Alias Test",
            Category = UnitCategory.Other,
            Dimension = Dimension.None,
            Aliases = ImmutableArray.Create("alias1")
        };
        registry.Register(unit);
        var retrieved = registry.Get("alias1");
        retrieved.Should().NotBeNull();
        retrieved!.Symbol.Should().Be("aliastest");
    }

    // ── SIUnitSystem ──────────────────────────────────────────────

    [Fact]
    public void SIUnitSystem_Instance_IsNotNull()
    {
        SIUnitSystem.Instance.Should().NotBeNull();
    }

    [Fact]
    public void SIUnitSystem_Instance_IsSingleton()
    {
        SIUnitSystem.Instance.Should().BeSameAs(SIUnitSystem.Instance);
    }

    [Fact]
    public void SIUnitSystem_Name_IsSI()
    {
        SIUnitSystem.Instance.Name.Should().Be("SI");
    }

    [Fact]
    public void SIUnitSystem_BaseUnits_Has7BaseUnits()
    {
        SIUnitSystem.Instance.BaseUnits.Should().HaveCount(7);
    }

    [Fact]
    public void SIUnitSystem_BaseUnits_ContainsMeter()
    {
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "m");
    }

    [Fact]
    public void SIUnitSystem_BaseUnits_ContainsKilogram()
    {
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "kg");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_Meter()
    {
        var unit = SIUnitSystem.Instance.GetUnit("m");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Meter");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_Newton()
    {
        var unit = SIUnitSystem.Instance.GetUnit("N");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Newton");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_Joule()
    {
        var unit = SIUnitSystem.Instance.GetUnit("J");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Joule");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_Pascal()
    {
        var unit = SIUnitSystem.Instance.GetUnit("Pa");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Pascal");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_Hertz()
    {
        var unit = SIUnitSystem.Instance.GetUnit("Hz");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Hertz");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_Ohm()
    {
        var unit = SIUnitSystem.Instance.GetUnit("Ω");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Ohm");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_Ohm_ByAlias()
    {
        var unit = SIUnitSystem.Instance.GetUnit("Ohm");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Ohm");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_Unknown_ReturnsNull()
    {
        var unit = SIUnitSystem.Instance.GetUnit("zzz");
        unit.Should().BeNull();
    }

    [Fact]
    public void SIUnitSystem_GetByCategory_Force()
    {
        var unit = SIUnitSystem.Instance.GetUnit("N");
        unit.Should().NotBeNull();
        unit!.Category.Should().Be(UnitCategory.Force);
    }

    [Fact]
    public void SIUnitSystem_Default_IsInstance()
    {
        SIUnitSystem.Instance.Default.Should().BeSameAs(SIUnitSystem.Instance);
    }

    // ── CGSUnitSystem ─────────────────────────────────────────────

    [Fact]
    public void CGSUnitSystem_Instance_IsNotNull()
    {
        CGSUnitSystem.Instance.Should().NotBeNull();
    }

    [Fact]
    public void CGSUnitSystem_Name_IsCGS()
    {
        CGSUnitSystem.Instance.Name.Should().Be("CGS");
    }

    [Fact]
    public void CGSUnitSystem_BaseUnits_Has3()
    {
        CGSUnitSystem.Instance.BaseUnits.Should().HaveCount(3);
    }

    [Fact]
    public void CGSUnitSystem_GetUnit_Centimeter()
    {
        var unit = CGSUnitSystem.Instance.GetUnit("cm");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Centimeter");
    }

    [Fact]
    public void CGSUnitSystem_GetUnit_Gram()
    {
        var unit = CGSUnitSystem.Instance.GetUnit("g");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Gram");
    }

    [Fact]
    public void CGSUnitSystem_GetUnit_Second()
    {
        var unit = CGSUnitSystem.Instance.GetUnit("s");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("Second");
    }

    [Fact]
    public void CGSUnitSystem_Centimeter_ScaleFactor()
    {
        var unit = CGSUnitSystem.Instance.GetUnit("cm");
        unit!.ScaleFactor.Should().Be(0.01);
    }

    [Fact]
    public void CGSUnitSystem_Gram_ScaleFactor()
    {
        var unit = CGSUnitSystem.Instance.GetUnit("g");
        unit!.ScaleFactor.Should().Be(0.001);
    }

    [Fact]
    public void CGSUnitSystem_GetAll()
    {
        var all = CGSUnitSystem.Instance.BaseUnits;
        all.Should().HaveCount(3);
    }

    [Fact]
    public void CGSUnitSystem_Default_IsInstance()
    {
        CGSUnitSystem.Instance.Default.Should().BeSameAs(CGSUnitSystem.Instance);
    }

    // ── ImperialUnitSystem ────────────────────────────────────────

    [Fact]
    public void ImperialUnitSystem_Instance_IsNotNull()
    {
        var imperial = new ImperialUnitSystem();
        imperial.Should().NotBeNull();
    }

    [Fact]
    public void ImperialUnitSystem_Name_IsImperial()
    {
        var imperial = new ImperialUnitSystem();
        imperial.Name.Should().Be("Imperial");
    }

    [Fact]
    public void ImperialUnitSystem_BaseUnits_ContainsFoot()
    {
        var imperial = new ImperialUnitSystem();
        imperial.BaseUnits.Should().Contain(u => u.Symbol == "ft");
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_Foot()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("ft");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("foot");
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_Mile()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("mi");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("mile");
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_Pound()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("lb");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("pound");
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_Gallon()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("gal");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("gallon");
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_Fahrenheit()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("°F");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("degree Fahrenheit");
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_PoundForce()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("lbf");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("pound-force");
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_Horsepower()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("hp");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("horsepower");
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_BTU()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("BTU");
        unit.Should().NotBeNull();
        unit!.Name.Should().Be("British thermal unit");
    }

    [Fact]
    public void ImperialUnitSystem_Foot_HasAlias_Feet()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("ft");
        unit.Should().NotBeNull();
        unit!.Aliases.Should().Contain("feet");
    }

    [Fact]
    public void ImperialUnitSystem_Pound_HasAlias_Lbs()
    {
        var imperial = new ImperialUnitSystem();
        var unit = imperial.GetUnit("lb");
        unit.Should().NotBeNull();
        unit!.Aliases.Should().Contain("lbs");
    }

    [Fact]
    public void ImperialUnitSystem_GetByCategory_Length()
    {
        var imperial = new ImperialUnitSystem();
        var units = imperial.GetByCategory(UnitCategory.Length);
        units.Should().Contain(u => u.Symbol == "ft");
        units.Should().Contain(u => u.Symbol == "in");
        units.Should().Contain(u => u.Symbol == "yd");
        units.Should().Contain(u => u.Symbol == "mi");
    }

    [Fact]
    public void ImperialUnitSystem_Foot_ScaleFactor_IsMeters()
    {
        var imperial = new ImperialUnitSystem();
        var foot = imperial.GetUnit("ft");
        foot!.ScaleFactor.Should().Be(0.3048);
    }

    [Fact]
    public void ImperialUnitSystem_Mile_ScaleFactor_IsMeters()
    {
        var imperial = new ImperialUnitSystem();
        var mile = imperial.GetUnit("mi");
        mile!.ScaleFactor.Should().Be(1609.344);
    }

    [Fact]
    public void ImperialUnitSystem_GetAll_ReturnsAllUnits()
    {
        var imperial = new ImperialUnitSystem();
        var all = imperial.GetAll();
        all.Count.Should().BeGreaterThanOrEqualTo(14);
    }

    // ── CustomUnitSystem ──────────────────────────────────────────

    [Fact]
    public void CustomUnitSystem_Builder_WithUnit()
    {
        var unit = new Unit
        {
            Symbol = "foo",
            Name = "Foo",
            Category = UnitCategory.Other,
            Dimension = Dimension.None
        };
        var system = new CustomUnitSystem.Builder()
            .WithUnit(unit)
            .Build();
        system.Should().NotBeNull();
    }

    [Fact]
    public void CustomUnitSystem_Builder_Named()
    {
        var system = new CustomUnitSystem.Builder()
            .Named("MySystem")
            .Build();
        system.Should().NotBeNull();
    }

    [Fact]
    public void CustomUnitSystem_Builder_WithUnits()
    {
        var units = new[]
        {
            new Unit { Symbol = "a", Name = "A", Category = UnitCategory.Length, Dimension = Dimension.FromBaseDimensions(length: 1) },
            new Unit { Symbol = "b", Name = "B", Category = UnitCategory.Mass, Dimension = Dimension.FromBaseDimensions(mass: 1) }
        };
        var system = new CustomUnitSystem.Builder()
            .WithUnits(units)
            .Build();
        system.Should().NotBeNull();
    }

    [Fact]
    public void CustomUnitSystem_Default_IsSelf()
    {
        var system = new CustomUnitSystem.Builder().Build();
        system.Default.Should().BeSameAs(system);
    }

    [Fact]
    public void CustomUnitSystem_BaseUnits_ContainsRegisteredUnits()
    {
        var unit = new Unit { Symbol = "x", Name = "X", Category = UnitCategory.Other, Dimension = Dimension.None };
        var system = new CustomUnitSystem.Builder().WithUnit(unit).Build();
        system.BaseUnits.Should().Contain(u => u.Symbol == "x");
    }

    // ── UnitBuilder ───────────────────────────────────────────────

    [Fact]
    public void UnitBuilder_Build_DefaultValues()
    {
        var unit = new UnitBuilder().Build();
        unit.Symbol.Should().BeEmpty();
        unit.Name.Should().BeEmpty();
        unit.ScaleFactor.Should().Be(1.0);
    }

    [Fact]
    public void UnitBuilder_FluentChain()
    {
        var unit = new UnitBuilder()
            .WithSymbol("km")
            .WithName("Kilometer")
            .WithDimension(Dimension.FromBaseDimensions(length: 1))
            .WithCategory(UnitCategory.Length)
            .WithScaleFactor(1000.0)
            .WithAlias("kilometer")
            .Build();
        unit.Symbol.Should().Be("km");
        unit.Name.Should().Be("Kilometer");
        unit.Category.Should().Be(UnitCategory.Length);
        unit.ScaleFactor.Should().Be(1000.0);
        unit.Aliases.Should().Contain("kilometer");
    }

    [Fact]
    public void UnitBuilder_WithMultipleAliases()
    {
        var unit = new UnitBuilder()
            .WithSymbol("Ω")
            .WithAlias("Ohm")
            .WithAlias("ohm")
            .Build();
        unit.Aliases.Should().HaveCount(2);
    }

    [Fact]
    public void UnitBuilder_WithDimension()
    {
        var unit = new UnitBuilder()
            .WithDimension(DerivedDimension.Force)
            .Build();
        unit.Dimension.Should().Be(DerivedDimension.Force);
    }

    // ── UnitFormatter ─────────────────────────────────────────────

    [Fact]
    public void UnitFormatter_Instance_IsNotNull()
    {
        UnitFormatter.Instance.Should().NotBeNull();
    }

    [Fact]
    public void UnitFormatter_Instance_IsSingleton()
    {
        UnitFormatter.Instance.Should().BeSameAs(UnitFormatter.Instance);
    }

    [Fact]
    public void UnitFormatter_Format_ReturnsSymbol()
    {
        var unit = new Unit { Symbol = "m", Name = "Meter" };
        UnitFormatter.Instance.Format(unit).Should().Be("m");
    }

    [Fact]
    public void UnitFormatter_Format_EmptySymbol_ReturnsName()
    {
        var unit = new Unit { Symbol = "", Name = "Meter" };
        UnitFormatter.Instance.Format(unit).Should().Be("Meter");
    }

    [Fact]
    public void UnitFormatter_Format_Null_Throws()
    {
        Action act = () => UnitFormatter.Instance.Format(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnitFormatter_FormatWithPrefix()
    {
        var unit = new Unit { Symbol = "m" };
        var result = UnitFormatter.Instance.FormatWithPrefix(unit, UnitPrefixes.Kilo);
        result.Should().Be("km");
    }

    [Fact]
    public void UnitFormatter_FormatWithPrefix_NullUnit_Throws()
    {
        Action act = () => UnitFormatter.Instance.FormatWithPrefix(null!, UnitPrefixes.Kilo);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnitFormatter_FormatWithPrefix_NullPrefix_Throws()
    {
        var unit = new Unit { Symbol = "m" };
        Action act = () => UnitFormatter.Instance.FormatWithPrefix(unit, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnitFormatter_FormatQuantity()
    {
        var unit = new Unit { Symbol = "m" };
        var result = UnitFormatter.Instance.FormatQuantity(5.0, unit);
        result.Should().Be("5 m");
    }

    [Fact]
    public void UnitFormatter_FormatQuantity_NullUnit_Throws()
    {
        Action act = () => UnitFormatter.Instance.FormatQuantity(1.0, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnitPrefix_AllPrefixes_CountIs20()
    {
        UnitPrefixes.All().Should().HaveCount(20);
    }

    [Fact]
    public void UnitPrefix_Yotta_Through_Yocto_AllExist()
    {
        var all = UnitPrefixes.All();
        all.Should().Contain(p => p.Symbol == "Y" && p.Factor == 1e24);
        all.Should().Contain(p => p.Symbol == "Z" && p.Factor == 1e21);
        all.Should().Contain(p => p.Symbol == "E" && p.Factor == 1e18);
        all.Should().Contain(p => p.Symbol == "P" && p.Factor == 1e15);
        all.Should().Contain(p => p.Symbol == "T" && p.Factor == 1e12);
        all.Should().Contain(p => p.Symbol == "G" && p.Factor == 1e9);
        all.Should().Contain(p => p.Symbol == "M" && p.Factor == 1e6);
        all.Should().Contain(p => p.Symbol == "k" && p.Factor == 1e3);
        all.Should().Contain(p => p.Symbol == "h" && p.Factor == 1e2);
        all.Should().Contain(p => p.Symbol == "da" && p.Factor == 1e1);
        all.Should().Contain(p => p.Symbol == "d" && p.Factor == 1e-1);
        all.Should().Contain(p => p.Symbol == "c" && p.Factor == 1e-2);
        all.Should().Contain(p => p.Symbol == "m" && p.Factor == 1e-3);
        all.Should().Contain(p => p.Symbol == "\u03BC" && p.Factor == 1e-6);
        all.Should().Contain(p => p.Symbol == "n" && p.Factor == 1e-9);
        all.Should().Contain(p => p.Symbol == "p" && p.Factor == 1e-12);
        all.Should().Contain(p => p.Symbol == "f" && p.Factor == 1e-15);
        all.Should().Contain(p => p.Symbol == "a" && p.Factor == 1e-18);
        all.Should().Contain(p => p.Symbol == "z" && p.Factor == 1e-21);
        all.Should().Contain(p => p.Symbol == "y" && p.Factor == 1e-24);
    }

    [Fact]
    public void UnitPrefix_FromSymbol_ReturnsNullForUnknown()
    {
        UnitPrefixes.FromSymbol("xx").Should().BeNull();
    }

    [Fact]
    public void UnitPrefix_FromName_CaseInsensitive_VariousCases()
    {
        UnitPrefixes.FromName("KILO").Should().NotBeNull();
        UnitPrefixes.FromName("milli").Should().NotBeNull();
        UnitPrefixes.FromName("Micro").Should().NotBeNull();
    }

    [Fact]
    public void UnitPrefix_FromFactor_ExactMatch()
    {
        var prefix = UnitPrefixes.FromFactor(1e6);
        prefix.Should().NotBeNull();
        prefix!.Symbol.Should().Be("M");
    }

    [Fact]
    public void UnitPrefix_FromFactor_NoMatch_ReturnsNull()
    {
        UnitPrefixes.FromFactor(1.5).Should().BeNull();
    }

    [Fact]
    public void UnitPrefix_Equality_SameValues()
    {
        var a = new UnitPrefix { Symbol = "k", Name = "kilo", Factor = 1000 };
        var b = new UnitPrefix { Symbol = "k", Name = "kilo", Factor = 1000 };
        a.Should().Be(b);
    }

    [Fact]
    public void UnitPrefix_Equality_DifferentFactor()
    {
        var a = new UnitPrefix { Symbol = "k", Factor = 1000 };
        var b = new UnitPrefix { Symbol = "k", Factor = 100 };
        a.Should().NotBe(b);
    }

    [Fact]
    public void UnitCategory_AllValues()
    {
        var values = Enum.GetValues<UnitCategory>();
        values.Should().HaveCountGreaterThanOrEqualTo(20);
    }

    [Fact]
    public void UnitCategory_HasBaseQuantities()
    {
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Length);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Mass);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Time);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.ElectricCurrent);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Temperature);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.AmountOfSubstance);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.LuminousIntensity);
    }

    [Fact]
    public void UnitCategory_HasDerivedQuantities()
    {
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Frequency);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Force);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Energy);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Power);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Pressure);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Voltage);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Resistance);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Capacitance);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Inductance);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Area);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Volume);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Speed);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Acceleration);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Density);
    }

    [Fact]
    public void UnitRegistry_ThreadSafety_ConcurrentRegisters()
    {
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            var unit = new Unit
            {
                Symbol = $"test_{i}",
                Name = $"Test {i}",
                Category = UnitCategory.Other,
                Dimension = Dimension.None,
                ScaleFactor = i
            };
            UnitRegistry.Instance.Register(unit);
        })).ToArray();
        Task.WaitAll(tasks);
        UnitRegistry.Instance.Get("test_50").Should().NotBeNull();
    }

    [Fact]
    public void UnitRegistry_Register_WithMultipleAliases()
    {
        var unit = new Unit
        {
            Symbol = "aliased",
            Name = "Aliased Unit",
            Category = UnitCategory.Other,
            Dimension = Dimension.None,
            Aliases = ImmutableArray.Create("alias1", "alias2", "alias3")
        };
        UnitRegistry.Instance.Register(unit);
        UnitRegistry.Instance.Get("alias1").Should().NotBeNull();
        UnitRegistry.Instance.Get("alias2").Should().NotBeNull();
        UnitRegistry.Instance.Get("alias3").Should().NotBeNull();
    }

    [Fact]
    public void UnitRegistry_GetAll_ReturnsCopy()
    {
        var all1 = UnitRegistry.Instance.GetAll();
        var all2 = UnitRegistry.Instance.GetAll();
        all1.Should().NotBeSameAs(all2);
    }

    [Fact]
    public void SIUnitSystem_BaseUnits_ContainsAll7()
    {
        var baseUnits = SIUnitSystem.Instance.BaseUnits;
        baseUnits.Should().HaveCount(7);
        baseUnits.Should().Contain(u => u.Symbol == "m");
        baseUnits.Should().Contain(u => u.Symbol == "kg");
        baseUnits.Should().Contain(u => u.Symbol == "s");
        baseUnits.Should().Contain(u => u.Symbol == "A");
        baseUnits.Should().Contain(u => u.Symbol == "K");
        baseUnits.Should().Contain(u => u.Symbol == "mol");
        baseUnits.Should().Contain(u => u.Symbol == "cd");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_AllDerivedUnits()
    {
        SIUnitSystem.Instance.GetUnit("N").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("J").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("W").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Pa").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("C").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("V").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("F").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("\u03A9").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("S").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Wb").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("T").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("H").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Hz").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("lm").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("lx").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Bq").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Gy").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Sv").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("kat").Should().NotBeNull();
    }

    [Fact]
    public void SIUnitSystem_GetUnit_ByAlias()
    {
        SIUnitSystem.Instance.GetUnit("Newton").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Joule").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Watt").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Pascal").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Coulomb").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Volt").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Farad").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Ohm").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Siemens").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Weber").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Tesla").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Henry").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Hertz").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Lumen").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Lux").Should().NotBeNull();
    }

    [Fact]
    public void CGSUnitSystem_BaseUnits_Contains3()
    {
        CGSUnitSystem.Instance.BaseUnits.Should().HaveCount(3);
    }

    [Fact]
    public void CGSUnitSystem_GetUnit_DerivedUnits()
    {
        CGSUnitSystem.Instance.GetUnit("dyne").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("erg").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("gauss").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("maxwell").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("poise").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("stokes").Should().NotBeNull();
    }

    [Fact]
    public void ImperialUnitSystem_Foot_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("ft")!.ScaleFactor.Should().Be(0.3048);
    }

    [Fact]
    public void ImperialUnitSystem_Inch_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("in")!.ScaleFactor.Should().Be(0.0254);
    }

    [Fact]
    public void ImperialUnitSystem_Yard_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("yd")!.ScaleFactor.Should().Be(0.9144);
    }

    [Fact]
    public void ImperialUnitSystem_Mile_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("mi")!.ScaleFactor.Should().Be(1609.344);
    }

    [Fact]
    public void ImperialUnitSystem_Pound_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("lb")!.ScaleFactor.Should().Be(0.45359237);
    }

    [Fact]
    public void ImperialUnitSystem_Ounce_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("oz")!.ScaleFactor.Should().Be(0.028349523125);
    }

    [Fact]
    public void ImperialUnitSystem_Gallon_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("gal")!.ScaleFactor.Should().Be(0.003785411784);
    }

    [Fact]
    public void ImperialUnitSystem_Fahrenheit_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("\u00B0F")!.ScaleFactor.Should().Be(5.0 / 9.0);
    }

    [Fact]
    public void ImperialUnitSystem_Rankine_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("\u00B0R")!.ScaleFactor.Should().Be(5.0 / 9.0);
    }

    [Fact]
    public void ImperialUnitSystem_BTU_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("BTU")!.ScaleFactor.Should().Be(1055.05585262);
    }

    [Fact]
    public void ImperialUnitSystem_Horsepower_ScaleFactor()
    {
        var imperial = new ImperialUnitSystem();
        imperial.GetUnit("hp")!.ScaleFactor.Should().Be(745.699872);
    }

[Fact]
    public void CustomUnitSystem_Default_IsSelf_Builder()
    {
        var system = new CustomUnitSystem.Builder().Build();
        system.Default.Should().BeSameAs(system);
    }

    [Fact]
    public void CustomUnitSystem_Builder_MultipleUnits()
    {
        var u1 = new Unit { Symbol = "a", Name = "A", Category = UnitCategory.Length, Dimension = Dimension.FromBaseDimensions(length: 1) };
        var u2 = new Unit { Symbol = "b", Name = "B", Category = UnitCategory.Mass, Dimension = Dimension.FromBaseDimensions(mass: 1) };
        var system = new CustomUnitSystem.Builder().WithUnits(new[] { u1, u2 }).Build();
        system.BaseUnits.Should().HaveCount(2);
    }

    [Fact]
    public void UnitBuilder_WithDimensionless()
    {
        var unit = new UnitBuilder()
            .WithSymbol("rad")
            .WithName("Radian")
            .WithCategory(UnitCategory.Dimensionless)
            .WithDimension(Dimension.None)
            .Build();
        unit.Category.Should().Be(UnitCategory.Dimensionless);
        unit.Dimension.IsDimensionless.Should().BeTrue();
    }

    [Fact]
    public void UnitBuilder_WithDerivedDimension()
    {
        var unit = new UnitBuilder()
            .WithSymbol("N")
            .WithName("Newton")
            .WithCategory(UnitCategory.Force)
            .WithDimension(DerivedDimension.Force)
            .WithScaleFactor(1.0)
            .Build();
        unit.Dimension.Should().Be(DerivedDimension.Force);
    }

    [Fact]
    public void UnitBuilder_MultipleAliases()
    {
        var unit = new UnitBuilder()
            .WithSymbol("\u03A9")
            .WithAlias("Ohm")
            .WithAlias("ohm")
            .WithAlias("OHM")
            .Build();
        unit.Aliases.Should().HaveCount(3);
    }

    [Fact]
    public void UnitBuilder_WithZeroScaleFactor()
    {
        var unit = new UnitBuilder()
            .WithSymbol("zero")
            .WithScaleFactor(0.0)
            .Build();
        unit.ScaleFactor.Should().Be(0.0);
    }

    [Fact]
    public void UnitFormatter_FormatWithPrefix_VariousPrefixes()
    {
        var unit = new Unit { Symbol = "m" };
        UnitFormatter.Instance.FormatWithPrefix(unit, UnitPrefixes.Kilo).Should().Be("km");
        UnitFormatter.Instance.FormatWithPrefix(unit, UnitPrefixes.Milli).Should().Be("mm");
        UnitFormatter.Instance.FormatWithPrefix(unit, UnitPrefixes.Micro).Should().Be("\u03BCm");
        UnitFormatter.Instance.FormatWithPrefix(unit, UnitPrefixes.Nano).Should().Be("nm");
    }

    [Fact]
    public void UnitFormatter_FormatQuantity_VariousValues()
    {
        var unit = new Unit { Symbol = "m" };
        UnitFormatter.Instance.FormatQuantity(0, unit).Should().Be("0 m");
        UnitFormatter.Instance.FormatQuantity(-5.5, unit).Should().Be("-5.5 m");
        UnitFormatter.Instance.FormatQuantity(1e6, unit).Should().Be("1000000 m");
    }

    [Fact]
    public void Unit_Equality_DifferentScaleFactor()
    {
        var a = new Unit { Symbol = "m", ScaleFactor = 1.0 };
        var b = new Unit { Symbol = "m", ScaleFactor = 1000.0 };
        a.Should().NotBe(b);
    }

    [Fact]
    public void Unit_Equality_DifferentAliases()
    {
        var a = new Unit { Symbol = "m", Aliases = ImmutableArray.Create("meter") };
        var b = new Unit { Symbol = "m", Aliases = ImmutableArray.Create("metre") };
        a.Should().NotBe(b);
    }

    [Fact]
    public void Unit_Immutability_WithPrefixCreatesNew()
    {
        var original = new Unit { Symbol = "m", ScaleFactor = 1.0 };
        var prefixed = original.WithPrefix(UnitPrefixes.Kilo);
        original.Symbol.Should().Be("m");
        original.ScaleFactor.Should().Be(1.0);
        prefixed.Symbol.Should().Be("km");
        prefixed.ScaleFactor.Should().Be(1000.0);
    }

    [Fact]
    public void SIUnitSystem_Default_IsInstance_EndOfFile()
    {
        SIUnitSystem.Instance.Default.Should().BeSameAs(SIUnitSystem.Instance);
    }

    [Fact]
    public void UnitPrefix_AllStandardPrefixes()
    {
        UnitPrefixes.Yotta.Factor.Should().Be(1e24);
        UnitPrefixes.Zetta.Factor.Should().Be(1e21);
        UnitPrefixes.Exa.Factor.Should().Be(1e18);
        UnitPrefixes.Peta.Factor.Should().Be(1e15);
        UnitPrefixes.Tera.Factor.Should().Be(1e12);
        UnitPrefixes.Giga.Factor.Should().Be(1e9);
        UnitPrefixes.Mega.Factor.Should().Be(1e6);
        UnitPrefixes.Kilo.Factor.Should().Be(1e3);
        UnitPrefixes.Hecto.Factor.Should().Be(1e2);
        UnitPrefixes.Deca.Factor.Should().Be(1e1);
        UnitPrefixes.Deci.Factor.Should().Be(1e-1);
        UnitPrefixes.Centi.Factor.Should().Be(1e-2);
        UnitPrefixes.Milli.Factor.Should().Be(1e-3);
        UnitPrefixes.Micro.Factor.Should().Be(1e-6);
        UnitPrefixes.Nano.Factor.Should().Be(1e-9);
        UnitPrefixes.Pico.Factor.Should().Be(1e-12);
        UnitPrefixes.Femto.Factor.Should().Be(1e-15);
        UnitPrefixes.Atto.Factor.Should().Be(1e-18);
        UnitPrefixes.Zepto.Factor.Should().Be(1e-21);
        UnitPrefixes.Yocto.Factor.Should().Be(1e-24);
    }

    [Fact]
    public void UnitCategory_AllCategoriesPresent()
    {
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Length);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Mass);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Time);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.ElectricCurrent);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Temperature);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.AmountOfSubstance);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.LuminousIntensity);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Force);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Energy);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Power);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Pressure);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Frequency);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Voltage);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Resistance);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Capacitance);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.MagneticFlux);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.MagneticField);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Area);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Volume);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Density);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Angle);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Speed);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Acceleration);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Information);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Other);
        Enum.GetValues<UnitCategory>().Should().Contain(UnitCategory.Dimensionless);
    }

    [Fact]
    public void UnitRegistry_GetByDimension_ReturnsMatching()
    {
        var dim = new DimensionBuilder().Length().Build();
        var units = UnitRegistry.Instance.GetByDimension(dim);
        units.Should().Contain(u => u.Symbol == "m");
    }

    [Fact]
    public void SIUnitSystem_GetUnit_KnownUnits()
    {
        SIUnitSystem.Instance.GetUnit("m").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("kg").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("s").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("A").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("K").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("mol").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("cd").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("N").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("J").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("W").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Pa").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Hz").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("V").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("\u03A9").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("F").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("Wb").Should().NotBeNull();
        SIUnitSystem.Instance.GetUnit("T").Should().NotBeNull();
    }

    [Fact]
    public void CGSUnitSystem_GetUnit_KnownUnits()
    {
        CGSUnitSystem.Instance.GetUnit("cm").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("g").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("s").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("dyn").Should().NotBeNull();
        CGSUnitSystem.Instance.GetUnit("erg").Should().NotBeNull();
    }

    [Fact]
    public void ImperialUnitSystem_GetUnit_KnownUnits()
    {
        ImperialUnitSystem.Instance.GetUnit("ft").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("in").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("yd").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("mi").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("lb").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("oz").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("gal").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("qt").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("pt").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("\u00B0F").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("lbf").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("hp").Should().NotBeNull();
        ImperialUnitSystem.Instance.GetUnit("BTU").Should().NotBeNull();
    }

    [Fact]
    public void CustomUnitSystem_Builder_CreatesSystem()
    {
        var system = new CustomUnitSystem.Builder()
            .Named("TestSystem")
            .WithUnit(new Unit { Symbol = "test", Name = "Test Unit", Category = UnitCategory.Length, Dimension = new DimensionBuilder().Length().Build(), ScaleFactor = 1.0 })
            .Build();
        system.Name.Should().Be("TestSystem");
        system.GetUnit("test").Should().NotBeNull();
    }

    [Fact]
    public void UnitBuilder_CreatesUnitWithAllProperties()
    {
        var unit = new UnitBuilder()
            .Named("Test")
            .WithSymbol("tst")
            .WithCategory(UnitCategory.Length)
            .WithDimension(new DimensionBuilder().Length().Build())
            .WithScaleFactor(42.0)
            .WithAlias("alias1")
            .WithAlias("alias2")
            .Build();
        unit.Name.Should().Be("Test");
        unit.Symbol.Should().Be("tst");
        unit.Category.Should().Be(UnitCategory.Length);
        unit.ScaleFactor.Should().Be(42.0);
        unit.Aliases.Should().Contain("alias1");
        unit.Aliases.Should().Contain("alias2");
    }

    [Fact]
    public void UnitFormatter_FormatCompound_Various()
    {
        var units = new[] { new Unit { Symbol = "kg" }, new Unit { Symbol = "m" }, new Unit { Symbol = "s" } };
        var exps = new[] { 1.0, 1.0, -2.0 };
        UnitFormatter.Instance.FormatCompound(units, exps).Should().Be("kg m s^-2");
    }

    [Fact]
    public void UnitFormatter_FormatWithNames_Various()
    {
        var unit = new Unit { Symbol = "m", Name = "Meter" };
        UnitFormatter.Instance.FormatWithNames(unit).Should().Be("Meter");
    }

    [Fact]
    public void Unit_WithPrefix_Immutable()
    {
        var u = new Unit { Symbol = "m", ScaleFactor = 1.0 };
        var p = u.WithPrefix(UnitPrefixes.Kilo);
        u.Symbol.Should().Be("m");
        u.ScaleFactor.Should().Be(1.0);
        p.Symbol.Should().Be("km");
        p.ScaleFactor.Should().Be(1000.0);
    }

    [Fact]
    public void UnitRegistry_ThreadSafe()
    {
        Parallel.For(0, 100, _ => {
            UnitRegistry.Instance.Get("m").Should().NotBeNull();
            UnitRegistry.Instance.GetByCategory(UnitCategory.Length).Should().NotBeEmpty();
        });
    }

    [Fact]
    public void Unit_Equality_SameUnit()
    {
        var u1 = new Unit { Symbol = "m", ScaleFactor = 1.0 };
        var u2 = new Unit { Symbol = "m", ScaleFactor = 1.0 };
        u1.Should().Be(u2);
    }

    [Fact]
    public void Unit_HashCode_Stable()
    {
        var u = new Unit { Symbol = "m", ScaleFactor = 1.0 };
        u.GetHashCode().Should().Be(u.GetHashCode());
    }

    [Fact]
    public void UnitSystem_BaseUnits_ReturnsBaseUnits()
    {
        SIUnitSystem.Instance.BaseUnits.Should().HaveCount(7);
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "m");
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "kg");
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "s");
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "A");
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "K");
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "mol");
        SIUnitSystem.Instance.BaseUnits.Should().Contain(u => u.Symbol == "cd");
    }

    [Fact]
    public void UnitPrefix_SymbolAndName()
    {
        UnitPrefixes.Kilo.Symbol.Should().Be("k");
        UnitPrefixes.Kilo.Name.Should().Be("Kilo");
        UnitPrefixes.Milli.Symbol.Should().Be("m");
        UnitPrefixes.Milli.Name.Should().Be("Milli");
    }

    [Fact]
    public void UnitCategory_Dimensionless_Exists()
    {
        Enum.IsDefined(typeof(UnitCategory), UnitCategory.Dimensionless).Should().BeTrue();
    }

    [Fact]
    public void UnitFormatter_FormatQuantity_WithPrefix()
    {
        var unit = new Unit { Symbol = "m" };
        UnitFormatter.Instance.FormatQuantity(1000.0, unit, UnitPrefixes.Kilo).Should().Be("1 km");
    }
}
