namespace MathVerse.Foundation.Tests;

public class ConstantTests
{
    [Fact]
    public void Pi_HasCorrectSymbol()
    {
        BuiltinConstants.Pi.Symbol.Should().Be("\u03C0");
    }

    [Fact]
    public void Pi_HasCorrectName()
    {
        BuiltinConstants.Pi.Name.Should().Be("Pi");
    }

    [Fact]
    public void Pi_HasTranscendentalCategory()
    {
        BuiltinConstants.Pi.Category.Should().Be(ConstantCategory.Transcendental);
    }

    [Fact]
    public void Pi_NumericValueMatchesSystemMath()
    {
        BuiltinConstants.Pi.NumericValue.Should().Be(System.Math.PI);
    }

    [Fact]
    public void Pi_ComplexValueHasZeroImaginary()
    {
        BuiltinConstants.Pi.ComplexValue.Imaginary.Should().Be(0);
    }

    [Fact]
    public void Pi_IsExact()
    {
        BuiltinConstants.Pi.IsExact.Should().BeTrue();
    }

    [Fact]
    public void Pi_HasAliases()
    {
        BuiltinConstants.Pi.Aliases.Should().Contain("pi");
    }

    [Fact]
    public void Pi_ToStringReturnsSymbol()
    {
        BuiltinConstants.Pi.ToString().Should().Be("\u03C0");
    }

    [Fact]
    public void Tau_HasCorrectSymbol()
    {
        BuiltinConstants.Tau.Symbol.Should().Be("\u03C4");
    }

    [Fact]
    public void Tau_NumericValueMatchesSystemMath()
    {
        BuiltinConstants.Tau.NumericValue.Should().Be(System.Math.Tau);
    }

    [Fact]
    public void Tau_HasTranscendentalCategory()
    {
        BuiltinConstants.Tau.Category.Should().Be(ConstantCategory.Transcendental);
    }

    [Fact]
    public void Tau_IsExact()
    {
        BuiltinConstants.Tau.IsExact.Should().BeTrue();
    }

    [Fact]
    public void E_HasCorrectSymbol()
    {
        BuiltinConstants.E.Symbol.Should().Be("e");
    }

    [Fact]
    public void E_NumericValueMatchesSystemMath()
    {
        BuiltinConstants.E.NumericValue.Should().Be(System.Math.E);
    }

    [Fact]
    public void E_HasTranscendentalCategory()
    {
        BuiltinConstants.E.Category.Should().Be(ConstantCategory.Transcendental);
    }

    [Fact]
    public void E_IsExact()
    {
        BuiltinConstants.E.IsExact.Should().BeTrue();
    }

    [Fact]
    public void Phi_HasCorrectSymbol()
    {
        BuiltinConstants.Phi.Symbol.Should().Be("\u03C6");
    }

    [Fact]
    public void Phi_NumericValueIsApproximatelyGoldenRatio()
    {
        BuiltinConstants.Phi.NumericValue.Should().BeApproximately(1.6180339887, 1e-9);
    }

    [Fact]
    public void Phi_IsFundamentalCategory()
    {
        BuiltinConstants.Phi.Category.Should().Be(ConstantCategory.Fundamental);
    }

    [Fact]
    public void Gamma_HasCorrectSymbol()
    {
        BuiltinConstants.Gamma.Symbol.Should().Be("\u03B3");
    }

    [Fact]
    public void Gamma_NumericValueIsApproximatelyExpected()
    {
        BuiltinConstants.Gamma.NumericValue.Should().BeApproximately(0.5772156649, 1e-9);
    }

    [Fact]
    public void Gamma_IsAnalysisCategory()
    {
        BuiltinConstants.Gamma.Category.Should().Be(ConstantCategory.Analysis);
    }

    [Fact]
    public void I_HasCorrectSymbol()
    {
        BuiltinConstants.I.Symbol.Should().Be("i");
    }

    [Fact]
    public void I_ComplexValueIsImaginaryUnit()
    {
        BuiltinConstants.I.ComplexValue.Should().Be(new System.Numerics.Complex(0, 1));
    }

    [Fact]
    public void I_NumericValueIsNaN()
    {
        double.IsNaN(BuiltinConstants.I.NumericValue).Should().BeTrue();
    }

    [Fact]
    public void I_IsFundamentalCategory()
    {
        BuiltinConstants.I.Category.Should().Be(ConstantCategory.Fundamental);
    }

    [Fact]
    public void Infinity_HasCorrectSymbol()
    {
        BuiltinConstants.Infinity.Symbol.Should().Be("\u221E");
    }

    [Fact]
    public void Infinity_NumericValueIsPositiveInfinity()
    {
        BuiltinConstants.Infinity.NumericValue.Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void Infinity_IsExact()
    {
        BuiltinConstants.Infinity.IsExact.Should().BeTrue();
    }

    [Fact]
    public void NaN_NumericValueIsNaN()
    {
        double.IsNaN(BuiltinConstants.NaN.NumericValue).Should().BeTrue();
    }

    [Fact]
    public void NaN_IsNotExact()
    {
        BuiltinConstants.NaN.IsExact.Should().BeFalse();
    }

    [Fact]
    public void NaN_ComplexValueHasNaNComponents()
    {
        double.IsNaN(BuiltinConstants.NaN.ComplexValue.Real).Should().BeTrue();
        double.IsNaN(BuiltinConstants.NaN.ComplexValue.Imaginary).Should().BeTrue();
    }

    [Fact]
    public void Epsilon_NumericValueIsMachineEpsilon()
    {
        BuiltinConstants.Epsilon.NumericValue.Should().BeApproximately(2.2204460492503131e-16, 1e-30);
    }

    [Fact]
    public void Epsilon_IsNotExact()
    {
        BuiltinConstants.Epsilon.IsExact.Should().BeFalse();
    }

    [Fact]
    public void Catalan_HasCorrectSymbol()
    {
        BuiltinConstants.Catalan.Symbol.Should().Be("G");
    }

    [Fact]
    public void Catalan_IsNumberTheoryCategory()
    {
        BuiltinConstants.Catalan.Category.Should().Be(ConstantCategory.NumberTheory);
    }

    [Fact]
    public void Apery_HasCorrectSymbol()
    {
        BuiltinConstants.Apery.Symbol.Should().Be("\u03B6(3)");
    }

    [Fact]
    public void Apery_IsNumberTheoryCategory()
    {
        BuiltinConstants.Apery.Category.Should().Be(ConstantCategory.NumberTheory);
    }

    [Fact]
    public void FeigenbaumAlpha_HasCorrectSymbol()
    {
        BuiltinConstants.FeigenbaumAlpha.Symbol.Should().Be("\u03B1");
    }

    [Fact]
    public void FeigenbaumAlpha_IsCombinatoricsCategory()
    {
        BuiltinConstants.FeigenbaumAlpha.Category.Should().Be(ConstantCategory.Combinatorics);
    }

    [Fact]
    public void FeigenbaumDelta_HasCorrectSymbol()
    {
        BuiltinConstants.FeigenbaumDelta.Symbol.Should().Be("\u03B4");
    }

    [Fact]
    public void FeigenbaumDelta_IsCombinatoricsCategory()
    {
        BuiltinConstants.FeigenbaumDelta.Category.Should().Be(ConstantCategory.Combinatorics);
    }

    [Fact]
    public void MathConstant_DefaultValues()
    {
        var c = new MathConstant();
        c.Symbol.Should().BeEmpty();
        c.Name.Should().BeEmpty();
        c.IsExact.Should().BeFalse();
    }

    [Fact]
    public void ConstantRegistry_GetByName_ReturnsPi()
    {
        ConstantRegistry.Instance.Get("Pi").Should().Be(BuiltinConstants.Pi);
    }

    [Fact]
    public void ConstantRegistry_GetBySymbol_ReturnsPi()
    {
        ConstantRegistry.Instance.Get("\u03C0").Should().Be(BuiltinConstants.Pi);
    }

    [Fact]
    public void ConstantRegistry_GetByAlias_ReturnsPi()
    {
        ConstantRegistry.Instance.Get("pi").Should().Be(BuiltinConstants.Pi);
    }

    [Fact]
    public void ConstantRegistry_GetByName_IsCaseInsensitive()
    {
        ConstantRegistry.Instance.Get("pi").Should().Be(BuiltinConstants.Pi);
    }

    [Fact]
    public void ConstantRegistry_GetByName_ReturnsNullForUnknown()
    {
        ConstantRegistry.Instance.Get("NonexistentConstant").Should().BeNull();
    }

    [Fact]
    public void ConstantRegistry_GetByName_ThrowsForNull()
    {
        Action act = () => ConstantRegistry.Instance.Get(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_TranscendentalContainsPi()
    {
        ConstantRegistry.Instance.GetByCategory(ConstantCategory.Transcendental)
            .Should().Contain(BuiltinConstants.Pi);
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_TranscendentalContainsE()
    {
        ConstantRegistry.Instance.GetByCategory(ConstantCategory.Transcendental)
            .Should().Contain(BuiltinConstants.E);
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_FundamentalContainsI()
    {
        ConstantRegistry.Instance.GetByCategory(ConstantCategory.Fundamental)
            .Should().Contain(BuiltinConstants.I);
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_EmptyForUnknownCategory()
    {
        ConstantRegistry.Instance.GetByCategory(ConstantCategory.Physical)
            .Should().BeEmpty();
    }

    [Fact]
    public void ConstantRegistry_GetAll_ContainsAllBuiltIns()
    {
        var all = ConstantRegistry.Instance.GetAll();
        all.Should().Contain(BuiltinConstants.Pi);
        all.Should().Contain(BuiltinConstants.E);
        all.Should().Contain(BuiltinConstants.Tau);
        all.Should().Contain(BuiltinConstants.Phi);
        all.Should().Contain(BuiltinConstants.Gamma);
        all.Should().Contain(BuiltinConstants.I);
        all.Should().Contain(BuiltinConstants.Infinity);
        all.Should().Contain(BuiltinConstants.NaN);
        all.Should().Contain(BuiltinConstants.Epsilon);
        all.Should().Contain(BuiltinConstants.Catalan);
        all.Should().Contain(BuiltinConstants.Apery);
        all.Should().Contain(BuiltinConstants.FeigenbaumAlpha);
        all.Should().Contain(BuiltinConstants.FeigenbaumDelta);
    }

    [Fact]
    public void ConstantRegistry_Register_ThrowsForNull()
    {
        Action act = () => ConstantRegistry.Instance.Register(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConstantRegistry_Register_AddsCustomConstant()
    {
        var custom = new MathConstant
        {
            Symbol = "X",
            Name = "CustomTestConst",
            Category = ConstantCategory.Derived,
            NumericValue = 42.0,
            ComplexValue = new System.Numerics.Complex(42, 0),
            Aliases = System.Collections.Immutable.ImmutableArray.Create("custom"),
            Description = "A test constant",
            IsExact = true
        };
        ConstantRegistry.Instance.Register(custom);
        ConstantRegistry.Instance.Get("CustomTestConst").Should().Be(custom);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_PiReturnsTrue()
    {
        ConstantLookup.Instance.TryGetExact("\u03C0", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Pi);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_EpsilonReturnsFalse()
    {
        ConstantLookup.Instance.TryGetExact("\u03B5", out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetExact_NullReturnsFalse()
    {
        ConstantLookup.Instance.TryGetExact(null!, out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetExact_UnknownReturnsFalse()
    {
        ConstantLookup.Instance.TryGetExact("ZZZZ", out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetExact_IByName()
    {
        ConstantLookup.Instance.TryGetExact("I", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.I);
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_PiReturnsTrue()
    {
        ConstantLookup.Instance.TryGetNumeric("\u03C0", out var v).Should().BeTrue();
        v.Should().Be(System.Math.PI);
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_EulerReturnsTrue()
    {
        ConstantLookup.Instance.TryGetNumeric("Euler", out var v).Should().BeTrue();
        v.Should().Be(System.Math.E);
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_NaNReturnsFalse()
    {
        ConstantLookup.Instance.TryGetNumeric("NaN", out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_NullReturnsFalse()
    {
        ConstantLookup.Instance.TryGetNumeric(null!, out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_UnknownReturnsFalse()
    {
        ConstantLookup.Instance.TryGetNumeric("Unknown", out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetExact_TauByName()
    {
        ConstantLookup.Instance.TryGetExact("Tau", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Tau);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_PhiByAlias()
    {
        ConstantLookup.Instance.TryGetExact("golden ratio", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Phi);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_GammaByName()
    {
        ConstantLookup.Instance.TryGetExact("Gamma", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Gamma);
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_FeigenbaumAlpha()
    {
        ConstantLookup.Instance.TryGetNumeric("\u03B1", out var v).Should().BeTrue();
        v.Should().BeApproximately(2.502907875, 1e-6);
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_FeigenbaumDelta()
    {
        ConstantLookup.Instance.TryGetNumeric("\u03B4", out var v).Should().BeTrue();
        v.Should().BeApproximately(4.669201609, 1e-6);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_CatalanBySymbol()
    {
        ConstantLookup.Instance.TryGetExact("G", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Catalan);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_AperyByAlias()
    {
        ConstantLookup.Instance.TryGetExact("zeta(3)", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Apery);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_EpsilonReturnsFalse_Added()
    {
        ConstantLookup.Instance.TryGetExact("\u03B5", out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_EpsilonReturnsTrue()
    {
        ConstantLookup.Instance.TryGetNumeric("\u03B5", out var v).Should().BeTrue();
        v.Should().BeApproximately(2.2204460492503131e-16, 1e-30);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_InfinityReturnsFalse()
    {
        ConstantLookup.Instance.TryGetExact("\u221E", out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_InfinityReturnsTrue()
    {
        ConstantLookup.Instance.TryGetNumeric("\u221E", out var v).Should().BeTrue();
        v.Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_NaNReturnsFalse_Added()
    {
        ConstantLookup.Instance.TryGetExact("NaN", out _).Should().BeFalse();
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_NaNReturnsFalse_Added()
    {
        ConstantLookup.Instance.TryGetNumeric("NaN", out _).Should().BeFalse();
    }

    [Fact]
    public async Task ConstantRegistry_ThreadSafety_ConcurrentRegisters()
    {
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            var c = new MathConstant
            {
                Symbol = $"T{i}",
                Name = $"Test{i}",
                Category = ConstantCategory.Derived,
                NumericValue = i,
                ComplexValue = new System.Numerics.Complex(i, 0),
                Aliases = System.Collections.Immutable.ImmutableArray.Create($"t{i}"),
                Description = "Test",
                IsExact = true
            };
            ConstantRegistry.Instance.Register(c);
        })).ToArray();
        await Task.WhenAll(tasks);
        ConstantRegistry.Instance.Get("T50").Should().NotBeNull();
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_PhysicalIsEmpty()
    {
        ConstantRegistry.Instance.GetByCategory(ConstantCategory.Physical).Should().BeEmpty();
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_NumberTheory()
    {
        var cats = ConstantRegistry.Instance.GetByCategory(ConstantCategory.NumberTheory);
        cats.Should().Contain(BuiltinConstants.Catalan);
        cats.Should().Contain(BuiltinConstants.Apery);
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_Combinatorics()
    {
        var cats = ConstantRegistry.Instance.GetByCategory(ConstantCategory.Combinatorics);
        cats.Should().Contain(BuiltinConstants.FeigenbaumAlpha);
        cats.Should().Contain(BuiltinConstants.FeigenbaumDelta);
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_Analysis()
    {
        var cats = ConstantRegistry.Instance.GetByCategory(ConstantCategory.Analysis);
        cats.Should().Contain(BuiltinConstants.Gamma);
    }

    [Fact]
    public void ConstantRegistry_GetByCategory_Fundamental()
    {
        var cats = ConstantRegistry.Instance.GetByCategory(ConstantCategory.Fundamental);
        cats.Should().Contain(BuiltinConstants.I);
        cats.Should().Contain(BuiltinConstants.Phi);
    }

    [Fact]
    public void ConstantProvider_CreatesWithFunctions()
    {
        var provider = new ConstantProvider(
            () => 3.14,
            () => new System.Numerics.Complex(3.14, 0));
        provider.GetNumeric().Should().Be(3.14);
        provider.GetComplex().Should().Be(new System.Numerics.Complex(3.14, 0));
    }

    [Fact]
    public void ConstantProvider_NullNumericProvider_Throws()
    {
        Action act = () => new ConstantProvider(null!, () => default);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConstantProvider_NullComplexProvider_Throws()
    {
        Action act = () => new ConstantProvider(() => 1.0, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BuiltinConstants_Pi_PrecisionVerification()
    {
        BuiltinConstants.Pi.NumericValue.Should().BeApproximately(Math.PI, 1e-15);
    }

    [Fact]
    public void BuiltinConstants_Tau_PrecisionVerification()
    {
        BuiltinConstants.Tau.NumericValue.Should().BeApproximately(Math.Tau, 1e-15);
    }

    [Fact]
    public void BuiltinConstants_E_PrecisionVerification()
    {
        BuiltinConstants.E.NumericValue.Should().BeApproximately(Math.E, 1e-15);
    }

    [Fact]
    public void BuiltinConstants_Phi_ValueVerification()
    {
        var expected = (1 + Math.Sqrt(5)) / 2;
        BuiltinConstants.Phi.NumericValue.Should().BeApproximately(expected, 1e-12);
    }

    [Fact]
    public void BuiltinConstants_Gamma_ValueVerification()
    {
        BuiltinConstants.Gamma.NumericValue.Should().BeApproximately(0.5772156649015328606, 1e-12);
    }

    [Fact]
    public void BuiltinConstants_Catalan_ValueVerification()
    {
        BuiltinConstants.Catalan.NumericValue.Should().BeApproximately(0.915965594177219, 1e-12);
    }

    [Fact]
    public void BuiltinConstants_Apery_ValueVerification()
    {
        BuiltinConstants.Apery.NumericValue.Should().BeApproximately(1.202056903159594, 1e-12);
    }

    [Fact]
    public void BuiltinConstants_FeigenbaumAlpha_ValueVerification()
    {
        BuiltinConstants.FeigenbaumAlpha.NumericValue.Should().BeApproximately(2.5029078750958928, 1e-12);
    }

    [Fact]
    public void BuiltinConstants_FeigenbaumDelta_ValueVerification()
    {
        BuiltinConstants.FeigenbaumDelta.NumericValue.Should().BeApproximately(4.6692016091029906, 1e-12);
    }

    [Fact]
    public void ConstantMetadata_DefaultValues()
    {
        var meta = new ConstantMetadata();
        meta.Symbol.Should().BeEmpty();
        meta.Name.Should().BeEmpty();
        meta.Category.Should().Be(ConstantCategory.Derived);
        meta.Description.Should().BeEmpty();
        meta.IsExact.Should().BeFalse();
    }

    [Fact]
    public void ConstantRegistry_GetAll_DistinctConstants()
    {
        var all = ConstantRegistry.Instance.GetAll();
        all.Distinct().Should().HaveCount(all.Count);
    }

    [Fact]
    public void ConstantRegistry_Register_NullThrows()
    {
        Action act = () => ConstantRegistry.Instance.Register(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BuiltinConstants_I_ComplexValueVerification()
    {
        BuiltinConstants.I.ComplexValue.Should().Be(new System.Numerics.Complex(0, 1));
    }

    [Fact]
    public void BuiltinConstants_Infinity_ComplexValueVerification()
    {
        BuiltinConstants.Infinity.ComplexValue.Should().Be(new System.Numerics.Complex(double.PositiveInfinity, 0));
    }

    [Fact]
    public void BuiltinConstants_NaN_ComplexValueHasNaNComponents()
    {
        double.IsNaN(BuiltinConstants.NaN.ComplexValue.Real).Should().BeTrue();
        double.IsNaN(BuiltinConstants.NaN.ComplexValue.Imaginary).Should().BeTrue();
    }

    [Fact]
    public void ConstantLookup_TryGetExact_PiBySymbol()
    {
        ConstantLookup.Instance.TryGetExact("\u03C0", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Pi);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_TauBySymbol()
    {
        ConstantLookup.Instance.TryGetExact("\u03C4", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Tau);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_EBySymbol()
    {
        ConstantLookup.Instance.TryGetExact("e", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.E);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_IBySymbol()
    {
        ConstantLookup.Instance.TryGetExact("i", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.I);
    }

    [Fact]
    public void ConstantLookup_TryGetExact_PhiBySymbol()
    {
        ConstantLookup.Instance.TryGetExact("\u03C6", out var c).Should().BeTrue();
        c.Should().Be(BuiltinConstants.Phi);
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_PiByName()
    {
        ConstantLookup.Instance.TryGetNumeric("Pi", out var v).Should().BeTrue();
        v.Should().Be(Math.PI);
    }

    [Fact]
    public void ConstantLookup_TryGetNumeric_EByName()
    {
        ConstantLookup.Instance.TryGetNumeric("E", out var v).Should().BeTrue();
        v.Should().Be(Math.E);
    }

    [Fact]
    public void ConstantRegistry_RegisterAndGet()
    {
        var custom = new MathConstant("c", "Custom", ConstantCategory.Derived, 42.0, new System.Numerics.Complex(42, 0), new[] { "forty-two" }, "Custom constant", true);
        ConstantRegistry.Instance.Register(custom);
        ConstantRegistry.Instance.Get("c").Should().Be(custom);
    }

    [Fact]
    public void ConstantRegistry_GetAll_ReturnsAll()
    {
        ConstantRegistry.Instance.GetAll().Should().HaveCountGreaterThan(10);
    }

    [Fact]
    public void MathConstant_Equality_SameSymbol()
    {
        var c1 = new MathConstant("x", "X", ConstantCategory.Fundamental, 1.0, new System.Numerics.Complex(1, 0), null, "desc", true);
        var c2 = new MathConstant("x", "X", ConstantCategory.Fundamental, 1.0, new System.Numerics.Complex(1, 0), null, "desc", true);
        c1.Equals(c2).Should().BeTrue();
    }

    [Fact]
    public void MathConstant_HashCode_Stable()
    {
        BuiltinConstants.Pi.GetHashCode().Should().Be(BuiltinConstants.Pi.GetHashCode());
    }

    [Fact]
    public void MathConstant_ToString_ReturnsSymbol()
    {
        BuiltinConstants.Pi.ToString().Should().Be("\u03C0");
    }

    [Fact]
    public void BuiltinConstants_Catalan_Value()
    {
        BuiltinConstants.Catalan.NumericValue.Should().BeApproximately(0.9159655941, 1e-9);
    }

    [Fact]
    public void BuiltinConstants_Apery_Value()
    {
        BuiltinConstants.Apery.NumericValue.Should().BeApproximately(1.202056903, 1e-9);
    }

    [Fact]
    public void BuiltinConstants_FeigenbaumAlpha_Value()
    {
        BuiltinConstants.FiegenbaumAlpha.NumericValue.Should().BeApproximately(2.502907875, 1e-9);
    }

    [Fact]
    public void BuiltinConstants_FeigenbaumDelta_Value()
    {
        BuiltinConstants.FiegenbaumDelta.NumericValue.Should().BeApproximately(4.669201609, 1e-9);
    }

    [Fact]
    public void BuiltinConstants_Epsilon_IsMachineEpsilon()
    {
        BuiltinConstants.Epsilon.NumericValue.Should().Be(2.2204460492503131e-16);
    }

    [Fact]
    public void ConstantProvider_ImplementsInterface()
    {
        var provider = new ConstantProvider(() => 3.14, () => new System.Numerics.Complex(3.14, 0));
        provider.GetNumeric().Should().Be(3.14);
    }

    [Fact]
    public void ConstantCategory_HasAllValues()
    {
        Enum.GetValues<ConstantCategory>().Should().Contain(ConstantCategory.Fundamental);
        Enum.GetValues<ConstantCategory>().Should().Contain(ConstantCategory.Transcendental);
        Enum.GetValues<ConstantCategory>().Should().Contain(ConstantCategory.NumberTheory);
        Enum.GetValues<ConstantCategory>().Should().Contain(ConstantCategory.Analysis);
        Enum.GetValues<ConstantCategory>().Should().Contain(ConstantCategory.Combinatorics);
        Enum.GetValues<ConstantCategory>().Should().Contain(ConstantCategory.Physical);
        Enum.GetValues<ConstantCategory>().Should().Contain(ConstantCategory.Mathematical);
        Enum.GetValues<ConstantCategory>().Should().Contain(ConstantCategory.Derived);
    }

    [Fact]
    public void ConstantLookup_ConcurrentAccess()
    {
        Parallel.For(0, 100, _ => {
            ConstantLookup.Instance.TryGetExact("pi", out _).Should().BeTrue();
            ConstantLookup.Instance.TryGetNumeric("e", out _).Should().BeTrue();
        });
    }

    [Fact]
    public void MathConstant_WithAliases()
    {
        var c = new MathConstant("pi", "Pi", ConstantCategory.Fundamental, Math.PI, new System.Numerics.Complex(Math.PI, 0), new[] { "π", "PI" }, "test", true);
        c.Aliases.Should().Contain("π");
        c.Aliases.Should().Contain("PI");
    }

    [Fact]
    public void MathConstant_IsExact_Flag()
    {
        BuiltinConstants.Pi.IsExact.Should().BeTrue();
        BuiltinConstants.E.IsExact.Should().BeTrue();
        BuiltinConstants.Epsilon.IsExact.Should().BeFalse();
    }

    [Fact]
    public void ConstantRegistry_ThreadSafe()
    {
        Parallel.For(0, 50, i => {
            var c = new MathConstant($"c{i}", $"C{i}", ConstantCategory.Derived, i, new System.Numerics.Complex(i, 0), null, "test", true);
            ConstantRegistry.Instance.Register(c);
        });
        ConstantRegistry.Instance.GetAll().Should().HaveCountGreaterThan(10);
    }

    [Fact]
    public void BuiltinConstants_Infinity_Symbol()
    {
        BuiltinConstants.Infinity.Symbol.Should().Be("\u221E");
    }

    [Fact]
    public void BuiltinConstants_NaN_Symbol()
    {
        BuiltinConstants.NaN.Symbol.Should().Be("NaN");
    }

    [Fact]
    public void ConstantMetadata_CanBeEmpty()
    {
        var meta = new ConstantMetadata("", "", "", System.Collections.Immutable.ImmutableArray<string>.Empty);
        meta.Provenance.Should().Be("");
    }
}

