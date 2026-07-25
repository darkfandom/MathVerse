namespace MathVerse.Math.Calculus;

/// <summary>
/// Main symbolic integration engine.
/// Computes indefinite and definite integrals of mathematical expressions.
/// Returns an <see cref="IntegralExpression"/> when unable to compute the integral.
/// </summary>
public sealed class Integrator
{
    internal readonly Differentiator _differentiator;

    /// <summary>
    /// Initializes a new <see cref="Integrator"/> with optional simplification options.
    /// </summary>
    /// <param name="simplificationOptions">Options controlling simplification after integration.</param>
    public Integrator(SimplificationOptions? simplificationOptions = null)
    {
        _differentiator = new Differentiator(simplificationOptions);
    }

    /// <summary>
    /// Computes the indefinite integral of the expression with respect to the given variable.
    /// Returns an <see cref="IntegralExpression"/> if the integral cannot be computed symbolically.
    /// </summary>
    /// <param name="expr">The expression to integrate.</param>
    /// <param name="variable">The variable of integration.</param>
    /// <returns>The integral expression, or an <see cref="IntegralExpression"/> if unable to compute.</returns>
    public Expression IndefiniteIntegrate(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is IntegralExpression)
            return expr;

        var result = TryIntegrate(expr, variable);
        return result ?? Expr.Integral(expr, Expr.Variable(variable));
    }

    /// <summary>
    /// Computes the definite integral of the expression from the lower bound to the upper bound.
    /// Uses the fundamental theorem of calculus: F(b) - F(a).
    /// </summary>
    /// <param name="expr">The expression to integrate.</param>
    /// <param name="variable">The variable of integration.</param>
    /// <param name="lower">The lower bound.</param>
    /// <param name="upper">The upper bound.</param>
    /// <returns>The definite integral result, or an <see cref="IntegralExpression"/> if unable to compute.</returns>
    public Expression DefiniteIntegrate(Expression expr, string variable, Expression lower, Expression upper)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));
        _ = lower ?? throw new ArgumentNullException(nameof(lower));
        _ = upper ?? throw new ArgumentNullException(nameof(upper));

        var antiderivative = IndefiniteIntegrate(expr, variable);
        if (antiderivative is IntegralExpression)
            return Expr.Integral(expr, Expr.Variable(variable), lower, upper);

        var upperEval = CalculusUtils.EvaluateAt(antiderivative, variable, ExtractDouble(upper));
        var lowerEval = CalculusUtils.EvaluateAt(antiderivative, variable, ExtractDouble(lower));

        return Expr.Literal(upperEval - lowerEval);
    }

    /// <summary>
    /// Attempts to integrate the expression by pattern matching on its structure.
    /// Returns null if unable to compute the integral.
    /// </summary>
    private Expression? TryIntegrate(Expression expr, string variable)
    {
        if (CalculusUtils.IsConstant(expr, variable))
            return Expr.Multiply(expr, Expr.Variable(variable));

        if (expr is VariableExpression varExpr && varExpr.Name == variable)
            return Expr.Multiply(Expr.Literal(0.5), Expr.Pow(Expr.Variable(variable), Expr.Literal(2.0)));

        var powerResult = IntegrationRules.IntegratePower(expr, variable);
        if (powerResult is not null)
            return powerResult;

        var expResult = IntegrationRules.IntegrateExponential(expr, variable);
        if (expResult is not null)
            return expResult;

        var trigResult = IntegrationRules.IntegrateTrigonometric(expr, variable);
        if (trigResult is not null)
            return trigResult;

        var logResult = IntegrationRules.IntegrateLogarithmic(expr, variable);
        if (logResult is not null)
            return logResult;

        if (expr is BinaryExpression bin)
        {
            if (bin.Operator.Equals(MathOperator.Add))
            {
                var sumResult = IntegrationRules.IntegrateSum(expr, variable);
                if (sumResult is not null)
                    return sumResult;
            }

            if (bin.Operator.Equals(MathOperator.Subtract))
            {
                var leftInt = TryIntegrate(bin.Left, variable);
                var rightInt = TryIntegrate(bin.Right, variable);
                if (leftInt is not null && rightInt is not null)
                    return Expr.Subtract(leftInt, rightInt);
            }

            if (bin.Operator.Equals(MathOperator.Multiply))
            {
                if (CalculusUtils.IsZero(bin.Left) || CalculusUtils.IsZero(bin.Right))
                    return Expr.Literal(0);

                if (CalculusUtils.IsConstant(bin.Left, variable))
                    return Expr.Multiply(bin.Left, IndefiniteIntegrate(bin.Right, variable));

                if (CalculusUtils.IsConstant(bin.Right, variable))
                    return Expr.Multiply(bin.Right, IndefiniteIntegrate(bin.Left, variable));

                var byPartsResult = IntegrationRules.IntegrateByParts(expr, variable);
                if (byPartsResult is not null)
                    return byPartsResult;
            }

            if (bin.Operator.Equals(MathOperator.Divide))
            {
                if (CalculusUtils.IsConstant(bin.Left, variable))
                    return Expr.Multiply(
                        bin.Left,
                        IndefiniteIntegrate(
                            Expr.Divide(Expr.Literal(1.0), bin.Right),
                            variable));

                var quotientResult = TryIntegrateQuotient(bin, variable);
                if (quotientResult is not null)
                    return quotientResult;
            }
        }

        if (expr is UnaryExpression unary && unary.Operator.Equals(MathOperator.Negate))
        {
            var innerInt = TryIntegrate(unary.Operand, variable);
            if (innerInt is not null)
                return Expr.Negate(innerInt);
        }

        if (expr is FunctionCallExpression funcCall)
        {
            if (funcCall.Name == "sqrt" && funcCall.Arguments.Count == 1 &&
                funcCall.Arguments[0] is VariableExpression sqrtVar && sqrtVar.Name == variable)
            {
                return Expr.Multiply(
                    Expr.Literal(2.0 / 3.0),
                    Expr.Pow(Expr.Variable(variable), Expr.Literal(1.5)));
            }

            if (funcCall.Name == "1/x" || (funcCall.Name == "reciprocal" && funcCall.Arguments.Count == 1 &&
                funcCall.Arguments[0] is VariableExpression recVar && recVar.Name == variable))
            {
                return Expr.Ln(Expr.Call("abs", Expr.Variable(variable)));
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to integrate a quotient expression by polynomial long division or partial fractions.
    /// </summary>
    private Expression? TryIntegrateQuotient(BinaryExpression bin, string variable)
    {
        if (bin.Right is VariableExpression varExpr && varExpr.Name == variable)
        {
            if (bin.Left is VariableExpression leftVar && leftVar.Name == variable)
                return Expr.Variable(variable);

            if (CalculusUtils.IsConstant(bin.Left, variable))
            {
                var c = bin.Left;
                return Expr.Multiply(c, Expr.Ln(Expr.Call("abs", Expr.Variable(variable))));
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts a double value from an expression if possible.
    /// </summary>
    private static double ExtractDouble(Expression expr)
    {
        if (expr is LiteralExpression lit)
            return lit.Value;

        if (expr is ConstantExpression constant)
            return constant.Value;

        if (expr is UnaryExpression unary && unary.Operator.Equals(MathOperator.Negate))
            return -ExtractDouble(unary.Operand);

        return double.NaN;
    }
}
