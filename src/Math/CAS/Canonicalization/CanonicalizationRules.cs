namespace MathVerse.Math.CAS.Canonicalization;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Immutable;

public static class CanonicalizationRules
{
    public static bool IsAssociativeOp(MathOperator op)
    {
        return op.Equals(MathOperator.Add) ||
               op.Equals(MathOperator.Multiply) ||
               op.Equals(MathOperator.And) ||
               op.Equals(MathOperator.Or) ||
               op.Equals(MathOperator.Union) ||
               op.Equals(MathOperator.Intersection);
    }

    public static bool IsCommutativeOp(MathOperator op)
    {
        return op.Equals(MathOperator.Add) ||
               op.Equals(MathOperator.Multiply) ||
               op.Equals(MathOperator.Equal) ||
               op.Equals(MathOperator.NotEqual) ||
               op.Equals(MathOperator.And) ||
               op.Equals(MathOperator.Or) ||
               op.Equals(MathOperator.Xor) ||
               op.Equals(MathOperator.Equivalent) ||
               op.Equals(MathOperator.Union) ||
               op.Equals(MathOperator.Intersection) ||
               op.Equals(MathOperator.Dot);
    }

    public static Expression FlattenAssociative(Expression expr)
    {
        if (expr is not BinaryExpression bin)
            return expr;

        if (!IsAssociativeOp(bin.Operator))
            return expr;

        var flattened = FlattenAssociativeBinary(bin);
        return flattened;
    }

    private static Expression FlattenAssociativeBinary(BinaryExpression expr)
    {
        var op = expr.Operator;
        var left = FlattenAssociative(expr.Left);
        var right = FlattenAssociative(expr.Right);

        var leftParts = left is BinaryExpression lb && lb.Operator.Equals(op) ? CollectAssociative(lb, op) : [left];
        var rightParts = right is BinaryExpression rb && rb.Operator.Equals(op) ? CollectAssociative(rb, op) : [right];

        var allParts = leftParts.Concat(rightParts).ToArray();
        if (allParts.Length == 1) return allParts[0];

        return BuildLeftAssociative(allParts, op);
    }

    private static Expression[] CollectAssociative(BinaryExpression expr, MathOperator op)
    {
        var left = expr.Left is BinaryExpression lb && lb.Operator.Equals(op) ? CollectAssociative(lb, op) : [expr.Left];
        var right = expr.Right is BinaryExpression rb && rb.Operator.Equals(op) ? CollectAssociative(rb, op) : [expr.Right];
        return left.Concat(right).ToArray();
    }

    private static Expression BuildLeftAssociative(Expression[] parts, MathOperator op)
    {
        var result = parts[0];
        for (var i = 1; i < parts.Length; i++)
            result = Expr.Binary(result, op.Symbol, parts[i]);
        return result;
    }

    public static ImmutableArray<Expression> SortCommutativeArgs(IReadOnlyList<Expression> args)
    {
        if (args.Count <= 1) return args.ToImmutableArray();

        var sorted = args.OrderBy(GetSortKey).ToImmutableArray();
        return sorted;
    }

    public static string GetSortKey(Expression expr)
    {
        return expr.Kind switch
        {
            ExpressionKind.Literal => $"0_{(expr as LiteralExpression)!.Value:0.000000}",
            ExpressionKind.Variable => $"1_{(expr as VariableExpression)!.Name}",
            ExpressionKind.Constant => $"2_{(expr as ConstantExpression)!.Name}",
            ExpressionKind.FunctionCall => $"3_{(expr as FunctionCallExpression)!.Name}",
            ExpressionKind.Unary => $"4_{(expr as UnaryExpression)!.Operator.Symbol}",
            ExpressionKind.Binary => $"5_{(expr as BinaryExpression)!.Operator.Symbol}",
            _ => $"9_{expr}"
        };
    }

    public static Expression NormalizeNegation(Expression expr)
    {
        return expr switch
        {
            UnaryExpression u when u.Operator.Equals(MathOperator.Negate) =>
                u.Operand switch
                {
                    LiteralExpression lit => Expr.Literal(-lit.Value),
                    UnaryExpression uu when uu.Operator.Equals(MathOperator.Negate) => uu.Operand,
                    BinaryExpression b when b.Operator.Equals(MathOperator.Subtract) => Expr.Add(b.Right, Expr.Negate(b.Left)),
                    BinaryExpression b when b.Operator.Equals(MathOperator.Add) =>
                        Expr.Add(Expr.Negate(b.Left), Expr.Negate(b.Right)),
                    _ => expr
                },
            BinaryExpression b when b.Operator.Equals(MathOperator.Subtract) =>
                Expr.Add(b.Left, Expr.Negate(b.Right)),
            _ => expr
        };
    }

    public static Expression NormalizeDivision(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Divide) =>
                b.Right switch
                {
                    LiteralExpression lit when lit.Value != 0 => Expr.Multiply(b.Left, Expr.Literal(1.0 / lit.Value)),
                    UnaryExpression u when u.Operator.Equals(MathOperator.Negate) => Expr.Divide(Expr.Negate(b.Left), u.Operand),
                    _ => Expr.Multiply(b.Left, Expr.Pow(b.Right, Expr.Literal(-1.0)))
                },
            _ => expr
        };
    }

    public static Expression NormalizePower(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Power) =>
                b.Right switch
                {
                    LiteralExpression lit when lit.Value == 0.5 => Expr.Call("sqrt", b.Left),
                    LiteralExpression lit when lit.Value == -1.0 => Expr.Call("inv", b.Left),
                    LiteralExpression lit when lit.Value == 2.0 => Expr.Multiply(b.Left, b.Left),
                    LiteralExpression lit when lit.Value == 3.0 => Expr.Multiply(Expr.Multiply(b.Left, b.Left), b.Left),
                    LiteralExpression lit when lit.Value < 0 => Expr.Divide(Expr.Literal(1.0), Expr.Pow(b.Left, Expr.Literal(-lit.Value))),
                    _ => b
                },
            _ => expr
        };
    }

    public static Expression CollectLikeTerms(Expression expr)
    {
        if (expr is not BinaryExpression bin || !bin.Operator.Equals(MathOperator.Add))
            return expr;

        var terms = CollectAddTerms(bin);
        var grouped = GroupLikeTerms(terms);
        var combined = grouped
            .Select(g => CombineLikeTerms(g))
            .Where(t => !IsZero(t))
            .ToArray();

        if (combined.Length == 0) return Expr.Literal(0);
        if (combined.Length == 1) return combined[0];

        return combined.Aggregate(Expr.Add);
    }

    private static List<Expression> CollectAddTerms(BinaryExpression expr)
    {
        var terms = new List<Expression>();
        CollectAddTermsRecursive(expr, terms);
        return terms;
    }

    private static void CollectAddTermsRecursive(Expression expr, List<Expression> terms)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Add))
        {
            CollectAddTermsRecursive(b.Left, terms);
            CollectAddTermsRecursive(b.Right, terms);
        }
        else
        {
            terms.Add(expr);
        }
    }

    private static List<List<Expression>> GroupLikeTerms(List<Expression> terms)
    {
        var groups = new Dictionary<string, List<Expression>>();

        foreach (var term in terms)
        {
            var key = GetTermKey(term);
            if (!groups.TryGetValue(key, out var list))
            {
                list = [];
                groups[key] = list;
            }
            list.Add(term);
        }

        return groups.Values.ToList();
    }

    private static string GetTermKey(Expression expr)
    {
        return expr switch
        {
            LiteralExpression lit => $"const:{lit.Value}",
            VariableExpression v => $"var:{v.Name}",
            BinaryExpression b when b.Operator.Equals(MathOperator.Multiply) => GetMulKey(b),
            BinaryExpression b when b.Operator.Equals(MathOperator.Power) => $"pow:{GetTermKey(b.Left)}:{GetTermKey(b.Right)}",
            FunctionCallExpression f => $"func:{f.Name}:{string.Join(",", f.Arguments.Select(GetTermKey))}",
            UnaryExpression u => $"unary:{u.Operator.Symbol}:{GetTermKey(u.Operand)}",
            _ => expr.ToString()
        };
    }

    private static string GetMulKey(BinaryExpression expr)
    {
        var factors = new List<string>();
        CollectMulFactors(expr, factors);
        factors.Sort();
        return $"mul:{string.Join(":", factors)}";
    }

    private static void CollectMulFactors(Expression expr, List<string> factors)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(MathOperator.Multiply))
        {
            CollectMulFactors(b.Left, factors);
            CollectMulFactors(b.Right, factors);
        }
        else
        {
            factors.Add(GetTermKey(expr));
        }
    }

    private static Expression CombineLikeTerms(List<Expression> terms)
    {
        if (terms.Count == 1) return terms[0];

        var first = terms[0];
        var coefficient = ExtractCoefficient(first);
        var variablePart = ExtractVariablePart(first);

        var totalCoeff = coefficient;
        for (var i = 1; i < terms.Count; i++)
        {
            totalCoeff = AddCoefficients(totalCoeff, ExtractCoefficient(terms[i]));
        }

        if (IsZero(totalCoeff)) return Expr.Literal(0);

        if (IsOne(totalCoeff)) return variablePart;

        return Expr.Multiply(totalCoeff, variablePart);
    }

    private static Expression ExtractCoefficient(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Multiply) && IsConstant(b.Left) => b.Left,
            BinaryExpression b when b.Operator.Equals(MathOperator.Multiply) && IsConstant(b.Right) => b.Right,
            LiteralExpression => expr,
            _ => Expr.Literal(1.0)
        };
    }

    private static Expression ExtractVariablePart(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Multiply) && IsConstant(b.Left) => b.Right,
            BinaryExpression b when b.Operator.Equals(MathOperator.Multiply) && IsConstant(b.Right) => b.Left,
            _ => expr
        };
    }

    private static bool IsConstant(Expression expr) => expr is LiteralExpression or ConstantExpression;

    private static bool IsZero(Expression expr) => expr is LiteralExpression lit && lit.Value == 0;

    private static bool IsOne(Expression expr) => expr is LiteralExpression lit && lit.Value == 1;

    private static Expression AddCoefficients(Expression a, Expression b)
    {
        if (a is LiteralExpression la && b is LiteralExpression lb)
            return Expr.Literal(la.Value + lb.Value);
        return Expr.Add(a, b);
    }

    public static Expression CanonicalizeFunctionArgs(FunctionCallExpression func)
    {
        if (CanonicalizationContext.Default.ExcludedFunctions.Contains(func.Name))
            return func;

        var sortedArgs = SortCommutativeArgs(func.Arguments);
        if (sortedArgs.SequenceEqual(func.Arguments))
            return func;

        return new FunctionCallExpression(func.Name, sortedArgs);
    }
}