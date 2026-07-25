namespace MathVerse.Math.Semantics.Binding;

/// <summary>
/// Context for a binding operation, carrying the symbol table and diagnostics.
/// </summary>
public sealed class BindingContext
{
    /// <summary>Initializes a binding context.</summary>
    public BindingContext(SymbolTable symbolTable, SemanticDiagnosticBag diagnostics)
    {
        SymbolTable = symbolTable;
        Diagnostics = diagnostics;
    }

    /// <summary>Gets the symbol table.</summary>
    public SymbolTable SymbolTable { get; }

    /// <summary>Gets the diagnostics bag.</summary>
    public SemanticDiagnosticBag Diagnostics { get; }
}

/// <summary>
/// Result of a binding operation.
/// </summary>
/// <param name="Expression">The bound expression tree.</param>
/// <param name="Diagnostics">Diagnostics produced during binding.</param>
public sealed record BindingResult(
    BoundExpression Expression,
    SemanticDiagnosticBag Diagnostics)
{
    /// <summary>Gets whether binding succeeded without errors.</summary>
    public bool Success => !Diagnostics.HasErrors;
}
