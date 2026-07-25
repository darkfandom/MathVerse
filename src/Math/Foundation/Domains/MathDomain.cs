namespace MathVerse.Math.Foundation.Domains;

public sealed record MathDomain
{
    public string Name { get; init; } = string.Empty;

    public DomainKind Kind { get; init; }

    public ImmutableArray<MathDomain> Parents { get; init; } = ImmutableArray<MathDomain>.Empty;

    public Func<double, bool>? DoublePredicate { get; init; }

    public Func<Complex, bool>? ComplexPredicate { get; init; }

    public bool IsSupersetOf(MathDomain other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        if (ReferenceEquals(this, other)) return true;
        if (Kind.HasFlag(other.Kind)) return true;
        return Parents.Any(p => p.IsSupersetOf(other));
    }

    public bool IsSubsetOf(MathDomain other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        return other.IsSupersetOf(this);
    }

    public bool IsCompatibleWith(MathDomain other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        return IsSupersetOf(other) || other.IsSupersetOf(this) || Kind == other.Kind;
    }

    public bool Contains(double value)
    {
        return DoublePredicate?.Invoke(value) ?? false;
    }

    public bool Contains(Complex value)
    {
        return ComplexPredicate?.Invoke(value) ?? false;
    }

    public override string ToString()
    {
        return Name;
    }
}
