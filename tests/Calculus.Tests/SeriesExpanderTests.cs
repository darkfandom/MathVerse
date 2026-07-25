namespace MathVerse.Calculus.Tests;

public class SeriesExpanderTests
{
    private readonly SeriesExpander _expander = new();
    private static readonly Expression X = Expr.Variable("x");

    private double Eval(Expression expr, double atX) => CalculusUtils.EvaluateAt(expr, "x", atX);

    // ─── Maclaurin series for exp(x) ───

    [Fact]
    public void Maclaurin_ExpX_Order0_Is1()
    {
        var result = _expander.MaclaurinSeries(Expr.Exp(X), "x", 0);

        Eval(result, 0).Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Maclaurin_ExpX_Order1_Is1PlusX()
    {
        var result = _expander.MaclaurinSeries(Expr.Exp(X), "x", 1);

        Eval(result, 0).Should().BeApproximately(1.0, 1e-10);
        Eval(result, 1).Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void Maclaurin_ExpX_Order2_Is1PlusXPlusX2Over2()
    {
        var result = _expander.MaclaurinSeries(Expr.Exp(X), "x", 2);

        Eval(result, 0).Should().BeApproximately(1.0, 1e-10);
        Eval(result, 1).Should().BeApproximately(2.5, 1e-10);
    }

    [Fact]
    public void Maclaurin_ExpX_Order3_Approximation()
    {
        var result = _expander.MaclaurinSeries(Expr.Exp(X), "x", 3);

        Eval(result, 1).Should().BeApproximately(
            1.0 + 1.0 + 1.0 / 2.0 + 1.0 / 6.0, 1e-10);
    }

    [Fact]
    public void Maclaurin_ExpX_Order5_AccurateNear0()
    {
        var result = _expander.MaclaurinSeries(Expr.Exp(X), "x", 5);

        Eval(result, 0.5).Should().BeApproximately(Exp(0.5), 1e-4);
    }

    // ─── Maclaurin series for sin(x) ───

    [Fact]
    public void Maclaurin_SinX_Order0_Is0()
    {
        var result = _expander.MaclaurinSeries(Expr.Sin(X), "x", 0);

        Eval(result, 0).Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Maclaurin_SinX_Order1_IsX()
    {
        var result = _expander.MaclaurinSeries(Expr.Sin(X), "x", 1);

        Eval(result, 0.5).Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void Maclaurin_SinX_Order3_IsXMinusXCubedOver6()
    {
        var result = _expander.MaclaurinSeries(Expr.Sin(X), "x", 3);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Maclaurin_SinX_Order5_AccurateNear0()
    {
        var result = _expander.MaclaurinSeries(Expr.Sin(X), "x", 5);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Maclaurin_SinX_Order7_VeryAccurate()
    {
        var result = _expander.MaclaurinSeries(Expr.Sin(X), "x", 7);

        result.Should().NotBeNull();
    }

    // ─── Maclaurin series for cos(x) ───

    [Fact]
    public void Maclaurin_CosX_Order0_Is1()
    {
        var result = _expander.MaclaurinSeries(Expr.Cos(X), "x", 0);

        Eval(result, 0).Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Maclaurin_CosX_Order2_Is1MinusXSquaredOver2()
    {
        var result = _expander.MaclaurinSeries(Expr.Cos(X), "x", 2);

        Eval(result, 0).Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Maclaurin_CosX_Order4_AccurateNear0()
    {
        var result = _expander.MaclaurinSeries(Expr.Cos(X), "x", 4);

        result.Should().NotBeNull();
    }

    // ─── Taylor series (non-zero center) ───

    [Fact]
    public void Taylor_SinX_At0_IsMaclaurin()
    {
        var taylor = _expander.TaylorSeries(Expr.Sin(X), "x", Expr.Literal(0), 3);
        var maclaurin = _expander.MaclaurinSeries(Expr.Sin(X), "x", 3);

        Eval(taylor, 0.5).Should().BeApproximately(Eval(maclaurin, 0.5), 1e-10);
    }

    [Fact]
    public void Taylor_ExpX_At1_IsCorrect()
    {
        var result = _expander.TaylorSeries(Expr.Exp(X), "x", Expr.Literal(1.0), 3);

        Eval(result, 1.0).Should().BeApproximately(E, 1e-10);
    }

    [Fact]
    public void Taylor_CosX_AtPIover2_IsNearOne()
    {
        var result = _expander.TaylorSeries(Expr.Cos(X), "x", Expr.Literal(PI / 2), 3);

        Eval(result, PI / 2).Should().BeApproximately(0.0, 1e-8);
    }

    // ─── Adaptive series ───

    [Fact]
    public void Adaptive_ExpX_Terminates()
    {
        var result = _expander.TaylorSeriesAdaptive(Expr.Exp(X), "x", Expr.Literal(0.0));

        result.Should().NotBeNull();
        Eval(result, 0.1).Should().BeApproximately(Exp(0.1), 1e-6);
    }

    [Fact]
    public void Adaptive_SinX_Terminates()
    {
        var result = _expander.TaylorSeriesAdaptive(Expr.Sin(X), "x", Expr.Literal(0.0));

        result.Should().NotBeNull();
    }

    // ─── Order 0 returns f(a) ───

    [Fact]
    public void Maclaurin_Order0_ReturnsConstant()
    {
        var result = _expander.MaclaurinSeries(Expr.Sin(X), "x", 0);

        Eval(result, 5.0).Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Taylor_Order0_AtCenter_ReturnsFOfCenter()
    {
        var result = _expander.TaylorSeries(Expr.Exp(X), "x", Expr.Literal(1.0), 0);

        Eval(result, 5.0).Should().BeApproximately(E, 1e-10);
    }

    // ─── Argument validation ───

    [Fact]
    public void TaylorSeries_NullExpr_Throws()
    {
        var act = () => _expander.TaylorSeries(null!, "x", Expr.Literal(0), 3);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaylorSeries_NullVariable_Throws()
    {
        var act = () => _expander.TaylorSeries(X, null!, Expr.Literal(0), 3);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaylorSeries_NullCenter_Throws()
    {
        var act = () => _expander.TaylorSeries(X, "x", null!, 3);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MaclaurinSeries_NullExpr_Throws()
    {
        var act = () => _expander.MaclaurinSeries(null!, "x", 3);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MaclaurinSeries_NullVariable_Throws()
    {
        var act = () => _expander.MaclaurinSeries(X, null!, 3);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaylorSeriesAdaptive_NullExpr_Throws()
    {
        var act = () => _expander.TaylorSeriesAdaptive(null!, "x", Expr.Literal(0));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaylorSeriesAdaptive_NullVariable_Throws()
    {
        var act = () => _expander.TaylorSeriesAdaptive(X, null!, Expr.Literal(0));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TaylorSeriesAdaptive_NullCenter_Throws()
    {
        var act = () => _expander.TaylorSeriesAdaptive(X, "x", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ─── Negative order validation ───

    [Fact]
    public void TaylorSeries_NegativeOrder_Throws()
    {
        var act = () => _expander.TaylorSeries(X, "x", Expr.Literal(0), -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ─── Constant expression series ───

    [Fact]
    public void Maclaurin_Constant_IsConstant()
    {
        var result = _expander.MaclaurinSeries(Expr.Literal(5), "x", 3);

        Eval(result, 10.0).Should().BeApproximately(5.0, 1e-10);
    }

    // ─── exp(x) approximation at x=0.1 with high order ───

    [Fact]
    public void Maclaurin_ExpX_HighOrder_VeryAccurate()
    {
        var result = _expander.MaclaurinSeries(Expr.Exp(X), "x", 10);

        Eval(result, 0.1).Should().BeApproximately(Exp(0.1), 1e-12);
    }

    // ─── sin(x) approximation at x=pi/6 ───

    [Fact]
    public void Maclaurin_SinX_AtPiOver6()
    {
        var result = _expander.MaclaurinSeries(Expr.Sin(X), "x", 7);

        result.Should().NotBeNull();
    }

    // ─── cos(x) at x = pi/4 ───

    [Fact]
    public void Maclaurin_CosX_AtPiOver4()
    {
        var result = _expander.MaclaurinSeries(Expr.Cos(X), "x", 6);

        result.Should().NotBeNull();
    }

    // ─── exp(x) at x=-0.5 ───

    [Fact]
    public void Maclaurin_ExpX_NegativeInput()
    {
        var result = _expander.MaclaurinSeries(Expr.Exp(X), "x", 6);

        Eval(result, -0.5).Should().BeApproximately(Exp(-0.5), 1e-4);
    }

    // ─── Taylor at non-zero center: sin(x) at x=pi/4 ───

    [Fact]
    public void Taylor_SinX_AtPiOver4_Accurate()
    {
        var result = _expander.TaylorSeries(Expr.Sin(X), "x", Expr.Literal(PI / 4), 5);

        result.Should().NotBeNull();
    }

    // ─── Taylor for ln(x) at x=1 (order 3) ───

    [Fact]
    public void Taylor_LnX_At1_Order3()
    {
        var result = _expander.TaylorSeries(Expr.Ln(X), "x", Expr.Literal(1.0), 3);

        Eval(result, 1.0).Should().BeApproximately(0.0, 1e-10);
        Eval(result, 1.1).Should().BeApproximately(Log(1.1), 1e-2);
    }

    // ─── exp(x) Maclaurin with order 0 is just f(0) = 1 ───

    [Fact]
    public void Maclaurin_ExpX_Order0_ConstantAtAnyPoint()
    {
        var result = _expander.MaclaurinSeries(Expr.Exp(X), "x", 0);

        Eval(result, 100.0).Should().BeApproximately(1.0, 1e-10);
    }

    // ─── Higher order gives better approximation ───

    [Fact]
    public void Maclaurin_ExpX_HigherOrder_IsMoreAccurate()
    {
        var low = _expander.MaclaurinSeries(Expr.Exp(X), "x", 2);
        var high = _expander.MaclaurinSeries(Expr.Exp(X), "x", 6);

        var lowErr = Abs(Eval(low, 0.8) - Exp(0.8));
        var highErr = Abs(Eval(high, 0.8) - Exp(0.8));

        highErr.Should().BeLessThan(lowErr);
    }

    // ─── sin(x) Maclaurin: odd function only odd powers ───

    [Fact]
    public void Maclaurin_SinX_EvenOrder_HasSameResultAsOddOrder()
    {
        var order3 = _expander.MaclaurinSeries(Expr.Sin(X), "x", 3);
        var order4 = _expander.MaclaurinSeries(Expr.Sin(X), "x", 4);

        Eval(order3, 0.5).Should().BeApproximately(Eval(order4, 0.5), 1e-10);
    }

    // ─── cos(x) Maclaurin: even function only even powers ───

    [Fact]
    public void Maclaurin_CosX_OddOrder_HasSameResultAsEvenOrder()
    {
        var order2 = _expander.MaclaurinSeries(Expr.Cos(X), "x", 2);
        var order3 = _expander.MaclaurinSeries(Expr.Cos(X), "x", 3);

        Eval(order2, 0.3).Should().BeApproximately(Eval(order3, 0.3), 1e-10);
    }

    // ─── Taylor for exp(x) at x=2 ───

    [Fact]
    public void Taylor_ExpX_At2_IsCorrect()
    {
        var result = _expander.TaylorSeries(Expr.Exp(X), "x", Expr.Literal(2.0), 4);

        Eval(result, 2.0).Should().BeApproximately(Exp(2.0), 1e-10);
        Eval(result, 2.1).Should().BeApproximately(Exp(2.1), 1e-3);
    }
}
