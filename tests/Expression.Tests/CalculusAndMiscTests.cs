using ExprType = MathVerse.Math.Expressions.Expression;
namespace MathVerse.Expression.Tests;

public class CalculusAndMiscTests
{
    // ───────────────────────── DerivativeExpression ─────────────────────────

    [Fact]
    public void DerivativeExpression_Kind_IsDerivative()
    {
        var expr = Expr.Derivative(Expr.Variable("x"), Expr.Variable("x"));

        expr.Kind.Should().Be(ExpressionKind.Derivative);
    }

    [Fact]
    public void DerivativeExpression_Properties_AreCorrect()
    {
        var f = Expr.Sin(Expr.Variable("x"));
        var v = Expr.Variable("x");
        var expr = new DerivativeExpression(f, v, 3);

        expr.Function.Should().BeSameAs(f);
        expr.Variable.Should().BeSameAs(v);
        expr.Order.Should().Be(3);
    }

    [Fact]
    public void DerivativeExpression_DefaultOrder_IsOne()
    {
        var expr = Expr.Derivative(Expr.Variable("x"), Expr.Variable("x"));

        expr.Order.Should().Be(1);
    }

    [Fact]
    public void DerivativeExpression_Children_ReturnsFunctionAndVariable()
    {
        var f = Expr.Sin(Expr.Variable("x"));
        var v = Expr.Variable("x");
        var expr = Expr.Derivative(f, v);

        expr.Children.Should().HaveCount(2);
        expr.Children[0].Should().BeSameAs(f);
        expr.Children[1].Should().BeSameAs(v);
    }

    [Fact]
    public void DerivativeExpression_DepthAndNodeCount_CalculatedCorrectly()
    {
        var expr = Expr.Derivative(Expr.Variable("x"), Expr.Variable("x"));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void DerivativeExpression_Equal_SameProperties()
    {
        var x = Expr.Variable("x");
        var a = Expr.Derivative(Expr.Sin(x), x, 2);
        var b = Expr.Derivative(Expr.Sin(x), x, 2);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void DerivativeExpression_NotEqual_DifferentFunction()
    {
        var x = Expr.Variable("x");
        var a = Expr.Derivative(Expr.Sin(x), x);
        var b = Expr.Derivative(Expr.Cos(x), x);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void DerivativeExpression_NotEqual_DifferentOrder()
    {
        var x = Expr.Variable("x");
        var a = Expr.Derivative(Expr.Sin(x), x, 1);
        var b = Expr.Derivative(Expr.Sin(x), x, 2);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void DerivativeExpression_NullFunction_Throws()
    {
        var x = Expr.Variable("x");
        var act = () => new DerivativeExpression(null!, x);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void DerivativeExpression_ZeroOrNegativeOrder_Throws()
    {
        var x = Expr.Variable("x");
        var act = () => new DerivativeExpression(x, x, 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DerivativeExpression_Factory_CreatesCorrectly()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Derivative(Expr.Sin(x), x, 2);

        expr.Should().BeOfType<DerivativeExpression>();
        expr.Order.Should().Be(2);
    }

    [Fact]
    public void DerivativeExpression_GetHashCode_Consistent()
    {
        var x = Expr.Variable("x");
        var a = Expr.Derivative(Expr.Sin(x), x);
        var b = Expr.Derivative(Expr.Sin(x), x);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── IntegralExpression ─────────────────────────

    [Fact]
    public void IntegralExpression_Kind_IsIntegral()
    {
        var expr = Expr.Integral(Expr.Variable("x"), Expr.Variable("x"));

        expr.Kind.Should().Be(ExpressionKind.Integral);
    }

    [Fact]
    public void IntegralExpression_Indefinite_Properties()
    {
        var integrand = Expr.Sin(Expr.Variable("x"));
        var v = Expr.Variable("x");
        var expr = new IntegralExpression(integrand, v);

        expr.Integrand.Should().BeSameAs(integrand);
        expr.Variable.Should().BeSameAs(v);
        expr.LowerBound.Should().BeNull();
        expr.UpperBound.Should().BeNull();
        expr.IsDefinite.Should().BeFalse();
    }

    [Fact]
    public void IntegralExpression_Definite_Properties()
    {
        var integrand = Expr.Variable("x");
        var v = Expr.Variable("x");
        var lo = Expr.Literal(0);
        var hi = Expr.Literal(1);
        var expr = new IntegralExpression(integrand, v, lo, hi);

        expr.LowerBound.Should().BeSameAs(lo);
        expr.UpperBound.Should().BeSameAs(hi);
        expr.IsDefinite.Should().BeTrue();
    }

    [Fact]
    public void IntegralExpression_Children_Indefinite_ReturnsIntegrandAndVariable()
    {
        var expr = Expr.Integral(Expr.Variable("x"), Expr.Variable("x"));

        expr.Children.Should().HaveCount(2);
    }

    [Fact]
    public void IntegralExpression_Children_Definite_IncludesBounds()
    {
        var expr = Expr.Integral(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), Expr.Literal(1));

        expr.Children.Should().HaveCount(4);
    }

    [Fact]
    public void IntegralExpression_Equal_SameProperties()
    {
        var a = Expr.Integral(Expr.Variable("x"), Expr.Variable("t"));
        var b = Expr.Integral(Expr.Variable("x"), Expr.Variable("t"));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void IntegralExpression_NotEqual_IndefiniteVsDefinite()
    {
        var integrand = Expr.Variable("x");
        var v = Expr.Variable("x");
        var a = new IntegralExpression(integrand, v);
        var b = new IntegralExpression(integrand, v, Expr.Literal(0), Expr.Literal(1));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void IntegralExpression_NotEqual_DifferentBounds()
    {
        var integrand = Expr.Variable("x");
        var v = Expr.Variable("x");
        var a = new IntegralExpression(integrand, v, Expr.Literal(0), Expr.Literal(1));
        var b = new IntegralExpression(integrand, v, Expr.Literal(0), Expr.Literal(2));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void IntegralExpression_NullIntegrand_Throws()
    {
        var v = Expr.Variable("x");
        var act = () => new IntegralExpression(null!, v);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void IntegralExpression_Factory_Definite()
    {
        var expr = Expr.Integral(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), Expr.Literal(1));

        expr.Should().BeOfType<IntegralExpression>();
        expr.IsDefinite.Should().BeTrue();
    }

    [Fact]
    public void IntegralExpression_Depth_CalculatedCorrectly()
    {
        var expr = Expr.Integral(Expr.Sin(Expr.Variable("x")), Expr.Variable("x"));

        expr.Depth.Should().Be(2);
    }

    [Fact]
    public void IntegralExpression_GetHashCode_Consistent()
    {
        var a = Expr.Integral(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), Expr.Literal(1));
        var b = Expr.Integral(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), Expr.Literal(1));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── SummationExpression ─────────────────────────

    [Fact]
    public void SummationExpression_Kind_IsSummation()
    {
        var expr = Expr.Summation(Expr.Variable("k"), Expr.Literal(1), Expr.Literal(10), Expr.Variable("k"));

        expr.Kind.Should().Be(ExpressionKind.Summation);
    }

    [Fact]
    public void SummationExpression_Properties_AreCorrect()
    {
        var v = Expr.Variable("k");
        var lo = Expr.Literal(1);
        var hi = Expr.Literal(10);
        var body = Expr.Pow(Expr.Variable("k"), Expr.Literal(2));
        var expr = new SummationExpression(v, lo, hi, body);

        expr.Variable.Should().BeSameAs(v);
        expr.LowerBound.Should().BeSameAs(lo);
        expr.UpperBound.Should().BeSameAs(hi);
        expr.Body.Should().BeSameAs(body);
    }

    [Fact]
    public void SummationExpression_Children_FourElements()
    {
        var expr = Expr.Summation(Expr.Variable("k"), Expr.Literal(1), Expr.Literal(10), Expr.Variable("k"));

        expr.Children.Should().HaveCount(4);
    }

    [Fact]
    public void SummationExpression_Equal_SameProperties()
    {
        var a = Expr.Summation(Expr.Variable("k"), Expr.Literal(1), Expr.Literal(10), Expr.Variable("k"));
        var b = Expr.Summation(Expr.Variable("k"), Expr.Literal(1), Expr.Literal(10), Expr.Variable("k"));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void SummationExpression_NotEqual_DifferentBody()
    {
        var a = Expr.Summation(Expr.Variable("k"), Expr.Literal(1), Expr.Literal(10), Expr.Variable("k"));
        var b = Expr.Summation(Expr.Variable("k"), Expr.Literal(1), Expr.Literal(10), Expr.Literal(5));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void SummationExpression_DepthAndNodeCount_CalculatedCorrectly()
    {
        var expr = Expr.Summation(Expr.Variable("k"), Expr.Literal(1), Expr.Variable("n"), Expr.Variable("k"));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(5);
    }

    [Fact]
    public void SummationExpression_GetHashCode_Consistent()
    {
        var a = Expr.Summation(Expr.Variable("k"), Expr.Literal(0), Expr.Literal(5), Expr.Variable("k"));
        var b = Expr.Summation(Expr.Variable("k"), Expr.Literal(0), Expr.Literal(5), Expr.Variable("k"));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── ProductExpression ─────────────────────────

    [Fact]
    public void ProductExpression_Kind_IsProduct()
    {
        var expr = Expr.Product(Expr.Variable("k"), Expr.Literal(1), Expr.Variable("n"), Expr.Variable("k"));

        expr.Kind.Should().Be(ExpressionKind.Product);
    }

    [Fact]
    public void ProductExpression_Properties_AreCorrect()
    {
        var v = Expr.Variable("i");
        var lo = Expr.Literal(1);
        var hi = Expr.Variable("n");
        var body = Expr.Variable("i");
        var expr = new ProductExpression(v, lo, hi, body);

        expr.Variable.Should().BeSameAs(v);
        expr.LowerBound.Should().BeSameAs(lo);
        expr.UpperBound.Should().BeSameAs(hi);
        expr.Body.Should().BeSameAs(body);
    }

    [Fact]
    public void ProductExpression_Children_FourElements()
    {
        var expr = Expr.Product(Expr.Variable("i"), Expr.Literal(1), Expr.Variable("n"), Expr.Factorial(Expr.Variable("i")));

        expr.Children.Should().HaveCount(4);
    }

    [Fact]
    public void ProductExpression_Equal_SameProperties()
    {
        var a = Expr.Product(Expr.Variable("i"), Expr.Literal(1), Expr.Literal(5), Expr.Variable("i"));
        var b = Expr.Product(Expr.Variable("i"), Expr.Literal(1), Expr.Literal(5), Expr.Variable("i"));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void ProductExpression_NotEqual_DifferentBound()
    {
        var a = Expr.Product(Expr.Variable("i"), Expr.Literal(1), Expr.Literal(5), Expr.Variable("i"));
        var b = Expr.Product(Expr.Variable("i"), Expr.Literal(0), Expr.Literal(5), Expr.Variable("i"));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ProductExpression_DepthAndNodeCount_CalculatedCorrectly()
    {
        var expr = Expr.Product(Expr.Variable("i"), Expr.Literal(1), Expr.Variable("n"), Expr.Variable("i"));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(5);
    }

    [Fact]
    public void ProductExpression_GetHashCode_Consistent()
    {
        var a = Expr.Product(Expr.Variable("i"), Expr.Literal(0), Expr.Variable("n"), Expr.Variable("i"));
        var b = Expr.Product(Expr.Variable("i"), Expr.Literal(0), Expr.Variable("n"), Expr.Variable("i"));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── LimitExpression ─────────────────────────

    [Fact]
    public void LimitExpression_Kind_IsLimit()
    {
        var expr = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0));

        expr.Kind.Should().Be(ExpressionKind.Limit);
    }

    [Fact]
    public void LimitExpression_Properties_AreCorrect()
    {
        var body = Expr.Divide(Expr.Sin(Expr.Variable("x")), Expr.Variable("x"));
        var v = Expr.Variable("x");
        var t = Expr.Literal(0);
        var expr = new LimitExpression(body, v, t, LimitDirection.Both);

        expr.Body.Should().BeSameAs(body);
        expr.Variable.Should().BeSameAs(v);
        expr.Target.Should().BeSameAs(t);
        expr.Direction.Should().Be(LimitDirection.Both);
    }

    [Fact]
    public void LimitExpression_DefaultDirection_IsBoth()
    {
        var expr = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0));

        expr.Direction.Should().Be(LimitDirection.Both);
    }

    [Fact]
    public void LimitExpression_Children_ReturnsThreeElements()
    {
        var expr = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0));

        expr.Children.Should().HaveCount(3);
    }

    [Fact]
    public void LimitExpression_Equal_SameProperties()
    {
        var a = Expr.Limit(Expr.Sin(Expr.Variable("x")), Expr.Variable("x"), Expr.Literal(0), LimitDirection.Both);
        var b = Expr.Limit(Expr.Sin(Expr.Variable("x")), Expr.Variable("x"), Expr.Literal(0), LimitDirection.Both);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void LimitExpression_NotEqual_DifferentDirection()
    {
        var a = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), LimitDirection.Left);
        var b = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), LimitDirection.Right);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void LimitExpression_NotEqual_DifferentTarget()
    {
        var a = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0));
        var b = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(1));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void LimitExpression_DepthAndNodeCount_CalculatedCorrectly()
    {
        var expr = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(4);
    }

    [Fact]
    public void LimitExpression_GetHashCode_Consistent()
    {
        var a = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), LimitDirection.Left);
        var b = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0), LimitDirection.Left);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── FactorialExpression ─────────────────────────

    [Fact]
    public void FactorialExpression_Kind_IsFactorial()
    {
        var expr = Expr.Factorial(Expr.Literal(5));

        expr.Kind.Should().Be(ExpressionKind.Factorial);
    }

    [Fact]
    public void FactorialExpression_Properties_AreCorrect()
    {
        var op = Expr.Variable("n");
        var expr = new FactorialExpression(op);

        expr.Operand.Should().BeSameAs(op);
    }

    [Fact]
    public void FactorialExpression_Children_SingleElement()
    {
        var expr = Expr.Factorial(Expr.Variable("n"));

        expr.Children.Should().HaveCount(1);
        expr.Children[0].Should().BeOfType<VariableExpression>();
    }

    [Fact]
    public void FactorialExpression_Equal_SameOperand()
    {
        var a = Expr.Factorial(Expr.Variable("n"));
        var b = Expr.Factorial(Expr.Variable("n"));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void FactorialExpression_NotEqual_DifferentOperand()
    {
        var a = Expr.Factorial(Expr.Literal(5));
        var b = Expr.Factorial(Expr.Literal(6));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void FactorialExpression_Depth_IsOne()
    {
        var expr = Expr.Factorial(Expr.Literal(5));

        expr.Depth.Should().Be(1);
    }

    [Fact]
    public void FactorialExpression_NodeCount_IsTwo()
    {
        var expr = Expr.Factorial(Expr.Variable("x"));

        expr.NodeCount.Should().Be(2);
    }

    [Fact]
    public void FactorialExpression_GetHashCode_Consistent()
    {
        var a = Expr.Factorial(Expr.Variable("n"));
        var b = Expr.Factorial(Expr.Variable("n"));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── RangeExpression ─────────────────────────

    [Fact]
    public void RangeExpression_Kind_IsRange()
    {
        var expr = Expr.Range(Expr.Literal(1), Expr.Literal(10));

        expr.Kind.Should().Be(ExpressionKind.Range);
    }

    [Fact]
    public void RangeExpression_Properties_AreCorrect()
    {
        var start = Expr.Literal(1);
        var end = Expr.Literal(10);
        var step = Expr.Literal(2);
        var expr = new RangeExpression(start, end, step);

        expr.Start.Should().BeSameAs(start);
        expr.End.Should().BeSameAs(end);
        expr.Step.Should().BeSameAs(step);
    }

    [Fact]
    public void RangeExpression_DefaultStep_IsNull()
    {
        var expr = Expr.Range(Expr.Literal(0), Expr.Literal(5));

        expr.Step.Should().BeNull();
    }

    [Fact]
    public void RangeExpression_Children_TwoWithoutStep()
    {
        var expr = Expr.Range(Expr.Literal(1), Expr.Literal(5));

        expr.Children.Should().HaveCount(2);
    }

    [Fact]
    public void RangeExpression_Children_ThreeWithStep()
    {
        var expr = Expr.Range(Expr.Literal(1), Expr.Literal(5), Expr.Literal(2));

        expr.Children.Should().HaveCount(3);
    }

    [Fact]
    public void RangeExpression_Equal_SameProperties()
    {
        var a = Expr.Range(Expr.Literal(1), Expr.Literal(10));
        var b = Expr.Range(Expr.Literal(1), Expr.Literal(10));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void RangeExpression_NotEqual_DifferentStep()
    {
        var a = Expr.Range(Expr.Literal(1), Expr.Literal(10));
        var b = Expr.Range(Expr.Literal(1), Expr.Literal(10), Expr.Literal(2));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void RangeExpression_Depth_CalculatedCorrectly()
    {
        var expr = Expr.Range(Expr.Literal(1), Expr.Variable("n"));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void RangeExpression_GetHashCode_Consistent()
    {
        var a = Expr.Range(Expr.Literal(0), Expr.Literal(9), Expr.Literal(3));
        var b = Expr.Range(Expr.Literal(0), Expr.Literal(9), Expr.Literal(3));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── IntervalExpression ─────────────────────────

    [Fact]
    public void IntervalExpression_Kind_IsInterval()
    {
        var expr = Expr.Interval(Expr.Literal(0), Expr.Literal(1));

        expr.Kind.Should().Be(ExpressionKind.Interval);
    }

    [Fact]
    public void IntervalExpression_Properties_AreCorrect()
    {
        var lo = Expr.Literal(0);
        var hi = Expr.Literal(1);
        var expr = new IntervalExpression(lo, hi, false, true);

        expr.Lower.Should().BeSameAs(lo);
        expr.Upper.Should().BeSameAs(hi);
        expr.LowerClosed.Should().BeFalse();
        expr.UpperClosed.Should().BeTrue();
    }

    [Fact]
    public void IntervalExpression_DefaultClosed_IsTrue()
    {
        var expr = Expr.Interval(Expr.Literal(0), Expr.Literal(1));

        expr.LowerClosed.Should().BeTrue();
        expr.UpperClosed.Should().BeTrue();
    }

    [Fact]
    public void IntervalExpression_Children_TwoElements()
    {
        var expr = Expr.Interval(Expr.Literal(0), Expr.Literal(1));

        expr.Children.Should().HaveCount(2);
    }

    [Fact]
    public void IntervalExpression_Equal_SameProperties()
    {
        var a = Expr.Interval(Expr.Literal(0), Expr.Literal(1), false, false);
        var b = Expr.Interval(Expr.Literal(0), Expr.Literal(1), false, false);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void IntervalExpression_NotEqual_DifferentClosedness()
    {
        var a = Expr.Interval(Expr.Literal(0), Expr.Literal(1), true, false);
        var b = Expr.Interval(Expr.Literal(0), Expr.Literal(1), true, true);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void IntervalExpression_Depth_CalculatedCorrectly()
    {
        var expr = Expr.Interval(Expr.Literal(0), Expr.Variable("x"));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void IntervalExpression_GetHashCode_Consistent()
    {
        var a = Expr.Interval(Expr.Literal(0), Expr.Literal(1), false, false);
        var b = Expr.Interval(Expr.Literal(0), Expr.Literal(1), false, false);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── SetExpression ─────────────────────────

    [Fact]
    public void SetExpression_Kind_IsSet()
    {
        var expr = Expr.Set(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        expr.Kind.Should().Be(ExpressionKind.Set);
    }

    [Fact]
    public void SetExpression_Elements_AreCorrect()
    {
        var elements = new ExprType[] { Expr.Literal(1), Expr.Variable("x") };
        var expr = new SetExpression(elements);

        expr.Elements.Should().HaveCount(2);
        expr.Elements[0].Should().BeSameAs(elements[0]);
        expr.Elements[1].Should().BeSameAs(elements[1]);
    }

    [Fact]
    public void SetExpression_Children_EqualsElements()
    {
        var expr = Expr.Set(Expr.Literal(1), Expr.Literal(2));

        expr.Children.Should().BeSameAs(expr.Elements);
    }

    [Fact]
    public void SetExpression_Equal_SameElements()
    {
        var a = Expr.Set(Expr.Literal(1), Expr.Variable("x"));
        var b = Expr.Set(Expr.Literal(1), Expr.Variable("x"));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void SetExpression_NotEqual_DifferentCount()
    {
        var a = Expr.Set(Expr.Literal(1));
        var b = Expr.Set(Expr.Literal(1), Expr.Literal(2));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void SetExpression_NotEqual_DifferentElement()
    {
        var a = Expr.Set(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Set(Expr.Literal(1), Expr.Literal(9));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void SetExpression_Depth_CalculatedCorrectly()
    {
        var expr = Expr.Set(Expr.Literal(1), Expr.Variable("x"));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void SetExpression_GetHashCode_Consistent()
    {
        var a = Expr.Set(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));
        var b = Expr.Set(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── ComplexExpression ─────────────────────────

    [Fact]
    public void ComplexExpression_Kind_IsComplex()
    {
        var expr = Expr.Complex(Expr.Literal(3), Expr.Literal(4));

        expr.Kind.Should().Be(ExpressionKind.Complex);
    }

    [Fact]
    public void ComplexExpression_Properties_AreCorrect()
    {
        var r = Expr.Literal(1);
        var i = Expr.Literal(2);
        var expr = new ComplexExpression(r, i);

        expr.Real.Should().BeSameAs(r);
        expr.Imaginary.Should().BeSameAs(i);
    }

    [Fact]
    public void ComplexExpression_Children_TwoElements()
    {
        var expr = Expr.Complex(Expr.Variable("a"), Expr.Variable("b"));

        expr.Children.Should().HaveCount(2);
    }

    [Fact]
    public void ComplexExpression_Equal_SameProperties()
    {
        var a = Expr.Complex(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Complex(Expr.Literal(1), Expr.Literal(2));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void ComplexExpression_NotEqual_DifferentImaginary()
    {
        var a = Expr.Complex(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Complex(Expr.Literal(1), Expr.Literal(3));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ComplexExpression_DepthAndNodeCount_CalculatedCorrectly()
    {
        var expr = Expr.Complex(Expr.Literal(1), Expr.Variable("x"));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void ComplexExpression_GetHashCode_Consistent()
    {
        var a = Expr.Complex(Expr.Literal(3), Expr.Literal(4));
        var b = Expr.Complex(Expr.Literal(3), Expr.Literal(4));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── PolynomialExpression ─────────────────────────

    [Fact]
    public void PolynomialExpression_Kind_IsPolynomial()
    {
        var expr = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(3), Expr.Literal(2), Expr.Literal(1));

        expr.Kind.Should().Be(ExpressionKind.Polynomial);
    }

    [Fact]
    public void PolynomialExpression_Properties_AreCorrect()
    {
        var v = Expr.Variable("x");
        var coeffs = new ExprType[] { Expr.Literal(1), Expr.Literal(0), Expr.Literal(5) };
        var expr = new PolynomialExpression(v, coeffs);

        expr.Variable.Should().BeSameAs(v);
        expr.Coefficients.Should().HaveCount(3);
        expr.Degree.Should().Be(2);
    }

    [Fact]
    public void PolynomialExpression_Children_IncludesVariableAndCoefficients()
    {
        var expr = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(2), Expr.Literal(3));

        expr.Children.Should().HaveCount(3);
        expr.Children[0].Should().BeSameAs(expr.Variable);
    }

    [Fact]
    public void PolynomialExpression_Equality_SameCoefficients()
    {
        var a = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1), Expr.Literal(2));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void PolynomialExpression_NotEqual_DifferentCoefficient()
    {
        var a = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1), Expr.Literal(9));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void PolynomialExpression_NotEqual_DifferentDegree()
    {
        var a = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void PolynomialExpression_Depth_CalculatedCorrectly()
    {
        var expr = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(3), Expr.Literal(2));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(4);
    }

    [Fact]
    public void PolynomialExpression_GetHashCode_Consistent()
    {
        var a = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));
        var b = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── BooleanExpression ─────────────────────────

    [Fact]
    public void BooleanExpression_Kind_IsBoolean()
    {
        var expr = Expr.Boolean(true);

        expr.Kind.Should().Be(ExpressionKind.Boolean);
    }

    [Fact]
    public void BooleanExpression_Value_IsCorrect()
    {
        var expr = new BooleanExpression(true);

        expr.Value.Should().BeTrue();
    }

    [Fact]
    public void BooleanExpression_Children_Empty()
    {
        var expr = Expr.Boolean(false);

        expr.Children.Should().BeEmpty();
    }

    [Fact]
    public void BooleanExpression_Depth_IsZero()
    {
        var expr = Expr.Boolean(true);

        expr.Depth.Should().Be(0);
    }

    [Fact]
    public void BooleanExpression_NodeCount_IsOne()
    {
        var expr = Expr.Boolean(false);

        expr.NodeCount.Should().Be(1);
    }

    [Fact]
    public void BooleanExpression_Equal_SameValue()
    {
        var a = Expr.Boolean(true);
        var b = Expr.Boolean(true);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void BooleanExpression_NotEqual_DifferentValue()
    {
        var a = Expr.Boolean(true);
        var b = Expr.Boolean(false);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void BooleanExpression_GetHashCode_Consistent()
    {
        var a = Expr.Boolean(true);
        var b = Expr.Boolean(true);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── RelationExpression ─────────────────────────

    [Fact]
    public void RelationExpression_Kind_IsRelation()
    {
        var expr = Expr.Equal(Expr.Literal(1), Expr.Literal(2));

        expr.Kind.Should().Be(ExpressionKind.Relation);
    }

    [Fact]
    public void RelationExpression_Properties_AreCorrect()
    {
        var left = Expr.Variable("x");
        var right = Expr.Literal(5);
        var expr = new RelationExpression(MathOperator.GreaterThan, left, right);

        expr.Operator.Should().Be(MathOperator.GreaterThan);
        expr.Left.Should().BeSameAs(left);
        expr.Right.Should().BeSameAs(right);
    }

    [Fact]
    public void RelationExpression_FactoryMethods_CreateCorrectOperator()
    {
        Expr.Equal(Expr.Literal(1), Expr.Literal(2)).Operator.Should().Be(MathOperator.Equal);
        Expr.NotEqual(Expr.Literal(1), Expr.Literal(2)).Operator.Should().Be(MathOperator.NotEqual);
        Expr.LessThan(Expr.Literal(1), Expr.Literal(2)).Operator.Should().Be(MathOperator.LessThan);
        Expr.GreaterThan(Expr.Literal(1), Expr.Literal(2)).Operator.Should().Be(MathOperator.GreaterThan);
        Expr.LessThanOrEqual(Expr.Literal(1), Expr.Literal(2)).Operator.Should().Be(MathOperator.LessThanOrEqual);
        Expr.GreaterThanOrEqual(Expr.Literal(1), Expr.Literal(2)).Operator.Should().Be(MathOperator.GreaterThanOrEqual);
    }

    [Fact]
    public void RelationExpression_Children_TwoElements()
    {
        var expr = Expr.Equal(Expr.Variable("x"), Expr.Literal(0));

        expr.Children.Should().HaveCount(2);
    }

    [Fact]
    public void RelationExpression_Equal_SameProperties()
    {
        var a = Expr.LessThan(Expr.Variable("x"), Expr.Literal(5));
        var b = Expr.LessThan(Expr.Variable("x"), Expr.Literal(5));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void RelationExpression_NotEqual_DifferentOperator()
    {
        var a = Expr.LessThan(Expr.Variable("x"), Expr.Literal(5));
        var b = Expr.GreaterThan(Expr.Variable("x"), Expr.Literal(5));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void RelationExpression_DepthAndNodeCount_CalculatedCorrectly()
    {
        var expr = Expr.Equal(Expr.Variable("x"), Expr.Literal(5));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void RelationExpression_GetHashCode_Consistent()
    {
        var a = Expr.Equal(Expr.Variable("x"), Expr.Literal(0));
        var b = Expr.Equal(Expr.Variable("x"), Expr.Literal(0));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── AssignmentExpression ─────────────────────────

    [Fact]
    public void AssignmentExpression_Kind_IsAssignment()
    {
        var expr = Expr.Assign(Expr.Variable("x"), Expr.Literal(42));

        expr.Kind.Should().Be(ExpressionKind.Assignment);
    }

    [Fact]
    public void AssignmentExpression_Properties_AreCorrect()
    {
        var target = Expr.Variable("x");
        var value = Expr.Literal(10);
        var expr = new AssignmentExpression(target, value);

        expr.Target.Should().BeSameAs(target);
        expr.Value.Should().BeSameAs(value);
    }

    [Fact]
    public void AssignmentExpression_Children_TwoElements()
    {
        var expr = Expr.Assign(Expr.Variable("x"), Expr.Literal(42));

        expr.Children.Should().HaveCount(2);
    }

    [Fact]
    public void AssignmentExpression_Equal_SameProperties()
    {
        var a = Expr.Assign(Expr.Variable("x"), Expr.Literal(5));
        var b = Expr.Assign(Expr.Variable("x"), Expr.Literal(5));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void AssignmentExpression_NotEqual_DifferentTarget()
    {
        var a = Expr.Assign(Expr.Variable("x"), Expr.Literal(5));
        var b = Expr.Assign(Expr.Variable("y"), Expr.Literal(5));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void AssignmentExpression_DepthAndNodeCount_CalculatedCorrectly()
    {
        var expr = Expr.Assign(Expr.Variable("x"), Expr.Literal(1));

        expr.Depth.Should().Be(1);
        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void AssignmentExpression_GetHashCode_Consistent()
    {
        var a = Expr.Assign(Expr.Variable("x"), Expr.Literal(42));
        var b = Expr.Assign(Expr.Variable("x"), Expr.Literal(42));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── CompositionExpression ─────────────────────────

    [Fact]
    public void CompositionExpression_Kind_IsComposition()
    {
        var expr = Expr.Compose(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("x")));

        expr.Kind.Should().Be(ExpressionKind.Composition);
    }

    [Fact]
    public void CompositionExpression_Functions_AreCorrect()
    {
        var fns = new ExprType[] { Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("x")) };
        var expr = new CompositionExpression(fns);

        expr.Functions.Should().HaveCount(2);
        expr.Functions[0].Should().BeSameAs(fns[0]);
        expr.Functions[1].Should().BeSameAs(fns[1]);
    }

    [Fact]
    public void CompositionExpression_Children_EqualsFunctions()
    {
        var sx = Expr.Sin(Expr.Variable("x"));
        var cx = Expr.Cos(Expr.Variable("x"));
        var expr = Expr.Compose(sx, cx);

        expr.Children.Should().BeSameAs(expr.Functions);
    }

    [Fact]
    public void CompositionExpression_Equal_SameFunctions()
    {
        var sx1 = Expr.Sin(Expr.Variable("x"));
        var cx1 = Expr.Cos(Expr.Variable("x"));
        var a = Expr.Compose(sx1, cx1);
        var sx2 = Expr.Sin(Expr.Variable("x"));
        var cx2 = Expr.Cos(Expr.Variable("x"));
        var b = Expr.Compose(sx2, cx2);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void CompositionExpression_NotEqual_DifferentOrder()
    {
        var a = Expr.Compose(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("x")));
        var b = Expr.Compose(Expr.Cos(Expr.Variable("x")), Expr.Sin(Expr.Variable("x")));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void CompositionExpression_NotEqual_DifferentCount()
    {
        var a = Expr.Compose(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("x")));
        var b = Expr.Compose(Expr.Sin(Expr.Variable("x")));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void CompositionExpression_Empty_Throws()
    {
        var act = () => new CompositionExpression([]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CompositionExpression_DepthAndNodeCount_CalculatedCorrectly()
    {
        var expr = Expr.Compose(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("x")));

        expr.Depth.Should().Be(2);
        expr.NodeCount.Should().Be(5);
    }

    [Fact]
    public void CompositionExpression_GetHashCode_Consistent()
    {
        var a = Expr.Compose(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("x")), Expr.Tan(Expr.Variable("x")));
        var b = Expr.Compose(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("x")), Expr.Tan(Expr.Variable("x")));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── IdentityExpression ─────────────────────────

    [Fact]
    public void IdentityExpression_Kind_IsIdentity()
    {
        var expr = Expr.Identity("add");

        expr.Kind.Should().Be(ExpressionKind.Identity);
    }

    [Fact]
    public void IdentityExpression_Operation_IsCorrect()
    {
        var expr = new IdentityExpression("multiply");

        expr.Operation.Should().Be("multiply");
    }

    [Fact]
    public void IdentityExpression_Children_Empty()
    {
        var expr = Expr.Identity("add");

        expr.Children.Should().BeEmpty();
    }

    [Fact]
    public void IdentityExpression_Depth_IsZero()
    {
        var expr = Expr.Identity("add");

        expr.Depth.Should().Be(0);
    }

    [Fact]
    public void IdentityExpression_NodeCount_IsOne()
    {
        var expr = Expr.Identity("multiply");

        expr.NodeCount.Should().Be(1);
    }

    [Fact]
    public void IdentityExpression_Equal_SameOperation()
    {
        var a = Expr.Identity("add");
        var b = Expr.Identity("add");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void IdentityExpression_NotEqual_DifferentOperation()
    {
        var a = Expr.Identity("add");
        var b = Expr.Identity("multiply");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void IdentityExpression_NullOperation_Throws()
    {
        var act = () => new IdentityExpression(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IdentityExpression_GetHashCode_Consistent()
    {
        var a = Expr.Identity("add");
        var b = Expr.Identity("add");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // ───────────────────────── NullExpression ─────────────────────────

    [Fact]
    public void NullExpression_Kind_IsNull()
    {
        var expr = NullExpression.Instance;

        expr.Kind.Should().Be(ExpressionKind.Null);
    }

    [Fact]
    public void NullExpression_IsSingleton()
    {
        var a = NullExpression.Instance;
        var b = NullExpression.Instance;

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void NullExpression_Children_Empty()
    {
        NullExpression.Instance.Children.Should().BeEmpty();
    }

    [Fact]
    public void NullExpression_Depth_IsZero()
    {
        NullExpression.Instance.Depth.Should().Be(0);
    }

    [Fact]
    public void NullExpression_NodeCount_IsOne()
    {
        NullExpression.Instance.NodeCount.Should().Be(1);
    }

    [Fact]
    public void NullExpression_Equal_AnyNullExpression()
    {
        var a = NullExpression.Instance;
        var b = NullExpression.Instance;

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void NullExpression_NotEqual_OtherExpression()
    {
        var a = NullExpression.Instance;
        var b = Expr.Literal(0);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void NullExpression_GetHashCode_Consistent()
    {
        var a = NullExpression.Instance;
        var b = NullExpression.Instance;

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void NullExpression_Factory_ReturnsSingleton()
    {
        var expr = Expr.Null;

        expr.Should().BeSameAs(NullExpression.Instance);
    }

    // ───────────────────────── AnnotatedExpression ─────────────────────────

    [Fact]
    public void AnnotatedExpression_Kind_MatchesInner()
    {
        var inner = Expr.Literal(42);
        var expr = new AnnotatedExpression(inner, "unit", "meters");

        expr.Kind.Should().Be(ExpressionKind.Literal);
    }

    [Fact]
    public void AnnotatedExpression_Properties_AreCorrect()
    {
        var inner = Expr.Variable("x");
        var expr = new AnnotatedExpression(inner, "comment", "positive");

        expr.Inner.Should().BeSameAs(inner);
        expr.Key.Should().Be("comment");
        expr.AnnotationValue.Should().Be("positive");
    }

    [Fact]
    public void AnnotatedExpression_Children_SingleElement()
    {
        var inner = Expr.Variable("x");
        var expr = inner.WithAnnotation("note", "important");

        expr.Children.Should().HaveCount(1);
        expr.Children[0].Should().BeSameAs(inner);
    }

    [Fact]
    public void AnnotatedExpression_DepthAndNodeCount_DelegatesToInner()
    {
        var inner = Expr.Literal(42);
        var expr = new AnnotatedExpression(inner, "tag", "value");

        expr.Depth.Should().Be(inner.Depth);
        expr.NodeCount.Should().Be(inner.NodeCount);
    }

    [Fact]
    public void AnnotatedExpression_Equal_SameInnerAndKey()
    {
        var a = new AnnotatedExpression(Expr.Literal(5), "unit", "kg");
        var b = new AnnotatedExpression(Expr.Literal(5), "unit", "kg");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void AnnotatedExpression_NotEqual_DifferentKey()
    {
        var a = new AnnotatedExpression(Expr.Literal(5), "a", 1);
        var b = new AnnotatedExpression(Expr.Literal(5), "b", 1);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void AnnotatedExpression_NotEqual_DifferentInner()
    {
        var a = new AnnotatedExpression(Expr.Literal(5), "tag", "v");
        var b = new AnnotatedExpression(Expr.Literal(6), "tag", "v");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void AnnotatedExpression_Equality_IgnoresAnnotationValue()
    {
        var a = new AnnotatedExpression(Expr.Literal(5), "key", "value1");
        var b = new AnnotatedExpression(Expr.Literal(5), "key", "value2");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void AnnotatedExpression_NullInner_Throws()
    {
        var act = () => new AnnotatedExpression(null!, "k", "v");

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void AnnotatedExpression_NullKey_Throws()
    {
        var act = () => new AnnotatedExpression(Expr.Literal(1), null!, "v");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AnnotatedExpression_WithAnnotation_ExtensionMethod()
    {
        var inner = Expr.Literal(100);
        var expr = inner.WithAnnotation("units", "cm");

        expr.Should().BeOfType<AnnotatedExpression>();
        expr.As<AnnotatedExpression>().Key.Should().Be("units");
        expr.As<AnnotatedExpression>().AnnotationValue.Should().Be("cm");
    }

    [Fact]
    public void AnnotatedExpression_GetHashCode_Consistent()
    {
        var a = new AnnotatedExpression(Expr.Literal(5), "tag", "val");
        var b = new AnnotatedExpression(Expr.Literal(5), "tag", "val");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
