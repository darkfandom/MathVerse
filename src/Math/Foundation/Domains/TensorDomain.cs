namespace MathVerse.Math.Foundation.Domains;

public sealed record TensorDomain
{
    public MathDomain ElementDomain { get; init; }

    public ImmutableArray<int> Shape { get; init; }

    public MathDomain Domain { get; }

    public TensorDomain(MathDomain elementDomain, ImmutableArray<int> shape)
    {
        ElementDomain = elementDomain;
        Shape = shape;
        string shapeStr = string.Join("x", shape);
        Domain = new MathDomain
        {
            Name = $"Tensor({shapeStr}) over {elementDomain.Name}",
            Kind = DomainKind.Tensor,
            Parents = ImmutableArray<MathDomain>.Empty,
            DoublePredicate = _ => false,
            ComplexPredicate = _ => false
        };
    }
}
