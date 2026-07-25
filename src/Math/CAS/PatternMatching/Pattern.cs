namespace MathVerse.Math.CAS.PatternMatching;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public abstract record Pattern
{
    public static WildcardPattern Wildcard => new();
    public static VariablePattern Variable(string name, Type? typeConstraint = null, Predicate<Expression>? constraint = null) => new(name, typeConstraint, constraint);
    public static PredicatePattern FromPredicate(Predicate<Expression> predicate) => new(predicate);
    public static SequencePattern Sequence(params Pattern[] patterns) => new(patterns);
    public static StructuralPattern Structural(Expression template) => new(template);
}

public sealed record WildcardPattern : Pattern
{
    public override string ToString() => "_";
}

public sealed record VariablePattern : Pattern
{
    public string Name { get; init; } = string.Empty;
    public Type? TypeConstraint { get; init; }
    public Predicate<Expression>? Constraint { get; init; }

    public VariablePattern(string name, Type? typeConstraint = null, Predicate<Expression>? constraint = null)
    {
        Name = name;
        TypeConstraint = typeConstraint;
        Constraint = constraint;
    }

    public override string ToString() => $"_{Name}";
}

public sealed record PredicatePattern : Pattern
{
    public Predicate<Expression> MatchPredicate { get; init; } = _ => false;

    public PredicatePattern(Predicate<Expression> predicate)
    {
        MatchPredicate = predicate;
    }

    public override string ToString() => "?";
}

public sealed record SequencePattern : Pattern
{
    public ImmutableArray<Pattern> Patterns { get; init; }

    public SequencePattern(params Pattern[] patterns)
    {
        Patterns = patterns.ToImmutableArray();
    }

    public override string ToString() => $"[{string.Join(", ", Patterns)}]";
}

public sealed record StructuralPattern : Pattern
{
    public Expression Template { get; init; } = default!;

    public StructuralPattern(Expression template)
    {
        Template = template;
    }

    public override string ToString() => Template.ToString();
}