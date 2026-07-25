namespace MathVerse.Expression.Tests;

public class FunctionExpressionTests
{
    // ─── FunctionCallExpression ───

    [Fact]
    public void FunctionCallExpression_Kind_IsFunctionCall()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Call("sin", x);

        expr.Kind.Should().Be(ExpressionKind.FunctionCall);
    }

    [Fact]
    public void FunctionCallExpression_Properties_AreCorrect()
    {
        var x = Expr.Variable("x");
        var y = Expr.Variable("y");
        var expr = Expr.Call("f", x, y);

        expr.Name.Should().Be("f");
        expr.Arguments.Should().HaveCount(2);
        expr.Arguments[0].Should().BeSameAs(x);
        expr.Arguments[1].Should().BeSameAs(y);
    }

    [Fact]
    public void FunctionCallExpression_Children_EqualsArguments()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Call("abs", x);

        expr.Children.Should().BeSameAs(expr.Arguments);
    }

    [Fact]
    public void FunctionCallExpression_NullName_Throws()
    {
        var act = () => new FunctionCallExpression(null!, [Expr.Literal(1)]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FunctionCallExpression_EmptyName_Throws()
    {
        var act = () => new FunctionCallExpression("", [Expr.Literal(1)]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FunctionCallExpression_EqualWhenSameNameAndArguments()
    {
        var x = Expr.Variable("x");
        var a = Expr.Sin(x);
        var b = Expr.Sin(x);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void FunctionCallExpression_NotEqualWhenDifferentName()
    {
        var x = Expr.Variable("x");
        var a = Expr.Sin(x);
        var b = Expr.Cos(x);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void FunctionCallExpression_NotEqualWhenDifferentArguments()
    {
        var x = Expr.Variable("x");
        var y = Expr.Variable("y");
        var a = Expr.Call("f", x);
        var b = Expr.Call("f", y);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void FunctionCallExpression_SinCosTan_FactoryMethods()
    {
        var x = Expr.Variable("x");

        Expr.Sin(x).Name.Should().Be("sin");
        Expr.Cos(x).Name.Should().Be("cos");
        Expr.Tan(x).Name.Should().Be("tan");
    }

    [Fact]
    public void FunctionCallExpression_ComputedDepthAndNodeCount()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Call("sin", x);

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(2);
    }

    // ─── ParameterExpression ───

    [Fact]
    public void ParameterExpression_Properties_AreCorrect()
    {
        var param = Expr.Parameter("x");

        param.Name.Should().Be("x");
        param.Kind.Should().Be(ExpressionKind.Parameter);
        param.Depth.Should().Be(0);
        param.NodeCount.Should().Be(1);
    }

    [Fact]
    public void ParameterExpression_Children_IsEmpty()
    {
        var param = Expr.Parameter("x");

        param.Children.Should().BeEmpty();
    }

    [Fact]
    public void ParameterExpression_NullName_Throws()
    {
        var act = () => new ParameterExpression(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ParameterExpression_EmptyName_Throws()
    {
        var act = () => new ParameterExpression("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ParameterExpression_EqualWhenSameName()
    {
        var a = Expr.Parameter("x");
        var b = Expr.Parameter("x");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void ParameterExpression_NotEqualWhenDifferentName()
    {
        var a = Expr.Parameter("x");
        var b = Expr.Parameter("y");

        a.Equals(b).Should().BeFalse();
    }

    // ─── LambdaExpression ───

    [Fact]
    public void LambdaExpression_Kind_IsLambda()
    {
        var p = Expr.Parameter("x");
        var lambda = Expr.Lambda(p, p);

        lambda.Kind.Should().Be(ExpressionKind.Lambda);
    }

    [Fact]
    public void LambdaExpression_Properties_AreCorrect()
    {
        var p = Expr.Parameter("x");
        var body = Expr.Variable("x");
        var lambda = Expr.Lambda(p, body);

        lambda.Parameters.Should().HaveCount(1);
        lambda.Parameters[0].Should().BeSameAs(p);
        lambda.Body.Should().BeSameAs(body);
    }

    [Fact]
    public void LambdaExpression_Children_IncludesParametersAndBody()
    {
        var p = Expr.Parameter("x");
        var body = Expr.Variable("x");
        var lambda = Expr.Lambda(p, body);

        lambda.Children.Should().HaveCount(2);
        lambda.Children[0].Should().BeSameAs(p);
        lambda.Children[1].Should().BeSameAs(body);
    }

    [Fact]
    public void LambdaExpression_EqualWhenSameParametersAndBody()
    {
        var p1 = Expr.Parameter("x");
        var body1 = Expr.Variable("x");
        var a = Expr.Lambda(p1, body1);

        var p2 = Expr.Parameter("x");
        var body2 = Expr.Variable("x");
        var b = Expr.Lambda(p2, body2);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void LambdaExpression_NotEqualWhenDifferentBody()
    {
        var p = Expr.Parameter("x");
        var a = Expr.Lambda(p, p);
        var b = Expr.Lambda(p, Expr.Variable("y"));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void LambdaExpression_NullBody_Throws()
    {
        var p = Expr.Parameter("x");
        var act = () => Expr.Lambda(p, null!);

        act.Should().Throw<Exception>();
    }

    // ─── EquationExpression ───

    [Fact]
    public void EquationExpression_Kind_IsEquation()
    {
        var eq = Expr.Equation(Expr.Literal(1), Expr.Literal(2));

        eq.Kind.Should().Be(ExpressionKind.Equation);
    }

    [Fact]
    public void EquationExpression_Properties_AreCorrect()
    {
        var left = Expr.Variable("x");
        var right = Expr.Literal(5);
        var eq = Expr.Equation(left, right);

        eq.Left.Should().BeSameAs(left);
        eq.Right.Should().BeSameAs(right);
    }

    [Fact]
    public void EquationExpression_Children_IsLeftAndRight()
    {
        var eq = Expr.Equation(Expr.Literal(1), Expr.Literal(2));

        eq.Children.Should().HaveCount(2);
        eq.Children[0].Should().BeSameAs(eq.Left);
        eq.Children[1].Should().BeSameAs(eq.Right);
    }

    [Fact]
    public void EquationExpression_EqualWhenSameLeftAndRight()
    {
        var left = Expr.Variable("x");
        var right = Expr.Literal(5);
        var a = Expr.Equation(left, right);
        var b = Expr.Equation(left, right);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void EquationExpression_NotEqualWhenDifferentRight()
    {
        var left = Expr.Variable("x");
        var a = Expr.Equation(left, Expr.Literal(5));
        var b = Expr.Equation(left, Expr.Literal(6));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void EquationExpression_NullLeft_Throws()
    {
        var act = () => new EquationExpression(null!, Expr.Literal(1));

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void EquationExpression_NullRight_Throws()
    {
        var act = () => new EquationExpression(Expr.Literal(1), null!);

        act.Should().Throw<Exception>();
    }
}
