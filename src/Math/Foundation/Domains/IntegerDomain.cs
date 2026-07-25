namespace MathVerse.Math.Foundation.Domains;

public sealed class IntegerDomain
{
    private static readonly Lazy<MathDomain> LazyInstance = new(() => new MathDomain
    {
        Name = "Integer",
        Kind = DomainKind.Integer,
        Parents = ImmutableArray.Create<MathDomain>(RationalDomain.Instance),
        DoublePredicate = v => !double.IsNaN(v) && !double.IsInfinity(v) && System.Math.Floor(v) == v,
        ComplexPredicate = v => v.Imaginary == 0.0 && !double.IsNaN(v.Real) && !double.IsInfinity(v.Real) && System.Math.Floor(v.Real) == v.Real
    });

    public static MathDomain Instance => LazyInstance.Value;
}
