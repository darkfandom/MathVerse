namespace MathVerse.Math.Parsing.Parser;

/// <summary>
/// Represents the result of a parse operation.
/// </summary>
public sealed class ParserResult
{
    /// <summary>Initializes a parser result.</summary>
    public ParserResult(SyntaxTree syntaxTree, bool success, DiagnosticBag diagnostics)
    {
        SyntaxTree = syntaxTree;
        Success = success;
        Diagnostics = diagnostics;
    }

    /// <summary>Gets the syntax tree (may be partial on error).</summary>
    public SyntaxTree SyntaxTree { get; }

    /// <summary>Gets whether parsing succeeded without errors.</summary>
    public bool Success { get; }

    /// <summary>Gets the diagnostics.</summary>
    public DiagnosticBag Diagnostics { get; }

    /// <summary>Gets the root expression if parsing succeeded.</summary>
    public ExpressionSyntax? Root => Success ? SyntaxTree.Root : null;

    /// <summary>Gets whether there are errors.</summary>
    public bool HasErrors => Diagnostics.HasErrors;

    /// <summary>Creates a successful result.</summary>
    public static ParserResult Succeeded(SyntaxTree syntaxTree, DiagnosticBag diagnostics) =>
        new(syntaxTree, true, diagnostics);

    /// <summary>Creates a failed result.</summary>
    public static ParserResult Failed(SyntaxTree syntaxTree, DiagnosticBag diagnostics) =>
        new(syntaxTree, false, diagnostics);
}
