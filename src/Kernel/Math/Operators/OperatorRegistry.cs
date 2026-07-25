namespace MathVerse.Math.Operators;

/// <summary>
/// Registry of all known mathematical operators.
/// </summary>
public sealed class OperatorRegistry
{
    private readonly Dictionary<string, MathOperator> _bySymbol = new(StringComparer.Ordinal);
    private readonly Dictionary<string, MathOperator> _byName = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MathOperator> _all = [];

    /// <summary>Initializes the default operator registry.</summary>
    public OperatorRegistry()
    {
        Register(MathOperator.Add);
        Register(MathOperator.Subtract);
        Register(MathOperator.Multiply);
        Register(MathOperator.Divide);
        Register(MathOperator.Modulo);
        Register(MathOperator.Power);
        Register(MathOperator.Negate);
        Register(MathOperator.Abs);

        Register(MathOperator.Equal);
        Register(MathOperator.NotEqual);
        Register(MathOperator.LessThan);
        Register(MathOperator.GreaterThan);
        Register(MathOperator.LessThanOrEqual);
        Register(MathOperator.GreaterThanOrEqual);

        Register(MathOperator.And);
        Register(MathOperator.Or);
        Register(MathOperator.Not);
        Register(MathOperator.Xor);
        Register(MathOperator.Implies);
        Register(MathOperator.Equivalent);

        Register(MathOperator.Union);
        Register(MathOperator.Intersection);
        Register(MathOperator.SetDifference);
        Register(MathOperator.ElementOf);
        Register(MathOperator.Subset);
        Register(MathOperator.ProperSubset);
        Register(MathOperator.Superset);

        Register(MathOperator.Transpose);
        Register(MathOperator.Inverse);
        Register(MathOperator.Determinant);
        Register(MathOperator.Dot);
        Register(MathOperator.Cross);
        Register(MathOperator.Kronecker);

        Register(MathOperator.Differential);
        Register(MathOperator.Partial);
        Register(MathOperator.Gradient);

        Register(MathOperator.Compose);
        Register(MathOperator.Apply);

        Register(MathOperator.Assign);
        Register(MathOperator.AddAssign);
        Register(MathOperator.MultiplyAssign);
    }

    /// <summary>Registers a new operator.</summary>
    public void Register(MathOperator op)
    {
        Guard.NotNull(op, nameof(op));
        _bySymbol[op.Symbol] = op;
        _byName[op.Name] = op;
        if (!_all.Contains(op))
            _all.Add(op);
    }

    /// <summary>Gets an operator by symbol.</summary>
    public MathOperator? GetBySymbol(string symbol) =>
        _bySymbol.TryGetValue(symbol, out var op) ? op : null;

    /// <summary>Gets an operator by name.</summary>
    public MathOperator? GetByName(string name) =>
        _byName.TryGetValue(name, out var op) ? op : null;

    /// <summary>Gets all registered operators.</summary>
    public IReadOnlyList<MathOperator> GetAll() => _all;

    /// <summary>Gets operators by category.</summary>
    public IReadOnlyList<MathOperator> GetByCategory(OperatorCategory category)
    {
        var result = new List<MathOperator>();
        foreach (var op in _all)
        {
            if (op.Category == category)
                result.Add(op);
        }
        return result;
    }

    /// <summary>Tries to get an operator by symbol.</summary>
    public bool TryGet(string symbol, out MathOperator op) =>
        _bySymbol.TryGetValue(symbol, out op!);
}
