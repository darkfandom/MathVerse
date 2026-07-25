namespace MathVerse.Math.Operators;

/// <summary>
/// Categorizes mathematical operators.
/// </summary>
public enum OperatorCategory
{
    /// <summary>Arithmetic operators (+, -, *, /, ^).</summary>
    Arithmetic,

    /// <summary>Logical operators (and, or, not, xor).</summary>
    Logical,

    /// <summary>Relational operators (==, !=, &lt;, &gt;, &lt;=, &gt;=).</summary>
    Relational,

    /// <summary>Set operators (∪, ∩, \, ∈, ⊂).</summary>
    Set,

    /// <summary>Matrix operators (transpose, inverse, determinant).</summary>
    Matrix,

    /// <summary>Tensor operators (outer product, contraction).</summary>
    Tensor,

    /// <summary>Calculus operators (derivative, integral, limit).</summary>
    Calculus,

    /// <summary>Functional operators (composition, application).</summary>
    Functional,

    /// <summary>Assignment operators (=, +=, -=).</summary>
    Assignment,

    /// <summary>Custom user-defined operators.</summary>
    Custom
}
