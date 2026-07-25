namespace MathVerse.Math.CAS.PatternMatching;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public sealed record PatternMatchError
{
    public string Message { get; init; } = string.Empty;
    public Expression? Expression { get; init; }
    public Pattern? Pattern { get; init; }
}

public sealed record PatternMatchResult
{
    public bool Success { get; init; }
    public ImmutableDictionary<string, Expression> Bindings { get; init; } = ImmutableDictionary<string, Expression>.Empty;
    public ImmutableArray<PatternMatchError> Errors { get; init; } = [];

    public static PatternMatchResult SuccessResult(ImmutableDictionary<string, Expression> bindings) =>
        new() { Success = true, Bindings = bindings };

    public static PatternMatchResult Failure(params PatternMatchError[] errors) =>
        new() { Success = false, Errors = errors.ToImmutableArray() };
}