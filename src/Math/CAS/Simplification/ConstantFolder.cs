namespace MathVerse.Math.CAS.Simplification;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Immutable;

public static class ConstantFolder
{
    public static ImmutableArray<string> LastAppliedRules { get; private set; }

    public static Expression Fold(Expression expr)
    {
        LastAppliedRules = [];
        return FoldRecursive(expr);
    }

    private static double? GetDoubleValue(Expression expr) => expr switch
    {
        LiteralExpression l => l.Value,
        ConstantExpression c => c.Value,
        _ => null
    };

    private static Expression FoldRecursive(Expression expr)
    {
        if (expr is BinaryExpression b) return FoldBinary(b);
        if (expr is UnaryExpression u) return FoldUnary(u);
        if (expr is FunctionCallExpression f) return FoldFunction(f);
        return expr;
    }

    private static Expression FoldBinary(BinaryExpression expr)
    {
        var left = FoldRecursive(expr.Left);
        var right = FoldRecursive(expr.Right);

        var lv = GetDoubleValue(left);
        var rv = GetDoubleValue(right);

        if (lv.HasValue && rv.HasValue)
        {
            var result = expr.Operator.Equals(MathOperator.Add) ? lv.Value + rv.Value :
                expr.Operator.Equals(MathOperator.Subtract) ? lv.Value - rv.Value :
                expr.Operator.Equals(MathOperator.Multiply) ? lv.Value * rv.Value :
                expr.Operator.Equals(MathOperator.Divide) && rv.Value != 0 ? lv.Value / rv.Value :
                expr.Operator.Equals(MathOperator.Modulo) && rv.Value != 0 ? lv.Value % rv.Value :
                expr.Operator.Equals(MathOperator.Power) ? System.Math.Pow(lv.Value, rv.Value) :
                double.NaN;

            if (!double.IsNaN(result))
            {
                LastAppliedRules = LastAppliedRules.Add($"FoldBinary:{expr.Operator.Symbol}");
                return Expr.Literal(result);
            }
        }

        if (expr.Operator.Equals(MathOperator.Add))
        {
            if (lv.HasValue && lv.Value == 0) return right;
            if (rv.HasValue && rv.Value == 0) return left;
        }

        if (expr.Operator.Equals(MathOperator.Subtract))
        {
            if (rv.HasValue && rv.Value == 0) return left;
            if (lv.HasValue && lv.Value == 0) return Expr.Negate(right);
        }

        if (expr.Operator.Equals(MathOperator.Multiply))
        {
            if ((lv.HasValue && lv.Value == 0) || (rv.HasValue && rv.Value == 0))
                return Expr.Literal(0);
            if (lv.HasValue && lv.Value == 1) return right;
            if (rv.HasValue && rv.Value == 1) return left;
            if (lv.HasValue && lv.Value == -1) return Expr.Negate(right);
            if (rv.HasValue && rv.Value == -1) return Expr.Negate(left);
        }

        if (expr.Operator.Equals(MathOperator.Divide))
        {
            if (lv.HasValue && lv.Value == 0) return Expr.Literal(0);
            if (rv.HasValue && rv.Value == 1) return left;
            if (rv.HasValue && rv.Value == -1) return Expr.Negate(left);
            if (left.Equals(right)) return Expr.Literal(1);
        }

        if (expr.Operator.Equals(MathOperator.Power))
        {
            if (rv.HasValue)
            {
                if (rv.Value == 0) return Expr.Literal(1);
                if (rv.Value == 1) return left;
            }
            if (lv.HasValue)
            {
                if (lv.Value == 0 && rv.HasValue && rv.Value > 0) return Expr.Literal(0);
                if (lv.Value == 1) return Expr.Literal(1);
                if (lv.Value == -1 && rv.HasValue)
                    return Expr.Literal(rv.Value % 2 == 0 ? 1 : -1);
            }
        }

        return new BinaryExpression(expr.Operator, left, right);
    }

    private static Expression FoldUnary(UnaryExpression expr)
    {
        var operand = FoldRecursive(expr.Operand);

        if (operand is LiteralExpression l)
        {
            var result = expr.Operator.Equals(MathOperator.Negate) ? -l.Value :
                expr.Operator.Equals(MathOperator.Abs) ? System.Math.Abs(l.Value) :
                double.NaN;

            if (!double.IsNaN(result))
            {
                LastAppliedRules = LastAppliedRules.Add($"FoldUnary:{expr.Operator.Symbol}");
                return Expr.Literal(result);
            }
        }

        if (expr.Operator.Equals(MathOperator.Negate) && operand is UnaryExpression u && u.Operator.Equals(MathOperator.Negate))
        {
            LastAppliedRules = LastAppliedRules.Add("FoldDoubleNegate");
            return u.Operand;
        }

        return new UnaryExpression(expr.Operator, operand);
    }

    private static Expression FoldFunction(FunctionCallExpression expr)
    {
        var args = expr.Arguments.Select(FoldRecursive).ToArray();

        var doubleArgs = args.Select(GetDoubleValue).ToArray();
        if (doubleArgs.All(a => a.HasValue))
        {
            var values = doubleArgs.Select(a => a!.Value).ToArray();
            var result = EvaluateFunction(expr.Name, values);
            if (!double.IsNaN(result))
            {
                LastAppliedRules = LastAppliedRules.Add($"FoldFunction:{expr.Name}");
                return Expr.Literal(result);
            }
        }

        return new FunctionCallExpression(expr.Name, args);
    }

    private static double EvaluateFunction(string name, double[] args)
    {
        return name.ToLowerInvariant() switch
        {
            "sin" => System.Math.Sin(args[0]),
            "cos" => System.Math.Cos(args[0]),
            "tan" => System.Math.Tan(args[0]),
            "asin" => System.Math.Asin(args[0]),
            "acos" => System.Math.Acos(args[0]),
            "atan" => System.Math.Atan(args[0]),
            "ln" or "log" => args.Length == 1 ? System.Math.Log(args[0]) : System.Math.Log(args[0], args[1]),
            "log10" => System.Math.Log10(args[0]),
            "exp" => System.Math.Exp(args[0]),
            "sqrt" => System.Math.Sqrt(args[0]),
            "cbrt" => System.Math.Cbrt(args[0]),
            "sinh" => System.Math.Sinh(args[0]),
            "cosh" => System.Math.Cosh(args[0]),
            "tanh" => System.Math.Tanh(args[0]),
            "abs" => System.Math.Abs(args[0]),
            "floor" => System.Math.Floor(args[0]),
            "ceil" => System.Math.Ceiling(args[0]),
            "round" => System.Math.Round(args[0]),
            "sign" => System.Math.Sign(args[0]),
            _ => double.NaN
        };
    }
}