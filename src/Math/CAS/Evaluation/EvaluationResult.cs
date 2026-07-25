namespace MathVerse.Math.CAS.Evaluation;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;
using System.Numerics;

public sealed record EvaluationResult
{
    public Expression Original { get; init; } = default!;
    public Expression Result { get; init; } = default!;
    public ImmutableDictionary<string, double> VariableValues { get; init; } = ImmutableDictionary<string, double>.Empty;
    public bool IsExact { get; init; }
}