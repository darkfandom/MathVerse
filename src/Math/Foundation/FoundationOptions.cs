namespace MathVerse.Math.Foundation;

public sealed record FoundationOptions
{
    public bool EnableDimensionChecking { get; init; } = true;

    public bool EnableAutoConversion { get; init; } = false;

    public string DefaultUnitSystem { get; init; } = "SI";

    public int MaxConversionPathLength { get; init; } = 5;

    public bool EnableConstantCaching { get; init; } = true;
}
