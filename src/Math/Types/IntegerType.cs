namespace MathVerse.Math.Types;

/// <summary>Represents integer numbers. Singleton per value.</summary>
public sealed class IntegerType : ScalarType
{
    /// <summary>The singleton instance.</summary>
    public static readonly IntegerType Instance = new();

    private IntegerType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Integer;

    /// <inheritdoc/>
    public override string Name => "Integer";

    /// <inheritdoc/>
    public override bool IsIntegral => true;

    /// <inheritdoc/>
    public override bool IsField => false;

    /// <inheritdoc/>
    public override bool IsOrdered => true;

    /// <inheritdoc/>
    public override bool IsAlgebraicallyClosed => false;

    /// <inheritdoc/>
    public override ScalarType Supertype => RationalType.Instance;

    /// <summary>Creates a typed integer constant.</summary>
    public static TypedInteger Create(int value) => new(value);

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is IntegerType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(IntegerType).GetHashCode();
}

/// <summary>A typed integer constant carrying its value.</summary>
public sealed class TypedInteger : ScalarType
{
    /// <summary>The integer value.</summary>
    public int Value { get; }

    /// <summary>Creates a typed integer.</summary>
    public TypedInteger(int value) { Value = value; }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Integer;

    /// <inheritdoc/>
    public override string Name => Value.ToString();

    /// <inheritdoc/>
    public override bool IsIntegral => true;

    /// <inheritdoc/>
    public override bool IsField => false;

    /// <inheritdoc/>
    public override bool IsOrdered => true;

    /// <inheritdoc/>
    public override bool IsAlgebraicallyClosed => false;

    /// <inheritdoc/>
    public override ScalarType Supertype => RationalType.Instance;

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is TypedInteger ti && ti.Value == Value;

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();
}
