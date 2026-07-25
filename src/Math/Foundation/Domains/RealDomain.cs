namespace MathVerse.Math.Foundation.Domains;

public sealed class RealDomain
{
    private static readonly Lazy<MathDomain> LazyInstance = new(() => new MathDomain
    {
        Name = "Real",
        Kind = DomainKind.Real,
        Parents = ImmutableArray.Create(ComplexDomain.Instance),
        DoublePredicate = v => !double.IsNaN(v) && !double.IsInfinity(v),
        ComplexPredicate = v => v.Imaginary == 0.0 && !double.IsNaN(v.Real) && !double.IsInfinity(v.Real)
    });

    public static MathDomain Instance => LazyInstance.Value;
}
