namespace MathVerse.Math.CAS.Rewriting;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public sealed record RewriteResult
{
    public Expression Original { get; init; } = default!;
    public Expression Result { get; init; } = default!;
    public ImmutableArray<RewriteStep> Steps { get; init; } = [];
    public bool Terminated { get; init; }
    public RewriteTerminationReason Reason { get; init; }
}

public enum RewriteTerminationReason
{
    MaxIterations,
    NoChange,
    Complete
}

public sealed record RewriteStep
{
    public RewriteRule Rule { get; init; } = default!;
    public Expression Before { get; init; } = default!;
    public Expression After { get; init; } = default!;
    public ImmutableDictionary<string, Expression> Bindings { get; init; } = ImmutableDictionary<string, Expression>.Empty;
}