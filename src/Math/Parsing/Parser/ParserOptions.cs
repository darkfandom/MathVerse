namespace MathVerse.Math.Parsing.Parser;

/// <summary>
/// Configuration options for the mathematical parser.
/// </summary>
public sealed record ParserOptions
{
    /// <summary>Gets the default options.</summary>
    public static ParserOptions Default { get; } = new();

    /// <summary>Gets whether implicit multiplication is allowed (e.g., "2x" means 2*x).</summary>
    public bool AllowImplicitMultiplication { get; init; } = true;

    /// <summary>Gets whether equations (a = b) are allowed.</summary>
    public bool AllowEquations { get; init; } = true;

    /// <summary>Gets whether assignments (x := y) are allowed.</summary>
    public bool AllowAssignments { get; init; } = true;

    /// <summary>Gets whether lambda expressions are allowed.</summary>
    public bool AllowLambdas { get; init; } = true;

    /// <summary>Gets whether calculus expressions are allowed.</summary>
    public bool AllowCalculus { get; init; } = true;

    /// <summary>Gets whether matrix/vector literals are allowed.</summary>
    public bool AllowLinearAlgebra { get; init; } = true;

    /// <summary>Gets whether set/interval literals are allowed.</summary>
    public bool AllowSets { get; init; } = true;

    /// <summary>Gets whether piecewise expressions are allowed.</summary>
    public bool AllowPiecewise { get; init; } = true;

    /// <summary>Gets the maximum expression nesting depth.</summary>
    public int MaxNestingDepth { get; init; } = 128;

    /// <summary>Gets the lexer options to use.</summary>
    public LexerOptions LexerOptions { get; init; } = LexerOptions.Default;
}
