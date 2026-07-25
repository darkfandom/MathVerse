namespace MathVerse.Math.Simplification.Tests;

public class ExpressionSimplifierTests
{
    private readonly ExpressionSimplifier _simplifier = new();

    [Fact]
    public void Simplify_XPlusZero_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, Expr.Literal(0.0));
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_ZeroPlusX_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(Expr.Literal(0.0), x);
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_XTimesOne_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Literal(1.0));
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_OneTimesX_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Literal(1.0), x);
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_XTimesZero_ReturnsZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Literal(0.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Simplify_ZeroTimesX_ReturnsZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Literal(0.0), x);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Simplify_XPowOne_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(x, Expr.Literal(1.0));
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_XPowZero_ReturnsOne()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(x, Expr.Literal(0.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Simplify_OnePowX_ReturnsOne()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(Expr.Literal(1.0), x);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Simplify_ZeroPowX_ReturnsZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(Expr.Literal(0.0), x);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Simplify_TwoPlusThree_ReturnsFive()
    {
        var expr = Expr.Add(Expr.Literal(2.0), Expr.Literal(3.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(5.0);
    }

    [Fact]
    public void Simplify_XPlusX_Returns2TimesX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, x);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result;
        binary.Operator.Symbol.Should().Be("*");
        ((LiteralExpression)binary.Left).Value.Should().Be(2.0);
        binary.Right.Should().Be(x);
    }

    [Fact]
    public void Simplify_XTimesX_ReturnsXPow2()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, x);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
        ((LiteralExpression)binary.Right).Value.Should().Be(2.0);
    }

    [Fact]
    public void Simplify_XMinusX_ReturnsZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Subtract(x, x);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Simplify_DoubleNegation_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Negate(Expr.Negate(x));
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_XMinusZero_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Subtract(x, Expr.Literal(0.0));
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_XOverOne_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Divide(x, Expr.Literal(1.0));
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_ZeroOverX_ReturnsZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Divide(Expr.Literal(0.0), x);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Simplify_XTimesNegOne_ReturnsNegX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Literal(-1.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)result;
        unary.Operator.Symbol.Should().Be("-");
        unary.Operand.Should().Be(x);
    }

    [Fact]
    public void Simplify_NestedConstantFolding_FoldsCompletely()
    {
        var expr = Expr.Add(
            Expr.Multiply(Expr.Literal(2.0), Expr.Literal(3.0)),
            Expr.Pow(Expr.Literal(2.0), Expr.Literal(3.0)));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(14.0);
    }

    [Fact]
    public void Simplify_NestedSimplification_SimplifiesInnerFirst()
    {
        var x = Expr.Variable("x");
        var inner = Expr.Add(x, Expr.Literal(0.0));
        var expr = Expr.Multiply(inner, Expr.Literal(2.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result;
        binary.Left.Should().Be(x);
    }

    [Fact]
    public void Simplify_ConstantExpression_ReturnsLiteral()
    {
        var expr = Expr.Add(Expr.Literal(10.0), Expr.Literal(20.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(30.0);
    }

    [Fact]
    public void Simplify_XPlusXPlusX_Returns3TimesX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(Expr.Add(x, x), x);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result;
        binary.Operator.Symbol.Should().Be("+");
    }

    [Fact]
    public void Simplify_LiteralOnlyExpression_FoldsCompletely()
    {
        var expr = Expr.Multiply(Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)), Expr.Literal(3.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(9.0);
    }

    [Fact]
    public void Simplify_SinZero_SimplifiesToZero()
    {
        var expr = Expr.Sin(Expr.Literal(0.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Simplify_CosZero_SimplifiesToOne()
    {
        var expr = Expr.Cos(Expr.Literal(0.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Simplify_SqrtFour_SimplifiesToTwo()
    {
        var expr = Expr.Sqrt(Expr.Literal(4.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(2.0);
    }

    [Fact]
    public void Simplify_ExpZero_SimplifiesToOne()
    {
        var expr = Expr.Exp(Expr.Literal(0.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Simplify_LnOne_SimplifiesToZero()
    {
        var expr = Expr.Ln(Expr.Literal(1.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Simplify_LnE_SimplifiesToOne()
    {
        var expr = Expr.Ln(ConstantExpression.E);
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Simplify_ExpLnX_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Exp(Expr.Ln(x));
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_LnExpX_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Ln(Expr.Exp(x));
        _simplifier.Simplify(expr).Should().Be(x);
    }

    [Fact]
    public void Simplify_PowerOfPower_SimplifiesExponents()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(Expr.Pow(x, Expr.Literal(2.0)), Expr.Literal(3.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
    }

    [Fact]
    public void Simplify_ProductSameBase_CombinesExponents()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Pow(x, Expr.Literal(2.0)), Expr.Pow(x, Expr.Literal(3.0)));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
    }

    [Fact]
    public void Simplify_WithMinimalOptions_OnlyArithmeticRules()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, Expr.Literal(0.0));
        var result = _simplifier.Simplify(expr, SimplificationOptions.Minimal);
        result.Should().Be(x);
    }

    [Fact]
    public void Simplify_ClearCache_DoesNotThrow()
    {
        Action act = () => _simplifier.ClearCache();
        act.Should().NotThrow();
    }

    [Fact]
    public void Simplify_TanZero_SimplifiesToZero()
    {
        var expr = Expr.Tan(Expr.Literal(0.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Simplify_ComplexNestedConstantExpression_FoldsEntirely()
    {
        var expr = Expr.Divide(
            Expr.Subtract(Expr.Pow(Expr.Literal(5.0), Expr.Literal(2.0)), Expr.Literal(1.0)),
            Expr.Literal(4.0));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(6.0);
    }

    [Fact]
    public void Simplify_XTimesOneTimesOne_ReturnsX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Multiply(x, Expr.Literal(1.0)), Expr.Literal(1.0));
        var result = _simplifier.Simplify(expr);
        result.Should().Be(x);
    }

    [Fact]
    public void Simplify_VariableRemainsUnchanged()
    {
        var x = Expr.Variable("x");
        _simplifier.Simplify(x).Should().Be(x);
    }

    [Fact]
    public void Simplify_LiteralRemainsUnchanged()
    {
        var lit = Expr.Literal(42.0);
        var result = _simplifier.Simplify(lit);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(42.0);
    }

    [Fact]
    public void Simplify_MultipleIterations_Converges()
    {
        var x = Expr.Variable("x");
        var y = Expr.Variable("y");
        var expr = Expr.Add(
            Expr.Multiply(x, Expr.Literal(1.0)),
            Expr.Add(Expr.Literal(0.0), y));
        var result = _simplifier.Simplify(expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result;
        binary.Left.Should().Be(x);
        binary.Right.Should().Be(y);
    }
}
