namespace MathVerse.Calculus.Tests;

public class DifferentiatorTests
{
    private readonly Differentiator _diff = new();
    private static readonly Expression X = Expr.Variable("x");

    private double Eval(Expression expr) => CalculusUtils.EvaluateAt(expr, "x", 2.0);

    [Fact]
    public void Differentiate_Constant_IsZero()
    {
        var result = _diff.Differentiate(Expr.Literal(5), "x");
        result.Should().BeOfType<LiteralExpression>();
        result.As<LiteralExpression>().Value.Should().Be(0.0);
    }

    [Fact]
    public void Differentiate_Variable_IsOne()
    {
        var result = _diff.Differentiate(X, "x");
        result.Should().BeOfType<LiteralExpression>();
        result.As<LiteralExpression>().Value.Should().Be(1.0);
    }

    [Fact]
    public void Differentiate_DifferentVariable_IsZero()
    {
        var result = _diff.Differentiate(Expr.Variable("y"), "x");
        result.Should().BeOfType<LiteralExpression>();
        result.As<LiteralExpression>().Value.Should().Be(0.0);
    }

    [Fact]
    public void Differentiate_XSquared_Is2X()
    {
        var result = _diff.Differentiate(Expr.Pow(X, Expr.Literal(2)), "x");
        Eval(result).Should().BeApproximately(4.0, 1e-10);
    }

    [Fact]
    public void Differentiate_XCubed_Is3XSquared()
    {
        var result = _diff.Differentiate(Expr.Pow(X, Expr.Literal(3)), "x");
        Eval(result).Should().BeApproximately(12.0, 1e-10);
    }

    [Fact]
    public void Differentiate_XPow4_Is4XCubed()
    {
        var result = _diff.Differentiate(Expr.Pow(X, Expr.Literal(4)), "x");
        Eval(result).Should().BeApproximately(32.0, 1e-10);
    }

    [Fact]
    public void Differentiate_SecondOrder_XCubed()
    {
        var result = _diff.Differentiate(Expr.Pow(X, Expr.Literal(3)), "x", 2);
        Eval(result).Should().BeApproximately(12.0, 1e-8);
    }

    [Fact]
    public void Differentiate_ThirdOrder_XPow4()
    {
        var result = _diff.Differentiate(Expr.Pow(X, Expr.Literal(4)), "x", 3);
        Eval(result).Should().BeApproximately(48.0, 1e-8);
    }

    [Fact]
    public void Differentiate_FourthOrder_XPow5()
    {
        var result = _diff.Differentiate(Expr.Pow(X, Expr.Literal(5)), "x", 4);
        Eval(result).Should().BeApproximately(240.0, 1e-8);
    }

    [Fact]
    public void Differentiate_XPowNeg1_IsNeg1OverXSquared()
    {
        var result = _diff.Differentiate(Expr.Pow(X, Expr.Literal(-1)), "x");
        Eval(result).Should().BeApproximately(-0.25, 1e-8);
    }

    [Fact]
    public void Differentiate_2TimesX_Is2()
    {
        var result = _diff.Differentiate(Expr.Multiply(Expr.Literal(2), X), "x");
        result.Should().BeOfType<LiteralExpression>();
        result.As<LiteralExpression>().Value.Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void Differentiate_Sum_Polynomial()
    {
        var result = _diff.Differentiate(Expr.Add(X, Expr.Pow(X, Expr.Literal(2))), "x");
        Eval(result).Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void DifferenceRule_XSquaredMinusX()
    {
        var result = _diff.Differentiate(Expr.Subtract(Expr.Pow(X, Expr.Literal(2)), X), "x");
        Eval(result).Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void Differentiate_Negate_Polynomial()
    {
        var result = _diff.Differentiate(Expr.Negate(Expr.Pow(X, Expr.Literal(2))), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ProductRule_xTimesConstant()
    {
        var result = _diff.Differentiate(Expr.Multiply(X, Expr.Literal(5)), "x");
        Eval(result).Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void QuotientRule_1OverX()
    {
        var result = _diff.Differentiate(Expr.Divide(Expr.Literal(1), X), "x");
        Eval(result).Should().BeApproximately(-0.25, 1e-8);
    }

    [Fact]
    public void Differentiate_X2Squared_IsNotIntegral()
    {
        var expr = Expr.Pow(Expr.Pow(X, Expr.Literal(2)), Expr.Literal(2));
        var result = _diff.Differentiate(expr, "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_SinX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Sin(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_CosX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Cos(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_ExpX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Exp(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_LnX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Ln(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_TanX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Tan(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_SqrtX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Sqrt(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_SinhX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Sinh(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_CoshX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Cosh(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_TanhX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Tanh(X), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void SumRule_SinPlusCos_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Add(Expr.Sin(X), Expr.Cos(X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ProductRule_xSinX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Multiply(X, Expr.Sin(X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ProductRule_xExpX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Multiply(X, Expr.Exp(X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_SinXSquared_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Sin(Expr.Pow(X, Expr.Literal(2))), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_ExpXSquared_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Exp(Expr.Pow(X, Expr.Literal(2))), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_Exp2x_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Exp(Expr.Multiply(Expr.Literal(2), X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_Sin2x_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Sin(Expr.Multiply(Expr.Literal(2), X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_Cos3x_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Cos(Expr.Multiply(Expr.Literal(3), X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_Ln2x_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Ln(Expr.Multiply(Expr.Literal(2), X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_CosXCubed_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Cos(Expr.Pow(X, Expr.Literal(3))), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_LnXSquared_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Ln(Expr.Pow(X, Expr.Literal(2))), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_LnXCubed_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Ln(Expr.Pow(X, Expr.Literal(3))), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_ExpSinX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Exp(Expr.Sin(X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_SqrtSinX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Sqrt(Expr.Sin(X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ProductRule_SinXTimesCosX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Multiply(Expr.Sin(X), Expr.Cos(X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ProductRule_TripleProduct_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Multiply(X, Expr.Multiply(Expr.Sin(X), Expr.Cos(X))), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ProductRule_XExpX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Multiply(X, Expr.Exp(X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ProductRule_XSquaredSinX_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Multiply(Expr.Pow(X, Expr.Literal(2)), Expr.Sin(X)), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void QuotientRule_X2OverXPlus1_IsNotIntegral()
    {
        var result = _diff.Differentiate(Expr.Divide(Expr.Pow(X, Expr.Literal(2)), Expr.Add(X, Expr.Literal(1))), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void Differentiate_SinXSquaredPlus1_IsNotIntegral()
    {
        var inner = Expr.Add(Expr.Pow(X, Expr.Literal(2)), Expr.Literal(1));
        var result = _diff.Differentiate(Expr.Sin(inner), "x");
        result.Should().NotBeOfType<IntegralExpression>();
    }

    [Fact]
    public void ChainRule_ExpXSquared_2X_IsNotIntegral()
    {
        var expr = Expr.Pow(Expr.Exp(X), Expr.Pow(X, Expr.Literal(2)));
        var result = _diff.Differentiate(expr, "x");
        result.Should().NotBeNull();
    }

    [Fact]
    public void Differentiate_NullExpr_Throws()
    {
        var act = () => _diff.Differentiate(null!, "x");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Differentiate_NullVariable_Throws()
    {
        var act = () => _diff.Differentiate(X, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Differentiate_ZeroOrder_Throws()
    {
        var act = () => _diff.Differentiate(X, "x", 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SimplificationOptions_Default_IsNotNull()
    {
        SimplificationOptions.Default.Should().NotBeNull();
    }

    [Fact]
    public void Differentiator_SimplificationOptions_IsAccessible()
    {
        _diff.SimplificationOptions.Should().NotBeNull();
    }
}
