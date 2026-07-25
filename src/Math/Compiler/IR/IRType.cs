namespace MathVerse.Math.Compiler.IR;

using System;

public enum IRType
{
    Float32,
    Float64,
    Int32,
    Int64,
    Bool,
    Void,
    Pointer,
    Vector,
    Tensor
}

public static class IRTypeHelper
{
    public static int SizeInBytes(IRType type)
    {
        return type switch
        {
            IRType.Float32 => 4,
            IRType.Float64 => 8,
            IRType.Int32 => 4,
            IRType.Int64 => 8,
            IRType.Bool => 1,
            IRType.Void => 0,
            IRType.Pointer => 8,
            IRType.Vector => 32,
            IRType.Tensor => 64,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown IR type")
        };
    }

    public static bool IsFloatingPoint(IRType type)
        => type == IRType.Float32 || type == IRType.Float64;

    public static bool IsInteger(IRType type)
        => type == IRType.Int32 || type == IRType.Int64;

    public static bool IsNumeric(IRType type)
        => IsFloatingPoint(type) || IsInteger(type);

    public static IRType Widen(IRType left, IRType right)
    {
        if (left == right) return left;
        if (left == IRType.Float64 || right == IRType.Float64) return IRType.Float64;
        if (left == IRType.Float32 || right == IRType.Float32) return IRType.Float32;
        if (left == IRType.Int64 || right == IRType.Int64) return IRType.Int64;
        return IRType.Int32;
    }

    public static string ToDisplayName(IRType type)
    {
        return type switch
        {
            IRType.Float32 => "f32",
            IRType.Float64 => "f64",
            IRType.Int32 => "i32",
            IRType.Int64 => "i64",
            IRType.Bool => "bool",
            IRType.Void => "void",
            IRType.Pointer => "ptr",
            IRType.Vector => "vec",
            IRType.Tensor => "tensor",
            _ => "unknown"
        };
    }
}
