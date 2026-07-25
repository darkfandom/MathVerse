namespace MathVerse.Math.Semantics.Symbols;

/// <summary>
/// Abstract base class for all symbols in the semantic model.
/// </summary>
public abstract class Symbol
{
    /// <summary>Initializes a symbol.</summary>
    protected Symbol(string name, SymbolKind kind)
    {
        Name = name;
        Kind = kind;
    }

    /// <summary>Gets the symbol name.</summary>
    public string Name { get; }

    /// <summary>Gets the symbol kind.</summary>
    public SymbolKind Kind { get; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Symbol other && Kind == other.Kind && Name == other.Name;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, Name);

    /// <inheritdoc/>
    public override string ToString() => $"{Kind} '{Name}'";
}

/// <summary>Represents a variable symbol.</summary>
public sealed class VariableSymbol : Symbol
{
    /// <summary>Initializes a variable symbol.</summary>
    public VariableSymbol(string name, bool isMutable = true)
        : base(name, SymbolKind.Variable)
    {
        IsMutable = isMutable;
    }

    /// <summary>Gets whether the variable can be reassigned.</summary>
    public bool IsMutable { get; }
}

/// <summary>Represents a function symbol.</summary>
public sealed class FunctionSymbol : Symbol
{
    /// <summary>Initializes a function symbol.</summary>
    public FunctionSymbol(string name, IReadOnlyList<ParameterSymbol> parameters, Expression? body = null)
        : base(name, SymbolKind.Function)
    {
        Parameters = parameters;
        Body = body;
    }

    /// <summary>Gets the function parameters.</summary>
    public IReadOnlyList<ParameterSymbol> Parameters { get; }

    /// <summary>Gets the optional function body (null for built-ins).</summary>
    public Expression? Body { get; }

    /// <summary>Gets the parameter count.</summary>
    public int ParameterCount => Parameters.Count;
}

/// <summary>Represents a named constant.</summary>
public sealed class ConstantSymbol : Symbol
{
    /// <summary>Initializes a constant symbol.</summary>
    public ConstantSymbol(string name, double value)
        : base(name, SymbolKind.Constant)
    {
        Value = value;
    }

    /// <summary>Gets the constant value.</summary>
    public double Value { get; }
}

/// <summary>Represents a function parameter.</summary>
public sealed class ParameterSymbol : Symbol
{
    /// <summary>Initializes a parameter symbol.</summary>
    public ParameterSymbol(string name, int ordinal)
        : base(name, SymbolKind.Parameter)
    {
        Ordinal = ordinal;
    }

    /// <summary>Gets the parameter ordinal (0-based position).</summary>
    public int Ordinal { get; }
}

/// <summary>Represents a namespace.</summary>
public sealed class NamespaceSymbol : Symbol
{
    /// <summary>Initializes a namespace symbol.</summary>
    public NamespaceSymbol(string name)
        : base(name, SymbolKind.Namespace)
    {
        Members = new Dictionary<string, Symbol>();
    }

    /// <summary>Gets the member symbols in this namespace.</summary>
    public Dictionary<string, Symbol> Members { get; }

    /// <summary>Declares a symbol in this namespace.</summary>
    public void Declare(Symbol symbol) => Members[symbol.Name] = symbol;
}

/// <summary>Represents a type symbol.</summary>
public sealed class TypeSymbol : Symbol
{
    /// <summary>Initializes a type symbol.</summary>
    public TypeSymbol(string name)
        : base(name, SymbolKind.Type) { }
}
