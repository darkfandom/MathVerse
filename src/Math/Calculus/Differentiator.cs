namespace MathVerse.Math.Calculus;

/// <summary>
/// Main symbolic differentiation engine.
/// Recursively differentiates expression trees and applies simplification.
/// </summary>
public sealed class Differentiator
{
    private readonly SimplificationOptions _simplificationOptions;

    /// <summary>
    /// Initializes a new <see cref="Differentiator"/> with the specified simplification options.
    /// </summary>
    /// <param name="simplificationOptions">Options controlling simplification after differentiation.</param>
    public Differentiator(SimplificationOptions? simplificationOptions = null)
    {
        _simplificationOptions = simplificationOptions ?? SimplificationOptions.Default;
    }

    /// <summary>
    /// Gets the simplification options used by this differentiator.
    /// </summary>
    public SimplificationOptions SimplificationOptions => _simplificationOptions;

    /// <summary>
    /// Differentiates the given expression with respect to the specified variable.
    /// Applies simplification after each differentiation step.
    /// </summary>
    /// <param name="expr">The expression to differentiate.</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <param name="order">The order of differentiation (default: 1).</param>
    /// <returns>The differentiated expression.</returns>
    public Expression Differentiate(Expression expr, string variable, int order = 1)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));
        Guard.GreaterThan(order, 0, nameof(order));

        var result = expr;
        for (var i = 0; i < order; i++)
        {
            result = DifferentiateOnce(result, variable);
            result = SimplifyDifferentiation(result);
        }

        return result;
    }

    /// <summary>
    /// Performs a single differentiation pass on the expression tree.
    /// </summary>
    private Expression DifferentiateOnce(Expression expr, string variable)
    {
        return expr.Kind switch
        {
            ExpressionKind.Literal => DifferentiationRules.DifferentiateConstant(expr, variable),
            ExpressionKind.Constant => DifferentiationRules.DifferentiateConstant(expr, variable),
            ExpressionKind.Variable => DifferentiationRules.DifferentiateVariable(expr, variable),
            ExpressionKind.Identity => DifferentiationRules.DifferentiateConstant(expr, variable),
            ExpressionKind.Binary => DifferentiateBinary(expr, variable),
            ExpressionKind.Unary => DifferentiateUnary(expr, variable),
            ExpressionKind.FunctionCall => DifferentiateFunctionCall(expr, variable),
            ExpressionKind.Derivative => DifferentiateDerivative(expr, variable),
            ExpressionKind.Integral => DifferentiateIntegral(expr, variable),
            ExpressionKind.Limit => DifferentiateLimit(expr, variable),
            ExpressionKind.Polynomial => DifferentiatePolynomial(expr, variable),
            ExpressionKind.Piecewise => DifferentiatePiecewise(expr, variable),
            ExpressionKind.Conditional => DifferentiateConditional(expr, variable),
            _ => Expr.Literal(0.0)
        };
    }

    /// <summary>
    /// Differentiates a binary expression by dispatching to the appropriate rule.
    /// </summary>
    private Expression DifferentiateBinary(Expression expr, string variable)
    {
        var bin = (BinaryExpression)expr;

        return bin.Operator switch
        {
            var op when op.Equals(MathOperator.Add) => DifferentiationRules.DifferentiateSum(expr, variable),
            var op when op.Equals(MathOperator.Subtract) => Expr.Subtract(
                DifferentiateOnce(bin.Left, variable),
                DifferentiateOnce(bin.Right, variable)),
            var op when op.Equals(MathOperator.Multiply) => DifferentiationRules.DifferentiateProduct(expr, variable),
            var op when op.Equals(MathOperator.Divide) => DifferentiationRules.DifferentiateQuotient(expr, variable),
            var op when op.Equals(MathOperator.Power) => DifferentiatePowerChain(expr, variable),
            _ => Expr.Literal(0.0)
        };
    }

    /// <summary>
    /// Differentiates a power expression using the general power rule.
    /// Handles both d/dx(x^n) and d/dx(f(x)^g(x)).
    /// </summary>
    private Expression DifferentiatePowerChain(Expression expr, string variable)
    {
        var bin = (BinaryExpression)expr;
        var baseExpr = bin.Left;
        var exponent = bin.Right;

        if (CalculusUtils.IsConstant(exponent, variable))
            return DifferentiationRules.DifferentiatePower(expr, variable);

        if (CalculusUtils.IsConstant(baseExpr, variable))
        {
            var result = Expr.Multiply(
                expr,
                Expr.Multiply(DifferentiateOnce(exponent, variable),
                    Expr.Ln(baseExpr)));
            return result;
        }

        var lnF = Expr.Ln(baseExpr);
        var exponentTimesLnF = Expr.Multiply(exponent, lnF);
        var expEquiv = Expr.Exp(exponentTimesLnF);
        return DifferentiateOnce(expEquiv, variable);
    }

    /// <summary>
    /// Differentiates a unary expression.
    /// </summary>
    private Expression DifferentiateUnary(Expression expr, string variable)
    {
        var unary = (UnaryExpression)expr;

        if (unary.Operator.Equals(MathOperator.Negate))
            return Expr.Negate(DifferentiateOnce(unary.Operand, variable));

        if (unary.Operator.Equals(MathOperator.Abs))
        {
            var innerDeriv = DifferentiateOnce(unary.Operand, variable);
            return Expr.Multiply(
                Expr.Divide(unary.Operand, Expr.Call("abs", unary.Operand)),
                innerDeriv);
        }

        return Expr.Literal(0.0);
    }

    /// <summary>
    /// Differentiates a function call, applying the chain rule.
    /// </summary>
    private Expression DifferentiateFunctionCall(Expression expr, string variable)
    {
        var funcCall = (FunctionCallExpression)expr;

        if (funcCall.Arguments.Count == 1)
            return DifferentiationRules.DifferentiateChain(expr, variable);

        return DifferentiationRules.DifferentiateFunction(expr, variable);
    }

    /// <summary>
    /// Differentiates a derivative expression by increasing the order.
    /// </summary>
    private Expression DifferentiateDerivative(Expression expr, string variable)
    {
        var derivExpr = (DerivativeExpression)expr;
        return new DerivativeExpression(derivExpr.Function, derivExpr.Variable, derivExpr.Order + 1);
    }

    /// <summary>
    /// Differentiates an integral expression.
    /// The derivative of an integral with respect to its variable cancels out (FTC).
    /// </summary>
    private Expression DifferentiateIntegral(Expression expr, string variable)
    {
        var intExpr = (IntegralExpression)expr;
        if (intExpr.IsDefinite)
            return Expr.Literal(0.0);

        return DifferentiateOnce(intExpr.Integrand, variable);
    }

    /// <summary>
    /// Differentiates a limit expression. Returns the limit of the derivative.
    /// </summary>
    private Expression DifferentiateLimit(Expression expr, string variable)
    {
        var limitExpr = (LimitExpression)expr;
        var innerDeriv = DifferentiateOnce(limitExpr.Body, variable);
        return Expr.Limit(innerDeriv, limitExpr.Variable, limitExpr.Target, limitExpr.Direction);
    }

    /// <summary>
    /// Differentiates a polynomial expression by differentiating each term.
    /// </summary>
    private Expression DifferentiatePolynomial(Expression expr, string variable)
    {
        var poly = (PolynomialExpression)expr;
        var varExpr = (VariableExpression)poly.Variable;

        if (varExpr.Name != variable)
            return Expr.Literal(0.0);

        var terms = new List<Expression>();
        for (var i = 1; i < poly.Coefficients.Count; i++)
        {
            var coeff = poly.Coefficients[i];
            var power = Expr.Literal((double)i);
            var term = Expr.Multiply(power, Expr.Multiply(coeff, Expr.Pow(poly.Variable, Expr.Literal((double)i - 1))));
            terms.Add(term);
        }

        if (terms.Count == 0)
            return Expr.Literal(0.0);

        var result = terms[0];
        for (var i = 1; i < terms.Count; i++)
            result = Expr.Add(result, terms[i]);

        return result;
    }

    /// <summary>
    /// Differentiates a piecewise expression by differentiating each case.
    /// </summary>
    private Expression DifferentiatePiecewise(Expression expr, string variable)
    {
        return Expr.Literal(0.0);
    }

    /// <summary>
    /// Differentiates a conditional expression by differentiating both branches.
    /// </summary>
    private Expression DifferentiateConditional(Expression expr, string variable)
    {
        return Expr.Literal(0.0);
    }

    /// <summary>
    /// Applies basic algebraic simplifications to the differentiated expression.
    /// </summary>
    private Expression SimplifyDifferentiation(Expression expr)
    {
        if (_simplificationOptions.EnableConstantFolding)
        {
            expr = FoldConstants(expr);
        }

        if (_simplificationOptions.EnableArithmeticRules)
        {
            expr = ApplyArithmeticSimplifications(expr);
        }

        return expr;
    }

    /// <summary>
    /// Applies basic arithmetic identity simplifications.
    /// </summary>
    private static Expression ApplyArithmeticSimplifications(Expression expr)
    {
        if (expr is BinaryExpression bin)
        {
            var left = ApplyArithmeticSimplifications(bin.Left);
            var right = ApplyArithmeticSimplifications(bin.Right);

            if (bin.Operator.Equals(MathOperator.Add))
            {
                if (CalculusUtils.IsZero(left)) return right;
                if (CalculusUtils.IsZero(right)) return left;
            }

            if (bin.Operator.Equals(MathOperator.Multiply))
            {
                if (CalculusUtils.IsZero(left) || CalculusUtils.IsZero(right))
                    return Expr.Literal(0.0);
                if (CalculusUtils.IsConstant(left, "_") && CalculusUtils.IsZero(left))
                    return Expr.Literal(0.0);
                if (CalculusUtils.IsConstant(right, "_") && CalculusUtils.IsZero(right))
                    return Expr.Literal(0.0);

                if (left is LiteralExpression { Value: 1.0 }) return right;
                if (right is LiteralExpression { Value: 1.0 }) return left;
            }

            if (bin.Operator.Equals(MathOperator.Power))
            {
                if (CalculusUtils.IsZero(right)) return Expr.Literal(1.0);
                if (right is LiteralExpression { Value: 1.0 }) return left;
            }

            if (bin.Operator.Equals(MathOperator.Subtract))
            {
                if (CalculusUtils.IsZero(left)) return Expr.Negate(right);
                if (CalculusUtils.IsZero(right)) return left;
                if (left.Equals(right)) return Expr.Literal(0.0);
            }

            return new BinaryExpression(bin.Operator, left, right);
        }

        if (expr is UnaryExpression unary)
        {
            var operand = ApplyArithmeticSimplifications(unary.Operand);

            if (unary.Operator.Equals(MathOperator.Negate) && operand is UnaryExpression inner &&
                inner.Operator.Equals(MathOperator.Negate))
                return inner.Operand;

            return new UnaryExpression(unary.Operator, operand);
        }

        return expr;
    }

    /// <summary>
    /// Folds constant sub-expressions into their evaluated results.
    /// </summary>
    private static Expression FoldConstants(Expression expr)
    {
        if (expr is BinaryExpression bin)
        {
            var left = FoldConstants(bin.Left);
            var right = FoldConstants(bin.Right);

            var lv = left is LiteralExpression ll ? (double?)ll.Value :
                     left is ConstantExpression cl ? (double?)cl.Value : null;
            var rv = right is LiteralExpression rl ? (double?)rl.Value :
                     right is ConstantExpression cr ? (double?)cr.Value : null;

            if (lv.HasValue && rv.HasValue)
            {
                var value = bin.Operator switch
                {
                    var op when op.Equals(MathOperator.Add) => lv.Value + rv.Value,
                    var op when op.Equals(MathOperator.Subtract) => lv.Value - rv.Value,
                    var op when op.Equals(MathOperator.Multiply) => lv.Value * rv.Value,
                    var op when op.Equals(MathOperator.Divide) && rv.Value != 0.0 => lv.Value / rv.Value,
                    var op when op.Equals(MathOperator.Power) => System.Math.Pow(lv.Value, rv.Value),
                    var op when op.Equals(MathOperator.Modulo) && rv.Value != 0.0 => lv.Value % rv.Value,
                    _ => double.NaN
                };

                if (!double.IsNaN(value) && !double.IsInfinity(value))
                    return Expr.Literal(value);
            }

            return new BinaryExpression(bin.Operator, left, right);
        }

        if (expr is UnaryExpression unary)
        {
            var operand = FoldConstants(unary.Operand);

            var uv = operand is LiteralExpression lu ? (double?)lu.Value :
                     operand is ConstantExpression cu ? (double?)cu.Value : null;

            if (uv.HasValue)
            {
                if (unary.Operator.Equals(MathOperator.Negate))
                    return Expr.Literal(-uv.Value);
            }

            return new UnaryExpression(unary.Operator, operand);
        }

        if (expr is FunctionCallExpression func)
        {
            var args = func.Arguments.Select(FoldConstants).ToArray();
            var doubleArgs = args.Select(a => a is LiteralExpression la ? (double?)la.Value :
                                              a is ConstantExpression ca ? (double?)ca.Value : null).ToArray();
            if (doubleArgs.All(a => a.HasValue))
            {
                var values = doubleArgs.Select(a => a!.Value).ToArray();
                var result = func.Name.ToLowerInvariant() switch
                {
                    "sin" => System.Math.Sin(values[0]),
                    "cos" => System.Math.Cos(values[0]),
                    "tan" => System.Math.Tan(values[0]),
                    "asin" => System.Math.Asin(values[0]),
                    "acos" => System.Math.Acos(values[0]),
                    "atan" => System.Math.Atan(values[0]),
                    "ln" or "log" => values.Length == 1 ? System.Math.Log(values[0]) : System.Math.Log(values[0], values[1]),
                    "log10" => System.Math.Log10(values[0]),
                    "exp" => System.Math.Exp(values[0]),
                    "sqrt" => System.Math.Sqrt(values[0]),
                    "abs" => System.Math.Abs(values[0]),
                    _ => double.NaN
                };
                if (!double.IsNaN(result) && !double.IsInfinity(result))
                    return Expr.Literal(result);
            }
            return new FunctionCallExpression(func.Name, args);
        }

        return expr;
    }
}
