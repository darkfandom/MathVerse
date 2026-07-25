namespace MathVerse.Math.Foundation.Domains;

public sealed class NaturalDomain
{
    private static readonly Lazy<MathDomain> LazyInstance = new(() => new MathDomain
    {
        Name = "Natural",
        Kind = DomainKind.Natural,
        Parents = ImmutableArray.Create<MathDomain>(WholeDomain.Instance, IntegerDomain.Instance),
        DoublePredicate = v => IntegerDomain.Instance.Contains(v) && v >= 0,
        ComplexPredicate = v => v.Imaginary == 0.0 && IntegerDomain.Instance.Contains(v.Real) && v.Real >= 0
    });

    public static MathDomain Instance => LazyInstance.Value;
}
