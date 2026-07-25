namespace MathVerse.Math.Numerics.Evaluation;

using System.Collections.Immutable;
using MathVerse.Math.Expressions;

public sealed record NumericContext
{
    public NumericPrecision Precision { get; init; } = NumericPrecision.Double;
    public int MaxIterations { get; init; } = 1000;
    public double Tolerance { get; init; } = 1e-12;
    public bool AllowComplex { get; init; } = true;
    public bool ThrowOnError { get; init; } = false;
    public ImmutableDictionary<string, double> Constants { get; init; } = ImmutableDictionary<string, double>.Empty;
    public ImmutableDictionary<string, Expression> Variables { get; init; } = ImmutableDictionary<string, Expression>.Empty;

    public static NumericContext Default { get; } = new();

    public NumericContext WithPrecision(NumericPrecision precision) => this with { Precision = precision };

    public NumericContext WithTolerance(double tolerance) => this with { Tolerance = tolerance };

    public NumericContext WithMaxIterations(int maxIterations) => this with { MaxIterations = maxIterations };

    public NumericContext WithComplexAllowed(bool allow) => this with { AllowComplex = allow };

    public NumericContext WithThrowOnError(bool throwOnError) => this with { ThrowOnError = throwOnError };

    public NumericContext AddConstant(string name, double value) => this with { Constants = Constants.SetItem(name, value) };

    public NumericContext AddConstants(IEnumerable<KeyValuePair<string, double>> constants) => this with { Constants = Constants.AddRange(constants) };

    public NumericContext AddVariable(string name, Expression expression) => this with { Variables = Variables.SetItem(name, expression) };

    public NumericContext AddVariables(IEnumerable<KeyValuePair<string, Expression>> variables) => this with { Variables = Variables.AddRange(variables) };

    public NumericContext RemoveConstant(string name) => this with { Constants = Constants.Remove(name) };

    public NumericContext RemoveVariable(string name) => this with { Variables = Variables.Remove(name) };

    public NumericContext ClearConstants() => this with { Constants = ImmutableDictionary<string, double>.Empty };

    public NumericContext ClearVariables() => this with { Variables = ImmutableDictionary<string, Expression>.Empty };
}