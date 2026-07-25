namespace MathVerse.Math.CAS.Expansion;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public sealed record ExpansionResult
{
    public Expression Original { get; init; } = default!;
    public Expression Expanded { get; init; } = default!;
    public ImmutableArray<string> Steps { get; init; }
}