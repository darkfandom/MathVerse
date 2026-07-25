namespace MathVerse.Math.Foundation.Domains;

public sealed record FiniteFieldDomain
{
    public int Characteristic { get; init; }

    public MathDomain Domain { get; }

    public FiniteFieldDomain(int characteristic)
    {
        Characteristic = characteristic;
        Domain = new MathDomain
        {
            Name = $"GF({characteristic})",
            Kind = DomainKind.FiniteField,
            Parents = ImmutableArray<MathDomain>.Empty,
            DoublePredicate = v =>
            {
                if (double.IsNaN(v) || double.IsInfinity(v)) return false;
                return System.Math.IEEERemainder(v, characteristic) == 0;
            },
            ComplexPredicate = v => v.Imaginary == 0.0
                && !double.IsNaN(v.Real)
                && !double.IsInfinity(v.Real)
                && System.Math.IEEERemainder(v.Real, characteristic) == 0
        };
    }
}
