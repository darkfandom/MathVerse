namespace MathVerse.Math.Expressions;

/// <summary>
/// Categorizes all expression node types in the mathematical expression tree.
/// </summary>
public enum ExpressionKind
{
    /// <summary>A literal numeric value.</summary>
    Literal,

    /// <summary>A named variable.</summary>
    Variable,

    /// <summary>A named mathematical constant (e.g., pi, e).</summary>
    Constant,

    /// <summary>A binary operation (add, mul, pow, etc.).</summary>
    Binary,

    /// <summary>A unary operation (neg, abs, etc.).</summary>
    Unary,

    /// <summary>A function call.</summary>
    FunctionCall,

    /// <summary>A lambda/anonymous function.</summary>
    Lambda,

    /// <summary>A lambda parameter.</summary>
    Parameter,

    /// <summary>An equation (left = right).</summary>
    Equation,

    /// <summary>A piecewise-defined expression.</summary>
    Piecewise,

    /// <summary>An if-then-else conditional.</summary>
    Conditional,

    /// <summary>An ordered tuple of expressions.</summary>
    Tuple,

    /// <summary>A vector literal.</summary>
    Vector,

    /// <summary>A matrix literal.</summary>
    Matrix,

    /// <summary>A tensor literal.</summary>
    Tensor,

    /// <summary>An indexing operation.</summary>
    Index,

    /// <summary>A slicing operation.</summary>
    Slice,

    /// <summary>A derivative (d/dx).</summary>
    Derivative,

    /// <summary>An integral.</summary>
    Integral,

    /// <summary>A summation (Sigma).</summary>
    Summation,

    /// <summary>A product (Pi).</summary>
    Product,

    /// <summary>A limit expression.</summary>
    Limit,

    /// <summary>A factorial expression.</summary>
    Factorial,

    /// <summary>An integer range expression.</summary>
    Range,

    /// <summary>A continuous interval.</summary>
    Interval,

    /// <summary>A set literal.</summary>
    Set,

    /// <summary>A complex number expression.</summary>
    Complex,

    /// <summary>A polynomial expression.</summary>
    Polynomial,

    /// <summary>A boolean literal or operation.</summary>
    Boolean,

    /// <summary>A relational comparison (==, !=, &lt;, &gt;, etc.).</summary>
    Relation,

    /// <summary>An assignment expression.</summary>
    Assignment,

    /// <summary>A function composition expression.</summary>
    Composition,

    /// <summary>An identity element expression.</summary>
    Identity,

    /// <summary>An undefined/null expression.</summary>
    Null
}
