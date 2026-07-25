namespace MathVerse.Math.Parsing.Tests;

public sealed class ParserRecoveryTests
{
    private static ParserResult Parse(string source) => ParsingFacade.Parse(source);

    // ───────────────────────────────────────────────────────
    //  Empty & Whitespace Input
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_EmptyInput_ReturnsFailure()
    {
        var result = Parse("");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_OnlyWhitespace_ReturnsFailure()
    {
        var result = Parse("   ");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_OnlyTabsAndNewlines_ReturnsFailure()
    {
        var result = Parse("\t\n\r\n");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_OnlyLineComment_ReturnsFailure()
    {
        var result = Parse("// just a comment");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_OnlyBlockComment_ReturnsFailure()
    {
        var result = Parse("/* block comment */");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    // ───────────────────────────────────────────────────────
    //  Incomplete Expressions
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_IncompleteAddition_ReturnsFailure()
    {
        var result = Parse("1 +");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_IncompleteMultiply_ReturnsFailure()
    {
        var result = Parse("2 *");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_IncompletePower_ReturnsFailure()
    {
        var result = Parse("3 ^");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_OnlyOperator_ReturnsFailure()
    {
        var result = Parse("+");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_OnlyMinusOperator_ReturnsFailure()
    {
        var result = Parse("-");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_IncompleteEquation_ReturnsFailure()
    {
        var result = Parse("x =");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_SubtractTrailingOperator_ReturnsFailure()
    {
        var result = Parse("10 -");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    // ───────────────────────────────────────────────────────
    //  Mismatched Delimiters
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_MismatchedOpenParen_ReturnsFailure()
    {
        var result = Parse("(1 + 2");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_MismatchedOpenBracket_ReturnsFailure()
    {
        var result = Parse("[1, 2");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_MismatchedOpenBrace_ReturnsFailure()
    {
        var result = Parse("{1, 2");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_NestedMismatchedParens_ReturnsFailure()
    {
        var result = Parse("((1)");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_CloseParenOnly_ReturnsFailure()
    {
        var result = Parse(")");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_CloseBracketOnly_ReturnsFailure()
    {
        var result = Parse("]");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_CloseBraceOnly_ReturnsFailure()
    {
        var result = Parse("}");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    // ───────────────────────────────────────────────────────
    //  Unexpected Tokens
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_UnexpectedStar_ReturnsFailure()
    {
        var result = Parse("*");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_UnexpectedSlash_ReturnsFailure()
    {
        var result = Parse("/");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_MissingFunctionClosingParen_ReturnsFailure()
    {
        var result = Parse("sin(");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_InvalidCalculusIntegral_ReturnsFailure()
    {
        var result = Parse("\u222B");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_MinusStarSequence_ReturnsFailure()
    {
        var result = Parse("-*");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_InvalidPartialSyntax_ReturnsFailure()
    {
        var result = Parse("\u2202 + 1");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    // ───────────────────────────────────────────────────────
    //  Double Operators & Mixed Valid/Invalid
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_DoublePlus_ParsesSuccessfully()
    {
        var result = Parse("1 ++ 2");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_PlusPlusUnary_PlusParses()
    {
        var result = Parse("1 + + 2");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_ValidExpressionFollowedByGarbage_ParsesValidPart()
    {
        var result = Parse("1 + 2  @#$");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_ValidThenInvalidTokens_IgnoresTrailingGarbage()
    {
        var result = Parse("42 xyz !!!");
        result.Success.Should().BeTrue();
        ((LiteralExpressionSyntax)result.Root!).Token.Value.Should().Be(42);
    }

    // ───────────────────────────────────────────────────────
    //  Diagnostic Properties
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_ErrorDiagnostic_HasNonEmptyMessage()
    {
        var result = Parse("(");
        result.HasErrors.Should().BeTrue();
        var errors = result.Diagnostics.GetErrors();
        errors.Should().NotBeEmpty();
        errors[0].Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Parse_ErrorDiagnostic_HasCodeStartingWithMV()
    {
        var result = Parse("(");
        var errors = result.Diagnostics.GetErrors();
        errors.Should().NotBeEmpty();
        errors[0].Code.Should().StartWith("MV");
    }

    [Fact]
    public void Parse_ErrorDiagnostic_HasPositiveLineAndColumn()
    {
        var result = Parse("(");
        var errors = result.Diagnostics.GetErrors();
        errors.Should().NotBeEmpty();
        errors[0].Line.Should().BeGreaterThan(0);
        errors[0].Column.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Parse_ErrorDiagnostic_HasErrorSeverity()
    {
        var result = Parse("(");
        var errors = result.Diagnostics.GetErrors();
        errors.Should().NotBeEmpty();
        errors[0].Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_ErrorDiagnostic_HasCodeMV0001()
    {
        var result = Parse("(");
        var errors = result.Diagnostics.GetErrors();
        errors.Should().NotBeEmpty();
        errors[0].Code.Should().Be("MV0001");
    }

    // ───────────────────────────────────────────────────────
    //  Partial AST on Error
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_ErrorResult_SyntaxTreeNotNull()
    {
        var result = Parse("(");
        result.SyntaxTree.Should().NotBeNull();
    }

    [Fact]
    public void Parse_ErrorResult_SyntaxTreeHasRoot()
    {
        var result = Parse("(");
        result.SyntaxTree.Root.Should().NotBeNull();
    }

    [Fact]
    public void Parse_ErrorResult_RootIsNullOnFailure()
    {
        var result = Parse("(");
        result.Root.Should().BeNull();
    }

    [Fact]
    public void Parse_ErrorResult_SyntaxTreeRootIsFallbackLiteral()
    {
        var result = Parse("(");
        result.SyntaxTree.Root.Should().BeOfType<LiteralExpressionSyntax>();
    }

    [Fact]
    public void Parse_ErrorResult_FallbackRootHasZeroValue()
    {
        var result = Parse("(");
        var fallback = (LiteralExpressionSyntax)result.SyntaxTree.Root!;
        fallback.Token.Value.Should().Be(0);
    }

    // ───────────────────────────────────────────────────────
    //  DiagnosticBag Count
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_SingleError_HasExactlyOneDiagnostic()
    {
        var result = Parse("(");
        result.Diagnostics.GetErrors().Should().HaveCount(1);
    }

    [Fact]
    public void Parse_DifferentError_HasExactlyOneDiagnostic()
    {
        var result = Parse(")");
        result.Diagnostics.GetErrors().Should().HaveCount(1);
    }

    [Fact]
    public void Parse_SuccessResult_HasNoErrors()
    {
        var result = Parse("1 + 2");
        result.Diagnostics.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Parse_SuccessResult_DiagnosticBagCountIsZero()
    {
        var result = Parse("42");
        result.Diagnostics.Count.Should().Be(0);
    }

    // ───────────────────────────────────────────────────────
    //  Error Message Content
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_EmptyInput_ErrorMessageMentionsUnexpectedToken()
    {
        var result = Parse("");
        var errors = result.Diagnostics.GetErrors();
        errors[0].Message.Should().Contain("Unexpected token");
    }

    [Fact]
    public void Parse_CloseParenOnly_ErrorMessageMentionsCloseParen()
    {
        var result = Parse(")");
        var errors = result.Diagnostics.GetErrors();
        errors[0].Message.Should().Contain("CloseParen");
    }

    [Fact]
    public void Parse_CloseBracketOnly_ErrorMessageMentionsCloseBracket()
    {
        var result = Parse("]");
        var errors = result.Diagnostics.GetErrors();
        errors[0].Message.Should().Contain("CloseBracket");
    }

    [Fact]
    public void Parse_MismatchedParens_ErrorMessageMentionsExpected()
    {
        var result = Parse("(1 + 2");
        var errors = result.Diagnostics.GetErrors();
        errors[0].Message.Should().Contain("Expected");
    }

    [Fact]
    public void Parse_MissingFunctionArg_ErrorMessageMentionsUnexpectedToken()
    {
        var result = Parse("sin(");
        var errors = result.Diagnostics.GetErrors();
        errors[0].Message.Should().Contain("Unexpected token");
    }

    // ───────────────────────────────────────────────────────
    //  Source Position Tracking in Diagnostics
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_ErrorDiagnostic_LineIsPositive()
    {
        var result = Parse("(");
        var errors = result.Diagnostics.GetErrors();
        errors[0].Line.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Parse_ErrorDiagnostic_ColumnIsPositive()
    {
        var result = Parse("(");
        var errors = result.Diagnostics.GetErrors();
        errors[0].Column.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Parse_ErrorDiagnostic_LengthIsNonNegative()
    {
        var result = Parse("(");
        var errors = result.Diagnostics.GetErrors();
        errors[0].Length.Should().BeGreaterThanOrEqualTo(0);
    }

    // ───────────────────────────────────────────────────────
    //  Unknown Function Names
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_UnknownFunctionName_ParsesAsIdentifier()
    {
        var result = Parse("f(x)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<IdentifierNameSyntax>();
    }

    [Fact]
    public void Parse_UnknownFunctionWithMultipleArgs_ParsesAsIdentifier()
    {
        var result = Parse("myFunc(a, b, c)");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<IdentifierNameSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Malformed Numeric Input
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_MalformedScientificNotation_ParsesIntegerPart()
    {
        var result = Parse("1e5");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LiteralExpressionSyntax>();
        var lit = (LiteralExpressionSyntax)result.Root!;
        lit.Token.Value.Should().Be(100000.0);
    }

    [Fact]
    public void Parse_MultipleDotsNumber_ParsesFirstNumber()
    {
        var result = Parse("1.2.3");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<LiteralExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Multiple Error Scenarios Produce Consistent Results
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_VariousErrors_AllReturnExactlyOneError()
    {
        string[] invalidInputs = ["", "(", ")", "[", "]", "{", "}", "+", "-", "*", "/", "sin(", "\u222B"];
        foreach (var input in invalidInputs)
        {
            var result = Parse(input);
            result.HasErrors.Should().BeTrue(because: $"'{input}' should produce an error");
            result.Diagnostics.GetErrors().Length.Should().Be(1, because: $"'{input}' should produce exactly one error");
        }
    }

    [Fact]
    public void Parse_VariousErrors_AllHaveMV0001Code()
    {
        string[] invalidInputs = ["", "(", ")", "[", "]", "sin("];
        foreach (var input in invalidInputs)
        {
            var result = Parse(input);
            var errors = result.Diagnostics.GetErrors();
            errors[0].Code.Should().Be("MV0001", because: $"'{input}' should use MV0001");
        }
    }

    // ───────────────────────────────────────────────────────
    //  Valid Expressions After Invalid Are Still Parsed
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_ValidInputAfterFailedParse_SubsequentParseWorks()
    {
        Parse("(");
        var result = Parse("1 + 2");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_ParserReusable_AfterError()
    {
        var r1 = Parse("(((");
        r1.Success.Should().BeFalse();
        var r2 = Parse("sin(pi)");
        r2.Success.Should().BeTrue();
        r2.Root.Should().BeOfType<FunctionCallExpressionSyntax>();
    }

    // ───────────────────────────────────────────────────────
    //  Syntax Tree Always Has Diagnostics
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_SuccessResult_SyntaxTreeDiagnosticsIsEmpty()
    {
        var result = Parse("1 + 2");
        result.SyntaxTree.Diagnostics.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void Parse_ErrorResult_SyntaxTreeDiagnosticsHasErrors()
    {
        var result = Parse("(");
        result.SyntaxTree.Diagnostics.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_ErrorResult_SyntaxTreeDiagnosticsMatchResultDiagnostics()
    {
        var result = Parse("(");
        result.SyntaxTree.Diagnostics.Count.Should().Be(result.Diagnostics.Count);
    }

    // ───────────────────────────────────────────────────────
    //  Comment Handling
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_ExpressionWithLeadingComment_ParsesExpression()
    {
        var result = Parse("// comment\n1 + 2");
        result.Success.Should().BeTrue();
        result.Root.Should().BeOfType<BinaryExpressionSyntax>();
    }

    [Fact]
    public void Parse_ExpressionWithBlockComment_ParsesExpression()
    {
        var result = Parse("/* comment */ 42");
        result.Success.Should().BeTrue();
        ((LiteralExpressionSyntax)result.Root!).Token.Value.Should().Be(42);
    }

    // ───────────────────────────────────────────────────────
    //  Nested Errors in Different Structures
    // ───────────────────────────────────────────────────────

    [Fact]
    public void Parse_VectorMissingBracket_ReturnsFailure()
    {
        var result = Parse("[1, 2, 3");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_SetMissingBrace_ReturnsFailure()
    {
        var result = Parse("{1, 2, 3");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Parse_TupleMissingParen_ReturnsFailure()
    {
        var result = Parse("(1, 2, 3");
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }
}
