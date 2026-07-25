namespace MathVerse.Algebra.Tests;

public class PolynomialOperationsTests
{
    // ─── Divide ───

    [Fact]
    public void Divide_ExactDivision_RemainderZero()
    {
        var dividend = Polynomial.FromCoefficients("x", 1, 2, 1);
        var divisor = Polynomial.FromCoefficients("x", 1, 1);

        var (quotient, remainder) = PolynomialOperations.Divide(dividend, divisor);

        quotient.Degree.Should().Be(1);
        quotient.CoefficientAt(0).Should().Be(1.0);
        quotient.CoefficientAt(1).Should().Be(1.0);
        remainder.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Divide_LineicIntoCubic()
    {
        var dividend = Polynomial.FromCoefficients("x", 0, 0, 0, 1);
        var divisor = Polynomial.FromCoefficients("x", 0, 1);

        var (quotient, remainder) = PolynomialOperations.Divide(dividend, divisor);

        quotient.Degree.Should().Be(2);
        remainder.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Divide_WithRemainder()
    {
        var dividend = Polynomial.FromCoefficients("x", 1, 0, 1);
        var divisor = Polynomial.FromCoefficients("x", 1, 1);

        var (quotient, remainder) = PolynomialOperations.Divide(dividend, divisor);

        quotient.Degree.Should().Be(1);
        remainder.IsZero.Should().BeFalse();
    }

    [Fact]
    public void Divide_DividendSmallerDegree_ReturnsZeroQuotient()
    {
        var dividend = Polynomial.FromCoefficients("x", 1);
        var divisor = Polynomial.FromCoefficients("x", 0, 0, 1);

        var (quotient, remainder) = PolynomialOperations.Divide(dividend, divisor);

        quotient.IsZero.Should().BeTrue();
        remainder.Equals(dividend).Should().BeTrue();
    }

    [Fact]
    public void Divide_ByOne_IsIdentity()
    {
        var p = Polynomial.FromCoefficients("x", 3, 7, -2);
        var one = Polynomial.One("x");

        var (quotient, remainder) = PolynomialOperations.Divide(p, one);

        quotient.Equals(p).Should().BeTrue();
        remainder.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Divide_ByZero_Throws()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2);
        var z = Polynomial.Zero("x");

        var act = () => PolynomialOperations.Divide(p, z);

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Divide_SelfDivision_IsOneRemZero()
    {
        var p = Polynomial.FromCoefficients("x", 2, -3, 1);

        var (quotient, remainder) = PolynomialOperations.Divide(p, p);

        quotient.CoefficientAt(0).Should().Be(1.0);
        remainder.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Divide_DifferentVariable_Throws()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2);
        var b = Polynomial.FromCoefficients("y", 1);

        var act = () => PolynomialOperations.Divide(a, b);

        act.Should().Throw<ArgumentException>();
    }

    // ─── GCD ───

    [Fact]
    public void GCD_TwoCoprimePolynomials_IsOne()
    {
        var a = Polynomial.FromCoefficients("x", 1, 1);
        var b = Polynomial.FromCoefficients("x", 1, 2);

        var gcd = PolynomialOperations.GCD(a, b);

        gcd.Degree.Should().Be(0);
        gcd.CoefficientAt(0).Should().Be(1.0);
    }

    [Fact]
    public void GCD_SharedFactor_ReturnsCorrectGCD()
    {
        var a = Polynomial.FromCoefficients("x", -1, 1);
        var b = Polynomial.FromCoefficients("x", -4, 4);

        var gcd = PolynomialOperations.GCD(a, b);

        gcd.Degree.Should().Be(1);
        gcd.Evaluate(1.0).Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void GCD_WithZero_ReturnsOther()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2);
        var z = Polynomial.Zero("x");

        var gcd = PolynomialOperations.GCD(a, z);

        gcd.Degree.Should().Be(1);
    }

    // ─── LCM ───

    [Fact]
    public void LCM_CoprimePolynomials_IsProduct()
    {
        var a = Polynomial.FromCoefficients("x", 1, 1);
        var b = Polynomial.FromCoefficients("x", 1, 2);

        var lcm = PolynomialOperations.LCM(a, b);

        lcm.Degree.Should().Be(2);
    }

    [Fact]
    public void LCM_WithZero_IsZero()
    {
        var a = Polynomial.FromCoefficients("x", 1, 2);
        var z = Polynomial.Zero("x");

        PolynomialOperations.LCM(a, z).IsZero.Should().BeTrue();
    }

    // ─── Bisection ───

    [Fact]
    public void Bisection_RootAtTwo_FindsRoot()
    {
        var p = Polynomial.FromCoefficients("x", -4, 0, 1);

        var result = PolynomialOperations.Bisection(p, 1.0, 3.0);

        result.IsDefined.Should().BeTrue();
        result.Value.Should().BeApproximately(2.0, 1e-8);
    }

    [Fact]
    public void Bisection_RootAtZero()
    {
        var p = Polynomial.FromCoefficients("x", 0, 1);

        var result = PolynomialOperations.Bisection(p, -1.0, 1.0);

        result.IsDefined.Should().BeTrue();
        result.Value.Should().BeApproximately(0.0, 1e-8);
    }

    [Fact]
    public void Bisection_NoSignChange_ReturnsUndefined()
    {
        var p = Polynomial.FromCoefficients("x", 1, 1);

        var result = PolynomialOperations.Bisection(p, 1.0, 3.0);

        result.IsUndefined.Should().BeTrue();
    }

    [Fact]
    public void Bisection_NegativeRoot()
    {
        var p = Polynomial.FromCoefficients("x", -9, 0, 1);

        var result = PolynomialOperations.Bisection(p, -4.0, 0.0);

        result.IsDefined.Should().BeTrue();
        result.Value.Should().BeApproximately(-3.0, 1e-8);
    }

    // ─── NewtonRaphson ───

    [Fact]
    public void NewtonRaphson_RootAtTwo_FindsRoot()
    {
        var p = Polynomial.FromCoefficients("x", -4, 0, 1);

        var result = PolynomialOperations.NewtonRaphson(p, 3.0);

        result.IsDefined.Should().BeTrue();
        result.Value.Should().BeApproximately(2.0, 1e-8);
    }

    [Fact]
    public void NewtonRaphson_Lineic_FindsExactRoot()
    {
        var p = Polynomial.FromCoefficients("x", -6, 2);

        var result = PolynomialOperations.NewtonRaphson(p, 5.0);

        result.IsDefined.Should().BeTrue();
        result.Value.Should().BeApproximately(3.0, 1e-8);
    }

    [Fact]
    public void NewtonRaphson_NegativeRoot()
    {
        var p = Polynomial.FromCoefficients("x", 1, 2);

        var result = PolynomialOperations.NewtonRaphson(p, 0.0);

        result.IsDefined.Should().BeTrue();
        result.Value.Should().BeApproximately(-0.5, 1e-8);
    }

    // ─── EvaluateDerivative ───

    [Fact]
    public void EvaluateDerivative_Quadratic_BecomesLinear()
    {
        var p = Polynomial.FromCoefficients("x", 1, 0, 1);

        var d = PolynomialOperations.EvaluateDerivative(p);

        d.Degree.Should().Be(1);
        d.CoefficientAt(1).Should().Be(2.0);
    }

    // ─── EvaluateIntegral ───

    [Fact]
    public void EvaluateIntegral_Lineic_Quadratic()
    {
        var p = Polynomial.FromCoefficients("x", 0, 1);

        var integral = PolynomialOperations.EvaluateIntegral(p);

        integral.Degree.Should().Be(2);
        integral.CoefficientAt(2).Should().Be(0.5);
    }
}
