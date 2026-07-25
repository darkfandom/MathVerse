using System.Collections.Concurrent;

namespace MathVerse.Math.Foundation.Domains;

public sealed class DomainRegistry
{
    private static readonly Lazy<DomainRegistry> LazyInstance = new(() => new DomainRegistry());

    public static DomainRegistry Instance => LazyInstance.Value;

    private readonly ConcurrentDictionary<string, MathDomain> _byName = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<DomainKind, MathDomain> _byKind = new();

    private DomainRegistry()
    {
        Register(ComplexDomain.Instance);
        Register(QuaternionDomain.Instance);
        Register(RealDomain.Instance);
        Register(RationalDomain.Instance);
        Register(IntegerDomain.Instance);
        Register(WholeDomain.Instance);
        Register(NaturalDomain.Instance);
        Register(BooleanDomain.Instance);
    }

    public MathDomain? Get(DomainKind kind)
    {
        _byKind.TryGetValue(kind, out MathDomain? domain);
        return domain;
    }

    public MathDomain? Get(string name)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        _byName.TryGetValue(name, out MathDomain? domain);
        return domain;
    }

    public void Register(MathDomain domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        _byName[domain.Name] = domain;
        _byKind[domain.Kind] = domain;
    }

    public IReadOnlyList<MathDomain> GetAll()
    {
        return _byName.Values.ToList().AsReadOnly();
    }
}
