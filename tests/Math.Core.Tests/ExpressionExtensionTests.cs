namespace MathVerse.Math.Core.Tests;

public class ExpressionExtensionTests
{
    [Fact]
    public void IsConstant_Literal_ReturnsTrue()
    {
        Expr.Literal(5.0).IsConstant().Should().BeTrue();
    }

    [Fact]
    public void IsConstant_Variable_ReturnsFalse()
    {
        Expr.Variable("x").IsConstant().Should().BeFalse();
    }

    [Fact]
    public void IsConstant_BinaryWithLiterals_ReturnsTrue()
    {
        Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)).IsConstant().Should().BeTrue();
    }

    [Fact]
    public void IsConstant_BinaryWithVariable_ReturnsFalse()
    {
        Expr.Add(Expr.Variable("x"), Expr.Literal(1.0)).IsConstant().Should().BeFalse();
    }

    [Fact]
    public void IsConstant_FunctionCallWithVariable_ReturnsFalse()
    {
        Expr.Sin(Expr.Variable("x")).IsConstant().Should().BeFalse();
    }

    [Fact]
    public void IsConstant_Constant_ReturnsTrue()
    {
        Expr.Constant("pi", System.Math.PI).IsConstant().Should().BeTrue();
    }

    [Fact]
    public void IsVariable_VariableExpression_ReturnsTrue()
    {
        Expr.Variable("x").IsVariable().Should().BeTrue();
    }

    [Fact]
    public void IsVariable_LiteralExpression_ReturnsFalse()
    {
        Expr.Literal(5.0).IsVariable().Should().BeFalse();
    }

    [Fact]
    public void IsVariable_BinaryExpression_ReturnsFalse()
    {
        Expr.Add(Expr.Variable("x"), Expr.Literal(1.0)).IsVariable().Should().BeFalse();
    }

    [Fact]
    public void IsZero_ZeroLiteral_ReturnsTrue()
    {
        Expr.Literal(0.0).IsZero().Should().BeTrue();
    }

    [Fact]
    public void IsZero_NonZeroLiteral_ReturnsFalse()
    {
        Expr.Literal(1.0).IsZero().Should().BeFalse();
    }

    [Fact]
    public void IsZero_NegativeZero_ReturnsTrue()
    {
        Expr.Literal(-0.0).IsZero().Should().BeTrue();
    }

    [Fact]
    public void IsZero_Variable_ReturnsFalse()
    {
        Expr.Variable("x").IsZero().Should().BeFalse();
    }

    [Fact]
    public void IsOne_OneLiteral_ReturnsTrue()
    {
        Expr.Literal(1.0).IsOne().Should().BeTrue();
    }

    [Fact]
    public void IsOne_NonOneLiteral_ReturnsFalse()
    {
        Expr.Literal(2.0).IsOne().Should().BeFalse();
    }

    [Fact]
    public void IsOne_ZeroLiteral_ReturnsFalse()
    {
        Expr.Literal(0.0).IsOne().Should().BeFalse();
    }

    [Fact]
    public void GetDoubleValue_Literal_ReturnsDefinedValue()
    {
        var result = Expr.Literal(42.0).GetDoubleValue();
        result.Match(
            v => { v.Should().Be(42.0); return 0; },
            _ => { false.Should().BeTrue(); return 0; });
    }

    [Fact]
    public void GetDoubleValue_Constant_ReturnsDefinedValue()
    {
        var result = Expr.Constant("pi", System.Math.PI).GetDoubleValue();
        result.Match(
            v => { v.Should().Be(System.Math.PI); return 0; },
            _ => { false.Should().BeTrue(); return 0; });
    }

    [Fact]
    public void GetDoubleValue_NegateLiteral_ReturnsNegativeValue()
    {
        var expr = Expr.Negate(Expr.Literal(5.0));
        var result = expr.GetDoubleValue();
        result.Match(
            v => { v.Should().Be(-5.0); return 0; },
            _ => { false.Should().BeTrue(); return 0; });
    }

    [Fact]
    public void GetDoubleValue_Variable_ReturnsUndefined()
    {
        var result = Expr.Variable("x").GetDoubleValue();
        result.Match(
            _ => { false.Should().BeTrue(); return 0; },
            _ => { true.Should().BeTrue(); return 0; });
    }

    [Fact]
    public void GetDoubleValue_BinaryExpression_ReturnsUndefined()
    {
        var result = Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)).GetDoubleValue();
        result.Match(
            _ => { false.Should().BeTrue(); return 0; },
            _ => { true.Should().BeTrue(); return 0; });
    }

    [Fact]
    public void Variables_Literal_ReturnsEmptySet()
    {
        Expr.Literal(5.0).Variables().Should().BeEmpty();
    }

    [Fact]
    public void Variables_SingleVariable_ReturnsOneVariable()
    {
        Expr.Variable("x").Variables().Should().ContainSingle("x");
    }

    [Fact]
    public void Variables_BinaryWithVariables_ReturnsAllVariables()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        var variables = expr.Variables();
        variables.Should().HaveCount(2);
        variables.Should().Contain("x");
        variables.Should().Contain("y");
    }

    [Fact]
    public void Variables_ComplexExpression_ReturnsAllDistinctVariables()
    {
        var expr = Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Variable("y")),
            Expr.Variable("x"));
        var variables = expr.Variables();
        variables.Should().HaveCount(2);
        variables.Should().Contain("x");
        variables.Should().Contain("y");
    }

    [Fact]
    public void Variables_FunctionCallWithVariable_ReturnsVariable()
    {
        var expr = Expr.Sin(Expr.Variable("x"));
        expr.Variables().Should().ContainSingle("x");
    }

    [Fact]
    public void IsInteger_IntegerLiteral_ReturnsTrue()
    {
        Expr.Literal(5.0).IsInteger().Should().BeTrue();
    }

    [Fact]
    public void IsInteger_NonIntegerLiteral_ReturnsFalse()
    {
        Expr.Literal(3.14).IsInteger().Should().BeFalse();
    }

    [Fact]
    public void IsInteger_ZeroLiteral_ReturnsTrue()
    {
        Expr.Literal(0.0).IsInteger().Should().BeTrue();
    }

    [Fact]
    public void IsNumericLiteral_Literal_ReturnsTrue()
    {
        Expr.Literal(5.0).IsNumericLiteral().Should().BeTrue();
    }

    [Fact]
    public void IsNumericLiteral_Constant_ReturnsTrue()
    {
        Expr.Constant("e", System.Math.E).IsNumericLiteral().Should().BeTrue();
    }

    [Fact]
    public void IsNumericLiteral_Variable_ReturnsFalse()
    {
        Expr.Variable("x").IsNumericLiteral().Should().BeFalse();
    }

    [Fact]
    public void ReplaceVariable_ReplacesCorrectly()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        var result = expr.ReplaceVariable("x", Expr.Literal(10.0));
        var variables = result.Variables();
        variables.Should().BeEmpty();
        result.IsConstant().Should().BeTrue();
    }

    [Fact]
    public void ReplaceVariable_DoesNotReplaceUnrelatedVariables()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        var result = expr.ReplaceVariable("x", Expr.Literal(10.0));
        var variables = result.Variables();
        variables.Should().ContainSingle("y");
    }

    [Fact]
    public void ReplaceVariable_NestedExpression_ReplacesAllOccurrences()
    {
        var expr = Expr.Multiply(Expr.Variable("x"), Expr.Variable("x"));
        var result = expr.ReplaceVariable("x", Expr.Literal(3.0));
        result.IsConstant().Should().BeTrue();
        result.Variables().Should().BeEmpty();
    }

    [Fact]
    public void BooleanExpression_GetDoubleValue_OneForTrue()
    {
        Expr.Boolean(true).GetDoubleValue().Match(
            v => { v.Should().Be(1.0); return 0; },
            _ => { false.Should().BeTrue(); return 0; });
    }

    [Fact]
    public void BooleanExpression_GetDoubleValue_ZeroForFalse()
    {
        Expr.Boolean(false).GetDoubleValue().Match(
            v => { v.Should().Be(0.0); return 0; },
            _ => { false.Should().BeTrue(); return 0; });
    }
}
