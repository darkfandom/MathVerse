namespace MathVerse.Math.Algebra;

/// <summary>
/// Converts an <see cref="Expression"/> tree into a <see cref="Polynomial"/> by walking
/// the tree and collecting terms by degree in the specified variable.
/// </summary>
public static class ExpressionToPolynomial
{
    /// <summary>
    /// Converts an expression to a polynomial in the given variable.
    /// </summary>
    /// <param name="expr">The expression to convert.</param>
    /// <param name="variable">The variable name.</param>
    /// <returns>The resulting polynomial, or null if the expression cannot be converted.</returns>
    public static Polynomial? Convert(Expression expr, string variable)
    {
        if (expr is null)
            return null;

        var terms = new Dictionary<int, double>();
        if (!Walk(expr, variable, terms))
            return null;

        if (terms.Count == 0)
            return Polynomial.Zero(variable);

        int maxDeg = terms.Keys.Max();
        var coeffs = new double[maxDeg + 1];
        foreach (var kv in terms)
            coeffs[kv.Key] = kv.Value;

        return new Polynomial(variable, ImmutableArray.Create(coeffs));
    }

    private static bool Walk(Expression expr, string variable, Dictionary<int, double> terms)
    {
        switch (expr)
        {
            case LiteralExpression literal:
                Accumulate(terms, 0, literal.Value);
                return true;

            case VariableExpression v when v.Name == variable:
                Accumulate(terms, 1, 1.0);
                return true;

            case VariableExpression:
                return false;

            case ConstantExpression c:
                Accumulate(terms, 0, c.Value);
                return true;

            case UnaryExpression unary when unary.Operator == MathOperator.Negate:
            {
                var inner = new Dictionary<int, double>();
                if (!Walk(unary.Operand, variable, inner))
                    return false;
                foreach (var kv in inner)
                    Accumulate(terms, kv.Key, -kv.Value);
                return true;
            }

            case BinaryExpression binary:
                return WalkBinary(binary, variable, terms);

            default:
                return false;
        }
    }

    private static bool WalkBinary(BinaryExpression binary, string variable, Dictionary<int, double> terms)
    {
        if (binary.Operator == MathOperator.Add)
        {
            if (!Walk(binary.Left, variable, terms))
                return false;
            if (!Walk(binary.Right, variable, terms))
                return false;
            return true;
        }

        if (binary.Operator == MathOperator.Subtract)
        {
            if (!Walk(binary.Left, variable, terms))
                return false;
            var right = new Dictionary<int, double>();
            if (!Walk(binary.Right, variable, right))
                return false;
            foreach (var kv in right)
                Accumulate(terms, kv.Key, -kv.Value);
            return true;
        }

        if (binary.Operator == MathOperator.Multiply)
        {
            var left = new Dictionary<int, double>();
            var right = new Dictionary<int, double>();
            if (!Walk(binary.Left, variable, left))
                return false;
            if (!Walk(binary.Right, variable, right))
                return false;

            foreach (var lk in left)
            {
                foreach (var rk in right)
                    Accumulate(terms, lk.Key + rk.Key, lk.Value * rk.Value);
            }
            return true;
        }

        if (binary.Operator == MathOperator.Power)
        {
            if (binary.Right is LiteralExpression expLit &&
                expLit.Value >= 0 &&
                expLit.Value == System.Math.Floor(expLit.Value))
            {
                int exp = (int)expLit.Value;
                var baseTerms = new Dictionary<int, double>();
                if (!Walk(binary.Left, variable, baseTerms))
                    return false;

                var result = new Dictionary<int, double> { [0] = 1.0 };
                for (int i = 0; i < exp; i++)
                {
                    var next = new Dictionary<int, double>();
                    foreach (var rk in result)
                    {
                        foreach (var bk in baseTerms)
                            Accumulate(next, rk.Key + bk.Key, rk.Value * bk.Value);
                    }
                    result = next;
                }

                foreach (var kv in result)
                    Accumulate(terms, kv.Key, kv.Value);
                return true;
            }

            return false;
        }

        return false;
    }

    private static void Accumulate(Dictionary<int, double> terms, int degree, double coefficient)
    {
        if (terms.TryGetValue(degree, out double existing))
            terms[degree] = existing + coefficient;
        else
            terms[degree] = coefficient;
    }
}
