namespace MathVerse.Foundation.Tests;

public sealed class ConversionTests
{
    // ── ConversionRule ────────────────────────────────────────────

    [Fact]
    public void ConversionRule_DefaultValues()
    {
        var rule = new ConversionRule();
        rule.From.Should().BeEmpty();
        rule.To.Should().BeEmpty();
        rule.IsExact.Should().BeFalse();
        rule.Description.Should().BeEmpty();
        rule.Converter.Should().NotBeNull();
    }

    [Fact]
    public void ConversionRule_DefaultConverter_IsIdentity()
    {
        var rule = new ConversionRule();
        rule.Converter(42.0).Should().Be(42.0);
    }

    [Fact]
    public void ConversionRule_WithProperties()
    {
        var rule = new ConversionRule
        {
            From = "m",
            To = "km",
            Converter = v => v / 1000.0,
            IsExact = true,
            Description = "meters to kilometers"
        };
        rule.From.Should().Be("m");
        rule.To.Should().Be("km");
        rule.IsExact.Should().BeTrue();
        rule.Description.Should().Be("meters to kilometers");
        rule.Converter(1000.0).Should().Be(1.0);
    }

    [Fact]
    public void ConversionRule_Converter_Executes()
    {
        var rule = new ConversionRule
        {
            Converter = v => v * 2.0
        };
        rule.Converter(5.0).Should().Be(10.0);
    }

    [Fact]
    public void ConversionRule_Equality_SameValues()
    {
        var a = new ConversionRule { From = "a", To = "b", Description = "test" };
        var b = new ConversionRule { From = "a", To = "b", Description = "test" };
        a.Should().Be(b);
    }

    [Fact]
    public void ConversionRule_Equality_DifferentFrom()
    {
        var a = new ConversionRule { From = "x", To = "b" };
        var b = new ConversionRule { From = "y", To = "b" };
        a.Should().NotBe(b);
    }

    // ── ConversionPath ────────────────────────────────────────────

    [Fact]
    public void ConversionPath_DefaultValues()
    {
        var path = new ConversionPath();
        path.From.Should().BeEmpty();
        path.To.Should().BeEmpty();
        path.Steps.Should().BeEmpty();
        path.IsDirect.Should().BeFalse();
        path.StepCount.Should().Be(0);
    }

    [Fact]
    public void ConversionPath_IsDirect_OneStep()
    {
        var rule = new ConversionRule { From = "m", To = "km" };
        var path = new ConversionPath
        {
            Steps = ImmutableArray.Create(rule),
            From = "m",
            To = "km"
        };
        path.IsDirect.Should().BeTrue();
        path.StepCount.Should().Be(1);
    }

    [Fact]
    public void ConversionPath_IsNotDirect_MultipleSteps()
    {
        var rule1 = new ConversionRule { From = "a", To = "b" };
        var rule2 = new ConversionRule { From = "b", To = "c" };
        var path = new ConversionPath
        {
            Steps = ImmutableArray.Create(rule1, rule2),
            From = "a",
            To = "c"
        };
        path.IsDirect.Should().BeFalse();
        path.StepCount.Should().Be(2);
    }

    [Fact]
    public void ConversionPath_Convert_SingleStep()
    {
        var rule = new ConversionRule { From = "m", To = "km", Converter = v => v / 1000.0 };
        var path = new ConversionPath
        {
            Steps = ImmutableArray.Create(rule),
            From = "m",
            To = "km"
        };
        path.Convert(5000.0).Should().Be(5.0);
    }

    [Fact]
    public void ConversionPath_Convert_MultipleSteps()
    {
        var rule1 = new ConversionRule { Converter = v => v * 100.0 };
        var rule2 = new ConversionRule { Converter = v => v / 1000.0 };
        var path = new ConversionPath
        {
            Steps = ImmutableArray.Create(rule1, rule2)
        };
        path.Convert(1.0).Should().Be(0.1);
    }

    [Fact]
    public void ConversionPath_Convert_EmptySteps()
    {
        var path = new ConversionPath();
        path.Convert(42.0).Should().Be(42.0);
    }

    [Fact]
    public void ConversionPath_Convert_ThreeSteps()
    {
        var r1 = new ConversionRule { Converter = v => v * 2.0 };
        var r2 = new ConversionRule { Converter = v => v + 3.0 };
        var r3 = new ConversionRule { Converter = v => v / 5.0 };
        var path = new ConversionPath
        {
            Steps = ImmutableArray.Create(r1, r2, r3)
        };
        path.Convert(4.0).Should().Be(2.2);
    }

    // ── ConversionResult ──────────────────────────────────────────

    [Fact]
    public void ConversionResult_Succeeded()
    {
        var path = new ConversionPath { From = "m", To = "km" };
        var result = ConversionResult.Succeeded(5.0, path);
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(5.0);
        result.Path.Should().Be(path);
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void ConversionResult_Failed()
    {
        var result = ConversionResult.Failed("not found");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("not found");
        result.Path.Should().BeNull();
    }

    [Fact]
    public void ConversionResult_DefaultValues()
    {
        var result = new ConversionResult();
        result.Success.Should().BeFalse();
        result.ConvertedValue.Should().Be(0.0);
        result.Path.Should().BeNull();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void ConversionResult_Succeeded_WithZeroValue()
    {
        var path = new ConversionPath { From = "a", To = "b" };
        var result = ConversionResult.Succeeded(0.0, path);
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(0.0);
    }

    [Fact]
    public void ConversionResult_Failed_EmptyError()
    {
        var result = ConversionResult.Failed("");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().BeEmpty();
    }

    // ── ConversionGraph ───────────────────────────────────────────

    [Fact]
    public void ConversionGraph_Instance_IsNotNull()
    {
        ConversionGraph.Instance.Should().NotBeNull();
    }

    [Fact]
    public void ConversionGraph_Instance_IsSingleton()
    {
        ConversionGraph.Instance.Should().BeSameAs(ConversionGraph.Instance);
    }

    [Fact]
    public void ConversionGraph_AddRule_AndConvert()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule
        {
            From = "cg_testA",
            To = "cg_testB",
            Converter = v => v * 2.0,
            Description = "double"
        });
        var result = ConversionGraph.Instance.Convert(5.0, "cg_testA", "cg_testB");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(10.0);
    }

    [Fact]
    public void ConversionGraph_AddRule_AutoReverse()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule
        {
            From = "cg_revA",
            To = "cg_revB",
            Converter = v => v * 2.0,
            Description = "double"
        });
        var result = ConversionGraph.Instance.Convert(10.0, "cg_revB", "cg_revA");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(5.0);
    }

    [Fact]
    public void ConversionGraph_Convert_SameUnit()
    {
        var result = ConversionGraph.Instance.Convert(42.0, "cg_sameX", "cg_sameX");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(42.0);
    }

    [Fact]
    public void ConversionGraph_Convert_NoPath_ReturnsFailed()
    {
        var result = ConversionGraph.Instance.Convert(1.0, "cg_unknownA_987", "cg_unknownB_987");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No conversion path");
    }

    [Fact]
    public void ConversionGraph_AddRule_Null_Throws()
    {
        Action act = () => ConversionGraph.Instance.AddRule(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConversionGraph_FindPath_Direct()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule
        {
            From = "cg_fpA",
            To = "cg_fpB",
            Converter = v => v,
            Description = "identity"
        });
        var path = ConversionGraph.Instance.FindPath("cg_fpA", "cg_fpB");
        path.Should().NotBeNull();
        path!.IsDirect.Should().BeTrue();
        path.StepCount.Should().Be(1);
    }

    [Fact]
    public void ConversionGraph_FindPath_Indirect()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_indA", To = "cg_indB", Converter = v => v, Description = "" });
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_indB", To = "cg_indC", Converter = v => v, Description = "" });
        var path = ConversionGraph.Instance.FindPath("cg_indA", "cg_indC");
        path.Should().NotBeNull();
        path!.IsDirect.Should().BeFalse();
        path.StepCount.Should().Be(2);
    }

    [Fact]
    public void ConversionGraph_FindPath_NoPath()
    {
        var path = ConversionGraph.Instance.FindPath("cg_noA_876", "cg_noB_876");
        path.Should().BeNull();
    }

    [Fact]
    public void ConversionGraph_CanConvert_True()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_ccA", To = "cg_ccB", Converter = v => v, Description = "" });
        ConversionGraph.Instance.CanConvert("cg_ccA", "cg_ccB").Should().BeTrue();
    }

    [Fact]
    public void ConversionGraph_CanConvert_False()
    {
        ConversionGraph.Instance.CanConvert("cg_nope1_876", "cg_nope2_876").Should().BeFalse();
    }

    [Fact]
    public void ConversionGraph_CanConvert_Reverse()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_crA", To = "cg_crB", Converter = v => v * 3.0, Description = "triple" });
        ConversionGraph.Instance.CanConvert("cg_crB", "cg_crA").Should().BeTrue();
    }

    [Fact]
    public void ConversionGraph_Convert_ThroughIntermediate()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_mToCm", To = "cg_cmToM", Converter = v => v / 100.0, Description = "m to cm" });
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_cmToM", To = "cg_mmToCm", Converter = v => v / 10.0, Description = "cm to mm" });
        var result = ConversionGraph.Instance.Convert(1.0, "cg_mToCm", "cg_mmToCm");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(0.001);
    }

    [Fact]
    public void ConversionGraph_AddRule_LinearConversion()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule
        {
            From = "cg_linA",
            To = "cg_linB",
            Converter = v => v * 2.0 + 1.0,
            Description = "2x+1"
        });
        var result = ConversionGraph.Instance.Convert(3.0, "cg_linA", "cg_linB");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(7.0);
    }

    [Fact]
    public void ConversionGraph_AddRule_LinearReverse()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule
        {
            From = "cg_linRevA",
            To = "cg_linRevB",
            Converter = v => v * 2.0 + 1.0,
            Description = "2x+1"
        });
        var result = ConversionGraph.Instance.Convert(3.0, "cg_linRevA", "cg_linRevB");
        result.Success.Should().BeTrue();
        var reverse = ConversionGraph.Instance.Convert(result.ConvertedValue, "cg_linRevB", "cg_linRevA");
        reverse.Success.Should().BeTrue();
    }

    [Fact]
    public void ConversionGraph_Convert_CaseInsensitive()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_caseA", To = "cg_caseB", Converter = v => v * 5.0, Description = "" });
        var result = ConversionGraph.Instance.Convert(2.0, "CG_CASEA", "CG_CASEB");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(10.0);
    }

    // ── UnitConverter ─────────────────────────────────────────────

    [Fact]
    public void UnitConverter_Instance_IsNotNull()
    {
        UnitConverter.Instance.Should().NotBeNull();
    }

    [Fact]
    public void UnitConverter_Instance_IsSingleton()
    {
        UnitConverter.Instance.Should().BeSameAs(UnitConverter.Instance);
    }

    [Fact]
    public void UnitConverter_Convert_SameUnit()
    {
        var result = UnitConverter.Instance.Convert(42.0, "uc_sameX", "uc_sameX");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(42.0);
    }

    [Fact]
    public void UnitConverter_Convert_UnknownUnits()
    {
        var result = UnitConverter.Instance.Convert(1.0, "uc_zzz1_765", "uc_zzz2_765");
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void UnitConverter_Convert_WithUnitObjects()
    {
        var from = new Unit { Symbol = "uc_fromA" };
        var to = new Unit { Symbol = "uc_toB" };
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "uc_fromA", To = "uc_toB", Converter = v => v * 10.0, Description = "x10" });
        var result = UnitConverter.Instance.Convert(5.0, from, to);
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(50.0);
    }

    [Fact]
    public void UnitConverter_Convert_NullFrom_Throws()
    {
        Action act = () => UnitConverter.Instance.Convert(1.0, null!, new Unit { Symbol = "uc_x" });
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnitConverter_Convert_NullTo_Throws()
    {
        Action act = () => UnitConverter.Instance.Convert(1.0, new Unit { Symbol = "uc_x" }, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnitConverter_CanConvert_True()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "uc_ccX", To = "uc_ccY", Converter = v => v, Description = "" });
        UnitConverter.Instance.CanConvert("uc_ccX", "uc_ccY").Should().BeTrue();
    }

    [Fact]
    public void UnitConverter_CanConvert_False()
    {
        UnitConverter.Instance.CanConvert("uc_unk1_765", "uc_unk2_765").Should().BeFalse();
    }

    [Fact]
    public void UnitConverter_Convert_Identity_WithUnits()
    {
        var unit = new Unit { Symbol = "uc_ident" };
        var result = UnitConverter.Instance.Convert(99.0, unit, unit);
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(99.0);
    }

    [Fact]
    public void UnitConverter_TryConvert_Zero()
    {
        var success = UnitConverter.Instance.TryConvert(0.0, "uc_zeroX", "uc_zeroX", out var result);
        success.Should().BeTrue();
        result.Should().Be(0.0);
    }

    [Fact]
    public void UnitConverter_Convert_NegativeValue()
    {
        var from = new Unit { Symbol = "uc_negFrom" };
        var to = new Unit { Symbol = "uc_negTo" };
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "uc_negFrom", To = "uc_negTo", Converter = v => v * 2.0, Description = "" });
        var result = UnitConverter.Instance.Convert(-3.0, from, to);
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(-6.0);
    }

    [Fact]
    public void ConversionRule_Equality_DifferentTo()
    {
        var a = new ConversionRule { From = "x", To = "b" };
        var b = new ConversionRule { From = "x", To = "c" };
        a.Should().NotBe(b);
    }

    [Fact]
    public void ConversionRule_Equality_Null()
    {
        var rule = new ConversionRule { From = "a", To = "b" };
        rule.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void ConversionPath_FromAndTo_AreSet()
    {
        var path = new ConversionPath { From = "x", To = "y" };
        path.From.Should().Be("x");
        path.To.Should().Be("y");
    }

    [Fact]
    public void ConversionGraph_Convert_ViaBFS_MultiHop()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_hopA", To = "cg_hopB", Converter = v => v * 2.0, Description = "" });
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_hopB", To = "cg_hopC", Converter = v => v * 3.0, Description = "" });
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_hopC", To = "cg_hopD", Converter = v => v * 5.0, Description = "" });
        var result = ConversionGraph.Instance.Convert(1.0, "cg_hopA", "cg_hopD");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(30.0);
    }

    [Fact]
    public void ConversionGraph_CanConvert_SameUnit_Unregistered()
    {
        ConversionGraph.Instance.CanConvert("cg_unregSame_A", "cg_unregSame_A").Should().BeFalse();
    }

    [Fact]
    public void ConversionGraph_AddRule_MultipleFromSameNode()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_multiA", To = "cg_multiB", Converter = v => v + 1.0, Description = "" });
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "cg_multiA", To = "cg_multiC", Converter = v => v + 2.0, Description = "" });
        var resultB = ConversionGraph.Instance.Convert(10.0, "cg_multiA", "cg_multiB");
        var resultC = ConversionGraph.Instance.Convert(10.0, "cg_multiA", "cg_multiC");
        resultB.ConvertedValue.Should().Be(11.0);
        resultC.ConvertedValue.Should().Be(12.0);
    }

    [Fact]
    public void ConversionRule_ExactFlag()
    {
        var rule = new ConversionRule { From = "a", To = "b", Converter = v => v * 2.0, IsExact = true };
        rule.IsExact.Should().BeTrue();
    }

    [Fact]
    public void ConversionPath_IsDirect_TrueForSingleStep()
    {
        var path = new ConversionPath
        {
            Steps = new[] { new ConversionRule { From = "a", To = "b", Converter = v => v * 2.0 } }.ToImmutableArray(),
            From = "a",
            To = "b"
        };
        path.IsDirect.Should().BeTrue();
    }

    [Fact]
    public void ConversionPath_IsDirect_FalseForMultiStep()
    {
        var path = new ConversionPath
        {
            Steps = new[] {
                new ConversionRule { From = "a", To = "b", Converter = v => v * 2.0 },
                new ConversionRule { From = "b", To = "c", Converter = v => v * 3.0 }
            }.ToImmutableArray(),
            From = "a",
            To = "c"
        };
        path.IsDirect.Should().BeFalse();
    }

    [Fact]
    public void ConversionPath_StepCount()
    {
        var path = new ConversionPath
        {
            Steps = new[] {
                new ConversionRule { From = "a", To = "b", Converter = v => v * 2.0 },
                new ConversionRule { From = "b", To = "c", Converter = v => v * 3.0 },
                new ConversionRule { From = "c", To = "d", Converter = v => v * 4.0 }
            }.ToImmutableArray(),
            From = "a",
            To = "d"
        };
        path.StepCount.Should().Be(3);
    }

    [Fact]
    public void ConversionPath_Convert_ChainedConversions()
    {
        var path = new ConversionPath
        {
            Steps = new[] {
                new ConversionRule { From = "a", To = "b", Converter = v => v * 2.0 },
                new ConversionRule { From = "b", To = "c", Converter = v => v + 10.0 }
            }.ToImmutableArray(),
            From = "a",
            To = "c"
        };
        path.Convert(5.0).Should().Be(20.0);
    }

    [Fact]
    public void ConversionResult_Ok_CreatesSuccess()
    {
        var path = new ConversionPath { From = "a", To = "b", Steps = ImmutableArray<ConversionRule>.Empty };
        var result = ConversionResult.Ok(42.0, path);
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(42.0);
        result.Path.Should().Be(path);
    }

    [Fact]
    public void ConversionResult_Fail_CreatesFailure()
    {
        var result = ConversionResult.Fail("No path found");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("No path found");
    }

    [Fact]
    public void UnitConverter_Convert_TemperatureCtoF()
    {
        var celsius = UnitRegistry.Instance.Get("\u00B0C")!;
        var fahrenheit = UnitRegistry.Instance.Get("\u00B0F")!;
        UnitConverter.Instance.Convert(0.0, celsius, fahrenheit).Should().Be(32.0);
        UnitConverter.Instance.Convert(100.0, celsius, fahrenheit).Should().Be(212.0);
    }

    [Fact]
    public void UnitConverter_Convert_TemperatureFtoK()
    {
        var f = UnitRegistry.Instance.Get("\u00B0F")!;
        var k = UnitRegistry.Instance.Get("K")!;
        UnitConverter.Instance.Convert(32.0, f, k).Should().BeApproximately(273.15, 0.01);
    }

    [Fact]
    public void UnitConverter_Convert_Length_MtoFt()
    {
        var m = UnitRegistry.Instance.Get("m")!;
        var ft = UnitRegistry.Instance.Get("ft")!;
        UnitConverter.Instance.Convert(1.0, m, ft).Should().BeApproximately(3.28084, 0.001);
    }

    [Fact]
    public void UnitConverter_Convert_Mass_KgToLb()
    {
        var kg = UnitRegistry.Instance.Get("kg")!;
        var lb = UnitRegistry.Instance.Get("lb")!;
        UnitConverter.Instance.Convert(1.0, kg, lb).Should().BeApproximately(2.20462, 0.001);
    }

    [Fact]
    public void UnitConverter_Convert_Volume_LtoGal()
    {
        var l = UnitRegistry.Instance.Get("L")!;
        var gal = UnitRegistry.Instance.Get("gal")!;
        UnitConverter.Instance.Convert(1.0, l, gal).Should().BeApproximately(0.264172, 0.001);
    }

    [Fact]
    public void UnitConverter_CanConvert_Compatible()
    {
        var m = UnitRegistry.Instance.Get("m")!;
        var ft = UnitRegistry.Instance.Get("ft")!;
        UnitConverter.Instance.CanConvert(m, ft).Should().BeTrue();
    }

    [Fact]
    public void UnitConverter_CanConvert_Incompatible()
    {
        var m = UnitRegistry.Instance.Get("m")!;
        var kg = UnitRegistry.Instance.Get("kg")!;
        UnitConverter.Instance.CanConvert(m, kg).Should().BeFalse();
    }

    [Fact]
    public void UnitConverter_ConvertQuantity_PreservesValue()
    {
        var q = new PhysicalQuantity(1.0, UnitRegistry.Instance.Get("m")!);
        var cm = UnitRegistry.Instance.Get("cm")!;
        var converted = UnitConverter.Instance.ConvertQuantity(q, cm);
        converted.Value.Should().Be(100.0);
        converted.Unit.Symbol.Should().Be("cm");
    }

    [Fact]
    public void UnitConverter_GetConversionFactors_ReturnsAllUnits()
    {
        var factors = UnitConverter.Instance.GetConversionFactors(UnitRegistry.Instance.Get("m")!, UnitCategory.Length);
        factors.Should().NotBeEmpty();
        factors.Should().Contain(f => f.symbol == "m" && f.factor == 1.0);
        factors.Should().Contain(f => f.symbol == "cm" && f.factor == 100.0);
    }

    [Fact]
    public void ConversionGraph_ThreadSafe()
    {
        Parallel.For(0, 50, i => {
            ConversionGraph.Instance.CanConvert("m", "cm").Should().BeTrue();
            ConversionGraph.Instance.Convert(1.0, "m", "cm").ConvertedValue.Should().Be(100.0);
        });
    }

    [Fact]
    public void UnitConverter_Convert_StringSymbols()
    {
        UnitConverter.Instance.Convert(1.0, "m", "cm").Should().Be(100.0);
        UnitConverter.Instance.Convert(100.0, "cm", "m").Should().Be(1.0);
    }

    [Fact]
    public void UnitConverter_TryConvert_StringSymbols()
    {
        var result = UnitConverter.Instance.TryConvert(1.0, "m", "cm");
        result.Success.Should().BeTrue();
        result.ConvertedValue.Should().Be(100.0);
    }

    [Fact]
    public void ConversionGraph_AddRule_BidirectionalForExact()
    {
        var countBefore = ConversionGraph.Instance.FindPath("m", "ft")?.Steps.Length ?? 0;
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "m", To = "ft", Converter = v => v * 3.28084, IsExact = true, Description = "test" });
        var path = ConversionGraph.Instance.FindPath("ft", "m");
        path.Should().NotBeNull();
    }

    [Fact]
    public void ConversionResult_Equality()
    {
        var r1 = ConversionResult.Ok(1.0, null);
        var r2 = ConversionResult.Ok(1.0, null);
        r1.Should().Be(r2);
    }

    [Fact]
    public void ConversionGraph_FindPath_ReturnsShortest()
    {
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "shortA", To = "shortB", Converter = v => v * 2.0 });
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "shortA", To = "shortC", Converter = v => v * 3.0 });
        ConversionGraph.Instance.AddRule(new ConversionRule { From = "shortC", To = "shortB", Converter = v => v * 4.0 });
        var path = ConversionGraph.Instance.FindPath("shortA", "shortB");
        path!.IsDirect.Should().BeTrue();
    }
}

