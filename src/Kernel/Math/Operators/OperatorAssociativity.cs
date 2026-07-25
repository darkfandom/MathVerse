namespace MathVerse.Math.Operators;

/// <summary>
/// Specifies the associativity of an operator.
/// </summary>
public enum OperatorAssociativity
{
    /// <summary>Left-to-right associativity (a + b + c = (a + b) + c).</summary>
    Left,

    /// <summary>Right-to-left associativity (a ^ b ^ c = a ^ (b ^ c)).</summary>
    Right,

    /// <summary>No associativity (a ~ b ~ c is invalid).</summary>
    None
}
