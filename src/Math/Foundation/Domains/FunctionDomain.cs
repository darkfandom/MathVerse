namespace MathVerse.Math.Foundation.Domains;

public sealed record FunctionDomain
{
    public MathDomain Codomain { get; init; }

    public MathDomain Domain { get; }

    public FunctionDomain(MathDomain codomain)
    {
        Codomain = codomain;
        Domain = new MathDomain
        {
            Name = $"Function -> {codomain.Name}",
            Kind = DomainKind.Function,
            Parents = ImmutableArray<MathDomain>.Empty,
            DoublePredicate = _ => true,
            ComplexPredicate = _ => true
        };
    }
}
