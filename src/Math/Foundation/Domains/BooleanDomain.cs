namespace MathVerse.Math.Foundation.Domains;

public sealed class BooleanDomain
{
    private static readonly Lazy<MathDomain> LazyInstance = new(() => new MathDomain
    {
        Name = "Boolean",
        Kind = DomainKind.Boolean,
        Parents = ImmutableArray<MathDomain>.Empty,
        DoublePredicate = v => v == 0.0 || v == 1.0,
        ComplexPredicate = v => v.Imaginary == 0.0 && (v.Real == 0.0 || v.Real == 1.0)
    });

    public static MathDomain Instance => LazyInstance.Value;
}
