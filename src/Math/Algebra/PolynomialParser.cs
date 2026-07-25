namespace MathVerse.Math.Algebra;

/// <summary>
/// Parses <see cref="Expression"/> trees into <see cref="Polynomial"/> instances,
/// recognizing literals, variables, addition, subtraction, multiplication, and
/// integer power expressions.
/// </summary>
public static class PolynomialParser
{
    /// <summary>
    /// Attempts to parse an expression into a polynomial in the specified variable.
    /// </summary>
    /// <param name="expr">The expression to parse.</param>
    /// <param name="variable">The variable name to treat as the polynomial variable.</param>
    /// <returns>True and the polynomial if successful; false and the zero polynomial otherwise.</returns>
    public static (bool Success, Polynomial Polynomial) TryParse(Expression expr, string variable)
    {
        if (expr is null)
            return (false, Polynomial.Zero(variable));

        var terms = new Dictionary<int, double>();
        if (!CollectTerms(expr, variable, terms))
            return (false, Polynomial.Zero(variable));

        if (terms.Count == 0)
            return (true, Polynomial.Zero(variable));

        int maxDeg = terms.Keys.Max();
        var coeffs = new double[maxDeg + 1];
        foreach (var kv in terms)
            coeffs[kv.Key] = kv.Value;

        return (true, new Polynomial(variable, ImmutableArray.Create(coeffs)));
    }

    private static bool CollectTerms(Expression expr, string variable, Dictionary<int, double> terms)
    {
        switch (expr)
        {
            case LiteralExpression literal:
                AddTerm(terms, 0, literal.Value);
                return true;

            case VariableExpression v when v.Name == variable:
                AddTerm(terms, 1, 1.0);
                return true;

            case VariableExpression:
                return false;

            case ConstantExpression c:
                AddTerm(terms, 0, c.Value);
                return true;

            case BinaryExpression binary:
                return CollectBinary(binary, variable, terms);

            case UnaryExpression unary:
                return CollectUnary(unary, variable, terms);

            default:
                return false;
        }
    }

    private static bool CollectBinary(BinaryExpression binary, string variable, Dictionary<int, double> terms)
    {
        if (binary.Operator == MathOperator.Add)
        {
            if (!CollectTerms(binary.Left, variable, terms))
                return false;
            if (!CollectTerms(binary.Right, variable, terms))
                return false;
            return true;
        }

        if (binary.Operator == MathOperator.Subtract)
        {
            if (!CollectTerms(binary.Left, variable, terms))
                return false;
            var rightTerms = new Dictionary<int, double>();
            if (!CollectTerms(binary.Right, variable, rightTerms))
                return false;
            foreach (var kv in rightTerms)
                AddTerm(terms, kv.Key, -kv.Value);
            return true;
        }

        if (binary.Operator == MathOperator.Multiply)
        {
            var leftTerms = new Dictionary<int, double>();
            var rightTerms = new Dictionary<int, double>();
            if (!CollectTerms(binary.Left, variable, leftTerms))
                return false;
            if (!CollectTerms(binary.Right, variable, rightTerms))
                return false;

            foreach (var lk in leftTerms)
            {
                foreach (var rk in rightTerms)
                    AddTerm(terms, lk.Key + rk.Key, lk.Value * rk.Value);
            }
            return true;
        }

        if (binary.Operator == MathOperator.Power)
        {
            if (!TryGetIntExponent(binary.Right, out int exp) || exp < 0)
                return false;

            var baseTerms = new Dictionary<int, double>();
            if (!CollectTerms(binary.Left, variable, baseTerms))
                return false;

            var result = new Dictionary<int, double> { [0] = 1.0 };
            for (int i = 0; i < exp; i++)
            {
                var newResult = new Dictionary<int, double>();
                foreach (var rk in result)
                {
                    foreach (var bk in baseTerms)
                        AddTerm(newResult, rk.Key + bk.Key, rk.Value * bk.Value);
                }
                result = newResult;
            }

            foreach (var kv in result)
                AddTerm(terms, kv.Key, kv.Value);
            return true;
        }

        return false;
    }

    private static bool CollectUnary(UnaryExpression unary, string variable, Dictionary<int, double> terms)
    {
        if (unary.Operator == MathOperator.Negate)
        {
            var innerTerms = new Dictionary<int, double>();
            if (!CollectTerms(unary.Operand, variable, innerTerms))
                return false;
            foreach (var kv in innerTerms)
                AddTerm(terms, kv.Key, -kv.Value);
            return true;
        }

        return false;
    }

    private static bool TryGetIntExponent(Expression expr, out int value)
    {
        if (expr is LiteralExpression lit && lit.Value >= 0 && lit.Value == System.Math.Floor(lit.Value))
        {
            value = (int)lit.Value;
            return true;
        }
        value = 0;
        return false;
    }

    private static void AddTerm(Dictionary<int, double> terms, int degree, double coefficient)
    {
        if (terms.TryGetValue(degree, out double existing))
            terms[degree] = existing + coefficient;
        else
            terms[degree] = coefficient;
    }
}
