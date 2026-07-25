namespace MathVerse.Math.Semantics.Symbols;

/// <summary>
/// Represents a lexical scope containing symbol declarations.
/// </summary>
public sealed class SymbolScope
{
    private readonly Dictionary<string, Symbol> _symbols = new(StringComparer.Ordinal);

    /// <summary>Initializes a symbol scope.</summary>
    public SymbolScope(ScopeKind kind, SymbolScope? parent = null)
    {
        Kind = kind;
        Parent = parent;
    }

    /// <summary>Gets the scope kind.</summary>
    public ScopeKind Kind { get; }

    /// <summary>Gets the parent scope (null for global).</summary>
    public SymbolScope? Parent { get; }

    /// <summary>Declares a symbol in this scope.</summary>
    public bool Declare(Symbol symbol)
    {
        return _symbols.TryAdd(symbol.Name, symbol);
    }

    /// <summary>Looks up a symbol in this scope only.</summary>
    public Symbol? LookupLocal(string name) =>
        _symbols.TryGetValue(name, out var sym) ? sym : null;

    /// <summary>Looks up a symbol in this scope or any parent scope.</summary>
    public Symbol? Lookup(string name)
    {
        var current = this;
        while (current is not null)
        {
            if (current._symbols.TryGetValue(name, out var sym))
                return sym;
            current = current.Parent;
        }
        return null;
    }

    /// <summary>Gets all symbols declared in this scope.</summary>
    public IReadOnlyDictionary<string, Symbol> Symbols => _symbols;

    /// <summary>Gets whether this scope contains the named symbol locally.</summary>
    public bool ContainsLocal(string name) => _symbols.ContainsKey(name);
}
