namespace MathVerse.Math.CAS.Factorization;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Immutable;

public sealed record FactorizationResult
{
    public Expression Original { get; init; } = default!;
    public Expression Factored { get; init; } = default!;
    public ImmutableArray<string> Steps { get; init; }
    public bool IsFullyFactored { get; init; }
}