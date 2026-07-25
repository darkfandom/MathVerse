namespace MathVerse.Math.CAS.Factorization;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Concurrent;
using System.Collections.Immutable;

public sealed class Factorizer
{
    private static readonly Lazy<Factorizer> _instance = new(() => new Factorizer());
    public static Factorizer Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, FactorizationResult> _cache = new();
    private readonly CommonFactorExtractor _commonFactor = new();
    private readonly PolynomialFactorizer _polynomial = new();
    private readonly TrigonometricFactorizer _trigonometric = new();

    private Factorizer() { }

    public FactorizationResult Factor(Expression expr, FactorizationOptions? options = null)
    {
        options ??= FactorizationOptions.Default;

        var cacheKey = $"{expr.NodeId}:{options.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var result = FactorCore(expr, options);
        _cache.TryAdd(cacheKey, result);
        return result;
    }

    private FactorizationResult FactorCore(Expression expr, FactorizationOptions options)
    {
        var original = expr;
        var current = expr;
        var steps = new List<string>();

        if (options.FactorCommonTerms)
        {
            var factored = _commonFactor.ExtractCommonFactor(current, steps);
            if (!factored.Equals(current))
            {
                steps.Add("ExtractCommonFactor");
                current = factored;
            }
        }

        if (options.FactorPolynomials)
        {
            var factored = _polynomial.FactorPolynomial(current, steps);
            if (!factored.Equals(current))
            {
                steps.Add("FactorPolynomial");
                current = factored;
            }
        }

        if (options.FactorTrigonometric)
        {
            var factored = _trigonometric.FactorTrig(current, steps);
            if (!factored.Equals(current))
            {
                steps.Add("FactorTrigonometric");
                current = factored;
            }
        }

        var isFullyFactored = IsFullyFactored(current, options);

        return new FactorizationResult
        {
            Original = original,
            Factored = current,
            Steps = steps.Distinct().ToImmutableArray(),
            IsFullyFactored = isFullyFactored
        };
    }

    private static bool IsFullyFactored(Expression expr, FactorizationOptions options)
    {
        return expr switch
        {
            BinaryExpression b when b.Operator.Equals(MathOperator.Multiply) => true,
            _ => true
        };
    }

    public void ClearCache() => _cache.Clear();
    public int CacheCount => _cache.Count;
}