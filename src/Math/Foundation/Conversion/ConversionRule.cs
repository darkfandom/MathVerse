namespace MathVerse.Math.Foundation.Conversion;

public sealed record ConversionRule
{
    public string From { get; init; } = string.Empty;

    public string To { get; init; } = string.Empty;

    public Func<double, double> Converter { get; init; } = v => v;

    public bool IsExact { get; init; }

    public string Description { get; init; } = string.Empty;
}
