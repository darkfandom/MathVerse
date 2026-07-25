namespace MathVerse.Math.Rewriting;

using MathVerse.Math.Expressions;
using MathVerse.Math.Visitors;

/// <summary>
/// Context provided to rewrite rules during application.
/// </summary>
public sealed class RewriteContext
{
    /// <summary>Initializes a rewrite context.</summary>
    public RewriteContext(Expression root, int passNumber)
    {
        Root = Guard.NotNull(root, nameof(root));
        PassNumber = passNumber;
    }

    /// <summary>Gets the root expression being rewritten.</summary>
    public Expression Root { get; }

    /// <summary>Gets the current pass number (0-based).</summary>
    public int PassNumber { get; }

    /// <summary>Gets or sets metadata for the rewrite session.</summary>
    public Dictionary<string, object> Metadata { get; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Applies rewrite rules to expression trees.
/// </summary>
public sealed class RewriteEngine
{
    private readonly RuleSet _ruleSet;

    /// <summary>Initializes a rewrite engine with the specified rule set.</summary>
    public RewriteEngine(RuleSet ruleSet)
    {
        _ruleSet = Guard.NotNull(ruleSet, nameof(ruleSet));
    }

    /// <summary>Applies all rules once (bottom-up).</summary>
    public Expression ApplyOnce(Expression expression)
    {
        var visitor = new RuleApplyingVisitor(_ruleSet);
        return visitor.Transform(expression);
    }

    /// <summary>Applies rules repeatedly until no more changes occur (fixpoint).</summary>
    public Expression ApplyToFixpoint(Expression expression, int maxPasses = 100)
    {
        var current = expression;

        for (var pass = 0; pass < maxPasses; pass++)
        {
            var visitor = new RuleApplyingVisitor(_ruleSet);
            var next = visitor.Transform(current);

            if (next.Equals(current))
                return current;

            current = next;
        }

        return current;
    }

    /// <summary>Applies rules up to the specified number of passes.</summary>
    public Expression ApplyPasses(Expression expression, int passes)
    {
        var current = expression;

        for (var pass = 0; pass < passes; pass++)
        {
            var visitor = new RuleApplyingVisitor(_ruleSet);
            var next = visitor.Transform(current);

            if (next.Equals(current))
                return current;

            current = next;
        }

        return current;
    }

    private sealed class RuleApplyingVisitor : ExpressionTransformerBase
    {
        private readonly RuleSet _rules;

        public RuleApplyingVisitor(RuleSet rules)
        {
            _rules = rules;
        }

        public override Expression Visit(BinaryExpression expression)
        {
            var result = base.Visit(expression);
            return ApplyRules(result);
        }

        public override Expression Visit(UnaryExpression expression)
        {
            var result = base.Visit(expression);
            return ApplyRules(result);
        }

        public override Expression Visit(FunctionCallExpression expression)
        {
            var result = base.Visit(expression);
            return ApplyRules(result);
        }

        public override Expression Visit(LiteralExpression expression) =>
            ApplyRules(expression);

        public override Expression Visit(VariableExpression expression) =>
            ApplyRules(expression);

        public override Expression Visit(ConstantExpression expression) =>
            ApplyRules(expression);

        public override Expression Visit(RelationExpression expression)
        {
            var result = base.Visit(expression);
            return ApplyRules(result);
        }

        private Expression ApplyRules(Expression expression)
        {
            var current = expression;

            foreach (var rule in _rules.Rules)
            {
                var rewritten = rule.TryRewrite(current);
                if (rewritten is not null)
                    current = rewritten;
            }

            return current;
        }
    }
}
