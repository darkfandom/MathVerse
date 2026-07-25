namespace MathVerse.Math.Compiler.Expressions;

using System;

/// <summary>Optimizes expression ASTs before lowering. Applies constant folding and algebraic simplification.</summary>
public sealed class ExpressionOptimizer
{
    /// <summary>Optimizes the given AST and returns a new optimized AST.</summary>
    public ExpressionNode Optimize(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        ExpressionNode optimized = root;
        bool changed = true;

        while (changed)
        {
            changed = false;
            var pass1 = ConstantFold(optimized);
            if (!ReferenceEquals(pass1, optimized) && !pass1.StructuralEquals(optimized))
            {
                optimized = pass1;
                changed = true;
                continue;
            }

            var pass2 = AlgebraicSimplify(optimized);
            if (!ReferenceEquals(pass2, optimized) && !pass2.StructuralEquals(optimized))
            {
                optimized = pass2;
                changed = true;
                continue;
            }

            var pass3 = SimplifyUnary(optimized);
            if (!ReferenceEquals(pass3, optimized) && !pass3.StructuralEquals(optimized))
            {
                optimized = pass3;
                changed = true;
                continue;
            }
        }

        return optimized;
    }

    /// <summary>Evaluates constant sub-expressions and replaces them with number nodes.</summary>
    public ExpressionNode ConstantFold(ExpressionNode node)
    {
        if (node is NumberNode or VariableNode) return node;

        if (node is BinaryOpNode bin)
        {
            var left = ConstantFold(bin.Left);
            var right = ConstantFold(bin.Right);

            if (left is NumberNode ln && right is NumberNode rn)
            {
                double result = bin.Op switch
                {
                    BinaryOperator.Add => ln.Value + rn.Value,
                    BinaryOperator.Subtract => ln.Value - rn.Value,
                    BinaryOperator.Multiply => ln.Value * rn.Value,
                    BinaryOperator.Divide => ln.Value / rn.Value,
                    BinaryOperator.Power => Math.Pow(ln.Value, rn.Value),
                    _ => double.NaN,
                };
                return new NumberNode(result);
            }

            if (!ReferenceEquals(left, bin.Left) || !ReferenceEquals(right, bin.Right))
                return new BinaryOpNode(left, bin.Op, right);

            return bin;
        }

        if (node is UnaryOpNode unary)
        {
            var operand = ConstantFold(unary.Operand);
            if (operand is NumberNode num)
            {
                double result = unary.Op switch
                {
                    UnaryOperator.Negate => -num.Value,
                    UnaryOperator.Positive => num.Value,
                    _ => double.NaN,
                };
                return new NumberNode(result);
            }

            if (!ReferenceEquals(operand, unary.Operand))
                return new UnaryOpNode(unary.Op, operand);

            return unary;
        }

        if (node is FunctionNode func)
        {
            var args = new ExpressionNode[func.Arguments.Count];
            bool allConstant = true;

            for (int i = 0; i < func.Arguments.Count; i++)
            {
                args[i] = ConstantFold(func.Arguments[i]);
                if (args[i] is not NumberNode)
                    allConstant = false;
            }

            if (allConstant)
            {
                double result = EvaluateFunction(func.FunctionName, args);
                return new NumberNode(result);
            }

            bool anyChanged = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (!ReferenceEquals(args[i], func.Arguments[i]))
                {
                    anyChanged = true;
                    break;
                }
            }

            if (anyChanged)
                return new FunctionNode(func.FunctionName, args);

            return func;
        }

        return node;
    }

    /// <summary>Applies algebraic simplifications: x+0=x, x*1=x, x*0=0, x^0=1, x^1=x, 0/x=0.</summary>
    public ExpressionNode AlgebraicSimplify(ExpressionNode node)
    {
        if (node is BinaryOpNode bin)
        {
            var left = AlgebraicSimplify(bin.Left);
            var right = AlgebraicSimplify(bin.Right);

            if (left is NumberNode ln)
            {
                if (bin.Op == BinaryOperator.Add)
                {
                    if (ln.Value == 0) return right;
                    if (right is NumberNode rn) return new NumberNode(ln.Value + rn.Value);
                }
                if (bin.Op == BinaryOperator.Subtract && right is NumberNode rn2)
                    return new NumberNode(ln.Value - rn2.Value);
                if (bin.Op == BinaryOperator.Multiply)
                {
                    if (ln.Value == 0) return new NumberNode(0);
                    if (ln.Value == 1) return right;
                    if (right is NumberNode rn3) return new NumberNode(ln.Value * rn3.Value);
                }
                if (bin.Op == BinaryOperator.Divide)
                {
                    if (ln.Value == 0) return new NumberNode(0);
                    if (right is NumberNode rn4) return new NumberNode(ln.Value / rn4.Value);
                }
            }

            if (right is NumberNode rn5)
            {
                if (bin.Op == BinaryOperator.Add)
                {
                    if (rn5.Value == 0) return left;
                }
                if (bin.Op == BinaryOperator.Subtract && rn5.Value == 0)
                    return left;
                if (bin.Op == BinaryOperator.Multiply)
                {
                    if (rn5.Value == 0) return new NumberNode(0);
                    if (rn5.Value == 1) return left;
                }
                if (bin.Op == BinaryOperator.Power)
                {
                    if (rn5.Value == 0) return new NumberNode(1);
                    if (rn5.Value == 1) return left;
                }
            }

            if (!ReferenceEquals(left, bin.Left) || !ReferenceEquals(right, bin.Right))
                return new BinaryOpNode(left, bin.Op, right);

            return bin;
        }

        if (node is UnaryOpNode unary)
        {
            var operand = AlgebraicSimplify(unary.Operand);
            if (!ReferenceEquals(operand, unary.Operand))
                return new UnaryOpNode(unary.Op, operand);
            return unary;
        }

        if (node is FunctionNode func)
        {
            var args = new ExpressionNode[func.Arguments.Count];
            bool anyChanged = false;
            for (int i = 0; i < func.Arguments.Count; i++)
            {
                args[i] = AlgebraicSimplify(func.Arguments[i]);
                if (!ReferenceEquals(args[i], func.Arguments[i]))
                    anyChanged = true;
            }

            if (anyChanged)
                return new FunctionNode(func.FunctionName, args);

            return func;
        }

        return node;
    }

    /// <summary>Simplifies unary operations: +x = x, --x = x.</summary>
    public ExpressionNode SimplifyUnary(ExpressionNode node)
    {
        if (node is UnaryOpNode unary)
        {
            var operand = SimplifyUnary(unary.Operand);

            if (unary.Op == UnaryOperator.Positive)
                return operand;

            if (unary.Op == UnaryOperator.Negate && operand is UnaryOpNode inner && inner.Op == UnaryOperator.Negate)
                return SimplifyUnary(inner.Operand);

            if (unary.Op == UnaryOperator.Negate && operand is NumberNode num)
                return new NumberNode(-num.Value);

            if (!ReferenceEquals(operand, unary.Operand))
                return new UnaryOpNode(unary.Op, operand);

            return unary;
        }

        if (node is BinaryOpNode bin)
        {
            var left = SimplifyUnary(bin.Left);
            var right = SimplifyUnary(bin.Right);
            if (!ReferenceEquals(left, bin.Left) || !ReferenceEquals(right, bin.Right))
                return new BinaryOpNode(left, bin.Op, right);
            return bin;
        }

        if (node is FunctionNode func)
        {
            var args = new ExpressionNode[func.Arguments.Count];
            bool anyChanged = false;
            for (int i = 0; i < func.Arguments.Count; i++)
            {
                args[i] = SimplifyUnary(func.Arguments[i]);
                if (!ReferenceEquals(args[i], func.Arguments[i]))
                    anyChanged = true;
            }
            if (anyChanged)
                return new FunctionNode(func.FunctionName, args);
            return func;
        }

        return node;
    }

    private static double EvaluateFunction(string name, ExpressionNode[] args)
    {
        if (args.Length == 0) return double.NaN;
        if (args[0] is not NumberNode num) return double.NaN;

        return name.ToLowerInvariant() switch
        {
            "sin" => Math.Sin(num.Value),
            "cos" => Math.Cos(num.Value),
            "tan" => Math.Tan(num.Value),
            "asin" => Math.Asin(num.Value),
            "acos" => Math.Acos(num.Value),
            "atan" => Math.Atan(num.Value),
            "ln" => Math.Log(num.Value),
            "log" => Math.Log10(num.Value),
            "exp" => Math.Exp(num.Value),
            "sqrt" => Math.Sqrt(num.Value),
            "abs" => Math.Abs(num.Value),
            "ceil" => Math.Ceiling(num.Value),
            "floor" => Math.Floor(num.Value),
            _ => double.NaN,
        };
    }
}
