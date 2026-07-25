namespace MathVerse.Math.CAS.Simplification;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Concurrent;
using System.Collections.Immutable;

public sealed class Simplifier
{
    private static readonly Lazy<Simplifier> _instance = new(() => new Simplifier());
    public static Simplifier Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, SimplificationResult> _cache = new();
    private readonly AlgebraicSimplifier _algebraic = new();

    private Simplifier() { }

    public SimplificationResult Simplify(Expression expr, SimplificationOptions? options = null)
    {
        options ??= SimplificationOptions.Default;

        var cacheKey = $"{expr.NodeId}:{options.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var result = SimplifyCore(expr, options);
        _cache.TryAdd(cacheKey, result);
        return result;
    }

    public Expression SimplifyInPlace(Expression expr)
    {
        return Simplify(expr).Simplified;
    }

    private SimplificationResult SimplifyCore(Expression expr, SimplificationOptions options)
    {
        var original = expr;
        var current = expr;
        var appliedRules = new List<string>();
        var steps = 0;

        for (var iter = 0; iter < options.MaxIterations; iter++)
        {
            var previous = current;

            if (options.ConstantFolding)
            {
                var folded = ConstantFolder.Fold(current);
                if (!folded.Equals(current))
                {
                    appliedRules.Add("ConstantFolding");
                    current = folded;
                }
            }

            if (options.AlgebraicSimplification)
            {
                var simplified = _algebraic.Simplify(current);
                if (!simplified.Equals(current))
                {
                    appliedRules.AddRange(_algebraic.LastAppliedRules);
                    current = simplified;
                }
            }

            if (options.TrigonometricSimplification)
            {
                var simplified = TrigonometricSimplifier.Simplify(current);
                if (!simplified.Equals(current))
                {
                    appliedRules.AddRange(TrigonometricSimplifier.LastAppliedRules);
                    current = simplified;
                }
            }

            if (options.LogarithmicSimplification)
            {
                var simplified = LogarithmicSimplifier.Simplify(current);
                if (!simplified.Equals(current))
                {
                    appliedRules.AddRange(LogarithmicSimplifier.LastAppliedRules);
                    current = simplified;
                }
            }

            if (options.PowerSimplification)
            {
                var simplified = PowerSimplifier.Simplify(current);
                if (!simplified.Equals(current))
                {
                    appliedRules.AddRange(PowerSimplifier.LastAppliedRules);
                    current = simplified;
                }
            }

            steps++;

            if (AreEquivalent(previous, current, options.Tolerance))
                break;
        }

        return new SimplificationResult
        {
            Original = original,
            Simplified = current,
            AppliedRules = appliedRules.Distinct().ToImmutableArray(),
            Steps = steps
        };
    }

    private static bool AreEquivalent(Expression a, Expression b, double tolerance)
    {
        if (a.Equals(b)) return true;

        if (a is LiteralExpression la && b is LiteralExpression lb)
            return System.Math.Abs(la.Value - lb.Value) < tolerance;

        return false;
    }

    public void ClearCache() => _cache.Clear();
    public int CacheCount => _cache.Count;
}