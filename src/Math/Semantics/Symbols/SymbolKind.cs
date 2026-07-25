namespace MathVerse.Math.Semantics.Symbols;

/// <summary>Categorizes the different kinds of symbols.</summary>
public enum SymbolKind
{
    /// <summary>A variable symbol.</summary>
    Variable,
    /// <summary>A function symbol.</summary>
    Function,
    /// <summary>A named constant.</summary>
    Constant,
    /// <summary>A function parameter.</summary>
    Parameter,
    /// <summary>A namespace.</summary>
    Namespace,
    /// <summary>A type.</summary>
    Type,
    /// <summary>An operator.</summary>
    Operator,
}
