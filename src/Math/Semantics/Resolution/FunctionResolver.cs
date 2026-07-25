namespace MathVerse.Math.Semantics.Resolution;

/// <summary>
/// Resolves function calls to their declared function symbols.
/// Validates arity and reports mismatches.
/// </summary>
public sealed class FunctionResolver
{
    private readonly BindingContext _context;

    /// <summary>Initializes a function resolver.</summary>
    public FunctionResolver(BindingContext context)
    {
        _context = context;
    }

    /// <summary>Resolves a function call and validates arity.</summary>
    public FunctionSymbol? Resolve(string name, int argumentCount)
    {
        var symbol = _context.SymbolTable.Lookup(name);
        if (symbol is not FunctionSymbol func)
        {
            _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedFunction,
                $"Undefined function '{name}'.");
            return null;
        }

        if (argumentCount < func.ParameterCount)
        {
            _context.Diagnostics.ReportError(SemanticDiagnosticCode.TooFewArguments,
                $"Function '{name}' expects {func.ParameterCount} arguments, got {argumentCount}.");
        }
        else if (argumentCount > func.ParameterCount && func.ParameterCount > 0)
        {
            _context.Diagnostics.ReportWarning(SemanticDiagnosticCode.TooManyArguments,
                $"Function '{name}' expects {func.ParameterCount} arguments, got {argumentCount}.");
        }

        return func;
    }

    /// <summary>Gets the expected parameter count for a function, or -1 if not found.</summary>
    public int GetExpectedParameterCount(string name)
    {
        var symbol = _context.SymbolTable.Lookup(name);
        return symbol is FunctionSymbol func ? func.ParameterCount : -1;
    }
}
