namespace MathVerse.Math.Types.Inference;

/// <summary>A type environment that maps identifiers to their types during inference.</summary>
public sealed class TypeEnvironment
{
    private readonly ImmutableDictionary<string, MathType> _bindings;
    private readonly TypeEnvironment? _parent;

    /// <summary>The number of direct bindings.</summary>
    public int Count => _bindings.Count;

    /// <summary>Creates a root type environment.</summary>
    public TypeEnvironment()
    {
        _bindings = ImmutableDictionary<string, MathType>.Empty;
        _parent = null;
    }

    private TypeEnvironment(ImmutableDictionary<string, MathType> bindings, TypeEnvironment? parent)
    {
        _bindings = bindings;
        _parent = parent;
    }

    /// <summary>Looks up a name, searching parent scopes if needed.</summary>
    public MathType? Lookup(string name)
    {
        if (_bindings.TryGetValue(name, out var type))
            return type;
        return _parent?.Lookup(name);
    }

    /// <summary>Binds a name to a type in the current scope.</summary>
    public TypeEnvironment Bind(string name, MathType type)
    {
        return new TypeEnvironment(_bindings.SetItem(name, type), _parent);
    }

    /// <summary>Binds multiple names to types.</summary>
    public TypeEnvironment BindAll(IReadOnlyDictionary<string, MathType> bindings)
    {
        var result = _bindings;
        foreach (var kvp in bindings)
        {
            result = result.SetItem(kvp.Key, kvp.Value);
        }
        return new TypeEnvironment(result, _parent);
    }

    /// <summary>Creates a child scope.</summary>
    public TypeEnvironment CreateChild()
    {
        return new TypeEnvironment(ImmutableDictionary<string, MathType>.Empty, this);
    }

    /// <summary>Whether a name is defined in this scope or any parent.</summary>
    public bool IsDefined(string name) => Lookup(name) is not null;

    /// <summary>Gets all defined names in this scope (not parent).</summary>
    public IEnumerable<string> DefinedNames => _bindings.Keys;

    /// <summary>Merges another environment into this one.</summary>
    public TypeEnvironment Merge(TypeEnvironment other)
    {
        var result = _bindings;
        foreach (var kvp in other._bindings)
        {
            result = result.SetItem(kvp.Key, kvp.Value);
        }
        return new TypeEnvironment(result, _parent);
    }
}
