namespace MathVerse.Math.CAS.Evaluation;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Immutable;
using System.Numerics;

public static class NumericEvaluator
{
    public static Expression EvaluateDouble(Expression expr, ImmutableDictionary<string, double> vars)
    {
        return expr switch
        {
            LiteralExpression l => l,
            VariableExpression v when vars.TryGetValue(v.Name, out var val) => Expr.Literal(val),
            ConstantExpression c => Expr.Literal(c.Value),
            BinaryExpression b => EvaluateBinary(b, vars),
            UnaryExpression u => EvaluateUnary(u, vars),
            FunctionCallExpression f => EvaluateFunction(f, vars),
            _ => expr
        };
    }

    public static Expression EvaluateComplex(Expression expr, ImmutableDictionary<string, Complex> vars)
    {
        return expr switch
        {
            LiteralExpression l => Expr.Call("complex", Expr.Literal(l.Value), Expr.Literal(0)),
            VariableExpression v when vars.TryGetValue(v.Name, out var val) => 
                Expr.Call("complex", Expr.Literal(val.Real), Expr.Literal(val.Imaginary)),
            ConstantExpression c => Expr.Call("complex", Expr.Literal(c.Value), Expr.Literal(0)),
            BinaryExpression b => EvaluateBinaryComplex(b, vars),
            UnaryExpression u => EvaluateUnaryComplex(u, vars),
            FunctionCallExpression f => EvaluateFunctionComplex(f, vars),
            _ => expr
        };
    }

    private static Expression EvaluateBinary(BinaryExpression expr, ImmutableDictionary<string, double> vars)
    {
        var left = EvaluateDouble(expr.Left, vars);
        var right = EvaluateDouble(expr.Right, vars);

        if (left is LiteralExpression ll && right is LiteralExpression rl)
        {
            var result = expr.Operator switch
            {
                _ when expr.Operator.Equals(MathOperator.Add) => ll.Value + rl.Value,
                _ when expr.Operator.Equals(MathOperator.Subtract) => ll.Value - rl.Value,
                _ when expr.Operator.Equals(MathOperator.Multiply) => ll.Value * rl.Value,
                _ when expr.Operator.Equals(MathOperator.Divide) && rl.Value != 0 => ll.Value / rl.Value,
                _ when expr.Operator.Equals(MathOperator.Modulo) && rl.Value != 0 => ll.Value % rl.Value,
                _ when expr.Operator.Equals(MathOperator.Power) => System.Math.Pow(ll.Value, rl.Value),
                _ => double.NaN
            };

            if (!double.IsNaN(result))
                return Expr.Literal(result);
        }

        return new BinaryExpression(expr.Operator, left, right);
    }

    private static Expression EvaluateUnary(UnaryExpression expr, ImmutableDictionary<string, double> vars)
    {
        var operand = EvaluateDouble(expr.Operand, vars);

        if (operand is LiteralExpression l)
        {
            var result = expr.Operator switch
            {
                _ when expr.Operator.Equals(MathOperator.Negate) => -l.Value,
                _ when expr.Operator.Equals(MathOperator.Abs) => System.Math.Abs(l.Value),
                _ => double.NaN
            };

            if (!double.IsNaN(result))
                return Expr.Literal(result);
        }

        return new UnaryExpression(expr.Operator, operand);
    }

    private static Expression EvaluateFunction(FunctionCallExpression expr, ImmutableDictionary<string, double> vars)
    {
        var args = expr.Arguments.Select(a => EvaluateDouble(a, vars)).ToArray();

        if (args.All(a => a is LiteralExpression))
        {
            var values = args.Cast<LiteralExpression>().Select(a => a.Value).ToArray();
            var result = EvaluateBuiltinFunction(expr.Name, values);

            if (!double.IsNaN(result))
                return Expr.Literal(result);
        }

        return new FunctionCallExpression(expr.Name, args);
    }

    private static double EvaluateBuiltinFunction(string name, double[] args)
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

    private static Expression EvaluateBinaryComplex(BinaryExpression expr, ImmutableDictionary<string, Complex> vars)
    {
        var left = EvaluateComplex(expr.Left, vars);
        var right = EvaluateComplex(expr.Right, vars);

        if (left is FunctionCallExpression lc && lc.Name == "complex" && lc.Arguments.Count == 2 &&
            lc.Arguments[0] is LiteralExpression ll && lc.Arguments[1] is LiteralExpression rl &&
            right is FunctionCallExpression rc && rc.Name == "complex" && rc.Arguments.Count == 2 &&
            rc.Arguments[0] is LiteralExpression rll && rc.Arguments[1] is LiteralExpression rrl)
        {
            var lv = new Complex(ll.Value, rl.Value);
            var rv = new Complex(rll.Value, rrl.Value);
            Complex result = expr.Operator switch
            {
                _ when expr.Operator.Equals(MathOperator.Add) => lv + rv,
                _ when expr.Operator.Equals(MathOperator.Subtract) => lv - rv,
                _ when expr.Operator.Equals(MathOperator.Multiply) => lv * rv,
                _ when expr.Operator.Equals(MathOperator.Divide) => rv != 0 ? lv / rv : Complex.NaN,
                _ when expr.Operator.Equals(MathOperator.Power) => Complex.Pow(lv, rv),
                _ => Complex.NaN
            };

            if (!Complex.IsNaN(result))
                return Expr.Call("complex", Expr.Literal(result.Real), Expr.Literal(result.Imaginary));
        }

        return new BinaryExpression(expr.Operator, left, right);
    }

    private static Expression EvaluateUnaryComplex(UnaryExpression expr, ImmutableDictionary<string, Complex> vars)
    {
        var operand = EvaluateComplex(expr.Operand, vars);

        if (operand is FunctionCallExpression fc && fc.Name == "complex" && fc.Arguments.Count == 2 &&
            fc.Arguments[0] is LiteralExpression ll && fc.Arguments[1] is LiteralExpression rl)
        {
            var val = new Complex(ll.Value, rl.Value);
            Complex result = expr.Operator switch
            {
                _ when expr.Operator.Equals(MathOperator.Negate) => -val,
                _ when expr.Operator.Equals(MathOperator.Abs) => Complex.Abs(val),
                _ => Complex.NaN
            };

            if (!Complex.IsNaN(result))
                return Expr.Call("complex", Expr.Literal(result.Real), Expr.Literal(result.Imaginary));
        }

        return new UnaryExpression(expr.Operator, operand);
    }

    private static Expression EvaluateFunctionComplex(FunctionCallExpression expr, ImmutableDictionary<string, Complex> vars)
    {
        var args = expr.Arguments.Select(a => EvaluateComplex(a, vars)).ToArray();

        if (args.All(a => a is FunctionCallExpression fc && fc.Name == "complex" && fc.Arguments.Count == 2 &&
            fc.Arguments[0] is LiteralExpression && fc.Arguments[1] is LiteralExpression))
        {
            var values = args.Select(a =>
            {
                var fc = (FunctionCallExpression)a;
                return new Complex(((LiteralExpression)fc.Arguments[0]).Value, ((LiteralExpression)fc.Arguments[1]).Value);
            }).ToArray();

            Complex result = expr.Name.ToLowerInvariant() switch
            {
                "sin" => Complex.Sin(values[0]),
                "cos" => Complex.Cos(values[0]),
                "tan" => Complex.Tan(values[0]),
                "exp" => Complex.Exp(values[0]),
                "log" or "ln" => Complex.Log(values[0]),
                "sqrt" => Complex.Sqrt(values[0]),
                _ => Complex.NaN
            };

            if (!Complex.IsNaN(result))
                return Expr.Call("complex", Expr.Literal(result.Real), Expr.Literal(result.Imaginary));
        }

        return new FunctionCallExpression(expr.Name, args);
    }
}