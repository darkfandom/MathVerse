namespace MathVerse.Math.Foundation.Domains;

public sealed class WholeDomain
{
    private static readonly Lazy<MathDomain> LazyInstance = new(() => new MathDomain
    {
        Name = "Whole",
        Kind = DomainKind.Whole,
        Parents = ImmutableArray.Create<MathDomain>(IntegerDomain.Instance),
        DoublePredicate = v => IntegerDomain.Instance.Contains(v) && v > 0,
        ComplexPredicate = v => v.Imaginary == 0.0 && IntegerDomain.Instance.Contains(v.Real) && v.Real > 0
    });

    public static MathDomain Instance => LazyInstance.Value;
}
