namespace MathVerse.Core;

/// <summary>
/// Represents a value object that is defined by its attributes rather than identity.
/// Value objects are immutable and equality is based on attribute values.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>Gets the components that define equality for this value object.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc/>
    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is ValueObject other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
                current * 23 + (obj?.GetHashCode() ?? 0));
    }

    /// <summary>Equality operator.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        Equals(left, right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !Equals(left, right);
}
