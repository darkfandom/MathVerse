namespace MathVerse.Math.CAS.Factorization;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Generic;

public class TrigonometricFactorizer
{
    public Expression FactorTrig(Expression expr, List<string> steps)
    {
        return FactorTrigRecursive(expr, steps);
    }

    private Expression FactorTrigRecursive(Expression expr, List<string> steps)
    {
        return expr switch
        {
            BinaryExpression b => FactorTrigBinary(b, steps),
            UnaryExpression u => new UnaryExpression(u.Operator, FactorTrigRecursive(u.Operand, steps)),
            FunctionCallExpression f => new FunctionCallExpression(f.Name, f.Arguments.Select(a => FactorTrigRecursive(a, steps)).ToArray()),
            _ => expr
        };
    }

    private Expression FactorTrigBinary(BinaryExpression expr, List<string> steps)
    {
        var left = FactorTrigRecursive(expr.Left, steps);
        var right = FactorTrigRecursive(expr.Right, steps);
        var binary = new BinaryExpression(expr.Operator, left, right);

        if (expr.Operator.Equals(MathOperator.Add) || expr.Operator.Equals(MathOperator.Subtract))
        {
            var factored = TryFactorTrigSum(binary, steps);
            if (factored != null) return factored;
        }

        if (expr.Operator.Equals(MathOperator.Multiply))
        {
            var factored = TryFactorTrigProduct(binary, steps);
            if (factored != null) return factored;
        }

        return binary;
    }

    private Expression? TryFactorTrigSum(BinaryExpression expr, List<string> steps)
    {
        var left = expr.Left;
        var right = expr.Right;

        if (IsSinSquared(left) && IsCosSquared(right))
        {
            steps.Add("SinSquaredPlusCosSquared");
            return Expr.Literal(1);
        }
        if (IsCosSquared(left) && IsSinSquared(right))
        {
            steps.Add("CosSquaredPlusSinSquared");
            return Expr.Literal(1);
        }

        if (IsSinCosProduct(left, right))
        {
            steps.Add("SinCosProduct");
            return Expr.Multiply(Expr.Literal(0.5), Expr.Call("sin", Expr.Multiply(Expr.Literal(2), ExtractAngle(left))));
        }

        if (IsSinCosProduct(right, left))
        {
            steps.Add("SinCosProduct");
            return Expr.Multiply(Expr.Literal(0.5), Expr.Call("sin", Expr.Multiply(Expr.Literal(2), ExtractAngle(right))));
        }

        return null;
    }

    private Expression? TryFactorTrigProduct(BinaryExpression expr, List<string> steps)
    {
        return null;
    }

    private bool IsSinSquared(Expression expr)
    {
        return expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power) &&
               b.Left is FunctionCallExpression f && f.Name == "sin" && f.Arguments.Count == 1 &&
               b.Right is LiteralExpression l && System.Math.Abs(l.Value - 2) < 1e-10;
    }

    private bool IsCosSquared(Expression expr)
    {
        return expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power) &&
               b.Left is FunctionCallExpression f && f.Name == "cos" && f.Arguments.Count == 1 &&
               b.Right is LiteralExpression l && System.Math.Abs(l.Value - 2) < 1e-10;
    }

    private bool IsSinCosProduct(Expression a, Expression b)
    {
        return IsSin(a) && IsCos(b);
    }

    private bool IsSin(Expression expr)
    {
        return expr is FunctionCallExpression f && f.Name == "sin" && f.Arguments.Count == 1;
    }

    private bool IsCos(Expression expr)
    {
        return expr is FunctionCallExpression f && f.Name == "cos" && f.Arguments.Count == 1;
    }

    private Expression ExtractAngle(Expression expr)
    {
        if (expr is FunctionCallExpression f && (f.Name == "sin" || f.Name == "cos") && f.Arguments.Count == 1)
            return f.Arguments[0];
        return Expr.Literal(0);
    }

    public Expression FactorQuadratic(Expression expr) => expr;
    public Expression FactorCubic(Expression expr) => expr;
    public Expression FactorByGrouping(Expression expr) => expr;
    public Expression FactorDifferenceOfSquares(Expression expr) => expr;
    public Expression FactorSumDifferenceOfCubes(Expression expr) => expr;
}