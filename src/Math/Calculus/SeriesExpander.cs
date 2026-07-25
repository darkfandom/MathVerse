namespace MathVerse.Math.Calculus;

/// <summary>
/// Computes Taylor and Maclaurin series expansions of mathematical expressions.
/// Builds polynomial approximations by computing derivatives at a center point.
/// </summary>
public sealed class SeriesExpander
{
    private readonly Differentiator _differentiator;

    /// <summary>
    /// Initializes a new <see cref="SeriesExpander"/>.
    /// </summary>
    public SeriesExpander()
    {
        _differentiator = new Differentiator();
    }

    /// <summary>
    /// Computes the Taylor series of the expression about the given center point
    /// up to the specified order.
    /// f(x) = sum_{n=0}^{order} f^{(n)}(a) / n! * (x - a)^n
    /// </summary>
    /// <param name="expr">The expression to expand.</param>
    /// <param name="variable">The variable of expansion.</param>
    /// <param name="center">The center point (a).</param>
    /// <param name="order">The maximum order of the Taylor polynomial.</param>
    /// <returns>The Taylor series polynomial expression.</returns>
    public Expression TaylorSeries(Expression expr, string variable, Expression center, int order)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));
        _ = center ?? throw new ArgumentNullException(nameof(center));
        Guard.GreaterThanOrEqualTo(order, 0, nameof(order));

        var centerValue = ExtractDouble(center);
        var terms = new List<Expression>();

        var currentDerivative = expr;
        var factorialN = 1.0;

        for (var n = 0; n <= order; n++)
        {
            var derivAtCenter = CalculusUtils.EvaluateAt(currentDerivative, variable, centerValue);

            if (!double.IsNaN(derivAtCenter) && !double.IsInfinity(derivAtCenter) &&
                System.Math.Abs(derivAtCenter) > 1e-15)
            {
                var coefficient = derivAtCenter / factorialN;
                var powerTerm = Expr.Pow(
                    Expr.Subtract(Expr.Variable(variable), center),
                    Expr.Literal((double)n));

                terms.Add(Expr.Multiply(Expr.Literal(coefficient), powerTerm));
            }

            if (n < order)
                currentDerivative = _differentiator.Differentiate(currentDerivative, variable);

            factorialN *= (n + 1);
        }

        if (terms.Count == 0)
            return Expr.Literal(0.0);

        var result = terms[0];
        for (var i = 1; i < terms.Count; i++)
            result = Expr.Add(result, terms[i]);

        return result;
    }

    /// <summary>
    /// Computes the Maclaurin series (Taylor series about x = 0)
    /// of the expression up to the specified order.
    /// f(x) = sum_{n=0}^{order} f^{(n)}(0) / n! * x^n
    /// </summary>
    /// <param name="expr">The expression to expand.</param>
    /// <param name="variable">The variable of expansion.</param>
    /// <param name="order">The maximum order of the Maclaurin polynomial.</param>
    /// <returns>The Maclaurin series polynomial expression.</returns>
    public Expression MaclaurinSeries(Expression expr, string variable, int order)
    {
        return TaylorSeries(expr, variable, Expr.Literal(0.0), order);
    }

    /// <summary>
    /// Computes a Taylor series with automatic order selection based on
    /// the desired accuracy threshold.
    /// </summary>
    /// <param name="expr">The expression to expand.</param>
    /// <param name="variable">The variable of expansion.</param>
    /// <param name="center">The center point.</param>
    /// <param name="accuracy">The desired accuracy (coefficients below this magnitude are truncated).</param>
    /// <param name="maxOrder">The maximum order to expand to.</param>
    /// <returns>The Taylor series polynomial expression.</returns>
    public Expression TaylorSeriesAdaptive(Expression expr, string variable, Expression center, double accuracy = 1e-10, int maxOrder = 50)
    {
        _ = expr ?? throw new ArgumentNullException(nameof(expr));
        _ = variable ?? throw new ArgumentNullException(nameof(variable));
        _ = center ?? throw new ArgumentNullException(nameof(center));
        Guard.GreaterThan(accuracy, 0.0, nameof(accuracy));
        Guard.GreaterThan(maxOrder, 0, nameof(maxOrder));

        var centerValue = ExtractDouble(center);
        var terms = new List<Expression>();

        var currentDerivative = expr;
        var factorialN = 1.0;

        for (var n = 0; n <= maxOrder; n++)
        {
            var derivAtCenter = CalculusUtils.EvaluateAt(currentDerivative, variable, centerValue);

            if (!double.IsNaN(derivAtCenter) && !double.IsInfinity(derivAtCenter))
            {
                var coefficient = derivAtCenter / factorialN;

                if (System.Math.Abs(coefficient) > accuracy)
                {
                    var powerTerm = Expr.Pow(
                        Expr.Subtract(Expr.Variable(variable), center),
                        Expr.Literal((double)n));

                    terms.Add(Expr.Multiply(Expr.Literal(coefficient), powerTerm));
                }
                else if (n > 5 && terms.Count > 0)
                {
                    break;
                }
            }

            currentDerivative = _differentiator.Differentiate(currentDerivative, variable);
            factorialN *= (n + 1);
        }

        if (terms.Count == 0)
            return Expr.Literal(0.0);

        var result = terms[0];
        for (var i = 1; i < terms.Count; i++)
            result = Expr.Add(result, terms[i]);

        return result;
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
