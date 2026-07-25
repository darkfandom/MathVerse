namespace MathVerse.Math.CAS.Canonicalization;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public sealed record CanonicalForm
{
    public Expression Expression { get; init; } = default!;
    public ImmutableArray<string> AppliedRules { get; init; }
    public bool IsCanonical { get; init; }

    public static CanonicalForm From(Expression expr)
    {
        var canonicalizer = Canonicalizer.Instance;
        var form = canonicalizer.Canonicalize(expr);
        return form;
    }
}