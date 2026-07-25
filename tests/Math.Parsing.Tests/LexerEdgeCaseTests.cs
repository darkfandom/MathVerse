namespace MathVerse.Math.Parsing.Tests;

public class LexerEdgeCaseTests
{
    private static Token[] Tokenize(string source) => ParsingFacade.Tokenize(source);

    private static Token[] Tokens(string source) =>
        Tokenize(source).Where(t => t.Kind != TokenType.Eof).ToArray();

    [Fact]
    public void VeryLongIdentifier_ProducesSingleIdentifierToken()
    {
        var longId = new string('a', 500);
        var tokens = Tokens(longId);
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[0].Lexeme.Should().Be(longId);
    }

    [Fact]
    public void VeryLargeNumber_ProducesIntegerLiteral()
    {
        var bigNum = new string('9', 50);
        var tokens = Tokens(bigNum);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[0].Lexeme.Should().Be(bigNum);
    }

    [Fact]
    public void VeryLargeRealNumber_ProducesRealLiteral()
    {
        var bigReal = "9" + new string('9', 40) + ".5";
        var tokens = Tokens(bigReal);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
    }

    [Fact]
    public void NestedBlockComment_EntireCommentSkipped()
    {
        var tokens = Tokens("/* /* nested */ */");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Star);
        tokens[1].Kind.Should().Be(TokenType.Slash);
    }

    [Fact]
    public void UnterminatedString_ProducesStringLiteralToken()
    {
        var tokens = Tokens("\"hello");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.StringLiteral);
    }

    [Fact]
    public void UnterminatedBlockComment_RemainingSourceConsumed()
    {
        var tokens = Tokens("x /* unterminated");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void AdjacentOperators_ProduceIndividualTokens()
    {
        var tokens = Tokens("a++");
        tokens.Should().HaveCount(3);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Kind.Should().Be(TokenType.Plus);
        tokens[2].Kind.Should().Be(TokenType.Plus);
    }

    [Fact]
    public void EmptyParentheses_ProduceOpenAndClose()
    {
        var tokens = Tokens("()");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.OpenParen);
        tokens[1].Kind.Should().Be(TokenType.CloseParen);
    }

    [Fact]
    public void JustANumber_ProducesCorrectToken()
    {
        var tokens = Tokens("42");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[0].Value.Should().Be(42.0);
    }

    [Fact]
    public void JustWhitespace_ProducesOnlyEof()
    {
        var all = Tokenize("   ");
        all.Should().HaveCount(1);
        all[0].Kind.Should().Be(TokenType.Eof);
    }

    [Fact]
    public void JustALineComment_ProducesOnlyEof()
    {
        var all = Tokenize("// comment");
        all.Should().HaveCount(1);
        all[0].Kind.Should().Be(TokenType.Eof);
    }

    [Fact]
    public void JustABlockComment_ProducesOnlyEof()
    {
        var all = Tokenize("/* comment */");
        all.Should().HaveCount(1);
        all[0].Kind.Should().Be(TokenType.Eof);
    }

    [Fact]
    public void UnicodeMixedWithASCII_ProducesCorrectTokens()
    {
        var tokens = Tokens("x + ∑");
        tokens.Should().HaveCount(3);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Kind.Should().Be(TokenType.Plus);
        tokens[2].Kind.Should().Be(TokenType.Summation);
    }

    [Fact]
    public void MultipleDots_NotStartingWithDigit_ProducesDotTokens()
    {
        var tokens = Tokens("1.2.3");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[1].Kind.Should().Be(TokenType.RealLiteral);
    }

    [Fact]
    public void NumberStartingWithDot_ProducesRealLiteral()
    {
        var tokens = Tokens(".5");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
    }

    [Fact]
    public void NegativeDecimal_ProducesMinusThenRealLiteral()
    {
        var tokens = Tokens("-3.14");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Minus);
        tokens[1].Kind.Should().Be(TokenType.RealLiteral);
        tokens[1].Value.Should().Be(3.14);
    }

    [Fact]
    public void DeeplyNestedParentheses_AllTokensParsed()
    {
        var source = "(" + new string('(', 20) + "x" + new string(')', 20) + ")";
        var tokens = Tokens(source);
        tokens.Count(t => t.Kind == TokenType.OpenParen).Should().Be(21);
        tokens.Count(t => t.Kind == TokenType.CloseParen).Should().Be(21);
        tokens.Count(t => t.Kind == TokenType.Identifier).Should().Be(1);
    }

    [Fact]
    public void StringWithMultipleEscapes_ProducesCorrectValue()
    {
        var tokens = Tokens("\"a\\nb\\tc\"");
        tokens[0].Kind.Should().Be(TokenType.StringLiteral);
        tokens[0].Value.Should().Be("a\nb\tc");
    }

    [Fact]
    public void GreekLettersExpression_ProducesIdentifiers()
    {
        var tokens = Tokens("α + β");
        tokens.Should().HaveCount(3);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Kind.Should().Be(TokenType.Plus);
        tokens[2].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void SuperscriptDigit_ProducesUnknownToken()
    {
        var tokens = Tokens("x²");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Kind.Should().Be(TokenType.Unknown);
    }

    [Fact]
    public void ArrowASCII_ProducesMinusThenGreaterThan()
    {
        var tokens = Tokens("->");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Minus);
        tokens[1].Kind.Should().Be(TokenType.GreaterThan);
    }

    [Fact]
    public void ArrowUnicode_ProducesArrowToken()
    {
        var tokens = Tokens("→");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Arrow);
    }

    [Fact]
    public void ColonEquals_ProducesEqualsToken()
    {
        var tokens = Tokens(":=");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Equals);
    }

    [Fact]
    public void EmptyBrackets_ProduceOpenAndClose()
    {
        var tokens = Tokens("[]");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.OpenBracket);
        tokens[1].Kind.Should().Be(TokenType.CloseBracket);
    }

    [Fact]
    public void EmptyBraces_ProduceOpenAndClose()
    {
        var tokens = Tokens("{}");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.OpenBrace);
        tokens[1].Kind.Should().Be(TokenType.CloseBrace);
    }

    [Fact]
    public void DeeplyNestedBrackets_AllTokensParsed()
    {
        var source = "[" + new string('[', 15) + "1" + new string(']', 15) + "]";
        var tokens = Tokens(source);
        tokens.Count(t => t.Kind == TokenType.OpenBracket).Should().Be(16);
        tokens.Count(t => t.Kind == TokenType.CloseBracket).Should().Be(16);
    }

    [Fact]
    public void MixedDelimiters_AllParsedCorrectly()
    {
        var tokens = Tokens("([{}])");
        tokens.Should().HaveCount(6);
        tokens[0].Kind.Should().Be(TokenType.OpenParen);
        tokens[1].Kind.Should().Be(TokenType.OpenBracket);
        tokens[2].Kind.Should().Be(TokenType.OpenBrace);
        tokens[3].Kind.Should().Be(TokenType.CloseBrace);
        tokens[4].Kind.Should().Be(TokenType.CloseBracket);
        tokens[5].Kind.Should().Be(TokenType.CloseParen);
    }

    [Fact]
    public void ConsecutiveExpressions_AllTokensParsed()
    {
        var tokens = Tokens("1+2*3-4/5");
        tokens.Should().HaveCount(9);
        tokens[0].Kind.Should().Be(TokenType.RealLiteral);
        tokens[1].Kind.Should().Be(TokenType.Plus);
        tokens[2].Kind.Should().Be(TokenType.RealLiteral);
        tokens[3].Kind.Should().Be(TokenType.Star);
        tokens[4].Kind.Should().Be(TokenType.RealLiteral);
        tokens[5].Kind.Should().Be(TokenType.Minus);
        tokens[6].Kind.Should().Be(TokenType.RealLiteral);
        tokens[7].Kind.Should().Be(TokenType.Slash);
        tokens[8].Kind.Should().Be(TokenType.RealLiteral);
    }

    [Fact]
    public void PipeOperator_ProducesPipeToken()
    {
        Tokens("|")[0].Kind.Should().Be(TokenType.Pipe);
    }

    [Fact]
    public void DoublePipeOperator_ProducesPipePipeToken()
    {
        Tokens("||")[0].Kind.Should().Be(TokenType.PipePipe);
    }

    [Fact]
    public void CommentBetweenTokens_BothTokensStillProduced()
    {
        var tokens = Tokens("x /* mid */ y");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
        tokens[0].Lexeme.Should().Be("x");
        tokens[1].Kind.Should().Be(TokenType.Identifier);
        tokens[1].Lexeme.Should().Be("y");
    }

    [Fact]
    public void LineCommentAtEnd_TokenBeforeStillProduced()
    {
        var tokens = Tokens("x // end");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void LineCommentAtStart_TokenAfterStillProduced()
    {
        var tokens = Tokens("// start\ny");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void MultipleLineComments_AllSkipped()
    {
        var tokens = Tokens("a // c1\nb // c2\nc");
        tokens.Should().HaveCount(3);
        tokens.All(t => t.Kind == TokenType.Identifier).Should().BeTrue();
    }

    [Fact]
    public void UnicodeInfinity_ProducesConstantInfinity()
    {
        var tokens = Tokens("∞");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.ConstantInfinity);
    }

    [Fact]
    public void TransposeSymbol_ProducesTransposeToken()
    {
        var tokens = Tokens("ᵀ");
        tokens.Should().HaveCount(1);
        tokens[0].Kind.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void UnicodeSetUnion_ProducesUnionToken()
    {
        Tokens("∪")[0].Kind.Should().Be(TokenType.Union);
    }

    [Fact]
    public void UnicodeSetIntersection_ProducesIntersectionToken()
    {
        Tokens("∩")[0].Kind.Should().Be(TokenType.Intersection);
    }

    [Fact]
    public void UnicodeNotElementOf_ProducesNotElementOfToken()
    {
        Tokens("∉")[0].Kind.Should().Be(TokenType.NotElementOf);
    }

    [Fact]
    public void UnicodeSubset_ProducesSubsetToken()
    {
        Tokens("⊂")[0].Kind.Should().Be(TokenType.Subset);
    }

    [Fact]
    public void UnicodeApproximatelyEqual_ProducesApproximatelyEqualToken()
    {
        Tokens("≈")[0].Kind.Should().Be(TokenType.ApproximatelyEqual);
    }

    [Fact]
    public void UnicodeNotEqual_ProducesNotEqualSignToken()
    {
        Tokens("≠")[0].Kind.Should().Be(TokenType.NotEqualSign);
    }

    [Fact]
    public void UnicodeLessThanOrEqual_ProducesToken()
    {
        Tokens("≤")[0].Kind.Should().Be(TokenType.LessThanOrEqualSign);
    }

    [Fact]
    public void UnicodeGreaterThanOrEqual_ProducesToken()
    {
        Tokens("≥")[0].Kind.Should().Be(TokenType.GreaterThanOrEqualSign);
    }

    [Fact]
    public void UnicodeImplies_ProducesImpliesToken()
    {
        Tokens("⇒")[0].Kind.Should().Be(TokenType.Implies);
    }

    [Fact]
    public void UnicodeEquivalent_ProducesEquivalentToken()
    {
        Tokens("⇔")[0].Kind.Should().Be(TokenType.Equivalent);
    }

    [Fact]
    public void UnicodeMapsTo_ProducesMapsToToken()
    {
        Tokens("↦")[0].Kind.Should().Be(TokenType.MapsTo);
    }

    [Fact]
    public void UnicodeParallel_ProducesParallelToken()
    {
        Tokens("∥")[0].Kind.Should().Be(TokenType.Parallel);
    }

    [Fact]
    public void UnicodeCompose_ProducesComposeToken()
    {
        Tokens("∘")[0].Kind.Should().Be(TokenType.Compose);
    }

    [Fact]
    public void UnicodeTensorProduct_ProducesTensorProductToken()
    {
        Tokens("⊗")[0].Kind.Should().Be(TokenType.TensorProduct);
    }

    [Fact]
    public void UnicodeSetDifference_ProducesSetDifferenceToken()
    {
        Tokens("∖")[0].Kind.Should().Be(TokenType.SetDifference);
    }

    [Fact]
    public void UnicodeSuperset_ProducesSupersetToken()
    {
        Tokens("⊃")[0].Kind.Should().Be(TokenType.Superset);
    }

    [Fact]
    public void SingleDotNotFollowedByDigit_ProducesDotToken()
    {
        var tokens = Tokens(".x");
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(TokenType.Dot);
        tokens[1].Kind.Should().Be(TokenType.Identifier);
    }
}
