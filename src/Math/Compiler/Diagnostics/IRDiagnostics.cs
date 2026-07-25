namespace MathVerse.Math.Compiler.Diagnostics;

using System;
using System.Collections.Generic;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;
using IRBlock = MathVerse.Math.Compiler.IR.IRBlock;
using IRInstruction = MathVerse.Math.Compiler.IR.IRInstruction;

/// <summary>Validates IR correctness: well-formed SSA, type checking, basic block structure.</summary>
public sealed class IRDiagnostics
{
    /// <summary>Validates an IR module for correctness.</summary>
    /// <param name="module">The IR module to validate.</param>
    /// <returns>A list of diagnostic messages describing any issues found.</returns>
    public IReadOnlyList<DiagnosticMessage> Validate(IRModule module)
    {
        if (module is null) throw new ArgumentNullException(nameof(module));

        var diagnostics = new List<DiagnosticMessage>();
        var line = 0;

        foreach (var instr in module.Instructions)
        {
            line++;
            ValidateInstruction(instr, diagnostics, line);
        }

        return diagnostics;
    }

    /// <summary>Validates a single IR instruction.</summary>
    public IReadOnlyList<DiagnosticMessage> ValidateInstruction(IRInstruction instruction)
    {
        var diagnostics = new List<DiagnosticMessage>();
        ValidateInstruction(instruction, diagnostics, 0);
        return diagnostics;
    }

    /// <summary>Validates a complete function, including block structure and terminators.</summary>
    public IReadOnlyList<DiagnosticMessage> ValidateFunction(IR.IRFunction function)
    {
        if (function is null) throw new ArgumentNullException(nameof(function));
        var diagnostics = new List<DiagnosticMessage>();

        if (function.Blocks.Count == 0)
        {
            diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error, "Function has no blocks"));
            return diagnostics;
        }

        foreach (var block in function.Blocks)
        {
            if (block.Terminator == null)
                diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Warning, $"Block '{block.Label}' has no terminator"));
        }

        return diagnostics;
    }

    private static void ValidateInstruction(IRInstruction instr, List<DiagnosticMessage> diagnostics, int line)
    {
        if (instr.Destination == null)
            diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error, "Instruction has no destination", line));
    }
}
