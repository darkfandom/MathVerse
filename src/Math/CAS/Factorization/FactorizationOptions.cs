namespace MathVerse.Math.CAS.Factorization;

using System.Collections.Immutable;

public sealed record FactorizationOptions
{
    public bool FactorCommonTerms { get; init; } = true;
    public bool FactorPolynomials { get; init; } = true;
    public bool FactorTrigonometric { get; init; } = true;
    public bool FactorOverComplex { get; init; } = false;
    public bool FactorOverReals { get; init; } = true;
    public int MaxDegree { get; init; } = 5;

    public static FactorizationOptions Default { get; } = new();
    public static FactorizationOptions Minimal { get; } = new()
    {
        FactorCommonTerms = true,
        FactorPolynomials = false,
        FactorTrigonometric = false,
        FactorOverComplex = false,
        FactorOverReals = false,
        MaxDegree = 2
    };
    public static FactorizationOptions Full { get; } = new()
    {
        FactorCommonTerms = true,
        FactorPolynomials = true,
        FactorTrigonometric = true,
        FactorOverComplex = true,
        FactorOverReals = true,
        MaxDegree = 10
    };
}