namespace MathVerse.Math.Compiler.Graph;

using System;
using System.Collections.Generic;

/// <summary>Enumerates the kinds of operations a graph node can represent.</summary>
public enum GraphOperation
{
    /// <summary>An input node that receives external data.</summary>
    Input,

    /// <summary>An output node that produces final results.</summary>
    Output,

    /// <summary>Element-wise addition.</summary>
    Add,

    /// <summary>Element-wise subtraction.</summary>
    Sub,

    /// <summary>Element-wise multiplication.</summary>
    Mul,

    /// <summary>Element-wise division.</summary>
    Div,

    /// <summary>Matrix multiplication.</summary>
    MatMul,

    /// <summary>Convolution operation.</summary>
    Conv,

    /// <summary>ReLU activation.</summary>
    Relu,

    /// <summary>Softmax activation.</summary>
    Softmax,

    /// <summary>Sigmoid activation.</summary>
    Sigmoid,

    /// <summary>Tanh activation.</summary>
    Tanh,

    /// <summary>Exponential function.</summary>
    Exp,

    /// <summary>Logarithm function.</summary>
    Log,

    /// <summary>Square root function.</summary>
    Sqrt,

    /// <summary>Power function.</summary>
    Pow,

    /// <summary>Negation.</summary>
    Neg,

    /// <summary>Absolute value.</summary>
    Abs,

    /// <summary>Reshape operation.</summary>
    Reshape,

    /// <summary>Transpose operation.</summary>
    Transpose,

    /// <summary>Sum reduction.</summary>
    Sum,

    /// <summary>Mean reduction.</summary>
    Mean,

    /// <summary>Max reduction.</summary>
    Max,

    /// <summary>A custom or user-defined operation.</summary>
    Custom,
}

/// <summary>Represents a node in a computation graph.</summary>
/// <param name="Id">Unique identifier for this node.</param>
/// <param name="Operation">The operation this node performs.</param>
/// <param name="Inputs">IDs of input nodes.</param>
/// <param name="Outputs">IDs of output nodes that consume this node's result.</param>
/// <param name="Metadata">Additional metadata key-value pairs.</param>
public sealed record GraphNode
(
    int Id,
    GraphOperation Operation,
    IReadOnlyList<int> Inputs,
    IReadOnlyList<int> Outputs,
    IReadOnlyDictionary<string, object>? Metadata = null
)
{
    /// <summary>Whether this node has any inputs.</summary>
    public bool HasInputs => Inputs.Count > 0;

    /// <summary>Whether this node has any outputs.</summary>
    public bool HasOutputs => Outputs.Count > 0;

    /// <summary>Whether this node is an input node.</summary>
    public bool IsInput => Operation == GraphOperation.Input;

    /// <summary>Whether this node is an output node.</summary>
    public bool IsOutput => Operation == GraphOperation.Output;

    /// <summary>Whether this node is a leaf (no inputs).</summary>
    public bool IsLeaf => Inputs.Count == 0;

    /// <summary>Whether this node is a root (no outputs).</summary>
    public bool IsRoot => Outputs.Count == 0;

    /// <summary>Creates a copy with an additional input.</summary>
    public GraphNode WithAddedInput(int inputId) =>
        this with { Inputs = [.. Inputs, inputId] };

    /// <summary>Creates a copy with an additional output.</summary>
    public GraphNode WithAddedOutput(int outputId) =>
        this with { Outputs = [.. Outputs, outputId] };

    /// <inheritdoc />
    public override string ToString() => $"Node({Id}, {Operation}, In=[{string.Join(",", Inputs)}], Out=[{string.Join(",", Outputs)}])";

    /// <summary>Creates a node with a name and expression string (used by domain compilers).</summary>
    public GraphNode(string name, string expression) : this(
        0,
        GraphOperation.Custom,
        Array.Empty<int>(),
        Array.Empty<int>(),
        new Dictionary<string, object> { ["Name"] = name, ["Expression"] = expression }) { }
}
