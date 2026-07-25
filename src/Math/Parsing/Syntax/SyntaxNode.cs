namespace MathVerse.Math.Parsing.Syntax;

/// <summary>
/// Abstract base class for all syntax tree nodes. Immutable and AOT-compatible.
/// </summary>
public abstract class SyntaxNode
{
    /// <summary>Initializes a syntax node.</summary>
    protected SyntaxNode(SyntaxKind kind, int position, int fullLength)
    {
        Kind = kind;
        Position = position;
        FullLength = fullLength;
    }

    /// <summary>Gets the syntax kind.</summary>
    public SyntaxKind Kind { get; }

    /// <summary>Gets the 0-based start position in source.</summary>
    public int Position { get; }

    /// <summary>Gets the full length of this node including trivia.</summary>
    public int FullLength { get; }

    /// <summary>Gets the end position (position + full length).</summary>
    public int EndPosition => Position + FullLength;

    /// <summary>Gets the child nodes.</summary>
    public abstract IReadOnlyList<SyntaxNode> Children { get; }

    /// <summary>Gets whether this node is a token.</summary>
    public bool IsToken => this is SyntaxToken;

    /// <summary>Gets whether this node is a trivia.</summary>
    public bool IsTrivia => Kind == SyntaxKind.WhitespaceTrivia ||
                             Kind == SyntaxKind.LineCommentTrivia ||
                             Kind == SyntaxKind.BlockCommentTrivia ||
                             Kind == SyntaxKind.NewlineTrivia;

    /// <summary>Returns a string representation for debugging.</summary>
    public override string ToString() => $"{Kind} [{Position}..{EndPosition})";
}

/// <summary>
/// Represents a leaf token node in the syntax tree.
/// </summary>
public sealed class SyntaxToken : SyntaxNode
{
    /// <summary>Initializes a syntax token.</summary>
    public SyntaxToken(SyntaxKind kind, int position, string text, object? value = null)
        : base(kind, position, text.Length)
    {
        Text = text;
        Value = value;
    }

    /// <summary>Gets the raw text of this token.</summary>
    public string Text { get; }

    /// <summary>Gets the parsed value.</summary>
    public object? Value { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [];

    /// <inheritdoc/>
    public override string ToString() => Value is not null ? $"{Kind}('{Text}', {Value})" : $"{Kind}('{Text}')";
}

/// <summary>
/// Represents a trivia node (whitespace, comments).
/// </summary>
public sealed class SyntaxTrivia : SyntaxNode
{
    /// <summary>Initializes a syntax trivia.</summary>
    public SyntaxTrivia(SyntaxKind kind, int position, string text)
        : base(kind, position, text.Length)
    {
        Text = text;
    }

    /// <summary>Gets the trivia text.</summary>
    public string Text { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [];

    /// <inheritdoc/>
    public override string ToString() => $"{Kind}('{Text}')";
}
