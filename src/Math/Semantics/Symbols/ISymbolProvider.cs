namespace MathVerse.Math.Semantics.Symbols;

/// <summary>
/// Interface for providing symbols to the semantic analysis pipeline.
/// </summary>
public interface ISymbolProvider
{
    /// <summary>Gets the name of this provider.</summary>
    string Name { get; }

    /// <summary>Populates a namespace with built-in symbols.</summary>
    void Register(NamespaceSymbol target);
}
