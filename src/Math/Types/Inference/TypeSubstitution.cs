namespace MathVerse.Math.Types.Inference;

/// <summary>A substitution mapping type variables to concrete types.</summary>
public sealed class TypeSubstitution : IEquatable<TypeSubstitution>
{
    private readonly ImmutableDictionary<int, MathType> _mapping;

    /// <summary>The number of mapped variables.</summary>
    public int Count => _mapping.Count;

    /// <summary>Creates an empty substitution.</summary>
    public TypeSubstitution()
    {
        _mapping = ImmutableDictionary<int, MathType>.Empty;
    }

    private TypeSubstitution(ImmutableDictionary<int, MathType> mapping)
    {
        _mapping = mapping;
    }

    /// <summary>Whether this variable is mapped.</summary>
    public bool Contains(int variableId) => _mapping.ContainsKey(variableId);

    /// <summary>Gets the type for a variable, or null if unmapped.</summary>
    public MathType? Get(int variableId) =>
        _mapping.TryGetValue(variableId, out var type) ? type : null;

    /// <summary>Adds or replaces a mapping.</summary>
    public TypeSubstitution Add(int variableId, MathType type)
    {
        return new TypeSubstitution(_mapping.SetItem(variableId, type));
    }

    /// <summary>Composes two substitutions: apply this first, then other.</summary>
    public TypeSubstitution Compose(TypeSubstitution other)
    {
        var result = _mapping;
        foreach (var kvp in other._mapping)
        {
            var resolved = ApplyTo(kvp.Value);
            result = result.SetItem(kvp.Key, resolved);
        }
        return new TypeSubstitution(result);
    }

    /// <summary>Applies this substitution to a type.</summary>
    public MathType ApplyTo(MathType type)
    {
        if (type is TypeVariable tv && _mapping.TryGetValue(tv.Id, out var resolved))
        {
            return ApplyTo(resolved);
        }

        if (type is FunctionType ft)
        {
            var newParams = ft.ParameterTypes.Select(ApplyTo).ToList();
            var newReturn = ApplyTo(ft.ReturnType);
            return new FunctionType(newParams, newReturn);
        }

        if (type is VectorType vt)
        {
            return new VectorType(ApplyTo(vt.ElementType), vt.Dimension);
        }

        if (type is MatrixType mt)
        {
            return new MatrixType(ApplyTo(mt.ElementType), mt.Rows, mt.Columns);
        }

        if (type is TensorType tt)
        {
            return new TensorType(ApplyTo(tt.ElementType), tt.Shape);
        }

        if (type is TupleType tuple)
        {
            var newElements = tuple.ElementTypes.Select(ApplyTo).ToList();
            return new TupleType(newElements);
        }

        if (type is SetType st)
        {
            return new SetType(ApplyTo(st.ElementType), st.Cardinality);
        }

        if (type is SequenceType seq)
        {
            return new SequenceType(ApplyTo(seq.ElementType), seq.Length);
        }

        if (type is PolynomialType poly)
        {
            return new PolynomialType(ApplyTo(poly.CoefficientType), poly.VariableCount, poly.MaxDegree);
        }

        if (type is EquationType eq)
        {
            return new EquationType(ApplyTo(eq.LeftType), ApplyTo(eq.RightType), eq.Operator);
        }

        return type;
    }

    /// <summary>Whether any variables remain unmapped.</summary>
    public bool HasUnresolved => _mapping.Values.Any(t => t is TypeVariable);

    /// <summary>Gets all mapped variable IDs.</summary>
    public IEnumerable<int> MappedVariables => _mapping.Keys;

    /// <inheritdoc/>
    public bool Equals(TypeSubstitution? other)
    {
        if (other is null) return false;
        if (other._mapping.Count != _mapping.Count) return false;
        foreach (var kvp in _mapping)
        {
            if (!other._mapping.TryGetValue(kvp.Key, out var otherType)) return false;
            if (!kvp.Value.Equals(otherType)) return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TypeSubstitution);

    /// <inheritdoc/>
    public override int GetHashCode() => _mapping.Count;

    /// <inheritdoc/>
    public override string ToString()
    {
        var pairs = _mapping.Select(kvp => $"?{kvp.Key} := {kvp.Value.Name}");
        return $"[{string.Join(", ", pairs)}]";
    }
}
