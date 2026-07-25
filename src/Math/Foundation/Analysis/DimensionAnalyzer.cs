using System.Collections.Concurrent;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Expressions;

namespace MathVerse.Math.Foundation.Analysis;

public sealed class DimensionAnalyzer
{
    private static readonly Lazy<DimensionAnalyzer> LazyInstance = new(() => new DimensionAnalyzer());

    public static DimensionAnalyzer Instance => LazyInstance.Value;

    private readonly ConcurrentDictionary<string, Dimension> _variableDimensions = new(StringComparer.OrdinalIgnoreCase);
    private readonly DimensionDiagnostics _diagnostics = new();

    private DimensionAnalyzer()
    {
    }

    public Dimension AnalyzeExpression(Expression expr)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        return expr switch
        {
            LiteralExpression => Dimension.None,
            VariableExpression v => _variableDimensions.TryGetValue(v.Name, out var dim) ? dim : Dimension.None,
            ConstantExpression => Dimension.None,
            BinaryExpression b => AnalyzeBinary(b),
            UnaryExpression u => AnalyzeUnary(u),
            FunctionCallExpression f => AnalyzeFunction(f),
            _ => Dimension.None
        };
    }

    private Dimension AnalyzeBinary(BinaryExpression binary)
    {
        var left = AnalyzeExpression(binary.Left);
        var right = AnalyzeExpression(binary.Right);

        return binary.Operator switch
        {
            var op when op == MathOperator.Add || op == MathOperator.Subtract =>
                ValidateBinaryArithmetic(op, left, right, binary),
            var op when op == MathOperator.Multiply => left.Multiply(right),
            var op when op == MathOperator.Divide => left.Divide(right),
            var op when op == MathOperator.Power => Dimension.None,
            _ => Dimension.None
        };
    }

    private Dimension ValidateBinaryArithmetic(MathOperator op, Dimension left, Dimension right, BinaryExpression binary)
    {
        if (!left.IsCompatibleWith(right))
        {
            _diagnostics.Add(new DimensionDiagnostic
            {
                Rule = op == MathOperator.Add ? DimensionRule.Addition : DimensionRule.Subtraction,
                Message = $"Incompatible dimensions for {op.Symbol}: {left} vs {right}",
                Expression = binary.ToString(),
                ExpectedDimension = left,
                ActualDimension = right
            });
        }
        return left;
    }

    private Dimension AnalyzeUnary(UnaryExpression unary)
    {
        var operand = AnalyzeExpression(unary.Operand);
        return operand;
    }

    private Dimension AnalyzeFunction(FunctionCallExpression func)
    {
        var argDimensions = func.Arguments.Select(AnalyzeExpression).ToArray();
        return DimensionInferenceEngine.InferFromContext(func.Name, argDimensions) ?? Dimension.None;
    }

    public Dimension CheckDimensionalConsistency(Expression expr)
    {
        _diagnostics.Clear();
        return AnalyzeExpression(expr);
    }

    public Dimension GetResultDimension(Expression expr)
    {
        return AnalyzeExpression(expr);
    }

    public void SetVariableDimension(string name, Dimension dimension)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        if (dimension is null) throw new ArgumentNullException(nameof(dimension));
        _variableDimensions[name] = dimension;
    }

    public Dimension GetVariableDimension(string name)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        return _variableDimensions.TryGetValue(name, out var dim) ? dim : Dimension.None;
    }

    public void Clear()
    {
        _variableDimensions.Clear();
        _diagnostics.Clear();
    }

    public DimensionDiagnostics Diagnostics => _diagnostics;
}
