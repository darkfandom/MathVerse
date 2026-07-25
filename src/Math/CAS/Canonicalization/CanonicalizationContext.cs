namespace MathVerse.Math.CAS.Canonicalization;

using System.Collections.Immutable;

public sealed record CanonicalizationContext
{
    public bool FlattenAssociative { get; init; } = true;
    public bool SortCommutative { get; init; } = true;
    public bool NormalizeNegation { get; init; } = true;
    public bool NormalizeDivision { get; init; } = true;
    public bool NormalizePower { get; init; } = true;
    public bool CollectLikeTerms { get; init; } = true;
    public ImmutableHashSet<string> ExcludedFunctions { get; init; } = ImmutableHashSet<string>.Empty;

    public static CanonicalizationContext Default { get; } = new();
    public static CanonicalizationContext Minimal { get; } = new()
    {
        FlattenAssociative = false,
        SortCommutative = false,
        NormalizeNegation = false,
        NormalizeDivision = false,
        NormalizePower = false,
        CollectLikeTerms = false
    };
    public static CanonicalizationContext Full { get; } = new()
    {
        FlattenAssociative = true,
        SortCommutative = true,
        NormalizeNegation = true,
        NormalizeDivision = true,
        NormalizePower = true,
        CollectLikeTerms = true
    };
}