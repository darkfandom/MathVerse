namespace MathVerse.Math.Core.Tests;

public class MathContextTests
{
    [Fact]
    public void Default_HasCorrectPrecisionDigits()
    {
        MathContext.Default.PrecisionDigits.Should().Be(15);
    }

    [Fact]
    public void Default_HasCorrectComparisonTolerance()
    {
        MathContext.Default.ComparisonTolerance.Should().Be(1e-10);
    }

    [Fact]
    public void Default_HasCorrectZeroTolerance()
    {
        MathContext.Default.ZeroTolerance.Should().Be(1e-12);
    }

    [Fact]
    public void Default_HasCorrectMaxIterations()
    {
        MathContext.Default.MaxIterations.Should().Be(100);
    }

    [Fact]
    public void HighPrecision_HasTighterPrecisionDigits()
    {
        MathContext.HighPrecision.PrecisionDigits.Should().Be(16);
    }

    [Fact]
    public void HighPrecision_HasTighterComparisonTolerance()
    {
        MathContext.HighPrecision.ComparisonTolerance.Should().Be(1e-14);
    }

    [Fact]
    public void HighPrecision_HasTighterZeroTolerance()
    {
        MathContext.HighPrecision.ZeroTolerance.Should().Be(1e-15);
    }

    [Fact]
    public void HighPrecision_HasMoreIterations()
    {
        MathContext.HighPrecision.MaxIterations.Should().Be(200);
    }

    [Fact]
    public void SinglePrecision_HasFewerDigits()
    {
        MathContext.SinglePrecision.PrecisionDigits.Should().Be(7);
    }

    [Fact]
    public void SinglePrecision_HasLooserComparisonTolerance()
    {
        MathContext.SinglePrecision.ComparisonTolerance.Should().Be(1e-5);
    }

    [Fact]
    public void SinglePrecision_HasLooserZeroTolerance()
    {
        MathContext.SinglePrecision.ZeroTolerance.Should().Be(1e-6);
    }

    [Fact]
    public void SinglePrecision_HasFewerIterations()
    {
        MathContext.SinglePrecision.MaxIterations.Should().Be(50);
    }

    [Fact]
    public void IsEffectivelyZero_Zero_ReturnsTrue()
    {
        MathContext.Default.IsEffectivelyZero(0.0).Should().BeTrue();
    }

    [Fact]
    public void IsEffectivelyZero_VerySmallPositive_ReturnsTrue()
    {
        MathContext.Default.IsEffectivelyZero(1e-15).Should().BeTrue();
    }

    [Fact]
    public void IsEffectivelyZero_NonZero_ReturnsFalse()
    {
        MathContext.Default.IsEffectivelyZero(1.0).Should().BeFalse();
    }

    [Fact]
    public void IsEffectivelyZero_NegativeNonZero_ReturnsFalse()
    {
        MathContext.Default.IsEffectivelyZero(-1.0).Should().BeFalse();
    }

    [Fact]
    public void AreApproximatelyEqual_SameValue_ReturnsTrue()
    {
        MathContext.Default.AreApproximatelyEqual(1.0, 1.0).Should().BeTrue();
    }

    [Fact]
    public void AreApproximatelyEqual_WithinTolerance_ReturnsTrue()
    {
        MathContext.Default.AreApproximatelyEqual(1.0, 1.0 + 1e-11).Should().BeTrue();
    }

    [Fact]
    public void AreApproximatelyEqual_OutsideTolerance_ReturnsFalse()
    {
        MathContext.Default.AreApproximatelyEqual(1.0, 1.1).Should().BeFalse();
    }

    [Fact]
    public void AreApproximatelyEqual_HighPrecision_StricterComparison()
    {
        MathContext.HighPrecision.AreApproximatelyEqual(1.0, 1.0 + 1e-15).Should().BeTrue();
        MathContext.HighPrecision.AreApproximatelyEqual(1.0, 1.0 + 1e-13).Should().BeFalse();
    }

    [Fact]
    public void AreApproximatelyEqual_SinglePrecision_LooserComparison()
    {
        MathContext.SinglePrecision.AreApproximatelyEqual(1.0, 1.0 + 1e-6).Should().BeTrue();
    }

    [Fact]
    public void Round_Zero_ReturnsZero()
    {
        MathContext.Default.Round(0.0).Should().Be(0.0);
    }

    [Fact]
    public void Round_ExactValue_ReturnsSameValue()
    {
        MathContext.Default.Round(42.0).Should().Be(42.0);
    }

    [Fact]
    public void Round_ReducesPrecision()
    {
        var result = MathContext.SinglePrecision.Round(1.23456789);
        result.Should().BeApproximately(1.234568, 1e-4);
    }

    [Fact]
    public void CustomContext_InvalidPrecisionDigits_Throws()
    {
        Action act = () => new MathContext(precisionDigits: 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CustomContext_InvalidComparisonTolerance_Throws()
    {
        Action act = () => new MathContext(comparisonTolerance: 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CustomContext_InvalidZeroTolerance_Throws()
    {
        Action act = () => new MathContext(zeroTolerance: 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CustomContext_InvalidMaxIterations_Throws()
    {
        Action act = () => new MathContext(maxIterations: 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CustomContext_AllParametersApplied()
    {
        var ctx = new MathContext(precisionDigits: 10, comparisonTolerance: 1e-8, zeroTolerance: 1e-9, maxIterations: 200);
        ctx.PrecisionDigits.Should().Be(10);
        ctx.ComparisonTolerance.Should().Be(1e-8);
        ctx.ZeroTolerance.Should().Be(1e-9);
        ctx.MaxIterations.Should().Be(200);
    }
}
