namespace MathVerse.Math.Semantics;

/// <summary>
/// Central representation of the semantic analysis results.
/// Provides access to the bound tree, diagnostics, symbol table, reference graph, and constant folding.
/// </summary>
public sealed class SemanticModel
{
    private readonly ConstantFolder _folder;

    /// <summary>Initializes a semantic model.</summary>
    public SemanticModel(
        BoundExpression boundTree,
        SymbolTable symbolTable,
        SemanticDiagnosticBag diagnostics,
        ReferenceGraph referenceGraph,
        DependencyGraph dependencyGraph)
    {
        BoundTree = boundTree;
        SymbolTable = symbolTable;
        Diagnostics = diagnostics;
        ReferenceGraph = referenceGraph;
        DependencyGraph = dependencyGraph;
        _folder = new ConstantFolder(diagnostics);
    }

    /// <summary>Gets the root bound expression.</summary>
    public BoundExpression BoundTree { get; }

    /// <summary>Gets the symbol table with all declarations.</summary>
    public SymbolTable SymbolTable { get; }

    /// <summary>Gets all semantic diagnostics.</summary>
    public SemanticDiagnosticBag Diagnostics { get; }

    /// <summary>Gets the reference graph.</summary>
    public ReferenceGraph ReferenceGraph { get; }

    /// <summary>Gets the dependency graph.</summary>
    public DependencyGraph DependencyGraph { get; }

    /// <summary>Gets whether analysis completed without errors.</summary>
    public bool Success => !Diagnostics.HasErrors;

    /// <summary>Gets the number of bound nodes in the tree.</summary>
    public int NodeCount => CountNodes(BoundTree);

    /// <summary>Gets the number of symbol references.</summary>
    public int ReferenceCount => ReferenceGraph.Count;

    /// <summary>Gets the number of declared symbols.</summary>
    public int SymbolCount => SymbolTable.AllDeclared.Count;

    /// <summary>Attempts to evaluate the expression as a constant.</summary>
    public double? EvaluateConstant() => _folder.TryFold(BoundTree);

    /// <summary>Folds constants in the expression tree.</summary>
    public BoundExpression FoldConstants() => _folder.Fold(BoundTree);

    /// <summary>Checks whether a symbol is referenced in the bound tree.</summary>
    public bool IsSymbolUsed(string name)
    {
        var sym = SymbolTable.Lookup(name);
        return sym is not null && ReferenceGraph.IsReferenced(sym);
    }

    /// <summary>Gets diagnostics at a specific severity.</summary>
    public IReadOnlyList<SemanticDiagnostic> GetDiagnostics(SemanticSeverity severity) =>
        Diagnostics.GetBySeverity(severity);

    private static int CountNodes(BoundExpression expr)
    {
        return expr switch
        {
            BoundBinaryExpression b => 1 + CountNodes(b.Left) + CountNodes(b.Right),
            BoundUnaryExpression u => 1 + CountNodes(u.Operand),
            BoundFunctionCallExpression f => 1 + f.Arguments.Sum(CountNodes),
            BoundAssignmentExpression a => 1 + CountNodes(a.Value),
            _ => 1,
        };
    }
}
