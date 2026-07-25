namespace MathVerse.Math.CAS.Expansion;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Generic;
using System.Collections.Immutable;

public class DistributiveExpander
{
    public Expression ExpandMulOverAdd(Expression expr, List<string> steps)
    {
        return ExpandMulOverAddRecursive(expr, steps);
    }

    private Expression ExpandMulOverAddRecursive(Expression expr, List<string> steps)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Multiply) =>
                ExpandMulOverAddBinary(b, steps),
            BinaryExpression b => new BinaryExpression(b.Operator,
                ExpandMulOverAddRecursive(b.Left, steps),
                ExpandMulOverAddRecursive(b.Right, steps)),
            UnaryExpression u => new UnaryExpression(u.Operator, ExpandMulOverAddRecursive(u.Operand, steps)),
            FunctionCallExpression f => new FunctionCallExpression(f.Name, f.Arguments.Select(a => ExpandMulOverAddRecursive(a, steps)).ToArray()),
            _ => expr
        };
    }

    private Expression ExpandMulOverAddBinary(BinaryExpression expr, List<string> steps)
    {
        var left = ExpandMulOverAddRecursive(expr.Left, steps);
        var right = ExpandMulOverAddRecursive(expr.Right, steps);

        if (left is BinaryExpression bl && (bl.Operator.Equals(MathOperator.Add) || bl.Operator.Equals(MathOperator.Subtract)))
        {
            steps.Add("DistributeMulOverAdd");
            var isSub = bl.Operator.Equals(MathOperator.Subtract);
            var distributedLeft = isSub
                ? Expr.Subtract(Expr.Multiply(bl.Left, right), Expr.Multiply(bl.Right, right))
                : Expr.Add(Expr.Multiply(bl.Left, right), Expr.Multiply(bl.Right, right));
            return ExpandMulOverAddRecursive(distributedLeft, steps);
        }

        if (right is BinaryExpression br && (br.Operator.Equals(MathOperator.Add) || br.Operator.Equals(MathOperator.Subtract)))
        {
            steps.Add("DistributeMulOverAdd");
            var isSub = br.Operator.Equals(MathOperator.Subtract);
            var distributedRight = isSub
                ? Expr.Subtract(Expr.Multiply(left, br.Left), Expr.Multiply(left, br.Right))
                : Expr.Add(Expr.Multiply(left, br.Left), Expr.Multiply(left, br.Right));
            return ExpandMulOverAddRecursive(distributedRight, steps);
        }

        return new BinaryExpression(MathOperator.Multiply, left, right);
    }

    public Expression ExpandDivOverAdd(Expression expr, List<string> steps)
    {
        return ExpandDivOverAddRecursive(expr, steps);
    }

    private Expression ExpandDivOverAddRecursive(Expression expr, List<string> steps)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Divide) =>
                ExpandDivOverAddBinary(b, steps),
            BinaryExpression b => new BinaryExpression(b.Operator,
                ExpandDivOverAddRecursive(b.Left, steps),
                ExpandDivOverAddRecursive(b.Right, steps)),
            UnaryExpression u => new UnaryExpression(u.Operator, ExpandDivOverAddRecursive(u.Operand, steps)),
            FunctionCallExpression f => new FunctionCallExpression(f.Name, f.Arguments.Select(a => ExpandDivOverAddRecursive(a, steps)).ToArray()),
            _ => expr
        };
    }

    private Expression ExpandDivOverAddBinary(BinaryExpression expr, List<string> steps)
    {
        var numerator = ExpandDivOverAddRecursive(expr.Left, steps);
        var denominator = ExpandDivOverAddRecursive(expr.Right, steps);

        if (numerator is BinaryExpression n && (n.Operator.Equals(MathOperator.Add) || n.Operator.Equals(MathOperator.Subtract)))
        {
            steps.Add("DistributeDivOverAdd");
            var isSub = n.Operator.Equals(MathOperator.Subtract);
            var distributed = isSub
                ? Expr.Subtract(Expr.Divide(n.Left, denominator), Expr.Divide(n.Right, denominator))
                : Expr.Add(Expr.Divide(n.Left, denominator), Expr.Divide(n.Right, denominator));
            return ExpandDivOverAddRecursive(distributed, steps);
        }

        return new BinaryExpression(MathOperator.Divide, numerator, denominator);
    }

    public Expression ExpandPowOverMul(Expression expr, List<string> steps)
    {
        return ExpandPowOverMulRecursive(expr, steps);
    }

    private Expression ExpandPowOverMulRecursive(Expression expr, List<string> steps)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Power) =>
                ExpandPowOverMulBinary(b, steps),
            BinaryExpression b => new BinaryExpression(b.Operator,
                ExpandPowOverMulRecursive(b.Left, steps),
                ExpandPowOverMulRecursive(b.Right, steps)),
            UnaryExpression u => new UnaryExpression(u.Operator, ExpandPowOverMulRecursive(u.Operand, steps)),
            FunctionCallExpression f => new FunctionCallExpression(f.Name, f.Arguments.Select(a => ExpandPowOverMulRecursive(a, steps)).ToArray()),
            _ => expr
        };
    }

    private Expression ExpandPowOverMulBinary(BinaryExpression expr, List<string> steps)
    {
        var baseExpr = ExpandPowOverMulRecursive(expr.Left, steps);
        var expExpr = ExpandPowOverMulRecursive(expr.Right, steps);

        if (baseExpr is BinaryExpression bm && bm.Operator.Equals(MathOperator.Multiply))
        {
            steps.Add("DistributePowOverMul");
            var factors = FlattenMul(bm);
            var results = factors.Select(f => Expr.Pow(f, expExpr)).ToArray();
            return ExpandPowOverMulRecursive(BuildMulChain(results), steps);
        }

        if (baseExpr is BinaryExpression bd && bd.Operator.Equals(MathOperator.Divide))
        {
            steps.Add("DistributePowOverDiv");
            var numPow = Expr.Pow(bd.Left, expExpr);
            var denPow = Expr.Pow(bd.Right, expExpr);
            return ExpandPowOverMulRecursive(Expr.Divide(numPow, denPow), steps);
        }

        return new BinaryExpression(MathOperator.Power, baseExpr, expExpr);
    }

    public Expression ExpandFunctionOverAdd(Expression expr, List<string> steps)
    {
        return ExpandFunctionOverAddRecursive(expr, steps);
    }

    private Expression ExpandFunctionOverAddRecursive(Expression expr, List<string> steps)
    {
        return expr switch
        {
            FunctionCallExpression f => ExpandFunctionCall(f, steps),
            BinaryExpression b => new BinaryExpression(b.Operator,
                ExpandFunctionOverAddRecursive(b.Left, steps),
                ExpandFunctionOverAddRecursive(b.Right, steps)),
            UnaryExpression u => new UnaryExpression(u.Operator, ExpandFunctionOverAddRecursive(u.Operand, steps)),
            _ => expr
        };
    }

    private Expression ExpandFunctionCall(FunctionCallExpression expr, List<string> steps)
    {
        var args = expr.Arguments.Select(a => ExpandFunctionOverAddRecursive(a, steps)).ToArray();
        var func = new FunctionCallExpression(expr.Name, args);

        if (expr.Name == "sin" || expr.Name == "cos" || expr.Name == "tan")
        {
            foreach (var arg in args)
            {
                if (arg is BinaryExpression b && (b.Operator.Equals(MathOperator.Add) || b.Operator.Equals(MathOperator.Subtract)))
                {
                    steps.Add($"ExpandTrigOverAdd:{expr.Name}");
                    return ApplyTrigAngleSum(expr.Name, b);
                }
            }
        }

        if (expr.Name == "exp")
        {
            foreach (var arg in args)
            {
                if (arg is BinaryExpression b && b.Operator.Equals(MathOperator.Add))
                {
                    steps.Add("ExpandExpOverAdd");
                    return Expr.Multiply(Expr.Call("exp", b.Left), Expr.Call("exp", b.Right));
                }
            }
        }

        if (expr.Name == "log" || expr.Name == "ln")
        {
            foreach (var arg in args)
            {
                if (arg is BinaryExpression b)
                {
                    if (b.Operator.Equals(MathOperator.Multiply))
                    {
                        steps.Add($"ExpandLogOverMul:{expr.Name}");
                        return Expr.Add(Expr.Call(expr.Name, b.Left), Expr.Call(expr.Name, b.Right));
                    }
                    if (b.Operator.Equals(MathOperator.Divide))
                    {
                        steps.Add($"ExpandLogOverDiv:{expr.Name}");
                        return Expr.Subtract(Expr.Call(expr.Name, b.Left), Expr.Call(expr.Name, b.Right));
                    }
                    if (b.Operator.Equals(MathOperator.Power))
                    {
                        steps.Add($"ExpandLogOverPow:{expr.Name}");
                        return Expr.Multiply(b.Right, Expr.Call(expr.Name, b.Left));
                    }
                }
            }
        }

        return func;
    }

    private Expression ApplyTrigAngleSum(string func, BinaryExpression expr)
    {
        var A = expr.Left;
        var B = expr.Right;
        var isSub = expr.Operator.Equals(MathOperator.Subtract);

        return func switch
        {
            "sin" => isSub
                ? Expr.Subtract(Expr.Multiply(Expr.Call("sin", A), Expr.Call("cos", B)),
                                Expr.Multiply(Expr.Call("cos", A), Expr.Call("sin", B)))
                : Expr.Add(Expr.Multiply(Expr.Call("sin", A), Expr.Call("cos", B)),
                           Expr.Multiply(Expr.Call("cos", A), Expr.Call("sin", B))),
            "cos" => isSub
                ? Expr.Add(Expr.Multiply(Expr.Call("cos", A), Expr.Call("cos", B)),
                           Expr.Multiply(Expr.Call("sin", A), Expr.Call("sin", B)))
                : Expr.Subtract(Expr.Multiply(Expr.Call("cos", A), Expr.Call("cos", B)),
                                Expr.Multiply(Expr.Call("sin", A), Expr.Call("sin", B))),
            "tan" => isSub
                ? Expr.Divide(Expr.Subtract(Expr.Call("tan", A), Expr.Call("tan", B)),
                              Expr.Add(Expr.Literal(1), Expr.Multiply(Expr.Call("tan", A), Expr.Call("tan", B))))
                : Expr.Divide(Expr.Add(Expr.Call("tan", A), Expr.Call("tan", B)),
                              Expr.Subtract(Expr.Literal(1), Expr.Multiply(Expr.Call("tan", A), Expr.Call("tan", B)))),
            _ => Expr.Call(func, expr)
        };
    }

    private List<Expression> FlattenMul(BinaryExpression expr)
    {
        var factors = new List<Expression>();
        FlattenMulRecursive(expr, factors);
        return factors;
    }

    private void FlattenMulRecursive(Expression expr, List<Expression> factors)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            FlattenMulRecursive(b.Left, factors);
            FlattenMulRecursive(b.Right, factors);
        }
        else
        {
            factors.Add(expr);
        }
    }

    private Expression BuildMulChain(Expression[] factors)
    {
        if (factors.Length == 0) return Expr.Literal(1);
        var result = factors[0];
        for (var i = 1; i < factors.Length; i++)
            result = Expr.Multiply(result, factors[i]);
        return result;
    }
}