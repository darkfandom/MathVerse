namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum ConstraintType
{
    Equality,
    InequalityLess,
    InequalityGreater
}

public sealed record Constraint
{
    public Func<Vector, double> Function { get; init; }
    public ConstraintType Type { get; init; }
    public string? Name { get; init; }

    public Constraint(Func<Vector, double> function, ConstraintType type, string? name = null)
    {
        Function = function;
        Type = type;
        Name = name;
    }

    public double Evaluate(Vector x) => Function(x);

    public bool IsSatisfied(Vector x, double tolerance = 1e-10)
    {
        double value = Function(x);
return Type switch
        {
            ConstraintType.Equality => System.Math.Abs(value) <= tolerance,
            ConstraintType.InequalityLess => value <= tolerance,
            ConstraintType.InequalityGreater => value >= -tolerance,
            _ => false
        };
    }

    public static Constraint Equality(Func<Vector, double> function, string? name = null)
        => new(function, ConstraintType.Equality, name);

    public static Constraint InequalityLess(Func<Vector, double> function, string? name = null)
        => new(function, ConstraintType.InequalityLess, name);

    public static Constraint InequalityGreater(Func<Vector, double> function, string? name = null)
        => new(function, ConstraintType.InequalityGreater, name);
}


