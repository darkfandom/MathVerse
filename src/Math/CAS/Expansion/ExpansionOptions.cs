namespace MathVerse.Math.CAS.Expansion;

using System.Collections.Immutable;

public sealed record ExpansionOptions
{
    public bool DistributeMultiplication { get; init; } = true;
    public bool ExpandPowers { get; init; } = true;
    public bool ExpandFunctions { get; init; } = true;
    public bool ExpandLogarithms { get; init; } = true;
    public bool ExpandTrigonometric { get; init; } = true;
    public int MaxDepth { get; init; } = 100;

    public static ExpansionOptions Default { get; } = new();
    public static ExpansionOptions Minimal { get; } = new()
    {
        DistributeMultiplication = true,
        ExpandPowers = false,
        ExpandFunctions = false,
        ExpandLogarithms = false,
        ExpandTrigonometric = false,
        MaxDepth = 10
    };
    public static ExpansionOptions Full { get; } = new()
    {
        DistributeMultiplication = true,
        ExpandPowers = true,
        ExpandFunctions = true,
        ExpandLogarithms = true,
        ExpandTrigonometric = true,
        MaxDepth = 100
    };
}