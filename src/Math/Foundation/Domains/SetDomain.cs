namespace MathVerse.Math.Foundation.Domains;

public sealed record SetDomain
{
    public MathDomain ElementDomain { get; init; }

    public MathDomain Domain { get; }

    public SetDomain(MathDomain elementDomain)
    {
        ElementDomain = elementDomain;
        Domain = new MathDomain
        {
            Name = $"Set({elementDomain.Name})",
            Kind = DomainKind.Set,
            Parents = ImmutableArray<MathDomain>.Empty,
            DoublePredicate = _ => true,
            ComplexPredicate = _ => true
        };
    }
}
