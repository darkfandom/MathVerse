namespace MathVerse.Math.CAS.Expansion;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Generic;

public class PolynomialExpander
{
    public Expression ExpandPolynomial(Expression expr)
    {
        return ExpandPolynomialRecursive(expr);
    }

    private Expression ExpandPolynomialRecursive(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b => ExpandPolynomialBinary(b),
            UnaryExpression u => new UnaryExpression(u.Operator, ExpandPolynomialRecursive(u.Operand)),
            FunctionCallExpression f => new FunctionCallExpression(f.Name, f.Arguments.Select(ExpandPolynomialRecursive).ToArray()),
            _ => expr
        };
    }

    private Expression ExpandPolynomialBinary(BinaryExpression expr)
    {
        var left = ExpandPolynomialRecursive(expr.Left);
        var right = ExpandPolynomialRecursive(expr.Right);
        var binary = new BinaryExpression(expr.Operator, left, right);

        if (expr.Operator.Equals(MathOperator.Power))
        {
            return ExpandPower(binary);
        }

        if (expr.Operator.Equals(MathOperator.Multiply))
        {
            return ExpandMultiply(binary);
        }

        return binary;
    }

    private Expression ExpandMultiply(BinaryExpression expr)
    {
        var factors = new List<Expression>();
        CollectMulFactors(expr, factors);

        var expandedFactors = factors.Select(ExpandPolynomialRecursive).ToList();

        var result = expandedFactors[0];
        for (var i = 1; i < expandedFactors.Count; i++)
        {
            result = MultiplyExpanded(result, expandedFactors[i]);
        }

        return result;
    }

    private Expression MultiplyExpanded(Expression a, Expression b)
    {
        if (a is BinaryExpression ba && (ba.Operator.Equals(MathOperator.Add) || ba.Operator.Equals(MathOperator.Subtract)))
        {
            var left = MultiplyExpanded(ba.Left, b);
            var right = MultiplyExpanded(ba.Right, b);
            return ba.Operator.Equals(MathOperator.Add) ? Expr.Add(left, right) : Expr.Subtract(left, right);
        }

        if (b is BinaryExpression bb && (bb.Operator.Equals(MathOperator.Add) || bb.Operator.Equals(MathOperator.Subtract)))
        {
            var left = MultiplyExpanded(a, bb.Left);
            var right = MultiplyExpanded(a, bb.Right);
            return bb.Operator.Equals(MathOperator.Add) ? Expr.Add(left, right) : Expr.Subtract(left, right);
        }

        return Expr.Multiply(a, b);
    }

    private void CollectMulFactors(Expression expr, List<Expression> factors)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            CollectMulFactors(b.Left, factors);
            CollectMulFactors(b.Right, factors);
        }
        else
        {
            factors.Add(expr);
        }
    }

    private Expression ExpandPower(BinaryExpression expr)
    {
        var baseExpr = ExpandPolynomialRecursive(expr.Left);
        var expExpr = expr.Right;

        if (expExpr is LiteralExpression l && l.Value > 0 && l.Value == System.Math.Floor(l.Value))
        {
            var n = (int)l.Value;
            if (n == 0) return Expr.Literal(1);
            if (n == 1) return baseExpr;

            var result = baseExpr;
            for (var i = 1; i < n; i++)
            {
                result = MultiplyExpanded(result, baseExpr);
            }
            return result;
        }

        return new BinaryExpression(MathOperator.Power, baseExpr, expExpr);
    }
}