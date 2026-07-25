namespace MathVerse.Math.Parsing.Tests;

public class LexerTests
{
    private static Token[] Tokenize(string source) => ParsingFacade.Tokenize(source);

    private static Token[] TokenizeWithOptions(string source, LexerOptions options) =>
        ParsingFacade.Tokenize(source, options);

    private static Token[] Tokens(string source) =>
        Tokenize(source).Where(t => t.Kind != TokenType.Eof).ToArray();

    // ─────────────────────────────────────────────
    //  Numbers
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_Zero_ProducesIntegerLiteral()
    {
        var tokens = Tokens("0");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[0].Value.Should().Be(0.0);
    }

    [Fact]
    public void Tokenize_FortyTwo_ProducesIntegerLiteral()
    {
        var tokens = Tokens("42");
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[0].Value.Should().Be(42.0);
    }

    [Fact]
    public void Tokenize_ThreeDotFourteen_ProducesRealLiteral()
    {
        var tokens = Tokens("3.14");
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[0].Value.Should().Be(3.14);
    }

    [Fact]
    public void Tokenize_MinusFive_ProducesMinusThenInteger()
    {
        var tokens = Tokens("-5");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Minus);
        tokens[1].Kind.Should().Be(TokenType.RealLiteral);
        tokens[1].Value.Should().Be(5.0);
    }

    [Fact]
    public void Tokenize_ScientificOneEFive_ProducesRealLiteral()
    {
        var tokens = Tokens("1e5");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
    }

    [Fact]
    public void Tokenize_ScientificOnePointFiveEMinusTen_ProducesRealLiteral()
    {
        var tokens = Tokens("1.5E-10");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
    }

    [Fact]
    public void Tokenize_DotFive_ProducesRealLiteral()
    {
        var tokens = Tokens(".5");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
    }

    [Fact]
    public void Tokenize_LargeNumber_ProducesIntegerLiteral()
    {
        var tokens = Tokens("999999");
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[0].Value.Should().Be(999999.0);
    }

    [Fact]
    public void Tokenize_Hundred_ProducesIntegerLiteral()
    {
        var tokens = Tokens("100");
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[0].Value.Should().Be(100.0);
    }

    // ─────────────────────────────────────────────
    //  Identifiers
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_SingleVariable_ProducesIdentifier()
    {
        var tokens = Tokens("x");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Tokenize_Alpha_ProducesIdentifier()
    {
        Tokens("alpha")[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Tokenize_ComplexIdentifier_ProducesIdentifier()
    {
        Tokens("myVar123")[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Tokenize_UnderscoreStart_ProducesIdentifier()
    {
        Tokens("_foo")[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Tokenize_UnderscoreOnly_ProducesIdentifier()
    {
        Tokens("_")[0].Kind.Should().Be(TokenType.Identifier);
    }

    // ─────────────────────────────────────────────
    //  Operators
    // ─────────────────────────────────────────────

    [Theory]
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Star)]
    [InlineData("/", TokenType.Slash)]
    [InlineData("%", TokenType.Percent)]
    [InlineData("^", TokenType.Caret)]
    [InlineData("=", TokenType.Equals)]
    [InlineData("==", TokenType.EqualsEquals)]
    [InlineData("!=", TokenType.NotEquals)]
    [InlineData("<", TokenType.LessThan)]
    [InlineData(">", TokenType.GreaterThan)]
    [InlineData("<=", TokenType.LessThanOrEqual)]
    [InlineData(">=", TokenType.GreaterThanOrEqual)]
    [InlineData("!", TokenType.Exclamation)]
    [InlineData("&&", TokenType.AmpersandAmpersand)]
    [InlineData("||", TokenType.PipePipe)]
    public void Tokenize_SingleOperator_ProducesCorrectType(string source, TokenType expected)
    {
        var tokens = Tokens(source);
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(expected);
    }

    [Fact]
    public void Tokenize_PlusEquals_ProducesPlusEquals()
    {
        Tokens("+=")[0].Kind.Should().Be(TokenType.PlusEquals);
    }

    [Fact]
    public void Tokenize_MinusEquals_ProducesMinusEquals()
    {
        Tokens("-=")[0].Kind.Should().Be(TokenType.MinusEquals);
    }

    [Fact]
    public void Tokenize_StarEquals_ProducesStarEquals()
    {
        Tokens("*=")[0].Kind.Should().Be(TokenType.StarEquals);
    }

    [Fact]
    public void Tokenize_DoubleStar_ProducesTwoStarTokens()
    {
        var tokens = Tokens("**");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Star);
        tokens[1].Kind.Should().Be(TokenType.Star);
    }

    [Fact]
    public void Tokenize_ColonEquals_ProducesEqualsToken()
    {
        var tokens = Tokens(":=");
        tokens[0].Kind.Should().Be(TokenType.Equals);
    }

    // ─────────────────────────────────────────────
    //  Delimiters & Punctuation
    // ─────────────────────────────────────────────

    [Theory]
    [InlineData("(", TokenType.OpenParen)]
    [InlineData(")", TokenType.CloseParen)]
    [InlineData("[", TokenType.OpenBracket)]
    [InlineData("]", TokenType.CloseBracket)]
    [InlineData("{", TokenType.OpenBrace)]
    [InlineData("}", TokenType.CloseBrace)]
    public void Tokenize_Delimiter_ProducesCorrectType(string source, TokenType expected)
    {
        Tokens(source)[0].Kind.Should().Be(expected);
    }

    [Theory]
    [InlineData(",", TokenType.Comma)]
    [InlineData(";", TokenType.Semicolon)]
    [InlineData(":", TokenType.Colon)]
    public void Tokenize_Punctuation_ProducesCorrectType(string source, TokenType expected)
    {
        Tokens(source)[0].Kind.Should().Be(expected);
    }

    [Fact]
    public void Tokenize_DoubleDot_ProducesDotDot()
    {
        Tokens("..")[0].Kind.Should().Be(TokenType.DotDot);
    }

    [Fact]
    public void Tokenize_DotDotDot_ProducesDotDotThenDot()
    {
        var tokens = Tokens("...");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.DotDot);
        tokens[1].Kind.Should().Be(TokenType.Dot);
    }

    [Fact]
    public void Tokenize_SingleDot_ProducesDot()
    {
        Tokens(".")[0].Kind.Should().Be(TokenType.Dot);
    }

    [Fact]
    public void Tokenize_SinglePipe_ProducesPipe()
    {
        Tokens("|")[0].Kind.Should().Be(TokenType.Pipe);
    }

    // ─────────────────────────────────────────────
    //  Strings
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_SimpleString_ProducesStringLiteral()
    {
        var tokens = Tokens("\"hello\"");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.StringLiteral);
        tokens[0].Value.Should().Be("hello");
    }

    [Fact]
    public void Tokenize_EscapedQuotes_ProducesCorrectValue()
    {
        var tokens = Tokens("\"with \\\"escape\\\"\"");
        tokens[0].Kind.Should().Be(TokenType.StringLiteral);
        tokens[0].Value.Should().Be("with \"escape\"");
    }

    [Fact]
    public void Tokenize_EmptyString_ProducesStringLiteral()
    {
        var tokens = Tokens("\"\"");
        tokens[0].Kind.Should().Be(TokenType.StringLiteral);
        tokens[0].Value.Should().Be(string.Empty);
    }

    [Fact]
    public void Tokenize_StringWithNewlineEscape_ProducesCorrectValue()
    {
        var tokens = Tokens("\"line1\\nline2\"");
        tokens[0].Kind.Should().Be(TokenType.StringLiteral);
        tokens[0].Value.Should().Be("line1\nline2");
    }

    [Fact]
    public void Tokenize_StringWithTabEscape_ProducesCorrectValue()
    {
        var tokens = Tokens("\"col1\\tcol2\"");
        tokens[0].Kind.Should().Be(TokenType.StringLiteral);
        tokens[0].Value.Should().Be("col1\tcol2");
    }

    // ─────────────────────────────────────────────
    //  Keywords
    // ─────────────────────────────────────────────

    [Theory]
    [InlineData("fn", TokenType.KeywordFn)]
    [InlineData("if", TokenType.KeywordIf)]
    [InlineData("then", TokenType.KeywordThen)]
    [InlineData("else", TokenType.KeywordElse)]
    [InlineData("elif", TokenType.KeywordElif)]
    [InlineData("let", TokenType.KeywordLet)]
    [InlineData("in", TokenType.KeywordIn)]
    [InlineData("where", TokenType.KeywordWhere)]
    [InlineData("piecewise", TokenType.KeywordPiecewise)]
    [InlineData("true", TokenType.KeywordTrue)]
    [InlineData("false", TokenType.KeywordFalse)]
    public void Tokenize_Keyword_ProducesCorrectType(string source, TokenType expected)
    {
        Tokens(source)[0].Kind.Should().Be(expected);
    }

    [Fact]
    public void Tokenize_Lim_ProducesLimitToken()
    {
        Tokens("lim")[0].Kind.Should().Be(TokenType.Limit);
    }

    // ─────────────────────────────────────────────
    //  Named Functions
    // ─────────────────────────────────────────────

    [Theory]
    [InlineData("sin", TokenType.FuncSin)]
    [InlineData("cos", TokenType.FuncCos)]
    [InlineData("tan", TokenType.FuncTan)]
    [InlineData("asin", TokenType.FuncAsin)]
    [InlineData("acos", TokenType.FuncAcos)]
    [InlineData("atan", TokenType.FuncAtan)]
    [InlineData("sinh", TokenType.FuncSinh)]
    [InlineData("cosh", TokenType.FuncCosh)]
    [InlineData("tanh", TokenType.FuncTanh)]
    [InlineData("ln", TokenType.FuncLn)]
    [InlineData("log", TokenType.FuncLog)]
    [InlineData("log10", TokenType.FuncLog10)]
    [InlineData("exp", TokenType.FuncExp)]
    [InlineData("sqrt", TokenType.FuncSqrt)]
    [InlineData("cbrt", TokenType.FuncCbrt)]
    [InlineData("abs", TokenType.FuncAbs)]
    [InlineData("floor", TokenType.FuncFloor)]
    [InlineData("ceil", TokenType.FuncCeil)]
    [InlineData("round", TokenType.FuncRound)]
    [InlineData("min", TokenType.FuncMin)]
    [InlineData("max", TokenType.FuncMax)]
    [InlineData("det", TokenType.FuncDet)]
    [InlineData("mod", TokenType.FuncMod)]
    public void Tokenize_NamedFunction_ProducesCorrectType(string source, TokenType expected)
    {
        Tokens(source)[0].Kind.Should().Be(expected);
    }

    // ─────────────────────────────────────────────
    //  Named Constants
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_Pi_ProducesConstantPi()
    {
        var tokens = Tokens("pi");
        tokens[0].Kind.Should().Be(TokenType.ConstantPi);
        tokens[0].Value.Should().Be("pi");
    }

    [Fact]
    public void Tokenize_E_ProducesConstantE()
    {
        var tokens = Tokens("e");
        tokens[0].Kind.Should().Be(TokenType.ConstantE);
        tokens[0].Value.Should().Be("e");
    }

    [Fact]
    public void Tokenize_I_ProducesConstantI()
    {
        var tokens = Tokens("i");
        tokens[0].Kind.Should().Be(TokenType.ConstantI);
        tokens[0].Value.Should().Be("i");
    }

    // ─────────────────────────────────────────────
    //  Unicode Symbols
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_Summation_ProducesSummationToken()
    {
        Tokens("∑")[0].Kind.Should().Be(TokenType.Summation);
    }

    [Fact]
    public void Tokenize_Product_ProducesProductToken()
    {
        Tokens("∏")[0].Kind.Should().Be(TokenType.Product);
    }

    [Fact]
    public void Tokenize_Integral_ProducesIntegralToken()
    {
        Tokens("∫")[0].Kind.Should().Be(TokenType.Integral);
    }

    [Fact]
    public void Tokenize_Wedge_ProducesWedgeToken()
    {
        Tokens("∧")[0].Kind.Should().Be(TokenType.Wedge);
    }

    [Fact]
    public void Tokenize_Vee_ProducesVeeToken()
    {
        Tokens("∨")[0].Kind.Should().Be(TokenType.Vee);
    }

    [Fact]
    public void Tokenize_ElementOf_ProducesElementOfToken()
    {
        Tokens("∈")[0].Kind.Should().Be(TokenType.ElementOf);
    }

    [Fact]
    public void Tokenize_Negation_ProducesNegationToken()
    {
        Tokens("¬")[0].Kind.Should().Be(TokenType.Negation);
    }

    [Fact]
    public void Tokenize_CrossProduct_ProducesCrossProductToken()
    {
        Tokens("×")[0].Kind.Should().Be(TokenType.CrossProduct);
    }

    [Fact]
    public void Tokenize_DotProduct_ProducesDotProductToken()
    {
        Tokens("·")[0].Kind.Should().Be(TokenType.DotProduct);
    }

    [Fact]
    public void Tokenize_Partial_ProducesPartialToken()
    {
        Tokens("∂")[0].Kind.Should().Be(TokenType.Partial);
    }

    [Fact]
    public void Tokenize_Nabla_ProducesNablaToken()
    {
        Tokens("∇")[0].Kind.Should().Be(TokenType.Nabla);
    }

    // ─────────────────────────────────────────────
    //  Expressions
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_OnePlusTwo_ProducesThreeTokens()
    {
        var tokens = Tokens("1 + 2");
        tokens.Should().HaveCount(3);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[1].Kind.Should().Be(TokenType.Plus);
        tokens[2].Kind.Should().Be(TokenType.RealLiteral);
    }

    [Fact]
    public void Tokenize_XTimesY_ProducesThreeTokens()
    {
        var tokens = Tokens("x * y");
        tokens.Should().HaveCount(3);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Kind.Should().Be(TokenType.Star);
        tokens[2].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Tokenize_ParenthesizedExpression_ProducesCorrectSequence()
    {
        var tokens = Tokens("(a + b)");
        tokens.Should().HaveCount(5);
        tokens[0].Kind.Should().Be(TokenType.OpenParen);
        tokens[1].Kind.Should().Be(TokenType.Identifier);
        tokens[2].Kind.Should().Be(TokenType.Plus);
        tokens[3].Kind.Should().Be(TokenType.Identifier);
        tokens[4].Kind.Should().Be(TokenType.CloseParen);
    }

    [Fact]
    public void Tokenize_SinXCosY_ProducesCorrectSequence()
    {
        var tokens = Tokens("sin(x) + cos(y)");
        tokens.Should().HaveCount(9);
        tokens[0].Kind.Should().Be(TokenType.FuncSin);
        tokens[1].Kind.Should().Be(TokenType.OpenParen);
        tokens[2].Kind.Should().Be(TokenType.Identifier);
        tokens[3].Kind.Should().Be(TokenType.CloseParen);
        tokens[4].Kind.Should().Be(TokenType.Plus);
        tokens[5].Kind.Should().Be(TokenType.FuncCos);
        tokens[6].Kind.Should().Be(TokenType.OpenParen);
        tokens[7].Kind.Should().Be(TokenType.Identifier);
        tokens[8].Kind.Should().Be(TokenType.CloseParen);
    }

    // ─────────────────────────────────────────────
    //  EOF
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_AlwaysAppendsEofToken()
    {
        var all = Tokenize("42");
        all.Last().Kind.Should().Be(TokenType.Eof);
    }

    [Fact]
    public void Tokenize_EmptySource_ProducesOnlyEof()
    {
        var all = Tokenize("");
        all.Should().HaveCount(1);
        all[0].Kind.Should().Be(TokenType.Eof);
    }

    // ─────────────────────────────────────────────
    //  Position Tracking
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_SingleToken_PositionIsLineOneColumnOne()
    {
        var tokens = Tokens("x");
        tokens[0].Position.Line.Should().Be(1);
        tokens[0].Position.Column.Should().Be(1);
    }

    [Fact]
    public void Tokenize_PaddedToken_ColumnReflectsOffset()
    {
        var tokens = Tokens("  x");
        tokens[0].Position.Column.Should().Be(3);
    }

    [Fact]
    public void Tokenize_TokensOnSameLine_HaveSameLine()
    {
        var tokens = Tokens("a b c");
        tokens.All(t => t.Position.Line == 1).Should().BeTrue();
    }

    [Fact]
    public void Tokenize_TokensOnSameLine_ColumnsAreCorrect()
    {
        var tokens = Tokens("a + b");
        tokens[0].Position.Column.Should().Be(1);
        tokens[1].Position.Column.Should().Be(3);
        tokens[2].Position.Column.Should().Be(5);
    }

    // ─────────────────────────────────────────────
    //  Whitespace Handling
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_DefaultOptions_SkipsWhitespace()
    {
        var tokens = Tokens("1 + 2");
        tokens.Should().HaveCount(3);
        tokens.Should().NotContain(t => t.Kind == TokenType.Whitespace);
    }

    [Fact]
    public void Tokenize_KeepWhitespace_ProducesWhitespaceTokens()
    {
        var opts = new LexerOptions { SkipWhitespace = false };
        var tokens = TokenizeWithOptions("1+2", opts)
            .Where(t => t.Kind != TokenType.Eof).ToArray();
        tokens.Should().NotBeEmpty();
    }

    [Fact]
    public void Tokenize_OnlyWhitespace_ProducesOnlyEof()
    {
        var all = Tokenize("   ");
        all.Should().HaveCount(1);
        all[0].Kind.Should().Be(TokenType.Eof);
    }

    // ─────────────────────────────────────────────
    //  Comments
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_LineComment_SkippedByDefault()
    {
        var tokens = Tokens("x // comment\ny");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Position.Line.Should().Be(2);
    }

    [Fact]
    public void Tokenize_BlockComment_SkippedByDefault()
    {
        var tokens = Tokens("x /* comment */ y");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Tokenize_OnlyLineComment_ProducesOnlyEof()
    {
        var all = Tokenize("// just a comment");
        all.Should().HaveCount(1);
        all[0].Kind.Should().Be(TokenType.Eof);
    }

    [Fact]
    public void Tokenize_OnlyBlockComment_ProducesOnlyEof()
    {
        var all = Tokenize("/* just a comment */");
        all.Should().HaveCount(1);
        all[0].Kind.Should().Be(TokenType.Eof);
    }

    // ─────────────────────────────────────────────
    //  Multi-line
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_MultiLine_TokensOnCorrectLines()
    {
        var tokens = Tokens("x\ny");
        tokens.Should().HaveCount(2);
        tokens[0].Position.Line.Should().Be(1);
        tokens[1].Position.Line.Should().Be(2);
    }

    [Fact]
    public void Tokenize_AfterNewline_ColumnResetsToOne()
    {
        var tokens = Tokens("x\ny");
        tokens[1].Position.Column.Should().Be(1);
    }

    [Fact]
    public void Tokenize_ThreeLines_AllTokensTracked()
    {
        var tokens = Tokens("a\nb\nc");
        tokens.Should().HaveCount(3);
        tokens[0].Position.Line.Should().Be(1);
        tokens[1].Position.Line.Should().Be(2);
        tokens[2].Position.Line.Should().Be(3);
    }

    // ─────────────────────────────────────────────
    //  Complex Expressions
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_PowerExpression()
    {
        var tokens = Tokens("x ^ 2");
        tokens.Should().HaveCount(3);
        tokens[1].Kind.Should().Be(TokenType.Caret);
    }

    [Fact]
    public void Tokenize_CompoundAssignment()
    {
        var tokens = Tokens("x += 1");
        tokens.Should().HaveCount(3);
        tokens[1].Kind.Should().Be(TokenType.PlusEquals);
    }

    [Fact]
    public void Tokenize_ComparisonExpression()
    {
        var tokens = Tokens("x <= y");
        tokens.Should().HaveCount(3);
        tokens[1].Kind.Should().Be(TokenType.LessThanOrEqual);
    }

    [Fact]
    public void Tokenize_LogicalExpression()
    {
        var tokens = Tokens("a && b || c");
        tokens.Should().HaveCount(5);
        tokens[1].Kind.Should().Be(TokenType.AmpersandAmpersand);
        tokens[3].Kind.Should().Be(TokenType.PipePipe);
    }

    [Fact]
    public void Tokenize_NestedFunctionCall()
    {
        var tokens = Tokens("sin(cos(x))");
        tokens.Should().HaveCount(7);
        tokens[0].Kind.Should().Be(TokenType.FuncSin);
        tokens[1].Kind.Should().Be(TokenType.OpenParen);
        tokens[2].Kind.Should().Be(TokenType.FuncCos);
        tokens[3].Kind.Should().Be(TokenType.OpenParen);
        tokens[4].Kind.Should().Be(TokenType.Identifier);
        tokens[5].Kind.Should().Be(TokenType.CloseParen);
        tokens[6].Kind.Should().Be(TokenType.CloseParen);
    }

    // ─────────────────────────────────────────────
    //  Invalid / Unknown
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_UnknownUnicode_ProducesUnknownToken()
    {
        var tokens = Tokens("★");
        tokens[0].Kind.Should().Be(TokenType.Unknown);
    }

    // ─────────────────────────────────────────────
    //  Greek Letters
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_GreekAlpha_ProducesIdentifier()
    {
        Tokens("α")[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Tokenize_GreekPi_ProducesIdentifier()
    {
        Tokens("π")[0].Kind.Should().Be(TokenType.Identifier);
    }

    // ─────────────────────────────────────────────
    //  Hex (Not Supported)
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_HexNotation_ProducesIntegerThenIdentifier()
    {
        var tokens = Tokens("0xFF");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[1].Kind.Should().Be(TokenType.Identifier);
    }

    // ─────────────────────────────────────────────
    //  Semicolons
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_SemicolonSeparated_ProducesSemicolonToken()
    {
        var tokens = Tokens("x; y");
        tokens.Should().HaveCount(3);
        tokens[1].Kind.Should().Be(TokenType.Semicolon);
    }

    // ─────────────────────────────────────────────
    //  Parentheses
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_NestedParentheses_AllDelimitersParsed()
    {
        var tokens = Tokens("((x))");
        tokens.Should().HaveCount(5);
        tokens[0].Kind.Should().Be(TokenType.OpenParen);
        tokens[1].Kind.Should().Be(TokenType.OpenParen);
        tokens[2].Kind.Should().Be(TokenType.Identifier);
        tokens[3].Kind.Should().Be(TokenType.CloseParen);
        tokens[4].Kind.Should().Be(TokenType.CloseParen);
    }

    [Fact]
    public void Tokenize_EmptyParentheses_ProducesOpenAndClose()
    {
        var tokens = Tokens("()");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.OpenParen);
        tokens[1].Kind.Should().Be(TokenType.CloseParen);
    }

    // ─────────────────────────────────────────────
    //  Unicode Mixed with ASCII
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_UnicodeSummationExpression()
    {
        var tokens = Tokens("x + ∑");
        tokens.Should().HaveCount(3);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Kind.Should().Be(TokenType.Plus);
        tokens[2].Kind.Should().Be(TokenType.Summation);
    }

    // ─────────────────────────────────────────────
    //  Token Lexeme
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_NumberToken_LexemeMatchesSource()
    {
        var tokens = Tokens("42");
        tokens[0].Lexeme.Should().Be("42");
    }

    [Fact]
    public void Tokenize_OperatorToken_LexemeMatchesSource()
    {
        var tokens = Tokens("+");
        tokens[0].Lexeme.Should().Be("+");
    }

    [Fact]
    public void Tokenize_IdentifierToken_LexemeMatchesSource()
    {
        var tokens = Tokens("foo");
        tokens[0].Lexeme.Should().Be("foo");
    }

    [Fact]
    public void Tokenize_EofToken_HasEmptyLexeme()
    {
        var all = Tokenize("x");
        all.Last().Lexeme.Should().BeEmpty();
    }

    [Fact]
    public void Tokenize_EofToken_HasZeroLength()
    {
        var all = Tokenize("x");
        all.Last().Length.Should().Be(0);
    }

    // ─────────────────────────────────────────────
    //  Token Length
    // ─────────────────────────────────────────────

    [Fact]
    public void Tokenize_SingleCharToken_HasLengthOne()
    {
        Tokens("+")[0].Length.Should().Be(1);
    }

    [Fact]
    public void Tokenize_MultiCharNumber_HasCorrectLength()
    {
        Tokens("42")[0].Length.Should().Be(2);
    }

    [Fact]
    public void Tokenize_FourCharIdentifier_HasLengthFour()
    {
        Tokens("sqrt")[0].Length.Should().Be(4);
    }

    [Fact]
    public void Tokenize_UnicodeToken_HasLengthOne()
    {
        Tokens("∑")[0].Length.Should().Be(1);
    }
}
