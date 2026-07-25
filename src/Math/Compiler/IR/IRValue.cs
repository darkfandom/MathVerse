namespace MathVerse.Math.Compiler.IR;

using System;
using System.Threading;

public sealed class IRValue
{
    private static int _globalId;

    public string Name { get; }
    public IRType Type { get; }
    public bool IsConstant { get; }
    public double? ConstantValue { get; }
    public int Id { get; }

    private IRValue(string name, IRType type, bool isConstant, double? constantValue)
    {
        Name = name;
        Type = type;
        IsConstant = isConstant;
        ConstantValue = constantValue;
        Id = Interlocked.Increment(ref _globalId);
    }

    public static IRValue CreateRegister(string name, IRType type)
        => new(name, type, false, null);

    public static IRValue CreateConstant(string name, double value, IRType type = IRType.Float64)
        => new(name, type, true, value);

    public static IRValue CreateConstant(double value, IRType type = IRType.Float64)
        => new($"c{Interlocked.Increment(ref _globalId)}", type, true, value);

    public static IRValue CreateVoid()
        => new("_void", IRType.Void, true, null);

    public override string ToString()
        => IsConstant && ConstantValue.HasValue
            ? $"{Name}:{IRTypeHelper.ToDisplayName(Type)} = {ConstantValue.Value}"
            : $"{Name}:{IRTypeHelper.ToDisplayName(Type)}";

    public override bool Equals(object? obj)
        => obj is IRValue other && Id == other.Id;

    public override int GetHashCode() => Id;

    public IROperandKind Kind => IsConstant ? IROperandKind.Constant : IROperandKind.Variable;

    /// <summary>Implicitly converts an IRValue to an IROperand.</summary>
    public static implicit operator IROperand(IRValue val)
        => val.IsConstant && val.ConstantValue.HasValue
            ? IROperand.CreateConstant(val.ConstantValue.Value)
            : IROperand.CreateVariable(val.Name);
}
