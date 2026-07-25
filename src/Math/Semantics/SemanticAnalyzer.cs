namespace MathVerse.Math.Semantics;

/// <summary>
/// Public entry point for semantic analysis. Orchestrates parsing, binding,
/// constant folding, reference tracking, and circular dependency detection.
/// </summary>
public sealed class SemanticAnalyzer
{
    private static readonly SyntaxToExpressionConverter s_converter = new();

    /// <summary>Analyzes a mathematical expression string end-to-end.</summary>
    public SemanticModel Analyze(string input)
    {
        var diagnostics = new SemanticDiagnosticBag();
        var symbolTable = new SymbolTable();
        var context = new BindingContext(symbolTable, diagnostics);

        var parserResult = Parser.Parse(input);
        var expression = s_converter.ConvertSyntaxTree(parserResult.SyntaxTree);

        var binder = new Binder(context);
        var bindingResult = binder.Bind(expression);

        var referenceGraph = new ReferenceGraph();
        var dependencyGraph = new DependencyGraph();
        TrackReferences(bindingResult.Expression, referenceGraph);

        return new SemanticModel(
            bindingResult.Expression,
            symbolTable,
            bindingResult.Diagnostics,
            referenceGraph,
            dependencyGraph);
    }

    /// <summary>Analyzes a pre-parsed expression.</summary>
    public SemanticModel AnalyzeExpression(Expression expression, SemanticDiagnosticBag? diagnostics = null)
    {
        diagnostics ??= new SemanticDiagnosticBag();
        var symbolTable = new SymbolTable();
        var context = new BindingContext(symbolTable, diagnostics);

        var binder = new Binder(context);
        var bindingResult = binder.Bind(expression);

        var referenceGraph = new ReferenceGraph();
        TrackReferences(bindingResult.Expression, referenceGraph);

        return new SemanticModel(
            bindingResult.Expression,
            symbolTable,
            bindingResult.Diagnostics,
            referenceGraph,
            new DependencyGraph());
    }

    /// <summary>Analyzes with custom symbol pre-registration.</summary>
    public SemanticModel Analyze(string input, Action<SymbolTable> configureSymbols)
    {
        var diagnostics = new SemanticDiagnosticBag();
        var symbolTable = new SymbolTable();
        configureSymbols(symbolTable);
        var context = new BindingContext(symbolTable, diagnostics);

        var parserResult = Parser.Parse(input);
        var expression = s_converter.ConvertSyntaxTree(parserResult.SyntaxTree);

        var binder = new Binder(context);
        var bindingResult = binder.Bind(expression);

        var referenceGraph = new ReferenceGraph();
        TrackReferences(bindingResult.Expression, referenceGraph);

        return new SemanticModel(
            bindingResult.Expression,
            symbolTable,
            bindingResult.Diagnostics,
            referenceGraph,
            new DependencyGraph());
    }

    private static void TrackReferences(BoundExpression expr, ReferenceGraph graph)
    {
        switch (expr)
        {
            case BoundLiteralExpression:
                break;
            case BoundConstantExpression c:
                graph.AddReference(c.Constant, $"constant:{c.Constant.Name}");
                break;
            case BoundVariableExpression v:
                graph.AddReference(v.Symbol, $"variable:{v.Symbol.Name}");
                break;
            case BoundBinaryExpression b:
                TrackReferences(b.Left, graph);
                TrackReferences(b.Right, graph);
                break;
            case BoundUnaryExpression u:
                TrackReferences(u.Operand, graph);
                break;
            case BoundFunctionCallExpression f:
                graph.AddReference(f.Function, $"call:{f.Function.Name}");
                foreach (var arg in f.Arguments)
                    TrackReferences(arg, graph);
                break;
            case BoundAssignmentExpression a:
                graph.AddReference(a.Target, $"assign:{a.Target.Name}", isWrite: true);
                TrackReferences(a.Value, graph);
                break;
        }
    }
}
