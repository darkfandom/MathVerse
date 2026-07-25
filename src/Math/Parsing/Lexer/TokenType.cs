namespace MathVerse.Math.Parsing.Lexer;

/// <summary>
/// Categorizes all tokens produced by the mathematical lexer.
/// </summary>
public enum TokenType
{
    /// <summary>An unrecognized token.</summary>
    Unknown,

    /// <summary>End of input.</summary>
    Eof,

    // ─── Literals ───

    /// <summary>An integer literal (e.g., 42).</summary>
    IntegerLiteral,

    /// <summary>A real/floating-point literal (e.g., 3.14).</summary>
    RealLiteral,

    /// <summary>A string literal (e.g., "hello").</summary>
    StringLiteral,

    /// <summary>A complex literal suffix (e.g., 3i, 2.5i).</summary>
    ComplexLiteral,

    // ─── Identifiers & Keywords ───

    /// <summary>An identifier (e.g., x, alpha, myVar).</summary>
    Identifier,

    /// <summary>The 'fn' keyword.</summary>
    KeywordFn,

    /// <summary>The 'if' keyword.</summary>
    KeywordIf,

    /// <summary>The 'then' keyword.</summary>
    KeywordThen,

    /// <summary>The 'else' keyword.</summary>
    KeywordElse,

    /// <summary>The 'elif' keyword.</summary>
    KeywordElif,

    /// <summary>The 'let' keyword.</summary>
    KeywordLet,

    /// <summary>The 'in' keyword.</summary>
    KeywordIn,

    /// <summary>The 'where' keyword.</summary>
    KeywordWhere,

    /// <summary>The 'piecewise' keyword.</summary>
    KeywordPiecewise,

    /// <summary>The 'true' keyword.</summary>
    KeywordTrue,

    /// <summary>The 'false' keyword.</summary>
    KeywordFalse,

    // ─── Named Constants ───

    /// <summary>The constant 'pi'.</summary>
    ConstantPi,

    /// <summary>The constant 'e' (Euler's number).</summary>
    ConstantE,

    /// <summary>The imaginary unit 'i'.</summary>
    ConstantI,

    /// <summary>The infinity symbol.</summary>
    ConstantInfinity,

    // ─── Arithmetic Operators ───

    /// <summary>The '+' operator.</summary>
    Plus,

    /// <summary>The '-' operator.</summary>
    Minus,

    /// <summary>The '*' operator.</summary>
    Star,

    /// <summary>The '/' operator.</summary>
    Slash,

    /// <summary>The '%' operator.</summary>
    Percent,

    /// <summary>The '^' operator.</summary>
    Caret,

    // ─── Assignment ───

    /// <summary>The '=' operator (assignment or equation).</summary>
    Equals,

    /// <summary>The '+=' operator.</summary>
    PlusEquals,

    /// <summary>The '-=' operator.</summary>
    MinusEquals,

    /// <summary>The '*=' operator.</summary>
    StarEquals,

    // ─── Comparison Operators ───

    /// <summary>The '==' operator.</summary>
    EqualsEquals,

    /// <summary>The '!=' operator.</summary>
    NotEquals,

    /// <summary>The '&lt;' operator.</summary>
    LessThan,

    /// <summary>The '&gt;' operator.</summary>
    GreaterThan,

    /// <summary>The '&lt;=' operator.</summary>
    LessThanOrEqual,

    /// <summary>The '&gt;=' operator.</summary>
    GreaterThanOrEqual,

    // ─── Logical Operators ───

    /// <summary>The '!' operator (logical not).</summary>
    Exclamation,

    /// <summary>The '&amp;&amp;' or '∧' operator.</summary>
    AmpersandAmpersand,

    /// <summary>The '||' or '∨' operator.</summary>
    PipePipe,

    // ─── Calculus Operators ───

    /// <summary>The summation symbol '∑'.</summary>
    Summation,

    /// <summary>The product symbol '∏'.</summary>
    Product,

    /// <summary>The integral symbol '∫'.</summary>
    Integral,

    /// <summary>The differential 'd' in calculus context.</summary>
    Differential,

    /// <summary>The partial derivative symbol '∂'.</summary>
    Partial,

    /// <summary>The gradient/nabla symbol '∇'.</summary>
    Nabla,

    /// <summary>The limit keyword 'lim'.</summary>
    Limit,

    // ─── Unicode Operators ───

    /// <summary>The logical AND '∧'.</summary>
    Wedge,

    /// <summary>The logical OR '∨'.</summary>
    Vee,

    /// <summary>The logical NOT '¬'.</summary>
    Negation,

    /// <summary>The implies '⇒'.</summary>
    Implies,

    /// <summary>The iff '⇔'.</summary>
    Equivalent,

    /// <summary>The element-of '∈'.</summary>
    ElementOf,

    /// <summary>The not-element-of '∉'.</summary>
    NotElementOf,

    /// <summary>The subset '⊂'.</summary>
    Subset,

    /// <summary>The superset '⊃'.</summary>
    Superset,

    /// <summary>The union '∪'.</summary>
    Union,

    /// <summary>The intersection '∩'.</summary>
    Intersection,

    /// <summary>The set difference '\\', '∖'.</summary>
    SetDifference,

    /// <summary>The cross product '×'.</summary>
    CrossProduct,

    /// <summary>The dot product '·'.</summary>
    DotProduct,

    /// <summary>The compose '∘'.</summary>
    Compose,

    /// <summary>The tensor product '⊗'.</summary>
    TensorProduct,

    /// <summary>The transpose 'ᵀ'.</summary>
    Transpose,

    /// <summary>The inverse '⁻¹'.</summary>
    Inverse,

    /// <summary>The arrow '→'.</summary>
    Arrow,

    /// <summary>The mapping arrow '↦'.</summary>
    MapsTo,

    /// <summary>The parallel '∥'.</summary>
    Parallel,

    /// <summary>The not-equal '≠'.</summary>
    NotEqualSign,

    /// <summary>The less-than-or-equal '≤'.</summary>
    LessThanOrEqualSign,

    /// <summary>The greater-than-or-equal '≥'.</summary>
    GreaterThanOrEqualSign,

    /// <summary>The approximately-equal '≈'.</summary>
    ApproximatelyEqual,

    // ─── Named Functions ───

    /// <summary>The 'sin' function.</summary>
    FuncSin,

    /// <summary>The 'cos' function.</summary>
    FuncCos,

    /// <summary>The 'tan' function.</summary>
    FuncTan,

    /// <summary>The 'asin' function.</summary>
    FuncAsin,

    /// <summary>The 'acos' function.</summary>
    FuncAcos,

    /// <summary>The 'atan' function.</summary>
    FuncAtan,

    /// <summary>The 'sinh' function.</summary>
    FuncSinh,

    /// <summary>The 'cosh' function.</summary>
    FuncCosh,

    /// <summary>The 'tanh' function.</summary>
    FuncTanh,

    /// <summary>The 'ln' function.</summary>
    FuncLn,

    /// <summary>The 'log' function.</summary>
    FuncLog,

    /// <summary>The 'log10' function.</summary>
    FuncLog10,

    /// <summary>The 'exp' function.</summary>
    FuncExp,

    /// <summary>The 'sqrt' function.</summary>
    FuncSqrt,

    /// <summary>The 'cbrt' function.</summary>
    FuncCbrt,

    /// <summary>The 'abs' function.</summary>
    FuncAbs,

    /// <summary>The 'floor' function.</summary>
    FuncFloor,

    /// <summary>The 'ceil' function.</summary>
    FuncCeil,

    /// <summary>The 'round' function.</summary>
    FuncRound,

    /// <summary>The 'min' function.</summary>
    FuncMin,

    /// <summary>The 'max' function.</summary>
    FuncMax,

    /// <summary>The 'det' function.</summary>
    FuncDet,

    /// <summary>The 'transpose' function.</summary>
    FuncTranspose,

    /// <summary>The 'inverse' function.</summary>
    FuncInverse,

    /// <summary>The 'mod' function.</summary>
    FuncMod,

    // ─── Punctuation ───

    /// <summary>The '(' token.</summary>
    OpenParen,

    /// <summary>The ')' token.</summary>
    CloseParen,

    /// <summary>The '[' token.</summary>
    OpenBracket,

    /// <summary>The ']' token.</summary>
    CloseBracket,

    /// <summary>The '{' token.</summary>
    OpenBrace,

    /// <summary>The '}' token.</summary>
    CloseBrace,

    /// <summary>The ',' token.</summary>
    Comma,

    /// <summary>The ';' token.</summary>
    Semicolon,

    /// <summary>The ':' token.</summary>
    Colon,

    /// <summary>The '.' token.</summary>
    Dot,

    /// <summary>The '..' token (range).</summary>
    DotDot,

    /// <summary>The '...' token.</summary>
    DotDotDot,

    /// <summary>The '|' token (absolute value / pipe).</summary>
    Pipe,

    /// <summary>The '|' matching for abs.</summary>
    DoublePipe,

    // ─── Special ───

    /// <summary>Whitespace.</summary>
    Whitespace,

    /// <summary>A single-line comment.</summary>
    LineComment,

    /// <summary>A block comment.</summary>
    BlockComment,

    /// <summary>A newline character.</summary>
    Newline,

    /// <summary>An implicit multiplication (detected between adjacent tokens).</summary>
    ImplicitMultiply,
}
