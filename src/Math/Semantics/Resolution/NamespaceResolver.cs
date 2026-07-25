namespace MathVerse.Math.Semantics.Resolution;

/// <summary>
/// Resolves namespace-qualified names (e.g., std.g, std.c).
/// </summary>
public sealed class NamespaceResolver
{
    private readonly BindingContext _context;

    /// <summary>Initializes a namespace resolver.</summary>
    public NamespaceResolver(BindingContext context)
    {
        _context = context;
    }

    /// <summary>Resolves a qualified name to a symbol.</summary>
    public Symbol? Resolve(params string[] parts)
    {
        if (parts.Length == 0) return null;

        var first = _context.SymbolTable.LookupGlobal(parts[0]);
        if (first is not NamespaceSymbol ns)
        {
            _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedNamespace,
                $"'{parts[0]}' is not a namespace.");
            return null;
        }

        for (int i = 1; i < parts.Length; i++)
        {
            if (ns.Members.TryGetValue(parts[i], out var member))
            {
                if (i == parts.Length - 1)
                    return member;
                if (member is NamespaceSymbol nextNs)
                    ns = nextNs;
                else
                {
                    _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedNamespace,
                        $"'{parts[i]}' is not a namespace in '{ns.Name}'.");
                    return null;
                }
            }
            else
            {
                _context.Diagnostics.ReportError(SemanticDiagnosticCode.UndefinedMember,
                    $"Member '{parts[i]}' not found in namespace '{ns.Name}'.");
                return null;
            }
        }
        return ns;
    }

    /// <summary>Lists all members of a namespace.</summary>
    public IReadOnlyList<string> ListMembers(string namespaceName)
    {
        var sym = _context.SymbolTable.LookupGlobal(namespaceName);
        if (sym is NamespaceSymbol ns)
            return ns.Members.Keys.ToList();
        return [];
    }
}
