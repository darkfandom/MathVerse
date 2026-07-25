namespace MathVerse.Math.Types.Domains;

/// <summary>Registry of built-in and custom mathematical domains.</summary>
public sealed class DomainRegistry
{
    private readonly Dictionary<string, MathematicalDomain> _domains = new();
    private readonly Dictionary<MathType, MathematicalDomain> _typeMap = new();

    /// <summary>The number of registered domains.</summary>
    public int Count => _domains.Count;

    /// <summary>All registered domains.</summary>
    public IReadOnlyCollection<MathematicalDomain> Domains => _domains.Values;

    /// <summary>Creates a DomainRegistry with all built-in domains.</summary>
    public DomainRegistry()
    {
        RegisterBuiltIns();
    }

    /// <summary>Registers a domain.</summary>
    public void Register(MathematicalDomain domain)
    {
        _domains[domain.Symbol] = domain;
        _domains[domain.Name] = domain;
        _typeMap[domain.ElementType] = domain;
    }

    /// <summary>Resolves a domain by symbol (e.g., "ℕ") or name (e.g., "Natural").</summary>
    public MathematicalDomain? Resolve(string identifier)
    {
        if (_domains.TryGetValue(identifier, out var domain))
            return domain;
        return null;
    }

    /// <summary>Resolves a domain by element type.</summary>
    public MathematicalDomain? ResolveByType(MathType type)
    {
        if (_typeMap.TryGetValue(type, out var domain))
            return domain;
        return null;
    }

    /// <summary>Whether a domain is registered.</summary>
    public bool Contains(string identifier) => _domains.ContainsKey(identifier);

    /// <summary>Checks if a type belongs to a given domain.</summary>
    public bool IsMemberOf(MathType type, string domainSymbol)
    {
        var domain = Resolve(domainSymbol);
        if (domain is null) return false;
        return type.Equals(domain.ElementType);
    }

    /// <summary>Returns the smallest domain containing both types.</summary>
    public MathematicalDomain? FindCommonDomain(MathType left, MathType right)
    {
        var leftDomain = ResolveByType(left);
        var rightDomain = ResolveByType(right);

        if (leftDomain is null || rightDomain is null) return null;
        if (leftDomain.Equals(rightDomain)) return leftDomain;

        var leftChain = GetAncestry(leftDomain);
        var rightChain = GetAncestry(rightDomain);

        foreach (var lc in leftChain)
        {
            if (rightChain.Any(rc => rc.Equals(lc)))
                return lc;
        }

        return null;
    }

    private static IReadOnlyList<MathematicalDomain> GetAncestry(MathematicalDomain domain)
    {
        var chain = new List<MathematicalDomain>();
        var current = domain;
        while (current is not null)
        {
            chain.Add(current);
            current = current.Parent;
        }
        return chain;
    }

    private void RegisterBuiltIns()
    {
        var naturals = new MathematicalDomain("ℕ", "Natural Numbers", IntegerType.Instance,
            isCommutative: true, isOrdered: true, isField: false,
            isFinite: false, cardinality: null, parent: null);

        var integers = new MathematicalDomain("ℤ", "Integers", IntegerType.Instance,
            isCommutative: true, isOrdered: true, isField: false,
            isFinite: false, parent: naturals);

        var rationals = new MathematicalDomain("ℚ", "Rational Numbers", RationalType.Instance,
            isCommutative: true, isOrdered: true, isField: true,
            isFinite: false, parent: integers);

        var reals = new MathematicalDomain("ℝ", "Real Numbers", RealType.Instance,
            isCommutative: true, isOrdered: true, isField: true,
            isFinite: false, parent: rationals);

        var complexes = new MathematicalDomain("ℂ", "Complex Numbers", ComplexType.Instance,
            isCommutative: true, isOrdered: false, isAlgebraicallyClosed: true, isField: true,
            isFinite: false, parent: reals);

        var quaternions = new MathematicalDomain("ℍ", "Quaternions", ComplexType.Instance,
            isCommutative: false, isOrdered: false, isField: false,
            isFinite: false, parent: complexes);

        var octonions = new MathematicalDomain("𝕆", "Octonions", ComplexType.Instance,
            isCommutative: false, isOrdered: false, isField: false,
            isFinite: false, parent: quaternions);

        Register(naturals);
        Register(integers);
        Register(rationals);
        Register(reals);
        Register(complexes);
        Register(quaternions);
        Register(octonions);
    }
}
