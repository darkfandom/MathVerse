namespace MathVerse.Math.Types;

/// <summary>Represents a function type: (DomainType) → RangeType.</summary>
public sealed class FunctionType : MathType
{
    /// <summary>The parameter types.</summary>
    public IReadOnlyList<MathType> ParameterTypes { get; }

    /// <summary>The return type.</summary>
    public MathType ReturnType { get; }

    /// <summary>Creates a function type.</summary>
    public FunctionType(IReadOnlyList<MathType> parameterTypes, MathType returnType)
    {
        ParameterTypes = parameterTypes;
        ReturnType = returnType;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Function;

    /// <inheritdoc/>
    public override string Name
    {
        get
        {
            var parameters = string.Join(", ", ParameterTypes.Select(t => t.Name));
            return $"({parameters}) → {ReturnType.Name}";
        }
    }

    /// <summary>Arity of the function.</summary>
    public int Arity => ParameterTypes.Count;

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not FunctionType ft) return false;
        if (ft.Arity != Arity) return false;
        if (!ft.ReturnType.Equals(ReturnType)) return false;
        for (int i = 0; i < Arity; i++)
        {
            if (!ft.ParameterTypes[i].Equals(ParameterTypes[i])) return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ReturnType);
        foreach (var p in ParameterTypes)
        {
            hash.Add(p);
        }
        return hash.ToHashCode();
    }
}
