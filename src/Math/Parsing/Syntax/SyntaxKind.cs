namespace MathVerse.Math.Parsing.Syntax;

/// <summary>
/// Categorizes all syntax nodes, tokens, and trivia in the syntax tree.
/// </summary>
public enum SyntaxKind
{
    // ─── Trivia ───

    /// <summary>Whitespace trivia.</summary>
    WhitespaceTrivia,

    /// <summary>Line comment trivia.</summary>
    LineCommentTrivia,

    /// <summary>Block comment trivia.</summary>
    BlockCommentTrivia,

    /// <summary>Newline trivia.</summary>
    NewlineTrivia,

    // ─── Token Kinds ───

    /// <summary>An unknown token.</summary>
    UnknownToken,

    /// <summary>End of file.</summary>
    EndOfFile,

    /// <summary>An integer literal token.</summary>
    IntegerLiteralToken,

    /// <summary>A real literal token.</summary>
    RealLiteralToken,

    /// <summary>A string literal token.</summary>
    StringLiteralToken,

    /// <summary>An identifier token.</summary>
    IdentifierToken,

    /// <summary>The '+' token.</summary>
    PlusToken,

    /// <summary>The '-' token.</summary>
    MinusToken,

    /// <summary>The '*' token.</summary>
    StarToken,

    /// <summary>The '/' token.</summary>
    SlashToken,

    /// <summary>The '%' token.</summary>
    PercentToken,

    /// <summary>The '^' token.</summary>
    CaretToken,

    /// <summary>The '=' token.</summary>
    EqualsToken,

    /// <summary>The '==' token.</summary>
    EqualsEqualsToken,

    /// <summary>The '!=' token.</summary>
    NotEqualsToken,

    /// <summary>The '&lt;' token.</summary>
    LessThanToken,

    /// <summary>The '&gt;' token.</summary>
    GreaterThanToken,

    /// <summary>The '&lt;=' token.</summary>
    LessThanOrEqualToken,

    /// <summary>The '&gt;=' token.</summary>
    GreaterThanOrEqualToken,

    /// <summary>The '!' token.</summary>
    ExclamationToken,

    /// <summary>The '&amp;&amp;' token.</summary>
    AmpersandAmpersandToken,

    /// <summary>The '||' token.</summary>
    PipePipeToken,

    /// <summary>The '(' token.</summary>
    OpenParenToken,

    /// <summary>The ')' token.</summary>
    CloseParenToken,

    /// <summary>The '[' token.</summary>
    OpenBracketToken,

    /// <summary>The ']' token.</summary>
    CloseBracketToken,

    /// <summary>The '{' token.</summary>
    OpenBraceToken,

    /// <summary>The '}' token.</summary>
    CloseBraceToken,

    /// <summary>The ',' token.</summary>
    CommaToken,

    /// <summary>The ';' token.</summary>
    SemicolonToken,

    /// <summary>The ':' token.</summary>
    ColonToken,

    /// <summary>The '.' token.</summary>
    DotToken,

    /// <summary>The '..' token.</summary>
    DotDotToken,

    /// <summary>The '|' token.</summary>
    PipeToken,

    /// <summary>Implicit multiplication token.</summary>
    ImplicitMultiplyToken,

    // ─── Keyword Tokens ───

    /// <summary>The 'fn' keyword.</summary>
    FnKeyword,

    /// <summary>The 'if' keyword.</summary>
    IfKeyword,

    /// <summary>The 'then' keyword.</summary>
    ThenKeyword,

    /// <summary>The 'else' keyword.</summary>
    ElseKeyword,

    /// <summary>The 'elif' keyword.</summary>
    ElifKeyword,

    /// <summary>The 'let' keyword.</summary>
    LetKeyword,

    /// <summary>The 'in' keyword.</summary>
    InKeyword,

    /// <summary>The 'where' keyword.</summary>
    WhereKeyword,

    /// <summary>The 'piecewise' keyword.</summary>
    PiecewiseKeyword,

    /// <summary>The 'true' keyword.</summary>
    TrueKeyword,

    /// <summary>The 'false' keyword.</summary>
    FalseKeyword,

    // ─── Unicode Symbol Tokens ───

    /// <summary>The summation '∑'.</summary>
    SummationToken,

    /// <summary>The product '∏'.</summary>
    ProductToken,

    /// <summary>The integral '∫'.</summary>
    IntegralToken,

    /// <summary>The partial '∂'.</summary>
    PartialToken,

    /// <summary>The nabla '∇'.</summary>
    NablaToken,

    /// <summary>The 'lim' keyword.</summary>
    LimitKeyword,

    /// <summary>The logical AND '∧'.</summary>
    WedgeToken,

    /// <summary>The logical OR '∨'.</summary>
    VeeToken,

    /// <summary>The logical NOT '¬'.</summary>
    NegationToken,

    /// <summary>The implies '⇒'.</summary>
    ImpliesToken,

    /// <summary>The iff '⇔'.</summary>
    EquivToken,

    /// <summary>The element-of '∈'.</summary>
    ElementOfToken,

    /// <summary>The subset '⊂'.</summary>
    SubsetToken,

    /// <summary>The superset '⊃'.</summary>
    SupersetToken,

    /// <summary>The union '∪'.</summary>
    UnionToken,

    /// <summary>The intersection '∩'.</summary>
    IntersectionToken,

    /// <summary>The set difference.</summary>
    SetDiffToken,

    /// <summary>The cross '×'.</summary>
    CrossProductToken,

    /// <summary>The dot '·'.</summary>
    DotProductToken,

    /// <summary>The compose '∘'.</summary>
    ComposeToken,

    /// <summary>The '→'.</summary>
    ArrowToken,

    /// <summary>The 'ᵀ'.</summary>
    TransposeToken,

    /// <summary>The '⁻¹'.</summary>
    InverseToken,

    // ─── Named Function Tokens ───

    /// <summary>sin function.</summary>
    SinKeyword,

    /// <summary>cos function.</summary>
    CosKeyword,

    /// <summary>tan function.</summary>
    TanKeyword,

    /// <summary>asin function.</summary>
    AsinKeyword,

    /// <summary>acos function.</summary>
    AcosKeyword,

    /// <summary>atan function.</summary>
    AtanKeyword,

    /// <summary>sinh function.</summary>
    SinhKeyword,

    /// <summary>cosh function.</summary>
    CoshKeyword,

    /// <summary>tanh function.</summary>
    TanhKeyword,

    /// <summary>ln function.</summary>
    LnKeyword,

    /// <summary>log function.</summary>
    LogKeyword,

    /// <summary>exp function.</summary>
    ExpKeyword,

    /// <summary>sqrt function.</summary>
    SqrtKeyword,

    /// <summary>abs function.</summary>
    AbsKeyword,

    /// <summary>floor function.</summary>
    FloorKeyword,

    /// <summary>ceil function.</summary>
    CeilKeyword,

    /// <summary>round function.</summary>
    RoundKeyword,

    /// <summary>min function.</summary>
    MinKeyword,

    /// <summary>max function.</summary>
    MaxKeyword,

    /// <summary>det function.</summary>
    DetKeyword,

    /// <summary>mod function.</summary>
    ModKeyword,

    // ─── Expression Syntax Nodes ───

    /// <summary>A literal expression.</summary>
    LiteralExpression,

    /// <summary>An identifier name expression.</summary>
    IdentifierNameExpression,

    /// <summary>A binary expression.</summary>
    BinaryExpression,

    /// <summary>A unary expression.</summary>
    UnaryExpression,

    /// <summary>A function call expression.</summary>
    FunctionCallExpression,

    /// <summary>A parenthesized expression.</summary>
    ParenthesizedExpression,

    /// <summary>A dotted/member access expression (e.g., Aᵀ).</summary>
    MemberAccessExpression,

    /// <summary>A postfix expression (factorial !, transpose ᵀ, inverse ⁻¹).</summary>
    PostfixExpression,

    /// <summary>A superscript expression (x²).</summary>
    SuperscriptExpression,

    /// <summary>An index expression (A[i,j]).</summary>
    IndexExpression,

    /// <summary>A slice expression (A[1:3,:]).</summary>
    SliceExpression,

    /// <summary>A range expression (1..10).</summary>
    RangeExpression,

    /// <summary>An equation expression (a = b).</summary>
    EquationExpression,

    /// <summary>An assignment expression (x := expr).</summary>
    AssignmentExpression,

    /// <summary>A conditional expression (if-then-else).</summary>
    ConditionalExpression,

    /// <summary>A piecewise expression.</summary>
    PiecewiseExpression,

    /// <summary>A piecewise case.</summary>
    PiecewiseCase,

    /// <summary>A lambda expression.</summary>
    LambdaExpression,

    /// <summary>A parameter list.</summary>
    ParameterList,

    /// <summary>A function argument list.</summary>
    ArgumentList,

    /// <summary>A vector literal [a, b, c].</summary>
    VectorExpression,

    /// <summary>A matrix expression [[a,b],[c,d]].</summary>
    MatrixExpression,

    /// <summary>A set expression {a, b, c}.</summary>
    SetExpression,

    /// <summary>A tuple expression (a, b, c).</summary>
    TupleExpression,

    /// <summary>An interval expression [a, b].</summary>
    IntervalExpression,

    /// <summary>A derivative expression (d/dx f).</summary>
    DerivativeExpression,

    /// <summary>An integral expression (∫ f dx).</summary>
    IntegralExpression,

    /// <summary>A summation expression (∑ f).</summary>
    SummationExpression,

    /// <summary>A product expression (∏ f).</summary>
    ProductExpression,

    /// <summary>A limit expression (lim f).</summary>
    LimitExpression,

    /// <summary>A complex number expression (a + bi).</summary>
    ComplexExpression,

    /// <summary>A block of statements separated by semicolons.</summary>
    StatementBlock,

    /// <summary>A compilation unit (root of syntax tree).</summary>
    CompilationUnit,
}
