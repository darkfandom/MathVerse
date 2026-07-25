namespace MathVerse.Math.Parsing.Lexer;

/// <summary>
/// Represents a position in the source text.
/// </summary>
public readonly record struct TokenPosition
{
    /// <summary>Initializes a token position.</summary>
    public TokenPosition(int line, int column, int offset)
    {
        Line = line;
        Column = column;
        Offset = offset;
    }

    /// <summary>Gets the 1-based line number.</summary>
    public int Line { get; }

    /// <summary>Gets the 1-based column number.</summary>
    public int Column { get; }

    /// <summary>Gets the 0-based character offset from the start.</summary>
    public int Offset { get; }

    /// <summary>Gets the start position.</summary>
    public static TokenPosition Start => new(1, 1, 0);

    /// <inheritdoc/>
    public override string ToString() => $"Line {Line}, Col {Column}";
}
