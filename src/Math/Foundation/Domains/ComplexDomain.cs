namespace MathVerse.Math.Foundation.Domains;

public sealed class ComplexDomain
{
    private static readonly Lazy<MathDomain> LazyInstance = new(() => new MathDomain
    {
        Name = "Complex",
        Kind = DomainKind.Complex,
        Parents = ImmutableArray<MathDomain>.Empty,
        DoublePredicate = _ => true,
        ComplexPredicate = _ => true
    });

    public static MathDomain Instance => LazyInstance.Value;
}
