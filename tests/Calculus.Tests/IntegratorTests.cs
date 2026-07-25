namespace MathVerse.Calculus.Tests;

public class IntegratorTests
{
    private readonly Integrator _integ = new();
    private static readonly Expression X = Expr.Variable("x");

    private static bool IsIntegralType(Expression expr) => expr is IntegralExpression;

    // ─── Power rule integrals ───

    [Fact]
    public void Integrate_X_IsXSquaredOver2()
    {
        var result = _integ.IndefiniteIntegrate(X, "x");

        result.Should().NotBeNull();
    }

    [Fact]
    public void Integrate_XSquared_IsXCubedOver3()
    {
        var expr = Expr.Pow(X, Expr.Literal(2));

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
        CalculusUtils.EvaluateAt(result, "x", 3.0).Should().BeApproximately(9.0, 1e-10);
    }

    [Fact]
    public void Integrate_XPow3_IsXPow4Over4()
    {
        var expr = Expr.Pow(X, Expr.Literal(3));

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
        CalculusUtils.EvaluateAt(result, "x", 2.0).Should().BeApproximately(4.0, 1e-10);
    }

    [Fact]
    public void Integrate_XPow4()
    {
        var expr = Expr.Pow(X, Expr.Literal(4));

        var result = _integ.IndefiniteIntegrate(expr, "x");

        CalculusUtils.EvaluateAt(result, "x", 1.0).Should().BeApproximately(0.2, 1e-10);
    }

    // ─── Trigonometric integrals ───

    [Fact]
    public void Integrate_SinX_IsNegCosX()
    {
        var expr = Expr.Sin(X);

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
        var resultType = result.GetType().Name;
        CalculusUtils.EvaluateAt(result, "x", PI).Should().BeApproximately(1.0, 1e-8);
    }

    [Fact]
    public void Integrate_CosX_IsSinX()
    {
        var expr = Expr.Cos(X);

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
        CalculusUtils.EvaluateAt(result, "x", PI / 2).Should().BeApproximately(1.0, 1e-8);
    }

    // ─── Exponential integrals ───

    [Fact]
    public void Integrate_ExpX_IsExpX()
    {
        var expr = Expr.Exp(X);

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
        CalculusUtils.EvaluateAt(result, "x", 0).Should().BeApproximately(1.0, 1e-8);
    }

    [Fact]
    public void Integrate_Exp2X()
    {
        var expr = Expr.Exp(Expr.Multiply(Expr.Literal(2), X));

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
        CalculusUtils.EvaluateAt(result, "x", 0).Should().BeApproximately(0.5, 1e-8);
    }

    // ─── Logarithmic integrals ───

    [Fact]
    public void Integrate_1OverX_IsLnAbsX()
    {
        var expr = Expr.Divide(Expr.Literal(1), X);

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
    }

    // ─── Constant integral ───

    [Fact]
    public void Integrate_Constant_CTimesX()
    {
        var expr = Expr.Literal(5);

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
        CalculusUtils.EvaluateAt(result, "x", 2.0).Should().BeApproximately(10.0, 1e-10);
    }

    // ─── Sum integral ───

    [Fact]
    public void Integrate_XPlusXSquared()
    {
        var expr = Expr.Add(X, Expr.Pow(X, Expr.Literal(2)));

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeNull();
    }

    // ─── Difference integral ───

    [Fact]
    public void Integrate_XMinusXSquared()
    {
        var expr = Expr.Subtract(X, Expr.Pow(X, Expr.Literal(2)));

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeNull();
    }

    // ─── Negate ───

    [Fact]
    public void Integrate_NegateX_IsNegXSquaredOver2()
    {
        var expr = Expr.Negate(X);

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeNull();
    }

    // ─── Definite integral ───

    [Fact]
    public void DefiniteIntegrate_X_0To1_IsHalf()
    {
        var result = _integ.DefiniteIntegrate(X, "x", Expr.Literal(0), Expr.Literal(1));

        result.Should().NotBeNull();
    }

    [Fact]
    public void DefiniteIntegrate_XSquared_0To1_IsThird()
    {
        var expr = Expr.Pow(X, Expr.Literal(2));

        var result = _integ.DefiniteIntegrate(expr, "x", Expr.Literal(0), Expr.Literal(1));

        result.As<LiteralExpression>().Value.Should().BeApproximately(1.0 / 3.0, 1e-10);
    }

    [Fact]
    public void DefiniteIntegrate_SinX_0ToPI_Is2()
    {
        var result = _integ.DefiniteIntegrate(Expr.Sin(X), "x", Expr.Literal(0), Expr.Literal(PI));

        result.As<LiteralExpression>().Value.Should().BeApproximately(2.0, 1e-8);
    }

    [Fact]
    public void DefiniteIntegrate_CosX_0ToPI_Is0()
    {
        var result = _integ.DefiniteIntegrate(Expr.Cos(X), "x", Expr.Literal(0), Expr.Literal(PI));

        result.As<LiteralExpression>().Value.Should().BeApproximately(0.0, 1e-8);
    }

    [Fact]
    public void DefiniteIntegrate_ExpX_0To1_IsEMinus1()
    {
        var result = _integ.DefiniteIntegrate(Expr.Exp(X), "x", Expr.Literal(0), Expr.Literal(1));

        result.As<LiteralExpression>().Value.Should().BeApproximately(E - 1.0, 1e-8);
    }

    // ─── Integral of integral returns integral ───

    [Fact]
    public void IndefiniteIntegrate_AlreadyIntegral_ReturnsSame()
    {
        var integral = Expr.Integral(X, Expr.Variable("x"));

        var result = _integ.IndefiniteIntegrate(integral, "x");

        result.Should().BeSameAs(integral);
    }

    // ─── sqrt(x) integral ───

    [Fact]
    public void Integrate_SqrtX()
    {
        var expr = Expr.Sqrt(X);

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeOfType<IntegralExpression>();
        CalculusUtils.EvaluateAt(result, "x", 4.0).Should().BeApproximately(16.0 / 3.0, 1e-6);
    }

    // ─── Negative exponent ───

    [Fact]
    public void Integrate_XPowNeg1_ReturnsIntegralExpression()
    {
        var expr = Expr.Pow(X, Expr.Literal(-1));

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().BeOfType<IntegralExpression>();
    }

    // ─── Definite: x^2 0 to 2 is 8/3 ───

    [Fact]
    public void DefiniteIntegrate_XSquared_0To2_Is8Over3()
    {
        var expr = Expr.Pow(X, Expr.Literal(2));

        var result = _integ.DefiniteIntegrate(expr, "x", Expr.Literal(0), Expr.Literal(2));

        result.As<LiteralExpression>().Value.Should().BeApproximately(8.0 / 3.0, 1e-10);
    }

    // ─── Sum of two functions definite ───

    [Fact]
    public void DefiniteIntegrate_SinPlusCos_0ToPIover2()
    {
        var expr = Expr.Add(Expr.Sin(X), Expr.Cos(X));

        var result = _integ.DefiniteIntegrate(expr, "x", Expr.Literal(0), Expr.Literal(PI / 2));

        result.As<LiteralExpression>().Value.Should().BeApproximately(2.0, 1e-8);
    }

    // ─── Null argument validation ───

    [Fact]
    public void IndefiniteIntegrate_NullExpr_Throws()
    {
        var act = () => _integ.IndefiniteIntegrate(null!, "x");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IndefiniteIntegrate_NullVariable_Throws()
    {
        var act = () => _integ.IndefiniteIntegrate(X, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DefiniteIntegrate_NullExpr_Throws()
    {
        var act = () => _integ.DefiniteIntegrate(null!, "x", Expr.Literal(0), Expr.Literal(1));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DefiniteIntegrate_NullLower_Throws()
    {
        var act = () => _integ.DefiniteIntegrate(X, "x", null!, Expr.Literal(1));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DefiniteIntegrate_NullUpper_Throws()
    {
        var act = () => _integ.DefiniteIntegrate(X, "x", Expr.Literal(0), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ─── Definite integral of polynomial sum ───

    [Fact]
    public void DefiniteIntegrate_XPlusXSquared_0To1()
    {
        var expr = Expr.Add(X, Expr.Pow(X, Expr.Literal(2)));

        var result = _integ.DefiniteIntegrate(expr, "x", Expr.Literal(0), Expr.Literal(1));

        result.Should().NotBeNull();
    }

    // ─── Definite integral of negated function ───

    [Fact]
    public void DefiniteIntegrate_NegateX_0To1_IsNegHalf()
    {
        var expr = Expr.Negate(X);

        var result = _integ.DefiniteIntegrate(expr, "x", Expr.Literal(0), Expr.Literal(1));

        result.Should().NotBeNull();
    }

    [Fact]
    public void DefiniteIntegrate_SinX_0To2PI_Is0()
    {
        var result = _integ.DefiniteIntegrate(Expr.Sin(X), "x", Expr.Literal(0), Expr.Literal(2 * PI));

        result.As<LiteralExpression>().Value.Should().BeApproximately(0.0, 1e-8);
    }

    // ─── x^3 0 to 1 is 1/4 ───

    [Fact]
    public void DefiniteIntegrate_XCubed_0To1_IsQuarter()
    {
        var expr = Expr.Pow(X, Expr.Literal(3));

        var result = _integ.DefiniteIntegrate(expr, "x", Expr.Literal(0), Expr.Literal(1));

        result.As<LiteralExpression>().Value.Should().BeApproximately(0.25, 1e-10);
    }

    // ─── 1 + x + x^2 integral ───

    [Fact]
    public void Integrate_1PlusXPlusXSquared()
    {
        var expr = Expr.Add(Expr.Add(Expr.Literal(1), X), Expr.Pow(X, Expr.Literal(2)));

        var result = _integ.IndefiniteIntegrate(expr, "x");

        result.Should().NotBeNull();
    }
}
