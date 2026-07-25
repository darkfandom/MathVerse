namespace MathVerse.Math.Simplification;

/// <summary>
/// Main simplification engine that applies algebraic rules and constant folding
/// to expression trees. Applies rules bottom-up until a fixpoint is reached.
/// </summary>
public sealed class ExpressionSimplifier
{
    private readonly Dictionary<Expression, Expression> _cache = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Simplifies the given expression using the specified options.
    /// </summary>
    /// <param name="input">The expression to simplify.</param>
    /// <param name="options">Optional simplification configuration. Uses <see cref="SimplificationOptions.Default"/> when <c>null</c>.</param>
    /// <returns>The simplified expression.</returns>
    public Expression Simplify(Expression input, SimplificationOptions? options = null)
    {
        Guard.NotNull(input, nameof(input));
        options ??= SimplificationOptions.Default;

        _cache.Clear();

        var rules = BuildRuleList(options);
        var folder = new ConstantFolder();
        var current = input;

        for (var iteration = 0; iteration < options.MaxIterations; iteration++)
        {
            Expression previous;

            if (options.EnableConstantFolding)
                current = folder.Fold(current);

            if (rules.Count > 0)
            {
                var visitor = new SimplificationVisitor(rules, _cache);
                current = current.Accept(visitor);
            }

            previous = current;
            current = options.EnableConstantFolding ? folder.Fold(current) : current;

            if (ReferenceEquals(current, previous))
                return current;
        }

        return current;
    }

    /// <summary>
    /// Clears the simplification cache.
    /// </summary>
    public void ClearCache() => _cache.Clear();

    private static List<SimplificationRule> BuildRuleList(SimplificationOptions options)
    {
        var rules = new List<SimplificationRule>();

        if (options.EnableArithmeticRules)
            rules.AddRange(RuleCollection.ArithmeticRules);

        if (options.EnablePowerRules)
            rules.AddRange(RuleCollection.PowerRules);

        if (options.EnableLogRules)
            rules.AddRange(RuleCollection.LogRules);

        if (options.EnableTrigRules)
            rules.AddRange(RuleCollection.TrigRules);

        rules.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return rules;
    }

    private sealed class SimplificationVisitor(
        List<SimplificationRule> rules,
        Dictionary<Expression, Expression> cache) : ExpressionTransformerBase
    {
        public override Expression Visit(LiteralExpression expression) => expression;

        public override Expression Visit(VariableExpression expression) => expression;

        public override Expression Visit(ConstantExpression expression) => expression;

        public override Expression Visit(BooleanExpression expression) => expression;

        public override Expression Visit(BinaryExpression expression)
        {
            var transformed = base.Visit(expression);
            return ApplyRules(transformed);
        }

        public override Expression Visit(UnaryExpression expression)
        {
            var transformed = base.Visit(expression);
            return ApplyRules(transformed);
        }

        public override Expression Visit(FunctionCallExpression expression)
        {
            var transformed = base.Visit(expression);
            return ApplyRules(transformed);
        }

        public override Expression Visit(RelationExpression expression)
        {
            var transformed = base.Visit(expression);
            return ApplyRules(transformed);
        }

        public override Expression Visit(ParameterExpression expression) => expression;

        private Expression ApplyRules(Expression expression)
        {
            if (cache.TryGetValue(expression, out var cached))
                return cached;

            var current = expression;

            foreach (var rule in rules)
            {
                var rewritten = rule.TryRewrite(current);
                if (rewritten is not null)
                    current = rewritten;
            }

            cache[expression] = current;
            return current;
        }
    }
}
