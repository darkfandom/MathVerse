namespace MathVerse.Math.Numerics.Evaluation;

using System.Collections.Immutable;
using MathVerse.Math.Expressions;
using System.Numerics;

public sealed record EvaluationContext
{
    public NumericContext Context { get; init; } = NumericContext.Default;
    public ImmutableDictionary<string, double> VariableValues { get; init; } = ImmutableDictionary<string, double>.Empty;
    public ImmutableDictionary<string, Complex> ComplexVariableValues { get; init; } = ImmutableDictionary<string, Complex>.Empty;
    public ImmutableDictionary<string, Expression> FunctionCache { get; init; } = ImmutableDictionary<string, Expression>.Empty;
    public int RecursionDepth { get; init; } = 0;
    public int MaxRecursionDepth { get; init; } = 1000;
    public bool EnableCaching { get; init; } = true;
    public bool TrackDependencies { get; init; } = false;

    public static EvaluationContext Default { get; } = new();

    public EvaluationContext WithContext(NumericContext context) => this with { Context = context };

    public EvaluationContext WithVariables(ImmutableDictionary<string, double> variables) => this with { VariableValues = variables };

    public EvaluationContext WithComplexVariables(ImmutableDictionary<string, Complex> variables) => this with { ComplexVariableValues = variables };

    public EvaluationContext WithFunctionCache(ImmutableDictionary<string, Expression> cache) => this with { FunctionCache = cache };

    public EvaluationContext WithRecursionDepth(int depth) => this with { RecursionDepth = depth };

    public EvaluationContext WithMaxRecursionDepth(int maxDepth) => this with { MaxRecursionDepth = maxDepth };

    public EvaluationContext WithCaching(bool enable) => this with { EnableCaching = enable };

    public EvaluationContext WithDependencyTracking(bool track) => this with { TrackDependencies = track };

    public EvaluationContext SetVariable(string name, double value) => this with { VariableValues = VariableValues.SetItem(name, value) };

    public EvaluationContext SetComplexVariable(string name, Complex value) => this with { ComplexVariableValues = ComplexVariableValues.SetItem(name, value) };

    public EvaluationContext RemoveVariable(string name) => this with { VariableValues = VariableValues.Remove(name) };

    public EvaluationContext RemoveComplexVariable(string name) => this with { ComplexVariableValues = ComplexVariableValues.Remove(name) };

    public EvaluationContext ClearVariables() => this with { VariableValues = ImmutableDictionary<string, double>.Empty };

    public EvaluationContext ClearComplexVariables() => this with { ComplexVariableValues = ImmutableDictionary<string, Complex>.Empty };

    public EvaluationContext ClearFunctionCache() => this with { FunctionCache = ImmutableDictionary<string, Expression>.Empty };

    public bool TryGetVariable(string name, out double value) => VariableValues.TryGetValue(name, out value);

    public bool TryGetComplexVariable(string name, out Complex value) => ComplexVariableValues.TryGetValue(name, out value);

    public bool HasVariable(string name) => VariableValues.ContainsKey(name) || ComplexVariableValues.ContainsKey(name);

    public EvaluationContext IncrementDepth() => this with { RecursionDepth = RecursionDepth + 1 };

    public bool IsMaxDepthReached => RecursionDepth >= MaxRecursionDepth;
}

public static class EvaluationContextExtensions
{
    public static EvaluationContext WithVariables(this EvaluationContext ctx, IEnumerable<KeyValuePair<string, double>> variables)
        => ctx with { VariableValues = ctx.VariableValues.AddRange(variables) };

    public static EvaluationContext WithComplexVariables(this EvaluationContext ctx, IEnumerable<KeyValuePair<string, Complex>> variables)
        => ctx with { ComplexVariableValues = ctx.ComplexVariableValues.AddRange(variables) };
}