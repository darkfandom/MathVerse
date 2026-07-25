using System.Collections.Concurrent;

namespace MathVerse.Math.Foundation.Constants;

public sealed class ConstantRegistry
{
    private static readonly Lazy<ConstantRegistry> LazyInstance = new(() => new ConstantRegistry());

    public static ConstantRegistry Instance => LazyInstance.Value;

    private readonly ConcurrentDictionary<string, MathConstant> _byNameOrSymbol = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<ConstantCategory, List<MathConstant>> _byCategory = new();

    private readonly object _categoryLock = new();

    private ConstantRegistry()
    {
        Register(BuiltinConstants.Pi);
        Register(BuiltinConstants.Tau);
        Register(BuiltinConstants.E);
        Register(BuiltinConstants.Phi);
        Register(BuiltinConstants.Gamma);
        Register(BuiltinConstants.I);
        Register(BuiltinConstants.Infinity);
        Register(BuiltinConstants.NaN);
        Register(BuiltinConstants.Epsilon);
        Register(BuiltinConstants.Catalan);
        Register(BuiltinConstants.Apery);
        Register(BuiltinConstants.FeigenbaumAlpha);
        Register(BuiltinConstants.FeigenbaumDelta);
    }

    public MathConstant? Get(string nameOrSymbol)
    {
        if (nameOrSymbol is null) throw new ArgumentNullException(nameof(nameOrSymbol));
        _byNameOrSymbol.TryGetValue(nameOrSymbol, out MathConstant? constant);
        return constant;
    }

    public IReadOnlyList<MathConstant> GetByCategory(ConstantCategory cat)
    {
        if (_byCategory.TryGetValue(cat, out List<MathConstant>? constants))
        {
            return constants.AsReadOnly();
        }
        return Array.Empty<MathConstant>();
    }

    public IReadOnlyList<MathConstant> GetAll()
    {
        return _byNameOrSymbol.Values.Distinct().ToList().AsReadOnly();
    }

    public void Register(MathConstant constant)
    {
        if (constant is null) throw new ArgumentNullException(nameof(constant));

        _byNameOrSymbol[constant.Name] = constant;
        _byNameOrSymbol[constant.Symbol] = constant;

        foreach (string alias in constant.Aliases)
        {
            _byNameOrSymbol[alias] = constant;
        }

        lock (_categoryLock)
        {
            if (!_byCategory.ContainsKey(constant.Category))
            {
                _byCategory[constant.Category] = new List<MathConstant>();
            }
            _byCategory[constant.Category].Add(constant);
        }
    }
}
