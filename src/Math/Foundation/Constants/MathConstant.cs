namespace MathVerse.Math.Foundation.Constants;

public sealed record MathConstant
{
    public string Symbol { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public ConstantCategory Category { get; init; }

    public double NumericValue { get; init; }

    public Complex ComplexValue { get; init; }

    public ImmutableArray<string> Aliases { get; init; } = ImmutableArray<string>.Empty;

    public string Description { get; init; } = string.Empty;

    public bool IsExact { get; init; }

    public override string ToString()
    {
        return Symbol;
    }
}
