namespace MathVerse.Math.Types.Inference;

/// <summary>Manages state during type inference: fresh variables, constraints, and substitution.</summary>
public sealed class InferenceContext
{
    private int _nextVarId;
    private readonly List<TypeConstraint> _constraints = new();
    private readonly Stack<TypeSubstitution> _substitutionStack = new();

    /// <summary>All generated constraints.</summary>
    public IReadOnlyList<TypeConstraint> Constraints => _constraints;

    /// <summary>The current substitution.</summary>
    public TypeSubstitution CurrentSubstitution =>
        _substitutionStack.Count > 0 ? _substitutionStack.Peek() : new TypeSubstitution();

    /// <summary>Creates an inference context.</summary>
    public InferenceContext()
    {
        _substitutionStack.Push(new TypeSubstitution());
    }

    /// <summary>Creates a fresh type variable.</summary>
    public TypeVariable FreshVariable(string? sourceName = null)
    {
        return new TypeVariable(_nextVarId++, sourceName);
    }

    /// <summary>Creates a fresh type variable with a specific ID (for testing).</summary>
    public TypeVariable FreshVariableWithId(int id, string? sourceName = null)
    {
        if (id >= _nextVarId) _nextVarId = id + 1;
        return new TypeVariable(id, sourceName);
    }

    /// <summary>Records a constraint.</summary>
    public void AddConstraint(TypeConstraint constraint)
    {
        _constraints.Add(constraint);
    }

    /// <summary>Records an equality constraint between two types.</summary>
    public void AddEquality(MathType left, MathType right, string? source = null)
    {
        _constraints.Add(new TypeConstraint(TypeConstraintKind.Equality, left, right,
            sourceExpression: source));
    }

    /// <summary>Records a numeric constraint on a type.</summary>
    public void AddNumericConstraint(MathType type, string? source = null)
    {
        _constraints.Add(new TypeConstraint(TypeConstraintKind.Numeric, type,
            sourceExpression: source));
    }

    /// <summary>Updates the substitution.</summary>
    public void UpdateSubstitution(TypeSubstitution newSubstitution)
    {
        _substitutionStack.Pop();
        _substitutionStack.Push(newSubstitution);
    }

    /// <summary>Pushes a scope for nested inference.</summary>
    public void PushScope()
    {
        _substitutionStack.Push(CurrentSubstitution);
    }

    /// <summary>Pops a scope.</summary>
    public void PopScope()
    {
        if (_substitutionStack.Count > 1)
            _substitutionStack.Pop();
    }

    /// <summary>Resolves a type through the current substitution.</summary>
    public MathType Resolve(MathType type)
    {
        return CurrentSubstitution.ApplyTo(type);
    }

    /// <summary>Whether a type variable is fully resolved.</summary>
    public bool IsResolved(TypeVariable variable) =>
        CurrentSubstitution.Contains(variable.Id);

    /// <summary>Gets the resolved type for a variable, or the variable itself.</summary>
    public MathType GetResolvedType(TypeVariable variable) =>
        CurrentSubstitution.Get(variable.Id) ?? variable;

    /// <summary>Clears all constraints.</summary>
    public void Clear()
    {
        _constraints.Clear();
    }
}
