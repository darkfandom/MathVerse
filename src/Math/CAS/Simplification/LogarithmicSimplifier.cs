namespace MathVerse.Math.CAS.Simplification;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Immutable;

public static class LogarithmicSimplifier
{
    public static ImmutableArray<string> LastAppliedRules { get; private set; }

    public static Expression Simplify(Expression expr)
    {
        LastAppliedRules = [];
        return SimplifyRecursive(expr);
    }

    private static Expression SimplifyRecursive(Expression expr)
    {
        if (expr is FunctionCallExpression f) return SimplifyFunction(f);
        if (expr is BinaryExpression b) return SimplifyBinary(b);
        if (expr is UnaryExpression u) return SimplifyUnary(u);
        return expr;
    }

    private static Expression SimplifyFunction(FunctionCallExpression expr)
    {
        var args = expr.Arguments.Select(SimplifyRecursive).ToArray();
        var func = new FunctionCallExpression(expr.Name, args);

        var name = expr.Name.ToLowerInvariant();
        if (name == "ln") return SimplifyLn(func);
        if (name == "log") return SimplifyLog(func);
        if (name == "log10") return SimplifyLog10(func);
        if (name == "exp") return SimplifyExp(func);
        return func;
    }

    private static Expression SimplifyLn(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        double? argVal = arg switch
        {
            LiteralExpression l => l.Value,
            ConstantExpression c => c.Value,
            _ => null
        };

        if (argVal.HasValue)
        {
            if (argVal.Value == 1) { LastAppliedRules = LastAppliedRules.Add("LnOne"); return Expr.Literal(0); }
            if (System.Math.Abs(argVal.Value - System.Math.E) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("LnE"); return Expr.Literal(1); }
            if (argVal.Value < 0) { LastAppliedRules = LastAppliedRules.Add("LnNegative"); return Expr.Call("ln", Expr.Call("abs", arg)); }
        }

        if (arg is FunctionCallExpression f)
        {
            if (f.Name == "exp" && f.Arguments.Count == 1)
            {
                LastAppliedRules = LastAppliedRules.Add("LnExpCancel");
                return f.Arguments[0];
            }
            if (f.Name == "pow" && f.Arguments.Count == 2)
            {
                LastAppliedRules = LastAppliedRules.Add("LnPower");
                return Expr.Multiply(f.Arguments[1], Expr.Call("ln", f.Arguments[0]));
            }
        }

        if (arg is BinaryExpression b)
        {
            if (b.Operator.Equals(MathOperator.Multiply))
            {
                LastAppliedRules = LastAppliedRules.Add("LnProduct");
                return Expr.Add(Expr.Call("ln", b.Left), Expr.Call("ln", b.Right));
            }
            if (b.Operator.Equals(MathOperator.Divide))
            {
                LastAppliedRules = LastAppliedRules.Add("LnQuotient");
                return Expr.Subtract(Expr.Call("ln", b.Left), Expr.Call("ln", b.Right));
            }
            if (b.Operator.Equals(MathOperator.Power))
            {
                LastAppliedRules = LastAppliedRules.Add("LnPower");
                return Expr.Multiply(b.Right, Expr.Call("ln", b.Left));
            }
        }

        return expr;
    }

    private static Expression SimplifyLog(FunctionCallExpression expr)
    {
        var args = expr.Arguments;
        if (args.Count == 1) return SimplifyLn(new FunctionCallExpression("ln", args));

        var arg = args[0];
        var baseExpr = args[1];

        if (arg is LiteralExpression l && baseExpr is LiteralExpression bl)
        {
            if (l.Value == 1) { LastAppliedRules = LastAppliedRules.Add("LogOne"); return Expr.Literal(0); }
            if (System.Math.Abs(l.Value - bl.Value) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("LogBase"); return Expr.Literal(1); }
        }

        if (arg is FunctionCallExpression f && f.Name == "pow" && f.Arguments.Count == 2)
        {
            LastAppliedRules = LastAppliedRules.Add("LogPower");
            return Expr.Multiply(f.Arguments[1], Expr.Call("log", f.Arguments[0], baseExpr));
        }

        if (arg is BinaryExpression b)
        {
            if (b.Operator.Equals(MathOperator.Multiply))
            {
                LastAppliedRules = LastAppliedRules.Add("LogProduct");
                return Expr.Add(Expr.Call("log", b.Left, baseExpr), Expr.Call("log", b.Right, baseExpr));
            }
            if (b.Operator.Equals(MathOperator.Divide))
            {
                LastAppliedRules = LastAppliedRules.Add("LogQuotient");
                return Expr.Subtract(Expr.Call("log", b.Left, baseExpr), Expr.Call("log", b.Right, baseExpr));
            }
            if (b.Operator.Equals(MathOperator.Power))
            {
                LastAppliedRules = LastAppliedRules.Add("LogPower");
                return Expr.Multiply(b.Right, Expr.Call("log", b.Left, baseExpr));
            }
        }

        if (baseExpr is LiteralExpression baseLit && arg is FunctionCallExpression fl && fl.Name == "log" && fl.Arguments.Count == 2)
        {
            LastAppliedRules = LastAppliedRules.Add("ChangeOfBase");
            return Expr.Divide(Expr.Call("ln", arg), Expr.Call("ln", baseExpr));
        }

        return expr;
    }

    private static Expression SimplifyLog10(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];
        if (arg is LiteralExpression l)
        {
            if (l.Value == 1) { LastAppliedRules = LastAppliedRules.Add("Log10One"); return Expr.Literal(0); }
            if (l.Value == 10) { LastAppliedRules = LastAppliedRules.Add("Log10Ten"); return Expr.Literal(1); }
        }
        return expr;
    }

    private static Expression SimplifyExp(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        if (arg is LiteralExpression l)
        {
            if (l.Value == 0) { LastAppliedRules = LastAppliedRules.Add("ExpZero"); return Expr.Literal(1); }
            if (System.Math.Abs(l.Value - 1.0) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("ExpOne"); return Expr.Literal(System.Math.E); }
        }

        if (arg is FunctionCallExpression f)
        {
            if (f.Name == "ln" && f.Arguments.Count == 1)
            {
                LastAppliedRules = LastAppliedRules.Add("ExpLnCancel");
                return f.Arguments[0];
            }
        }

        if (arg is BinaryExpression b)
        {
            if (b.Operator.Equals(MathOperator.Add))
            {
                LastAppliedRules = LastAppliedRules.Add("ExpSum");
                return Expr.Multiply(Expr.Call("exp", b.Left), Expr.Call("exp", b.Right));
            }
            if (b.Operator.Equals(MathOperator.Subtract))
            {
                LastAppliedRules = LastAppliedRules.Add("ExpDiff");
                return Expr.Divide(Expr.Call("exp", b.Left), Expr.Call("exp", b.Right));
            }
            if (b.Operator.Equals(MathOperator.Multiply))
            {
                if (b.Left is LiteralExpression ll && b.Right is FunctionCallExpression fl && fl.Name == "ln")
                {
                    LastAppliedRules = LastAppliedRules.Add("ExpMulLn");
                    return Expr.Pow(fl.Arguments[0], b.Left);
                }
                if (b.Right is LiteralExpression rl && b.Left is FunctionCallExpression fl2 && fl2.Name == "ln")
                {
                    LastAppliedRules = LastAppliedRules.Add("ExpMulLn");
                    return Expr.Pow(fl2.Arguments[0], b.Right);
                }
            }
        }

        return expr;
    }

    private static Expression SimplifyBinary(BinaryExpression expr)
    {
        var left = SimplifyRecursive(expr.Left);
        var right = SimplifyRecursive(expr.Right);
        return new BinaryExpression(expr.Operator, left, right);
    }

    private static Expression SimplifyUnary(UnaryExpression expr)
    {
        var operand = SimplifyRecursive(expr.Operand);
        return new UnaryExpression(expr.Operator, operand);
    }
}