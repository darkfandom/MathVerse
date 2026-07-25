namespace MathVerse.Math.Semantics.Symbols;

/// <summary>
/// Central symbol table that manages scopes and symbol declarations.
/// </summary>
public sealed class SymbolTable
{
    private readonly ScopeStack _scopes = new();
    private readonly List<Symbol> _allDeclared = [];
    private readonly NamespaceSymbol _globalNamespace;

    /// <summary>Initializes a symbol table pre-populated with built-ins.</summary>
    public SymbolTable()
    {
        _globalNamespace = new NamespaceSymbol(string.Empty);
        BuiltinRegistry.RegisterAll(this);
    }

    /// <summary>Gets the scope stack.</summary>
    public ScopeStack Scopes => _scopes;

    /// <summary>Gets all declared symbols.</summary>
    public IReadOnlyList<Symbol> AllDeclared => _allDeclared;

    /// <summary>Declares a symbol in the current scope.</summary>
    public bool Declare(Symbol symbol)
    {
        var ok = _scopes.Declare(symbol);
        if (ok) _allDeclared.Add(symbol);
        return ok;
    }

    /// <summary>Looks up a symbol through the scope chain.</summary>
    public Symbol? Lookup(string name) => _scopes.Lookup(name);

    /// <summary>Looks up a symbol in the current scope only.</summary>
    public Symbol? LookupLocal(string name) => _scopes.LookupLocal(name);

    /// <summary>Looks up a symbol in the global scope.</summary>
    public Symbol? LookupGlobal(string name) => _scopes.LookupGlobal(name);

    /// <summary>Enters a new scope.</summary>
    public SymbolScope EnterScope(ScopeKind kind) => _scopes.EnterScope(kind);

    /// <summary>Exits the current scope.</summary>
    public SymbolScope ExitScope() => _scopes.ExitScope();

    /// <summary>Checks if a symbol is declared as a constant.</summary>
    public bool IsConstant(string name) => _scopes.Lookup(name) is ConstantSymbol;

    /// <summary>Registers a built-in symbol directly.</summary>
    public void RegisterBuiltin(Symbol symbol) => _globalNamespace.Declare(symbol);
}
