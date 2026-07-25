namespace MathVerse.Math.Semantics.Resolution;

/// <summary>
/// Resolves identifier names to their corresponding symbols.
/// Handles variables, constants, parameters, functions, and namespace members.
/// </summary>
public sealed class IdentifierResolver
{
    private readonly BindingContext _context;

    /// <summary>Initializes an identifier resolver.</summary>
    public IdentifierResolver(BindingContext context)
    {
        _context = context;
    }

    /// <summary>Resolves a name to a bound expression.</summary>
    public BoundExpression ResolveIdentifier(string name)
    {
        var symbol = _context.SymbolTable.Lookup(name);
        if (symbol is null)
        {
            _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedVariable,
                $"Undefined symbol '{name}'.");
            return new BoundLiteralExpression(0.0);
        }

        return symbol switch
        {
            ConstantSymbol c => new BoundConstantExpression(c),
            VariableSymbol => new BoundVariableExpression(symbol),
            ParameterSymbol => new BoundVariableExpression(symbol),
            FunctionSymbol f => ResolveAsFunction(f),
            NamespaceSymbol ns => ResolveNamespaceMember(ns, name),
            _ => new BoundVariableExpression(symbol),
        };
    }

    /// <summary>Resolves a function by name, reporting an error if not found.</summary>
    public FunctionSymbol? ResolveFunction(string name)
    {
        var symbol = _context.SymbolTable.Lookup(name);
        if (symbol is FunctionSymbol func)
            return func;

        _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedFunction,
            $"Undefined function '{name}'.");
        return null;
    }

    /// <summary>Resolves a dotted name (e.g., std.g).</summary>
    public BoundExpression ResolveQualifiedName(string qualifiedName)
    {
        var parts = qualifiedName.Split('.');
        if (parts.Length == 1)
            return ResolveIdentifier(parts[0]);

        var nsSymbol = _context.SymbolTable.LookupGlobal(parts[0]);
        if (nsSymbol is not NamespaceSymbol ns)
        {
            _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedNamespace,
                $"Namespace '{parts[0]}' not defined.");
            return new BoundLiteralExpression(0.0);
        }

        for (int i = 1; i < parts.Length - 1; i++)
        {
            if (ns.Members.TryGetValue(parts[i], out var member) && member is NamespaceSymbol nextNs)
                ns = nextNs;
            else
            {
                _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedNamespace,
                    $"Namespace '{parts[i]}' not defined in '{ns.Name}'.");
                return new BoundLiteralExpression(0.0);
            }
        }

        var leaf = parts[^1];
        if (ns.Members.TryGetValue(leaf, out var leafSym)
            && leafSym is ConstantSymbol leafConst)
            return new BoundConstantExpression(leafConst);

        _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedMember,
            $"Member '{leaf}' not found in namespace '{ns.Name}'.");
        return new BoundLiteralExpression(0.0);
    }

    private BoundExpression ResolveAsFunction(FunctionSymbol func)
    {
        if (func.ParameterCount == 0)
            return new BoundFunctionCallExpression(func, []);
        return new BoundFunctionCallExpression(func,
            Enumerable.Range(0, func.ParameterCount)
                .Select(i => new BoundVariableExpression(new ParameterSymbol($"_", i)) as BoundExpression)
                .ToList());
    }

    private BoundExpression ResolveNamespaceMember(NamespaceSymbol ns, string name)
    {
        _context.Diagnostics.ReportInfo(SemanticDiagnosticCode.NotImplemented,
            $"Namespace '{name}' used as value — did you mean to access a member?");
        return new BoundLiteralExpression(0.0);
    }
}
