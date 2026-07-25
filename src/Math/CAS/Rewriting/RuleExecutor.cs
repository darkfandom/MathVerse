namespace MathVerse.Math.CAS.Rewriting;

using MathVerse.Math.Expressions;
using MathVerse.Math.CAS.PatternMatching;
using System.Collections.Immutable;

public static class RuleExecutor
{
    public static RewriteStep Execute(Expression expr, RewriteRule rule, RewriteDirection direction)
    {
        return direction switch
        {
            RewriteDirection.TopDown => ExecuteTopDown(expr, rule),
            RewriteDirection.BottomUp => ExecuteBottomUp(expr, rule),
            RewriteDirection.All => ExecuteAll(expr, rule),
            _ => ExecuteTopDown(expr, rule)
        };
    }

    public static RewriteStep Execute(Expression expr, ImmutableArray<RewriteRule> rules, RewriteDirection direction)
    {
        var current = expr;
        var lastStep = new RewriteStep();

        foreach (var rule in rules.OrderByDescending(r => r.Priority))
        {
            var step = Execute(current, rule, direction);

            if (!ExpressionEqualityComparer.Instance.Equals(step.After, step.Before))
            {
                current = step.After;
                lastStep = step;
                break;
            }
        }

        return lastStep with { After = current };
    }

    private static RewriteStep ExecuteTopDown(Expression expr, RewriteRule rule)
    {
        var matchResult = PatternMatcher.Instance.Match(rule.Pattern, expr);
        if (matchResult.Success && (rule.Condition?.Invoke(expr) ?? true))
        {
            var replacement = SubstitutePattern(rule.Replacement, matchResult.Bindings);
            return new RewriteStep
            {
                Rule = rule,
                Before = expr,
                After = replacement,
                Bindings = matchResult.Bindings
            };
        }

        var children = GetChildren(expr);
        if (children.Count == 0)
            return new RewriteStep { Rule = rule, Before = expr, After = expr };

        var newChildren = new Expression[children.Count];
        bool changed = false;

        for (int i = 0; i < children.Count; i++)
        {
            var step = ExecuteTopDown(children[i], rule);
            newChildren[i] = step.After;
            if (!ExpressionEqualityComparer.Instance.Equals(step.After, step.Before))
                changed = true;
        }

        if (!changed)
            return new RewriteStep { Rule = rule, Before = expr, After = expr };

        var newExpr = RebuildExpression(expr, newChildren);
        return new RewriteStep
        {
            Rule = rule,
            Before = expr,
            After = newExpr,
            Bindings = ImmutableDictionary<string, Expression>.Empty
        };
    }

    private static RewriteStep ExecuteBottomUp(Expression expr, RewriteRule rule)
    {
        var children = GetChildren(expr);
        if (children.Count == 0)
        {
            var matchResult = PatternMatcher.Instance.Match(rule.Pattern, expr);
            if (matchResult.Success && (rule.Condition?.Invoke(expr) ?? true))
            {
                var replacement = SubstitutePattern(rule.Replacement, matchResult.Bindings);
                return new RewriteStep
                {
                    Rule = rule,
                    Before = expr,
                    After = replacement,
                    Bindings = matchResult.Bindings
                };
            }
            return new RewriteStep { Rule = rule, Before = expr, After = expr };
        }

        var newChildren = new Expression[children.Count];
        bool changed = false;

        for (int i = 0; i < children.Count; i++)
        {
            var step = ExecuteBottomUp(children[i], rule);
            newChildren[i] = step.After;
            if (!ExpressionEqualityComparer.Instance.Equals(step.After, step.Before))
                changed = true;
        }

        var rebuilt = changed ? RebuildExpression(expr, newChildren) : expr;

        var rootMatchResult = PatternMatcher.Instance.Match(rule.Pattern, rebuilt);
        if (rootMatchResult.Success && (rule.Condition?.Invoke(rebuilt) ?? true))
        {
            var replacement = SubstitutePattern(rule.Replacement, rootMatchResult.Bindings);
            return new RewriteStep
            {
                Rule = rule,
                Before = expr,
                After = replacement,
                Bindings = rootMatchResult.Bindings
            };
        }

        return new RewriteStep
        {
            Rule = rule,
            Before = expr,
            After = rebuilt,
            Bindings = ImmutableDictionary<string, Expression>.Empty
        };
    }

    private static RewriteStep ExecuteAll(Expression expr, RewriteRule rule)
    {
        var topDown = ExecuteTopDown(expr, rule);
        if (!ExpressionEqualityComparer.Instance.Equals(topDown.After, topDown.Before))
            return topDown;

        return ExecuteBottomUp(expr, rule);
    }

    private static Expression SubstitutePattern(Expression pattern, ImmutableDictionary<string, Expression> bindings)
    {
        return new PatternSubstitutor(bindings).Visit(pattern);
    }

    private static IReadOnlyList<Expression> GetChildren(Expression expr)
    {
        return expr switch
        {
            BinaryExpression b => new[] { b.Left, b.Right },
            UnaryExpression u => new[] { u.Operand },
            FunctionCallExpression f => f.Arguments.ToArray(),
            _ => Array.Empty<Expression>()
        };
    }

    private static Expression RebuildExpression(Expression original, Expression[] newChildren)
    {
        return original switch
        {
            BinaryExpression b => new BinaryExpression(b.Operator, newChildren[0], newChildren[1]),
            UnaryExpression u => new UnaryExpression(u.Operator, newChildren[0]),
            FunctionCallExpression f => new FunctionCallExpression(f.Name, newChildren),
            _ => original
        };
    }

    private sealed class PatternSubstitutor : IExpressionVisitor<Expression>
    {
        private readonly ImmutableDictionary<string, Expression> _bindings;

        public PatternSubstitutor(ImmutableDictionary<string, Expression> bindings)
        {
            _bindings = bindings;
        }

        public Expression Visit(Expression expression)
        {
            return expression switch
            {
                LiteralExpression l => l,
                VariableExpression v => _bindings.TryGetValue(v.Name, out var replacement) ? replacement : v,
                BinaryExpression b => VisitBinary(b),
                UnaryExpression u => VisitUnary(u),
                FunctionCallExpression f => VisitFunctionCall(f),
                _ => expression
            };
        }

        private Expression VisitBinary(BinaryExpression expression)
        {
            var left = Visit(expression.Left);
            var right = Visit(expression.Right);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right)
                ? expression
                : new BinaryExpression(expression.Operator, left, right);
        }

        private Expression VisitUnary(UnaryExpression expression)
        {
            var operand = Visit(expression.Operand);
            return ReferenceEquals(operand, expression.Operand)
                ? expression
                : new UnaryExpression(expression.Operator, operand);
        }

        private Expression VisitFunctionCall(FunctionCallExpression expression)
        {
            var args = expression.Arguments.Select(Visit).ToArray();
            if (args.SequenceEqual(expression.Arguments))
                return expression;
            return new FunctionCallExpression(expression.Name, args);
        }
    }

    public interface IExpressionVisitor<TResult>
    {
        TResult Visit(Expression expression);
    }
}