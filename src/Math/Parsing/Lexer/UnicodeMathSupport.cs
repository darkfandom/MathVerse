namespace MathVerse.Math.Parsing.Lexer;

/// <summary>
/// Provides Unicode mathematical symbol detection and mapping.
/// </summary>
public static class UnicodeMathSupport
{
    private static readonly Dictionary<char, TokenType> SingleCharSymbols = new()
    {
        ['∑'] = TokenType.Summation,
        ['∏'] = TokenType.Product,
        ['∫'] = TokenType.Integral,
        ['∂'] = TokenType.Partial,
        ['∇'] = TokenType.Nabla,
        ['∧'] = TokenType.Wedge,
        ['∨'] = TokenType.Vee,
        ['¬'] = TokenType.Negation,
        ['⇒'] = TokenType.Implies,
        ['⇔'] = TokenType.Equivalent,
        ['∈'] = TokenType.ElementOf,
        ['∉'] = TokenType.NotElementOf,
        ['⊂'] = TokenType.Subset,
        ['⊃'] = TokenType.Superset,
        ['∪'] = TokenType.Union,
        ['∩'] = TokenType.Intersection,
        ['∖'] = TokenType.SetDifference,
        ['×'] = TokenType.CrossProduct,
        ['·'] = TokenType.DotProduct,
        ['∘'] = TokenType.Compose,
        ['⊗'] = TokenType.TensorProduct,
        ['→'] = TokenType.Arrow,
        ['↦'] = TokenType.MapsTo,
        ['∥'] = TokenType.Parallel,
        ['≠'] = TokenType.NotEqualSign,
        ['≤'] = TokenType.LessThanOrEqualSign,
        ['≥'] = TokenType.GreaterThanOrEqualSign,
        ['≈'] = TokenType.ApproximatelyEqual,
    };

    private static readonly Dictionary<string, TokenType> MultiCharSymbols = new(StringComparer.Ordinal)
    {
        ["ᵀ"] = TokenType.Transpose,
        ["⁻¹"] = TokenType.Inverse,
        ["∞"] = TokenType.ConstantInfinity,
    };

    private static readonly Dictionary<char, int> SuperScriptDigits = new()
    {
        ['⁰'] = 0, ['¹'] = 1, ['²'] = 2, ['³'] = 3, ['⁴'] = 4,
        ['⁵'] = 5, ['⁶'] = 6, ['⁷'] = 7, ['⁸'] = 8, ['⁹'] = 9,
    };

    private static readonly Dictionary<char, int> SubScriptDigits = new()
    {
        ['₀'] = 0, ['₁'] = 1, ['₂'] = 2, ['₃'] = 3, ['₄'] = 4,
        ['₅'] = 5, ['₆'] = 6, ['₇'] = 7, ['₈'] = 8, ['₉'] = 9,
    };

    private static readonly HashSet<char> GreekLetters =
    [
        'α', 'β', 'γ', 'δ', 'ε', 'ζ', 'η', 'θ', 'ι', 'κ', 'λ', 'μ',
        'ν', 'ξ', 'ο', 'π', 'ρ', 'σ', 'τ', 'υ', 'φ', 'χ', 'ψ', 'ω',
        'Γ', 'Δ', 'Θ', 'Λ', 'Ξ', 'Π', 'Σ', 'Φ', 'Ψ', 'Ω',
    ];

    /// <summary>Tries to map a single Unicode character to a token type.</summary>
    public static bool TryGetSymbolTokenType(char c, out TokenType tokenType)
    {
        return SingleCharSymbols.TryGetValue(c, out tokenType);
    }

    /// <summary>Tries to map a multi-character Unicode symbol to a token type.</summary>
    public static bool TryGetMultiCharSymbol(string text, out TokenType tokenType, out int length)
    {
        foreach (var kvp in MultiCharSymbols)
        {
            if (text.StartsWith(kvp.Key, StringComparison.Ordinal))
            {
                tokenType = kvp.Value;
                length = kvp.Key.Length;
                return true;
            }
        }
        tokenType = TokenType.Unknown;
        length = 0;
        return false;
    }

    /// <summary>Checks if a character is a Unicode mathematical symbol.</summary>
    public static bool IsMathSymbol(char c) =>
        SingleCharSymbols.ContainsKey(c) || MultiCharSymbols.ContainsKey(c.ToString());

    /// <summary>Checks if a character is a Greek letter.</summary>
    public static bool IsGreekLetter(char c) => GreekLetters.Contains(c);

    /// <summary>Checks if a character is a Unicode superscript digit.</summary>
    public static bool IsSuperScriptDigit(char c) => SuperScriptDigits.ContainsKey(c);

    /// <summary>Checks if a character is a Unicode subscript digit.</summary>
    public static bool IsSubScriptDigit(char c) => SubScriptDigits.ContainsKey(c);

    /// <summary>Tries to get the integer value of a superscript digit.</summary>
    public static bool TryGetSuperScriptValue(char c, out int value) =>
        SuperScriptDigits.TryGetValue(c, out value);

    /// <summary>Tries to get the integer value of a subscript digit.</summary>
    public static bool TryGetSubScriptValue(char c, out int value) =>
        SubScriptDigits.TryGetValue(c, out value);

    /// <summary>Checks if a character is a Unicode math identifier character (letter or math symbol).</summary>
    public static bool IsIdentifierChar(char c) =>
        char.IsLetter(c) || c == '_' || IsGreekLetter(c) || c > '\u2000';

    /// <summary>Checks if a character starts a number literal.</summary>
    public static bool IsDigitStart(char c) => char.IsDigit(c) || c == '.';

    /// <summary>Checks if a character is a Unicode whitespace character.</summary>
    public static bool IsUnicodeWhitespace(char c) =>
        c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\v' || c == '\f' ||
        c == '\u00A0' || c == '\u2000' || c == '\u2001' || c == '\u2002' ||
        c == '\u2003' || c == '\u2004' || c == '\u2005' || c == '\u2006' ||
        c == '\u2007' || c == '\u2008' || c == '\u2009' || c == '\u200A' ||
        c == '\u202F' || c == '\u205F' || c == '\u3000';
}
