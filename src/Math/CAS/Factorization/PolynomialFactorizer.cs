namespace MathVerse.Math.CAS.Factorization;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;

public class PolynomialFactorizer
{
    public Expression FactorPolynomial(Expression expr, List<string> steps)
    {
        return expr switch
        {
            BinaryExpression b => FactorBinary(b, steps),
            _ => expr
        };
    }

    private Expression FactorBinary(BinaryExpression expr, List<string> steps)
    {
        if (expr.Operator.Equals(MathOperator.Add) || expr.Operator.Equals(MathOperator.Subtract))
        {
            var terms = FlattenAddSub(expr);
            var factored = TryFactorSum(terms, steps);
            if (factored != null) return factored;
        }
        return expr;
    }

    private List<Expression> FlattenAddSub(BinaryExpression expr)
    {
        var terms = new List<Expression>();
        FlattenAddSubRecursive(expr, terms);
        return terms;
    }

    private void FlattenAddSubRecursive(Expression expr, List<Expression> terms)
    {
        if (expr is BinaryExpression b && (b.Operator.Equals(MathOperator.Add) || b.Operator.Equals(MathOperator.Subtract)))
        {
            FlattenAddSubRecursive(b.Left, terms);
            terms.Add(b.Operator.Equals(MathOperator.Subtract) ? Expr.Negate(b.Right) : b.Right);
        }
        else
        {
            terms.Add(expr);
        }
    }

    private Expression? TryFactorSum(List<Expression> terms, List<string> steps)
    {
        if (terms.Count == 2)
        {
            var diffOfSquares = FactorDifferenceOfSquares(terms[0], terms[1]);
            if (diffOfSquares != null) { steps.Add("DifferenceOfSquares"); return diffOfSquares; }

            var sumOfCubes = FactorSumOfCubes(terms[0], terms[1]);
            if (sumOfCubes != null) { steps.Add("SumOfCubes"); return sumOfCubes; }

            var diffOfCubes = FactorDifferenceOfCubes(terms[0], terms[1]);
            if (diffOfCubes != null) { steps.Add("DifferenceOfCubes"); return diffOfCubes; }
        }

        if (terms.Count == 3)
        {
            var quadratic = FactorQuadratic(terms[0], terms[1], terms[2]);
            if (quadratic != null) { steps.Add("FactorQuadratic"); return quadratic; }
        }

        return null;
    }

    private Expression? FactorDifferenceOfSquares(Expression a, Expression b)
    {
        if (IsPerfectSquare(a) && IsPerfectSquare(b))
        {
            var sqrtA = ExtractSquareRoot(a);
            var sqrtB = ExtractSquareRoot(b);
            return Expr.Multiply(Expr.Add(sqrtA, sqrtB), Expr.Subtract(sqrtA, sqrtB));
        }
        return null;
    }

    private Expression? FactorSumOfCubes(Expression a, Expression b)
    {
        if (IsPerfectCube(a) && IsPerfectCube(b))
        {
            var cbrtA = ExtractCubeRoot(a);
            var cbrtB = ExtractCubeRoot(b);
            return Expr.Multiply(
                Expr.Add(cbrtA, cbrtB),
                Expr.Subtract(Expr.Add(Expr.Pow(cbrtA, Expr.Literal(2)), Expr.Add(Expr.Multiply(cbrtA, cbrtB), Expr.Pow(cbrtB, Expr.Literal(2)))), Expr.Literal(0))
            );
        }
        return null;
    }

    private Expression? FactorDifferenceOfCubes(Expression a, Expression b)
    {
        if (IsPerfectCube(a) && IsPerfectCube(b))
        {
            var cbrtA = ExtractCubeRoot(a);
            var cbrtB = ExtractCubeRoot(b);
            return Expr.Multiply(
                Expr.Subtract(cbrtA, cbrtB),
                Expr.Add(Expr.Add(Expr.Pow(cbrtA, Expr.Literal(2)), Expr.Multiply(cbrtA, cbrtB)), Expr.Pow(cbrtB, Expr.Literal(2)))
            );
        }
        return null;
    }

    private Expression? FactorQuadratic(Expression a, Expression b, Expression c)
    {
        return null;
    }

    private bool IsPerfectSquare(Expression expr)
    {
        return expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power) &&
               b.Right is LiteralExpression l && System.Math.Abs(l.Value - 2) < 1e-10;
    }

    private bool IsPerfectCube(Expression expr)
    {
        return expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power) &&
               b.Right is LiteralExpression l && System.Math.Abs(l.Value - 3) < 1e-10;
    }

    private Expression ExtractSquareRoot(Expression expr)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power))
            return b.Left;
        return Expr.Call("sqrt", expr);
    }

    private Expression ExtractCubeRoot(Expression expr)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power))
            return b.Left;
        return Expr.Call("cbrt", expr);
    }

    public Expression FactorQuadratic(Expression expr) => expr;
    public Expression FactorCubic(Expression expr) => expr;
    public Expression FactorByGrouping(Expression expr) => expr;
    public Expression FactorDifferenceOfSquares(Expression expr) => expr;
    public Expression FactorSumDifferenceOfCubes(Expression expr) => expr;
}