namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class IRInstruction
{
    public IROpCode OpCode { get; }
    public IRValue? Result { get; }
    public virtual IReadOnlyList<IRValue> Operands { get; }
    public IRBlock? ParentBlock { get; internal set; }
    public int SequenceIndex { get; internal set; }

    internal List<IRValue> MutableOperands { get; }

    public IRValue? Left => Operands.Count > 0 ? Operands[0] : null;
    public IRValue? Right => Operands.Count > 1 ? Operands[1] : null;
    public IRValue? Destination => Operands.Count > 0 ? Operands[^1] : null;

    public IRInstruction(IROpCode opCode, IRValue? result, params IRValue[] operands)
    {
        OpCode = opCode;
        Result = result;
        MutableOperands = new List<IRValue>(operands);
        Operands = MutableOperands;
    }

    public IRInstruction(IROpCode opCode, IRValue? result, IReadOnlyList<IRValue> operands)
    {
        OpCode = opCode;
        Result = result;
        MutableOperands = new List<IRValue>(operands);
        Operands = MutableOperands;
    }

    public IRInstruction(IROperation op, IRValue? result, IReadOnlyList<IRValue> operands)
    {
        OpCode = MapOperation(op);
        Result = result;
        MutableOperands = new List<IRValue>(operands);
        Operands = MutableOperands;
    }

    public virtual bool HasSideEffects => OpCode is IROpCode.Store or IROpCode.Alloc or IROpCode.Call;

    public virtual bool IsTerminator => OpCode is IROpCode.Branch or IROpCode.CondBranch or IROpCode.Return;

    public bool IsMemoryOperation => OpCode is IROpCode.Load or IROpCode.Store or IROpCode.Alloc;

    public IROperation Operation => ReverseMapOpCode(OpCode);

    public IROperand? Operand => Left is not null ? (IROperand)Left : null;

    public static IRInstruction CreateFunction(string name, IReadOnlyList<IRValue> args, IRValue dest)
        => new(IROpCode.Call, dest, args.Prepend(IRValue.CreateRegister(name, IRType.Float64)).ToArray());

    public static IRInstruction CreateFunction(string name, IReadOnlyList<IROperand> args, IROperand dest)
        => CreateFunction(name, args.Select(a => (IRValue)a).ToList(), (IRValue)dest);

    public static IRInstruction CreateFunction(string name, IROperand[] args, IROperand dest)
        => CreateFunction(name, args.Select(a => (IRValue)a).ToArray(), (IRValue)dest);

    public static IRInstruction CreateBinary(IROperation op, IRValue left, IRValue right, IRValue result)
        => new(op, result, new[] { left, right });

    public static IRInstruction CreateUnary(IROperation op, IRValue operand, IRValue result)
        => new(op, result, new[] { operand });

    public static IRInstruction CreateLoadConst(double value, IRValue result)
        => new(IROpCode.Load, result, IRValue.CreateConstant(value));

    public static IRInstruction CreateLoadVar(string name, IRValue result)
        => new(IROpCode.Load, result, IRValue.CreateRegister(name, result.Type));

    public static IRInstruction CreateLoadVar(string name, IReadOnlyList<IRValue> args, IRValue result)
        => new(IROpCode.Call, result, args.Prepend(IRValue.CreateRegister(name, IRType.Float64)).ToArray());

    public static IRInstruction CreateStore(IRValue address, IRValue value)
        => new(IROpCode.Store, null, address, value);

    public IRInstruction WithOperands(IReadOnlyList<IRValue> newOperands)
        => new(OpCode, Result, newOperands);

    public IRInstruction WithResult(IRValue newResult)
        => new(OpCode, newResult, Operands);

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Result != null)
            sb.Append($"{Result} = ");
        sb.Append(OpCode);
        if (Operands.Count > 0)
        {
            sb.Append('(');
            for (var i = 0; i < Operands.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(Operands[i]);
            }
            sb.Append(')');
        }
        return sb.ToString();
    }

    private static IROpCode MapOperation(IROperation op)
    {
        return op switch
        {
            IROperation.Add => IROpCode.Add,
            IROperation.Sub => IROpCode.Sub,
            IROperation.Mul => IROpCode.Mul,
            IROperation.Div => IROpCode.Div,
            IROperation.Pow => IROpCode.Pow,
            IROperation.Mod => IROpCode.Mod,
            IROperation.Neg => IROpCode.Neg,
            IROperation.Abs => IROpCode.Abs,
            IROperation.Sin => IROpCode.Sin,
            IROperation.Cos => IROpCode.Cos,
            IROperation.Tan => IROpCode.Tan,
            IROperation.Exp => IROpCode.Exp,
            IROperation.Ln => IROpCode.Log,
            IROperation.Log => IROpCode.Log,
            IROperation.Sqrt => IROpCode.Sqrt,
            IROperation.Fma => IROpCode.Fma,
            IROperation.Dot => IROpCode.Dot,
            IROperation.MatMul => IROpCode.MatMul,
            IROperation.Reshape => IROpCode.Reshape,
            IROperation.Transpose => IROpCode.Transpose,
            IROperation.Sum => IROpCode.Sum,
            IROperation.VectorOp => IROpCode.VectorOp,
            IROperation.Load => IROpCode.Load,
            IROperation.Store => IROpCode.Store,
            IROperation.Alloc => IROpCode.Alloc,
            IROperation.Branch => IROpCode.Branch,
            IROperation.CondBranch => IROpCode.CondBranch,
            IROperation.Return => IROpCode.Return,
            IROperation.Phi => IROpCode.Phi,
            IROperation.Call => IROpCode.Call,
            IROperation.Cast => IROpCode.Cast,
            IROperation.LoadVar => IROpCode.Load,
            IROperation.LoadConst => IROpCode.Load,
            IROperation.StoreVar => IROpCode.Store,
            _ => IROpCode.Nop
        };
    }

    private static IROperation ReverseMapOpCode(IROpCode code)
    {
        return code switch
        {
            IROpCode.Add => IROperation.Add,
            IROpCode.Sub => IROperation.Sub,
            IROpCode.Mul => IROperation.Mul,
            IROpCode.Div => IROperation.Div,
            IROpCode.Pow => IROperation.Pow,
            IROpCode.Mod => IROperation.Mod,
            IROpCode.Neg => IROperation.Neg,
            IROpCode.Abs => IROperation.Abs,
            IROpCode.Sin => IROperation.Sin,
            IROpCode.Cos => IROperation.Cos,
            IROpCode.Tan => IROperation.Tan,
            IROpCode.Exp => IROperation.Exp,
            IROpCode.Log => IROperation.Ln,
            IROpCode.Sqrt => IROperation.Sqrt,
            IROpCode.Load => IROperation.Load,
            IROpCode.Store => IROperation.Store,
            IROpCode.Alloc => IROperation.Alloc,
            IROpCode.Branch => IROperation.Branch,
            IROpCode.CondBranch => IROperation.CondBranch,
            IROpCode.Return => IROperation.Return,
            IROpCode.Phi => IROperation.Phi,
            IROpCode.Call => IROperation.Call,
            IROpCode.Cast => IROperation.Cast,
            IROpCode.Fma => IROperation.Fma,
            IROpCode.Dot => IROperation.Dot,
            IROpCode.MatMul => IROperation.MatMul,
            IROpCode.Reshape => IROperation.Reshape,
            IROpCode.Transpose => IROperation.Transpose,
            IROpCode.Sum => IROperation.Sum,
            IROpCode.VectorOp => IROperation.VectorOp,
            IROpCode.Nop => IROperation.Nop,
            _ => IROperation.Nop,
        };
    }
}
