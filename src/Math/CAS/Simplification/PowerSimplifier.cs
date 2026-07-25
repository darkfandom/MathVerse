namespace MathVerse.Math.CAS.Simplification;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Immutable;

public static class PowerSimplifier
{
    public static ImmutableArray<string> LastAppliedRules { get; private set; }

    public static Expression Simplify(Expression expr)
    {
        LastAppliedRules = [];
        return SimplifyRecursive(expr);
    }

    private static Expression SimplifyRecursive(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b => SimplifyBinary(b),
            UnaryExpression u => SimplifyUnary(u),
            FunctionCallExpression f => SimplifyFunction(f),
            _ => expr
        };
    }

    private static Expression SimplifyBinary(BinaryExpression expr)
    {
        var left = SimplifyRecursive(expr.Left);
        var right = SimplifyRecursive(expr.Right);
        var binary = new BinaryExpression(expr.Operator, left, right);

        if (!binary.Operator.Equals(MathOperator.Power)) return binary;

        return SimplifyPower(binary);
    }

    private static Expression SimplifyUnary(UnaryExpression expr)
    {
        var operand = SimplifyRecursive(expr.Operand);
        return new UnaryExpression(expr.Operator, operand);
    }

    private static Expression SimplifyFunction(FunctionCallExpression expr)
    {
        var args = expr.Arguments.Select(SimplifyRecursive).ToArray();
        return new FunctionCallExpression(expr.Name, args);
    }

    private static Expression SimplifyPower(BinaryExpression expr)
    {
        var baseExpr = expr.Left;
        var expExpr = expr.Right;

        if (expExpr is LiteralExpression le)
        {
            if (System.Math.Abs(le.Value - 0) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("PowerZero"); return Expr.Literal(1); }
            if (System.Math.Abs(le.Value - 1) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("PowerOne"); return baseExpr; }
            if (System.Math.Abs(le.Value - 0.5) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("PowerHalf"); return Expr.Call("sqrt", baseExpr); }
            if (System.Math.Abs(le.Value - 2) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("PowerTwo"); return Expr.Multiply(baseExpr, baseExpr); }
            if (System.Math.Abs(le.Value - 3) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("PowerThree"); return Expr.Multiply(Expr.Multiply(baseExpr, baseExpr), baseExpr); }
            if (le.Value < 0) { LastAppliedRules = LastAppliedRules.Add("PowerNegative"); return Expr.Divide(Expr.Literal(1), Expr.Pow(baseExpr, Expr.Literal(-le.Value))); }
        }

        if (baseExpr is LiteralExpression bl)
        {
            if (bl.Value == 0 && expExpr is LiteralExpression el && el.Value > 0) { LastAppliedRules = LastAppliedRules.Add("ZeroPower"); return Expr.Literal(0); }
            if (bl.Value == 1) { LastAppliedRules = LastAppliedRules.Add("OnePower"); return Expr.Literal(1); }
            if (bl.Value == -1 && expExpr is LiteralExpression el2)
            {
                LastAppliedRules = LastAppliedRules.Add("NegOnePower");
                return Expr.Literal(el2.Value % 2 == 0 ? 1 : -1);
            }
        }

        if (baseExpr is BinaryExpression bp && bp.Operator.Equals(MathOperator.Power))
        {
            var newExp = Expr.Multiply(bp.Right, expExpr);
            LastAppliedRules = LastAppliedRules.Add("PowerOfPower");
            return SimplifyPower(new BinaryExpression(MathOperator.Power, bp.Left, newExp));
        }

        if (baseExpr is BinaryExpression bm && bm.Operator.Equals(MathOperator.Multiply))
        {
            LastAppliedRules = LastAppliedRules.Add("PowerOfProduct");
            return Expr.Multiply(
                SimplifyPower(new BinaryExpression(MathOperator.Power, bm.Left, expExpr)),
                SimplifyPower(new BinaryExpression(MathOperator.Power, bm.Right, expExpr))
            );
        }

        if (baseExpr is BinaryExpression bd && bd.Operator.Equals(MathOperator.Divide))
        {
            LastAppliedRules = LastAppliedRules.Add("PowerOfQuotient");
            return Expr.Divide(
                SimplifyPower(new BinaryExpression(MathOperator.Power, bd.Left, expExpr)),
                SimplifyPower(new BinaryExpression(MathOperator.Power, bd.Right, expExpr))
            );
        }

        return expr;
    }
}