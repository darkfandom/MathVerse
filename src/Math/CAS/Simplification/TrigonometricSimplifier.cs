namespace MathVerse.Math.CAS.Simplification;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Immutable;

public static class TrigonometricSimplifier
{
    public static ImmutableArray<string> LastAppliedRules { get; private set; }

    private static double? GetDoubleValue(Expression expr) => expr switch
    {
        LiteralExpression l => l.Value,
        ConstantExpression c => c.Value,
        _ => null
    };

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

    private static Expression SimplifyFunction(FunctionCallExpression expr)
    {
        var args = expr.Arguments.Select(SimplifyRecursive).ToArray();
        var simplified = new FunctionCallExpression(expr.Name, args);

        return expr.Name.ToLowerInvariant() switch
        {
            "sin" => SimplifySin(simplified),
            "cos" => SimplifyCos(simplified),
            "tan" => SimplifyTan(simplified),
            "asin" => SimplifyAsin(simplified),
            "acos" => SimplifyAcos(simplified),
            "atan" => SimplifyAtan(simplified),
            "sinh" => SimplifySinh(simplified),
            "cosh" => SimplifyCosh(simplified),
            "tanh" => SimplifyTanh(simplified),
            _ => simplified
        };
    }

    private static Expression SimplifySin(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        var argVal = GetDoubleValue(arg);
        if (argVal.HasValue)
        {
            if (argVal.Value == 0) { LastAppliedRules = LastAppliedRules.Add("SinZero"); return Expr.Literal(0); }
            if (System.Math.Abs(argVal.Value - System.Math.PI / 2) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("SinPiOver2"); return Expr.Literal(1); }
            if (System.Math.Abs(argVal.Value + System.Math.PI / 2) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("SinNegPiOver2"); return Expr.Literal(-1); }
            if (System.Math.Abs(argVal.Value - System.Math.PI) < 1e-12 || System.Math.Abs(argVal.Value + System.Math.PI) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("SinPi"); return Expr.Literal(0); }
        }

        if (arg is BinaryExpression b)
        {
            if (b.Operator.Equals(MathOperator.Add))
            {
                if (IsPi(b.Left) && b.Right is LiteralExpression r && r.Value == System.Math.PI / 2)
                {
                    LastAppliedRules = LastAppliedRules.Add("SinPiPlusPiOver2");
                    return Expr.Call("cos", b.Right);
                }
                if (b.Right is LiteralExpression r2 && r2.Value == System.Math.PI / 2 && IsPi(b.Left))
                {
                    LastAppliedRules = LastAppliedRules.Add("SinPiPlusPiOver2");
                    return Expr.Call("cos", b.Left);
                }
            }
        }

        if (arg is UnaryExpression u && u.Operator.Equals(MathOperator.Negate))
        {
            LastAppliedRules = LastAppliedRules.Add("SinNeg");
            return Expr.Negate(Expr.Call("sin", u.Operand));
        }

        if (arg is FunctionCallExpression f && f.Name == "asin" && f.Arguments.Count == 1)
        {
            LastAppliedRules = LastAppliedRules.Add("SinAsin");
            return f.Arguments[0];
        }

        return expr;
    }

    private static Expression SimplifyCos(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        var argVal = GetDoubleValue(arg);
        if (argVal.HasValue)
        {
            if (argVal.Value == 0) { LastAppliedRules = LastAppliedRules.Add("CosZero"); return Expr.Literal(1); }
            if (System.Math.Abs(argVal.Value - System.Math.PI / 2) < 1e-12 || System.Math.Abs(argVal.Value + System.Math.PI / 2) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("CosPiOver2"); return Expr.Literal(0); }
            if (System.Math.Abs(argVal.Value - System.Math.PI) < 1e-12) { LastAppliedRules = LastAppliedRules.Add("CosPi"); return Expr.Literal(-1); }
        }

        if (arg is UnaryExpression u && u.Operator.Equals(MathOperator.Negate))
        {
            LastAppliedRules = LastAppliedRules.Add("CosNeg");
            return Expr.Call("cos", u.Operand);
        }

        if (arg is FunctionCallExpression f && f.Name == "acos" && f.Arguments.Count == 1)
        {
            LastAppliedRules = LastAppliedRules.Add("CosAcos");
            return f.Arguments[0];
        }

        return expr;
    }

    private static Expression SimplifyTan(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        var argVal = GetDoubleValue(arg);
        if (argVal.HasValue && argVal.Value == 0) { LastAppliedRules = LastAppliedRules.Add("TanZero"); return Expr.Literal(0); }

        if (arg is FunctionCallExpression f && f.Name == "atan" && f.Arguments.Count == 1)
        {
            LastAppliedRules = LastAppliedRules.Add("TanAtan");
            return f.Arguments[0];
        }

        return expr;
    }

    private static Expression SimplifyAsin(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        var argVal = GetDoubleValue(arg);
        if (argVal.HasValue)
        {
            if (argVal.Value == 0) { LastAppliedRules = LastAppliedRules.Add("AsinZero"); return Expr.Literal(0); }
            if (argVal.Value == 1) { LastAppliedRules = LastAppliedRules.Add("AsinOne"); return Expr.Literal(System.Math.PI / 2); }
            if (argVal.Value == -1) { LastAppliedRules = LastAppliedRules.Add("AsinNegOne"); return Expr.Literal(-System.Math.PI / 2); }
        }

        if (arg is FunctionCallExpression f && f.Name == "sin" && f.Arguments.Count == 1)
        {
            LastAppliedRules = LastAppliedRules.Add("AsinSin");
            return f.Arguments[0];
        }

        return expr;
    }

    private static Expression SimplifyAcos(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        var argVal = GetDoubleValue(arg);
        if (argVal.HasValue)
        {
            if (argVal.Value == 1) { LastAppliedRules = LastAppliedRules.Add("AcosOne"); return Expr.Literal(0); }
            if (argVal.Value == 0) { LastAppliedRules = LastAppliedRules.Add("AcosZero"); return Expr.Literal(System.Math.PI / 2); }
            if (argVal.Value == -1) { LastAppliedRules = LastAppliedRules.Add("AcosNegOne"); return Expr.Literal(System.Math.PI); }
        }

        if (arg is FunctionCallExpression f && f.Name == "cos" && f.Arguments.Count == 1)
        {
            LastAppliedRules = LastAppliedRules.Add("AcosCos");
            return f.Arguments[0];
        }

        return expr;
    }

    private static Expression SimplifyAtan(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        var argVal = GetDoubleValue(arg);
        if (argVal.HasValue)
        {
            if (argVal.Value == 0) { LastAppliedRules = LastAppliedRules.Add("AtanZero"); return Expr.Literal(0); }
            if (argVal.Value == 1) { LastAppliedRules = LastAppliedRules.Add("AtanOne"); return Expr.Literal(System.Math.PI / 4); }
        }

        if (arg is FunctionCallExpression f && f.Name == "tan" && f.Arguments.Count == 1)
        {
            LastAppliedRules = LastAppliedRules.Add("AtanTan");
            return f.Arguments[0];
        }

        return expr;
    }

    private static Expression SimplifySinh(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];
        if (arg is LiteralExpression l && l.Value == 0) { LastAppliedRules = LastAppliedRules.Add("SinhZero"); return Expr.Literal(0); }
        if (arg is UnaryExpression u && u.Operator.Equals(MathOperator.Negate)) { LastAppliedRules = LastAppliedRules.Add("SinhNeg"); return Expr.Negate(Expr.Call("sinh", u.Operand)); }
        return expr;
    }

    private static Expression SimplifyCosh(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];
        if (arg is LiteralExpression l && l.Value == 0) { LastAppliedRules = LastAppliedRules.Add("CoshZero"); return Expr.Literal(1); }
        if (arg is UnaryExpression u && u.Operator.Equals(MathOperator.Negate)) { LastAppliedRules = LastAppliedRules.Add("CoshNeg"); return Expr.Call("cosh", u.Operand); }
        return expr;
    }

    private static Expression SimplifyTanh(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];
        if (arg is LiteralExpression l && l.Value == 0) { LastAppliedRules = LastAppliedRules.Add("TanhZero"); return Expr.Literal(0); }
        return expr;
    }

    private static Expression SimplifyBinary(BinaryExpression expr)
    {
        var left = SimplifyRecursive(expr.Left);
        var right = SimplifyRecursive(expr.Right);
        var binary = new BinaryExpression(expr.Operator, left, right);

        if (expr.Operator.Equals(MathOperator.Add) || expr.Operator.Equals(MathOperator.Subtract))
        {
            if (IsSinSquaredPlusCosSquared(left, right))
            {
                LastAppliedRules = LastAppliedRules.Add("SinSqPlusCosSq");
                return Expr.Literal(1);
            }
            if (IsCosSquaredPlusSinSquared(left, right))
            {
                LastAppliedRules = LastAppliedRules.Add("CosSqPlusSinSq");
                return Expr.Literal(1);
            }
        }

        if (expr.Operator.Equals(MathOperator.Multiply))
        {
            if (IsSinCosProduct(left, right))
            {
                LastAppliedRules = LastAppliedRules.Add("SinCosProduct");
                return Expr.Divide(Expr.Call("sin", Expr.Multiply(Expr.Literal(2), ExtractAngle(left))), Expr.Literal(2));
            }
        }

        return binary;
    }

    private static Expression SimplifyUnary(UnaryExpression expr)
    {
        var operand = SimplifyRecursive(expr.Operand);
        return new UnaryExpression(expr.Operator, operand);
    }

    private static bool IsPi(Expression expr)
    {
        if (expr is ConstantExpression c) return System.Math.Abs(c.Value - System.Math.PI) < 1e-12;
        if (expr is LiteralExpression l) return System.Math.Abs(l.Value - System.Math.PI) < 1e-12;
        return false;
    }

    private static bool IsSinSquaredPlusCosSquared(Expression a, Expression b)
    {
        if (IsSinSquared(a) && IsCosSquared(b) && ExtractAngle(a).Equals(ExtractAngle(b)))
            return true;
        if (IsCosSquared(a) && IsSinSquared(b) && ExtractAngle(a).Equals(ExtractAngle(b)))
            return true;
        return false;
    }

    private static bool IsCosSquaredPlusSinSquared(Expression a, Expression b) => IsSinSquaredPlusCosSquared(a, b);

    private static bool IsSinSquared(Expression expr)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power) &&
            b.Left is FunctionCallExpression f && f.Name == "sin" && f.Arguments.Count == 1)
        {
            if (b.Right is LiteralExpression l && l.Value == 2) return true;
            if (b.Right is ConstantExpression c && c.Name == "2") return true;
        }
        if (expr is BinaryExpression mul && mul.Operator.Equals(MathOperator.Multiply))
        {
            if (IsSin(mul.Left) && IsSin(mul.Right) &&
                mul.Left is FunctionCallExpression f1 && mul.Right is FunctionCallExpression f2 &&
                f1.Arguments[0].Equals(f2.Arguments[0]))
                return true;
        }
        return false;
    }

    private static bool IsCosSquared(Expression expr)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Power) &&
            b.Left is FunctionCallExpression f && f.Name == "cos" && f.Arguments.Count == 1)
        {
            if (b.Right is LiteralExpression l && l.Value == 2) return true;
            if (b.Right is ConstantExpression c && c.Name == "2") return true;
        }
        if (expr is BinaryExpression mul && mul.Operator.Equals(MathOperator.Multiply))
        {
            if (IsCos(mul.Left) && IsCos(mul.Right) &&
                mul.Left is FunctionCallExpression f1 && mul.Right is FunctionCallExpression f2 &&
                f1.Arguments[0].Equals(f2.Arguments[0]))
                return true;
        }
        return false;
    }

    private static bool IsSinCosProduct(Expression a, Expression b)
    {
        return (IsSin(a) && IsCos(b)) || (IsCos(a) && IsSin(b));
    }

    private static bool IsSin(Expression expr) => expr is FunctionCallExpression f && f.Name == "sin" && f.Arguments.Count == 1;
    private static bool IsCos(Expression expr) => expr is FunctionCallExpression f && f.Name == "cos" && f.Arguments.Count == 1;

    private static Expression ExtractAngle(Expression expr)
    {
        if (expr is FunctionCallExpression f && f.Arguments.Count == 1)
            return f.Arguments[0];
        if (expr is BinaryExpression mul && mul.Operator.Equals(MathOperator.Multiply))
        {
            if (mul.Left is FunctionCallExpression f1 && f1.Arguments.Count == 1)
                return f1.Arguments[0];
            if (mul.Right is FunctionCallExpression f2 && f2.Arguments.Count == 1)
                return f2.Arguments[0];
        }
        if (expr is BinaryExpression pow && pow.Operator.Equals(MathOperator.Power) &&
            pow.Left is FunctionCallExpression f3 && f3.Arguments.Count == 1)
            return f3.Arguments[0];
        return Expr.Literal(0);
    }
}