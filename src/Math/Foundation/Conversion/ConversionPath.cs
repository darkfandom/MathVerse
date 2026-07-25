namespace MathVerse.Math.Foundation.Conversion;

public sealed record ConversionPath
{
    public ImmutableArray<ConversionRule> Steps { get; init; } = ImmutableArray<ConversionRule>.Empty;

    public string From { get; init; } = string.Empty;

    public string To { get; init; } = string.Empty;

    public bool IsDirect => Steps.Length == 1;

    public int StepCount => Steps.Length;

    public double Convert(double value)
    {
        var result = value;
        foreach (var step in Steps)
            result = step.Converter(result);
        return result;
    }
}
