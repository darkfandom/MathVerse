namespace MathVerse.Math.Compiler.Tensor;

/// <summary>Enumeration of tensor operation types.</summary>
public enum TensorOpType
{
    /// <summary>Matrix multiplication.</summary>
    MatMul,
    /// <summary>Element-wise addition.</summary>
    Add,
    /// <summary>Element-wise subtraction.</summary>
    Sub,
    /// <summary>Element-wise multiplication.</summary>
    Mul,
    /// <summary>Element-wise division.</summary>
    Div,
    /// <summary>Reduction: sum over axes.</summary>
    Sum,
    /// <summary>Reduction: mean over axes.</summary>
    Mean,
    /// <summary>Reduction: max over axes.</summary>
    Max,
    /// <summary>Reduction: min over axes.</summary>
    Min,
    /// <summary>Reshape tensor dimensions.</summary>
    Reshape,
    /// <summary>Transpose tensor axes.</summary>
    Transpose,
    /// <summary>Unary negation.</summary>
    Neg,
    /// <summary>Exponentiation base e.</summary>
    Exp,
    /// <summary>Natural logarithm.</summary>
    Log,
    /// <summary>Square root.</summary>
    Sqrt,
    /// <summary>Copy tensor.</summary>
    Copy,
    /// <summary>Split along an axis.</summary>
    Split,
    /// <summary>Concatenate along an axis.</summary>
    Concat,
    /// <summary>Slice along dimensions.</summary>
    Slice,
    /// <summary>Broadcast to a new shape.</summary>
    Broadcast,
    /// <summary>Unary positive.</summary>
    Pos
}
