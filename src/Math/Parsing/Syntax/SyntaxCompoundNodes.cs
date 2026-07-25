namespace MathVerse.Math.Parsing.Syntax;

/// <summary>
/// An equation expression (left = right).
/// </summary>
public sealed class EquationExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes an equation expression.</summary>
    public EquationExpressionSyntax(ExpressionSyntax left, SyntaxToken equalsToken, ExpressionSyntax right)
        : base(SyntaxKind.EquationExpression, left.Position,
            right.Position + right.FullLength - left.Position)
    {
        Left = left;
        EqualsToken = equalsToken;
        Right = right;
    }

    /// <summary>Gets the left-hand side.</summary>
    public ExpressionSyntax Left { get; }

    /// <summary>Gets the '=' token.</summary>
    public SyntaxToken EqualsToken { get; }

    /// <summary>Gets the right-hand side.</summary>
    public ExpressionSyntax Right { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [Left, EqualsToken, Right];
}

/// <summary>
/// An assignment expression (target = value or target := value).
/// </summary>
public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes an assignment expression.</summary>
    public AssignmentExpressionSyntax(ExpressionSyntax target, SyntaxToken operatorToken, ExpressionSyntax value)
        : base(SyntaxKind.AssignmentExpression, target.Position,
            value.Position + value.FullLength - target.Position)
    {
        Target = target;
        OperatorToken = operatorToken;
        Value = value;
    }

    /// <summary>Gets the target expression.</summary>
    public ExpressionSyntax Target { get; }

    /// <summary>Gets the assignment operator token.</summary>
    public SyntaxToken OperatorToken { get; }

    /// <summary>Gets the value expression.</summary>
    public ExpressionSyntax Value { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [Target, OperatorToken, Value];
}

/// <summary>
/// A conditional expression (if condition then thenBranch else elseBranch).
/// </summary>
public sealed class ConditionalExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a conditional expression.</summary>
    public ConditionalExpressionSyntax(
        SyntaxToken ifKeyword, ExpressionSyntax condition,
        SyntaxToken thenKeyword, ExpressionSyntax thenBranch,
        SyntaxToken elseKeyword, ExpressionSyntax elseBranch)
        : base(SyntaxKind.ConditionalExpression, ifKeyword.Position,
            elseBranch.EndPosition - ifKeyword.Position)
    {
        IfKeyword = ifKeyword;
        Condition = condition;
        ThenKeyword = thenKeyword;
        ThenBranch = thenBranch;
        ElseKeyword = elseKeyword;
        ElseBranch = elseBranch;
    }

    /// <summary>Gets the 'if' keyword.</summary>
    public SyntaxToken IfKeyword { get; }

    /// <summary>Gets the condition.</summary>
    public ExpressionSyntax Condition { get; }

    /// <summary>Gets the 'then' keyword.</summary>
    public SyntaxToken ThenKeyword { get; }

    /// <summary>Gets the then branch.</summary>
    public ExpressionSyntax ThenBranch { get; }

    /// <summary>Gets the 'else' keyword.</summary>
    public SyntaxToken ElseKeyword { get; }

    /// <summary>Gets the else branch.</summary>
    public ExpressionSyntax ElseBranch { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [IfKeyword, Condition, ThenKeyword, ThenBranch, ElseKeyword, ElseBranch];
}

/// <summary>
/// A single case in a piecewise expression: value when condition.
/// </summary>
public sealed class PiecewiseCaseSyntax : SyntaxNode
{
    /// <summary>Initializes a piecewise case.</summary>
    public PiecewiseCaseSyntax(ExpressionSyntax value, SyntaxToken whenKeyword, ExpressionSyntax condition)
        : base(SyntaxKind.PiecewiseCase, value.Position,
            condition.Position + condition.FullLength - value.Position)
    {
        Value = value;
        WhenKeyword = whenKeyword;
        Condition = condition;
    }

    /// <summary>Gets the value expression.</summary>
    public ExpressionSyntax Value { get; }

    /// <summary>Gets the 'when' keyword.</summary>
    public SyntaxToken WhenKeyword { get; }

    /// <summary>Gets the condition expression.</summary>
    public ExpressionSyntax Condition { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [Value, WhenKeyword, Condition];
}

/// <summary>
/// A piecewise expression.
/// </summary>
public sealed class PiecewiseExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a piecewise expression.</summary>
    public PiecewiseExpressionSyntax(SyntaxToken keyword, SyntaxToken openBrace, IReadOnlyList<PiecewiseCaseSyntax> cases, SyntaxToken closeBrace)
        : base(SyntaxKind.PiecewiseExpression, keyword.Position,
            closeBrace.Position + closeBrace.FullLength - keyword.Position)
    {
        Keyword = keyword;
        OpenBrace = openBrace;
        Cases = cases;
        CloseBrace = closeBrace;
    }

    /// <summary>Gets the 'piecewise' keyword.</summary>
    public SyntaxToken Keyword { get; }

    /// <summary>Gets the open brace.</summary>
    public SyntaxToken OpenBrace { get; }

    /// <summary>Gets the cases.</summary>
    public IReadOnlyList<PiecewiseCaseSyntax> Cases { get; }

    /// <summary>Gets the close brace.</summary>
    public SyntaxToken CloseBrace { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { Keyword, OpenBrace };
            foreach (var c in Cases) list.Add(c);
            list.Add(CloseBrace);
            return list;
        }
    }
}

/// <summary>
/// A lambda expression: fn(params) => body.
/// </summary>
public sealed class LambdaExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a lambda expression.</summary>
    public LambdaExpressionSyntax(SyntaxToken fnKeyword, SyntaxToken openParen, IReadOnlyList<IdentifierNameSyntax> parameters, SyntaxToken closeParen, SyntaxToken arrowToken, ExpressionSyntax body)
        : base(SyntaxKind.LambdaExpression, fnKeyword.Position,
            body.Position + body.FullLength - fnKeyword.Position)
    {
        FnKeyword = fnKeyword;
        OpenParen = openParen;
        Parameters = parameters;
        CloseParen = closeParen;
        ArrowToken = arrowToken;
        Body = body;
    }

    /// <summary>Gets the 'fn' keyword.</summary>
    public SyntaxToken FnKeyword { get; }

    /// <summary>Gets the open parenthesis.</summary>
    public SyntaxToken OpenParen { get; }

    /// <summary>Gets the parameter names.</summary>
    public IReadOnlyList<IdentifierNameSyntax> Parameters { get; }

    /// <summary>Gets the close parenthesis.</summary>
    public SyntaxToken CloseParen { get; }

    /// <summary>Gets the arrow token (=> or →).</summary>
    public SyntaxToken ArrowToken { get; }

    /// <summary>Gets the body expression.</summary>
    public ExpressionSyntax Body { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { FnKeyword, OpenParen };
            foreach (var p in Parameters) list.Add(p);
            list.Add(CloseParen);
            list.Add(ArrowToken);
            list.Add(Body);
            return list;
        }
    }
}

/// <summary>
/// A vector literal expression [a, b, c].
/// </summary>
public sealed class VectorLiteralExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a vector literal expression.</summary>
    public VectorLiteralExpressionSyntax(SyntaxToken openBracket, IReadOnlyList<ExpressionSyntax> elements, SyntaxToken closeBracket)
        : base(SyntaxKind.VectorExpression, openBracket.Position,
            closeBracket.Position + closeBracket.FullLength - openBracket.Position)
    {
        OpenBracket = openBracket;
        Elements = elements;
        CloseBracket = closeBracket;
    }

    /// <summary>Gets the open bracket.</summary>
    public SyntaxToken OpenBracket { get; }

    /// <summary>Gets the element expressions.</summary>
    public IReadOnlyList<ExpressionSyntax> Elements { get; }

    /// <summary>Gets the close bracket.</summary>
    public SyntaxToken CloseBracket { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { OpenBracket };
            foreach (var e in Elements) list.Add(e);
            list.Add(CloseBracket);
            return list;
        }
    }
}

/// <summary>
/// A matrix literal expression [[a,b],[c,d]].
/// </summary>
public sealed class MatrixLiteralExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a matrix literal expression.</summary>
    public MatrixLiteralExpressionSyntax(SyntaxToken openBracket1, IReadOnlyList<VectorLiteralExpressionSyntax> rows, SyntaxToken closeBracket2)
        : base(SyntaxKind.MatrixExpression, openBracket1.Position,
            closeBracket2.Position + closeBracket2.FullLength - openBracket1.Position)
    {
        OpenBracket = openBracket1;
        Rows = rows;
        CloseBracket = closeBracket2;
    }

    /// <summary>Gets the outer open bracket.</summary>
    public SyntaxToken OpenBracket { get; }

    /// <summary>Gets the matrix rows.</summary>
    public IReadOnlyList<VectorLiteralExpressionSyntax> Rows { get; }

    /// <summary>Gets the outer close bracket.</summary>
    public SyntaxToken CloseBracket { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { OpenBracket };
            foreach (var r in Rows) list.Add(r);
            list.Add(CloseBracket);
            return list;
        }
    }
}

/// <summary>
/// A set literal expression {a, b, c}.
/// </summary>
public sealed class SetLiteralExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a set literal expression.</summary>
    public SetLiteralExpressionSyntax(SyntaxToken openBrace, IReadOnlyList<ExpressionSyntax> elements, SyntaxToken closeBrace)
        : base(SyntaxKind.SetExpression, openBrace.Position,
            closeBrace.Position + closeBrace.FullLength - openBrace.Position)
    {
        OpenBrace = openBrace;
        Elements = elements;
        CloseBrace = closeBrace;
    }

    /// <summary>Gets the open brace.</summary>
    public SyntaxToken OpenBrace { get; }

    /// <summary>Gets the set element expressions.</summary>
    public IReadOnlyList<ExpressionSyntax> Elements { get; }

    /// <summary>Gets the close brace.</summary>
    public SyntaxToken CloseBrace { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { OpenBrace };
            foreach (var e in Elements) list.Add(e);
            list.Add(CloseBrace);
            return list;
        }
    }
}

/// <summary>
/// A tuple expression (a, b, c).
/// </summary>
public sealed class TupleExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a tuple expression.</summary>
    public TupleExpressionSyntax(SyntaxToken openParen, IReadOnlyList<ExpressionSyntax> elements, SyntaxToken closeParen)
        : base(SyntaxKind.TupleExpression, openParen.Position,
            closeParen.Position + closeParen.FullLength - openParen.Position)
    {
        OpenParen = openParen;
        Elements = elements;
        CloseParen = closeParen;
    }

    /// <summary>Gets the open parenthesis.</summary>
    public SyntaxToken OpenParen { get; }

    /// <summary>Gets the element expressions.</summary>
    public IReadOnlyList<ExpressionSyntax> Elements { get; }

    /// <summary>Gets the close parenthesis.</summary>
    public SyntaxToken CloseParen { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { OpenParen };
            foreach (var e in Elements) list.Add(e);
            list.Add(CloseParen);
            return list;
        }
    }
}

/// <summary>
/// A derivative expression syntax node.
/// </summary>
public sealed class DerivativeExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a derivative expression.</summary>
    public DerivativeExpressionSyntax(SyntaxToken differentialToken, SyntaxToken variableToken, SyntaxToken divisionToken, ExpressionSyntax function)
        : base(SyntaxKind.DerivativeExpression, differentialToken.Position,
            function.Position + function.FullLength - differentialToken.Position)
    {
        DifferentialToken = differentialToken;
        VariableToken = variableToken;
        DivisionToken = divisionToken;
        Function = function;
    }

    /// <summary>Gets the 'd' or '∂' token.</summary>
    public SyntaxToken DifferentialToken { get; }

    /// <summary>Gets the variable token.</summary>
    public SyntaxToken VariableToken { get; }

    /// <summary>Gets the division token.</summary>
    public SyntaxToken DivisionToken { get; }

    /// <summary>Gets the function being differentiated.</summary>
    public ExpressionSyntax Function { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [DifferentialToken, VariableToken, DivisionToken, Function];
}

/// <summary>
/// An integral expression syntax node.
/// </summary>
public sealed class IntegralExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes an integral expression.</summary>
    public IntegralExpressionSyntax(SyntaxToken integralToken, ExpressionSyntax integrand, SyntaxToken differentialToken, SyntaxToken variableToken, ExpressionSyntax? lowerBound = null, ExpressionSyntax? upperBound = null)
        : base(SyntaxKind.IntegralExpression, integralToken.Position,
            (upperBound is not null ? upperBound.EndPosition : variableToken.EndPosition) - integralToken.Position)
    {
        IntegralToken = integralToken;
        Integrand = integrand;
        DifferentialToken = differentialToken;
        VariableToken = variableToken;
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    /// <summary>Gets the integral token '∫'.</summary>
    public SyntaxToken IntegralToken { get; }

    /// <summary>Gets the integrand.</summary>
    public ExpressionSyntax Integrand { get; }

    /// <summary>Gets the differential token 'd'.</summary>
    public SyntaxToken DifferentialToken { get; }

    /// <summary>Gets the integration variable token.</summary>
    public SyntaxToken VariableToken { get; }

    /// <summary>Gets the lower bound (null for indefinite).</summary>
    public ExpressionSyntax? LowerBound { get; }

    /// <summary>Gets the upper bound (null for indefinite).</summary>
    public ExpressionSyntax? UpperBound { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children
    {
        get
        {
            var list = new List<SyntaxNode> { IntegralToken, Integrand, DifferentialToken, VariableToken };
            if (LowerBound is not null) list.Add(LowerBound);
            if (UpperBound is not null) list.Add(UpperBound);
            return list;
        }
    }
}

/// <summary>
/// A summation expression syntax node.
/// </summary>
public sealed class SummationExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a summation expression.</summary>
    public SummationExpressionSyntax(SyntaxToken summationToken, SyntaxToken variableToken, SyntaxToken equalsToken, ExpressionSyntax lowerBound, SyntaxToken commaToken, ExpressionSyntax upperBound, ExpressionSyntax body)
        : base(SyntaxKind.SummationExpression, summationToken.Position,
            body.Position + body.FullLength - summationToken.Position)
    {
        SummationToken = summationToken;
        VariableToken = variableToken;
        EqualsToken = equalsToken;
        LowerBound = lowerBound;
        CommaToken = commaToken;
        UpperBound = upperBound;
        Body = body;
    }

    /// <summary>Gets the '∑' token.</summary>
    public SyntaxToken SummationToken { get; }

    /// <summary>Gets the summation variable token.</summary>
    public SyntaxToken VariableToken { get; }

    /// <summary>Gets the '=' token.</summary>
    public SyntaxToken EqualsToken { get; }

    /// <summary>Gets the lower bound.</summary>
    public ExpressionSyntax LowerBound { get; }

    /// <summary>Gets the comma token.</summary>
    public SyntaxToken CommaToken { get; }

    /// <summary>Gets the upper bound.</summary>
    public ExpressionSyntax UpperBound { get; }

    /// <summary>Gets the body expression.</summary>
    public ExpressionSyntax Body { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children =>
        [SummationToken, VariableToken, EqualsToken, LowerBound, CommaToken, UpperBound, Body];
}

/// <summary>
/// A product expression syntax node.
/// </summary>
public sealed class ProductExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a product expression.</summary>
    public ProductExpressionSyntax(SyntaxToken productToken, SyntaxToken variableToken, SyntaxToken equalsToken, ExpressionSyntax lowerBound, SyntaxToken commaToken, ExpressionSyntax upperBound, ExpressionSyntax body)
        : base(SyntaxKind.ProductExpression, productToken.Position,
            body.Position + body.FullLength - productToken.Position)
    {
        ProductToken = productToken;
        VariableToken = variableToken;
        EqualsToken = equalsToken;
        LowerBound = lowerBound;
        CommaToken = commaToken;
        UpperBound = upperBound;
        Body = body;
    }

    /// <summary>Gets the '∏' token.</summary>
    public SyntaxToken ProductToken { get; }

    /// <summary>Gets the product variable token.</summary>
    public SyntaxToken VariableToken { get; }

    /// <summary>Gets the '=' token.</summary>
    public SyntaxToken EqualsToken { get; }

    /// <summary>Gets the lower bound.</summary>
    public ExpressionSyntax LowerBound { get; }

    /// <summary>Gets the comma token.</summary>
    public SyntaxToken CommaToken { get; }

    /// <summary>Gets the upper bound.</summary>
    public ExpressionSyntax UpperBound { get; }

    /// <summary>Gets the body expression.</summary>
    public ExpressionSyntax Body { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children =>
        [ProductToken, VariableToken, EqualsToken, LowerBound, CommaToken, UpperBound, Body];
}

/// <summary>
/// A limit expression syntax node.
/// </summary>
public sealed class LimitExpressionSyntax : ExpressionSyntax
{
    /// <summary>Initializes a limit expression.</summary>
    public LimitExpressionSyntax(SyntaxToken limitKeyword, ExpressionSyntax body, SyntaxToken arrowToken, SyntaxToken variableToken, ExpressionSyntax target)
        : base(SyntaxKind.LimitExpression, limitKeyword.Position,
            target.Position + target.FullLength - limitKeyword.Position)
    {
        LimitKeyword = limitKeyword;
        Body = body;
        ArrowToken = arrowToken;
        VariableToken = variableToken;
        Target = target;
    }

    /// <summary>Gets the 'lim' keyword.</summary>
    public SyntaxToken LimitKeyword { get; }

    /// <summary>Gets the body expression.</summary>
    public ExpressionSyntax Body { get; }

    /// <summary>Gets the arrow token.</summary>
    public SyntaxToken ArrowToken { get; }

    /// <summary>Gets the limit variable token.</summary>
    public SyntaxToken VariableToken { get; }

    /// <summary>Gets the target expression.</summary>
    public ExpressionSyntax Target { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<SyntaxNode> Children => [LimitKeyword, Body, ArrowToken, VariableToken, Target];
}
