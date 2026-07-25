namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;

public abstract class IRVisitor
{
    public virtual void Visit(IRModule module)
    {
        foreach (var func in module.Functions)
            Visit(func);
    }

    public virtual void Visit(IRFunction function)
    {
        foreach (var block in function.Blocks)
            Visit(block);
    }

    public virtual void Visit(IRBlock block)
    {
        foreach (var instruction in block.Instructions)
            Visit(instruction);
        if (block.Terminator != null)
            Visit(block.Terminator);
    }

    public virtual void Visit(IRInstruction instruction)
    {
        switch (instruction.OpCode)
        {
            case IROpCode.Phi:
                if (instruction is IRPhiNode phi)
                    VisitPhi(phi);
                break;
            case IROpCode.Add:
            case IROpCode.Sub:
            case IROpCode.Mul:
            case IROpCode.Div:
            case IROpCode.Mod:
                VisitBinaryOp(instruction);
                break;
            case IROpCode.Neg:
            case IROpCode.Abs:
            case IROpCode.Sqrt:
            case IROpCode.Sin:
            case IROpCode.Cos:
            case IROpCode.Tan:
            case IROpCode.Log:
            case IROpCode.Exp:
                VisitUnaryOp(instruction);
                break;
            case IROpCode.Branch:
            case IROpCode.CondBranch:
                VisitBranch(instruction);
                break;
            case IROpCode.Return:
                VisitReturn(instruction);
                break;
            case IROpCode.Load:
            case IROpCode.Store:
                VisitMemoryOp(instruction);
                break;
            case IROpCode.Call:
                VisitCall(instruction);
                break;
            case IROpCode.Cast:
                VisitCast(instruction);
                break;
            default:
                VisitDefault(instruction);
                break;
        }
    }

    public virtual void VisitPhi(IRPhiNode phi) { }
    public virtual void VisitBinaryOp(IRInstruction instruction) { }
    public virtual void VisitUnaryOp(IRInstruction instruction) { }
    public virtual void VisitBranch(IRInstruction instruction) { }
    public virtual void VisitReturn(IRInstruction instruction) { }
    public virtual void VisitMemoryOp(IRInstruction instruction) { }
    public virtual void VisitCall(IRInstruction instruction) { }
    public virtual void VisitCast(IRInstruction instruction) { }
    public virtual void VisitDefault(IRInstruction instruction) { }
}

public abstract class IRVisitor<T>
{
    public virtual T Visit(IRModule module)
    {
        var defaultResult = default(T)!;
        foreach (var func in module.Functions)
            defaultResult = Visit(func);
        return defaultResult;
    }

    public virtual T Visit(IRFunction function)
    {
        var defaultResult = default(T)!;
        foreach (var block in function.Blocks)
            defaultResult = Visit(block);
        return defaultResult;
    }

    public virtual T Visit(IRBlock block)
    {
        var defaultResult = default(T)!;
        foreach (var instruction in block.Instructions)
            defaultResult = Visit(instruction);
        if (block.Terminator != null)
            defaultResult = Visit(block.Terminator);
        return defaultResult;
    }

    public virtual T Visit(IRInstruction instruction)
    {
        return instruction.OpCode switch
        {
            IROpCode.Phi when instruction is IRPhiNode phi => VisitPhi(phi),
            IROpCode.Add or IROpCode.Sub or IROpCode.Mul or IROpCode.Div or IROpCode.Mod
                => VisitBinaryOp(instruction),
            IROpCode.Neg or IROpCode.Abs or IROpCode.Sqrt or IROpCode.Sin or IROpCode.Cos
                or IROpCode.Tan or IROpCode.Log or IROpCode.Exp
                => VisitUnaryOp(instruction),
            IROpCode.Branch or IROpCode.CondBranch => VisitBranch(instruction),
            IROpCode.Return => VisitReturn(instruction),
            IROpCode.Load or IROpCode.Store => VisitMemoryOp(instruction),
            IROpCode.Call => VisitCall(instruction),
            IROpCode.Cast => VisitCast(instruction),
            _ => VisitDefault(instruction)
        };
    }

    public abstract T VisitPhi(IRPhiNode phi);
    public abstract T VisitBinaryOp(IRInstruction instruction);
    public abstract T VisitUnaryOp(IRInstruction instruction);
    public abstract T VisitBranch(IRInstruction instruction);
    public abstract T VisitReturn(IRInstruction instruction);
    public abstract T VisitMemoryOp(IRInstruction instruction);
    public abstract T VisitCall(IRInstruction instruction);
    public abstract T VisitCast(IRInstruction instruction);
    public abstract T VisitDefault(IRInstruction instruction);
}
