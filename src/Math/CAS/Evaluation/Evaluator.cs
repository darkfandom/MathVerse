namespace MathVerse.Math.CAS.Evaluation;

using MathVerse.Math.CAS.Substitution;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;

public sealed class Evaluator
{
    private static readonly Lazy<Evaluator> _instance = new(() => new Evaluator());
    public static Evaluator Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, EvaluationResult> _cache = new();

    private Evaluator() { }

    public EvaluationResult Evaluate(Expression expr, ImmutableDictionary<string, double>? vars = null, EvaluationOptions? options = null)
    {
        options ??= EvaluationOptions.Default;
        vars ??= ImmutableDictionary<string, double>.Empty;

        var cacheKey = $"{expr.NodeId}:{GetVarsHash(vars)}:{options.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var result = EvaluateCore(expr, vars, options);
        _cache.TryAdd(cacheKey, result);
        return result;
    }

    public double EvaluateToDouble(Expression expr, ImmutableDictionary<string, double>? vars = null, EvaluationOptions? options = null)
    {
        var result = Evaluate(expr, vars, options);
        if (result.Result is LiteralExpression lit)
            return lit.Value;

        if (result.Result is ComplexExpression c)
            return c.Real is LiteralExpression rl ? rl.Value : double.NaN;

        return double.NaN;
    }

    public Complex EvaluateToComplex(Expression expr, ImmutableDictionary<string, Complex>? vars = null, EvaluationOptions? options = null)
    {
        options ??= EvaluationOptions.Default;
        vars ??= ImmutableDictionary<string, Complex>.Empty;

        var doubleVars = vars.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.Real);
        var result = EvaluateCore(expr, doubleVars, options with { AllowComplex = true });

        return result.Result switch
        {
            LiteralExpression l => new Complex(l.Value, 0),
            ComplexExpression c when c.Real is LiteralExpression rl && c.Imaginary is LiteralExpression il =>
                new Complex(rl.Value, il.Value),
            _ => Complex.NaN
        };
    }

    private EvaluationResult EvaluateCore(Expression expr, ImmutableDictionary<string, double> vars, EvaluationOptions options)
    {
        var original = expr;
        var substituted = SubstituteVariables(expr, vars);
        var evaluated = NumericEvaluator.EvaluateDouble(substituted, vars);

        return new EvaluationResult
        {
            Original = original,
            Result = evaluated,
            VariableValues = vars,
            IsExact = evaluated is LiteralExpression
        };
    }

    private Expression SubstituteVariables(Expression expr, ImmutableDictionary<string, double> vars)
    {
        if (vars.IsEmpty)
            return expr;

        return SubstitutionEngine.SubstituteVariables(expr, vars.ToImmutableDictionary(
            kvp => kvp.Key,
            kvp => (Expression)Expr.Literal(kvp.Value)));
    }

    private static int GetVarsHash(ImmutableDictionary<string, double> vars)
    {
        var hash = new HashCode();
        foreach (var kvp in vars)
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }

    public void ClearCache() => _cache.Clear();
}