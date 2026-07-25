namespace MathVerse.Math.CAS.Rewriting;

using MathVerse.Math.CAS.PatternMatching;
using MathVerse.Math.Expressions;

public enum RewriteDirection
{
    TopDown,
    BottomUp,
    All
}

public sealed record RewriteRule
{
    public Pattern Pattern { get; init; } = default!;
    public Expression Replacement { get; init; } = default!;
    public string Name { get; init; } = string.Empty;
    public int Priority { get; init; }
    public Predicate<Expression>? Condition { get; init; }
    public RewriteDirection Direction { get; init; } = RewriteDirection.BottomUp;
}