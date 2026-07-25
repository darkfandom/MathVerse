namespace MathVerse.Math.Rewriting;

using MathVerse.Math.Expressions;

/// <summary>
/// Defines a rewrite rule that transforms expressions.
/// </summary>
public sealed class RewriteRule
{
    /// <summary>Initializes a rewrite rule.</summary>
    public RewriteRule(string name, Func<Expression, Expression?> tryRewrite, int priority = 0)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        TryRewrite = Guard.NotNull(tryRewrite, nameof(tryRewrite));
        Priority = priority;
    }

    /// <summary>Gets the rule name.</summary>
    public string Name { get; }

    /// <summary>Gets the rewrite function. Returns null if the rule doesn't apply.</summary>
    public Func<Expression, Expression?> TryRewrite { get; }

    /// <summary>Gets the rule priority (higher executes first).</summary>
    public int Priority { get; }

    /// <summary>Creates a rule that matches a condition and applies a transformation.</summary>
    public static RewriteRule Create(string name, Func<Expression, bool> condition, Func<Expression, Expression> transform, int priority = 0) =>
        new(name, expr => condition(expr) ? transform(expr) : null, priority);
}

/// <summary>
/// A named collection of rewrite rules.
/// </summary>
public sealed class RuleSet
{
    private readonly List<RewriteRule> _rules = [];

    /// <summary>Initializes an empty rule set.</summary>
    public RuleSet() { }

    /// <summary>Initializes a rule set with the specified rules.</summary>
    public RuleSet(IEnumerable<RewriteRule> rules)
    {
        _rules.AddRange(rules);
    }

    /// <summary>Gets the rules in priority order.</summary>
    public IReadOnlyList<RewriteRule> Rules => _rules.OrderByDescending(r => r.Priority).ToList();

    /// <summary>Adds a rule to the set.</summary>
    public void Add(RewriteRule rule)
    {
        _rules.Add(Guard.NotNull(rule, nameof(rule)));
    }

    /// <summary>Adds multiple rules to the set.</summary>
    public void AddRange(IEnumerable<RewriteRule> rules)
    {
        _rules.AddRange(rules);
    }
}
