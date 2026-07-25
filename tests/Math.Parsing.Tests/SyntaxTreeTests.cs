namespace Math.Parsing.Tests;

public class SyntaxTreeTests
{
    // ─────────────────────────────────────────────────────────
    //  SyntaxTree Basic Properties
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SyntaxTree_Root_ReturnsNonNull()
    {
        var tree = SyntaxTree.Parse("1");
        tree.Root.Should().NotBeNull();
    }

    [Fact]
    public void SyntaxTree_Diagnostics_ReturnsNonNull()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.Diagnostics.Should().NotBeNull();
    }

    [Fact]
    public void SyntaxTree_HasErrors_IsFalseForValidInput()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void SyntaxTree_Parse_CreatesTreeFromSource()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Should().NotBeNull();
        tree.Root.Should().NotBeNull();
    }

    [Fact]
    public void SyntaxTree_Root_IsExpressionSyntax()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.Root.Should().BeAssignableTo<ExpressionSyntax>();
    }

    [Fact]
    public void SyntaxTree_RootKind_IsBinaryExpression()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.Root.Kind.Should().Be(SyntaxKind.BinaryExpression);
    }

    [Fact]
    public void SyntaxTree_RootKind_IsLiteralForSingleNumber()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.Kind.Should().Be(SyntaxKind.LiteralExpression);
    }

    [Fact]
    public void SyntaxTree_RootKind_IsIdentifierForSingleVariable()
    {
        var tree = SyntaxTree.Parse("x");
        tree.Root.Kind.Should().Be(SyntaxKind.IdentifierNameExpression);
    }

    [Fact]
    public void SyntaxTree_LeadingTrivia_IsEmptyByDefault()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.LeadingTrivia.Should().BeEmpty();
    }

    [Fact]
    public void SyntaxTree_TrailingTrivia_IsEmptyByDefault()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.TrailingTrivia.Should().BeEmpty();
    }

    [Fact]
    public void SyntaxTree_GetDiagnostics_ReturnsArray()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var diagnostics = tree.GetDiagnostics();
        diagnostics.Should().NotBeNull();
    }

    [Fact]
    public void SyntaxTree_ToString_ReturnsNonEmpty()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.ToString().Should().NotBeNullOrWhiteSpace();
    }

    // ─────────────────────────────────────────────────────────
    //  SyntaxNode Properties
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SyntaxNode_Kind_ReturnsCorrectKind()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.Kind.Should().Be(SyntaxKind.LiteralExpression);
    }

    [Fact]
    public void SyntaxNode_Position_IsZeroForRoot()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.Position.Should().Be(0);
    }

    [Fact]
    public void SyntaxNode_FullLength_IsCorrectForLiteral()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.FullLength.Should().Be(2);
    }

    [Fact]
    public void SyntaxNode_EndPosition_EqualsPositionPlusFullLength()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.EndPosition.Should().Be(tree.Root.Position + tree.Root.FullLength);
    }

    [Fact]
    public void SyntaxNode_EndPosition_IsCorrectForLiteral()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.EndPosition.Should().Be(2);
    }

    [Fact]
    public void SyntaxNode_IsToken_IsFalseForExpression()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.IsToken.Should().BeFalse();
    }

    [Fact]
    public void SyntaxNode_IsTrivia_IsFalseForExpression()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.IsTrivia.Should().BeFalse();
    }

    [Fact]
    public void SyntaxNode_ToString_ReturnsNonEmpty()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void SyntaxNode_Children_IsNotEmptyForBinaryExpression()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.Root.Children.Should().NotBeEmpty();
    }

    [Fact]
    public void SyntaxNode_Position_IsZeroForBinaryRoot()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.Root.Position.Should().Be(0);
    }

    [Fact]
    public void SyntaxNode_FullLength_IsCorrectForBinaryExpression()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.FullLength.Should().Be(bin.Left.FullLength + bin.OperatorToken.FullLength + bin.Right.FullLength);
    }

    // ─────────────────────────────────────────────────────────
    //  SyntaxToken Properties
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SyntaxToken_Text_ReturnsRawText()
    {
        var token = new SyntaxToken(SyntaxKind.IntegerLiteralToken, 0, "42", 42);
        token.Text.Should().Be("42");
    }

    [Fact]
    public void SyntaxToken_Value_ReturnsParsedValue()
    {
        var token = new SyntaxToken(SyntaxKind.IntegerLiteralToken, 0, "42", 42);
        token.Value.Should().Be(42);
    }

    [Fact]
    public void SyntaxToken_Children_IsEmpty()
    {
        var token = new SyntaxToken(SyntaxKind.PlusToken, 0, "+");
        token.Children.Should().BeEmpty();
    }

    [Fact]
    public void SyntaxToken_IsToken_IsTrue()
    {
        var token = new SyntaxToken(SyntaxKind.PlusToken, 0, "+");
        token.IsToken.Should().BeTrue();
    }

    [Fact]
    public void SyntaxToken_Kind_ReturnsCorrectKind()
    {
        var token = new SyntaxToken(SyntaxKind.PlusToken, 0, "+");
        token.Kind.Should().Be(SyntaxKind.PlusToken);
    }

    [Fact]
    public void SyntaxToken_Position_ReturnsCorrectPosition()
    {
        var token = new SyntaxToken(SyntaxKind.IntegerLiteralToken, 5, "42", 42);
        token.Position.Should().Be(5);
    }

    [Fact]
    public void SyntaxToken_FullLength_EqualsTextLength()
    {
        var token = new SyntaxToken(SyntaxKind.IntegerLiteralToken, 0, "42", 42);
        token.FullLength.Should().Be(2);
    }

    [Fact]
    public void SyntaxToken_WithNullValue_HasNullValue()
    {
        var token = new SyntaxToken(SyntaxKind.PlusToken, 0, "+");
        token.Value.Should().BeNull();
    }

    [Fact]
    public void SyntaxToken_ToString_WithValue_IncludesValue()
    {
        var token = new SyntaxToken(SyntaxKind.IntegerLiteralToken, 0, "42", 42);
        token.ToString().Should().Contain("42");
    }

    [Fact]
    public void SyntaxToken_ToString_WithoutValue_ShowsKindAndText()
    {
        var token = new SyntaxToken(SyntaxKind.PlusToken, 0, "+");
        token.ToString().Should().Contain("PlusToken");
    }

    // ─────────────────────────────────────────────────────────
    //  SyntaxTrivia Properties
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SyntaxTrivia_Text_ReturnsTriviaText()
    {
        var trivia = new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, 0, "  ");
        trivia.Text.Should().Be("  ");
    }

    [Fact]
    public void SyntaxTrivia_Children_IsEmpty()
    {
        var trivia = new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, 0, "  ");
        trivia.Children.Should().BeEmpty();
    }

    [Fact]
    public void SyntaxTrivia_IsTrivia_IsTrue()
    {
        var trivia = new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, 0, "  ");
        trivia.IsTrivia.Should().BeTrue();
    }

    [Fact]
    public void SyntaxTrivia_IsToken_IsFalse()
    {
        var trivia = new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, 0, "  ");
        trivia.IsToken.Should().BeFalse();
    }

    [Fact]
    public void SyntaxTrivia_Position_IsCorrect()
    {
        var trivia = new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, 3, "  ");
        trivia.Position.Should().Be(3);
    }

    [Fact]
    public void SyntaxTrivia_FullLength_EqualsTextLength()
    {
        var trivia = new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, 0, "  ");
        trivia.FullLength.Should().Be(2);
    }

    [Fact]
    public void SyntaxTrivia_ToString_ReturnsFormattedString()
    {
        var trivia = new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, 0, "  ");
        trivia.ToString().Should().Contain("WhitespaceTrivia");
    }

    // ─────────────────────────────────────────────────────────
    //  BinaryExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void BinaryExpressionSyntax_Left_IsCorrect()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.Left.Should().NotBeNull();
        bin.Left.Kind.Should().Be(SyntaxKind.LiteralExpression);
    }

    [Fact]
    public void BinaryExpressionSyntax_Right_IsCorrect()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.Right.Should().NotBeNull();
        bin.Right.Kind.Should().Be(SyntaxKind.LiteralExpression);
    }

    [Fact]
    public void BinaryExpressionSyntax_OperatorToken_IsCorrect()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.OperatorToken.Kind.Should().Be(SyntaxKind.PlusToken);
    }

    [Fact]
    public void BinaryExpressionSyntax_OperatorToken_TextIsPlus()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.OperatorToken.Text.Should().Be("+");
    }

    [Fact]
    public void BinaryExpressionSyntax_Children_HasThreeElements()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.Children.Count.Should().Be(3);
    }

    // ─────────────────────────────────────────────────────────
    //  UnaryExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void UnaryExpressionSyntax_Operand_IsCorrect()
    {
        var tree = SyntaxTree.Parse("-x");
        var unary = (UnaryExpressionSyntax)tree.Root;
        unary.Operand.Should().NotBeNull();
        unary.Operand.Kind.Should().Be(SyntaxKind.IdentifierNameExpression);
    }

    [Fact]
    public void UnaryExpressionSyntax_OperatorToken_IsMinus()
    {
        var tree = SyntaxTree.Parse("-x");
        var unary = (UnaryExpressionSyntax)tree.Root;
        unary.OperatorToken.Kind.Should().Be(SyntaxKind.MinusToken);
    }

    [Fact]
    public void UnaryExpressionSyntax_IsPrefix_IsTrue()
    {
        var tree = SyntaxTree.Parse("-x");
        var unary = (UnaryExpressionSyntax)tree.Root;
        unary.IsPrefix.Should().BeTrue();
    }

    [Fact]
    public void UnaryExpressionSyntax_Position_IsOperatorPosition()
    {
        var tree = SyntaxTree.Parse("-x");
        var unary = (UnaryExpressionSyntax)tree.Root;
        unary.Position.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────
    //  ParenthesizedExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ParenthesizedExpressionSyntax_Inner_IsCorrect()
    {
        var tree = SyntaxTree.Parse("(1 + 2)");
        tree.Root.Kind.Should().Be(SyntaxKind.ParenthesizedExpression);
        var paren = (ParenthesizedExpressionSyntax)tree.Root;
        paren.Inner.Should().NotBeNull();
        paren.Inner.Kind.Should().Be(SyntaxKind.BinaryExpression);
    }

    [Fact]
    public void ParenthesizedExpressionSyntax_OpenParen_IsCorrect()
    {
        var tree = SyntaxTree.Parse("(1 + 2)");
        var paren = (ParenthesizedExpressionSyntax)tree.Root;
        paren.OpenParen.Text.Should().Be("(");
    }

    [Fact]
    public void ParenthesizedExpressionSyntax_CloseParen_IsCorrect()
    {
        var tree = SyntaxTree.Parse("(1 + 2)");
        var paren = (ParenthesizedExpressionSyntax)tree.Root;
        paren.CloseParen.Text.Should().Be(")");
    }

    // ─────────────────────────────────────────────────────────
    //  FunctionCallExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void FunctionCallExpressionSyntax_FunctionName_IsSin()
    {
        var tree = SyntaxTree.Parse("sin(x)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.FunctionName.Should().Be("sin");
    }

    [Fact]
    public void FunctionCallExpressionSyntax_Arguments_HasOneArg()
    {
        var tree = SyntaxTree.Parse("sin(x)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.Arguments.Count.Should().Be(1);
    }

    [Fact]
    public void FunctionCallExpressionSyntax_Argument_IsIdentifier()
    {
        var tree = SyntaxTree.Parse("sin(x)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.Arguments[0].Kind.Should().Be(SyntaxKind.IdentifierNameExpression);
    }

    [Fact]
    public void FunctionCallExpressionSyntax_OpenParen_IsCorrect()
    {
        var tree = SyntaxTree.Parse("sin(x)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.OpenParen.Text.Should().Be("(");
    }

    [Fact]
    public void FunctionCallExpressionSyntax_CloseParen_IsCorrect()
    {
        var tree = SyntaxTree.Parse("sin(x)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.CloseParen.Text.Should().Be(")");
    }

    [Fact]
    public void FunctionCallExpressionSyntax_TwoArgs_HasTwoArguments()
    {
        var tree = SyntaxTree.Parse("log(x, 10)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.Arguments.Count.Should().Be(2);
    }

    [Fact]
    public void FunctionCallExpressionSyntax_NameToken_TextIsFunctionName()
    {
        var tree = SyntaxTree.Parse("sqrt(x)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.NameToken.Text.Should().Be("sqrt");
    }

    // ─────────────────────────────────────────────────────────
    //  LiteralExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LiteralExpressionSyntax_Token_IsCorrect()
    {
        var tree = SyntaxTree.Parse("42");
        var lit = (LiteralExpressionSyntax)tree.Root;
        lit.Token.Kind.Should().Be(SyntaxKind.RealLiteralToken);
        lit.Token.Text.Should().Be("42");
    }

    [Fact]
    public void LiteralExpressionSyntax_Token_ValueIsParsed()
    {
        var tree = SyntaxTree.Parse("42");
        var lit = (LiteralExpressionSyntax)tree.Root;
        lit.Token.Value.Should().Be(42);
    }

    [Fact]
    public void LiteralExpressionSyntax_Children_HasOneToken()
    {
        var tree = SyntaxTree.Parse("42");
        var lit = (LiteralExpressionSyntax)tree.Root;
        lit.Children.Count.Should().Be(1);
    }

    [Fact]
    public void LiteralExpressionSyntax_RealLiteral_HasCorrectText()
    {
        var tree = SyntaxTree.Parse("3.14");
        var lit = (LiteralExpressionSyntax)tree.Root;
        lit.Token.Text.Should().Be("3.14");
    }

    // ─────────────────────────────────────────────────────────
    //  IdentifierNameSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void IdentifierNameSyntax_Name_IsCorrect()
    {
        var tree = SyntaxTree.Parse("x");
        var id = (IdentifierNameSyntax)tree.Root;
        id.Name.Should().Be("x");
    }

    [Fact]
    public void IdentifierNameSyntax_Identifier_TextIsCorrect()
    {
        var tree = SyntaxTree.Parse("x");
        var id = (IdentifierNameSyntax)tree.Root;
        id.Identifier.Text.Should().Be("x");
    }

    [Fact]
    public void IdentifierNameSyntax_Children_HasOneToken()
    {
        var tree = SyntaxTree.Parse("x");
        var id = (IdentifierNameSyntax)tree.Root;
        id.Children.Count.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────
    //  EquationExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void EquationExpressionSyntax_Left_IsCorrect()
    {
        var tree = SyntaxTree.Parse("x = 5");
        var eq = (EquationExpressionSyntax)tree.Root;
        eq.Left.Should().NotBeNull();
        eq.Left.Kind.Should().Be(SyntaxKind.IdentifierNameExpression);
    }

    [Fact]
    public void EquationExpressionSyntax_Right_IsCorrect()
    {
        var tree = SyntaxTree.Parse("x = 5");
        var eq = (EquationExpressionSyntax)tree.Root;
        eq.Right.Should().NotBeNull();
        eq.Right.Kind.Should().Be(SyntaxKind.LiteralExpression);
    }

    [Fact]
    public void EquationExpressionSyntax_EqualsToken_IsCorrect()
    {
        var tree = SyntaxTree.Parse("x = 5");
        var eq = (EquationExpressionSyntax)tree.Root;
        eq.EqualsToken.Kind.Should().Be(SyntaxKind.EqualsToken);
    }

    [Fact]
    public void EquationExpressionSyntax_Children_HasThreeElements()
    {
        var tree = SyntaxTree.Parse("x = 5");
        var eq = (EquationExpressionSyntax)tree.Root;
        eq.Children.Count.Should().Be(3);
    }

    // ─────────────────────────────────────────────────────────
    //  ConditionalExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ConditionalExpressionSyntax_HasAllBranches()
    {
        var tree = SyntaxTree.Parse("if x > 0 then x else -x");
        var cond = (ConditionalExpressionSyntax)tree.Root;
        cond.Condition.Should().NotBeNull();
        cond.ThenBranch.Should().NotBeNull();
        cond.ElseBranch.Should().NotBeNull();
    }

    [Fact]
    public void ConditionalExpressionSyntax_IfKeyword_IsCorrect()
    {
        var tree = SyntaxTree.Parse("if x > 0 then x else -x");
        var cond = (ConditionalExpressionSyntax)tree.Root;
        cond.IfKeyword.Kind.Should().Be(SyntaxKind.IfKeyword);
    }

    [Fact]
    public void ConditionalExpressionSyntax_ThenKeyword_IsCorrect()
    {
        var tree = SyntaxTree.Parse("if x > 0 then x else -x");
        var cond = (ConditionalExpressionSyntax)tree.Root;
        cond.ThenKeyword.Kind.Should().Be(SyntaxKind.ThenKeyword);
    }

    [Fact]
    public void ConditionalExpressionSyntax_ElseKeyword_IsCorrect()
    {
        var tree = SyntaxTree.Parse("if x > 0 then x else -x");
        var cond = (ConditionalExpressionSyntax)tree.Root;
        cond.ElseKeyword.Kind.Should().Be(SyntaxKind.ElseKeyword);
    }

    [Fact]
    public void ConditionalExpressionSyntax_Children_HasSixElements()
    {
        var tree = SyntaxTree.Parse("if x > 0 then x else -x");
        var cond = (ConditionalExpressionSyntax)tree.Root;
        cond.Children.Count.Should().Be(6);
    }

    // ─────────────────────────────────────────────────────────
    //  LambdaExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LambdaExpressionSyntax_Parameters_IsCorrect()
    {
        var tree = SyntaxTree.Parse("fn(x) \u2192 x + 1");
        var lam = (LambdaExpressionSyntax)tree.Root;
        lam.Parameters.Count.Should().Be(1);
        lam.Parameters[0].Name.Should().Be("x");
    }

    [Fact]
    public void LambdaExpressionSyntax_Body_IsCorrect()
    {
        var tree = SyntaxTree.Parse("fn(x) \u2192 x + 1");
        var lam = (LambdaExpressionSyntax)tree.Root;
        lam.Body.Should().NotBeNull();
        lam.Body.Kind.Should().Be(SyntaxKind.BinaryExpression);
    }

    [Fact]
    public void LambdaExpressionSyntax_FnKeyword_IsCorrect()
    {
        var tree = SyntaxTree.Parse("fn(x) \u2192 x + 1");
        var lam = (LambdaExpressionSyntax)tree.Root;
        lam.FnKeyword.Kind.Should().Be(SyntaxKind.FnKeyword);
    }

    [Fact]
    public void LambdaExpressionSyntax_TwoParams_HasTwoParameters()
    {
        var tree = SyntaxTree.Parse("fn(x, y) \u2192 x + y");
        var lam = (LambdaExpressionSyntax)tree.Root;
        lam.Parameters.Count.Should().Be(2);
    }

    [Fact]
    public void LambdaExpressionSyntax_CloseParen_IsCorrect()
    {
        var tree = SyntaxTree.Parse("fn(x) \u2192 x + 1");
        var lam = (LambdaExpressionSyntax)tree.Root;
        lam.CloseParen.Text.Should().Be(")");
    }

    // ─────────────────────────────────────────────────────────
    //  VectorLiteralExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void VectorLiteralExpressionSyntax_Elements_HasCorrectCount()
    {
        var tree = SyntaxTree.Parse("[1, 2, 3]");
        var vec = (VectorLiteralExpressionSyntax)tree.Root;
        vec.Elements.Count.Should().Be(3);
    }

    [Fact]
    public void VectorLiteralExpressionSyntax_OpenBracket_IsCorrect()
    {
        var tree = SyntaxTree.Parse("[1, 2, 3]");
        var vec = (VectorLiteralExpressionSyntax)tree.Root;
        vec.OpenBracket.Text.Should().Be("[");
    }

    [Fact]
    public void VectorLiteralExpressionSyntax_CloseBracket_IsCorrect()
    {
        var tree = SyntaxTree.Parse("[1, 2, 3]");
        var vec = (VectorLiteralExpressionSyntax)tree.Root;
        vec.CloseBracket.Text.Should().Be("]");
    }

    // ─────────────────────────────────────────────────────────
    //  SetLiteralExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SetLiteralExpressionSyntax_Elements_HasCorrectCount()
    {
        var tree = SyntaxTree.Parse("{1, 2, 3}");
        var set = (SetLiteralExpressionSyntax)tree.Root;
        set.Elements.Count.Should().Be(3);
    }

    [Fact]
    public void SetLiteralExpressionSyntax_OpenBrace_IsCorrect()
    {
        var tree = SyntaxTree.Parse("{1, 2, 3}");
        var set = (SetLiteralExpressionSyntax)tree.Root;
        set.OpenBrace.Text.Should().Be("{");
    }

    [Fact]
    public void SetLiteralExpressionSyntax_CloseBrace_IsCorrect()
    {
        var tree = SyntaxTree.Parse("{1, 2, 3}");
        var set = (SetLiteralExpressionSyntax)tree.Root;
        set.CloseBrace.Text.Should().Be("}");
    }

    // ─────────────────────────────────────────────────────────
    //  TupleExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void TupleExpressionSyntax_Elements_HasCorrectCount()
    {
        var tree = SyntaxTree.Parse("(1, 2, 3)");
        var tuple = (TupleExpressionSyntax)tree.Root;
        tuple.Elements.Count.Should().Be(3);
    }

    [Fact]
    public void TupleExpressionSyntax_OpenParen_IsCorrect()
    {
        var tree = SyntaxTree.Parse("(1, 2, 3)");
        var tuple = (TupleExpressionSyntax)tree.Root;
        tuple.OpenParen.Text.Should().Be("(");
    }

    [Fact]
    public void TupleExpressionSyntax_CloseParen_IsCorrect()
    {
        var tree = SyntaxTree.Parse("(1, 2, 3)");
        var tuple = (TupleExpressionSyntax)tree.Root;
        tuple.CloseParen.Text.Should().Be(")");
    }

    [Fact]
    public void TupleExpressionSyntax_EmptyTuple_HasZeroElements()
    {
        var tree = SyntaxTree.Parse("()");
        var tuple = (TupleExpressionSyntax)tree.Root;
        tuple.Elements.Count.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────
    //  Vector and Set Empty
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void VectorLiteralExpressionSyntax_Empty_HasZeroElements()
    {
        var tree = SyntaxTree.Parse("[]");
        var vec = (VectorLiteralExpressionSyntax)tree.Root;
        vec.Elements.Count.Should().Be(0);
    }

    [Fact]
    public void SetLiteralExpressionSyntax_Empty_HasZeroElements()
    {
        var tree = SyntaxTree.Parse("{}");
        var set = (SetLiteralExpressionSyntax)tree.Root;
        set.Elements.Count.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────
    //  Children Enumeration
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void BinaryExpressionSyntax_Children_MatchesStructure()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.Children[0].Should().Be(bin.Left);
        bin.Children[1].Should().Be(bin.OperatorToken);
        bin.Children[2].Should().Be(bin.Right);
    }

    [Fact]
    public void LiteralExpressionSyntax_Children_MatchesToken()
    {
        var tree = SyntaxTree.Parse("42");
        var lit = (LiteralExpressionSyntax)tree.Root;
        lit.Children[0].Should().Be(lit.Token);
    }

    [Fact]
    public void IdentifierNameSyntax_Children_MatchesIdentifier()
    {
        var tree = SyntaxTree.Parse("x");
        var id = (IdentifierNameSyntax)tree.Root;
        id.Children[0].Should().Be(id.Identifier);
    }

    [Fact]
    public void FunctionCallExpressionSyntax_Children_IncludesNameAndArgs()
    {
        var tree = SyntaxTree.Parse("sin(x)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.Children.Count.Should().Be(4);
    }

    [Fact]
    public void EquationExpressionSyntax_Children_MatchesStructure()
    {
        var tree = SyntaxTree.Parse("x = 5");
        var eq = (EquationExpressionSyntax)tree.Root;
        eq.Children.Count.Should().Be(3);
    }

    // ─────────────────────────────────────────────────────────
    //  Token Position Tracking
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Token_Position_MatchesSourceOffset()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.Left.Position.Should().Be(0);
        bin.OperatorToken.Position.Should().Be(2);
        bin.Right.Position.Should().Be(4);
    }

    [Fact]
    public void Token_Text_MatchesSourceSubstring()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        var bin = (BinaryExpressionSyntax)tree.Root;
        bin.Left.Should().BeOfType<LiteralExpressionSyntax>();
        ((LiteralExpressionSyntax)bin.Left).Token.Text.Should().Be("1");
        bin.Right.Should().BeOfType<LiteralExpressionSyntax>();
        ((LiteralExpressionSyntax)bin.Right).Token.Text.Should().Be("2");
    }

    // ─────────────────────────────────────────────────────────
    //  PostfixExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void PostfixExpressionSyntax_Operand_IsCorrect()
    {
        var tree = SyntaxTree.Parse("n!");
        var postfix = (PostfixExpressionSyntax)tree.Root;
        postfix.Operand.Should().NotBeNull();
        postfix.Operand.Kind.Should().Be(SyntaxKind.IdentifierNameExpression);
    }

    [Fact]
    public void PostfixExpressionSyntax_OperatorToken_IsExclamation()
    {
        var tree = SyntaxTree.Parse("n!");
        var postfix = (PostfixExpressionSyntax)tree.Root;
        postfix.OperatorToken.Kind.Should().Be(SyntaxKind.ExclamationToken);
    }

    [Fact]
    public void PostfixExpressionSyntax_Children_HasTwoElements()
    {
        var tree = SyntaxTree.Parse("n!");
        var postfix = (PostfixExpressionSyntax)tree.Root;
        postfix.Children.Count.Should().Be(2);
    }

    // ─────────────────────────────────────────────────────────
    //  SummationExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SummationExpressionSyntax_VariableToken_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u2211 n=1,10 n");
        var sum = (SummationExpressionSyntax)tree.Root;
        sum.VariableToken.Text.Should().Be("n");
    }

    [Fact]
    public void SummationExpressionSyntax_LowerBound_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u2211 n=1,10 n");
        var sum = (SummationExpressionSyntax)tree.Root;
        sum.LowerBound.Should().NotBeNull();
    }

    [Fact]
    public void SummationExpressionSyntax_UpperBound_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u2211 n=1,10 n");
        var sum = (SummationExpressionSyntax)tree.Root;
        sum.UpperBound.Should().NotBeNull();
    }

    [Fact]
    public void SummationExpressionSyntax_Body_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u2211 n=1,10 n");
        var sum = (SummationExpressionSyntax)tree.Root;
        sum.Body.Should().NotBeNull();
    }

    // ─────────────────────────────────────────────────────────
    //  LimitExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LimitExpressionSyntax_LimitKeyword_IsCorrect()
    {
        var tree = SyntaxTree.Parse("lim x \u2192 x 0");
        var lim = (LimitExpressionSyntax)tree.Root;
        lim.LimitKeyword.Kind.Should().Be(SyntaxKind.LimitKeyword);
    }

    [Fact]
    public void LimitExpressionSyntax_Body_IsCorrect()
    {
        var tree = SyntaxTree.Parse("lim x \u2192 x 0");
        var lim = (LimitExpressionSyntax)tree.Root;
        lim.Body.Should().NotBeNull();
    }

    [Fact]
    public void LimitExpressionSyntax_VariableToken_IsCorrect()
    {
        var tree = SyntaxTree.Parse("lim x \u2192 x 0");
        var lim = (LimitExpressionSyntax)tree.Root;
        lim.VariableToken.Text.Should().Be("x");
    }

    [Fact]
    public void LimitExpressionSyntax_Target_IsCorrect()
    {
        var tree = SyntaxTree.Parse("lim x \u2192 x 0");
        var lim = (LimitExpressionSyntax)tree.Root;
        lim.Target.Should().NotBeNull();
    }

    [Fact]
    public void LimitExpressionSyntax_Children_HasFiveElements()
    {
        var tree = SyntaxTree.Parse("lim x \u2192 x 0");
        var lim = (LimitExpressionSyntax)tree.Root;
        lim.Children.Count.Should().Be(5);
    }

    // ─────────────────────────────────────────────────────────
    //  DerivativeExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void DerivativeExpressionSyntax_VariableToken_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u2202x sin(x)");
        var deriv = (DerivativeExpressionSyntax)tree.Root;
        deriv.VariableToken.Text.Should().Be("x");
    }

    [Fact]
    public void DerivativeExpressionSyntax_Function_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u2202x sin(x)");
        var deriv = (DerivativeExpressionSyntax)tree.Root;
        deriv.Function.Should().NotBeNull();
        deriv.Function.Kind.Should().Be(SyntaxKind.FunctionCallExpression);
    }

    [Fact]
    public void DerivativeExpressionSyntax_DifferentialToken_IsPartial()
    {
        var tree = SyntaxTree.Parse("\u2202x sin(x)");
        var deriv = (DerivativeExpressionSyntax)tree.Root;
        deriv.DifferentialToken.Kind.Should().Be(SyntaxKind.PartialToken);
    }

    [Fact]
    public void DerivativeExpressionSyntax_Children_HasFourElements()
    {
        var tree = SyntaxTree.Parse("\u2202x sin(x)");
        var deriv = (DerivativeExpressionSyntax)tree.Root;
        deriv.Children.Count.Should().Be(4);
    }

    // ─────────────────────────────────────────────────────────
    //  AssignmentExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void AssignmentExpressionSyntax_Target_IsCorrect()
    {
        var opts = new ParserOptions { AllowEquations = false, AllowAssignments = true };
        var tree = SyntaxTree.Parse("x = 1", parserOptions: opts);
        var assign = (AssignmentExpressionSyntax)tree.Root;
        assign.Target.Should().NotBeNull();
        assign.Target.Kind.Should().Be(SyntaxKind.IdentifierNameExpression);
    }

    [Fact]
    public void AssignmentExpressionSyntax_Value_IsCorrect()
    {
        var opts = new ParserOptions { AllowEquations = false, AllowAssignments = true };
        var tree = SyntaxTree.Parse("x = 1", parserOptions: opts);
        var assign = (AssignmentExpressionSyntax)tree.Root;
        assign.Value.Should().NotBeNull();
        assign.Value.Kind.Should().Be(SyntaxKind.LiteralExpression);
    }

    // ─────────────────────────────────────────────────────────
    //  ProductExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProductExpressionSyntax_VariableToken_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u220F n=1,5 n");
        var prod = (ProductExpressionSyntax)tree.Root;
        prod.VariableToken.Text.Should().Be("n");
    }

    [Fact]
    public void ProductExpressionSyntax_Body_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u220F n=1,5 n");
        var prod = (ProductExpressionSyntax)tree.Root;
        prod.Body.Should().NotBeNull();
    }

    // ─────────────────────────────────────────────────────────
    //  IntervalExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void IntervalExpressionSyntax_BracketVector_ParsesAsVector()
    {
        var tree = SyntaxTree.Parse("[1, 2]");
        tree.Root.Kind.Should().Be(SyntaxKind.VectorExpression);
    }

    [Fact]
    public void IntervalExpressionSyntax_BracketVector_HasTwoElements()
    {
        var tree = SyntaxTree.Parse("[1, 2]");
        var vec = (VectorLiteralExpressionSyntax)tree.Root;
        vec.Elements.Count.Should().Be(2);
    }

    // ─────────────────────────────────────────────────────────
    //  Multiple Children Enumeration
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void MultipleChildren_DeepBinary_AllChildrenEnumerated()
    {
        var tree = SyntaxTree.Parse("1 + 2 + 3");
        tree.Root.Children.Count.Should().Be(3);
    }

    [Fact]
    public void MultipleChildren_FunctionCall_AllChildrenEnumerated()
    {
        var tree = SyntaxTree.Parse("sin(1, 2, 3)");
        var func = (FunctionCallExpressionSyntax)tree.Root;
        func.Children.Count.Should().Be(6);
    }

    // ─────────────────────────────────────────────────────────
    //  PiecewiseExpressionSyntax
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void PiecewiseExpressionSyntax_Cases_IsNotEmpty()
    {
        var tree = SyntaxTree.Parse("piecewise { x where x > 0, 0 where x <= 0 }");
        tree.Root.Kind.Should().Be(SyntaxKind.PiecewiseExpression);
    }

    // ─────────────────────────────────────────────────────────
    //  Edge Cases
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void EmptyTuple_HasZeroElements()
    {
        var tree = SyntaxTree.Parse("()");
        var tuple = (TupleExpressionSyntax)tree.Root;
        tuple.Elements.Should().BeEmpty();
    }

    [Fact]
    public void EmptyVector_HasZeroElements()
    {
        var tree = SyntaxTree.Parse("[]");
        var vec = (VectorLiteralExpressionSyntax)tree.Root;
        vec.Elements.Should().BeEmpty();
    }

    [Fact]
    public void EmptySet_HasZeroElements()
    {
        var tree = SyntaxTree.Parse("{}");
        var set = (SetLiteralExpressionSyntax)tree.Root;
        set.Elements.Should().BeEmpty();
    }

    [Fact]
    public void SyntaxTree_Root_EndPosition_IsCorrect()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.EndPosition.Should().Be(2);
    }

    [Fact]
    public void SyntaxNode_Kind_LiteralExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("42");
        tree.Root.Kind.Should().Be(SyntaxKind.LiteralExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_BinaryExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("1 + 2");
        tree.Root.Kind.Should().Be(SyntaxKind.BinaryExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_UnaryExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("-x");
        tree.Root.Kind.Should().Be(SyntaxKind.UnaryExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_FunctionCallExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("sin(x)");
        tree.Root.Kind.Should().Be(SyntaxKind.FunctionCallExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_EquationExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("x = 5");
        tree.Root.Kind.Should().Be(SyntaxKind.EquationExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_ConditionalExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("if x > 0 then x else 0");
        tree.Root.Kind.Should().Be(SyntaxKind.ConditionalExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_LambdaExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("fn(x) \u2192 x");
        tree.Root.Kind.Should().Be(SyntaxKind.LambdaExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_VectorExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("[1, 2]");
        tree.Root.Kind.Should().Be(SyntaxKind.VectorExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_SetExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("{1, 2}");
        tree.Root.Kind.Should().Be(SyntaxKind.SetExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_TupleExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("(1, 2)");
        tree.Root.Kind.Should().Be(SyntaxKind.TupleExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_SummationExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u2211 n=1,5 n");
        tree.Root.Kind.Should().Be(SyntaxKind.SummationExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_ProductExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u220F n=1,5 n");
        tree.Root.Kind.Should().Be(SyntaxKind.ProductExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_LimitExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("lim x \u2192 x 0");
        tree.Root.Kind.Should().Be(SyntaxKind.LimitExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_DerivativeExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("\u2202x sin(x)");
        tree.Root.Kind.Should().Be(SyntaxKind.DerivativeExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_PostfixExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("n!");
        tree.Root.Kind.Should().Be(SyntaxKind.PostfixExpression);
    }

    [Fact]
    public void SyntaxNode_Kind_ParenthesizedExpression_IsCorrect()
    {
        var tree = SyntaxTree.Parse("(1)");
        tree.Root.Kind.Should().Be(SyntaxKind.ParenthesizedExpression);
    }
}
