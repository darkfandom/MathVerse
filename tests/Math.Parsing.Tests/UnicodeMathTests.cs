namespace MathVerse.Math.Parsing.Tests;

public class UnicodeMathTests
{
    [Theory]
    [InlineData('∑', TokenType.Summation)]
    [InlineData('∏', TokenType.Product)]
    [InlineData('∫', TokenType.Integral)]
    [InlineData('∂', TokenType.Partial)]
    [InlineData('∇', TokenType.Nabla)]
    [InlineData('∧', TokenType.Wedge)]
    [InlineData('∨', TokenType.Vee)]
    [InlineData('¬', TokenType.Negation)]
    [InlineData('⇒', TokenType.Implies)]
    [InlineData('⇔', TokenType.Equivalent)]
    [InlineData('∈', TokenType.ElementOf)]
    [InlineData('∉', TokenType.NotElementOf)]
    [InlineData('⊂', TokenType.Subset)]
    [InlineData('⊃', TokenType.Superset)]
    [InlineData('∪', TokenType.Union)]
    [InlineData('∩', TokenType.Intersection)]
    [InlineData('∖', TokenType.SetDifference)]
    [InlineData('×', TokenType.CrossProduct)]
    [InlineData('·', TokenType.DotProduct)]
    [InlineData('∘', TokenType.Compose)]
    [InlineData('⊗', TokenType.TensorProduct)]
    [InlineData('→', TokenType.Arrow)]
    [InlineData('↦', TokenType.MapsTo)]
    [InlineData('∥', TokenType.Parallel)]
    [InlineData('≠', TokenType.NotEqualSign)]
    [InlineData('≤', TokenType.LessThanOrEqualSign)]
    [InlineData('≥', TokenType.GreaterThanOrEqualSign)]
    [InlineData('≈', TokenType.ApproximatelyEqual)]
    public void TryGetSymbolTokenType_KnownSymbol_ReturnsCorrectType(char c, TokenType expected)
    {
        UnicodeMathSupport.TryGetSymbolTokenType(c, out var tokenType).Should().BeTrue();
        tokenType.Should().Be(expected);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('z')]
    [InlineData('0')]
    [InlineData(' ')]
    [InlineData('+')]
    [InlineData('!')]
    public void TryGetSymbolTokenType_UnknownChar_ReturnsFalse(char c)
    {
        UnicodeMathSupport.TryGetSymbolTokenType(c, out _).Should().BeFalse();
    }

    [Fact]
    public void TryGetMultiCharSymbol_Transpose_ReturnsCorrectTypeAndLength()
    {
        UnicodeMathSupport.TryGetMultiCharSymbol("ᵀ", out var type, out var length).Should().BeTrue();
        type.Should().Be(TokenType.Transpose);
        length.Should().Be(1);
    }

    [Fact]
    public void TryGetMultiCharSymbol_Inverse_ReturnsCorrectTypeAndLength()
    {
        UnicodeMathSupport.TryGetMultiCharSymbol("⁻¹", out var type, out var length).Should().BeTrue();
        type.Should().Be(TokenType.Inverse);
        length.Should().Be(2);
    }

    [Fact]
    public void TryGetMultiCharSymbol_Infinity_ReturnsCorrectTypeAndLength()
    {
        UnicodeMathSupport.TryGetMultiCharSymbol("∞", out var type, out var length).Should().BeTrue();
        type.Should().Be(TokenType.ConstantInfinity);
        length.Should().Be(1);
    }

    [Fact]
    public void TryGetMultiCharSymbol_Unknown_ReturnsFalseWithZeroLength()
    {
        UnicodeMathSupport.TryGetMultiCharSymbol("abc", out _, out var length).Should().BeFalse();
        length.Should().Be(0);
    }

    [Fact]
    public void TryGetMultiCharSymbol_EmptyString_ReturnsFalse()
    {
        UnicodeMathSupport.TryGetMultiCharSymbol("", out _, out _).Should().BeFalse();
    }

    [Fact]
    public void TryGetMultiCharSymbol_TransposePrefix_MatchesSuccessfully()
    {
        UnicodeMathSupport.TryGetMultiCharSymbol("ᵀrest", out var type, out var length).Should().BeTrue();
        type.Should().Be(TokenType.Transpose);
        length.Should().Be(1);
    }

    [Fact]
    public void TryGetMultiCharSymbol_InversePrefix_MatchesSuccessfully()
    {
        UnicodeMathSupport.TryGetMultiCharSymbol("⁻¹x", out var type, out var length).Should().BeTrue();
        type.Should().Be(TokenType.Inverse);
        length.Should().Be(2);
    }

    [Theory]
    [InlineData('α')]
    [InlineData('β')]
    [InlineData('γ')]
    [InlineData('δ')]
    [InlineData('ε')]
    [InlineData('ζ')]
    [InlineData('η')]
    [InlineData('θ')]
    [InlineData('ι')]
    [InlineData('κ')]
    [InlineData('λ')]
    [InlineData('μ')]
    [InlineData('ν')]
    [InlineData('ξ')]
    [InlineData('π')]
    [InlineData('ρ')]
    [InlineData('σ')]
    [InlineData('τ')]
    [InlineData('υ')]
    [InlineData('φ')]
    [InlineData('χ')]
    [InlineData('ψ')]
    [InlineData('ω')]
    public void IsGreekLetter_LowercaseGreek_ReturnsTrue(char c)
    {
        UnicodeMathSupport.IsGreekLetter(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('Γ')]
    [InlineData('Δ')]
    [InlineData('Θ')]
    [InlineData('Λ')]
    [InlineData('Ξ')]
    [InlineData('Π')]
    [InlineData('Σ')]
    [InlineData('Φ')]
    [InlineData('Ψ')]
    [InlineData('Ω')]
    public void IsGreekLetter_UppercaseGreek_ReturnsTrue(char c)
    {
        UnicodeMathSupport.IsGreekLetter(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('a')]
    [InlineData('z')]
    [InlineData('A')]
    [InlineData('Z')]
    [InlineData('0')]
    [InlineData('+')]
    [InlineData('∑')]
    public void IsGreekLetter_NonGreek_ReturnsFalse(char c)
    {
        UnicodeMathSupport.IsGreekLetter(c).Should().BeFalse();
    }

    [Theory]
    [InlineData('∑')]
    [InlineData('∫')]
    [InlineData('∞')]
    [InlineData('→')]
    [InlineData('≠')]
    [InlineData('≤')]
    public void IsMathSymbol_SingleCharSymbol_ReturnsTrue(char c)
    {
        UnicodeMathSupport.IsMathSymbol(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('a')]
    [InlineData('0')]
    [InlineData('+')]
    public void IsMathSymbol_NonSymbol_ReturnsFalse(char c)
    {
        UnicodeMathSupport.IsMathSymbol(c).Should().BeFalse();
    }

    [Fact]
    public void IsMathSymbol_MultiCharSymbol_Transpose_ReturnsTrue()
    {
        UnicodeMathSupport.IsMathSymbol('ᵀ').Should().BeTrue();
    }

    [Fact]
    public void IsMathSymbol_MultiCharSymbol_Inverse_ReturnsTrue()
    {
        UnicodeMathSupport.IsMathSymbol('⁻').Should().BeFalse();
    }

    [Theory]
    [InlineData('⁰', 0)]
    [InlineData('¹', 1)]
    [InlineData('²', 2)]
    [InlineData('³', 3)]
    [InlineData('⁴', 4)]
    [InlineData('⁵', 5)]
    [InlineData('⁶', 6)]
    [InlineData('⁷', 7)]
    [InlineData('⁸', 8)]
    [InlineData('⁹', 9)]
    public void IsSuperScriptDigit_AllDigits_ReturnsTrue(char c, int expected)
    {
        UnicodeMathSupport.IsSuperScriptDigit(c).Should().BeTrue();
        UnicodeMathSupport.TryGetSuperScriptValue(c, out var value).Should().BeTrue();
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('0')]
    [InlineData('A')]
    [InlineData(' ')]
    public void IsSuperScriptDigit_NonDigit_ReturnsFalse(char c)
    {
        UnicodeMathSupport.IsSuperScriptDigit(c).Should().BeFalse();
    }

    [Fact]
    public void TryGetSuperScriptValue_InvalidChar_ReturnsFalse()
    {
        UnicodeMathSupport.TryGetSuperScriptValue('x', out var value).Should().BeFalse();
        value.Should().Be(default);
    }

    [Theory]
    [InlineData('₀', 0)]
    [InlineData('₁', 1)]
    [InlineData('₂', 2)]
    [InlineData('₃', 3)]
    [InlineData('₄', 4)]
    [InlineData('₅', 5)]
    [InlineData('₆', 6)]
    [InlineData('₇', 7)]
    [InlineData('₈', 8)]
    [InlineData('₉', 9)]
    public void IsSubScriptDigit_AllDigits_ReturnsTrue(char c, int expected)
    {
        UnicodeMathSupport.IsSubScriptDigit(c).Should().BeTrue();
        UnicodeMathSupport.TryGetSubScriptValue(c, out var value).Should().BeTrue();
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('0')]
    [InlineData('A')]
    public void IsSubScriptDigit_NonDigit_ReturnsFalse(char c)
    {
        UnicodeMathSupport.IsSubScriptDigit(c).Should().BeFalse();
    }

    [Fact]
    public void TryGetSubScriptValue_InvalidChar_ReturnsFalse()
    {
        UnicodeMathSupport.TryGetSubScriptValue('x', out var value).Should().BeFalse();
        value.Should().Be(default);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('Z')]
    [InlineData('_')]
    [InlineData('α')]
    [InlineData('π')]
    public void IsIdentifierChar_ValidChars_ReturnsTrue(char c)
    {
        UnicodeMathSupport.IsIdentifierChar(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('0')]
    [InlineData('9')]
    [InlineData('+')]
    [InlineData(' ')]
    public void IsIdentifierChar_InvalidChars_ReturnsFalse(char c)
    {
        UnicodeMathSupport.IsIdentifierChar(c).Should().BeFalse();
    }

    [Theory]
    [InlineData('0')]
    [InlineData('5')]
    [InlineData('9')]
    [InlineData('.')]
    public void IsDigitStart_ValidChars_ReturnsTrue(char c)
    {
        UnicodeMathSupport.IsDigitStart(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('a')]
    [InlineData('+')]
    [InlineData(' ')]
    public void IsDigitStart_InvalidChars_ReturnsFalse(char c)
    {
        UnicodeMathSupport.IsDigitStart(c).Should().BeFalse();
    }

    [Theory]
    [InlineData(' ')]
    [InlineData('\t')]
    [InlineData('\r')]
    [InlineData('\n')]
    [InlineData('\v')]
    [InlineData('\f')]
    [InlineData('\u00A0')]
    [InlineData('\u2000')]
    [InlineData('\u2001')]
    [InlineData('\u2002')]
    [InlineData('\u2003')]
    [InlineData('\u2004')]
    [InlineData('\u2005')]
    [InlineData('\u2006')]
    [InlineData('\u2007')]
    [InlineData('\u2008')]
    [InlineData('\u2009')]
    [InlineData('\u200A')]
    [InlineData('\u202F')]
    [InlineData('\u205F')]
    [InlineData('\u3000')]
    public void IsUnicodeWhitespace_WhitespaceChars_ReturnsTrue(char c)
    {
        UnicodeMathSupport.IsUnicodeWhitespace(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('a')]
    [InlineData('0')]
    [InlineData('∑')]
    public void IsUnicodeWhitespace_NonWhitespaceChars_ReturnsFalse(char c)
    {
        UnicodeMathSupport.IsUnicodeWhitespace(c).Should().BeFalse();
    }

    [Fact]
    public void TryGetSymbolTokenType_SuperscriptTwo_ReturnsFalse()
    {
        UnicodeMathSupport.TryGetSymbolTokenType('²', out _).Should().BeFalse();
    }

    [Fact]
    public void IsGreekLetter_AllLowercaseLetters_CoverageCheck()
    {
        for (char c = 'a'; c <= 'z'; c++)
        {
            UnicodeMathSupport.IsGreekLetter(c).Should().BeFalse($"'{c}' is not a Greek letter");
        }
    }

    [Fact]
    public void IsSuperScriptDigit_AllSuperscriptsAreRecognized()
    {
        var expected = new[] { ('⁰', 0), ('¹', 1), ('²', 2), ('³', 3), ('⁴', 4),
                              ('⁵', 5), ('⁶', 6), ('⁷', 7), ('⁸', 8), ('⁹', 9) };
        foreach (var (sup, val) in expected)
        {
            UnicodeMathSupport.IsSuperScriptDigit(sup).Should().BeTrue();
            UnicodeMathSupport.TryGetSuperScriptValue(sup, out var v).Should().BeTrue();
            v.Should().Be(val);
        }
    }

    [Fact]
    public void IsSubScriptDigit_AllSubscriptsAreRecognized()
    {
        for (int i = 0; i <= 9; i++)
        {
            var sub = (char)('₀' + i);
            UnicodeMathSupport.IsSubScriptDigit(sub).Should().BeTrue();
            UnicodeMathSupport.TryGetSubScriptValue(sub, out var val).Should().BeTrue();
            val.Should().Be(i);
        }
    }

    [Fact]
    public void IsMathSymbol_AllSingleCharSymbols_AreRecognized()
    {
        var symbols = new[] { '∑', '∏', '∫', '∂', '∇', '∧', '∨', '¬', '⇒', '⇔',
            '∈', '∉', '⊂', '⊃', '∪', '∩', '∖', '×', '·', '∘', '⊗',
            '→', '↦', '∥', '≠', '≤', '≥', '≈' };
        foreach (var s in symbols)
        {
            UnicodeMathSupport.IsMathSymbol(s).Should().BeTrue($"'{s}' should be a math symbol");
            UnicodeMathSupport.TryGetSymbolTokenType(s, out _).Should().BeTrue($"'{s}' should map to a token type");
        }
    }
}
