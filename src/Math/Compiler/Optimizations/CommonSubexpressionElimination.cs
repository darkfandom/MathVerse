namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Identifies identical expressions within a basic block and replaces duplicate
/// computations with a single computation. Uses a dictionary keyed by
/// (opcode, sorted operand names) to detect common subexpressions.
/// </summary>
public sealed class CommonSubexpressionElimination : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "CommonSubexpressionElimination";

    /// <inheritdoc />
    public IRModule Optimize(IRModule module)
    {
        foreach (var function in module.Functions)
            EliminateInFunction(function);
        return module;
    }

    private static void EliminateInFunction(IRFunction function)
    {
        foreach (var block in function.Blocks)
            EliminateInBlock(block);
    }

    private static void EliminateInBlock(IRBlock block)
    {
        var availableExpressions = new Dictionary<ExpressionKey, IRValue>();

        var toRemove = new List<IRInstruction>();

        for (var i = 0; i < block.Instructions.Count; i++)
        {
            var inst = block.Instructions[i];

            if (inst is IRPhiNode)
                continue;
            if (inst.HasSideEffects)
                continue;
            if (inst.IsTerminator)
                continue;
            if (inst.Result == null)
                continue;
            if (inst.Operands.Count == 0)
                continue;

            if (!IsCommutativeOpcode(inst.OpCode) && inst.Operands.Count < 2)
                continue;

            var key = BuildExpressionKey(inst);

            if (availableExpressions.TryGetValue(key, out var existing))
            {
                ReplaceAllUses(block, inst.Result, existing, i);
                toRemove.Add(inst);
            }
            else
            {
                availableExpressions[key] = inst.Result;
            }
        }

        foreach (var inst in toRemove)
            block.RemoveInstruction(inst);
    }

    private static ExpressionKey BuildExpressionKey(IRInstruction inst)
    {
        var operandNames = inst.Operands
            .Select(o => o.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        return new ExpressionKey(inst.OpCode, operandNames);
    }

    private static bool IsCommutativeOpcode(IROpCode opCode)
    {
        return opCode is IROpCode.Add or IROpCode.Mul;
    }

    private static void ReplaceAllUses(IRBlock block, IRValue oldValue, IRValue newValue, int startIndex)
    {
        for (var i = startIndex + 1; i < block.Instructions.Count; i++)
        {
            var inst = block.Instructions[i];
            if (inst is IRPhiNode phi)
            {
                var newEdges = phi.IncomingEdges
                    .Select(e => e.Value == oldValue ? (newValue, e.Block) : e)
                    .ToList();
                if (newEdges.Any(e => e.Item1 == newValue))
                {
                    var newPhi = new IRPhiNode(phi.Result!, newEdges);
                    block.Instructions[i] = newPhi;
                    newPhi.ParentBlock = block;
                    newPhi.SequenceIndex = i;
                }
            }
            else
            {
                var newOperands = inst.Operands
                    .Select(o => o == oldValue ? newValue : o)
                    .ToList();
                if (!newOperands.SequenceEqual(inst.Operands))
                {
                    var newInst = new IRInstruction(inst.OpCode, inst.Result, newOperands);
                    newInst.ParentBlock = block;
                    newInst.SequenceIndex = i;
                    block.Instructions[i] = newInst;
                    if (newInst.IsTerminator)
                        block.Terminator = newInst;
                }
            }
        }
    }

    private readonly struct ExpressionKey : IEquatable<ExpressionKey>
    {
        public IROpCode OpCode { get; }
        public IReadOnlyList<string> OperandNames { get; }

        public ExpressionKey(IROpCode opCode, IReadOnlyList<string> operandNames)
        {
            OpCode = opCode;
            OperandNames = operandNames;
        }

        public bool Equals(ExpressionKey other)
        {
            if (OpCode != other.OpCode)
                return false;
            if (OperandNames.Count != other.OperandNames.Count)
                return false;
            for (var i = 0; i < OperandNames.Count; i++)
            {
                if (!string.Equals(OperandNames[i], other.OperandNames[i], StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is ExpressionKey other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(OpCode);
            foreach (var name in OperandNames)
                hash.Add(name);
            return hash.ToHashCode();
        }
    }
}
