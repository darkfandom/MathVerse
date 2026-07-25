namespace MathVerse.Math.Simplification;

/// <summary>
/// Folds fully-constant subexpressions by evaluating numeric operations
/// using <c>System.Math</c>. Operates bottom-up.
/// </summary>
public sealed class ConstantFolder : ExpressionTransformerBase
{
    /// <summary>
    /// Folds all constant subexpressions in the given expression tree.
    /// </summary>
    /// <param name="expression">The expression to fold.</param>
    /// <returns>A new expression with all evaluable constant subtrees reduced to literals.</returns>
    public Expression Fold(Expression expression) =>
        expression.Accept(this);

    /// <inheritdoc/>
    public override Expression Visit(BinaryExpression expression)
    {
        var left = expression.Left.Accept(this);
        var right = expression.Right.Accept(this);

        if (left is LiteralExpression l && right is LiteralExpression r)
        {
            var folded = EvaluateBinary(expression.Operator, l.Value, r.Value);
            if (folded.HasValue)
                return Expr.Literal(folded.Value);
        }

        return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
            ? expression
            : new BinaryExpression(expression.Operator, left, right);
    }

    /// <inheritdoc/>
    public override Expression Visit(UnaryExpression expression)
    {
        var operand = expression.Operand.Accept(this);

        if (operand is LiteralExpression lit)
        {
            var folded = EvaluateUnary(expression.Operator, lit.Value);
            if (folded.HasValue)
                return Expr.Literal(folded.Value);
        }

        return ReferenceEquals(operand, expression.Operand)
            ? expression
            : new UnaryExpression(expression.Operator, operand);
    }

    /// <inheritdoc/>
    public override Expression Visit(FunctionCallExpression expression)
    {
        var args = TransformChildren(expression.Arguments);

        if (args.Count == 1 && args[0] is LiteralExpression lit)
        {
            var folded = EvaluateFunction(expression.Name, [lit.Value]);
            if (folded.HasValue)
                return Expr.Literal(folded.Value);
        }
        else if (args.Count == 2 && args[0] is LiteralExpression l1 && args[1] is LiteralExpression l2)
        {
            var folded = EvaluateFunction(expression.Name, [l1.Value, l2.Value]);
            if (folded.HasValue)
                return Expr.Literal(folded.Value);
        }

        return ReferenceEquals(args, expression.Arguments)
            ? expression
            : new FunctionCallExpression(expression.Name, args);
    }

    private static double? EvaluateBinary(MathOperator op, double left, double right) =>
        op.Symbol switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" => right != 0.0 ? left / right : null,
            "%" => right != 0.0 ? left % right : null,
            "^" => System.Math.Pow(left, right),
            _ => null,
        };

    private static double? EvaluateUnary(MathOperator op, double value) =>
        op.Symbol switch
        {
            "-" => -value,
            "|·|" => System.Math.Abs(value),
            _ => null,
        };

    private static double? EvaluateFunction(string name, double[] args) =>
        name switch
        {
            "sin" => System.Math.Sin(args[0]),
            "cos" => System.Math.Cos(args[0]),
            "tan" => System.Math.Tan(args[0]),
            "asin" => System.Math.Asin(args[0]),
            "acos" => System.Math.Acos(args[0]),
            "atan" => System.Math.Atan(args[0]),
            "ln" => args[0] > 0.0 ? System.Math.Log(args[0]) : null,
            "log" when args.Length == 2 && args[1] > 0.0 && args[1] != 1.0 && args[0] > 0.0 =>
                System.Math.Log(args[0], args[1]),
            "log10" => args[0] > 0.0 ? System.Math.Log10(args[0]) : null,
            "exp" => System.Math.Exp(args[0]),
            "sqrt" => args[0] >= 0.0 ? System.Math.Sqrt(args[0]) : null,
            "cbrt" => System.Math.Cbrt(args[0]),
            "sinh" => System.Math.Sinh(args[0]),
            "cosh" => System.Math.Cosh(args[0]),
            "tanh" => System.Math.Tanh(args[0]),
            "abs" => System.Math.Abs(args[0]),
            _ => null,
        };
}
