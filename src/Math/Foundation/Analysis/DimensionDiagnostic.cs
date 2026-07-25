using MathVerse.Math.Foundation.Dimensions;

namespace MathVerse.Math.Foundation.Analysis;

public sealed record DimensionDiagnostic
{
    public DimensionRule Rule { get; init; }

    public string Message { get; init; } = string.Empty;

    public string Expression { get; init; } = string.Empty;

    public Dimension? ExpectedDimension { get; init; }

    public Dimension? ActualDimension { get; init; }
}
