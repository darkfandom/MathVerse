namespace MathVerse.Math.Compiler.IR;

using System;

/// <summary>Represents an operand in the intermediate representation: a constant, variable, or temporary.</summary>
public sealed class IROperand : IEquatable<IROperand>
{
    /// <summary>The kind of operand.</summary>
    public IROperandKind Kind { get; }

    /// <summary>Constant value (only valid when Kind is Constant).</summary>
    public double ConstantValue { get; }

    /// <summary>Name of the variable or temporary (only valid when Kind is Variable or Temporary).</summary>
    public string Name { get; }

    /// <summary>Unique ID for temporaries (only valid when Kind is Temporary).</summary>
    public int TempId { get; }

    private IROperand(IROperandKind kind, double constantValue, string name, int tempId)
    {
        Kind = kind;
        ConstantValue = constantValue;
        Name = name;
        TempId = tempId;
    }

    /// <summary>Creates a constant operand with the specified value.</summary>
    public static IROperand CreateConstant(double value) =>
        new(IROperandKind.Constant, value, string.Empty, -1);

    /// <summary>Creates a variable operand with the specified name.</summary>
    public static IROperand CreateVariable(string name) =>
        new(IROperandKind.Variable, 0.0, name ?? throw new ArgumentNullException(nameof(name)), -1);

    /// <summary>Creates a temporary operand with the specified ID.</summary>
    public static IROperand CreateTemporary(int tempId, string? debugName = null) =>
        new(IROperandKind.Temporary, 0.0, debugName ?? $"t{tempId}", tempId);

    /// <inheritdoc />
    public bool Equals(IROperand? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Kind != other.Kind) return false;
        return Kind switch
        {
            IROperandKind.Constant => ConstantValue.Equals(other.ConstantValue),
            IROperandKind.Variable => string.Equals(Name, other.Name, StringComparison.Ordinal),
            IROperandKind.Temporary => TempId == other.TempId,
            _ => false,
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as IROperand);

    /// <inheritdoc />
    public override int GetHashCode() => Kind switch
    {
        IROperandKind.Constant => HashCode.Combine(Kind, ConstantValue),
        IROperandKind.Variable => HashCode.Combine(Kind, Name),
        IROperandKind.Temporary => HashCode.Combine(Kind, TempId),
        _ => Kind.GetHashCode(),
    };

    /// <inheritdoc />
    public override string ToString() => Kind switch
    {
        IROperandKind.Constant => ConstantValue.ToString("G"),
        IROperandKind.Variable => Name,
        IROperandKind.Temporary => Name,
        _ => "?",
    };

    /// <summary>Equality operator.</summary>
    public static bool operator ==(IROperand? left, IROperand? right) => Equals(left, right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(IROperand? left, IROperand? right) => !Equals(left, right);

    /// <summary>Implicitly converts an IROperand to an IRValue.</summary>
    public static implicit operator IRValue(IROperand op)
        => op.Kind switch
        {
            IROperandKind.Constant => IRValue.CreateConstant(op.Name, op.ConstantValue),
            _ => IRValue.CreateRegister(op.Name, IRType.Float64),
        };
}

/// <summary>Enumerates the kinds of IR operands.</summary>
public enum IROperandKind
{
    /// <summary>A numeric constant value.</summary>
    Constant,

    /// <summary>A named variable input.</summary>
    Variable,

    /// <summary>A compiler-generated temporary.</summary>
    Temporary,
}
