namespace MathVerse.Math.Foundation.Domains;

public sealed record VectorDomain
{
    public MathDomain ElementDomain { get; init; }

    public int Dimension { get; init; }

    public MathDomain Domain { get; }

    public VectorDomain(MathDomain elementDomain, int dimension)
    {
        ElementDomain = elementDomain;
        Dimension = dimension;
        Domain = new MathDomain
        {
            Name = $"{dimension}D Vector over {elementDomain.Name}",
            Kind = DomainKind.Vector,
            Parents = ImmutableArray<MathDomain>.Empty,
            DoublePredicate = _ => false,
            ComplexPredicate = _ => false
        };
    }
}
