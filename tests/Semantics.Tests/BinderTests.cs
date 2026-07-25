using FluentAssertions;
using MathVerse.Math.Semantics;
using MathVerse.Math.Semantics.Binding;
using MathVerse.Math.Semantics.Diagnostics;
using MathVerse.Math.Semantics.Resolution;
using MathVerse.Math.Semantics.Symbols;
using MathVerse.Math.Expressions;

namespace MathVerse.Semantics.Tests;

public class BinderTests
{
    private static BindingResult Bind(string expression)
    {
        var analyzer = new SemanticAnalyzer();
        var model = analyzer.Analyze(expression);
        return new BindingResult(model.BoundTree, model.Diagnostics);
    }

    [Fact]
    public void Bind_LiteralNumber()
    {
        var result = Bind("42");
        result.Expression.Should().BeOfType<BoundLiteralExpression>();
        ((BoundLiteralExpression)result.Expression).Value.Should().Be(42.0);
    }

    [Fact]
    public void Bind_NegativeNumber()
    {
        var result = Bind("-5");
        result.Expression.Should().BeOfType<BoundUnaryExpression>();
    }

    [Fact]
    public void Bind_Addition()
    {
        var result = Bind("2 + 3");
        result.Expression.Should().BeOfType<BoundBinaryExpression>();
        var bin = (BoundBinaryExpression)result.Expression;
        bin.Operator.Symbol.Should().Be("+");
        ((BoundLiteralExpression)bin.Left).Value.Should().Be(2.0);
        ((BoundLiteralExpression)bin.Right).Value.Should().Be(3.0);
    }

    [Fact]
    public void Bind_Subtraction()
    {
        var result = Bind("10 - 4");
        var bin = (BoundBinaryExpression)result.Expression;
        bin.Operator.Symbol.Should().Be("-");
    }

    [Fact]
    public void Bind_Multiplication()
    {
        var result = Bind("3 * 7");
        var bin = (BoundBinaryExpression)result.Expression;
        bin.Operator.Symbol.Should().Be("*");
    }

    [Fact]
    public void Bind_Division()
    {
        var result = Bind("8 / 2");
        var bin = (BoundBinaryExpression)result.Expression;
        bin.Operator.Symbol.Should().Be("/");
    }

    [Fact]
    public void Bind_Power()
    {
        var result = Bind("2 ^ 3");
        var bin = (BoundBinaryExpression)result.Expression;
        bin.Operator.Symbol.Should().Be("^");
    }

    [Fact]
    public void Bind_Parenthesized()
    {
        var result = Bind("(2 + 3)");
        result.Expression.Should().BeOfType<BoundBinaryExpression>();
    }

    [Fact]
    public void Bind_ComplexExpression()
    {
        var result = Bind("2 * (3 + 4)");
        result.Expression.Should().BeOfType<BoundBinaryExpression>();
        var bin = (BoundBinaryExpression)result.Expression;
        bin.Operator.Symbol.Should().Be("*");
        bin.Right.Should().BeOfType<BoundBinaryExpression>();
    }

    [Fact]
    public void Bind_KnownConstant()
    {
        var result = Bind("π");
        result.Expression.Should().BeOfType<BoundConstantExpression>();
    }

    [Fact]
    public void Bind_BuiltinFunction()
    {
        var result = Bind("sin(1.0)");
        result.Expression.Should().BeOfType<BoundFunctionCallExpression>();
        var fc = (BoundFunctionCallExpression)result.Expression;
        fc.Function.Name.Should().Be("sin");
        fc.Arguments.Should().HaveCount(1);
    }

    [Fact]
    public void Bind_TwoArgFunction()
    {
        var result = Bind("pow(2, 3)");
        var fc = (BoundFunctionCallExpression)result.Expression;
        fc.Function.Name.Should().Be("pow");
        fc.Arguments.Should().HaveCount(2);
    }

    [Fact]
    public void Bind_UndefinedVariable_Diagnostic()
    {
        var result = Bind("x");
        result.Diagnostics.HasErrors.Should().BeTrue();
        result.Diagnostics.All.Any(d => d.Code == SemanticDiagnosticCode.UndefinedVariable).Should().BeTrue();
    }

    [Fact]
    public void Bind_UndefinedFunction_Diagnostic()
    {
        var result = Bind("unknown(1)");
        result.Diagnostics.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Bind_FunctionTooFewArgs_Diagnostic()
    {
        var result = Bind("sin()");
        result.Diagnostics.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Bind_RelationalExpression()
    {
        var result = Bind("2 < 3");
        result.Expression.Should().BeOfType<BoundBinaryExpression>();
    }

    [Fact]
    public void Bind_BooleanLiteral()
    {
        var result = Bind("true");
        result.Expression.Should().BeOfType<BoundLiteralExpression>();
        ((BoundLiteralExpression)result.Expression).Value.Should().Be(1.0);
    }

    [Fact]
    public void Bind_Equation()
    {
        var result = Bind("x = 5");
        result.Expression.Should().BeOfType<BoundBinaryExpression>();
    }

    [Fact]
    public void Bind_Piecewise()
    {
        var result = Bind("piecewise(1, true, 0)");
        result.Expression.Should().NotBeNull();
    }

    [Fact]
    public void Bind_Vector()
    {
        var result = Bind("[1, 2, 3]");
        result.Expression.Should().NotBeNull();
    }

    [Fact]
    public void Bind_Set()
    {
        var result = Bind("{1, 2, 3}");
        result.Expression.Should().NotBeNull();
    }

    [Fact]
    public void Bind_Tuple()
    {
        var result = Bind("(1, 2, 3)");
        result.Expression.Should().NotBeNull();
    }

    [Fact]
    public void Bind_Factorial()
    {
        var result = Bind("5!");
        result.Expression.Should().BeOfType<BoundLiteralExpression>();
        ((BoundLiteralExpression)result.Expression).Value.Should().Be(120.0);
    }

    [Fact]
    public void Bind_Derivative()
    {
        var result = Bind("d/dx sin(x)");
        result.Expression.Should().NotBeNull();
    }

    [Fact]
    public void Bind_Summation()
    {
        var result = Bind("sum(i=1,10,i)");
        result.Expression.Should().NotBeNull();
    }

    [Fact]
    public void Bind_Product()
    {
        var result = Bind("prod(i=1,5,i)");
        result.Expression.Should().NotBeNull();
    }

    [Fact]
    public void Bind_Limit()
    {
        var result = Bind("lim(x->0, sin(x)/x)");
        result.Expression.Should().NotBeNull();
    }
}
