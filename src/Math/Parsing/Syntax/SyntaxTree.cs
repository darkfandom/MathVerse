namespace MathVerse.Math.Parsing.Syntax;

/// <summary>
/// Represents the root of an immutable syntax tree.
/// </summary>
public sealed class SyntaxTree
{
    /// <summary>Initializes a syntax tree.</summary>
    public SyntaxTree(ExpressionSyntax root, IReadOnlyList<SyntaxTrivia> leadingTrivia, IReadOnlyList<SyntaxTrivia> trailingTrivia, DiagnosticBag diagnostics)
    {
        Root = root;
        LeadingTrivia = leadingTrivia;
        TrailingTrivia = trailingTrivia;
        Diagnostics = diagnostics;
    }

    /// <summary>Gets the root expression node.</summary>
    public ExpressionSyntax Root { get; }

    /// <summary>Gets leading trivia.</summary>
    public IReadOnlyList<SyntaxTrivia> LeadingTrivia { get; }

    /// <summary>Gets trailing trivia.</summary>
    public IReadOnlyList<SyntaxTrivia> TrailingTrivia { get; }

    /// <summary>Gets diagnostics associated with this tree.</summary>
    public DiagnosticBag Diagnostics { get; }

    /// <summary>Gets whether the tree has any errors.</summary>
    public bool HasErrors => Diagnostics.HasErrors;

    /// <summary>Gets all diagnostics.</summary>
    public Diagnostic[] GetDiagnostics() => Diagnostics.GetAll();

    /// <summary>Creates a syntax tree from source text.</summary>
    public static SyntaxTree Parse(string source, LexerOptions? lexerOptions = null, ParserOptions? parserOptions = null)
    {
        return ParsingFacade.ParseSyntaxTree(source, lexerOptions, parserOptions);
    }

    /// <summary>Returns the full text of the tree.</summary>
    public override string ToString() => Root.ToString() ?? string.Empty;
}
