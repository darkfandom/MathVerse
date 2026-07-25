using System.Collections.Immutable;
using MathVerse.Math.Expressions;

namespace MathVerse.Math.CAS.SpecialFunctions;

public sealed record SpecialFunction
{
    public string Name { get; init; } = string.Empty;
    public ImmutableArray<string> Aliases { get; init; } = ImmutableArray<string>.Empty;
    public ImmutableArray<SpecialFunctionProperty> Properties { get; init; } = ImmutableArray<SpecialFunctionProperty>.Empty;
    public Func<ImmutableArray<Expression>, Expression> Evaluator { get; init; } = _ => Expr.Literal(0);

    public bool HasProperty(SpecialFunctionProperty property)
        => Properties.Contains(property);

    public bool IsMatch(string name)
        => string.Equals(Name, name, StringComparison.OrdinalIgnoreCase)
           || Aliases.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));

    public Expression Evaluate(ImmutableArray<Expression> args)
        => Evaluator(args);

    public override string ToString() => Name;
}

public enum SpecialFunctionProperty
{
    Analytic,
    Meromorphic,
    Entire,
    Periodic,
    Even,
    Odd,
    RealForReal,
    SatisfiesDE
}