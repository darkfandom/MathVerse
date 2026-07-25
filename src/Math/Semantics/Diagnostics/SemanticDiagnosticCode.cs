namespace MathVerse.Math.Semantics.Diagnostics;

/// <summary>Well-known semantic diagnostic codes.</summary>
public enum SemanticDiagnosticCode
{
    // Resolution (1xxx)

    /// <summary>Variable name could not be resolved.</summary>
    UndefinedVariable = 1001,
    /// <summary>Function name could not be resolved.</summary>
    UndefinedFunction = 1002,
    /// <summary>Symbol name could not be resolved.</summary>
    UndefinedSymbol = 1003,
    /// <summary>Namespace could not be resolved.</summary>
    UndefinedNamespace = 1004,
    /// <summary>Member of namespace could not be resolved.</summary>
    UndefinedMember = 1005,

    // Arity (2xxx)

    /// <summary>Argument count does not match expected arity.</summary>
    ArgumentCountMismatch = 2001,
    /// <summary>Too few arguments provided.</summary>
    TooFewArguments = 2002,
    /// <summary>Too many arguments provided.</summary>
    TooManyArguments = 2003,

    // Type (3xxx)

    /// <summary>Type mismatch between operands.</summary>
    TypeMismatch = 3001,
    /// <summary>Operator cannot be applied to the given operands.</summary>
    CannotApplyOperator = 3002,
    /// <summary>Argument type does not match parameter type.</summary>
    ArgumentTypeMismatch = 3003,

    // Binding (4xxx)

    /// <summary>Cannot assign to a constant or non-mutable symbol.</summary>
    CannotAssignToConstant = 4001,
    /// <summary>Cannot assign to an r-value or expression.</summary>
    CannotAssignToRValue = 4002,
    /// <summary>Expression is not callable.</summary>
    ExpressionNotCallable = 4003,

    // Semantic (5xxx)

    /// <summary>Constant folding failed.</summary>
    ConstantFoldingFailed = 5001,
    /// <summary>Circular dependency detected between symbols.</summary>
    CircularDependency = 5002,
    /// <summary>Symbol declared more than once.</summary>
    DuplicateSymbolDeclaration = 5003,
    /// <summary>Invalid literal value.</summary>
    InvalidLiteral = 5004,
    /// <summary>Division by zero detected.</summary>
    DivisionByZero = 5005,

    // General (9xxx)

    /// <summary>Internal compiler error.</summary>
    InternalError = 9001,
    /// <summary>Feature not yet implemented.</summary>
    NotImplemented = 9002,
}
