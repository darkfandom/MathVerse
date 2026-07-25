namespace MathVerse.Math.Calculus;

/// <summary>
/// Provides static methods implementing standard differentiation rules
/// for symbolic differentiation of mathematical expressions.
/// </summary>
public static class DifferentiationRules
{
    private static readonly Lazy<Differentiator> LazyDifferentiator = new(() => new Differentiator());

    /// <summary>
    /// Performs recursive differentiation by delegating to a shared <see cref="Differentiator"/> instance.
    /// Used internally by rule methods that need to differentiate sub-expressions.
    /// </summary>
    internal static Expression Differentiate(Expression expr, string variable) =>
        LazyDifferentiator.Value.Differentiate(expr, variable);

    /// <summary>
    /// Differentiates a constant expression with respect to the given variable.
    /// d/dx(c) = 0
    /// </summary>
    /// <param name="constant">The constant expression.</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <returns>Zero (<see cref="LiteralExpression"/> with value 0).</returns>
    public static Expression DifferentiateConstant(Expression constant, string variable)
    {
        _ = constant ?? throw new ArgumentNullException(nameof(constant));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        return Expr.Literal(0.0);
    }

    /// <summary>
    /// Differentiates a variable expression with respect to the given variable.
    /// d/dx(x) = 1, d/dx(y) = 0 for y != x.
    /// </summary>
    /// <param name="expr">The variable expression.</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <returns>One if the variable matches, zero otherwise.</returns>
    public static Expression DifferentiateVariable(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is VariableExpression varExpr && varExpr.Name == variable)
            return Expr.Literal(1.0);

        return Expr.Literal(0.0);
    }

    /// <summary>
    /// Differentiates a power expression using the power rule.
    /// d/dx(x^n) = n * x^(n-1)
    /// </summary>
    /// <param name="expr">The power expression (base ^ exponent).</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <returns>The derivative of the power expression.</returns>
    public static Expression DifferentiatePower(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not BinaryExpression bin || !bin.Operator.Equals(MathOperator.Power))
            return Expr.Literal(0.0);

        var baseExpr = bin.Left;
        var exponent = bin.Right;

        if (CalculusUtils.IsConstant(exponent, variable))
        {
            var newExponent = Expr.Subtract(exponent, Expr.Literal(1.0));
            var coefficient = exponent;
            var newBase = Expr.Pow(baseExpr, newExponent);
            var innerDerivative = Differentiate(baseExpr, variable);

            if (CalculusUtils.IsConstant(baseExpr, variable) || baseExpr.Equals(Expr.Variable(variable)))
                return Expr.Multiply(coefficient, newBase);

            return Expr.Multiply(coefficient, Expr.Multiply(newBase, innerDerivative));
        }

        return Expr.Literal(0.0);
    }

    /// <summary>
    /// Differentiates a sum expression using the sum rule.
    /// d/dx(f + g) = f' + g'
    /// </summary>
    /// <param name="expr">The sum expression.</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <returns>The derivative of the sum expression.</returns>
    public static Expression DifferentiateSum(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not BinaryExpression bin || !bin.Operator.Equals(MathOperator.Add))
            return Expr.Literal(0.0);

        var leftDeriv = Differentiate(bin.Left, variable);
        var rightDeriv = Differentiate(bin.Right, variable);

        return Expr.Add(leftDeriv, rightDeriv);
    }

    /// <summary>
    /// Differentiates a product expression using the product rule.
    /// d/dx(f * g) = f' * g + f * g'
    /// </summary>
    /// <param name="expr">The product expression.</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <returns>The derivative of the product expression.</returns>
    public static Expression DifferentiateProduct(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not BinaryExpression bin || !bin.Operator.Equals(MathOperator.Multiply))
            return Expr.Literal(0.0);

        var f = bin.Left;
        var g = bin.Right;
        var fPrime = Differentiate(f, variable);
        var gPrime = Differentiate(g, variable);

        var term1 = Expr.Multiply(fPrime, g);
        var term2 = Expr.Multiply(f, gPrime);

        return Expr.Add(term1, term2);
    }

    /// <summary>
    /// Differentiates a quotient expression using the quotient rule.
    /// d/dx(f / g) = (f' * g - f * g') / g^2
    /// </summary>
    /// <param name="expr">The quotient expression.</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <returns>The derivative of the quotient expression.</returns>
    public static Expression DifferentiateQuotient(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not BinaryExpression bin || !bin.Operator.Equals(MathOperator.Divide))
            return Expr.Literal(0.0);

        var f = bin.Left;
        var g = bin.Right;
        var fPrime = Differentiate(f, variable);
        var gPrime = Differentiate(g, variable);

        var numerator = Expr.Subtract(
            Expr.Multiply(fPrime, g),
            Expr.Multiply(f, gPrime));

        var denominator = Expr.Pow(g, Expr.Literal(2.0));

        return Expr.Divide(numerator, denominator);
    }

    /// <summary>
    /// Differentiates a function call expression using the chain rule.
    /// d/dx f(g(x)) = f'(g(x)) * g'(x)
    /// </summary>
    /// <param name="expr">The function call expression.</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <returns>The derivative using the chain rule.</returns>
    public static Expression DifferentiateChain(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not FunctionCallExpression funcCall || funcCall.Arguments.Count != 1)
            return Expr.Literal(0.0);

        var inner = funcCall.Arguments[0];
        var innerDerivative = Differentiate(inner, variable);

        if (CalculusUtils.IsZero(innerDerivative))
            return DifferentiateFunction(expr, variable);

        var outerDerivative = DifferentiateFunction(expr, variable);
        return Expr.Multiply(outerDerivative, innerDerivative);
    }

    /// <summary>
    /// Differentiates known elementary functions with respect to the given variable.
    /// Supports: sin, cos, tan, asin, acos, atan, exp, ln, log10, log, sqrt, cbrt, sinh, cosh, tanh.
    /// </summary>
    /// <param name="expr">The function call expression.</param>
    /// <param name="variable">The variable of differentiation.</param>
    /// <returns>The derivative of the function (without chain rule applied to the inner argument).</returns>
    public static Expression DifferentiateFunction(Expression expr, string variable)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));

        if (expr is not FunctionCallExpression funcCall || funcCall.Arguments.Count < 1)
            return Expr.Literal(0.0);

        var inner = funcCall.Arguments[0];
        var innerDeriv = Differentiate(inner, variable);

        Expression result = funcCall.Name switch
        {
            "sin" => Expr.Multiply(Expr.Cos(inner), innerDeriv),
            "cos" => Expr.Negate(Expr.Multiply(Expr.Sin(inner), innerDeriv)),
            "tan" => Expr.Multiply(
                Expr.Divide(Expr.Literal(1.0), Expr.Pow(Expr.Cos(inner), Expr.Literal(2.0))),
                innerDeriv),
            "asin" => Expr.Multiply(
                Expr.Divide(Expr.Literal(1.0), Expr.Sqrt(Expr.Subtract(Expr.Literal(1.0), Expr.Pow(inner, Expr.Literal(2.0))))),
                innerDeriv),
            "acos" => Expr.Negate(Expr.Multiply(
                Expr.Divide(Expr.Literal(1.0), Expr.Sqrt(Expr.Subtract(Expr.Literal(1.0), Expr.Pow(inner, Expr.Literal(2.0))))),
                innerDeriv)),
            "atan" => Expr.Multiply(
                Expr.Divide(Expr.Literal(1.0), Expr.Add(Expr.Literal(1.0), Expr.Pow(inner, Expr.Literal(2.0)))),
                innerDeriv),
            "exp" => Expr.Multiply(Expr.Exp(inner), innerDeriv),
            "ln" => Expr.Multiply(
                Expr.Divide(Expr.Literal(1.0), inner),
                innerDeriv),
            "log10" => Expr.Multiply(
                Expr.Divide(Expr.Literal(1.0), Expr.Multiply(inner, Expr.Literal(System.Math.Log(10.0)))),
                innerDeriv),
            "log" when funcCall.Arguments.Count == 2 => DifferentiateLogarithmWithBase(funcCall, variable),
            "sqrt" => Expr.Multiply(
                Expr.Divide(Expr.Literal(0.5), Expr.Sqrt(inner)),
                innerDeriv),
            "cbrt" => Expr.Multiply(
                Expr.Divide(
                    Expr.Literal(1.0 / 3.0),
                    Expr.Pow(inner, Expr.Literal(2.0 / 3.0))),
                innerDeriv),
            "sinh" => Expr.Multiply(Expr.Cosh(inner), innerDeriv),
            "cosh" => Expr.Multiply(Expr.Sinh(inner), innerDeriv),
            "tanh" => Expr.Multiply(
                Expr.Subtract(Expr.Literal(1.0), Expr.Pow(Expr.Tanh(inner), Expr.Literal(2.0))),
                innerDeriv),
            _ => Expr.Literal(0.0)
        };

        return result;
    }

    /// <summary>
    /// Differentiates a logarithm with an arbitrary base: d/dx log_b(x) = 1 / (x * ln(b))
    /// </summary>
    private static Expression DifferentiateLogarithmWithBase(FunctionCallExpression funcCall, string variable)
    {
        var inner = funcCall.Arguments[0];
        var baseExpr = funcCall.Arguments[1];
        var innerDeriv = Differentiate(inner, variable);

        return Expr.Multiply(
            Expr.Divide(Expr.Literal(1.0), Expr.Multiply(inner, Expr.Ln(baseExpr))),
            innerDeriv);
    }
}
