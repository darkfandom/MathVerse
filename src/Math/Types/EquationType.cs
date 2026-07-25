namespace MathVerse.Math.Types;

/// <summary>Represents an equation type with left-hand side, right-hand side, and operator.</summary>
public sealed class EquationType : MathType
{
    /// <summary>The left-hand side expression type.</summary>
    public MathType LeftType { get; }

    /// <summary>The right-hand side expression type.</summary>
    public MathType RightType { get; }

    /// <summary>The equation operator (=, ≤, ≥, &lt;, &gt;, ≠).</summary>
    public string Operator { get; }

    /// <summary>Creates an equation type.</summary>
    public EquationType(MathType leftType, MathType rightType, string @operator = "=")
    {
        LeftType = leftType;
        RightType = rightType;
        Operator = @operator;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Equation;

    /// <inheritdoc/>
    public override string Name => $"{LeftType.Name} {Operator} {RightType.Name}";

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not EquationType et) return false;
        return et.LeftType.Equals(LeftType)
            && et.RightType.Equals(RightType)
            && et.Operator == Operator;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(LeftType);
        hash.Add(RightType);
        hash.Add(Operator);
        return hash.ToHashCode();
    }
}
