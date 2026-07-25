namespace MathVerse.Math.Types;

/// <summary>Enumerates all mathematical type kinds in the system.</summary>
public enum TypeKind
{
    /// <summary>Unknown type (not yet inferred).</summary>
    Unknown = 0,

    /// <summary>Error type (recovery from type errors).</summary>
    Error = 1,

    /// <summary>Unit type (void equivalent).</summary>
    Unit = 2,

    /// <summary>Boolean type.</summary>
    Boolean = 10,

    /// <summary>Integer type.</summary>
    Integer = 20,

    /// <summary>Rational number type.</summary>
    Rational = 21,

    /// <summary>Real number type.</summary>
    Real = 22,

    /// <summary>Complex number type.</summary>
    Complex = 23,

    /// <summary>String type.</summary>
    String = 30,

    /// <summary>Scalar type (supertype of numeric scalars).</summary>
    Scalar = 40,

    /// <summary>Function type.</summary>
    Function = 50,

    /// <summary>Vector type.</summary>
    Vector = 60,

    /// <summary>Matrix type.</summary>
    Matrix = 61,

    /// <summary>Tensor type (rank &gt; 2).</summary>
    Tensor = 62,

    /// <summary>Polynomial type.</summary>
    Polynomial = 70,

    /// <summary>Equation type.</summary>
    Equation = 71,

    /// <summary>Set type.</summary>
    Set = 80,

    /// <summary>Tuple type.</summary>
    Tuple = 90,

    /// <summary>Sequence type.</summary>
    Sequence = 91,

    /// <summary>Domain type.</summary>
    Domain = 100,

    /// <summary>Generic (open) type parameter.</summary>
    Generic = 110,

    /// <summary>Tuple of named or positional types.</summary>
    Record = 120,
}
