namespace MathVerse.Math.Parsing.Lexer;

/// <summary>
/// Represents a single token produced by the lexer.
/// </summary>
public sealed record Token
{
    /// <summary>Initializes a token.</summary>
    public Token(TokenType kind, string lexeme, TokenPosition position, int length, object? value = null)
    {
        Kind = kind;
        Lexeme = lexeme;
        Position = position;
        Length = length;
        Value = value;
    }

    /// <summary>Gets the token type.</summary>
    public TokenType Kind { get; }

    /// <summary>Gets the raw text of the token.</summary>
    public string Lexeme { get; }

    /// <summary>Gets the position in source where this token starts.</summary>
    public TokenPosition Position { get; }

    /// <summary>Gets the length of this token in characters.</summary>
    public int Length { get; }

    /// <summary>Gets the parsed value (for literals).</summary>
    public object? Value { get; }

    /// <summary>Gets the 1-based end line.</summary>
    public int EndLine => Position.Line;

    /// <summary>Gets the 1-based end column.</summary>
    public int EndColumn => Position.Column + Length;

    /// <summary>The EOF token.</summary>
    public static Token Eof(TokenPosition position) =>
        new(TokenType.Eof, string.Empty, position, 0);

    /// <summary>Creates an unknown token.</summary>
    public static Token Unknown(string lexeme, TokenPosition position) =>
        new(TokenType.Unknown, lexeme, position, lexeme.Length);

    /// <inheritdoc/>
    public override string ToString() =>
        Value is not null ? $"{Kind}({Value})" : $"{Kind}('{Lexeme}')";
}
