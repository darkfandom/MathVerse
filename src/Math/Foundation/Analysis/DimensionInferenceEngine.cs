using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Expressions;

namespace MathVerse.Math.Foundation.Analysis;

public static class DimensionInferenceEngine
{
    public static Dimension? InferFromContext(string operation, Dimension[] argumentDimensions)
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));
        if (argumentDimensions is null || argumentDimensions.Length == 0)
            return Dimension.None;

        return operation switch
        {
            "+" or "-" => argumentDimensions.Length >= 1 ? argumentDimensions[0] : Dimension.None,
            "*" => argumentDimensions.Aggregate(Dimension.None, (acc, d) => acc.Multiply(d)),
            "/" => argumentDimensions.Length >= 2
                ? argumentDimensions[0].Divide(argumentDimensions[1])
                : argumentDimensions[0],
            "^" => argumentDimensions.Length >= 1 ? Dimension.None : Dimension.None,
            "sqrt" => argumentDimensions.Length >= 1 ? argumentDimensions[0].Power(0.5) : Dimension.None,
            "sin" or "cos" or "tan" or "asin" or "acos" or "atan" => Dimension.None,
            "ln" or "log" or "log10" or "exp" => Dimension.None,
            _ => Dimension.None
        };
    }

    public static Dimension? InferLiteralDimension(double value, Dictionary<string, Dimension>? context)
    {
        return Dimension.None;
    }

    public static Dimension? InferBinaryDimension(MathOperator op, Dimension? left, Dimension? right)
    {
        if (left is null || right is null) return null;

        return op switch
        {
            var o when o == MathOperator.Add || o == MathOperator.Subtract =>
                left.IsCompatibleWith(right) ? left : null,
            var o when o == MathOperator.Multiply => left.Multiply(right),
            var o when o == MathOperator.Divide => left.Divide(right),
            var o when o == MathOperator.Power => Dimension.None,
            _ => null
        };
    }

    public static Dimension? InferFunctionDimension(string functionName, Dimension[] argumentDimensions)
    {
        return InferFromContext(functionName, argumentDimensions);
    }
}
