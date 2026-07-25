namespace MathVerse.Math.Compiler.Tensor;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

/// <summary>Represents a symbolic tensor expression resulting from tensor operations.</summary>
public sealed class TensorExpression
{
    /// <summary>The type of tensor operation.</summary>
    public TensorOpType OpType { get; }
    /// <summary>The input tensor expressions that this operation consumes.</summary>
    public IReadOnlyList<TensorExpression> Inputs { get; }
    /// <summary>The output shape of this expression.</summary>
    public IReadOnlyList<int> Shape { get; }
    /// <summary>Optional string label for debugging.</summary>
    public string? Label { get; }

    /// <summary>Initializes a new instance of the <see cref="TensorExpression"/> class.</summary>
    public TensorExpression(TensorOpType opType, IReadOnlyList<TensorExpression> inputs, IReadOnlyList<int> shape, string? label = null)
    {
        OpType = opType;
        Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
        Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        Label = label;
    }

    /// <summary>Returns a shallow clone with a new label.</summary>
    public TensorExpression WithLabel(string label)
    {
        return new TensorExpression(OpType, Inputs, Shape, label);
    }

    /// <summary>Returns a human-readable string representation of this expression.</summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(OpType.ToString());
        if (Label != null)
            sb.Append('_').Append(Label);
        sb.Append('(').Append(Inputs.Count).Append(" inputs) -> [");
        for (var i = 0; i < Shape.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(Shape[i]);
        }
        sb.Append(']');
        return sb.ToString();
    }
}
