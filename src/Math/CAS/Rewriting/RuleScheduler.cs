namespace MathVerse.Math.CAS.Rewriting;

using System.Collections.Immutable;

public static class RuleScheduler
{
    public static ImmutableArray<RewriteRule> Schedule(ImmutableArray<RewriteRule> rules)
    {
        return rules
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToImmutableArray();
    }

    public static ImmutableArray<RewriteRule> Optimize(ImmutableArray<RewriteRule> rules)
    {
        var builder = ImmutableArray.CreateBuilder<RewriteRule>();
        var seen = new HashSet<string>();

        foreach (var rule in rules.OrderByDescending(r => r.Priority))
        {
            var key = rule.Pattern.ToString() + "->" + rule.Replacement.ToString();
            if (seen.Add(key))
                builder.Add(rule);
        }

        return builder.ToImmutableArray();
    }
}