namespace MathVerse.Math.Compiler.Memory;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Analyzes value lifetimes in IR functions, computing the first-use and last-use instruction
/// index for each value. Used by BufferReuse and MemoryPlanner.
/// </summary>
public sealed class LifetimeAnalyzer
{
    /// <summary>
    /// Analyzes all value lifetimes within a function.
    /// </summary>
    /// <param name="function">The IR function to analyze.</param>
    /// <returns>A dictionary mapping each IR value to its lifetime range.</returns>
    public IReadOnlyDictionary<IRValue, LifetimeRange> Analyze(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var lifetimes = new Dictionary<int, (IRValue Value, int First, int Last)>();
        var instructionIndex = 0;

        // Record parameter lifetimes as spanning the whole function
        for (var p = 0; p < function.Parameters.Count; p++)
        {
            var param = function.Parameters[p];
            lifetimes[param.Id] = (param, 0, int.MaxValue);
        }

        // Analyze each block
        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var block = function.Blocks[b];

            for (var i = 0; i < block.Instructions.Count; i++)
            {
                var inst = block.Instructions[i];

                // Record uses (operands)
                for (var j = 0; j < inst.Operands.Count; j++)
                {
                    var operand = inst.Operands[j];
                    if (operand.IsConstant) continue;

                    if (lifetimes.TryGetValue(operand.Id, out var existing))
                    {
                        lifetimes[operand.Id] = (existing.Value, existing.First, instructionIndex);
                    }
                    else
                    {
                        lifetimes[operand.Id] = (operand, instructionIndex, instructionIndex);
                    }
                }

                // Record definition (result)
                if (inst.Result != null && !inst.Result.IsConstant)
                {
                    if (lifetimes.ContainsKey(inst.Result.Id))
                    {
                        var existing = lifetimes[inst.Result.Id];
                        lifetimes[inst.Result.Id] = (existing.Value, Math.Min(existing.First, instructionIndex), existing.Last);
                    }
                    else
                    {
                        lifetimes[inst.Result.Id] = (inst.Result, instructionIndex, instructionIndex);
                    }
                }

                // Handle phi nodes specially
                if (inst is IRPhiNode phi)
                {
                    for (var j = 0; j < phi.IncomingEdges.Count; j++)
                    {
                        var incoming = phi.IncomingEdges[j].Value;
                        if (incoming.IsConstant) continue;

                        if (lifetimes.TryGetValue(incoming.Id, out var existing))
                        {
                            lifetimes[incoming.Id] = (existing.Value, existing.First, instructionIndex);
                        }
                        else
                        {
                            lifetimes[incoming.Id] = (incoming, instructionIndex, instructionIndex);
                        }
                    }
                }

                instructionIndex++;

                // Terminator is a separate instruction
                if (inst == block.Terminator && i != block.Instructions.Count - 1)
                {
                    // Terminator already counted in Instructions list, skip duplicate
                }
            }

            // Process terminator if it's not already in the Instructions list
            if (block.Terminator != null && !block.Instructions.Contains(block.Terminator))
            {
                var term = block.Terminator;
                for (var j = 0; j < term.Operands.Count; j++)
                {
                    var operand = term.Operands[j];
                    if (operand.IsConstant) continue;

                    if (lifetimes.TryGetValue(operand.Id, out var existing))
                    {
                        lifetimes[operand.Id] = (existing.Value, existing.First, instructionIndex);
                    }
                    else
                    {
                        lifetimes[operand.Id] = (operand, instructionIndex, instructionIndex);
                    }
                }

                instructionIndex++;
            }
        }

        // Build the result dictionary
        var result = new Dictionary<IRValue, LifetimeRange>(lifetimes.Count);
        foreach (var kvp in lifetimes)
        {
            var range = new LifetimeRange(kvp.Value.First, kvp.Value.Last);
            result[kvp.Value.Value] = range;
        }

        return result;
    }

    /// <summary>
    /// Analyzes lifetimes for all functions in a module.
    /// </summary>
    /// <param name="module">The IR module to analyze.</param>
    /// <returns>A dictionary mapping function names to their lifetime analysis results.</returns>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<IRValue, LifetimeRange>> AnalyzeModule(IRModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var results = new Dictionary<string, IReadOnlyDictionary<IRValue, LifetimeRange>>(module.Functions.Count);
        for (var i = 0; i < module.Functions.Count; i++)
        {
            var func = module.Functions[i];
            results[func.Name] = Analyze(func);
        }
        return results;
    }
}
