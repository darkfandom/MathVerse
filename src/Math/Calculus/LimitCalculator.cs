namespace MathVerse.Math.Calculus;

/// <summary>
/// Computes symbolic and numerical limits of mathematical expressions.
/// Handles polynomial limits, rational function limits, and L'Hopital's rule.
/// </summary>
public sealed class LimitCalculator
{
    private readonly Differentiator _differentiator;
    private readonly Integrator _integrator;

    /// <summary>
    /// Initializes a new <see cref="LimitCalculator"/>.
    /// </summary>
    public LimitCalculator()
    {
        _differentiator = new Differentiator();
        _integrator = new Integrator();
    }

    /// <summary>
    /// Computes the limit of the expression as the variable approaches the target value.
    /// </summary>
    /// <param name="expr">The expression whose limit is computed.</param>
    /// <param name="variable">The variable approaching the target.</param>
    /// <param name="target">The target value the variable approaches.</param>
    /// <param name="direction">The limit direction (Both, Left, or Right).</param>
    /// <returns>The limit expression, or a <see cref="LimitExpression"/> if unable to compute.</returns>
    public Expression ComputeLimit(Expression expr, string variable, Expression target, LimitDirection direction = LimitDirection.Both)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));
        _ = target ?? throw new ArgumentNullException(nameof(target));

        var targetValue = ExtractDouble(target);

        if (CalculusUtils.IsConstant(expr, variable))
            return expr;

        if (expr is VariableExpression varExpr && varExpr.Name == variable)
            return target;

        if (expr is BinaryExpression bin)
        {
            var result = TryComputeBinaryLimit(bin, variable, targetValue, direction);
            if (result is not null)
                return result;
        }

        if (expr is FunctionCallExpression funcCall)
        {
            var result = TryComputeFunctionLimit(funcCall, variable, targetValue, direction);
            if (result is not null)
                return result;
        }

        if (expr is UnaryExpression unary && unary.Operator.Equals(MathOperator.Negate))
        {
            var innerLimit = ComputeLimit(unary.Operand, variable, target, direction);
            if (innerLimit is not LimitExpression)
                return Expr.Negate(innerLimit);
        }

        var numericalResult = TryNumericalLimit(expr, variable, targetValue, direction);
        if (!double.IsNaN(numericalResult) && !double.IsInfinity(numericalResult))
            return Expr.Literal(numericalResult);

        return Expr.Limit(expr, Expr.Variable(variable), target, direction);
    }

    /// <summary>
    /// Attempts to compute the limit of a binary expression.
    /// </summary>
    private Expression? TryComputeBinaryLimit(BinaryExpression bin, string variable, double targetValue, LimitDirection direction)
    {
        if (bin.Operator.Equals(MathOperator.Divide))
        {
            var numeratorLimit = ComputeLimit(bin.Left, variable, Expr.Literal(targetValue), direction);
            var denominatorLimit = ComputeLimit(bin.Right, variable, Expr.Literal(targetValue), direction);

            if (numeratorLimit is LiteralExpression numLit && denominatorLimit is LiteralExpression denLit)
            {
                if (denLit.Value != 0.0)
                    return Expr.Literal(numLit.Value / denLit.Value);

                if (numLit.Value == 0.0 && denLit.Value == 0.0)
                    return ApplyLHopitalsRule(bin, variable, targetValue, direction);
            }
        }

        if (bin.Operator.Equals(MathOperator.Add) || bin.Operator.Equals(MathOperator.Subtract))
        {
            var leftLimit = ComputeLimit(bin.Left, variable, Expr.Literal(targetValue), direction);
            var rightLimit = ComputeLimit(bin.Right, variable, Expr.Literal(targetValue), direction);

            if (leftLimit is LiteralExpression leftLit && rightLimit is LiteralExpression rightLit)
            {
                return bin.Operator.Equals(MathOperator.Add)
                    ? Expr.Literal(leftLit.Value + rightLit.Value)
                    : Expr.Literal(leftLit.Value - rightLit.Value);
            }
        }

        if (bin.Operator.Equals(MathOperator.Multiply))
        {
            var leftLimit = ComputeLimit(bin.Left, variable, Expr.Literal(targetValue), direction);
            var rightLimit = ComputeLimit(bin.Right, variable, Expr.Literal(targetValue), direction);

            if (leftLimit is LiteralExpression leftLit && rightLimit is LiteralExpression rightLit)
                return Expr.Literal(leftLit.Value * rightLit.Value);
        }

        if (bin.Operator.Equals(MathOperator.Power))
        {
            var baseLimit = ComputeLimit(bin.Left, variable, Expr.Literal(targetValue), direction);
            var exponentLimit = ComputeLimit(bin.Right, variable, Expr.Literal(targetValue), direction);

            if (baseLimit is LiteralExpression baseLit && exponentLimit is LiteralExpression expLit)
                return Expr.Literal(System.Math.Pow(baseLit.Value, expLit.Value));

            if (baseLimit is LiteralExpression baseLit2 && System.Math.Abs(baseLit2.Value - System.Math.E) < 1e-10)
            {
                if (exponentLimit is LiteralExpression expLit2 && expLit2.Value == 0.0)
                    return Expr.Literal(1.0);
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to compute the limit of a function call expression.
    /// </summary>
    private Expression? TryComputeFunctionLimit(FunctionCallExpression funcCall, string variable, double targetValue, LimitDirection direction)
    {
        if (funcCall.Arguments.Count != 1)
            return null;

        var inner = funcCall.Arguments[0];
        var innerLimit = ComputeLimit(inner, variable, Expr.Literal(targetValue), direction);

        if (innerLimit is not LiteralExpression innerLit)
            return null;

        return funcCall.Name switch
        {
            "sin" => Expr.Literal(System.Math.Sin(innerLit.Value)),
            "cos" => Expr.Literal(System.Math.Cos(innerLit.Value)),
            "tan" => System.Math.Abs(System.Math.Cos(innerLit.Value)) < 1e-10
                ? Expr.Limit(funcCall, Expr.Variable(variable), Expr.Literal(targetValue), direction)
                : Expr.Literal(System.Math.Tan(innerLit.Value)),
            "exp" => Expr.Literal(System.Math.Exp(innerLit.Value)),
            "ln" when innerLit.Value > 0.0 => Expr.Literal(System.Math.Log(innerLit.Value)),
            "ln" when innerLit.Value == 0.0 => ConstantExpression.NegativeInfinity,
            "sqrt" when innerLit.Value >= 0.0 => Expr.Literal(System.Math.Sqrt(innerLit.Value)),
            "asin" when innerLit.Value >= -1.0 && innerLit.Value <= 1.0 => Expr.Literal(System.Math.Asin(innerLit.Value)),
            "acos" when innerLit.Value >= -1.0 && innerLit.Value <= 1.0 => Expr.Literal(System.Math.Acos(innerLit.Value)),
            "atan" => Expr.Literal(System.Math.Atan(innerLit.Value)),
            "sinh" => Expr.Literal(System.Math.Sinh(innerLit.Value)),
            "cosh" => Expr.Literal(System.Math.Cosh(innerLit.Value)),
            "tanh" => Expr.Literal(System.Math.Tanh(innerLit.Value)),
            _ => null
        };
    }

    /// <summary>
    /// Applies L'Hopital's rule for 0/0 or inf/inf indeterminate forms.
    /// lim f(x)/g(x) = lim f'(x)/g'(x)
    /// </summary>
    private Expression ApplyLHopitalsRule(BinaryExpression quotient, string variable, double targetValue, LimitDirection direction)
    {
        var numeratorDeriv = _differentiator.Differentiate(quotient.Left, variable);
        var denominatorDeriv = _differentiator.Differentiate(quotient.Right, variable);

        var newQuotient = Expr.Divide(numeratorDeriv, denominatorDeriv);
        return ComputeLimit(newQuotient, variable, Expr.Literal(targetValue), direction);
    }

    /// <summary>
    /// Attempts to evaluate the limit numerically by sampling values near the target.
    /// </summary>
    private double TryNumericalLimit(Expression expr, string variable, double targetValue, LimitDirection direction)
    {
        const int sampleCount = 20;
        const double epsilon = 1e-8;

        var values = new List<double>();

        for (var i = 1; i <= sampleCount; i++)
        {
            var h = epsilon * System.Math.Pow(0.5, i);
            double samplePoint;

            if (direction == LimitDirection.Left)
                samplePoint = targetValue - h;
            else if (direction == LimitDirection.Right)
                samplePoint = targetValue + h;
            else
                samplePoint = targetValue + (i % 2 == 0 ? h : -h);

            var value = CalculusUtils.EvaluateAt(expr, variable, samplePoint);
            if (!double.IsNaN(value) && !double.IsInfinity(value))
                values.Add(value);
        }

        if (values.Count < 2)
            return double.NaN;

        var allSame = true;
        for (var i = 1; i < values.Count; i++)
        {
            if (System.Math.Abs(values[i] - values[0]) > 1e-6)
            {
                allSame = false;
                break;
            }
        }

        return allSame ? values[0] : double.NaN;
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
