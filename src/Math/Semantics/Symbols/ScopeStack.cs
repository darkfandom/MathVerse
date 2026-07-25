namespace MathVerse.Math.Semantics.Symbols;

/// <summary>
/// Manages a stack of nested lexical scopes.
/// </summary>
public sealed class ScopeStack
{
    private readonly Stack<SymbolScope> _scopeStack = new();

    /// <summary>Initializes a scope stack with a global scope.</summary>
    public ScopeStack()
    {
        GlobalScope = new SymbolScope(ScopeKind.Global);
        _scopeStack.Push(GlobalScope);
    }

    /// <summary>Gets the global scope.</summary>
    public SymbolScope GlobalScope { get; }

    /// <summary>Gets the current (innermost) scope.</summary>
    public SymbolScope CurrentScope => _scopeStack.Peek();

    /// <summary>Gets the current nesting depth.</summary>
    public int Depth => _scopeStack.Count;

    /// <summary>Enters a new nested scope.</summary>
    public SymbolScope EnterScope(ScopeKind kind)
    {
        var scope = new SymbolScope(kind, CurrentScope);
        _scopeStack.Push(scope);
        return scope;
    }

    /// <summary>Exits the current scope and returns it.</summary>
    public SymbolScope ExitScope()
    {
        if (_scopeStack.Count <= 1)
            throw new InvalidOperationException("Cannot exit the global scope.");
        return _scopeStack.Pop();
    }

    /// <summary>Looks up a symbol through the scope chain.</summary>
    public Symbol? Lookup(string name) => CurrentScope.Lookup(name);

    /// <summary>Looks up a symbol in the current scope only.</summary>
    public Symbol? LookupLocal(string name) => CurrentScope.LookupLocal(name);

    /// <summary>Looks up a symbol in the global scope.</summary>
    public Symbol? LookupGlobal(string name) => GlobalScope.LookupLocal(name);

    /// <summary>Declares a symbol in the current scope.</summary>
    public bool Declare(Symbol symbol) => CurrentScope.Declare(symbol);
}
