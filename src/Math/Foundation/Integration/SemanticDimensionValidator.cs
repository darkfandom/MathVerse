using MathVerse.Math.Foundation.Analysis;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Expressions;

namespace MathVerse.Math.Foundation.Integration;

public static class SemanticDimensionValidator
{
    public static IReadOnlyList<DimensionDiagnostic> ValidateSemanticTree(Expression expr, Dictionary<string, Dimension> knownDimensions)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        if (knownDimensions is null) throw new ArgumentNullException(nameof(knownDimensions));

        var analyzer = DimensionAnalyzer.Instance;
        analyzer.Clear();

        foreach (var kvp in knownDimensions)
            analyzer.SetVariableDimension(kvp.Key, kvp.Value);

        analyzer.AnalyzeExpression(expr);
        return analyzer.Diagnostics.Diagnostics;
    }

    public static Dimension? InferFromSemanticContext(string operation, Dimension[] argumentDimensions)
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));
        if (argumentDimensions is null) throw new ArgumentNullException(nameof(argumentDimensions));

        return DimensionInferenceEngine.InferFromContext(operation, argumentDimensions);
    }
}
