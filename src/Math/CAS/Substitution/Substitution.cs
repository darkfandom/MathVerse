namespace MathVerse.Math.CAS.Substitution;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public sealed record Substitution
{
    public ImmutableDictionary<string, Expression> Variables { get; init; } = ImmutableDictionary<string, Expression>.Empty;
    public ImmutableDictionary<string, Expression> Functions { get; init; } = ImmutableDictionary<string, Expression>.Empty;

    public static Substitution Empty { get; } = new();

    public Substitution WithVariable(string name, Expression value)
    {
        return this with { Variables = Variables.SetItem(name, value) };
    }

    public Substitution WithFunction(string name, Expression value)
    {
        return this with { Functions = Functions.SetItem(name, value) };
    }

    public Expression Apply(Expression expr)
    {
        return SubstitutionEngine.Substitute(expr, this);
    }
}