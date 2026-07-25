namespace MathVerse.Expression.Tests;

public class OperatorExpressionTests
{
    // ──────────────────────────────────────────────
    //  BinaryExpression – construction & properties
    // ──────────────────────────────────────────────

    [Fact]
    public void BinaryExpression_Constructor_SetsProperties()
    {
        var x = Expr.Variable("x");
        var y = Expr.Variable("y");
        var expr = new BinaryExpression(MathOperator.Add, x, y);

        expr.Operator.Should().Be(MathOperator.Add);
        expr.Left.Should().BeSameAs(x);
        expr.Right.Should().BeSameAs(y);
    }

    [Fact]
    public void BinaryExpression_Kind_IsBinary()
    {
        var expr = Expr.Add(Expr.Literal(1), Expr.Literal(2));

        expr.Kind.Should().Be(ExpressionKind.Binary);
    }

    [Fact]
    public void BinaryExpression_Children_ReturnsLeftAndRight()
    {
        var x = Expr.Variable("x");
        var y = Expr.Variable("y");
        var expr = Expr.Multiply(x, y);

        expr.Children.Should().HaveCount(2);
        expr.Children[0].Should().BeSameAs(x);
        expr.Children[1].Should().BeSameAs(y);
    }

    [Fact]
    public void BinaryExpression_NullOperator_Throws()
    {
        var x = Expr.Literal(1);
        var y = Expr.Literal(2);

        var act = () => new BinaryExpression(null!, x, y);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BinaryExpression_NullLeft_Throws()
    {
        var y = Expr.Literal(2);

        var act = () => new BinaryExpression(MathOperator.Add, null!, y);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void BinaryExpression_NullRight_Throws()
    {
        var x = Expr.Literal(1);

        var act = () => new BinaryExpression(MathOperator.Add, x, null!);

        act.Should().Throw<Exception>();
    }

    // ──────────────────────────────────────────
    //  BinaryExpression – depth & node count
    // ──────────────────────────────────────────

    [Fact]
    public void BinaryExpression_Depth_LeafOperands_Returns1()
    {
        var expr = Expr.Add(Expr.Literal(1), Expr.Variable("x"));

        expr.Depth.Should().Be(1);
    }

    [Fact]
    public void BinaryExpression_NodeCount_LeafOperands_Returns3()
    {
        var expr = Expr.Add(Expr.Literal(1), Expr.Variable("x"));

        expr.NodeCount.Should().Be(3);
    }

    [Fact]
    public void BinaryExpression_Depth_NestedExpression_CalculatesCorrectly()
    {
        // (x + (y * z))
        // leaves: depth 0, nodeCount 1 each
        // y*z:     depth 1, nodeCount 3
        // x + ...: depth 2, nodeCount 5
        var x = Expr.Variable("x");
        var y = Expr.Variable("y");
        var z = Expr.Variable("z");
        var expr = Expr.Add(x, Expr.Multiply(y, z));

        expr.Depth.Should().Be(2);
        expr.NodeCount.Should().Be(5);
    }

    [Fact]
    public void BinaryExpression_Depth_AsymmetricTree_CalculatesCorrectly()
    {
        // ((a + b) * c) – left depth 1, right depth 0 → depth 2
        var a = Expr.Variable("a");
        var b = Expr.Variable("b");
        var c = Expr.Variable("c");
        var expr = Expr.Multiply(Expr.Add(a, b), c);

        expr.Depth.Should().Be(2);
        expr.NodeCount.Should().Be(5);
    }

    // ──────────────────────────────────────────
    //  BinaryExpression – factory methods
    // ──────────────────────────────────────────

    [Theory]
    [InlineData("Add")]
    [InlineData("Subtract")]
    [InlineData("Multiply")]
    [InlineData("Divide")]
    [InlineData("Pow")]
    public void BinaryExpression_FactoryMethods_CreateCorrectOperator(string factory)
    {
        var one = Expr.Literal(1);
        var two = Expr.Literal(2);

        BinaryExpression expr = factory switch
        {
            "Add" => Expr.Add(one, two),
            "Subtract" => Expr.Subtract(one, two),
            "Multiply" => Expr.Multiply(one, two),
            "Divide" => Expr.Divide(one, two),
            "Pow" => Expr.Pow(one, two),
            _ => throw new InvalidOperationException()
        };

        expr.Should().NotBeNull();
        expr.Kind.Should().Be(ExpressionKind.Binary);
    }

    // ──────────────────────────────────────────
    //  BinaryExpression – structural equality
    // ──────────────────────────────────────────

    [Fact]
    public void BinaryExpression_Equal_SameOperatorAndOperands()
    {
        var a = Expr.Add(Expr.Literal(1), Expr.Variable("x"));
        var b = Expr.Add(Expr.Literal(1), Expr.Variable("x"));

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void BinaryExpression_NotEqual_DifferentOperator()
    {
        var a = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Subtract(Expr.Literal(1), Expr.Literal(2));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void BinaryExpression_NotEqual_DifferentLeft()
    {
        var a = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Add(Expr.Literal(9), Expr.Literal(2));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void BinaryExpression_NotEqual_DifferentRight()
    {
        var a = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Add(Expr.Literal(1), Expr.Literal(9));

        a.Equals(b).Should().BeFalse();
    }

    // ──────────────────────────────────────────
    //  BinaryExpression – ToString
    // ──────────────────────────────────────────

    [Fact]
    public void BinaryExpression_ToString_ParenthesizedFormat()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));

        expr.ToString().Should().Be("(x + y)");
    }

    [Fact]
    public void BinaryExpression_ToString_NestedParentheses()
    {
        // (x * (y + z))
        var expr = Expr.Multiply(Expr.Variable("x"), Expr.Add(Expr.Variable("y"), Expr.Variable("z")));

        expr.ToString().Should().Be("(x * (y + z))");
    }

    // ─────────────────────────────────────────────
    //  UnaryExpression – construction & properties
    // ─────────────────────────────────────────────

    [Fact]
    public void UnaryExpression_Constructor_SetsProperties()
    {
        var x = Expr.Variable("x");
        var expr = new UnaryExpression(MathOperator.Negate, x);

        expr.Operator.Should().Be(MathOperator.Negate);
        expr.Operand.Should().BeSameAs(x);
    }

    [Fact]
    public void UnaryExpression_Kind_IsUnary()
    {
        var expr = Expr.Negate(Expr.Literal(5));

        expr.Kind.Should().Be(ExpressionKind.Unary);
    }

    [Fact]
    public void UnaryExpression_Children_ReturnsSingleOperand()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Abs(x);

        expr.Children.Should().HaveCount(1);
        expr.Children[0].Should().BeSameAs(x);
    }

    [Fact]
    public void UnaryExpression_NullOperator_Throws()
    {
        var act = () => new UnaryExpression(null!, Expr.Literal(1));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnaryExpression_NullOperand_Throws()
    {
        var act = () => new UnaryExpression(MathOperator.Negate, null!);

        act.Should().Throw<Exception>();
    }

    // ──────────────────────────────────────────
    //  UnaryExpression – depth & node count
    // ──────────────────────────────────────────

    [Fact]
    public void UnaryExpression_Depth_LeafOperand_Returns1()
    {
        var expr = Expr.Negate(Expr.Literal(3));

        expr.Depth.Should().Be(1);
    }

    [Fact]
    public void UnaryExpression_NodeCount_LeafOperand_Returns2()
    {
        var expr = Expr.Negate(Expr.Literal(3));

        expr.NodeCount.Should().Be(2);
    }

    [Fact]
    public void UnaryExpression_Depth_NestedBinaryOperand_CalculatesCorrectly()
    {
        // -(x + y): operand depth=1 → total depth=2, nodeCount=1+3=4
        var expr = Expr.Negate(Expr.Add(Expr.Variable("x"), Expr.Variable("y")));

        expr.Depth.Should().Be(2);
        expr.NodeCount.Should().Be(4);
    }

    [Fact]
    public void UnaryExpression_Depth_DoublyNested_CalculatesCorrectly()
    {
        // |-(x + y)|: operand depth=2 → total depth=3
        var inner = Expr.Negate(Expr.Add(Expr.Variable("x"), Expr.Variable("y")));
        var expr = Expr.Abs(inner);

        expr.Depth.Should().Be(3);
        expr.NodeCount.Should().Be(5);
    }

    // ──────────────────────────────────────────
    //  UnaryExpression – factory methods
    // ──────────────────────────────────────────

    [Theory]
    [InlineData("Negate")]
    [InlineData("Abs")]
    [InlineData("Not")]
    public void UnaryExpression_FactoryMethods_CreateCorrectType(string factory)
    {
        var x = Expr.Variable("x");

        UnaryExpression expr = factory switch
        {
            "Negate" => Expr.Negate(x),
            "Abs" => Expr.Abs(x),
            "Not" => Expr.Not(x),
            _ => throw new InvalidOperationException()
        };

        expr.Should().NotBeNull();
        expr.Kind.Should().Be(ExpressionKind.Unary);
    }

    // ──────────────────────────────────────────
    //  UnaryExpression – structural equality
    // ──────────────────────────────────────────

    [Fact]
    public void UnaryExpression_Equal_SameOperatorAndOperand()
    {
        var a = Expr.Negate(Expr.Variable("x"));
        var b = Expr.Negate(Expr.Variable("x"));

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void UnaryExpression_NotEqual_DifferentOperator()
    {
        var a = Expr.Negate(Expr.Variable("x"));
        var b = Expr.Abs(Expr.Variable("x"));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void UnaryExpression_NotEqual_DifferentOperand()
    {
        var a = Expr.Negate(Expr.Variable("x"));
        var b = Expr.Negate(Expr.Variable("y"));

        a.Equals(b).Should().BeFalse();
    }

    // ──────────────────────────────────────────
    //  UnaryExpression – ToString
    // ──────────────────────────────────────────

    [Fact]
    public void UnaryExpression_ToString_Negation_PrefixMinus()
    {
        var expr = Expr.Negate(Expr.Variable("x"));

        expr.ToString().Should().Be("-x");
    }

    [Fact]
    public void UnaryExpression_ToString_Abs_ParenthesizedSymbol()
    {
        var expr = Expr.Abs(Expr.Variable("x"));

        expr.ToString().Should().Be("|·|(x)");
    }

    [Fact]
    public void UnaryExpression_ToString_Not_ParenthesizedSymbol()
    {
        var expr = Expr.Not(Expr.Variable("x"));

        expr.ToString().Should().Be("¬(x)");
    }

    [Fact]
    public void UnaryExpression_ToString_NegationNestedOperand()
    {
        var expr = Expr.Negate(Expr.Add(Expr.Variable("x"), Expr.Variable("y")));

        expr.ToString().Should().Be("-(x + y)");
    }
}
