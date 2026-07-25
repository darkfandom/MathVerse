using FluentAssertions;
using MathVerse.Math.Semantics;
using MathVerse.Math.Semantics.Binding;
using MathVerse.Math.Semantics.Resolution;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class ConstantFolderTests
{
    private static ConstantFolder CreateFolder()
    {
        return new ConstantFolder(new MathVerse.Math.Semantics.Diagnostics.SemanticDiagnosticBag());
    }

    private static double? FoldValue(string expr)
    {
        var analyzer = new SemanticAnalyzer();
        var model = analyzer.Analyze(expr);
        return model.EvaluateConstant();
    }

    [Fact]
    public void Fold_Literal()
    {
        var folder = CreateFolder();
        var expr = new BoundLiteralExpression(42.0);
        folder.TryFold(expr).Should().Be(42.0);
    }

    [Fact]
    public void Fold_Constant()
    {
        var folder = CreateFolder();
        var c = new ConstantSymbol("pi", 3.14159);
        var expr = new BoundConstantExpression(c);
        folder.TryFold(expr).Should().Be(3.14159);
    }

    [Fact]
    public void Fold_Addition()
    {
        var folder = CreateFolder();
        var left = new BoundLiteralExpression(2.0);
        var right = new BoundLiteralExpression(3.0);
        var op = MathVerse.Math.Operators.MathOperator.Add;
        var bin = new BoundBinaryExpression(left, op, right);
        folder.TryFold(bin).Should().Be(5.0);
    }

    [Fact]
    public void Fold_Subtraction()
    {
        var folder = CreateFolder();
        var bin = new BoundBinaryExpression(
            new BoundLiteralExpression(10.0),
            MathVerse.Math.Operators.MathOperator.Subtract,
            new BoundLiteralExpression(4.0));
        folder.TryFold(bin).Should().Be(6.0);
    }

    [Fact]
    public void Fold_Multiplication()
    {
        var folder = CreateFolder();
        var bin = new BoundBinaryExpression(
            new BoundLiteralExpression(3.0),
            MathVerse.Math.Operators.MathOperator.Multiply,
            new BoundLiteralExpression(7.0));
        folder.TryFold(bin).Should().Be(21.0);
    }

    [Fact]
    public void Fold_Division()
    {
        var folder = CreateFolder();
        var bin = new BoundBinaryExpression(
            new BoundLiteralExpression(10.0),
            MathVerse.Math.Operators.MathOperator.Divide,
            new BoundLiteralExpression(2.0));
        folder.TryFold(bin).Should().Be(5.0);
    }

    [Fact]
    public void Fold_DivisionByZero()
    {
        var folder = CreateFolder();
        var bin = new BoundBinaryExpression(
            new BoundLiteralExpression(1.0),
            MathVerse.Math.Operators.MathOperator.Divide,
            new BoundLiteralExpression(0.0));
        folder.TryFold(bin).Should().Be(double.NaN);
    }

    [Fact]
    public void Fold_Power()
    {
        var folder = CreateFolder();
        var op = new MathVerse.Math.Operators.MathOperator("^", "Power",
            MathVerse.Math.Operators.OperatorCategory.Arithmetic, 2, 3,
            MathVerse.Math.Operators.OperatorAssociativity.Right);
        var bin = new BoundBinaryExpression(
            new BoundLiteralExpression(2.0), op,
            new BoundLiteralExpression(3.0));
        folder.TryFold(bin).Should().Be(8.0);
    }

    [Fact]
    public void Fold_Negate()
    {
        var folder = CreateFolder();
        var u = new BoundUnaryExpression(MathVerse.Math.Operators.MathOperator.Negate,
            new BoundLiteralExpression(5.0));
        folder.TryFold(u).Should().Be(-5.0);
    }

    [Fact]
    public void Fold_Sin()
    {
        var folder = CreateFolder();
        var func = new FunctionSymbol("sin", [new ParameterSymbol("x", 0)]);
        var call = new BoundFunctionCallExpression(func,
            [new BoundLiteralExpression(0.0)]);
        folder.TryFold(call).Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Fold_Cos()
    {
        var folder = CreateFolder();
        var func = new FunctionSymbol("cos", [new ParameterSymbol("x", 0)]);
        var call = new BoundFunctionCallExpression(func,
            [new BoundLiteralExpression(0.0)]);
        folder.TryFold(call).Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Fold_Sqrt()
    {
        var folder = CreateFolder();
        var func = new FunctionSymbol("sqrt", [new ParameterSymbol("x", 0)]);
        var call = new BoundFunctionCallExpression(func,
            [new BoundLiteralExpression(9.0)]);
        folder.TryFold(call).Should().Be(3.0);
    }

    [Fact]
    public void Fold_Abs()
    {
        var folder = CreateFolder();
        var func = new FunctionSymbol("abs", [new ParameterSymbol("x", 0)]);
        var call = new BoundFunctionCallExpression(func,
            [new BoundLiteralExpression(-5.0)]);
        folder.TryFold(call).Should().Be(5.0);
    }

    [Fact]
    public void Fold_Expression2Plus3()
    {
        FoldValue("2 + 3").Should().Be(5.0);
    }

    [Fact]
    public void Fold_ExpressionMultiply()
    {
        FoldValue("6 * 7").Should().Be(42.0);
    }

    [Fact]
    public void Fold_ExpressionNested()
    {
        FoldValue("(2 + 3) * 4").Should().Be(20.0);
    }

    [Fact]
    public void Fold_SinZero()
    {
        FoldValue("sin(0)").Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Fold_CosZero()
    {
        FoldValue("cos(0)").Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Fold_SqrtNine()
    {
        FoldValue("sqrt(9)").Should().Be(3.0);
    }

    [Fact]
    public void Fold_FactorialFive()
    {
        FoldValue("5!").Should().Be(120.0);
    }

    [Fact]
    public void Fold_ExpZero()
    {
        FoldValue("exp(0)").Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Fold_LnOne()
    {
        FoldValue("ln(1)").Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Fold_Log10()
    {
        FoldValue("log10(100)").Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void Fold_IsConstant_True()
    {
        var folder = CreateFolder();
        folder.IsConstant(new BoundLiteralExpression(1.0)).Should().BeTrue();
    }

    [Fact]
    public void Fold_IsConstant_False_ForVariable()
    {
        var folder = CreateFolder();
        folder.IsConstant(new BoundVariableExpression(new VariableSymbol("x"))).Should().BeFalse();
    }

    [Fact]
    public void Fold_ReplacesExpression()
    {
        var folder = CreateFolder();
        var bin = new BoundBinaryExpression(
            new BoundLiteralExpression(2.0),
            MathVerse.Math.Operators.MathOperator.Add,
            new BoundLiteralExpression(3.0));
        var folded = folder.Fold(bin);
        folded.Should().BeOfType<BoundLiteralExpression>();
        ((BoundLiteralExpression)folded).Value.Should().Be(5.0);
    }
}
