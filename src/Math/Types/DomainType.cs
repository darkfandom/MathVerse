namespace MathVerse.Math.Types;

/// <summary>Represents a mathematical domain type (e.g., ℕ, ℤ, ℚ, ℝ, ℂ).</summary>
public sealed class DomainType : MathType
{
    /// <summary>The domain name.</summary>
    public string DomainName { get; }

    /// <summary>The underlying element type.</summary>
    public MathType ElementType { get; }

    /// <summary>Creates a domain type.</summary>
    public DomainType(string domainName, MathType elementType)
    {
        DomainName = domainName;
        ElementType = elementType;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Domain;

    /// <inheritdoc/>
    public override string Name => $"{DomainName}";

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not DomainType dt) return false;
        return dt.DomainName == DomainName && dt.ElementType.Equals(ElementType);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(DomainName);
        hash.Add(ElementType);
        return hash.ToHashCode();
    }
}
