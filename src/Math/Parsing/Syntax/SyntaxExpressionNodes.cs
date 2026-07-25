namespace MathVerse.Math.Parsing.Syntax;

/// <summary>
/// Base class for expression syntax nodes.
/// </summary>
public abstract class ExpressionSyntax : SyntaxNode
{
    /// <summary>Initializes an expression syntax node.</summary>
    protected ExpressionSyntax(SyntaxKind kind, int position, int length)
        : base(kind, position, length) { }
}

/// <summary>
/// A literal expression (number, string, boolean).
/// </summary>
public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a literal expression.</summary>
    public LiteralExpressionSyntax(SyntaxToken token)
        : base(SyntaxKind.LiteralExpression, token.Position, token.FullLength)
    {
        Token = token;
    }

    /// <summary>Gets the literal token.</summary>
    public SyntaxToken Token { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [Token];
}

/// <summary>
/// An identifier name expression.
/// </summary>
public sealed class IdentifierNameSyntax : ExpressionSyntax
{
    /// <summary>Initializes an identifier name expression.</summary>
    public IdentifierNameSyntax(SyntaxToken identifier)
        : base(SyntaxKind.IdentifierNameExpression, identifier.Position, identifier.FullLength)
    {
        Identifier = identifier;
    }

    /// <summary>Gets the identifier token.</summary>
    public SyntaxToken Identifier { get; }

    /// <summary>Gets the identifier name.</summary>
    public string Name => Identifier.Text;

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [Identifier];
}

/// <summary>
/// A binary expression (left op right).
/// </summary>
public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a binary expression.</summary>
    public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        : base(SyntaxKind.BinaryExpression, left.Position, left.FullLength + operatorToken.FullLength + right.FullLength)
    {
        Left = left;
        OperatorToken = operatorToken;
        Right = right;
    }

    /// <summary>Gets the left operand.</summary>
    public ExpressionSyntax Left { get; }

    /// <summary>Gets the operator token.</summary>
    public SyntaxToken OperatorToken { get; }

    /// <summary>Gets the right operand.</summary>
    public ExpressionSyntax Right { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [Left, OperatorToken, Right];
}

/// <summary>
/// A unary expression (op operand or operand op).
/// </summary>
public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a unary expression.</summary>
    public UnaryExpressionSyntax(SyntaxToken operatorToken, ExpressionSyntax operand, bool isPrefix)
        : base(SyntaxKind.UnaryExpression,
            isPrefix ? operatorToken.Position : operand.Position,
            isPrefix ? operatorToken.FullLength + operand.FullLength : operand.FullLength + operatorToken.FullLength)
    {
        OperatorToken = operatorToken;
        Operand = operand;
        IsPrefix = isPrefix;
    }

    /// <summary>Gets the operator token.</summary>
    public SyntaxToken OperatorToken { get; }

    /// <summary>Gets the operand expression.</summary>
    public ExpressionSyntax Operand { get; }

    /// <summary>Gets whether the operator is a prefix.</summary>
    public bool IsPrefix { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [OperatorToken, Operand];
}

/// <summary>
/// A function call expression (name(args)).
/// </summary>
public sealed class FunctionCallExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a function call expression.</summary>
    public FunctionCallExpressionSyntax(SyntaxToken nameToken, SyntaxToken openParen, IReadOnlyList<ExpressionSyntax> arguments, SyntaxToken closeParen)
        : base(SyntaxKind.FunctionCallExpression, nameToken.Position,
            closeParen.Position + closeParen.FullLength - nameToken.Position)
    {
        NameToken = nameToken;
        OpenParen = openParen;
        Arguments = arguments;
        CloseParen = closeParen;
    }

    /// <summary>Gets the function name token.</summary>
    public SyntaxToken NameToken { get; }

    /// <summary>Gets the open parenthesis.</summary>
    public SyntaxToken OpenParen { get; }

    /// <summary>Gets the argument expressions.</summary>
    public IReadOnlyList<ExpressionSyntax> Arguments { get; }

    /// <summary>Gets the close parenthesis.</summary>
    public SyntaxToken CloseParen { get; }

    /// <summary>Gets the function name.</summary>
    public string FunctionName => NameToken.Text;

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { NameToken, OpenParen };
            foreach (var arg in Arguments)
                list.Add(arg);
            list.Add(CloseParen);
            return list;
        }
    }
}

/// <summary>
/// A parenthesized expression ((expr)).
/// </summary>
public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a parenthesized expression.</summary>
    public ParenthesizedExpressionSyntax(SyntaxToken openParen, ExpressionSyntax inner, SyntaxToken closeParen)
        : base(SyntaxKind.ParenthesizedExpression, openParen.Position,
            closeParen.Position + closeParen.FullLength - openParen.Position)
    {
        OpenParen = openParen;
        Inner = inner;
        CloseParen = closeParen;
    }

    /// <summary>Gets the open parenthesis.</summary>
    public SyntaxToken OpenParen { get; }

    /// <summary>Gets the inner expression.</summary>
    public ExpressionSyntax Inner { get; }

    /// <summary>Gets the close parenthesis.</summary>
    public SyntaxToken CloseParen { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [OpenParen, Inner, CloseParen];
}

/// <summary>
/// A postfix expression (operand op), e.g., n!, Aᵀ, A⁻¹.
/// </summary>
public sealed class PostfixExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a postfix expression.</summary>
    public PostfixExpressionSyntax(ExpressionSyntax operand, SyntaxToken operatorToken)
        : base(SyntaxKind.PostfixExpression, operand.Position,
            operand.FullLength + operatorToken.FullLength)
    {
        Operand = operand;
        OperatorToken = operatorToken;
    }

    /// <summary>Gets the operand expression.</summary>
    public ExpressionSyntax Operand { get; }

    /// <summary>Gets the postfix operator token.</summary>
    public SyntaxToken OperatorToken { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [Operand, OperatorToken];
}

/// <summary>
/// A superscript expression (base^exponent rendered with Unicode superscript).
/// </summary>
public sealed class SuperscriptExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a superscript expression.</summary>
    public SuperscriptExpressionSyntax(ExpressionSyntax baseExpr, SyntaxToken superscriptToken)
        : base(SyntaxKind.SuperscriptExpression, baseExpr.Position,
            baseExpr.FullLength + superscriptToken.FullLength)
    {
        Base = baseExpr;
        SuperscriptToken = superscriptToken;
    }

    /// <summary>Gets the base expression.</summary>
    public ExpressionSyntax Base { get; }

    /// <summary>Gets the superscript token.</summary>
    public SyntaxToken SuperscriptToken { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [Base, SuperscriptToken];
}

/// <summary>
/// An index expression (target[indices]).
/// </summary>
public sealed class IndexExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes an index expression.</summary>
    public IndexExpressionSyntax(ExpressionSyntax target, SyntaxToken openBracket, IReadOnlyList<ExpressionSyntax> indices, SyntaxToken closeBracket)
        : base(SyntaxKind.IndexExpression, target.Position,
            closeBracket.Position + closeBracket.FullLength - target.Position)
    {
        Target = target;
        OpenBracket = openBracket;
        Indices = indices;
        CloseBracket = closeBracket;
    }

    /// <summary>Gets the target expression.</summary>
    public ExpressionSyntax Target { get; }

    /// <summary>Gets the open bracket.</summary>
    public SyntaxToken OpenBracket { get; }

    /// <summary>Gets the index expressions.</summary>
    public IReadOnlyList<ExpressionSyntax> Indices { get; }

    /// <summary>Gets the close bracket.</summary>
    public SyntaxToken CloseBracket { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { Target, OpenBracket };
            foreach (var idx in Indices)
                list.Add(idx);
            list.Add(CloseBracket);
            return list;
        }
    }
}

/// <summary>
/// An interval expression [lower, upper] or (lower, upper).
/// </summary>
public sealed class IntervalExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes an interval expression.</summary>
    public IntervalExpressionSyntax(SyntaxToken openToken, ExpressionSyntax lower, SyntaxToken commaToken, ExpressionSyntax upper, SyntaxToken closeToken, bool lowerClosed, bool upperClosed)
        : base(SyntaxKind.IntervalExpression, openToken.Position,
            closeToken.Position + closeToken.FullLength - openToken.Position)
    {
        OpenToken = openToken;
        Lower = lower;
        CommaToken = commaToken;
        Upper = upper;
        CloseToken = closeToken;
        LowerClosed = lowerClosed;
        UpperClosed = upperClosed;
    }

    /// <summary>Gets the open delimiter.</summary>
    public SyntaxToken OpenToken { get; }

    /// <summary>Gets the lower bound.</summary>
    public ExpressionSyntax Lower { get; }

    /// <summary>Gets the comma token.</summary>
    public SyntaxToken CommaToken { get; }

    /// <summary>Gets the upper bound.</summary>
    public ExpressionSyntax Upper { get; }

    /// <summary>Gets the close delimiter.</summary>
    public SyntaxToken CloseToken { get; }

    /// <summary>Gets whether the lower bound is closed (included).</summary>
    public bool LowerClosed { get; }

    /// <summary>Gets whether the upper bound is closed (included).</summary>
    public bool UpperClosed { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [OpenToken, Lower, CommaToken, Upper, CloseToken];
}
