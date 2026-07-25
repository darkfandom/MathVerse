namespace MathVerse.Math.Foundation.Domains;

public sealed class DomainBuilder
{
    private string _name = string.Empty;

    private DomainKind _kind = DomainKind.None;

    private ImmutableArray<MathDomain> _parents = ImmutableArray<MathDomain>.Empty;

    private Func<double, bool>? _doublePredicate;

    private Func<Complex, bool>? _complexPredicate;

    public DomainBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public DomainBuilder OfKind(DomainKind kind)
    {
        _kind = kind;
        return this;
    }

    public DomainBuilder Extending(MathDomain parent)
    {
        if (parent is not null)
        {
            _parents = _parents.Add(parent);
        }
        return this;
    }

    public DomainBuilder Containing(Func<double, bool> predicate)
    {
        _doublePredicate = predicate;
        return this;
    }

    public DomainBuilder ContainingComplex(Func<Complex, bool> predicate)
    {
        _complexPredicate = predicate;
        return this;
    }

    public MathDomain Build()
    {
        return new MathDomain
        {
            Name = _name,
            Kind = _kind,
            Parents = _parents,
            DoublePredicate = _doublePredicate,
            ComplexPredicate = _complexPredicate
        };
    }
}
