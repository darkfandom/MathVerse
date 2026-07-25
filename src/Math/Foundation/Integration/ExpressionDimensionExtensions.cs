using MathVerse.Math.Foundation.Analysis;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Foundation.Quantities;
using MathVerse.Math.Expressions;

namespace MathVerse.Math.Foundation.Integration;

public static class ExpressionDimensionExtensions
{
    public static Dimension GetDimension(this Expression expr, DimensionAnalyzer analyzer)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        if (analyzer is null) throw new ArgumentNullException(nameof(analyzer));
        return analyzer.AnalyzeExpression(expr);
    }

    public static bool IsDimensionallyConsistent(this Expression expr, DimensionAnalyzer analyzer)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        if (analyzer is null) throw new ArgumentNullException(nameof(analyzer));
        analyzer.CheckDimensionalConsistency(expr);
        return !analyzer.Diagnostics.HasErrors;
    }

    public static PhysicalQuantity? EvaluateAsQuantity(this Expression expr, Dictionary<string, PhysicalQuantity> variableValues)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        if (variableValues is null) throw new ArgumentNullException(nameof(variableValues));

        return expr switch
        {
            LiteralExpression lit => new PhysicalQuantity { Value = lit.Value, Dimension = Dimension.None },
            VariableExpression v => variableValues.TryGetValue(v.Name, out var pq) ? pq : null,
            ConstantExpression => new PhysicalQuantity { Value = 0.0, Dimension = Dimension.None },
            BinaryExpression b => EvaluateBinaryAsQuantity(b, variableValues),
            UnaryExpression u => EvaluateUnaryAsQuantity(u, variableValues),
            FunctionCallExpression f => EvaluateFunctionAsQuantity(f, variableValues),
            _ => null
        };
    }

    private static PhysicalQuantity? EvaluateBinaryAsQuantity(BinaryExpression binary, Dictionary<string, PhysicalQuantity> variableValues)
    {
        var left = EvaluateAsQuantity(binary.Left, variableValues);
        var right = EvaluateAsQuantity(binary.Right, variableValues);
        if (left is null || right is null) return null;

        return binary.Operator switch
        {
            var op when op == MathOperator.Add => left + right,
            var op when op == MathOperator.Subtract => left - right,
            var op when op == MathOperator.Multiply => left * right,
            var op when op == MathOperator.Divide => left / right,
            _ => null
        };
    }

    private static PhysicalQuantity? EvaluateUnaryAsQuantity(UnaryExpression unary, Dictionary<string, PhysicalQuantity> variableValues)
    {
        var operand = EvaluateAsQuantity(unary.Operand, variableValues);
        if (operand is null) return null;

        return unary.Operator switch
        {
            var op when op == MathOperator.Negate => -operand,
            _ => null
        };
    }

    private static PhysicalQuantity? EvaluateFunctionAsQuantity(FunctionCallExpression func, Dictionary<string, PhysicalQuantity> variableValues)
    {
        if (func.Arguments.Count != 1) return null;
        var arg = EvaluateAsQuantity(func.Arguments[0], variableValues);
        if (arg is null) return null;

        return func.Name switch
        {
            "sin" or "cos" or "tan" or "asin" or "acos" or "atan" =>
                new PhysicalQuantity { Value = 0.0, Dimension = Dimension.None },
            "sqrt" => QuantityOperations.Sqrt(arg),
            _ => null
        };
    }

    public static Expression WithDimensions(this Expression expr, Dictionary<string, Dimension> variableDimensions)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        if (variableDimensions is null) throw new ArgumentNullException(nameof(variableDimensions));

        var analyzer = DimensionAnalyzer.Instance;
        foreach (var kvp in variableDimensions)
            analyzer.SetVariableDimension(kvp.Key, kvp.Value);

        return expr;
    }
}
