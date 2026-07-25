namespace MathVerse.Math.Compiler.Differentiation;

using System;

/// <summary>Provides forward-mode automatic differentiation using dual numbers.</summary>
public sealed class ForwardModeAD
{
    /// <summary>Computes the value and first derivative of a function at a given point using dual numbers.</summary>
    /// <param name="f">The function to differentiate, expressed as a mapping from DualNumber to DualNumber.</param>
    /// <param name="x">The point at which to evaluate the derivative.</param>
    /// <returns>A tuple of (function value, derivative value).</returns>
    public (double Value, double Derivative) Differentiate(Func<DualNumber, DualNumber> f, double x)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));

        DualNumber input = DualNumber.Create(x, 1.0);
        DualNumber result = f(input);
        return (result.Real, result.Dual);
    }

    /// <summary>Computes the value and first derivative of a multivariate function at a given point using forward-mode AD.</summary>
    /// <param name="f">The function to differentiate.</param>
    /// <param name="point">The evaluation point.</param>
    /// <param name="variableIndex">Which variable to differentiate with respect to (0-indexed).</param>
    /// <returns>A tuple of (function value, partial derivative).</returns>
    public (double Value, double PartialDerivative) DifferentiatePartial(
        Func<DualNumber[], DualNumber> f,
        double[] point,
        int variableIndex)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));
        if (variableIndex < 0 || variableIndex >= point.Length)
            throw new ArgumentOutOfRangeException(nameof(variableIndex));

        var inputs = new DualNumber[point.Length];
        for (int i = 0; i < point.Length; i++)
            inputs[i] = DualNumber.FromValue(point[i]);

        inputs[variableIndex] = DualNumber.Create(point[variableIndex], 1.0);

        DualNumber result = f(inputs);
        return (result.Real, result.Dual);
    }

    /// <summary>Computes the gradient of a multivariate function using forward-mode AD.</summary>
    /// <param name="f">The function to differentiate.</param>
    /// <param name="point">The evaluation point.</param>
    /// <returns>An array of partial derivatives (gradient vector).</returns>
    public double[] Gradient(Func<DualNumber[], DualNumber> f, double[] point)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));

        var gradient = new double[point.Length];

        for (int i = 0; i < point.Length; i++)
        {
            var inputs = new DualNumber[point.Length];
            for (int j = 0; j < point.Length; j++)
                inputs[j] = DualNumber.FromValue(point[j]);

            inputs[i] = DualNumber.Create(point[i], 1.0);
            DualNumber result = f(inputs);
            gradient[i] = result.Dual;
        }

        return gradient;
    }

    /// <summary>Computes the value and derivative of an expression AST at a given point.</summary>
    public (double Value, double Derivative) DifferentiateExpression(
        Expressions.ExpressionNode root,
        string variableName,
        double x)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));
        if (string.IsNullOrEmpty(variableName)) throw new ArgumentException("Variable name required.", nameof(variableName));

        DualNumber result = EvaluateExpression(root, variableName, x);
        return (result.Real, result.Dual);
    }

    /// <summary>Evaluates an expression AST with a dual number for a given variable.</summary>
    public DualNumber EvaluateExpression(Expressions.ExpressionNode root, string variableName, DualNumber x)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        return root switch
        {
            Expressions.NumberNode num => DualNumber.FromValue(num.Value),
            Expressions.VariableNode var => string.Equals(var.Name, variableName, StringComparison.Ordinal)
                ? x
                : DualNumber.FromValue(0),
            Expressions.BinaryOpNode bin => EvaluateBinary(bin, variableName, x),
            Expressions.UnaryOpNode unary => EvaluateUnary(unary, variableName, x),
            Expressions.FunctionNode func => EvaluateFunction(func, variableName, x),
            _ => throw new ArgumentException($"Unknown expression type: {root.GetType().Name}"),
        };
    }

    private DualNumber EvaluateBinary(Expressions.BinaryOpNode bin, string variableName, DualNumber x)
    {
        DualNumber left = EvaluateExpression(bin.Left, variableName, x);
        DualNumber right = EvaluateExpression(bin.Right, variableName, x);

        return bin.Op switch
        {
            Expressions.BinaryOperator.Add => left + right,
            Expressions.BinaryOperator.Subtract => left - right,
            Expressions.BinaryOperator.Multiply => left * right,
            Expressions.BinaryOperator.Divide => left / right,
            Expressions.BinaryOperator.Power => DualNumber.Pow(left, right.Real),
            _ => throw new ArgumentException($"Unknown binary operator: {bin.Op}"),
        };
    }

    private DualNumber EvaluateUnary(Expressions.UnaryOpNode unary, string variableName, DualNumber x)
    {
        DualNumber operand = EvaluateExpression(unary.Operand, variableName, x);
        return unary.Op switch
        {
            Expressions.UnaryOperator.Negate => -operand,
            Expressions.UnaryOperator.Positive => operand,
            _ => throw new ArgumentException($"Unknown unary operator: {unary.Op}"),
        };
    }

    private DualNumber EvaluateFunction(Expressions.FunctionNode func, string variableName, DualNumber x)
    {
        if (func.Arguments.Count != 1)
            throw new ArgumentException($"Multi-argument functions not supported in forward AD: {func.FunctionName}");

        DualNumber arg = EvaluateExpression(func.Arguments[0], variableName, x);

        return func.FunctionName.ToLowerInvariant() switch
        {
            "sin" => DualNumber.Sin(arg),
            "cos" => DualNumber.Cos(arg),
            "tan" => DualNumber.Tan(arg),
            "asin" => DualNumber.Asin(arg),
            "acos" => DualNumber.Acos(arg),
            "atan" => DualNumber.Atan(arg),
            "ln" => DualNumber.Ln(arg),
            "log" => DualNumber.Log10(arg),
            "exp" => DualNumber.Exp(arg),
            "sqrt" => DualNumber.Sqrt(arg),
            "abs" => DualNumber.Abs(arg),
            "ceil" => DualNumber.Ceil(arg),
            "floor" => DualNumber.Floor(arg),
            _ => throw new ArgumentException($"Unknown function: {func.FunctionName}"),
        };
    }
}
