namespace MathVerse.Math.CAS.Factorization;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Generic;

public class CommonFactorExtractor
{
    public Expression ExtractCommonFactor(Expression expr, List<string> steps)
    {
        return ExtractCommonFactorRecursive(expr, steps);
    }

    private Expression ExtractCommonFactorRecursive(Expression expr, List<string> steps)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Add) || b.Operator.Equals(MathOperator.Subtract) =>
                ExtractFromSum(b, steps),
            BinaryExpression b => new BinaryExpression(b.Operator,
                ExtractCommonFactorRecursive(b.Left, steps),
                ExtractCommonFactorRecursive(b.Right, steps)),
            UnaryExpression u => new UnaryExpression(u.Operator, ExtractCommonFactorRecursive(u.Operand, steps)),
            FunctionCallExpression f => new FunctionCallExpression(f.Name, f.Arguments.Select(a => ExtractCommonFactorRecursive(a, steps)).ToArray()),
            _ => expr
        };
    }

    private Expression ExtractFromSum(BinaryExpression expr, List<string> steps)
    {
        var terms = new List<Expression>();
        CollectAddSubTerms(expr, terms);

        var commonFactor = FindCommonFactor(terms);
        if (commonFactor == null) return expr;

        var factoredTerms = terms.Select(t => DivideByFactor(t, commonFactor)).ToArray();
        var sum = BuildAddSubChain(factoredTerms, expr.Operator);

        steps.Add($"ExtractCommonFactor:{commonFactor}");
        return Expr.Multiply(commonFactor, sum);
    }

    private void CollectAddSubTerms(BinaryExpression expr, List<Expression> terms)
    {
        if (expr.Operator.Equals(MathOperator.Add) || expr.Operator.Equals(MathOperator.Subtract))
        {
            CollectAddSubTerms((BinaryExpression)expr.Left, terms);
            terms.Add(expr.Operator.Equals(MathOperator.Subtract) ? Expr.Negate(expr.Right) : expr.Right);
        }
        else
        {
            terms.Add(expr);
        }
    }

    private Expression? FindCommonFactor(List<Expression> terms)
    {
        if (terms.Count < 2) return null;

        var firstFactors = GetAllFactors(terms[0]);
        var common = new HashSet<Expression>(firstFactors, ExpressionEqualityComparer.Instance);

        for (var i = 1; i < terms.Count; i++)
        {
            var termFactors = GetAllFactors(terms[i]);
            common.IntersectWith(termFactors);
            if (common.Count == 0) return null;
        }

        if (common.Count == 0) return null;

        var numericFactors = new List<LiteralExpression>();
        var otherFactors = new List<Expression>();

        foreach (var factor in common)
        {
            if (factor is LiteralExpression lit) numericFactors.Add(lit);
            else otherFactors.Add(factor);
        }

        Expression result = Expr.Literal(1.0);

        if (numericFactors.Count > 0)
        {
            var gcd = FactorOutGCD(numericFactors);
            if (gcd != 1) result = Expr.Multiply(result, Expr.Literal(gcd));
        }

        foreach (var factor in otherFactors)
        {
            result = Expr.Multiply(result, factor);
        }

        return result.Equals(Expr.Literal(1.0)) ? null : result;
    }

    private List<Expression> GetAllFactors(Expression expr)
    {
        var factors = new List<Expression>();
        ExtractFactorsRecursive(expr, factors);
        return factors;
    }

    private void ExtractFactorsRecursive(Expression expr, List<Expression> factors)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            ExtractFactorsRecursive(b.Left, factors);
            ExtractFactorsRecursive(b.Right, factors);
        }
        else
        {
            factors.Add(expr);
        }
    }

    private double FactorOutGCD(List<LiteralExpression> literals)
    {
        if (literals.Count == 0) return 1;

        var values = literals.Select(l => System.Math.Abs(l.Value)).ToArray();
        var gcd = values[0];

        for (var i = 1; i < values.Length; i++)
            gcd = ComputeGCD(gcd, values[i]);

        return gcd;
    }

    private static double ComputeGCD(double a, double b)
    {
        while (b > 1e-10)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    private Expression DivideByFactor(Expression term, Expression factor)
    {
        if (term is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            var factors = GetAllFactors(term);
            var remaining = factors.Where(f => !ExpressionEqualityComparer.Instance.Equals(f, factor)).ToList();
            return BuildMulChain(remaining);
        }
        if (ExpressionEqualityComparer.Instance.Equals(term, factor)) return Expr.Literal(1);
        return Expr.Divide(term, factor);
    }

    private Expression BuildMulChain(List<Expression> factors)
    {
        if (factors.Count == 0) return Expr.Literal(1);
        var result = factors[0];
        for (var i = 1; i < factors.Count; i++)
            result = Expr.Multiply(result, factors[i]);
        return result;
    }

    private Expression BuildAddSubChain(Expression[] terms, MathOperator op)
    {
        if (terms.Length == 0) return Expr.Literal(0);
        var result = terms[0];
        for (var i = 1; i < terms.Length; i++)
            result = op.Equals(MathOperator.Add) ? Expr.Add(result, terms[i]) : Expr.Subtract(result, terms[i]);
        return result;
    }

    public static double FactorOutGCD(IReadOnlyList<Expression> terms)
    {
        var literals = terms.OfType<LiteralExpression>().Select(l => System.Math.Abs(l.Value)).ToArray();
        if (literals.Length == 0) return 1;

        var gcd = literals[0];
        for (var i = 1; i < literals.Length; i++)
            gcd = ComputeGCD(gcd, literals[i]);

        return gcd;
    }
}

internal sealed class ExpressionEqualityComparer : IEqualityComparer<Expression>
{
    public static readonly ExpressionEqualityComparer Instance = new();

    public bool Equals(Expression? x, Expression? y) => x?.Equals(y) ?? y is null;

    public int GetHashCode(Expression obj) => obj?.GetHashCode() ?? 0;
}