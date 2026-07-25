namespace MathVerse.Math.Core.Tests;

public class MathHelperTests
{
    [Theory]
    [InlineData(12, 8, 4)]
    [InlineData(15, 25, 5)]
    [InlineData(7, 13, 1)]
    [InlineData(0, 5, 5)]
    [InlineData(100, 75, 25)]
    [InlineData(54, 24, 6)]
    [InlineData(17, 17, 17)]
    [InlineData(-12, 8, 4)]
    public void GCD_ReturnsCorrectResult(int a, int b, int expected)
    {
        MathHelper.GCD(a, b).Should().Be(expected);
    }

    [Theory]
    [InlineData(4, 6, 12)]
    [InlineData(3, 7, 21)]
    [InlineData(1, 5, 5)]
    [InlineData(6, 6, 6)]
    [InlineData(0, 5, 0)]
    [InlineData(5, 0, 0)]
    [InlineData(12, 8, 24)]
    public void LCM_ReturnsCorrectResult(int a, int b, int expected)
    {
        MathHelper.LCM(a, b).Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 1.0)]
    [InlineData(1, 1.0)]
    [InlineData(5, 120.0)]
    [InlineData(10, 3628800.0)]
    [InlineData(3, 6.0)]
    [InlineData(7, 5040.0)]
    public void Factorial_ReturnsCorrectResult(int n, double expected)
    {
        MathHelper.Factorial(n).Should().Be(expected);
    }

    [Fact]
    public void Factorial_NegativeValue_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => MathHelper.Factorial(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Factorial_ValueOver170_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => MathHelper.Factorial(171);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(5, 2, 10.0)]
    [InlineData(10, 3, 120.0)]
    [InlineData(5, 0, 1.0)]
    [InlineData(5, 5, 1.0)]
    [InlineData(6, 3, 20.0)]
    [InlineData(1, 1, 1.0)]
    [InlineData(10, 7, 120.0)]
    public void Binomial_ReturnsCorrectResult(int n, int k, double expected)
    {
        MathHelper.Binomial(n, k).Should().Be(expected);
    }

    [Fact]
    public void Binomial_NegativeN_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => MathHelper.Binomial(-1, 2);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Binomial_KExceedsN_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => MathHelper.Binomial(5, 6);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(17, true)]
    [InlineData(1, false)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(29, true)]
    [InlineData(100, false)]
    [InlineData(97, true)]
    public void IsPrime_ReturnsCorrectResult(int n, bool expected)
    {
        MathHelper.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(4, 5)]
    [InlineData(10, 11)]
    [InlineData(3, 3)]
    [InlineData(13, 13)]
    [InlineData(14, 17)]
    public void NextPrime_ReturnsCorrectResult(int n, int expected)
    {
        MathHelper.NextPrime(n).Should().Be(expected);
    }

    [Fact]
    public void NextPrime_LessThan2_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => MathHelper.NextPrime(1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Approximately_SameValue_ReturnsTrue()
    {
        MathHelper.Approximately(3.14, 3.14).Should().BeTrue();
    }

    [Fact]
    public void Approximately_CloseValues_ReturnsTrue()
    {
        MathHelper.Approximately(1.0, 1.0 + 1e-16).Should().BeTrue();
    }

    [Fact]
    public void Approximately_FarValues_ReturnsFalse()
    {
        MathHelper.Approximately(1.0, 2.0).Should().BeFalse();
    }

    [Fact]
    public void Approximately_ZeroTolerance_VeryCloseButOver_ReturnsFalse()
    {
        MathHelper.Approximately(0.0, double.Epsilon * 2, double.Epsilon).Should().BeFalse();
    }

    [Fact]
    public void DegreesToRadians_ZeroDegrees_ReturnsZero()
    {
        MathHelper.DegreesToRadians(0).Should().Be(0.0);
    }

    [Fact]
    public void DegreesToRadians_180Degrees_ReturnsPi()
    {
        MathHelper.DegreesToRadians(180).Should().BeApproximately(System.Math.PI, 1e-10);
    }

    [Fact]
    public void DegreesToRadians_90Degrees_ReturnsHalfPi()
    {
        MathHelper.DegreesToRadians(90).Should().BeApproximately(System.Math.PI / 2.0, 1e-10);
    }

    [Fact]
    public void RadiansToDegrees_ZeroRadians_ReturnsZero()
    {
        MathHelper.RadiansToDegrees(0).Should().Be(0.0);
    }

    [Fact]
    public void RadiansToDegrees_Pi_Returns180()
    {
        MathHelper.RadiansToDegrees(System.Math.PI).Should().BeApproximately(180.0, 1e-10);
    }

    [Fact]
    public void DegreesAndRadians_AreInverseOperations()
    {
        double degrees = 45.0;
        MathHelper.RadiansToDegrees(MathHelper.DegreesToRadians(degrees)).Should().BeApproximately(degrees, 1e-10);
    }

    [Fact]
    public void RadiansAndDegrees_AreInverseOperations()
    {
        double radians = System.Math.PI / 3.0;
        MathHelper.DegreesToRadians(MathHelper.RadiansToDegrees(radians)).Should().BeApproximately(radians, 1e-10);
    }

    [Theory]
    [InlineData(5.0, 3.0, 7.0, 5.0)]
    [InlineData(2.0, 3.0, 7.0, 3.0)]
    [InlineData(8.0, 3.0, 7.0, 7.0)]
    [InlineData(5.0, 5.0, 5.0, 5.0)]
    public void Clamp_WithinRange_ReturnsValue(double value, double min, double max, double expected)
    {
        MathHelper.Clamp(value, min, max).Should().Be(expected);
    }

    [Fact]
    public void Clamp_BelowRange_ReturnsMin()
    {
        MathHelper.Clamp(1.0, 3.0, 7.0).Should().Be(3.0);
    }

    [Fact]
    public void Clamp_AboveRange_ReturnsMax()
    {
        MathHelper.Clamp(10.0, 3.0, 7.0).Should().Be(7.0);
    }

    [Fact]
    public void Sign_PositiveValue_Returns1()
    {
        MathHelper.Sign(42.0).Should().Be(1);
    }

    [Fact]
    public void Sign_NegativeValue_ReturnsMinus1()
    {
        MathHelper.Sign(-42.0).Should().Be(-1);
    }

    [Fact]
    public void Sign_Zero_ReturnsZero()
    {
        MathHelper.Sign(0.0).Should().Be(0);
    }

    [Fact]
    public void Abs_PositiveValue_ReturnsSameValue()
    {
        MathHelper.Abs(42.0).Should().Be(42.0);
    }

    [Fact]
    public void Abs_NegativeValue_ReturnsPositiveValue()
    {
        MathHelper.Abs(-42.0).Should().Be(42.0);
    }

    [Fact]
    public void Abs_Zero_ReturnsZero()
    {
        MathHelper.Abs(0.0).Should().Be(0.0);
    }

    [Fact]
    public void Lerp_AtZero_ReturnsA()
    {
        MathHelper.Lerp(0.0, 10.0, 0.0).Should().Be(0.0);
    }

    [Fact]
    public void Lerp_AtOne_ReturnsB()
    {
        MathHelper.Lerp(0.0, 10.0, 1.0).Should().Be(10.0);
    }

    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint()
    {
        MathHelper.Lerp(0.0, 10.0, 0.5).Should().Be(5.0);
    }

    [Fact]
    public void LogBase_PowersOfBase_ReturnsExponent()
    {
        MathHelper.LogBase(8, 2).Should().Be(3);
    }

    [Fact]
    public void LogBase_NonPower_RoundsDown()
    {
        MathHelper.LogBase(10, 2).Should().Be(3);
    }

    [Fact]
    public void IsFinite_NormalValue_ReturnsTrue()
    {
        MathHelper.IsFinite(42.0).Should().BeTrue();
    }

    [Fact]
    public void IsFinite_Infinity_ReturnsFalse()
    {
        MathHelper.IsFinite(double.PositiveInfinity).Should().BeFalse();
    }

    [Fact]
    public void IsNaN_NaN_ReturnsTrue()
    {
        MathHelper.IsNaN(double.NaN).Should().BeTrue();
    }

    [Fact]
    public void IsNaN_NormalValue_ReturnsFalse()
    {
        MathHelper.IsNaN(42.0).Should().BeFalse();
    }
}
