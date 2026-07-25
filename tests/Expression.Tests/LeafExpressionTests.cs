namespace MathVerse.Expression.Tests;

public class LeafExpressionTests
{
    // ───────────────────────── LiteralExpression ─────────────────────────

    [Fact]
    public void Literal_CreatesWithCorrectValue()
    {
        var expr = new LiteralExpression(42.0);

        expr.Value.Should().Be(42.0);
        expr.Kind.Should().Be(ExpressionKind.Literal);
    }

    [Fact]
    public void Literal_IsLeafNode()
    {
        var expr = new LiteralExpression(7.5);

        expr.Depth.Should().Be(0);
        expr.NodeCount.Should().Be(1);
        expr.Children.Should().BeEmpty();
    }

    [Fact]
    public void Literal_HasUniqueNodeId()
    {
        var a = new LiteralExpression(1);
        var b = new LiteralExpression(1);

        a.NodeId.Should().NotBe(b.NodeId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-3.14)]
    [InlineData(double.MaxValue)]
    public void Literal_Equality_SameValue_ReturnsTrue(double value)
    {
        var a = new LiteralExpression(value);
        var b = new LiteralExpression(value);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Literal_Equality_DifferentValue_ReturnsFalse()
    {
        var a = new LiteralExpression(1);
        var b = new LiteralExpression(2);

        a.Equals(b).Should().BeFalse();
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Literal_Equality_DifferentType_ReturnsFalse()
    {
        var literal = new LiteralExpression(5);
        var variable = new VariableExpression("x");

        literal.Equals(variable).Should().BeFalse();
        (literal == variable).Should().BeFalse();
    }

    [Fact]
    public void Literal_Equality_Null_ReturnsFalse()
    {
        var expr = new LiteralExpression(5);

        expr.Equals(null).Should().BeFalse();
        (expr == null).Should().BeFalse();
        (expr != null).Should().BeTrue();
    }

    [Fact]
    public void Literal_GetHashCode_ConsistentWithEquals()
    {
        var a = new LiteralExpression(42);
        var b = new LiteralExpression(42);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Literal_GetHashCode_DifferentValues_HashMayDiffer()
    {
        var a = new LiteralExpression(1);
        var b = new LiteralExpression(999);

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void Literal_ToString_IntLikeValue_FormatsAsInteger()
    {
        var expr = new LiteralExpression(5);

        expr.ToString().Should().Be("5");
    }

    [Fact]
    public void Literal_ToString_DecimalValue_FormatsWithG()
    {
        var expr = new LiteralExpression(3.14);

        expr.ToString().Should().Be("3.14");
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Literal_ToString_SpecialValues_DoesNotThrow(double value)
    {
        var expr = new LiteralExpression(value);

        Action act = () => _ = expr.ToString();
        act.Should().NotThrow();
    }

    [Fact]
    public void Literal_Accept_Transformer_ReturnsExpression()
    {
        var expr = new LiteralExpression(10);
        var result = expr.Accept(ExpressionCloner.Instance);

        result.Should().BeOfType<LiteralExpression>();
        result.As<LiteralExpression>().Value.Should().Be(10);
    }

    [Fact]
    public void Literal_Accept_TypedVisitor_ReturnsT()
    {
        var expr = new LiteralExpression(7);
        var printer = ExpressionPrinter.Instance;

        var result = expr.Accept(printer);

        result.Should().Be("7");
    }

    [Fact]
    public void Literal_Accept_VoidVisitor_DoesNotThrow()
    {
        var expr = new LiteralExpression(1);
        var walker = new ExpressionWalker();

        Action act = () => expr.Accept(walker);

        act.Should().NotThrow();
    }

    [Fact]
    public void Literal_Factory_CreatesCorrectExpression()
    {
        var expr = Expr.Literal(42);

        expr.Should().BeOfType<LiteralExpression>();
        expr.Value.Should().Be(42);
        expr.Kind.Should().Be(ExpressionKind.Literal);
    }

    // ───────────────────────── VariableExpression ─────────────────────────

    [Fact]
    public void Variable_CreatesWithCorrectName()
    {
        var expr = new VariableExpression("x");

        expr.Name.Should().Be("x");
        expr.Kind.Should().Be(ExpressionKind.Variable);
    }

    [Fact]
    public void Variable_IsLeafNode()
    {
        var expr = new VariableExpression("theta");

        expr.Depth.Should().Be(0);
        expr.NodeCount.Should().Be(1);
        expr.Children.Should().BeEmpty();
    }

    [Fact]
    public void Variable_HasUniqueNodeId()
    {
        var a = new VariableExpression("x");
        var b = new VariableExpression("x");

        a.NodeId.Should().NotBe(b.NodeId);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("theta")]
    [InlineData("_")]
    [InlineData("abc123")]
    public void Variable_Equality_SameName_ReturnsTrue(string name)
    {
        var a = new VariableExpression(name);
        var b = new VariableExpression(name);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Variable_Equality_DifferentName_ReturnsFalse()
    {
        var a = new VariableExpression("x");
        var b = new VariableExpression("y");

        a.Equals(b).Should().BeFalse();
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Variable_Equality_DifferentType_ReturnsFalse()
    {
        var variable = new VariableExpression("x");
        var constant = new ConstantExpression("x", 1);

        variable.Equals(constant).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Variable_NullOrWhiteSpaceName_ThrowsArgumentException(string? name)
    {
        Action act = () => _ = new VariableExpression(name!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Variable_GetHashCode_ConsistentWithEquals()
    {
        var a = new VariableExpression("x");
        var b = new VariableExpression("x");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Variable_GetHashCode_DifferentNames_Differ()
    {
        var a = new VariableExpression("x");
        var b = new VariableExpression("y");

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void Variable_ToString_ReturnsName()
    {
        var expr = new VariableExpression("alpha");

        expr.ToString().Should().Be("alpha");
    }

    [Fact]
    public void Variable_Accept_Transformer_ReturnsExpression()
    {
        var expr = new VariableExpression("x");
        var result = expr.Accept(ExpressionCloner.Instance);

        result.Should().BeOfType<VariableExpression>();
        result.As<VariableExpression>().Name.Should().Be("x");
    }

    [Fact]
    public void Variable_Accept_TypedVisitor_ReturnsT()
    {
        var expr = new VariableExpression("z");
        var printer = ExpressionPrinter.Instance;

        var result = expr.Accept(printer);

        result.Should().Be("z");
    }

    [Fact]
    public void Variable_Accept_VoidVisitor_DoesNotThrow()
    {
        var expr = new VariableExpression("x");
        var walker = new ExpressionWalker();

        Action act = () => expr.Accept(walker);

        act.Should().NotThrow();
    }

    [Fact]
    public void Variable_Factory_CreatesCorrectExpression()
    {
        var expr = Expr.Variable("t");

        expr.Should().BeOfType<VariableExpression>();
        expr.Name.Should().Be("t");
        expr.Kind.Should().Be(ExpressionKind.Variable);
    }

    // ───────────────────────── ConstantExpression ─────────────────────────

    [Fact]
    public void Constant_CreatesWithCorrectNameAndValue()
    {
        var expr = new ConstantExpression("c", 3.14);

        expr.Name.Should().Be("c");
        expr.Value.Should().Be(3.14);
        expr.Kind.Should().Be(ExpressionKind.Constant);
    }

    [Fact]
    public void Constant_IsLeafNode()
    {
        var expr = new ConstantExpression("c", 1);

        expr.Depth.Should().Be(0);
        expr.NodeCount.Should().Be(1);
        expr.Children.Should().BeEmpty();
    }

    [Fact]
    public void Constant_HasUniqueNodeId()
    {
        var a = new ConstantExpression("c", 1);
        var b = new ConstantExpression("c", 1);

        a.NodeId.Should().NotBe(b.NodeId);
    }

    [Fact]
    public void Constant_StaticField_Pi()
    {
        ConstantExpression.Pi.Name.Should().Be("pi");
        ConstantExpression.Pi.Value.Should().Be(System.Math.PI);
    }

    [Fact]
    public void Constant_StaticField_E()
    {
        ConstantExpression.E.Name.Should().Be("e");
        ConstantExpression.E.Value.Should().Be(System.Math.E);
    }

    [Fact]
    public void Constant_StaticField_I()
    {
        ConstantExpression.I.Name.Should().Be("i");
        ConstantExpression.I.Value.Should().Be(double.NaN);
    }

    [Fact]
    public void Constant_StaticField_PositiveInfinity()
    {
        ConstantExpression.PositiveInfinity.Name.Should().Be("∞");
        ConstantExpression.PositiveInfinity.Value.Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void Constant_StaticField_NegativeInfinity()
    {
        ConstantExpression.NegativeInfinity.Name.Should().Be("-∞");
        ConstantExpression.NegativeInfinity.Value.Should().Be(double.NegativeInfinity);
    }

    [Fact]
    public void Constant_StaticField_NaN()
    {
        ConstantExpression.NaN.Name.Should().Be("NaN");
        ConstantExpression.NaN.Value.Should().Be(double.NaN);
    }

    [Fact]
    public void Constant_Equality_SameNameAndValue_ReturnsTrue()
    {
        var a = new ConstantExpression("pi", System.Math.PI);
        var b = new ConstantExpression("pi", System.Math.PI);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Constant_Equality_DifferentName_ReturnsFalse()
    {
        var a = new ConstantExpression("pi", 3.14);
        var b = new ConstantExpression("e", 3.14);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Constant_Equality_DifferentValue_ReturnsFalse()
    {
        var a = new ConstantExpression("c", 1.0);
        var b = new ConstantExpression("c", 2.0);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Constant_Equality_DifferentType_ReturnsFalse()
    {
        var constant = new ConstantExpression("pi", 3.14);
        var literal = new LiteralExpression(3.14);

        constant.Equals(literal).Should().BeFalse();
    }

    [Fact]
    public void Constant_GetHashCode_ConsistentWithEquals()
    {
        var a = new ConstantExpression("pi", System.Math.PI);
        var b = new ConstantExpression("pi", System.Math.PI);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Constant_GetHashCode_DifferentConstants_Differ()
    {
        var a = new ConstantExpression("pi", System.Math.PI);
        var b = new ConstantExpression("e", System.Math.E);

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void Constant_ToString_ReturnsName()
    {
        var expr = new ConstantExpression("pi", System.Math.PI);

        expr.ToString().Should().Be("pi");
    }

    [Fact]
    public void Constant_Accept_Transformer_ReturnsExpression()
    {
        var expr = new ConstantExpression("c", 5);
        var result = expr.Accept(ExpressionCloner.Instance);

        result.Should().BeOfType<ConstantExpression>();
        var constant = result.As<ConstantExpression>();
        constant.Name.Should().Be("c");
        constant.Value.Should().Be(5);
    }

    [Fact]
    public void Constant_Accept_TypedVisitor_ReturnsT()
    {
        var expr = ConstantExpression.Pi;
        var printer = ExpressionPrinter.Instance;

        var result = expr.Accept(printer);

        result.Should().Be("pi");
    }

    [Fact]
    public void Constant_Accept_VoidVisitor_DoesNotThrow()
    {
        var expr = ConstantExpression.E;
        var walker = new ExpressionWalker();

        Action act = () => expr.Accept(walker);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constant_Factory_CreatesCorrectExpression()
    {
        var expr = Expr.Constant("phi", 1.618);

        expr.Should().BeOfType<ConstantExpression>();
        expr.Name.Should().Be("phi");
        expr.Value.Should().Be(1.618);
        expr.Kind.Should().Be(ExpressionKind.Constant);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constant_NullOrWhiteSpaceName_ThrowsArgumentException(string? name)
    {
        Action act = () => _ = new ConstantExpression(name!, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Literal_Equals_ObjectOverload_WorksCorrectly()
    {
        var expr = new LiteralExpression(5);
        object boxed = new LiteralExpression(5);
        object notAnExpr = "hello";

        expr.Equals(boxed).Should().BeTrue();
        expr.Equals(notAnExpr).Should().BeFalse();
    }
}
