namespace MathVerse.Math.Numerics.RootFinding;

using System.Collections.Concurrent;
using System.Collections.Immutable;

public sealed class RootFinderRegistry
{
    private static readonly Lazy<RootFinderRegistry> _instance = new(() => new RootFinderRegistry());
    private readonly ConcurrentDictionary<string, IRootFinder> _finders = new(StringComparer.OrdinalIgnoreCase);

    private RootFinderRegistry()
    {
        Register("bisection", new BisectionFinder());
        Register("newton", new NewtonRaphsonFinder());
        Register("secant", new SecantFinder());
        Register("brent", new BrentFinder());
        Register("falseposition", new FalsePositionFinder());
        Register("regulafalsi", new FalsePositionFinder());
    }

    public static RootFinderRegistry Instance => _instance.Value;

    public IRootFinder Get(string name)
    {
        if (_finders.TryGetValue(name, out var finder))
        {
            return finder;
        }
        throw new ArgumentException($"Root finder '{name}' not registered", nameof(name));
    }

    public void Register(string name, IRootFinder finder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(finder);
        _finders[name] = finder;
    }

    public ImmutableArray<string> GetAll()
    {
        return _finders.Keys.ToImmutableArray();
    }
}