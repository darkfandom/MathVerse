namespace MathVerse.Math.Types;

/// <summary>Base class for all mathematical types in MathVerse. All types are immutable.</summary>
public abstract class MathType : IEquatable<MathType>
{
    /// <summary>The kind of this type.</summary>
    public abstract TypeKind Kind { get; }

    /// <summary>Display name for this type.</summary>
    public abstract string Name { get; }

    /// <summary>Whether this type is a numeric scalar type.</summary>
    public virtual bool IsNumeric => false;

    /// <summary>Whether this type is an integer-like type.</summary>
    public virtual bool IsIntegral => false;

    /// <summary>Whether this type represents a field.</summary>
    public virtual bool IsField => false;

    /// <summary>Whether this type is a generic type parameter.</summary>
    public virtual bool IsGenericParameter => false;

    /// <summary>Whether this is the error type.</summary>
    public virtual bool IsError => Kind == TypeKind.Error;

    /// <summary>Whether this is the unknown type.</summary>
    public bool IsUnknown => Kind == TypeKind.Unknown;

    /// <summary>Whether this is the unit type.</summary>
    public bool IsUnit => Kind == TypeKind.Unit;

    /// <summary>Determines structural equality.</summary>
    public abstract bool Equals(MathType? other);

    /// <summary>Computes a structural hash code.</summary>
    public abstract override int GetHashCode();

    /// <summary>Structural equality.</summary>
    public override bool Equals(object? obj) => Equals(obj as MathType);

    /// <summary>Display representation.</summary>
    public override string ToString() => Name;

    /// <summary>Implicit conversion from int.</summary>
    public static implicit operator MathType(int value) => IntegerType.Create(value);

    /// <summary>Implicit conversion from double.</summary>
    public static implicit operator MathType(double value) => RealType.Instance;
}
