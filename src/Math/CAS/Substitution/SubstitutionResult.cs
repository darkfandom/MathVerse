namespace MathVerse.Math.CAS.Substitution;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public sealed record SubstitutionResult
{
    public Expression Original { get; init; } = default!;
    public Expression Result { get; init; } = default!;
    public ImmutableArray<SubstitutionStep> Steps { get; init; } = [];
}