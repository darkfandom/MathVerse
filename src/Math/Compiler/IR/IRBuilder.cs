namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed class IRBuilder
{
    private readonly IRModule _module;
    private IRFunction? _currentFunction;
    private IRBlock? _currentBlock;
    private int _registerCounter;

    public IRBuilder(string moduleName)
    {
        _module = new IRModule(moduleName);
    }

    public IRBuilder(IRModule module)
    {
        _module = module;
    }

    public IRModule Module => _module;
    public IRFunction? CurrentFunction => _currentFunction;
    public IRBlock? CurrentBlock => _currentBlock;

    public IRFunction CreateFunction(string name, IRType returnType, IEnumerable<IRValue>? parameters = null)
    {
        _currentFunction = _module.CreateFunction(name, returnType, parameters);
        return _currentFunction;
    }

    public IRBlock CreateBlock(string? label = null)
    {
        if (_currentFunction == null)
            throw new InvalidOperationException("No current function. Call CreateFunction first.");
        _currentBlock = _currentFunction.CreateBlock(label);
        return _currentBlock;
    }

    public void SetInsertPoint(IRBlock block)
    {
        _currentBlock = block;
    }

    public IRValue CreateTempRegister(IRType type = IRType.Float64)
        => IRValue.CreateRegister($"%r{_registerCounter++}", type);

    public IRValue CreateNamedRegister(string name, IRType type = IRType.Float64)
        => IRValue.CreateRegister(name, type);

    public IRValue Emit(IROpCode opCode, IRValue? result, params IRValue[] operands)
    {
        if (_currentBlock == null)
            throw new InvalidOperationException("No current block.");
        var inst = new IRInstruction(opCode, result, operands);
        _currentBlock.AppendInstruction(inst);
        return result ?? IRValue.CreateVoid();
    }

    public IRValue BuildAdd(IRValue left, IRValue right, string? name = null)
    {
        var resultType = IRTypeHelper.Widen(left.Type, right.Type);
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", resultType);
        Emit(IROpCode.Add, result, left, right);
        return result;
    }

    public IRValue BuildSub(IRValue left, IRValue right, string? name = null)
    {
        var resultType = IRTypeHelper.Widen(left.Type, right.Type);
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", resultType);
        Emit(IROpCode.Sub, result, left, right);
        return result;
    }

    public IRValue BuildMul(IRValue left, IRValue right, string? name = null)
    {
        var resultType = IRTypeHelper.Widen(left.Type, right.Type);
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", resultType);
        Emit(IROpCode.Mul, result, left, right);
        return result;
    }

    public IRValue BuildDiv(IRValue left, IRValue right, string? name = null)
    {
        var resultType = IRTypeHelper.Widen(left.Type, right.Type);
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", resultType);
        Emit(IROpCode.Div, result, left, right);
        return result;
    }

    public IRValue BuildMod(IRValue left, IRValue right, string? name = null)
    {
        var resultType = IRTypeHelper.Widen(left.Type, right.Type);
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", resultType);
        Emit(IROpCode.Mod, result, left, right);
        return result;
    }

    public IRValue BuildNeg(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Neg, result, operand);
        return result;
    }

    public IRValue BuildAbs(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Abs, result, operand);
        return result;
    }

    public IRValue BuildFma(IRValue a, IRValue b, IRValue c, string? name = null)
    {
        var resultType = IRTypeHelper.Widen(IRTypeHelper.Widen(a.Type, b.Type), c.Type);
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", resultType);
        Emit(IROpCode.Fma, result, a, b, c);
        return result;
    }

    public IRValue BuildSqrt(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Sqrt, result, operand);
        return result;
    }

    public IRValue BuildSin(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Sin, result, operand);
        return result;
    }

    public IRValue BuildCos(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Cos, result, operand);
        return result;
    }

    public IRValue BuildTan(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Tan, result, operand);
        return result;
    }

    public IRValue BuildLog(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Log, result, operand);
        return result;
    }

    public IRValue BuildExp(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Exp, result, operand);
        return result;
    }

    public IRValue BuildPow(IRValue baseVal, IRValue exponent, string? name = null)
    {
        var resultType = IRTypeHelper.Widen(baseVal.Type, exponent.Type);
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", resultType);
        Emit(IROpCode.Pow, result, baseVal, exponent);
        return result;
    }

    public IRValue BuildDot(IRValue left, IRValue right, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", IRType.Vector);
        Emit(IROpCode.Dot, result, left, right);
        return result;
    }

    public IRValue BuildSum(IRValue operand, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", operand.Type);
        Emit(IROpCode.Sum, result, operand);
        return result;
    }

    public IRValue BuildReshape(IRValue source, IRValue targetShape, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", IRType.Tensor);
        Emit(IROpCode.Reshape, result, source, targetShape);
        return result;
    }

    public IRValue BuildTranspose(IRValue source, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", source.Type);
        Emit(IROpCode.Transpose, result, source);
        return result;
    }

    public IRValue BuildMatMul(IRValue left, IRValue right, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", IRType.Tensor);
        Emit(IROpCode.MatMul, result, left, right);
        return result;
    }

    public IRValue BuildLoad(IRValue address, IRType type, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", type);
        Emit(IROpCode.Load, result, address);
        return result;
    }

    public void BuildStore(IRValue address, IRValue value)
    {
        Emit(IROpCode.Store, null, address, value);
    }

    public IRValue BuildAlloc(IRType type, IRValue? size = null, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", IRType.Pointer);
        if (size != null)
            Emit(IROpCode.Alloc, result, size);
        else
            Emit(IROpCode.Alloc, result);
        return result;
    }

    public void BuildBranch(IRBlock target)
    {
        if (_currentBlock == null) throw new InvalidOperationException("No current block.");
        var inst = new IRInstruction(IROpCode.Branch, null, IRValue.CreateConstant(0.0));
        target.AddPredecessor(_currentBlock);
        _currentBlock.AppendInstruction(inst);
        _currentBlock.Terminator = inst;
    }

    public void BuildCondBranch(IRValue condition, IRBlock trueTarget, IRBlock falseTarget)
    {
        if (_currentBlock == null) throw new InvalidOperationException("No current block.");
        var inst = new IRInstruction(IROpCode.CondBranch, null, condition);
        trueTarget.AddPredecessor(_currentBlock);
        falseTarget.AddPredecessor(_currentBlock);
        _currentBlock.AppendInstruction(inst);
        _currentBlock.Terminator = inst;
    }

    public void BuildReturn(IRValue? value = null)
    {
        Emit(IROpCode.Return, null, value ?? IRValue.CreateVoid());
    }

    public IRValue BuildCall(IRFunction function, IEnumerable<IRValue> arguments, string? name = null)
    {
        var result = function.ReturnType == IRType.Void
            ? null
            : IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", function.ReturnType);
        var operands = arguments.Prepend(IRValue.CreateConstant(function.Name.GetHashCode())).ToArray();
        Emit(IROpCode.Call, result, operands);
        return result ?? IRValue.CreateVoid();
    }

    public IRValue BuildCast(IRValue value, IRType targetType, string? name = null)
    {
        if (value.Type == targetType) return value;
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", targetType);
        Emit(IROpCode.Cast, result, value);
        return result;
    }

    public IRValue BuildVectorOp(IRValue left, IRValue right, string? name = null)
    {
        var result = IRValue.CreateRegister(name ?? $"%r{_registerCounter++}", IRType.Vector);
        Emit(IROpCode.VectorOp, result, left, right);
        return result;
    }

    public IRPhiNode BuildPhi(IRType type, IEnumerable<(IRValue Value, IRBlock Block)> incoming)
    {
        var edges = incoming.ToImmutableArray();
        var result = IRValue.CreateRegister($"%phi{_registerCounter++}", type);
        var phi = new IRPhiNode(result, edges);
        _currentBlock?.AppendInstruction(phi);
        return phi;
    }

    public IRModule Build()
    {
        return _module;
    }
}
