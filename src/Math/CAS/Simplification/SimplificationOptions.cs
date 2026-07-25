namespace MathVerse.Math.CAS.Simplification;

using System.Collections.Immutable;

public sealed record SimplificationOptions
{
    public bool ConstantFolding { get; init; } = true;
    public bool AlgebraicSimplification { get; init; } = true;
    public bool TrigonometricSimplification { get; init; } = true;
    public bool LogarithmicSimplification { get; init; } = true;
    public bool PowerSimplification { get; init; } = true;
    public bool RationalSimplification { get; init; } = true;
    public bool ComplexSimplification { get; init; } = true;
    public int MaxIterations { get; init; } = 100;
    public double Tolerance { get; init; } = 1e-12;

    public static SimplificationOptions Default { get; } = new();
    public static SimplificationOptions Minimal { get; } = new()
    {
        ConstantFolding = true,
        AlgebraicSimplification = false,
        TrigonometricSimplification = false,
        LogarithmicSimplification = false,
        PowerSimplification = false,
        RationalSimplification = false,
        ComplexSimplification = false,
        MaxIterations = 1
    };
    public static SimplificationOptions Full { get; } = new()
    {
        ConstantFolding = true,
        AlgebraicSimplification = true,
        TrigonometricSimplification = true,
        LogarithmicSimplification = true,
        PowerSimplification = true,
        RationalSimplification = true,
        ComplexSimplification = true,
        MaxIterations = 100,
        Tolerance = 1e-12
    };
}