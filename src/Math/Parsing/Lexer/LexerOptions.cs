namespace MathVerse.Math.Parsing.Lexer;

/// <summary>
/// Configuration options for the mathematical lexer.
/// </summary>
public sealed record LexerOptions
{
    /// <summary>Gets the default options.</summary>
    public static LexerOptions Default { get; } = new();

    /// <summary>Gets whether to skip whitespace tokens.</summary>
    public bool SkipWhitespace { get; init; } = true;

    /// <summary>Gets whether to skip single-line comments.</summary>
    public bool SkipLineComments { get; init; } = true;

    /// <summary>Gets whether to skip block comments.</summary>
    public bool SkipBlockComments { get; init; } = true;

    /// <summary>Gets whether to skip newline tokens.</summary>
    public bool SkipNewlines { get; init; } = true;

    /// <summary>Gets whether to include comments as trivia tokens.</summary>
    public bool IncludeComments { get; init; } = false;

    /// <summary>Gets whether to support Unicode mathematical symbols.</summary>
    public bool EnableUnicode { get; init; } = true;

    /// <summary>Gets whether to detect implicit multiplication.</summary>
    public bool EnableImplicitMultiplication { get; init; } = true;

    /// <summary>Gets the maximum nesting depth for braces.</summary>
    public int MaxNestingDepth { get; init; } = 128;
}
