namespace MathVerse.Math.Simplification.Tests;

public class ConstantFolderTests
{
    private readonly ConstantFolder _folder = new();

    [Fact]
    public void Fold_LiteralAddition_FoldsToResult()
    {
        var expr = Expr.Add(Expr.Literal(2.0), Expr.Literal(3.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(5.0);
    }

    [Fact]
    public void Fold_LiteralSubtraction_FoldsToResult()
    {
        var expr = Expr.Subtract(Expr.Literal(10.0), Expr.Literal(3.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(7.0);
    }

    [Fact]
    public void Fold_LiteralMultiplication_FoldsToResult()
    {
        var expr = Expr.Multiply(Expr.Literal(4.0), Expr.Literal(5.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(20.0);
    }

    [Fact]
    public void Fold_LiteralDivision_FoldsToResult()
    {
        var expr = Expr.Divide(Expr.Literal(10.0), Expr.Literal(2.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(5.0);
    }

    [Fact]
    public void Fold_LiteralPower_FoldsToResult()
    {
        var expr = Expr.Pow(Expr.Literal(2.0), Expr.Literal(10.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1024.0);
    }

    [Fact]
    public void Fold_MixedOperations_FoldsBottomUp()
    {
        var expr = Expr.Add(Expr.Literal(2.0), Expr.Multiply(Expr.Literal(3.0), Expr.Literal(4.0)));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(14.0);
    }

    [Fact]
    public void Fold_SinZero_FoldsToZero()
    {
        var expr = Expr.Sin(Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Fold_CosZero_FoldsToOne()
    {
        var expr = Expr.Cos(Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Fold_SqrtFour_FoldsToTwo()
    {
        var expr = Expr.Sqrt(Expr.Literal(4.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(2.0);
    }

    [Fact]
    public void Fold_ExpZero_FoldsToOne()
    {
        var expr = Expr.Exp(Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Fold_NestedAddMultiplication_FoldsCorrectly()
    {
        var expr = Expr.Multiply(
            Expr.Add(Expr.Literal(2.0), Expr.Literal(3.0)),
            Expr.Add(Expr.Literal(4.0), Expr.Literal(1.0)));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(25.0);
    }

    [Fact]
    public void Fold_NegateLiteral_FoldsToNegative()
    {
        var expr = Expr.Negate(Expr.Literal(5.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(-5.0);
    }

    [Fact]
    public void Fold_AbsNegative_FoldsToPositive()
    {
        var expr = Expr.Abs(Expr.Negate(Expr.Literal(7.0)));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(7.0);
    }

    [Fact]
    public void Fold_DivisionByZero_ReturnsUnchanged()
    {
        var expr = Expr.Divide(Expr.Literal(1.0), Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void Fold_LogPositive_FoldsCorrectly()
    {
        var expr = Expr.Ln(Expr.Literal(1.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Fold_TanZero_FoldsToZero()
    {
        var expr = Expr.Tan(Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Fold_CbrtFolds()
    {
        var expr = Expr.Cbrt(Expr.Literal(27.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(3.0);
    }

    [Fact]
    public void Fold_SinhZero_FoldsToZero()
    {
        var expr = Expr.Sinh(Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Fold_CoshZero_FoldsToOne()
    {
        var expr = Expr.Cosh(Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Fold_TanhZero_FoldsToZero()
    {
        var expr = Expr.Tanh(Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void Fold_Log10Folds()
    {
        var expr = Expr.Log10(Expr.Literal(100.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(2.0);
    }

    [Fact]
    public void Fold_LogWithBase_Folds()
    {
        var expr = Expr.Log(Expr.Literal(8.0), Expr.Literal(2.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(3.0);
    }

    [Fact]
    public void Fold_Modulo_FoldsCorrectly()
    {
        var expr = Expr.Modulo(Expr.Literal(10.0), Expr.Literal(3.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void Fold_VariableExpr_RemainsUnchanged()
    {
        var expr = Expr.Variable("x");
        var result = _folder.Fold(expr);
        result.Should().BeSameAs(expr);
    }

    [Fact]
    public void Fold_MixedConstantAndVariable_OnlyFoldsConstants()
    {
        var expr = Expr.Add(Expr.Multiply(Expr.Literal(2.0), Expr.Literal(3.0)), Expr.Variable("x"));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result;
        binary.Right.Should().BeOfType<VariableExpression>();
    }

    [Fact]
    public void Fold_TernaryPower_FoldsCorrectly()
    {
        var expr = Expr.Pow(Expr.Literal(3.0), Expr.Literal(3.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(27.0);
    }

    [Fact]
    public void Fold_AsinOne_FoldsToHalfPi()
    {
        var expr = Expr.Asin(Expr.Literal(1.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().BeApproximately(System.Math.PI / 2.0, 1e-10);
    }

    [Fact]
    public void Fold_AcosOne_FoldsToZero()
    {
        var expr = Expr.Acos(Expr.Literal(1.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Fold_AtanZero_FoldsToZero()
    {
        var expr = Expr.Atan(Expr.Literal(0.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Fold_NegativePower_FoldsCorrectly()
    {
        var expr = Expr.Pow(Expr.Literal(2.0), Expr.Literal(-2.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.25);
    }

    [Fact]
    public void Fold_LnE_RemainsAsFunctionCall()
    {
        var expr = Expr.Ln(ConstantExpression.E);
        var result = _folder.Fold(expr);
        result.Should().BeOfType<FunctionCallExpression>();
    }

    [Fact]
    public void Fold_LiteralPowerPower_FoldsCorrectly()
    {
        var inner = Expr.Pow(Expr.Literal(2.0), Expr.Literal(3.0));
        var expr = Expr.Pow(inner, Expr.Literal(2.0));
        var result = _folder.Fold(expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(64.0);
    }
}
