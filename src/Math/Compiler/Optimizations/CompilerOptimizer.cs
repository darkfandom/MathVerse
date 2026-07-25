namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.Configuration;
using MathVerse.Math.Compiler.IR;

public interface ICompilationPass
{
    string Name { get; }
    IRModule Run(IRModule module);
}

public sealed class ConstantFoldingPass : ICompilationPass
{
    public string Name => "ConstantFolding";

    public IRModule Run(IRModule module)
    {
        foreach (var func in module.Functions)
        {
            foreach (var block in func.Blocks)
            {
                var toRemove = new List<IRInstruction>();
                var replacements = new Dictionary<IRValue, IRValue>();

                foreach (var inst in block.Instructions)
                {
                    if (inst.Result == null) continue;
                    if (inst.Operands.Count < 2) continue;

                    var left = Resolve(inst.Operands[0], replacements);
                    var right = Resolve(inst.Operands[1], replacements);

                    if (left.IsConstant && right.IsConstant && left.ConstantValue.HasValue && right.ConstantValue.HasValue)
                    {
                        var result = ComputeConstant(inst.OpCode, left.ConstantValue.Value, right.ConstantValue.Value);
                        if (result.HasValue)
                        {
                            replacements[inst.Result] = IRValue.CreateConstant(result.Value, inst.Result.Type);
                            toRemove.Add(inst);
                        }
                    }
                }

                foreach (var inst in toRemove)
                    block.RemoveInstruction(inst);

                ApplyReplacements(block, replacements);
            }
        }

        return module;
    }

    private static IRValue Resolve(IRValue value, Dictionary<IRValue, IRValue> replacements)
        => replacements.TryGetValue(value, out var replacement) ? replacement : value;

    private static double? ComputeConstant(IROpCode op, double left, double right)
    {
        return op switch
        {
            IROpCode.Add => left + right,
            IROpCode.Sub => left - right,
            IROpCode.Mul => left * right,
            IROpCode.Div when right != 0 => left / right,
            IROpCode.Mod when right != 0 => left % right,
            IROpCode.Pow => Math.Pow(left, right),
            _ => null
        };
    }

    private static void ApplyReplacements(IRBlock block, Dictionary<IRValue, IRValue> replacements)
    {
        foreach (var inst in block.Instructions)
        {
            for (var i = 0; i < inst.Operands.Count; i++)
            {
                if (replacements.TryGetValue(inst.Operands[i], out var replacement))
                    inst.MutableOperands[i] = replacement;
            }
        }
    }
}

public sealed class DeadCodeEliminationPass : ICompilationPass
{
    public string Name => "DeadCodeElimination";

    public IRModule Run(IRModule module)
    {
        foreach (var func in module.Functions)
        {
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var block in func.Blocks.ToList())
                {
                    var definedValues = new HashSet<IRValue>(func.GetDefinedValues());
                    var usedValues = new HashSet<IRValue>(func.GetUsedValues());

                    foreach (var inst in block.Instructions.ToList())
                    {
                        if (inst.Result == null) continue;
                        if (inst.OpCode == IROpCode.Phi) continue;
                        if (inst.HasSideEffects) continue;
                        if (!usedValues.Contains(inst.Result))
                        {
                            block.RemoveInstruction(inst);
                            changed = true;
                        }
                    }
                }
            }
        }

        return module;
    }
}

public sealed class CommonSubexpressionEliminationPass : ICompilationPass
{
    public string Name => "CommonSubexpressionElimination";

    public IRModule Run(IRModule module)
    {
        foreach (var func in module.Functions)
        {
            foreach (var block in func.Blocks)
            {
                var exprMap = new Dictionary<(IROpCode, string, string), IRValue>();

                foreach (var inst in block.Instructions.ToList())
                {
                    if (inst.Result == null) continue;
                    if (inst.Operands.Count < 2) continue;

                    var key = (inst.OpCode, inst.Operands[0].Name, inst.Operands[1].Name);
                    if (exprMap.TryGetValue(key, out var existing))
                    {
                        block.RemoveInstruction(inst);
                    }
                    else
                    {
                        exprMap[key] = inst.Result;
                    }
                }
            }
        }

        return module;
    }
}

public sealed class AlgebraicSimplificationPass : ICompilationPass
{
    public string Name => "AlgebraicSimplification";

    public IRModule Run(IRModule module)
    {
        foreach (var func in module.Functions)
        {
            foreach (var block in func.Blocks)
            {
                var replacements = new Dictionary<IRValue, IRValue>();
                var toRemove = new List<IRInstruction>();

                foreach (var inst in block.Instructions)
                {
                    if (inst.Result == null) continue;

                    if (inst.OpCode == IROpCode.Mul && inst.Operands.Count >= 2)
                    {
                        if (inst.Operands[0].IsConstant && inst.Operands[0].ConstantValue == 1.0)
                            replacements[inst.Result] = inst.Operands[1];
                        else if (inst.Operands[1].IsConstant && inst.Operands[1].ConstantValue == 1.0)
                            replacements[inst.Result] = inst.Operands[0];
                        else if ((inst.Operands[0].IsConstant && inst.Operands[0].ConstantValue == 0.0) ||
                                 (inst.Operands[1].IsConstant && inst.Operands[1].ConstantValue == 0.0))
                            replacements[inst.Result] = IRValue.CreateConstant(0.0, inst.Result.Type);
                    }
                    else if (inst.OpCode == IROpCode.Add && inst.Operands.Count >= 2)
                    {
                        if (inst.Operands[0].IsConstant && inst.Operands[0].ConstantValue == 0.0)
                            replacements[inst.Result] = inst.Operands[1];
                        else if (inst.Operands[1].IsConstant && inst.Operands[1].ConstantValue == 0.0)
                            replacements[inst.Result] = inst.Operands[0];
                    }
                    else if (inst.OpCode == IROpCode.Sub && inst.Operands.Count >= 2)
                    {
                        if (inst.Operands[0].Name == inst.Operands[1].Name)
                            replacements[inst.Result] = IRValue.CreateConstant(0.0, inst.Result.Type);
                    }
                }

                foreach (var inst in toRemove)
                    block.RemoveInstruction(inst);

                ApplyReplacements(block, replacements);
            }
        }

        return module;
    }

    private static void ApplyReplacements(IRBlock block, Dictionary<IRValue, IRValue> replacements)
    {
        foreach (var inst in block.Instructions)
        {
            for (var i = 0; i < inst.Operands.Count; i++)
            {
                if (replacements.TryGetValue(inst.Operands[i], out var replacement))
                    inst.MutableOperands[i] = replacement;
            }
        }
    }
}

public sealed class CompilerOptimizer
{
    private readonly List<ICompilationPass> _passes = new();

    public void AddPass(ICompilationPass pass) => _passes.Add(pass);

    public IRModule Optimize(IRModule module, OptimizationLevel level)
    {
        if (level == OptimizationLevel.None)
            return module;

        if (level == OptimizationLevel.Basic)
        {
            foreach (var pass in _passes.Where(p =>
                p is ConstantFoldingPass or DeadCodeEliminationPass))
                module = pass.Run(module);
        }
        else
        {
            foreach (var pass in _passes)
                module = pass.Run(module);
        }

        return module;
    }

    public static CompilerOptimizer CreateDefault()
    {
        var optimizer = new CompilerOptimizer();
        optimizer.AddPass(new ConstantFoldingPass());
        optimizer.AddPass(new DeadCodeEliminationPass());
        optimizer.AddPass(new CommonSubexpressionEliminationPass());
        optimizer.AddPass(new AlgebraicSimplificationPass());
        return optimizer;
    }
}
