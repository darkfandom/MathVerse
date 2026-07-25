namespace MathVerse.Math.CAS.Substitution;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public enum SubstitutionKind
{
    Variable,
    Function,
    Pattern,
    Simplification
}

public sealed record SubstitutionStep
{
    public string Description { get; init; } = string.Empty;
    public Expression Before { get; init; } = default!;
    public Expression After { get; init; } = default!;
    public SubstitutionKind Kind { get; init; }
}