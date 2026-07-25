namespace MathVerse.Math.Calculus;

/// <summary>
/// Provides static methods implementing standard integration rules
/// for symbolic integration of mathematical expressions.
/// </summary>
public static class IntegrationRules
{
    private static readonly Lazy<Integrator> LazyIntegrator = new(() => new Integrator());

    /// <summary>
    /// Performs indefinite integration by delegating to a shared <see cref="Integrator"/> instance.
    /// Used internally by rule methods that need to integrate sub-expressions.
    /// </summary>
    internal static Expression IndefiniteIntegrate(Expression expr, string variable) =>
        LazyIntegrator.Value.IndefiniteIntegrate(expr, variable);

    /// <summary>
    /// Integrates a power expression with respect to the given variable.
    /// Integral(x^n) = x^(n+1) / (n+1) + C, for n != -1
    /// </summary>
    /// <param name="expr">The power expression (base ^ exponent).</param>
    /// <param name="variable">The variable of integration.</param>
    /// <returns>The integral of the power expression, or null if not applicable.</returns>
    public static Expression? IntegratePower(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not BinaryExpression bin || !bin.Operator.Equals(MathOperator.Power))
            return null;

        if (bin.Left is not VariableExpression varExpr || varExpr.Name != variable)
            return null;

        if (bin.Right is not LiteralExpression litExp)
            return null;

        var n = litExp.Value;

        if (System.Math.Abs(n - (-1.0)) < 1e-10)
            return null;

        var newExponent = n + 1.0;
        var coefficient = 1.0 / newExponent;

        return Expr.Multiply(
            Expr.Literal(coefficient),
            Expr.Pow(Expr.Variable(variable), Expr.Literal(newExponent)));
    }

    /// <summary>
    /// Integrates a sum expression by integrating each term.
    /// Integral(f + g) = Integral(f) + Integral(g)
    /// </summary>
    /// <param name="expr">The sum expression.</param>
    /// <param name="variable">The variable of integration.</param>
    /// <returns>The integral of the sum, or null if any term cannot be integrated.</returns>
    public static Expression? IntegrateSum(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not BinaryExpression bin || !bin.Operator.Equals(MathOperator.Add))
            return null;

        var leftIntegral = IndefiniteIntegrate(bin.Left, variable);
        var rightIntegral = IndefiniteIntegrate(bin.Right, variable);

        if (leftIntegral is IntegralExpression || rightIntegral is IntegralExpression)
            return null;

        return Expr.Add(leftIntegral, rightIntegral);
    }

    /// <summary>
    /// Integrates the exponential function e^x.
    /// Integral(e^x) = e^x + C
    /// Integral(e^(ax)) = (1/a) * e^(ax) + C
    /// </summary>
    /// <param name="expr">The exponential expression.</param>
    /// <param name="variable">The variable of integration.</param>
    /// <returns>The integral of the exponential, or null if not applicable.</returns>
    public static Expression? IntegrateExponential(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not FunctionCallExpression funcCall || funcCall.Name != "exp" || funcCall.Arguments.Count != 1)
            return null;

        var inner = funcCall.Arguments[0];

        if (inner is VariableExpression varExpr && varExpr.Name == variable)
            return Expr.Exp(inner);

        if (inner is BinaryExpression innerBin && innerBin.Operator.Equals(MathOperator.Multiply) &&
            CalculusUtils.IsConstant(innerBin.Left, variable))
        {
            var a = innerBin.Left;
            var aInverse = Expr.Divide(Expr.Literal(1.0), a);
            return Expr.Multiply(aInverse, Expr.Exp(inner));
        }

        return null;
    }

    /// <summary>
    /// Integrates basic trigonometric functions.
    /// Integral(sin(x)) = -cos(x) + C
    /// Integral(cos(x)) = sin(x) + C
    /// </summary>
    /// <param name="expr">The trigonometric expression.</param>
    /// <param name="variable">The variable of integration.</param>
    /// <returns>The integral of the trigonometric function, or null if not applicable.</returns>
    public static Expression? IntegrateTrigonometric(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is FunctionCallExpression funcCall && funcCall.Arguments.Count == 1)
        {
            var inner = funcCall.Arguments[0];
            if (inner is VariableExpression varExpr && varExpr.Name == variable)
            {
                return funcCall.Name switch
                {
                    "sin" => Expr.Negate(Expr.Cos(inner)),
                    "cos" => Expr.Sin(inner),
                    "csc" => Expr.Divide(
                        Expr.Negate(Expr.Ln(Expr.Call("abs", Expr.Add(Expr.Call("csc", inner), Expr.Call("cot", inner))))),
                        Expr.Literal(1.0)),
                    "sec" => Expr.Ln(Expr.Call("abs", Expr.Add(Expr.Call("sec", inner), Expr.Tan(inner)))),
                    "cot" => Expr.Ln(Expr.Call("abs", Expr.Sin(inner))),
                    _ => null
                };
            }
        }

        if (expr is BinaryExpression bin && bin.Operator.Equals(MathOperator.Power))
        {
            if (bin.Left is FunctionCallExpression funcBase && funcBase.Arguments.Count == 1 &&
                funcBase.Name == "sec" && funcBase.Arguments[0] is VariableExpression secVar &&
                secVar.Name == variable && bin.Right is LiteralExpression expLit && expLit.Value == 2.0)
            {
                return Expr.Tan(Expr.Variable(variable));
            }
        }

        return null;
    }

    /// <summary>
    /// Integrates logarithmic expressions.
    /// Integral(1/x) = ln|x| + C
    /// Integral(ln(x)) = x*ln(x) - x + C
    /// </summary>
    /// <param name="expr">The logarithmic expression.</param>
    /// <param name="variable">The variable of integration.</param>
    /// <returns>The integral, or null if not applicable.</returns>
    public static Expression? IntegrateLogarithmic(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is BinaryExpression bin && bin.Operator.Equals(MathOperator.Divide))
        {
            if (bin.Left is LiteralExpression { Value: 1.0 } &&
                bin.Right is VariableExpression varExpr && varExpr.Name == variable)
            {
                return Expr.Ln(Expr.Call("abs", Expr.Variable(variable)));
            }
        }

        if (expr is FunctionCallExpression funcCall && funcCall.Name == "ln" &&
            funcCall.Arguments.Count == 1 && funcCall.Arguments[0] is VariableExpression lnVar &&
            lnVar.Name == variable)
        {
            return Expr.Subtract(
                Expr.Multiply(Expr.Variable(variable), Expr.Ln(Expr.Variable(variable))),
                Expr.Variable(variable));
        }

        return null;
    }

    /// <summary>
    /// Applies integration by parts formula:
    /// Integral(u dv) = u*v - Integral(v du)
    /// Requires the expression to be in the form f(x)*g'(x).
    /// </summary>
    /// <param name="expr">The product expression to integrate by parts.</param>
    /// <param name="variable">The variable of integration.</param>
    /// <returns>The integral using integration by parts, or null if not applicable.</returns>
    public static Expression? IntegrateByParts(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not BinaryExpression bin || !bin.Operator.Equals(MathOperator.Multiply))
            return null;

        var u = bin.Left;
        var dv = bin.Right;

        var du = LazyIntegrator.Value._differentiator.Differentiate(u, variable);
        var v = IndefiniteIntegrate(dv, variable);

        if (v is IntegralExpression)
            return null;

        var uv = Expr.Multiply(u, v);
        var vdu = Expr.Multiply(v, du);
        var integralVdu = IndefiniteIntegrate(vdu, variable);

        if (integralVdu is IntegralExpression)
            return null;

        return Expr.Subtract(uv, integralVdu);
    }
}
