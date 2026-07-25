namespace MathVerse.Math.Algebra;

/// <summary>
/// Converts a <see cref="Polynomial"/> back into an <see cref="Expression"/> tree
/// using the <see cref="Expr"/> factory methods.
/// </summary>
public static class PolynomialToExpression
{
    /// <summary>
    /// Converts a polynomial to an equivalent expression tree.
    /// </summary>
    /// <param name="poly">The polynomial to convert.</param>
    /// <returns>An expression tree representing the polynomial.</returns>
    public static Expression Convert(Polynomial poly)
    {
        if (poly.IsZero)
            return Expr.Literal(0);

        Expression? result = null;

        for (int i = poly.Coefficients.Length - 1; i >= 0; i--)
        {
            double coeff = poly.CoefficientAt(i);
            if (System.Math.Abs(coeff) < 1e-15)
                continue;

            Expression term;

            if (i == 0)
            {
                term = Expr.Literal(coeff);
            }
            else if (i == 1)
            {
                if (System.Math.Abs(coeff - 1.0) < 1e-15)
                    term = Expr.Variable(poly.Variable);
                else if (System.Math.Abs(coeff + 1.0) < 1e-15)
                    term = Expr.Negate(Expr.Variable(poly.Variable));
                else
                    term = Expr.Multiply(Expr.Literal(coeff), Expr.Variable(poly.Variable));
            }
            else
            {
                Expression varPow = Expr.Pow(Expr.Variable(poly.Variable), Expr.Literal(i));
                if (System.Math.Abs(coeff - 1.0) < 1e-15)
                    term = varPow;
                else if (System.Math.Abs(coeff + 1.0) < 1e-15)
                    term = Expr.Negate(varPow);
                else
                    term = Expr.Multiply(Expr.Literal(coeff), varPow);
            }

            result = result is null ? term : Expr.Add(result, term);
        }

        return result ?? Expr.Literal(0);
    }
}
