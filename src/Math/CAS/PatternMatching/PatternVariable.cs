namespace MathVerse.Math.CAS.PatternMatching;

public sealed record PatternVariable
{
    public string Name { get; init; } = string.Empty;
    public Type? TypeConstraint { get; init; }
    public Predicate<MathVerse.Math.Expressions.Expression>? Constraint { get; init; }

    public PatternVariable(string name, Type? typeConstraint = null, Predicate<MathVerse.Math.Expressions.Expression>? constraint = null)
    {
        Name = name;
        TypeConstraint = typeConstraint;
        Constraint = constraint;
    }
}