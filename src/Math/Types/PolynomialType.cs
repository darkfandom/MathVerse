namespace MathVerse.Math.Types;

/// <summary>Represents a polynomial type with variable type and degree.</summary>
public sealed class PolynomialType : MathType
{
    /// <summary>The coefficient type.</summary>
    public MathType CoefficientType { get; }

    /// <summary>The number of variables.</summary>
    public int VariableCount { get; }

    /// <summary>The maximum degree per variable. Null for unbounded.</summary>
    public int? MaxDegree { get; }

    /// <summary>Creates a polynomial type.</summary>
    public PolynomialType(MathType coefficientType, int variableCount = 1, int? maxDegree = null)
    {
        CoefficientType = coefficientType;
        VariableCount = variableCount;
        MaxDegree = maxDegree;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Polynomial;

    /// <inheritdoc/>
    public override string Name
    {
        get
        {
            var vars = VariableCount == 1 ? "x" : $"x₁…x{VariableCount}";
            var deg = MaxDegree.HasValue ? $", deg≤{MaxDegree.Value}" : string.Empty;
            return $"Poly<{CoefficientType.Name}, {vars}{deg}>";
        }
    }

    /// <inheritdoc/>
    public override bool IsNumeric => false;

    /// <summary>Whether this is univariate.</summary>
    public bool IsUnivariate => VariableCount == 1;

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not PolynomialType pt) return false;
        return pt.CoefficientType.Equals(CoefficientType)
            && pt.VariableCount == VariableCount
            && pt.MaxDegree == MaxDegree;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(CoefficientType);
        hash.Add(VariableCount);
        hash.Add(MaxDegree);
        return hash.ToHashCode();
    }
}
