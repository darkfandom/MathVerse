namespace MathVerse.Math.Foundation.Domains;

public sealed class DomainValidator
{
    private static readonly Lazy<DomainValidator> LazyInstance = new(() => new DomainValidator());

    public static DomainValidator Instance => LazyInstance.Value;

    private DomainValidator() { }

    public bool CanAdd(MathDomain a, MathDomain b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        if (a.IsCompatibleWith(b)) return true;
        return false;
    }

    public bool CanMultiply(MathDomain a, MathDomain b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        if (a.IsCompatibleWith(b)) return true;
        if (a.Kind == DomainKind.Vector && b.Kind == DomainKind.Vector) return true;
        if (a.Kind == DomainKind.Matrix && b.Kind == DomainKind.Matrix) return true;
        if (a.Kind == DomainKind.Vector && b.Kind.HasFlag(DomainKind.Real)) return true;
        if (a.Kind.HasFlag(DomainKind.Real) && b.Kind == DomainKind.Vector) return true;
        return false;
    }

    public bool CanApplyFunction(MathDomain domain, MathDomain codomain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (codomain is null) throw new ArgumentNullException(nameof(codomain));
        return true;
    }

    public MathDomain ResultDomain(MathDomain a, MathDomain b, string operation)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        if (operation is null) throw new ArgumentNullException(nameof(operation));

        if (a.Kind == DomainKind.Complex || b.Kind == DomainKind.Complex)
            return ComplexDomain.Instance;

        if (a.Kind == DomainKind.Quaternion || b.Kind == DomainKind.Quaternion)
            return QuaternionDomain.Instance;

        if (a.Kind == DomainKind.Matrix && b.Kind == DomainKind.Matrix)
            return a;

        if (a.Kind == DomainKind.Vector && b.Kind == DomainKind.Vector)
            return a;

        if (a.Kind == DomainKind.FiniteField && b.Kind == DomainKind.FiniteField)
            return a;

        if (a.IsSupersetOf(b)) return a;
        if (b.IsSupersetOf(a)) return b;

        if (a.Kind.HasFlag(DomainKind.Natural) && b.Kind.HasFlag(DomainKind.Natural))
            return NaturalDomain.Instance;

        if (a.Kind.HasFlag(DomainKind.Whole) && b.Kind.HasFlag(DomainKind.Whole))
            return WholeDomain.Instance;

        if (a.Kind.HasFlag(DomainKind.Integer) && b.Kind.HasFlag(DomainKind.Integer))
            return IntegerDomain.Instance;

        if (a.Kind.HasFlag(DomainKind.Rational) && b.Kind.HasFlag(DomainKind.Rational))
            return RationalDomain.Instance;

        if (a.Kind.HasFlag(DomainKind.Real) && b.Kind.HasFlag(DomainKind.Real))
            return RealDomain.Instance;

        return a;
    }
}
