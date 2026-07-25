namespace MathVerse.Math.Semantics.Resolution;

/// <summary>
/// Records a reference to a symbol at a specific location.
/// </summary>
/// <param name="Symbol">The referenced symbol.</param>
/// <param name="Location">Source location description.</param>
/// <param name="IsWrite">Whether this is a write (assignment) reference.</param>
public sealed record SymbolReference(Symbol Symbol, string? Location, bool IsWrite = false);

/// <summary>
/// Tracks all symbol references across the analyzed expression(s).
/// Provides read/write tracking, definition sites, and reference counts.
/// </summary>
public sealed class ReferenceGraph
{
    private readonly List<SymbolReference> _references = [];
    private readonly Dictionary<string, string?> _definitionSites = new(StringComparer.Ordinal);

    /// <summary>Records a reference to a symbol.</summary>
    public void AddReference(Symbol symbol, string? location, bool isWrite = false)
    {
        _references.Add(new SymbolReference(symbol, location, isWrite));
    }

    /// <summary>Records the definition site of a symbol.</summary>
    public void RecordDefinition(Symbol symbol, string? location)
    {
        _definitionSites[symbol.Name] = location;
    }

    /// <summary>Gets all references to a symbol.</summary>
    public IReadOnlyList<SymbolReference> GetReferences(Symbol symbol) =>
        _references.Where(r => r.Symbol.Equals(symbol)).ToList();

    /// <summary>Gets all references to a symbol by name.</summary>
    public IReadOnlyList<SymbolReference> GetReferences(string name) =>
        _references.Where(r => r.Symbol.Name == name).ToList();

    /// <summary>Gets all write references to a symbol.</summary>
    public IReadOnlyList<SymbolReference> GetWriteReferences(Symbol symbol) =>
        _references.Where(r => r.Symbol.Equals(symbol) && r.IsWrite).ToList();

    /// <summary>Gets all read references to a symbol.</summary>
    public IReadOnlyList<SymbolReference> GetReadReferences(Symbol symbol) =>
        _references.Where(r => r.Symbol.Equals(symbol) && !r.IsWrite).ToList();

    /// <summary>Gets the total reference count for a symbol.</summary>
    public int GetReferenceCount(Symbol symbol) =>
        _references.Count(r => r.Symbol.Equals(symbol));

    /// <summary>Gets whether a symbol is referenced at all.</summary>
    public bool IsReferenced(Symbol symbol) =>
        _references.Any(r => r.Symbol.Equals(symbol));

    /// <summary>Gets the definition site for a symbol, if known.</summary>
    public string? GetDefinitionSite(string symbolName) =>
        _definitionSites.TryGetValue(symbolName, out var site) ? site : null;

    /// <summary>Gets all tracked references.</summary>
    public IReadOnlyList<SymbolReference> AllReferences => _references;

    /// <summary>Gets the count of all references.</summary>
    public int Count => _references.Count;

    /// <summary>Gets whether the symbol is only read (never written).</summary>
    public bool IsReadOnly(Symbol symbol) =>
        !_references.Any(r => r.Symbol.Equals(symbol) && r.IsWrite);

    /// <summary>Gets symbols that are referenced but never written.</summary>
    public IReadOnlyList<string> GetNeverWrittenSymbols() =>
        _references
            .Where(r => !r.IsWrite)
            .Select(r => r.Symbol.Name)
            .Distinct()
            .Where(name => !_references.Any(r => r.Symbol.Name == name && r.IsWrite))
            .ToList();
}
