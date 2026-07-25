namespace MathVerse.Math.Simplification.Tests;

public class RuleCollectionTests
{
    #region ArithmeticRules

    [Fact]
    public void AdditiveIdentityRight_XPlusZero_SimplifiesToX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, Expr.Literal(0.0));
        ApplySingleArithmeticRule("AdditiveIdentityRight", expr).Should().Be(x);
    }

    [Fact]
    public void AdditiveIdentityRight_NonZero_DoesNotMatch()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, Expr.Literal(1.0));
        ApplySingleArithmeticRule("AdditiveIdentityRight", expr).Should().BeNull();
    }

    [Fact]
    public void AdditiveIdentityLeft_ZeroPlusX_SimplifiesToX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(Expr.Literal(0.0), x);
        ApplySingleArithmeticRule("AdditiveIdentityLeft", expr).Should().Be(x);
    }

    [Fact]
    public void SubtractZeroRight_XMinusZero_SimplifiesToX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Subtract(x, Expr.Literal(0.0));
        ApplySingleArithmeticRule("SubtractZeroRight", expr).Should().Be(x);
    }

    [Fact]
    public void MultiplicativeIdentityRight_XTimesOne_SimplifiesToX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Literal(1.0));
        ApplySingleArithmeticRule("MultiplicativeIdentityRight", expr).Should().Be(x);
    }

    [Fact]
    public void MultiplicativeIdentityLeft_OneTimesX_SimplifiesToX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Literal(1.0), x);
        ApplySingleArithmeticRule("MultiplicativeIdentityLeft", expr).Should().Be(x);
    }

    [Fact]
    public void MultiplyByZeroRight_XTimesZero_SimplifiesToZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Literal(0.0));
        var result = ApplySingleArithmeticRule("MultiplyByZeroRight", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void MultiplyByZeroLeft_ZeroTimesX_SimplifiesToZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Literal(0.0), x);
        var result = ApplySingleArithmeticRule("MultiplyByZeroLeft", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void ZeroNumeratorDivision_ZeroOverX_SimplifiesToZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Divide(Expr.Literal(0.0), x);
        var result = ApplySingleArithmeticRule("ZeroNumeratorDivision", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void DivideByOne_XOverOne_SimplifiesToX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Divide(x, Expr.Literal(1.0));
        ApplySingleArithmeticRule("DivideByOne", expr).Should().Be(x);
    }

    [Fact]
    public void PowerOfZero_XPowZero_SimplifiesToOne()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(x, Expr.Literal(0.0));
        var result = ApplySingleArithmeticRule("PowerOfZero", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(1.0);
    }

    [Fact]
    public void PowerOfOne_XPowOne_SimplifiesToX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(x, Expr.Literal(1.0));
        ApplySingleArithmeticRule("PowerOfOne", expr).Should().Be(x);
    }

    [Fact]
    public void BaseIsOne_OnePowX_SimplifiesToOne()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(Expr.Literal(1.0), x);
        var result = ApplySingleArithmeticRule("BaseIsOne", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(1.0);
    }

    [Fact]
    public void BaseIsZero_ZeroPowX_SimplifiesToZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(Expr.Literal(0.0), x);
        var result = ApplySingleArithmeticRule("BaseIsZero", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void SelfAddition_XPlusX_SimplifiesTo2TimesX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, x);
        var result = ApplySingleArithmeticRule("SelfAddition", expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result!;
        binary.Operator.Symbol.Should().Be("*");
        ((LiteralExpression)binary.Left).Value.Should().Be(2.0);
        binary.Right.Should().Be(x);
    }

    [Fact]
    public void SelfMultiplication_XTimesX_SimplifiesToXPow2()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, x);
        var result = ApplySingleArithmeticRule("SelfMultiplication", expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result!;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
        ((LiteralExpression)binary.Right).Value.Should().Be(2.0);
    }

    [Fact]
    public void DoubleNegation_DoubleNegateX_SimplifiesToX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Negate(Expr.Negate(x));
        ApplySingleArithmeticRule("DoubleNegation", expr).Should().Be(x);
    }

    [Fact]
    public void AddInverseRight_XPlusNegX_SimplifiesToZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, Expr.Negate(x));
        var result = ApplySingleArithmeticRule("AddInverseRight", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void AddInverseLeft_NegXPlusX_SimplifiesToZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(Expr.Negate(x), x);
        var result = ApplySingleArithmeticRule("AddInverseLeft", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void SelfSubtraction_XMinusX_SimplifiesToZero()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Subtract(x, x);
        var result = ApplySingleArithmeticRule("SelfSubtraction", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void MultiplyByNegOneRight_XTimesNegOne_SimplifiesToNegX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Literal(-1.0));
        var result = ApplySingleArithmeticRule("MultiplyByNegOneRight", expr);
        result.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)result!;
        unary.Operator.Symbol.Should().Be("-");
        unary.Operand.Should().Be(x);
    }

    [Fact]
    public void MultiplyByNegOneLeft_NegOneTimesX_SimplifiesToNegX()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Literal(-1.0), x);
        var result = ApplySingleArithmeticRule("MultiplyByNegOneLeft", expr);
        result.Should().BeOfType<UnaryExpression>();
        var unary = (UnaryExpression)result!;
        unary.Operator.Symbol.Should().Be("-");
        unary.Operand.Should().Be(x);
    }

    [Fact]
    public void ArithmeticRule_NonMatchingExpression_ReturnsNull()
    {
        var x = Expr.Variable("x");
        var y = Expr.Variable("y");
        var expr = Expr.Add(x, y);
        ApplySingleArithmeticRule("AdditiveIdentityRight", expr).Should().BeNull();
    }

    [Fact]
    public void ArithmeticRules_HasCorrectCount()
    {
        RuleCollection.ArithmeticRules.Should().HaveCount(21);
    }

    #endregion

    #region PowerRules

    [Fact]
    public void PowerOfPower_SimplifiesCorrectly()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Pow(Expr.Pow(x, Expr.Literal(2.0)), Expr.Literal(3.0));
        var result = ApplySinglePowerRule("PowerOfPower", expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result!;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
    }

    [Fact]
    public void ProductSameBase_SimplifiesCorrectly()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Pow(x, Expr.Literal(2.0)), Expr.Pow(x, Expr.Literal(3.0)));
        var result = ApplySinglePowerRule("ProductSameBase", expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result!;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
    }

    [Fact]
    public void ProductSameBase_DifferentBases_DoesNotMatch()
    {
        var x = Expr.Variable("x");
        var y = Expr.Variable("y");
        var expr = Expr.Multiply(Expr.Pow(x, Expr.Literal(2.0)), Expr.Pow(y, Expr.Literal(3.0)));
        ApplySinglePowerRule("ProductSameBase", expr).Should().BeNull();
    }

    [Fact]
    public void ProductSameBaseRight_SimplifiesCorrectly()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Pow(x, Expr.Literal(2.0)), x);
        var result = ApplySinglePowerRule("ProductSameBaseRight", expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result!;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
    }

    [Fact]
    public void ProductSameBaseLeft_SimplifiesCorrectly()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Pow(x, Expr.Literal(2.0)));
        var result = ApplySinglePowerRule("ProductSameBaseLeft", expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result!;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
    }

    [Fact]
    public void QuotientSameBase_SimplifiesCorrectly()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Divide(Expr.Pow(x, Expr.Literal(5.0)), Expr.Pow(x, Expr.Literal(2.0)));
        var result = ApplySinglePowerRule("QuotientSameBase", expr);
        result.Should().BeOfType<BinaryExpression>();
        var binary = (BinaryExpression)result!;
        binary.Operator.Symbol.Should().Be("^");
        binary.Left.Should().Be(x);
    }

    [Fact]
    public void PowerRules_HasCorrectCount()
    {
        RuleCollection.PowerRules.Should().HaveCount(5);
    }

    #endregion

    #region LogRules

    [Fact]
    public void LnOfOne_SimplifiesToZero()
    {
        var expr = Expr.Ln(Expr.Literal(1.0));
        var result = ApplySingleLogRule("LnOfOne", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void LnOfOne_NonOne_DoesNotMatch()
    {
        var expr = Expr.Ln(Expr.Literal(2.0));
        ApplySingleLogRule("LnOfOne", expr).Should().BeNull();
    }

    [Fact]
    public void LnOfE_SimplifiesToOne()
    {
        var expr = Expr.Ln(ConstantExpression.E);
        var result = ApplySingleLogRule("LnOfE", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(1.0);
    }

    [Fact]
    public void ExpOfZero_SimplifiesToOne()
    {
        var expr = Expr.Exp(Expr.Literal(0.0));
        var result = ApplySingleLogRule("ExpOfZero", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(1.0);
    }

    [Fact]
    public void ExpOfLn_SimplifiesToInner()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Exp(Expr.Ln(x));
        ApplySingleLogRule("ExpOfLn", expr).Should().Be(x);
    }

    [Fact]
    public void LnOfExp_SimplifiesToInner()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Ln(Expr.Exp(x));
        ApplySingleLogRule("LnOfExp", expr).Should().Be(x);
    }

    [Fact]
    public void LogRules_HasCorrectCount()
    {
        RuleCollection.LogRules.Should().HaveCount(8);
    }

    #endregion

    #region TrigRules

    [Fact]
    public void SinOfZero_SimplifiesToZero()
    {
        var expr = Expr.Sin(Expr.Literal(0.0));
        var result = ApplySingleTrigRule("SinOfZero", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void SinOfZero_NonZero_DoesNotMatch()
    {
        var expr = Expr.Sin(Expr.Literal(1.0));
        ApplySingleTrigRule("SinOfZero", expr).Should().BeNull();
    }

    [Fact]
    public void CosOfZero_SimplifiesToOne()
    {
        var expr = Expr.Cos(Expr.Literal(0.0));
        var result = ApplySingleTrigRule("CosOfZero", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(1.0);
    }

    [Fact]
    public void TanOfZero_SimplifiesToZero()
    {
        var expr = Expr.Tan(Expr.Literal(0.0));
        var result = ApplySingleTrigRule("TanOfZero", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void SinOfPiOverTwo_SimplifiesToOne()
    {
        var expr = Expr.Sin(Expr.Divide(ConstantExpression.Pi, Expr.Literal(2.0)));
        var result = ApplySingleTrigRule("SinOfPiOver2", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(1.0);
    }

    [Fact]
    public void CosOfPiOverTwo_SimplifiesToZero()
    {
        var expr = Expr.Cos(Expr.Divide(ConstantExpression.Pi, Expr.Literal(2.0)));
        var result = ApplySingleTrigRule("CosOfPiOver2", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void SinOfPi_SimplifiesToZero()
    {
        var expr = Expr.Sin(ConstantExpression.Pi);
        var result = ApplySingleTrigRule("SinOfPi", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(0.0);
    }

    [Fact]
    public void CosOfPi_SimplifiesToNegOne()
    {
        var expr = Expr.Cos(ConstantExpression.Pi);
        var result = ApplySingleTrigRule("CosOfPi", expr);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result!).Value.Should().Be(-1.0);
    }

    [Fact]
    public void TrigRules_HasCorrectCount()
    {
        RuleCollection.TrigRules.Should().HaveCount(7);
    }

    #endregion

    #region AllRules

    [Fact]
    public void AllRules_ContainsAllCategories()
    {
        var allRuleNames = RuleCollection.AllRules.Select(r => r.Name).ToList();
        allRuleNames.Should().Contain("AdditiveIdentityRight");
        allRuleNames.Should().Contain("PowerOfPower");
        allRuleNames.Should().Contain("LnOfOne");
        allRuleNames.Should().Contain("SinOfZero");
    }

    [Fact]
    public void AllRules_IsSortedByDescendingPriority()
    {
        var priorities = RuleCollection.AllRules.Select(r => r.Priority).ToList();
        priorities.Should().BeInDescendingOrder();
    }

    #endregion

    #region Helpers

    private static Expression? ApplySingleArithmeticRule(string ruleName, Expression expr)
    {
        var rule = RuleCollection.ArithmeticRules.First(r => r.Name == ruleName);
        return rule.TryRewrite(expr);
    }

    private static Expression? ApplySinglePowerRule(string ruleName, Expression expr)
    {
        var rule = RuleCollection.PowerRules.First(r => r.Name == ruleName);
        return rule.TryRewrite(expr);
    }

    private static Expression? ApplySingleLogRule(string ruleName, Expression expr)
    {
        var rule = RuleCollection.LogRules.First(r => r.Name == ruleName);
        return rule.TryRewrite(expr);
    }

    private static Expression? ApplySingleTrigRule(string ruleName, Expression expr)
    {
        var rule = RuleCollection.TrigRules.First(r => r.Name == ruleName);
        return rule.TryRewrite(expr);
    }

    #endregion
}
