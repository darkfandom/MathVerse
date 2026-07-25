namespace MathVerse.Math.Foundation.Constants;

public sealed record ConstantMetadata
{
    public string Provenance { get; init; } = string.Empty;

    public string Formula { get; init; } = string.Empty;

    public string FirstPublished { get; init; } = string.Empty;

    public ImmutableArray<string> RelatedConstants { get; init; } = ImmutableArray<string>.Empty;
}
