namespace MathVerse.Math.Compiler.Differentiation;

using System;

/// <summary>Represents a value in reverse-mode automatic differentiation with an associated gradient accumulator.</summary>
public sealed class AdjointValue
{
    /// <summary>The computed real value.</summary>
    public double Value { get; }

    /// <summary>The accumulated gradient (dL/dValue).</summary>
    public double Gradient { get; internal set; }

    /// <summary>Whether this value is a leaf (input) that should receive gradients.</summary>
    public bool IsLeaf { get; }

    /// <summary>Optional name for debugging.</summary>
    public string? Name { get; }

    /// <summary>Creates a new adjoint value.</summary>
    public AdjointValue(double value, bool isLeaf = false, string? name = null, double gradient = 0.0)
    {
        Value = value;
        Gradient = gradient;
        IsLeaf = isLeaf;
        Name = name;
    }

    /// <summary>Creates a leaf (input) value with the specified name and initial gradient of zero.</summary>
    public static AdjointValue CreateLeaf(double value, string? name = null) =>
        new(value, true, name, 0.0);

    /// <summary>Creates an intermediate (non-leaf) computed value.</summary>
    public static AdjointValue CreateIntermediate(double value, string? name = null) =>
        new(value, false, name, 0.0);

    /// <summary>Creates a constant value that does not participate in gradient computation.</summary>
    public static AdjointValue CreateConstant(double value, string? name = null) =>
        new(value, false, name, 0.0);

    /// <summary>Adds to the gradient accumulator.</summary>
    public void AccumulateGradient(double grad) => Gradient += grad;

    /// <summary>Resets the gradient to zero.</summary>
    public void ResetGradient() => Gradient = 0.0;

    /// <summary>Creates a new AdjointValue with the same value but reset gradient.</summary>
    public AdjointValue WithResetGradient() => new(Value, IsLeaf, Name, 0.0);

    /// <inheritdoc />
    public override string ToString() =>
        Name is not null ? $"{Name}={Value} (grad={Gradient})" : $"{Value} (grad={Gradient})";
}
