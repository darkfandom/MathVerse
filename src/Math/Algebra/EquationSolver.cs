namespace MathVerse.Math.Algebra;

/// <summary>
/// Solves polynomial equations symbolically by parsing expressions into polynomials
/// and applying algebraic solution formulas.
/// </summary>
public static class EquationSolver
{
    /// <summary>
    /// Solves a linear equation of the form ax + b = 0 for the given variable.
    /// </summary>
    /// <param name="left">Left-hand side of the equation.</param>
    /// <param name="right">Right-hand side of the equation.</param>
    /// <param name="variable">The variable to solve for.</param>
    /// <returns>An expression representing the solution.</returns>
    public static Expression SolveLinear(Expression left, Expression right, string variable)
    {
        var diff = Expr.Subtract(left, right);
        var (ok, poly) = PolynomialParser.TryParse(diff, variable);
        if (!ok || !poly.IsLinear)
            throw new ArgumentException("The equation is not linear in the specified variable.");

        double a = poly.CoefficientAt(1);
        double b = poly.CoefficientAt(0);

        if (System.Math.Abs(a) < 1e-15)
            throw new DivideByZeroException("Coefficient of the variable is zero.");

        return Expr.Literal(-b / a);
    }

    /// <summary>
    /// Solves a quadratic equation ax² + bx + c = 0 using the quadratic formula.
    /// </summary>
    /// <param name="left">Left-hand side of the equation.</param>
    /// <param name="right">Right-hand side of the equation.</param>
    /// <param name="variable">The variable to solve for.</param>
    /// <returns>Two expressions representing the roots (may be complex).</returns>
    public static (Expression X1, Expression X2) SolveQuadratic(Expression left, Expression right, string variable)
    {
        var diff = Expr.Subtract(left, right);
        var (ok, poly) = PolynomialParser.TryParse(diff, variable);
        if (!ok || !poly.IsQuadratic)
            throw new ArgumentException("The equation is not quadratic in the specified variable.");

        double a = poly.CoefficientAt(2);
        double b = poly.CoefficientAt(1);
        double c = poly.CoefficientAt(0);

        if (System.Math.Abs(a) < 1e-15)
        {
            var linearResult = SolveLinear(
                Expr.Add(Expr.Multiply(Expr.Literal(b), Expr.Variable(variable)), Expr.Literal(c)),
                Expr.Literal(0),
                variable);
            return (linearResult, linearResult);
        }

        double discriminant = b * b - 4.0 * a * c;
        double negB = -b;
        double twoA = 2.0 * a;

        if (discriminant >= 0)
        {
            double sqrtD = System.Math.Sqrt(discriminant);
            Expression x1 = Expr.Literal((negB - sqrtD) / twoA);
            Expression x2 = Expr.Literal((negB + sqrtD) / twoA);
            return (x1, x2);
        }
        else
        {
            double sqrtD = System.Math.Sqrt(-discriminant);
            var realPart = Expr.Literal(negB / twoA);
            var imagPart = Expr.Literal(sqrtD / twoA);

            Expression x1 = Expr.Complex(realPart, Expr.Negate(imagPart));
            Expression x2 = Expr.Complex(realPart, imagPart);
            return (x1, x2);
        }
    }

    /// <summary>
    /// Solves a cubic equation using Cardano's method.
    /// </summary>
    /// <param name="left">Left-hand side of the equation.</param>
    /// <param name="right">Right-hand side of the equation.</param>
    /// <param name="variable">The variable to solve for.</param>
    /// <returns>An array of up to three root expressions.</returns>
    public static ImmutableArray<Expression> SolveCubic(Expression left, Expression right, string variable)
    {
        var diff = Expr.Subtract(left, right);
        var (ok, poly) = PolynomialParser.TryParse(diff, variable);
        if (!ok || poly.Degree != 3)
            throw new ArgumentException("The equation is not cubic in the specified variable.");

        double a = poly.CoefficientAt(3);
        double b = poly.CoefficientAt(2);
        double c = poly.CoefficientAt(1);
        double d = poly.CoefficientAt(0);

        if (System.Math.Abs(a) < 1e-15)
        {
            var quadPoly = new Polynomial(variable, ImmutableArray.Create(d, c, b));
            var (x1, x2) = SolveQuadratic(
                Expr.Add(
                    Expr.Add(Expr.Multiply(Expr.Literal(b), Expr.Pow(Expr.Variable(variable), Expr.Literal(2))),
                             Expr.Multiply(Expr.Literal(c), Expr.Variable(variable))),
                    Expr.Literal(d)),
                Expr.Literal(0),
                variable);
            return [x1, x2];
        }

        double bn = b / a;
        double cn = c / a;
        double dn = d / a;

        double p = cn - bn * bn / 3.0;
        double q = 2.0 * bn * bn * bn / 27.0 - bn * cn / 3.0 + dn;

        double discriminant = q * q / 4.0 + p * p * p / 27.0;
        double shift = -bn / 3.0;

        var builder = ImmutableArray.CreateBuilder<Expression>();

        if (System.Math.Abs(discriminant) < 1e-15)
        {
            if (System.Math.Abs(q) < 1e-15)
            {
                builder.Add(Expr.Literal(shift));
            }
            else
            {
                double u = System.Math.Cbrt(-q / 2.0);
                builder.Add(Expr.Literal(2.0 * u + shift));
                builder.Add(Expr.Literal(-u + shift));
            }
        }
        else if (discriminant > 0)
        {
            double sqrtD = System.Math.Sqrt(discriminant);
            double u = System.Math.Cbrt(-q / 2.0 + sqrtD);
            double v = System.Math.Cbrt(-q / 2.0 - sqrtD);
            builder.Add(Expr.Literal(u + v + shift));
        }
        else
        {
            double r = System.Math.Sqrt(-p * p * p / 27.0);
            double theta = System.Math.Acos(-q / (2.0 * r));
            double m = 2.0 * System.Math.Cbrt(r);
            builder.Add(Expr.Literal(m * System.Math.Cos(theta / 3.0) + shift));
            builder.Add(Expr.Literal(m * System.Math.Cos((theta + 2.0 * System.Math.PI) / 3.0) + shift));
            builder.Add(Expr.Literal(m * System.Math.Cos((theta + 4.0 * System.Math.PI) / 3.0) + shift));
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Solves a system of two linear equations in two variables using substitution.
    /// </summary>
    /// <param name="equations">Array of exactly two equation expressions.</param>
    /// <param name="var1">The first variable name.</param>
    /// <param name="var2">The second variable name.</param>
    /// <returns>Two expressions representing the solution values.</returns>
    public static (Expression X1, Expression X2) SolveSystem2(
        ImmutableArray<Expression> equations, string var1, string var2)
    {
        if (equations.Length != 2)
            throw new ArgumentException("System requires exactly two equations.");

        var (a1, b1, c1) = ExtractLinearCoefficients(equations[0], var1, var2);
        var (a2, b2, c2) = ExtractLinearCoefficients(equations[1], var1, var2);

        double det = a1 * b2 - a2 * b1;
        if (System.Math.Abs(det) < 1e-15)
            throw new InvalidOperationException("The system is singular or degenerate.");

        double x1 = (c1 * b2 - c2 * b1) / det;
        double x2 = (a1 * c2 - a2 * c1) / det;

        return (Expr.Literal(x1), Expr.Literal(x2));
    }

    /// <summary>
    /// Solves a system of three linear equations in three variables using Cramer's rule.
    /// </summary>
    /// <param name="equations">Array of exactly three equation expressions.</param>
    /// <param name="var1">The first variable name.</param>
    /// <param name="var2">The second variable name.</param>
    /// <param name="var3">The third variable name.</param>
    /// <returns>Three expressions representing the solution values.</returns>
    public static (Expression X1, Expression X2, Expression X3) SolveSystem3(
        ImmutableArray<Expression> equations, string var1, string var2, string var3)
    {
        if (equations.Length != 3)
            throw new ArgumentException("System requires exactly three equations.");

        var (a1, b1, c1, d1) = ExtractTrilinearCoefficients(equations[0], var1, var2, var3);
        var (a2, b2, c2, d2) = ExtractTrilinearCoefficients(equations[1], var1, var2, var3);
        var (a3, b3, c3, d3) = ExtractTrilinearCoefficients(equations[2], var1, var2, var3);

        double det = a1 * (b2 * c3 - b3 * c2)
                   - b1 * (a2 * c3 - a3 * c2)
                   + c1 * (a2 * b3 - a3 * b2);

        if (System.Math.Abs(det) < 1e-15)
            throw new InvalidOperationException("The system is singular or degenerate.");

        double detX1 = d1 * (b2 * c3 - b3 * c2)
                     - b1 * (d2 * c3 - d3 * c2)
                     + c1 * (d2 * b3 - d3 * b2);

        double detX2 = a1 * (d2 * c3 - d3 * c2)
                     - d1 * (a2 * c3 - a3 * c2)
                     + c1 * (a2 * d3 - a3 * d2);

        double detX3 = a1 * (b2 * d3 - b3 * d2)
                     - b1 * (a2 * d3 - a3 * d2)
                     + d1 * (a2 * b3 - a3 * b2);

        return (Expr.Literal(detX1 / det), Expr.Literal(detX2 / det), Expr.Literal(detX3 / det));
    }

    private static (double A, double B, double C) ExtractLinearCoefficients(
        Expression equation, string var1, string var2)
    {
        Expression lhs, rhs;
        if (equation is EquationExpression eq)
        {
            lhs = eq.Left;
            rhs = eq.Right;
        }
        else
        {
            lhs = equation;
            rhs = Expr.Literal(0);
        }

        var diff = Expr.Subtract(lhs, rhs);
        var (ok, poly) = PolynomialParser.TryParse(diff, var1);
        if (!ok)
            throw new ArgumentException("Could not parse equation as polynomial.");

        double a = 0, b = 0;
        double c = -poly.CoefficientAt(0);

        var varExpr = Expr.Variable(var2);
        CollectPartial(poly, diff, var1, var2, ref a, ref b, ref c);

        return (a, b, c);
    }

    private static void CollectPartial(
        Polynomial poly, Expression expr, string var1, string var2,
        ref double a, ref double b, ref double c)
    {
        if (expr is BinaryExpression binary && binary.Operator == MathOperator.Add)
        {
            CollectPartial(poly, binary.Left, var1, var2, ref a, ref b, ref c);
            CollectPartial(poly, binary.Right, var1, var2, ref a, ref b, ref c);
        }
        else if (expr is BinaryExpression binarySub && binarySub.Operator == MathOperator.Subtract)
        {
            CollectPartial(poly, binarySub.Left, var1, var2, ref a, ref b, ref c);
            var negB = 0.0;
            var negC = 0.0;
            CollectPartial(poly, binarySub.Right, var1, var2, ref a, ref negB, ref negC);
            b -= negB;
            c -= negC;
        }
        else
        {
            var (ok, termPoly) = PolynomialParser.TryParse(expr, var2);
            if (ok && termPoly.Degree <= 1)
            {
                if (termPoly.IsConstant)
                    c += termPoly.CoefficientAt(0);
                else
                    b += termPoly.CoefficientAt(1);
            }
            else
            {
                var (okV, varPoly) = PolynomialParser.TryParse(expr, var1);
                if (okV && !varPoly.IsZero)
                    a += varPoly.LeadingCoefficient;
            }
        }
    }

    private static (double A, double B, double C, double D) ExtractTrilinearCoefficients(
        Expression equation, string var1, string var2, string var3)
    {
        Expression lhs, rhs;
        if (equation is EquationExpression eq)
        {
            lhs = eq.Left;
            rhs = eq.Right;
        }
        else
        {
            lhs = equation;
            rhs = Expr.Literal(0);
        }

        var diff = Expr.Subtract(lhs, rhs);

        var (a, b, c) = ExtractLinearCoefficients(equation, var1, var2);
        var terms = new List<Expression>();
        CollectAddends(diff, terms);

        double d = 0;
        foreach (var term in terms)
        {
            var (ok, tp) = PolynomialParser.TryParse(term, var3);
            if (ok && tp.Degree <= 1 && tp.IsConstant)
                d += tp.CoefficientAt(0);
        }

        d = -(d);

        var (a1, b1, c1) = ExtractLinearCoefficients(equation, var1, var3);
        double aFinal = a;
        double bFinal = b1;

        double dVal = -c;

        return (aFinal, b, b1, dVal);
    }

    private static void CollectAddends(Expression expr, List<Expression> result)
    {
        if (expr is BinaryExpression binary && binary.Operator == MathOperator.Add)
        {
            CollectAddends(binary.Left, result);
            CollectAddends(binary.Right, result);
        }
        else if (expr is BinaryExpression binarySub && binarySub.Operator == MathOperator.Subtract)
        {
            CollectAddends(binarySub.Left, result);
            result.Add(Expr.Negate(binarySub.Right));
        }
        else
        {
            result.Add(expr);
        }
    }
}
