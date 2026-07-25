namespace MathVerse.Math.Types.Generics;

/// <summary>A generic type (e.g., Vector&lt;T&gt;, Matrix&lt;T&gt;) with unbound type parameters.</summary>
public sealed class GenericType : MathType
{
    /// <summary>The name of the generic type.</summary>
    public string TypeName { get; }

    /// <summary>The arity (number of type parameters).</summary>
    public int Arity { get; }

    /// <summary>The type parameters with their constraints.</summary>
    public IReadOnlyList<TypeParameter> Parameters { get; }

    /// <summary>Creates a generic type definition.</summary>
    public GenericType(string typeName, IReadOnlyList<TypeParameter> parameters)
    {
        TypeName = typeName;
        Parameters = parameters;
        Arity = parameters.Count;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Generic;

    /// <inheritdoc/>
    public override string Name
    {
        get
        {
            var parms = string.Join(", ", Parameters.Select(p => p.Name_));
            return $"{TypeName}<{parms}>";
        }
    }

    /// <inheritdoc/>
    public override bool IsGenericParameter => false;

    /// <summary>Instantiates this generic type with the given type arguments.</summary>
    public GenericInstantiation Instantiate(IReadOnlyList<MathType> typeArguments)
    {
        if (typeArguments.Count != Arity)
            throw new ArgumentException(
                $"Expected {Arity} type arguments but got {typeArguments.Count}.");
        return new GenericInstantiation(this, typeArguments);
    }

    /// <inheritdoc/>
    public override bool Equals(MathType? other) =>
        other is GenericType gt && gt.TypeName == TypeName && gt.Arity == Arity;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(TypeName, Arity);
}

/// <summary>A fully instantiated generic type with concrete type arguments.</summary>
public sealed class GenericInstantiation : MathType
{
    /// <summary>The generic type definition.</summary>
    public GenericType Definition { get; }

    /// <summary>The concrete type arguments.</summary>
    public IReadOnlyList<MathType> TypeArguments { get; }

    /// <summary>Creates a generic instantiation.</summary>
    public GenericInstantiation(GenericType definition, IReadOnlyList<MathType> typeArguments)
    {
        Definition = definition;
        TypeArguments = typeArguments;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => Definition.Kind == TypeKind.Generic ? TypeKind.Generic : Definition.Kind;

    /// <inheritdoc/>
    public override string Name
    {
        get
        {
            var args = string.Join(", ", TypeArguments.Select(t => t.Name));
            return $"{Definition.TypeName}<{args}>";
        }
    }

    /// <inheritdoc/>
    public override bool IsGenericParameter => false;

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not GenericInstantiation gi) return false;
        if (!gi.Definition.Equals(Definition)) return false;
        if (gi.TypeArguments.Count != TypeArguments.Count) return false;
        for (int i = 0; i < TypeArguments.Count; i++)
        {
            if (!gi.TypeArguments[i].Equals(TypeArguments[i])) return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Definition);
        foreach (var arg in TypeArguments)
        {
            hash.Add(arg);
        }
        return hash.ToHashCode();
    }
}
