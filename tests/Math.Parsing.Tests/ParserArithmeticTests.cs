namespace MathVerse.Math.Parsing.Tests;

public sealed class ParserArithmeticTests
{
    private static ParserResult Parse(string source) => ParsingFacade.Parse(source);

    // ───────────────────────────────────────────────────────
    //  Literals
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_SingleInteger_ReturnsLiteralExpression()
    {
        var result = Parse("42");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LiteralExpressionSyntax>();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Kind.Should().Be(SyntaxKind.RealLiteralToken);
        lit.Token.Value.Should().Be(42.0);
    }

    [Fact]
    public void Parse_SingleReal_ReturnsLiteralExpression()
    {
        var result = Parse("3.14");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LiteralExpressionSyntax>();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Kind.Should().Be(SyntaxKind.RealLiteralToken);
        lit.Token.Value.Should().Be(3.14);
    }

    [Fact]
    public void Parse_ZeroInteger_ReturnsLiteralExpression()
    {
        var result = Parse("0");
        result.Success.Should().BeTrue();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Kind.Should().Be(SyntaxKind.RealLiteralToken);
        lit.Token.Value.Should().Be(0.0);
    }

    [Fact]
    public void Parse_LargeInteger_ReturnsLiteralExpression()
    {
        var result = Parse("999999");
        result.Success.Should().BeTrue();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Value.Should().Be(999999);
    }

    [Fact]
    public void Parse_NegativeIntegerLiteral_ReturnsUnaryExpression()
    {
        var result = Parse("-42");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<UnaryExpressionSyntax>();
        var unary = (UnaryExpressionSyntax)result.Root!;
        unary.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
        unary.IsPrefix.Should().BeTrue();
        unary.Operand.Should().BeOfType<LiteralExpressionSyntax>();
        ((LiteralExpressionSyntax)unary.Operand).Token.Value.Should().Be(42);
    }

    [Fact]
    public void Parse_DecimalStartingWithDot_ReturnsRealLiteral()
    {
        var result = Parse(".5");
        result.Success.Should().BeTrue();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Kind.Should().Be(SyntaxKind.RealLiteralToken);
    }

    [Fact]
    public void Parse_PiConstant_ReturnsLiteralExpression()
    {
        var result = Parse("pi");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LiteralExpressionSyntax>();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Value.Should().Be("pi");
    }

    [Fact]
    public void Parse_EulerConstant_ReturnsLiteralExpression()
    {
        var result = Parse("e");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LiteralExpressionSyntax>();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Value.Should().Be("e");
    }

    // ───────────────────────────────────────────────────────
    //  Binary Operators
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_Addition_ReturnsBinaryExpression()
    {
        var result = Parse("1 + 2");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<BinaryExpressionSyntax>();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
        ((LiteralExpressionSyntax)bin.Left).Token.Value.Should().Be(1);
        ((LiteralExpressionSyntax)bin.Right).Token.Value.Should().Be(2);
    }

    [Fact]
    public void Parse_Subtraction_ReturnsBinaryExpression()
    {
        var result = Parse("5 - 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
        ((LiteralExpressionSyntax)bin.Left).Token.Value.Should().Be(5);
        ((LiteralExpressionSyntax)bin.Right).Token.Value.Should().Be(3);
    }

    [Fact]
    public void Parse_Multiplication_ReturnsBinaryExpression()
    {
        var result = Parse("2 * 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
    }

    [Fact]
    public void Parse_Division_ReturnsBinaryExpression()
    {
        var result = Parse("6 / 2");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.SlashToken);
    }

    [Fact]
    public void Parse_Modulo_ReturnsBinaryExpression()
    {
        var result = Parse("7 % 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.PercentToken);
    }

    [Fact]
    public void Parse_Power_ReturnsBinaryExpression()
    {
        var result = Parse("2 ^ 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.CaretToken);
        ((LiteralExpressionSyntax)bin.Left).Token.Value.Should().Be(2);
        ((LiteralExpressionSyntax)bin.Right).Token.Value.Should().Be(3);
    }

    // ───────────────────────────────────────────────────────
    //  Unary Operators
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_UnaryMinus_ReturnsUnaryExpression()
    {
        var result = Parse("-x");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<UnaryExpressionSyntax>();
        var unary = (UnaryExpressionSyntax)result.Root!;
        unary.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
        unary.IsPrefix.Should().BeTrue();
        unary.Operand.Should().BeOfType<IdentifierNameSyntax>();
        ((IdentifierNameSyntax)unary.Operand).Name.Should().Be("x");
    }

    [Fact]
    public void Parse_UnaryPlus_Stripped()
    {
        var result = Parse("+x");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<IdentifierNameSyntax>();
        ((IdentifierNameSyntax)result.Root!).Name.Should().Be("x");
    }

    [Fact]
    public void Parse_LogicalNot_ReturnsUnaryExpression()
    {
        var result = Parse("!x");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<UnaryExpressionSyntax>();
        var unary = (UnaryExpressionSyntax)result.Root!;
        unary.OperatorToken.Kind.Should().Be(SyntaxKind.ExclamationToken);
        unary.IsPrefix.Should().BeTrue();
    }

    [Fact]
    public void Parse_DoubleNegation_ReturnsNestedUnary()
    {
        var result = Parse("--x");
        result.Success.Should().BeTrue();
        var outer = (UnaryExpressionSyntax)result.Root!;
        outer.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
        outer.Operand.Should().BeOfType<UnaryExpressionSyntax>();
        var inner = (UnaryExpressionSyntax)outer.Operand;
        inner.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
    }

    [Fact]
    public void Parse_NegationOfBoolean_ReturnsUnaryExpression()
    {
        var result = Parse("!true");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<UnaryExpressionSyntax>();
        var unary = (UnaryExpressionSyntax)result.Root!;
        unary.Operand.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_NegationOfFalse_ReturnsUnaryWithFalse()
    {
        var result = Parse("!false");
        result.Success.Should().BeTrue();
        var unary = (UnaryExpressionSyntax)result.Root!;
        unary.OperatorToken.Kind.Should().Be(SyntaxKind.ExclamationToken);
        ((LiteralExpressionSyntax)unary.Operand).Token.Value.Should().Be(false);
    }

    // ───────────────────────────────────────────────────────
    //  Parenthesized Expressions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_ParenthesizedExpression_ReturnsParenthesizedExpression()
    {
        var result = Parse("(1 + 2)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<ParenthesizedExpressionSyntax>();
        var paren = (ParenthesizedExpressionSyntax)result.Root!;
        paren.Inner.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_NestedParenthesized_ReturnsNestedParenthesizedExpression()
    {
        var result = Parse("((1 + 2))");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<ParenthesizedExpressionSyntax>();
        var outer = (ParenthesizedExpressionSyntax)result.Root!;
        outer.Inner.Should().BeOfType<ParenthesizedExpressionSyntax>();
    }

    [Fact]
    public void Parse_DeeplyNestedParens_ParsesCorrectly()
    {
        var result = Parse("((((1))))");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<ParenthesizedExpressionSyntax>();
        var level1 = (ParenthesizedExpressionSyntax)result.Root!;
        level1.Inner.Should().BeOfType<ParenthesizedExpressionSyntax>();
        var level2 = (ParenthesizedExpressionSyntax)level1.Inner;
        level2.Inner.Should().BeOfType<ParenthesizedExpressionSyntax>();
        var level3 = (ParenthesizedExpressionSyntax)level2.Inner;
        level3.Inner.Should().BeOfType<ParenthesizedExpressionSyntax>();
        var level4 = (ParenthesizedExpressionSyntax)level3.Inner;
        level4.Inner.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_NegativeInParens_ReturnsParenthesizedWithUnary()
    {
        var result = Parse("(-1)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<ParenthesizedExpressionSyntax>();
        var paren = (ParenthesizedExpressionSyntax)result.Root!;
        paren.Inner.Should().BeOfType<UnaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_NegationOfParenthesizedExpression_ReturnsUnary()
    {
        var result = Parse("-(1 + 2)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<UnaryExpressionSyntax>();
        var unary = (UnaryExpressionSyntax)result.Root!;
        unary.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
        unary.Operand.Should().BeOfType<ParenthesizedExpressionSyntax>();
    }

    [Fact]
    public void Parse_ParenthesizedUnaryMinus_ReturnsParenthesized()
    {
        var result = Parse("(-x)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<ParenthesizedExpressionSyntax>();
        var paren = (ParenthesizedExpressionSyntax)result.Root!;
        paren.Inner.Should().BeOfType<UnaryExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Precedence & Associativity
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_MultiplyHigherPrecedenceThanAdd_LeftSide()
    {
        var result = Parse("1 + 2 * 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
        ((LiteralExpressionSyntax)bin.Left).Token.Value.Should().Be(1);
        bin.Right.Should().BeOfType<BinaryExpressionSyntax>();
        var mul = (BinaryExpressionSyntax)bin.Right;
        mul.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
    }

    [Fact]
    public void Parse_MultiplyHigherPrecedenceThanAdd_RightSide()
    {
        var result = Parse("2 * 3 + 1");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
        bin.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var mul = (BinaryExpressionSyntax)bin.Left;
        mul.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
        ((LiteralExpressionSyntax)bin.Right).Token.Value.Should().Be(1);
    }

    [Fact]
    public void Parse_PowerHigherPrecedenceThanMultiply()
    {
        var result = Parse("2 ^ 3 * 4");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
        bin.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var pow = (BinaryExpressionSyntax)bin.Left;
        pow.OperatorToken.Kind.Should().Be(SyntaxKind.CaretToken);
        ((LiteralExpressionSyntax)bin.Right).Token.Value.Should().Be(4);
    }

    [Fact]
    public void Parse_PowerIsLeftAssociative()
    {
        var result = Parse("2 ^ 3 ^ 4");
        result.Success.Should().BeTrue();
        var root = (BinaryExpressionSyntax)result.Root!;
        root.OperatorToken.Kind.Should().Be(SyntaxKind.CaretToken);
        root.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var leftPow = (BinaryExpressionSyntax)root.Left;
        ((LiteralExpressionSyntax)leftPow.Left).Token.Value.Should().Be(2);
        ((LiteralExpressionSyntax)leftPow.Right).Token.Value.Should().Be(3);
        ((LiteralExpressionSyntax)root.Right).Token.Value.Should().Be(4);
    }

    [Fact]
    public void Parse_MultiplyIsLeftAssociative()
    {
        var result = Parse("2 * 3 * 4");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
        bin.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var leftMul = (BinaryExpressionSyntax)bin.Left;
        leftMul.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
        ((LiteralExpressionSyntax)bin.Right).Token.Value.Should().Be(4);
    }

    [Fact]
    public void Parse_UnaryMinusParsedBeforePower()
    {
        var result = Parse("-2 ^ 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.CaretToken);
        bin.Left.Should().BeOfType<UnaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_AdditionHigherPrecedenceThanComparison()
    {
        var result = Parse("1 + 2 > 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.GreaterThanToken);
        bin.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var add = (BinaryExpressionSyntax)bin.Left;
        add.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
    }

    // ───────────────────────────────────────────────────────
    //  Multiple Operations
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_MultipleAdditions_LeftAssociative()
    {
        var result = Parse("1 + 2 + 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
        bin.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var left = (BinaryExpressionSyntax)bin.Left;
        ((LiteralExpressionSyntax)left.Left).Token.Value.Should().Be(1);
        ((LiteralExpressionSyntax)left.Right).Token.Value.Should().Be(2);
        ((LiteralExpressionSyntax)bin.Right).Token.Value.Should().Be(3);
    }

    [Fact]
    public void Parse_SubtractionChain_LeftAssociative()
    {
        var result = Parse("1 - 2 - 3");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
        bin.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var left = (BinaryExpressionSyntax)bin.Left;
        ((LiteralExpressionSyntax)left.Left).Token.Value.Should().Be(1);
        ((LiteralExpressionSyntax)left.Right).Token.Value.Should().Be(2);
        ((LiteralExpressionSyntax)bin.Right).Token.Value.Should().Be(3);
    }

    [Fact]
    public void Parse_MixedOperations_PrecedenceCorrect()
    {
        var result = Parse("1 + 2 * 3 - 4 / 2");
        result.Success.Should().BeTrue();
        var root = (BinaryExpressionSyntax)result.Root!;
        root.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
        root.Left.Should().BeOfType<BinaryExpressionSyntax>();
        root.Right.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_ParenthesizedBinaryExpression_OverridesPrecedence()
    {
        var result = Parse("(1 + 2) * (3 - 4)");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
        bin.Left.Should().BeOfType<ParenthesizedExpressionSyntax>();
        bin.Right.Should().BeOfType<ParenthesizedExpressionSyntax>();
    }

    [Fact]
    public void Parse_ComplexNestedExpression()
    {
        var result = Parse("((1 + 2) * (3 - 4)) / (5 + 6)");
        result.Success.Should().BeTrue();
        var div = (BinaryExpressionSyntax)result.Root!;
        div.OperatorToken.Kind.Should().Be(SyntaxKind.SlashToken);
        div.Left.Should().BeOfType<ParenthesizedExpressionSyntax>();
        div.Right.Should().BeOfType<ParenthesizedExpressionSyntax>();
    }

    [Fact]
    public void Parse_DivisionChain_LeftAssociative()
    {
        var result = Parse("100 / 10 / 5");
        result.Success.Should().BeTrue();
        var div = (BinaryExpressionSyntax)result.Root!;
        div.OperatorToken.Kind.Should().Be(SyntaxKind.SlashToken);
        div.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var innerDiv = (BinaryExpressionSyntax)div.Left;
        ((LiteralExpressionSyntax)innerDiv.Left).Token.Value.Should().Be(100);
        ((LiteralExpressionSyntax)innerDiv.Right).Token.Value.Should().Be(10);
        ((LiteralExpressionSyntax)div.Right).Token.Value.Should().Be(5);
    }

    [Fact]
    public void Parse_ModuloWithAddition()
    {
        var result = Parse("10 % 3 + 1");
        result.Success.Should().BeTrue();
        var add = (BinaryExpressionSyntax)result.Root!;
        add.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
        add.Left.Should().BeOfType<BinaryExpressionSyntax>();
        var mod = (BinaryExpressionSyntax)add.Left;
        mod.OperatorToken.Kind.Should().Be(SyntaxKind.PercentToken);
    }

    // ───────────────────────────────────────────────────────
    //  Variables & Identifiers
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_SingleVariable_ReturnsIdentifier()
    {
        var result = Parse("x");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<IdentifierNameSyntax>();
        ((IdentifierNameSyntax)result.Root!).Name.Should().Be("x");
    }

    [Fact]
    public void Parse_LongerIdentifier_ReturnsIdentifier()
    {
        var result = Parse("alpha");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<IdentifierNameSyntax>();
        ((IdentifierNameSyntax)result.Root!).Name.Should().Be("alpha");
    }

    [Fact]
    public void Parse_UnderscoreIdentifier_ReturnsIdentifier()
    {
        var result = Parse("_x");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<IdentifierNameSyntax>();
        ((IdentifierNameSyntax)result.Root!).Name.Should().Be("_x");
    }

    [Fact]
    public void Parse_VariableWithAddition_ReturnsBinary()
    {
        var result = Parse("x + 1");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.Left.Should().BeOfType<IdentifierNameSyntax>();
        bin.Right.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_TwoVariables_ReturnsBinary()
    {
        var result = Parse("x + y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        ((IdentifierNameSyntax)bin.Left).Name.Should().Be("x");
        ((IdentifierNameSyntax)bin.Right).Name.Should().Be("y");
    }

    [Fact]
    public void Parse_GreekLetterIdentifier_ReturnsIdentifier()
    {
        var result = Parse("\u03B1");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<IdentifierNameSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Comparison Operators
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_EqualsEquals_ReturnsBinaryExpression()
    {
        var result = Parse("x == y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.EqualsEqualsToken);
    }

    [Fact]
    public void Parse_NotEquals_ReturnsBinaryExpression()
    {
        var result = Parse("x != y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.NotEqualsToken);
    }

    [Fact]
    public void Parse_LessThan_ReturnsBinaryExpression()
    {
        var result = Parse("x < y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.LessThanToken);
    }

    [Fact]
    public void Parse_GreaterThan_ReturnsBinaryExpression()
    {
        var result = Parse("x > y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.GreaterThanToken);
    }

    [Fact]
    public void Parse_LessThanOrEqual_ReturnsBinaryExpression()
    {
        var result = Parse("x <= y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.LessThanOrEqualToken);
    }

    [Fact]
    public void Parse_GreaterThanOrEqual_ReturnsBinaryExpression()
    {
        var result = Parse("x >= y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.GreaterThanOrEqualToken);
    }

    // ───────────────────────────────────────────────────────
    //  Boolean Literals
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_True_ReturnsLiteralExpression()
    {
        var result = Parse("true");
        result.Success.Should().BeTrue();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Kind.Should().Be(SyntaxKind.TrueKeyword);
        lit.Token.Value.Should().Be(true);
    }

    [Fact]
    public void Parse_False_ReturnsLiteralExpression()
    {
        var result = Parse("false");
        result.Success.Should().BeTrue();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Kind.Should().Be(SyntaxKind.FalseKeyword);
        lit.Token.Value.Should().Be(false);
    }

    // ───────────────────────────────────────────────────────
    //  Logical Operators
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_LogicalAnd_ReturnsBinaryExpression()
    {
        var result = Parse("x && y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.WedgeToken);
    }

    [Fact]
    public void Parse_LogicalOr_ReturnsBinaryExpression()
    {
        var result = Parse("x || y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.VeeToken);
    }

    [Fact]
    public void Parse_UnicodeAnd_ReturnsBinaryExpression()
    {
        var result = Parse("x \u2227 y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.WedgeToken);
    }

    [Fact]
    public void Parse_UnicodeOr_ReturnsBinaryExpression()
    {
        var result = Parse("x \u2228 y");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.VeeToken);
    }

    [Fact]
    public void Parse_AndHasLowerPrecedenceThanComparison()
    {
        var result = Parse("x < y && y > z");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.WedgeToken);
        bin.Left.Should().BeOfType<BinaryExpressionSyntax>();
        bin.Right.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_OrHasLowerPrecedenceThanAnd()
    {
        var result = Parse("x && y || z");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.VeeToken);
        bin.Left.Should().BeOfType<BinaryExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Equations
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_SimpleEquation_ReturnsEquationExpression()
    {
        var result = Parse("x = 5");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<EquationExpressionSyntax>();
        var eq = (EquationExpressionSyntax)result.Root!;
        eq.Left.Should().BeOfType<IdentifierNameSyntax>();
        eq.Right.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_EquationWithBinaryLeft_NestsEquationInRhs()
    {
        var result = Parse("x + 1 = 5");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
        bin.Right.Should().BeOfType<EquationExpressionSyntax>();
    }

    [Fact]
    public void Parse_EquationDisabled_ReturnsAssignmentExpression()
    {
        var opts = new ParserOptions { AllowEquations = false };
        var result = ParsingFacade.Parse("x = 5", opts);
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<AssignmentExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Semicolons / Statements
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_SemicolonSeparated_ReturnsLastExpression()
    {
        var result = Parse("1; 2; 3");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LiteralExpressionSyntax>();
        ((LiteralExpressionSyntax)result.Root!).Token.Value.Should().Be(3);
    }

    // ───────────────────────────────────────────────────────
    //  Unicode Negation
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_UnicodeNegation_ReturnsUnaryExpression()
    {
        var result = Parse("\u00ACx");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<UnaryExpressionSyntax>();
        var unary = (UnaryExpressionSyntax)result.Root!;
        unary.OperatorToken.Kind.Should().Be(SyntaxKind.NegationToken);
    }

    // ───────────────────────────────────────────────────────
    //  Complex Expressions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_EquationWithPowerBothSides()
    {
        var result = Parse("x ^ 2 = y ^ 2");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<BinaryExpressionSyntax>();
        var root = (BinaryExpressionSyntax)result.Root!;
        root.OperatorToken.Kind.Should().Be(SyntaxKind.CaretToken);
        ((IdentifierNameSyntax)root.Left).Name.Should().Be("x");
        var eqRhs = (EquationExpressionSyntax)root.Right;
        ((LiteralExpressionSyntax)eqRhs.Left).Token.Value.Should().Be(2);
        var eqRhsBin = (BinaryExpressionSyntax)eqRhs.Right;
        ((IdentifierNameSyntax)eqRhsBin.Left).Name.Should().Be("y");
        ((LiteralExpressionSyntax)eqRhsBin.Right).Token.Value.Should().Be(2);
    }

    [Fact]
    public void Parse_PowerChainFourTerms()
    {
        var result = Parse("2 ^ 2 ^ 2 ^ 2");
        result.Success.Should().BeTrue();
        var root = (BinaryExpressionSyntax)result.Root!;
        root.OperatorToken.Kind.Should().Be(SyntaxKind.CaretToken);
        root.Left.Should().BeOfType<BinaryExpressionSyntax>();
        root.Right.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_NestedParenthesizedMultiplication()
    {
        var result = Parse("(a + b) * (c + d)");
        result.Success.Should().BeTrue();
        var mul = (BinaryExpressionSyntax)result.Root!;
        mul.OperatorToken.Kind.Should().Be(SyntaxKind.StarToken);
        mul.Left.Should().BeOfType<ParenthesizedExpressionSyntax>();
        mul.Right.Should().BeOfType<ParenthesizedExpressionSyntax>();
    }

    [Fact]
    public void Parse_ComparisonInsideLogicalExpression()
    {
        var result = Parse("(x > 0) && (y < 10)");
        result.Success.Should().BeTrue();
        var and = (BinaryExpressionSyntax)result.Root!;
        and.OperatorToken.Kind.Should().Be(SyntaxKind.WedgeToken);
        and.Left.Should().BeOfType<ParenthesizedExpressionSyntax>();
        and.Right.Should().BeOfType<ParenthesizedExpressionSyntax>();
    }

    [Fact]
    public void Parse_BooleanExpressionWithLiterals()
    {
        var result = Parse("true && false");
        result.Success.Should().BeTrue();
        var bin = (BinaryExpressionSyntax)result.Root!;
        bin.Left.Should().BeOfType<LiteralExpressionSyntax>();
        bin.Right.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_MultipleUnaryMinus()
    {
        var result = Parse("- - - x");
        result.Success.Should().BeTrue();
        var outer = (UnaryExpressionSyntax)result.Root!;
        var mid = (UnaryExpressionSyntax)outer.Operand;
        var inner = (UnaryExpressionSyntax)mid.Operand;
        inner.Operand.Should().BeOfType<IdentifierNameSyntax>();
    }
}
