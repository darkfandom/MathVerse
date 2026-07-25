namespace MathVerse.Math.CAS.Rewriting;

using MathVerse.Math.Expressions;
using MathVerse.Math.CAS.PatternMatching;
using System.Collections.Immutable;

public sealed record RewritePipeline
{
    public ImmutableArray<RewriteRule> Rules { get; init; } = [];
    public int MaxIterations { get; init; } = 1000;
    public bool TerminateOnNoChange { get; init; } = true;

    public RewriteResult Apply(Expression expr)
    {
        var original = expr;
        var current = expr;
        var steps = ImmutableArray.CreateBuilder<RewriteStep>();
        var terminated = false;
        var reason = RewriteTerminationReason.Complete;

        for (int iter = 0; iter < MaxIterations; iter++)
        {
            var previous = current;
            bool changed = false;

            foreach (var rule in Rules.OrderByDescending(r => r.Priority))
            {
                var step = RuleExecutor.Execute(current, rule, rule.Direction);
                if (!step.After.Equals(step.Before))
                {
                    steps.Add(step);
                    current = step.After;
                    changed = true;
                    break;
                }
            }

            if (!changed)
            {
                if (TerminateOnNoChange)
                {
                    reason = RewriteTerminationReason.NoChange;
                    terminated = true;
                    break;
                }
            }

            if (AreEquivalent(previous, current))
            {
                reason = RewriteTerminationReason.Complete;
                terminated = true;
                break;
            }
        }

        if (!terminated)
        {
            reason = RewriteTerminationReason.MaxIterations;
            terminated = true;
        }

        return new RewriteResult
        {
            Original = original,
            Result = current,
            Steps = steps.ToImmutable(),
            Terminated = terminated,
            Reason = reason
        };
    }

    private static bool AreEquivalent(Expression a, Expression b) => a.Equals(b);
}