namespace MathVerse.Foundation.Tests;

public sealed class FoundationOptionsTests
{
    [Fact]
    public void Default_EnableDimensionChecking_IsTrue()
    {
        new FoundationOptions().EnableDimensionChecking.Should().BeTrue();
    }

    [Fact]
    public void Default_EnableAutoConversion_IsFalse()
    {
        new FoundationOptions().EnableAutoConversion.Should().BeFalse();
    }

    [Fact]
    public void Default_DefaultUnitSystem_IsSI()
    {
        new FoundationOptions().DefaultUnitSystem.Should().Be("SI");
    }

    [Fact]
    public void Default_MaxConversionPathLength_Is5()
    {
        new FoundationOptions().MaxConversionPathLength.Should().Be(5);
    }

    [Fact]
    public void Default_EnableConstantCaching_IsTrue()
    {
        new FoundationOptions().EnableConstantCaching.Should().BeTrue();
    }

    [Fact]
    public void CustomOptions_SetAllProperties()
    {
        var opts = new FoundationOptions
        {
            EnableDimensionChecking = false,
            EnableAutoConversion = true,
            DefaultUnitSystem = "CGS",
            MaxConversionPathLength = 10,
            EnableConstantCaching = false
        };
        opts.EnableDimensionChecking.Should().BeFalse();
        opts.EnableAutoConversion.Should().BeTrue();
        opts.DefaultUnitSystem.Should().Be("CGS");
        opts.MaxConversionPathLength.Should().Be(10);
        opts.EnableConstantCaching.Should().BeFalse();
    }

    [Fact]
    public void Options_IsRecord_SupportsWithExpression()
    {
        var original = new FoundationOptions { EnableDimensionChecking = true };
        var modified = original with { EnableDimensionChecking = false };
        modified.EnableDimensionChecking.Should().BeFalse();
        original.EnableDimensionChecking.Should().BeTrue();
    }
}

public sealed class FoundationConfigurationTests
{
    [Fact]
    public void Builder_DefaultBuild_CorrectOptions()
    {
        var opts = new FoundationConfiguration().Build();
        opts.EnableDimensionChecking.Should().BeTrue();
        opts.EnableAutoConversion.Should().BeFalse();
        opts.DefaultUnitSystem.Should().Be("SI");
        opts.MaxConversionPathLength.Should().Be(5);
        opts.EnableConstantCaching.Should().BeTrue();
    }

    [Fact]
    public void EnableDimensionChecking_ReturnsSelf()
    {
        var config = new FoundationConfiguration();
        config.EnableDimensionChecking(false).Should().BeSameAs(config);
    }

    [Fact]
    public void EnableDimensionChecking_False_BuildsFalse()
    {
        var opts = new FoundationConfiguration().EnableDimensionChecking(false).Build();
        opts.EnableDimensionChecking.Should().BeFalse();
    }

    [Fact]
    public void EnableAutoConversion_ReturnsSelf()
    {
        var config = new FoundationConfiguration();
        config.EnableAutoConversion(true).Should().BeSameAs(config);
    }

    [Fact]
    public void EnableAutoConversion_True_BuildsTrue()
    {
        var opts = new FoundationConfiguration().EnableAutoConversion(true).Build();
        opts.EnableAutoConversion.Should().BeTrue();
    }

    [Fact]
    public void WithDefaultUnitSystem_ReturnsSelf()
    {
        var config = new FoundationConfiguration();
        config.WithDefaultUnitSystem("CGS").Should().BeSameAs(config);
    }

    [Fact]
    public void WithDefaultUnitSystem_BuildsCorrectValue()
    {
        var opts = new FoundationConfiguration().WithDefaultUnitSystem("Imperial").Build();
        opts.DefaultUnitSystem.Should().Be("Imperial");
    }

    [Fact]
    public void WithMaxConversionPathLength_ReturnsSelf()
    {
        var config = new FoundationConfiguration();
        config.WithMaxConversionPathLength(10).Should().BeSameAs(config);
    }

    [Fact]
    public void WithMaxConversionPathLength_BuildsCorrectValue()
    {
        var opts = new FoundationConfiguration().WithMaxConversionPathLength(20).Build();
        opts.MaxConversionPathLength.Should().Be(20);
    }

    [Fact]
    public void EnableConstantCaching_ReturnsSelf()
    {
        var config = new FoundationConfiguration();
        config.EnableConstantCaching(false).Should().BeSameAs(config);
    }

    [Fact]
    public void EnableConstantCaching_False_BuildsFalse()
    {
        var opts = new FoundationConfiguration().EnableConstantCaching(false).Build();
        opts.EnableConstantCaching.Should().BeFalse();
    }

    [Fact]
    public void FluentChaining_AllMethodsTogether()
    {
        var opts = new FoundationConfiguration()
            .EnableDimensionChecking(false)
            .EnableAutoConversion(true)
            .WithDefaultUnitSystem("CGS")
            .WithMaxConversionPathLength(15)
            .EnableConstantCaching(false)
            .Build();
        opts.EnableDimensionChecking.Should().BeFalse();
        opts.EnableAutoConversion.Should().BeTrue();
        opts.DefaultUnitSystem.Should().Be("CGS");
        opts.MaxConversionPathLength.Should().Be(15);
        opts.EnableConstantCaching.Should().BeFalse();
    }
}

public sealed class FoundationServicesTests
{
    [Fact]
    public void Constructor_NullOptions_CreatesDefaultServices()
    {
        var services = new FoundationServices(null);
        services.Domains.Should().NotBeNull();
        services.Constants.Should().NotBeNull();
        services.Units.Should().NotBeNull();
        services.Conversions.Should().NotBeNull();
        services.DimensionAnalysis.Should().NotBeNull();
        services.UnitConversion.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithOptions_CreatesServices()
    {
        var opts = new FoundationOptions { EnableConstantCaching = true };
        var services = new FoundationServices(opts);
        services.Domains.Should().NotBeNull();
        services.Constants.Should().NotBeNull();
    }

    [Fact]
    public void Domains_IsNotNull()
    {
        new FoundationServices().Domains.Should().NotBeNull();
    }

    [Fact]
    public void Constants_IsNotNull()
    {
        new FoundationServices().Constants.Should().NotBeNull();
    }

    [Fact]
    public void Units_IsNotNull()
    {
        new FoundationServices().Units.Should().NotBeNull();
    }

    [Fact]
    public void Conversions_IsNotNull()
    {
        new FoundationServices().Conversions.Should().NotBeNull();
    }

    [Fact]
    public void DimensionAnalysis_IsNotNull()
    {
        new FoundationServices().DimensionAnalysis.Should().NotBeNull();
    }

    [Fact]
    public void UnitConversion_IsNotNull()
    {
        new FoundationServices().UnitConversion.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_RegistersBuiltinConstants()
    {
        var services = new FoundationServices();
        services.Constants.Get("pi").Should().NotBeNull();
    }
}

[Collection("DimensionAnalyzer")]
public sealed class FoundationEngineTests : IDisposable
{
    private readonly FoundationEngine _engine;

    public FoundationEngineTests()
    {
        _engine = new FoundationEngine();
        _engine.Clear();
    }

    public void Dispose()
    {
        _engine.Clear();
    }

    [Fact]
    public void Constructor_Default_CreatesEngine()
    {
        var engine = new FoundationEngine();
        engine.Services.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        Action act = () => new FoundationEngine(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithOptions_CreatesEngine()
    {
        var engine = new FoundationEngine(new FoundationOptions { EnableAutoConversion = true });
        engine.Services.Should().NotBeNull();
    }

    [Fact]
    public void Services_Property_ReturnsFoundationServices()
    {
        _engine.Services.Should().BeOfType<FoundationServices>();
    }

    [Fact]
    public void GetDomain_ByKind_ReturnsDomain()
    {
        var domain = _engine.GetDomain(DomainKind.Real);
        domain.Should().NotBeNull();
        domain!.Name.Should().Be("Real");
    }

    [Fact]
    public void GetDomain_ByKindNone_ReturnsNull()
    {
        var domain = _engine.GetDomain(DomainKind.None);
        domain.Should().BeNull();
    }

    [Fact]
    public void GetDomain_ByName_ReturnsDomain()
    {
        var domain = _engine.GetDomain("Real");
        domain.Should().NotBeNull();
    }

    [Fact]
    public void GetDomain_ByName_CaseInsensitive()
    {
        var domain = _engine.GetDomain("real");
        domain.Should().NotBeNull();
    }

    [Fact]
    public void GetDomain_UnknownName_ReturnsNull()
    {
        var domain = _engine.GetDomain("Nonexistent");
        domain.Should().BeNull();
    }

    [Fact]
    public void GetDomain_NullName_Throws()
    {
        Action act = () => _engine.GetDomain((string)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AreDomainsCompatible_SameDomain_ReturnsTrue()
    {
        var real = _engine.GetDomain(DomainKind.Real)!;
        _engine.AreDomainsCompatible(real, real).Should().BeTrue();
    }

    [Fact]
    public void AreDomainsCompatible_ComplexAndReal_ReturnsTrue()
    {
        var complex = _engine.GetDomain(DomainKind.Complex)!;
        var real = _engine.GetDomain(DomainKind.Real)!;
        _engine.AreDomainsCompatible(complex, real).Should().BeTrue();
    }

    [Fact]
    public void AreDomainsCompatible_NullLeft_Throws()
    {
        Action act = () => _engine.AreDomainsCompatible(null!, _engine.GetDomain(DomainKind.Real)!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AreDomainsCompatible_NullRight_Throws()
    {
        Action act = () => _engine.AreDomainsCompatible(_engine.GetDomain(DomainKind.Real)!, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetConstant_Pi_ReturnsPi()
    {
        var pi = _engine.GetConstant("pi");
        pi.Should().NotBeNull();
        pi!.NumericValue.Should().Be(System.Math.PI);
    }

    [Fact]
    public void GetConstant_E_ReturnsE()
    {
        var e = _engine.GetConstant("e");
        e.Should().NotBeNull();
        e!.NumericValue.Should().Be(System.Math.E);
    }

    [Fact]
    public void GetConstant_Unknown_ReturnsNull()
    {
        _engine.GetConstant("unknown_constant").Should().BeNull();
    }

    [Fact]
    public void GetConstantValue_Pi_ReturnsCorrectValue()
    {
        _engine.GetConstantValue("pi").Should().Be(System.Math.PI);
    }

    [Fact]
    public void GetConstantValue_Unknown_Throws()
    {
        Action act = () => _engine.GetConstantValue("nonexistent");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryGetConstant_Pi_ReturnsTrue()
    {
        _engine.TryGetConstant("pi", out var value).Should().BeTrue();
        value.Should().Be(System.Math.PI);
    }

    [Fact]
    public void TryGetConstant_Unknown_ReturnsFalse()
    {
        _engine.TryGetConstant("nonexistent", out var value).Should().BeFalse();
        value.Should().Be(0.0);
    }

    [Fact]
    public void GetUnit_Meter_ReturnsUnit()
    {
        var unit = _engine.GetUnit("m");
        unit.Should().NotBeNull();
        unit!.Symbol.Should().Be("m");
    }

    [Fact]
    public void GetUnit_Unknown_ReturnsNull()
    {
        _engine.GetUnit("unknown_unit").Should().BeNull();
    }

    [Fact]
    public void GetUnitsByCategory_Length_ReturnsUnits()
    {
        var units = _engine.GetUnitsByCategory(UnitCategory.Length);
        units.Should().NotBeEmpty();
    }

    [Fact]
    public void GetUnitsByDimension_Length_ReturnsUnits()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var units = _engine.GetUnitsByDimension(length);
        units.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateQuantity_MeterUnit_CreatesQuantity()
    {
        var pq = _engine.CreateQuantity(5.0, "m");
        pq.Value.Should().Be(5.0);
        pq.Unit.Should().NotBeNull();
    }

    [Fact]
    public void CreateQuantity_UnknownUnit_Throws()
    {
        Action act = () => _engine.CreateQuantity(1.0, "unknown");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AnalyzeExpression_Literal_ReturnsNone()
    {
        _engine.AnalyzeExpression(Expr.Literal(42)).Should().Be(Dimension.None);
    }

    [Fact]
    public void AnalyzeExpression_Null_Throws()
    {
        Action act = () => _engine.AnalyzeExpression(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CheckConsistency_LiteralExpr_ReturnsTrue()
    {
        _engine.CheckConsistency(Expr.Literal(5)).Should().BeTrue();
    }

    [Fact]
    public void CheckConsistency_NullExpr_Throws()
    {
        Action act = () => _engine.CheckConsistency(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CheckConsistency_CompatibleAddition_ReturnsTrue()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        _engine.Clear();
        _engine.Services.DimensionAnalysis.SetVariableDimension("x", dim);
        _engine.Services.DimensionAnalysis.SetVariableDimension("y", dim);
        _engine.CheckConsistency(Expr.Add(Expr.Variable("x"), Expr.Variable("y"))).Should().BeTrue();
    }

    [Fact]
    public void GetDiagnostics_IncompatibleAddition_ReturnsDiagnostics()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var mass = Dimension.FromBaseDimensions(mass: 1);
        _engine.Clear();
        _engine.Services.DimensionAnalysis.SetVariableDimension("x", length);
        _engine.Services.DimensionAnalysis.SetVariableDimension("y", mass);
        var diagnostics = _engine.GetDiagnostics(Expr.Add(Expr.Variable("x"), Expr.Variable("y")));
        diagnostics.Should().NotBeEmpty();
    }

    [Fact]
    public void GetDiagnostics_NullExpr_Throws()
    {
        Action act = () => _engine.GetDiagnostics(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CanConvert_NoRules_ReturnsFalse()
    {
        _engine.CanConvert("m", "m").Should().BeFalse();
    }

    [Fact]
    public void WithDimensions_NullExpr_Throws()
    {
        Action act = () => _engine.WithDimensions(null!, new Dictionary<string, Dimension>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithDimensions_NullVars_Throws()
    {
        Action act = () => _engine.WithDimensions(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithDimensions_ReturnsSameExpression()
    {
        var expr = Expr.Variable("x");
        var result = _engine.WithDimensions(expr, new Dictionary<string, Dimension> { ["x"] = Dimension.None });
        result.Should().BeSameAs(expr);
    }

    [Fact]
    public void EvaluateAsQuantity_NullExpr_Throws()
    {
        Action act = () => _engine.EvaluateAsQuantity(null!, new Dictionary<string, PhysicalQuantity>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EvaluateAsQuantity_NullVars_Throws()
    {
        Action act = () => _engine.EvaluateAsQuantity(Expr.Literal(1), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EvaluateAsQuantity_Literal_ReturnsQuantity()
    {
        var result = _engine.EvaluateAsQuantity(Expr.Literal(42), new Dictionary<string, PhysicalQuantity>());
        result.Should().NotBeNull();
        result!.Value.Should().Be(42);
    }

    [Fact]
    public void Clear_ResetsAnalyzer()
    {
        var uniqueKey = $"__clear_test_{Guid.NewGuid():N}__";
        _engine.Services.DimensionAnalysis.SetVariableDimension(uniqueKey, Dimension.FromBaseDimensions(length: 1));
        _engine.Clear();
        _engine.Services.DimensionAnalysis.GetVariableDimension(uniqueKey).Should().Be(Dimension.None);
    }

    [Fact]
    public void GetDomain_BooleanKind_ReturnsBoolean()
    {
        var domain = _engine.GetDomain(DomainKind.Boolean);
        domain.Should().NotBeNull();
        domain!.Name.Should().Be("Boolean");
    }

    [Fact]
    public void GetDomain_IntegerKind_ReturnsInteger()
    {
        var domain = _engine.GetDomain(DomainKind.Integer);
        domain.Should().NotBeNull();
        domain!.Name.Should().Be("Integer");
    }

    [Fact]
    public void GetConstant_Tau_ReturnsTau()
    {
        var tau = _engine.GetConstant("tau");
        tau.Should().NotBeNull();
        tau!.NumericValue.Should().Be(System.Math.Tau);
    }

    [Fact]
    public void GetConstant_Phi_ReturnsPhi()
    {
        var phi = _engine.GetConstant("phi");
        phi.Should().NotBeNull();
        phi!.NumericValue.Should().BeApproximately(1.618, 0.01);
    }

    [Fact]
    public void GetUnit_Kilogram_ReturnsUnit()
    {
        var unit = _engine.GetUnit("kg");
        unit.Should().NotBeNull();
        unit!.Symbol.Should().Be("kg");
    }

    [Fact]
    public void GetUnit_Second_ReturnsUnit()
    {
        var unit = _engine.GetUnit("s");
        unit.Should().NotBeNull();
        unit!.Symbol.Should().Be("s");
    }

    [Fact]
    public void GetUnit_Newton_ReturnsNull_NotInRegistry()
    {
        var unit = _engine.GetUnit("N");
        unit.Should().BeNull();
    }

    [Fact]
    public void AnalyzeExpression_VariableWithDimension_ReturnsDimension()
    {
        var dim = Dimension.FromBaseDimensions(length: 1);
        _engine.Services.DimensionAnalysis.SetVariableDimension("x", dim);
        _engine.AnalyzeExpression(Expr.Variable("x")).Should().Be(dim);
    }

    [Fact]
    public void CheckConsistency_NullVariablesCompatible_ReturnsTrue()
    {
        _engine.CheckConsistency(Expr.Literal(1.0)).Should().BeTrue();
    }

    [Fact]
    public void EvaluateAsQuantity_MissingVariable_ReturnsNull()
    {
        var result = _engine.EvaluateAsQuantity(
            Expr.Variable("missing"), new Dictionary<string, PhysicalQuantity>());
        result.Should().BeNull();
    }

    [Fact]
    public void GetUnitsByDimension_Derived_ReturnsUnitsOrEmpty()
    {
        var length = Dimension.FromBaseDimensions(length: 1);
        var units = _engine.GetUnitsByDimension(length);
        units.Should().NotBeEmpty();
    }

    [Fact]
    public void GetConstantValue_Gamma_ReturnsValue()
    {
        var gamma = _engine.GetConstant("gamma");
        gamma.Should().NotBeNull();
        gamma!.NumericValue.Should().BeApproximately(0.5772, 0.001);
    }

    [Fact]
    public void FoundationEngine_WithCustomOptions_UsesOptions()
    {
        var opts = new FoundationConfiguration()
            .EnableDimensionChecking(false)
            .EnableAutoConversion(true)
            .Build();
        var engine = new FoundationEngine(opts);
        engine.Options.EnableDimensionChecking.Should().BeFalse();
        engine.Options.EnableAutoConversion.Should().BeTrue();
    }

    [Fact]
    public void FoundationServices_ContainsAllRegistries()
    {
        var services = new FoundationServices();
        services.Domains.Should().NotBeNull();
        services.Constants.Should().NotBeNull();
        services.Units.Should().NotBeNull();
        services.Conversions.Should().NotBeNull();
        services.DimensionAnalysis.Should().NotBeNull();
        services.UnitConversion.Should().NotBeNull();
    }

    [Fact]
    public void FoundationConfiguration_FluentChaining()
    {
        var opts = new FoundationConfiguration()
            .EnableDimensionChecking(true)
            .EnableAutoConversion(false)
            .WithDefaultUnitSystem("CGS")
            .WithMaxConversionPathLength(10)
            .Build();
        opts.EnableDimensionChecking.Should().BeTrue();
        opts.EnableAutoConversion.Should().BeFalse();
        opts.DefaultUnitSystem.Should().Be("CGS");
        opts.MaxConversionPathLength.Should().Be(10);
    }

    [Fact]
    public void FoundationOptions_DefaultValues()
    {
        var opts = new FoundationOptions();
        opts.EnableDimensionChecking.Should().BeTrue();
        opts.EnableAutoConversion.Should().BeFalse();
        opts.DefaultUnitSystem.Should().Be("SI");
        opts.MaxConversionPathLength.Should().Be(5);
        opts.EnableConstantCaching.Should().BeTrue();
    }

    [Fact]
    public void FoundationEngine_GetConstantValue_Direct()
    {
        _engine.GetConstantValue("pi").Should().BeApproximately(Math.PI, 1e-10);
        _engine.GetConstantValue("e").Should().BeApproximately(Math.E, 1e-10);
    }

    [Fact]
    public void FoundationEngine_GetUnit_AllSIUnits()
    {
        _engine.GetUnit("m").Should().NotBeNull();
        _engine.GetUnit("kg").Should().NotBeNull();
        _engine.GetUnit("s").Should().NotBeNull();
        _engine.GetUnit("N").Should().NotBeNull();
        _engine.GetUnit("J").Should().NotBeNull();
        _engine.GetUnit("W").Should().NotBeNull();
        _engine.GetUnit("Pa").Should().NotBeNull();
        _engine.GetUnit("V").Should().NotBeNull();
    }

    [Fact]
    public void FoundationEngine_GetUnitsByCategory_Time()
    {
        var units = _engine.GetUnitsByCategory(UnitCategory.Time);
        units.Should().Contain(u => u.Symbol == "s");
        units.Should().Contain(u => u.Symbol == "ms");
    }

    [Fact]
    public void FoundationEngine_CreateQuantity_WithSymbol()
    {
        var q = _engine.CreateQuantity(5.0, "m");
        q.Value.Should().Be(5.0);
        q.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void FoundationEngine_ConvertQuantity_ToDifferentUnit()
    {
        var q = _engine.CreateQuantity(1.0, "km");
        var converted = _engine.Convert(q, "m");
        converted.Value.Should().Be(1000.0);
        converted.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void FoundationEngine_CheckConsistency_ValidExpression()
    {
        _engine.CheckConsistency(Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0))).Should().BeTrue();
    }

    [Fact]
    public void FoundationEngine_GetDiagnostics_EmptyForValid()
    {
        var diags = _engine.GetDiagnostics(Expr.Literal(1.0));
        diags.Should().BeEmpty();
    }

    [Fact]
    public void FoundationEngine_Convert_SameUnit()
    {
        _engine.Convert(5.0, "m", "m").Should().Be(5.0);
    }

    [Fact]
    public void FoundationEngine_Convert_DifferentCompatibleUnits()
    {
        _engine.Convert(1.0, "km", "m").Should().Be(1000.0);
    }

    [Fact]
    public void FoundationEngine_WithDimensions_AnnotatesExpression()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        var annotated = _engine.WithDimensions(expr, new Dictionary<string, Dimension> { ["x"] = Dimension.FromBaseDimensions(length: 1) });
        annotated.Should().NotBeNull();
    }

    [Fact]
    public void FoundationEngine_EvaluateAsQuantity_Valid()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        var vars = new Dictionary<string, PhysicalQuantity>
        {
            ["x"] = _engine.CreateQuantity(2.0, "m")
        };
        var result = _engine.EvaluateAsQuantity(expr, vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(3.0);
        result.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void FoundationEngine_EvaluateAsQuantity_MixedUnits()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        var vars = new Dictionary<string, PhysicalQuantity>
        {
            ["x"] = _engine.CreateQuantity(1.0, "m"),
            ["y"] = _engine.CreateQuantity(100.0, "cm")
        };
        var result = _engine.EvaluateAsQuantity(expr, vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(2.0);
    }

    [Fact]
    public void FoundationEngine_Clear_ResetsServices()
    {
        _engine.Services.DimensionAnalysis.SetVariableDimension("x", Dimension.FromBaseDimensions(length: 1));
        _engine.Clear();
        _engine.Services.DimensionAnalysis.GetVariableDimension("x").Should().Be(Dimension.None);
    }

    [Fact]
    public void FoundationEngine_AreDomainsCompatible_RealAndInteger()
    {
        _engine.AreDomainsCompatible(_engine.GetDomain(DomainKind.Real)!, _engine.GetDomain(DomainKind.Integer)!).Should().BeTrue();
    }

    [Fact]
    public void FoundationEngine_AreDomainsCompatible_RealAndComplex()
    {
        _engine.AreDomainsCompatible(_engine.GetDomain(DomainKind.Real)!, _engine.GetDomain(DomainKind.Complex)!).Should().BeTrue();
    }

    [Fact]
    public void FoundationEngine_GetConstant_WithAlias()
    {
        _engine.GetConstant("\u03C0").Should().NotBeNull();
        _engine.GetConstant("\u03C4").Should().NotBeNull();
    }

    [Fact]
    public void FoundationEngine_GetConstantValue_Tau()
    {
        _engine.GetConstantValue("tau").Should().BeApproximately(2 * Math.PI, 1e-10);
    }

    [Fact]
    public void FoundationEngine_GetDomain_ByName()
    {
        _engine.GetDomain("Real").Should().NotBeNull();
        _engine.GetDomain("Integer").Should().NotBeNull();
        _engine.GetDomain("Complex").Should().NotBeNull();
    }

    [Fact]
    public void FoundationEngine_AreDomainsCompatible_AllPairs()
    {
        var real = _engine.GetDomain(DomainKind.Real)!;
        var int_ = _engine.GetDomain(DomainKind.Integer)!;
        var complex = _engine.GetDomain(DomainKind.Complex)!;
        _engine.AreDomainsCompatible(real, int_).Should().BeTrue();
        _engine.AreDomainsCompatible(real, complex).Should().BeTrue();
        _engine.AreDomainsCompatible(int_, complex).Should().BeTrue();
    }

    [Fact]
    public void FoundationEngine_GetUnit_AllCategories()
    {
        _engine.GetUnit("m").Should().NotBeNull();
        _engine.GetUnit("kg").Should().NotBeNull();
        _engine.GetUnit("s").Should().NotBeNull();
        _engine.GetUnit("A").Should().NotBeNull();
        _engine.GetUnit("K").Should().NotBeNull();
        _engine.GetUnit("mol").Should().NotBeNull();
        _engine.GetUnit("cd").Should().NotBeNull();
        _engine.GetUnit("N").Should().NotBeNull();
        _engine.GetUnit("J").Should().NotBeNull();
        _engine.GetUnit("W").Should().NotBeNull();
    }

    [Fact]
    public void FoundationEngine_CreateQuantity()
    {
        var q = _engine.CreateQuantity(5.0, "m");
        q.Value.Should().Be(5.0);
        q.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void FoundationEngine_ConvertQuantity()
    {
        var q = _engine.CreateQuantity(1.0, "km");
        var converted = _engine.Convert(q, _engine.GetUnit("m")!);
        converted.Value.Should().Be(1000.0);
        converted.Unit.Symbol.Should().Be("m");
    }

    [Fact]
    public void FoundationEngine_AnalyzeExpression_Variable()
    {
        var expr = Expr.Variable("x");
        _engine.Services.DimensionAnalysis.SetVariableDimension("x", Meter.Dimension);
        var result = _engine.AnalyzeExpression(expr);
        result.Should().Be(Meter.Dimension);
        _engine.Services.DimensionAnalysis.Clear();
    }

    [Fact]
    public void FoundationEngine_CheckConsistency_Valid()
    {
        var expr = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        _engine.CheckConsistency(expr).Should().BeTrue();
    }

    [Fact]
    public void FoundationEngine_CheckConsistency_Invalid()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        _engine.Services.DimensionAnalysis.SetVariableDimension("x", Meter.Dimension);
        _engine.Services.DimensionAnalysis.SetVariableDimension("y", Kilogram.Dimension);
        _engine.CheckConsistency(expr).Should().BeFalse();
        _engine.Services.DimensionAnalysis.Clear();
    }

    [Fact]
    public void FoundationEngine_GetDiagnostics()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        _engine.Services.DimensionAnalysis.SetVariableDimension("x", Meter.Dimension);
        _engine.Services.DimensionAnalysis.SetVariableDimension("y", Kilogram.Dimension);
        var diags = _engine.GetDiagnostics(expr);
        diags.Should().NotBeEmpty();
        _engine.Services.DimensionAnalysis.Clear();
    }

    [Fact]
    public void FoundationEngine_Convert_StringSymbols()
    {
        var result = _engine.Convert(1.0, "m", "cm");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(100.0);
    }

    [Fact]
    public void FoundationEngine_Convert_Failure()
    {
        var result = _engine.Convert(1.0, "m", "kg");
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void FoundationEngine_WithDimensions()
    {
        var expr = Expr.Variable("x");
        var dims = new Dictionary<string, Dimension> { ["x"] = Meter.Dimension };
        var annotated = _engine.WithDimensions(expr, dims);
        annotated.Should().NotBeNull();
    }

    [Fact]
    public void FoundationEngine_EvaluateAsQuantity()
    {
        var expr = Expr.Literal(42.0);
        var vars = new Dictionary<string, PhysicalQuantity>();
        var result = _engine.EvaluateAsQuantity(expr, vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(42.0);
    }

    [Fact]
    public void FoundationEngine_EvaluateAsQuantity_WithVariables()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Literal(10));
        var vars = new Dictionary<string, PhysicalQuantity>
        {
            ["x"] = new PhysicalQuantity { Value = 5.0, Unit = Meter, Dimension = Meter.Dimension }
        };
        var result = _engine.EvaluateAsQuantity(expr, vars);
        result.Should().NotBeNull();
        result!.Value.Should().Be(15.0);
    }

    [Fact]
    public void FoundationEngine_Services_HasAllRegistries()
    {
        _engine.Services.Domains.Should().NotBeNull();
        _engine.Services.Constants.Should().NotBeNull();
        _engine.Services.Units.Should().NotBeNull();
        _engine.Services.Conversions.Should().NotBeNull();
        _engine.Services.DimensionAnalysis.Should().NotBeNull();
        _engine.Services.UnitConversion.Should().NotBeNull();
    }

    [Fact]
    public void FoundationEngine_Clear_ResetsAll()
    {
        _engine.Services.DimensionAnalysis.SetVariableDimension("x", Meter.Dimension);
        _engine.Clear();
        _engine.Services.DimensionAnalysis.GetVariableDimension("x").Should().Be(Dimension.None);
    }

    [Fact]
    public void FoundationEngine_ThreadSafe()
    {
        Parallel.For(0, 50, _ => {
            var q = _engine.CreateQuantity(1.0, "m");
            var converted = _engine.Convert(q, _engine.GetUnit("cm")!);
            converted.Value.Should().Be(100.0);
        });
    }

    [Fact]
    public void FoundationOptions_Defaults()
    {
        var opts = new FoundationOptions();
        opts.EnableDimensionChecking.Should().BeTrue();
        opts.EnableAutoConversion.Should().BeFalse();
        opts.DefaultUnitSystem.Should().Be("SI");
        opts.MaxConversionPathLength.Should().Be(5);
        opts.EnableConstantCaching.Should().BeTrue();
    }

    [Fact]
    public void FoundationConfiguration_Build_MultipleCalls()
    {
        var config = new FoundationConfiguration();
        config.Build().Should().NotBeNull();
        config.Build().Should().NotBeNull();
    }

    [Fact]
    public void FoundationServices_Constructor_WithOptions()
    {
        var opts = new FoundationOptions { DefaultUnitSystem = "CGS" };
        var services = new FoundationServices(opts);
        services.Units.Get("cm").Should().NotBeNull();
    }

    [Fact]
    public void FoundationConfiguration_EnableDimensionChecking()
    {
        var opts = new FoundationConfiguration().EnableDimensionChecking(false).Build();
        opts.EnableDimensionChecking.Should().BeFalse();
    }

    [Fact]
    public void FoundationConfiguration_EnableAutoConversion()
    {
        var opts = new FoundationConfiguration().EnableAutoConversion(true).Build();
        opts.EnableAutoConversion.Should().BeTrue();
    }

    [Fact]
    public void FoundationConfiguration_WithDefaultUnitSystem()
    {
        var opts = new FoundationConfiguration().WithDefaultUnitSystem("Imperial").Build();
        opts.DefaultUnitSystem.Should().Be("Imperial");
    }

    [Fact]
    public void FoundationConfiguration_WithMaxConversionPathLength()
    {
        var opts = new FoundationConfiguration().WithMaxConversionPathLength(10).Build();
        opts.MaxConversionPathLength.Should().Be(10);
    }
}

