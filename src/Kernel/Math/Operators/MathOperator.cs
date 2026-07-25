namespace MathVerse.Math.Operators;

/// <summary>
/// Represents a mathematical operator with its metadata.
/// </summary>
public sealed record MathOperator : IEquatable<MathOperator>
{
    /// <summary>Initializes a math operator.</summary>
    public MathOperator(
        string symbol,
        string name,
        OperatorCategory category,
        int arity,
        int precedence,
        OperatorAssociativity associativity = OperatorAssociativity.Left)
    {
        Symbol = symbol;
        Name = name;
        Category = category;
        Arity = arity;
        Precedence = precedence;
        Associativity = associativity;
    }

    /// <summary>Gets the operator symbol (e.g., "+", "*", "^").</summary>
    public string Symbol { get; }

    /// <summary>Gets the operator name (e.g., "Add", "Multiply", "Power").</summary>
    public string Name { get; }

    /// <summary>Gets the operator category.</summary>
    public OperatorCategory Category { get; }

    /// <summary>Gets the operator arity (1 for unary, 2 for binary).</summary>
    public int Arity { get; }

    /// <summary>Gets the operator precedence (higher binds tighter).</summary>
    public int Precedence { get; }

    /// <summary>Gets the operator associativity.</summary>
    public OperatorAssociativity Associativity { get; }

    /// <summary>Gets whether this is a unary operator.</summary>
    public bool IsUnary => Arity == 1;

    /// <summary>Gets whether this is a binary operator.</summary>
    public bool IsBinary => Arity == 2;

    /// <inheritdoc/>
    public bool Equals(MathOperator? other) =>
        other is not null && Symbol == other.Symbol && Name == other.Name;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Symbol, Name);

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    // ─── Arithmetic Operators ───

    /// <summary>Addition operator.</summary>
    public static readonly MathOperator Add = new("+", "Add", OperatorCategory.Arithmetic, 2, 1);

    /// <summary>Subtraction operator.</summary>
    public static readonly MathOperator Subtract = new("-", "Subtract", OperatorCategory.Arithmetic, 2, 1);

    /// <summary>Multiplication operator.</summary>
    public static readonly MathOperator Multiply = new("*", "Multiply", OperatorCategory.Arithmetic, 2, 2);

    /// <summary>Division operator.</summary>
    public static readonly MathOperator Divide = new("/", "Divide", OperatorCategory.Arithmetic, 2, 2);

    /// <summary>Modulo operator.</summary>
    public static readonly MathOperator Modulo = new("%", "Modulo", OperatorCategory.Arithmetic, 2, 2);

    /// <summary>Power operator.</summary>
    public static readonly MathOperator Power = new("^", "Power", OperatorCategory.Arithmetic, 2, 3, OperatorAssociativity.Right);

    /// <summary>Negation operator.</summary>
    public static readonly MathOperator Negate = new("-", "Negate", OperatorCategory.Arithmetic, 1, 4);

    /// <summary>Absolute value operator.</summary>
    public static readonly MathOperator Abs = new("|·|", "Abs", OperatorCategory.Arithmetic, 1, 4);

    // ─── Relational Operators ───

    /// <summary>Equal to operator.</summary>
    public static readonly MathOperator Equal = new("==", "Equal", OperatorCategory.Relational, 2, 0);

    /// <summary>Not equal to operator.</summary>
    public static readonly MathOperator NotEqual = new("!=", "NotEqual", OperatorCategory.Relational, 2, 0);

    /// <summary>Less than operator.</summary>
    public static readonly MathOperator LessThan = new("<", "LessThan", OperatorCategory.Relational, 2, 0);

    /// <summary>Greater than operator.</summary>
    public static readonly MathOperator GreaterThan = new(">", "GreaterThan", OperatorCategory.Relational, 2, 0);

    /// <summary>Less than or equal operator.</summary>
    public static readonly MathOperator LessThanOrEqual = new("<=", "LessThanOrEqual", OperatorCategory.Relational, 2, 0);

    /// <summary>Greater than or equal operator.</summary>
    public static readonly MathOperator GreaterThanOrEqual = new(">=", "GreaterThanOrEqual", OperatorCategory.Relational, 2, 0);

    // ─── Logical Operators ───

    /// <summary>Logical AND operator.</summary>
    public static readonly MathOperator And = new("∧", "And", OperatorCategory.Logical, 2, 0);

    /// <summary>Logical OR operator.</summary>
    public static readonly MathOperator Or = new("∨", "Or", OperatorCategory.Logical, 2, 0);

    /// <summary>Logical NOT operator.</summary>
    public static readonly MathOperator Not = new("¬", "Not", OperatorCategory.Logical, 1, 4);

    /// <summary>Exclusive OR operator.</summary>
    public static readonly MathOperator Xor = new("⊕", "Xor", OperatorCategory.Logical, 2, 0);

    /// <summary>Logical implication operator.</summary>
    public static readonly MathOperator Implies = new("⇒", "Implies", OperatorCategory.Logical, 2, 0);

    /// <summary>Logical equivalence operator.</summary>
    public static readonly MathOperator Equivalent = new("⇔", "Equivalent", OperatorCategory.Logical, 2, 0);

    // ─── Set Operators ───

    /// <summary>Set union operator.</summary>
    public static readonly MathOperator Union = new("∪", "Union", OperatorCategory.Set, 2, 1);

    /// <summary>Set intersection operator.</summary>
    public static readonly MathOperator Intersection = new("∩", "Intersection", OperatorCategory.Set, 2, 2);

    /// <summary>Set difference operator.</summary>
    public static readonly MathOperator SetDifference = new("\\", "SetDifference", OperatorCategory.Set, 2, 2);

    /// <summary>Element of operator.</summary>
    public static readonly MathOperator ElementOf = new("∈", "ElementOf", OperatorCategory.Set, 2, 0);

    /// <summary>Subset operator.</summary>
    public static readonly MathOperator Subset = new("⊂", "Subset", OperatorCategory.Set, 2, 0);

    /// <summary>Proper subset operator.</summary>
    public static readonly MathOperator ProperSubset = new("⊊", "ProperSubset", OperatorCategory.Set, 2, 0);

    /// <summary>Superset operator.</summary>
    public static readonly MathOperator Superset = new("⊃", "Superset", OperatorCategory.Set, 2, 0);

    // ─── Matrix Operators ───

    /// <summary>Matrix transpose operator.</summary>
    public static readonly MathOperator Transpose = new("ᵀ", "Transpose", OperatorCategory.Matrix, 1, 4);

    /// <summary>Matrix inverse operator.</summary>
    public static readonly MathOperator Inverse = new("⁻¹", "Inverse", OperatorCategory.Matrix, 1, 4);

    /// <summary>Matrix determinant operator.</summary>
    public static readonly MathOperator Determinant = new("det", "Determinant", OperatorCategory.Matrix, 1, 4);

    /// <summary>Matrix dot product operator.</summary>
    public static readonly MathOperator Dot = new("·", "Dot", OperatorCategory.Matrix, 2, 2);

    /// <summary>Matrix cross product operator.</summary>
    public static readonly MathOperator Cross = new("×", "Cross", OperatorCategory.Matrix, 2, 2);

    /// <summary>Matrix Kronecker product operator.</summary>
    public static readonly MathOperator Kronecker = new("⊗", "Kronecker", OperatorCategory.Matrix, 2, 2);

    // ─── Calculus Operators ───

    /// <summary>Differential operator.</summary>
    public static readonly MathOperator Differential = new("d", "Differential", OperatorCategory.Calculus, 1, 4);

    /// <summary>Partial differential operator.</summary>
    public static readonly MathOperator Partial = new("∂", "Partial", OperatorCategory.Calculus, 1, 4);

    /// <summary>Gradient operator.</summary>
    public static readonly MathOperator Gradient = new("∇", "Gradient", OperatorCategory.Calculus, 1, 4);

    // ─── Functional Operators ───

    /// <summary>Function composition operator.</summary>
    public static readonly MathOperator Compose = new("∘", "Compose", OperatorCategory.Functional, 2, 2);

    /// <summary>Function application operator.</summary>
    public static readonly MathOperator Apply = new("$", "Apply", OperatorCategory.Functional, 2, 1);

    // ─── Assignment Operators ───

    /// <summary>Assignment operator.</summary>
    public static readonly MathOperator Assign = new("=", "Assign", OperatorCategory.Assignment, 2, 0, OperatorAssociativity.Right);

    /// <summary>Add-assign operator.</summary>
    public static readonly MathOperator AddAssign = new("+=", "AddAssign", OperatorCategory.Assignment, 2, 0, OperatorAssociativity.Right);

    /// <summary>Multiply-assign operator.</summary>
    public static readonly MathOperator MultiplyAssign = new("*=", "MultiplyAssign", OperatorCategory.Assignment, 2, 0, OperatorAssociativity.Right);
}
