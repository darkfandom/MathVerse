namespace MathVerse.Calculus.Tests;

public class LimitCalculatorTests
{
    private readonly LimitCalculator _calc = new();
    private static readonly Expression X = Expr.Variable("x");

    private double Eval(Expression expr) => CalculusUtils.EvaluateAt(expr, "x", 2.0);

    private double GetLimitValue(Expression result)
    {
        if (result is LiteralExpression lit)
            return lit.Value;
        return double.NaN;
    }

    // ─── lim(x→0) sin(x)/x = 1 ───

    [Fact]
    public void Limit_SinXOverX_At0_Is1()
    {
        var expr = Expr.Divide(Expr.Sin(X), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-6);
    }

    // ─── lim(x→∞) 1/x = 0 ───

    [Fact]
    public void Limit_1OverX_AtInfinity_Is0()
    {
        var expr = Expr.Divide(Expr.Literal(1), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(1000000.0));

        GetLimitValue(result).Should().BeApproximately(0.0, 1e-6);
    }

    // ─── lim(x→2) (x^2-4)/(x-2) = 4 ───

    [Fact]
    public void Limit_X2Minus4OverXMinus2_At2_Is4()
    {
        var numerator = Expr.Subtract(Expr.Pow(X, Expr.Literal(2)), Expr.Literal(4));
        var denominator = Expr.Subtract(X, Expr.Literal(2));
        var expr = Expr.Divide(numerator, denominator);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(2));

        GetLimitValue(result).Should().BeApproximately(4.0, 1e-6);
    }

    // ─── lim(x→0) constant is constant ───

    [Fact]
    public void Limit_Constant_IsConstant()
    {
        var result = _calc.ComputeLimit(Expr.Literal(42), "x", Expr.Literal(0));

        GetLimitValue(result).Should().Be(42.0);
    }

    // ─── lim(x→a) x = a ───

    [Fact]
    public void Limit_X_At5_Is5()
    {
        var result = _calc.ComputeLimit(X, "x", Expr.Literal(5));

        GetLimitValue(result).Should().Be(5.0);
    }

    // ─── lim(x→0) x^2 = 0 ───

    [Fact]
    public void Limit_XSquared_At0_Is0()
    {
        var expr = Expr.Pow(X, Expr.Literal(2));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(0.0, 1e-10);
    }

    // ─── lim(x→1) x^2 + x = 2 ───

    [Fact]
    public void Limit_X2PlusX_At1_Is2()
    {
        var expr = Expr.Add(Expr.Pow(X, Expr.Literal(2)), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(1));

        GetLimitValue(result).Should().BeApproximately(2.0, 1e-10);
    }

    // ─── lim(x→0) sin(x)/x = 1 (L'Hopital) ───

    [Fact]
    public void Limit_SinXOverX_At0_LHopital()
    {
        var expr = Expr.Divide(Expr.Sin(X), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-6);
    }

    // ─── lim(x→0) (1-cos(x))/x = 0 ───

    [Fact]
    public void Limit_1MinusCosXOverX_At0_Is0()
    {
        var numerator = Expr.Subtract(Expr.Literal(1), Expr.Cos(X));
        var expr = Expr.Divide(numerator, X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        result.Should().NotBeNull();
    }

    // ─── lim(x→0) x/x = 1 ───

    [Fact]
    public void Limit_XOverX_At0_Is1()
    {
        var expr = Expr.Divide(X, X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        result.Should().NotBeNull();
    }

    // ─── lim(x→∞) 1/x^2 = 0 ───

    [Fact]
    public void Limit_1OverXSquared_AtLarge_Is0()
    {
        var expr = Expr.Divide(Expr.Literal(1), Expr.Pow(X, Expr.Literal(2)));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(1e10));

        GetLimitValue(result).Should().BeApproximately(0.0, 1e-10);
    }

    // ─── lim(x→2) x^2 = 4 ───

    [Fact]
    public void Limit_XSquared_At2_Is4()
    {
        var expr = Expr.Pow(X, Expr.Literal(2));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(2));

        GetLimitValue(result).Should().BeApproximately(4.0, 1e-10);
    }

    // ─── lim(x→0) exp(x) = 1 ───

    [Fact]
    public void Limit_ExpX_At0_Is1()
    {
        var result = _calc.ComputeLimit(Expr.Exp(X), "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-8);
    }

    // ─── lim(x→0) ln(x) = -inf ───

    [Fact]
    public void Limit_LnX_At0_IsNegativeInfinity()
    {
        var result = _calc.ComputeLimit(Expr.Ln(X), "x", Expr.Literal(0));

        if (result is ConstantExpression c)
            double.IsNegativeInfinity(c.Value).Should().BeTrue();
    }

    // ─── lim(x→1) ln(x) = 0 ───

    [Fact]
    public void Limit_LnX_At1_Is0()
    {
        var result = _calc.ComputeLimit(Expr.Ln(X), "x", Expr.Literal(1));

        GetLimitValue(result).Should().BeApproximately(0.0, 1e-8);
    }

    // ─── lim(x→0) tan(x)/x = 1 ───

    [Fact]
    public void Limit_TanXOverX_At0_Is1()
    {
        var expr = Expr.Divide(Expr.Tan(X), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-6);
    }

    // ─── lim(x→0) (e^x - 1)/x = 1 ───

    [Fact]
    public void Limit_ExpXMinus1OverX_At0_Is1()
    {
        var numerator = Expr.Subtract(Expr.Exp(X), Expr.Literal(1));
        var expr = Expr.Divide(numerator, X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-6);
    }

    // ─── lim(x→0) sin(3x)/x = 3 ───

    [Fact]
    public void Limit_Sin3XOverX_At0_Is3()
    {
        var expr = Expr.Divide(Expr.Sin(Expr.Multiply(Expr.Literal(3), X)), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        result.Should().NotBeNull();
    }

    // ─── lim(x→0) (1-cos(x))/x^2 = 1/2 ───

    [Fact]
    public void Limit_1MinusCosXOverXSquared_At0_IsHalf()
    {
        var numerator = Expr.Subtract(Expr.Literal(1), Expr.Cos(X));
        var expr = Expr.Divide(numerator, Expr.Pow(X, Expr.Literal(2)));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        result.Should().NotBeNull();
    }

    // ─── lim(x→3) x + 1 = 4 ───

    [Fact]
    public void Limit_XPlus1_At3_Is4()
    {
        var expr = Expr.Add(X, Expr.Literal(1));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(3));

        GetLimitValue(result).Should().BeApproximately(4.0, 1e-10);
    }

    // ─── lim(x→0) x^3/x = 0 ───

    [Fact]
    public void Limit_X3OverX_At0_Is0()
    {
        var expr = Expr.Divide(Expr.Pow(X, Expr.Literal(3)), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(0.0, 1e-6);
    }

    // ─── lim(x→2) sqrt(x) = sqrt(2) ───

    [Fact]
    public void Limit_SqrtX_At2_IsSqrt2()
    {
        var result = _calc.ComputeLimit(Expr.Sqrt(X), "x", Expr.Literal(2));

        GetLimitValue(result).Should().BeApproximately(Sqrt(2.0), 1e-8);
    }

    // ─── lim(x→1) (x^2 - 1)/(x - 1) = 2 ───

    [Fact]
    public void Limit_X2Minus1OverXMinus1_At1_Is2()
    {
        var numerator = Expr.Subtract(Expr.Pow(X, Expr.Literal(2)), Expr.Literal(1));
        var denominator = Expr.Subtract(X, Expr.Literal(1));
        var expr = Expr.Divide(numerator, denominator);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(1));

        GetLimitValue(result).Should().BeApproximately(2.0, 1e-6);
    }

    // ─── lim(x→0) sin(x)/tan(x) = 1 ───

    [Fact]
    public void Limit_SinXOverTanX_At0_Is1()
    {
        var expr = Expr.Divide(Expr.Sin(X), Expr.Tan(X));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-6);
    }

    // ─── lim(x→0) (sin(x) - x)/x^3 = -1/6 ───

    [Fact]
    public void Limit_SinXMinusXOverXCubed_At0_IsNegOneSixth()
    {
        var numerator = Expr.Subtract(Expr.Sin(X), X);
        var expr = Expr.Divide(numerator, Expr.Pow(X, Expr.Literal(3)));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        result.Should().NotBeNull();
    }

    // ─── lim(x→0) exp(-x) = 1 ───

    [Fact]
    public void Limit_ExpNegX_At0_Is1()
    {
        var expr = Expr.Exp(Expr.Negate(X));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-8);
    }

    // ─── lim(x→∞) x/x = 1 ───

    [Fact]
    public void Limit_XOverX_AtLarge_Is1()
    {
        var expr = Expr.Divide(X, X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(1e10));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-6);
    }

    // ─── lim(x→0) x^2/x = 0 ───

    [Fact]
    public void Limit_X2OverX_At0_Is0()
    {
        var expr = Expr.Divide(Expr.Pow(X, Expr.Literal(2)), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(0.0, 1e-6);
    }

    // ─── lim(x→0) (x + x^2)/x = 1 ───

    [Fact]
    public void Limit_XPlusX2OverX_At0_Is1()
    {
        var numerator = Expr.Add(X, Expr.Pow(X, Expr.Literal(2)));
        var expr = Expr.Divide(numerator, X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-6);
    }

    // ─── Negate: lim(x→0) -x^2 = 0 ───

    [Fact]
    public void Limit_NegXSquared_At0_Is0()
    {
        var expr = Expr.Negate(Expr.Pow(X, Expr.Literal(2)));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        result.Should().NotBeNull();
    }

    // ─── lim(x→π) sin(x) = 0 ───

    [Fact]
    public void Limit_SinX_AtPI_Is0()
    {
        var result = _calc.ComputeLimit(Expr.Sin(X), "x", Expr.Literal(PI));

        GetLimitValue(result).Should().BeApproximately(0.0, 1e-8);
    }

    // ─── lim(x→0) cos(x) = 1 ───

    [Fact]
    public void Limit_CosX_At0_Is1()
    {
        var result = _calc.ComputeLimit(Expr.Cos(X), "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-8);
    }

    // ─── lim(x→0) (e^(2x) - 1)/x = 2 ───

    [Fact]
    public void Limit_Exp2XMinus1OverX_At0_Is2()
    {
        var numerator = Expr.Subtract(Expr.Exp(Expr.Multiply(Expr.Literal(2), X)), Expr.Literal(1));
        var expr = Expr.Divide(numerator, X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        result.Should().NotBeNull();
    }

    // ─── lim(x→0) sin(x)/sin(x) = 1 ───

    [Fact]
    public void Limit_SinXOverSinX_At0_Is1()
    {
        var expr = Expr.Divide(Expr.Sin(X), Expr.Sin(X));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(1.0, 1e-6);
    }

    // ─── lim(x→4) sqrt(x) = 2 ───

    [Fact]
    public void Limit_SqrtX_At4_Is2()
    {
        var result = _calc.ComputeLimit(Expr.Sqrt(X), "x", Expr.Literal(4));

        GetLimitValue(result).Should().BeApproximately(2.0, 1e-8);
    }

    // ─── lim(x→0) (3x^2 + 2x)/x = 2 ───

    [Fact]
    public void Limit_3X2Plus2XOverX_At0_Is2()
    {
        var numerator = Expr.Add(Expr.Multiply(Expr.Literal(3), Expr.Pow(X, Expr.Literal(2))), Expr.Multiply(Expr.Literal(2), X));
        var expr = Expr.Divide(numerator, X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        GetLimitValue(result).Should().BeApproximately(2.0, 1e-6);
    }

    // ─── lim(x→0) sin(5x)/sin(3x) = 5/3 ───

    [Fact]
    public void Limit_Sin5XOverSin3X_At0_Is5Over3()
    {
        var expr = Expr.Divide(Expr.Sin(Expr.Multiply(Expr.Literal(5), X)), Expr.Sin(Expr.Multiply(Expr.Literal(3), X)));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(0));

        result.Should().NotBeNull();
    }

    // ─── Null validation ───

    [Fact]
    public void ComputeLimit_NullExpr_Throws()
    {
        var act = () => _calc.ComputeLimit(null!, "x", Expr.Literal(0));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ComputeLimit_NullVariable_Throws()
    {
        var act = () => _calc.ComputeLimit(X, null!, Expr.Literal(0));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ComputeLimit_NullTarget_Throws()
    {
        var act = () => _calc.ComputeLimit(X, "x", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ─── Power limit: lim(x→2) x^3 = 8 ───

    [Fact]
    public void Limit_XCubed_At2_Is8()
    {
        var expr = Expr.Pow(X, Expr.Literal(3));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(2));

        GetLimitValue(result).Should().BeApproximately(8.0, 1e-10);
    }

    // ─── Multiply limit: lim(x→3) 2x = 6 ───

    [Fact]
    public void Limit_2TimesX_At3_Is6()
    {
        var expr = Expr.Multiply(Expr.Literal(2), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(3));

        GetLimitValue(result).Should().BeApproximately(6.0, 1e-10);
    }

    // ─── Add limit: lim(x→2) x + x^2 = 6 ───

    [Fact]
    public void Limit_XPlusXSquared_At2_Is6()
    {
        var expr = Expr.Add(X, Expr.Pow(X, Expr.Literal(2)));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(2));

        GetLimitValue(result).Should().BeApproximately(6.0, 1e-10);
    }

    // ─── Subtract limit: lim(x→2) x^2 - x = 2 ───

    [Fact]
    public void Limit_X2MinusX_At2_Is2()
    {
        var expr = Expr.Subtract(Expr.Pow(X, Expr.Literal(2)), X);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(2));

        GetLimitValue(result).Should().BeApproximately(2.0, 1e-10);
    }

    // ─── Constant function in limit ───

    [Fact]
    public void Limit_ConstantFunction_IsConstant()
    {
        var expr = Expr.Literal(7);

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(100));

        GetLimitValue(result).Should().Be(7.0);
    }

    // ─── Different variable doesn't affect ───

    [Fact]
    public void Limit_ExpressionWithDifferentVariable_IsConstant()
    {
        var expr = Expr.Pow(Expr.Variable("y"), Expr.Literal(2));

        var result = _calc.ComputeLimit(expr, "x", Expr.Literal(5));

        result.Should().BeSameAs(expr);
    }
}
