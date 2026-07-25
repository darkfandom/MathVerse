namespace MathVerse.Math.CAS.Simplification;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public sealed record SimplificationResult
{
    public Expression Original { get; init; } = default!;
    public Expression Simplified { get; init; } = default!;
    public ImmutableArray<string> AppliedRules { get; init; }
    public bool Changed => !Original.Equals(Simplified);
    public int Steps { get; init; }
}