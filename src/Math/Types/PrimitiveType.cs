namespace MathVerse.Math.Types;

/// <summary>Base class for all primitive (non-composite) mathematical types.</summary>
public abstract class PrimitiveType : MathType
{
    /// <summary>Primitive types are not generic parameters.</summary>
    public override bool IsGenericParameter => false;

    /// <summary>Singleton instance for the default primitive.</summary>
    protected static readonly ImmutableDictionary<TypeKind, PrimitiveType> Instances =
        ImmutableDictionary<TypeKind, PrimitiveType>.Empty;
}
