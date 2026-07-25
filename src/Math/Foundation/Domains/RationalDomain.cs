namespace MathVerse.Math.Foundation.Domains;

public sealed class RationalDomain
{
    private static readonly Lazy<MathDomain> LazyInstance = new(() =>
    {
        Func<double, bool> pred = v =>
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return false;
            if (IntegerDomain.Instance.Contains(v)) return true;
            double abs = System.Math.Abs(v);
            for (int denom = 1; denom <= 10000; denom++)
            {
                double numer = abs * denom;
                if (System.Math.Abs(numer - System.Math.Round(numer)) < 1e-10)
                    return true;
            }
            return false;
        };

        return new MathDomain
        {
            Name = "Rational",
            Kind = DomainKind.Rational,
            Parents = ImmutableArray.Create<MathDomain>(RealDomain.Instance),
            DoublePredicate = pred,
            ComplexPredicate = v => v.Imaginary == 0.0 && pred(v.Real)
        };
    });

    public static MathDomain Instance => LazyInstance.Value;
}
