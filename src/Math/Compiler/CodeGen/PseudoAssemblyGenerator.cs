namespace MathVerse.Math.Compiler.CodeGen;

using System;
using System.Text;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Generates human-readable pseudo-assembly from IR. Produces instructions in the format
/// <c>  %1 = add %0, 2.0</c>.
/// </summary>
public sealed class PseudoAssemblyGenerator : CodeGenerator
{
    /// <inheritdoc/>
    public override string Generate(IRModule module)
    {
        var sb = new StringBuilder(2048);
        sb.AppendLine($"# Module: {module.Name}");
        sb.AppendLine($"# Functions: {module.Functions.Count}");
        sb.AppendLine();

        for (var i = 0; i < module.Functions.Count; i++)
        {
            if (i > 0) sb.AppendLine();
            sb.Append(GenerateFunction(module.Functions[i]));
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override string GenerateFunction(IRFunction function)
    {
        var sb = new StringBuilder(1024);

        sb.Append($"func @{function.Name}(");
        for (var i = 0; i < function.Parameters.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append($"%{IRTypeHelper.ToDisplayName(function.Parameters[i].Type)} {function.Parameters[i].Name}");
        }
        sb.AppendLine($") -> {IRTypeHelper.ToDisplayName(function.ReturnType)} {{");
        sb.AppendLine();

        foreach (var block in function.Blocks)
        {
            if (function.Blocks.Count > 1)
            {
                sb.AppendLine($"  {block.Label}:");
            }

            foreach (var inst in block.Instructions)
            {
                EmitInstruction(sb, inst);
            }

            if (block.Terminator != null && !block.Instructions.Contains(block.Terminator))
            {
                EmitInstruction(sb, block.Terminator);
            }

            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private void EmitInstruction(StringBuilder sb, IRInstruction inst)
    {
        sb.Append("    ");

        if (inst is IRPhiNode phi)
        {
            EmitPhi(sb, phi);
            return;
        }

        var result = inst.Result != null ? $"%{inst.Result.Name}" : "_";

        switch (inst.OpCode)
        {
            case IROpCode.Add:
            case IROpCode.Sub:
            case IROpCode.Mul:
            case IROpCode.Div:
            case IROpCode.Mod:
                EmitBinary(sb, inst, result);
                break;
            case IROpCode.Neg:
                sb.AppendLine($"{result} = neg %{inst.Operands[0].Name}");
                break;
            case IROpCode.Abs:
                sb.AppendLine($"{result} = abs %{inst.Operands[0].Name}");
                break;
            case IROpCode.Fma:
                sb.AppendLine($"{result} = fma %{inst.Operands[0].Name}, %{inst.Operands[1].Name}, %{inst.Operands[2].Name}");
                break;
            case IROpCode.Sqrt:
                sb.AppendLine($"{result} = sqrt %{inst.Operands[0].Name}");
                break;
            case IROpCode.Sin:
                sb.AppendLine($"{result} = sin %{inst.Operands[0].Name}");
                break;
            case IROpCode.Cos:
                sb.AppendLine($"{result} = cos %{inst.Operands[0].Name}");
                break;
            case IROpCode.Tan:
                sb.AppendLine($"{result} = tan %{inst.Operands[0].Name}");
                break;
            case IROpCode.Log:
                sb.AppendLine($"{result} = log %{inst.Operands[0].Name}");
                break;
            case IROpCode.Exp:
                sb.AppendLine($"{result} = exp %{inst.Operands[0].Name}");
                break;
            case IROpCode.Pow:
                sb.AppendLine($"{result} = pow %{inst.Operands[0].Name}, %{inst.Operands[1].Name}");
                break;
            case IROpCode.Dot:
                sb.AppendLine($"{result} = dot %{inst.Operands[0].Name}, %{inst.Operands[1].Name}");
                break;
            case IROpCode.Load:
                sb.AppendLine($"{result} = load %{inst.Operands[0].Name}");
                break;
            case IROpCode.Store:
                sb.AppendLine($"store %{inst.Operands[0].Name}, %{inst.Operands[1].Name}");
                break;
            case IROpCode.Alloc:
                sb.AppendLine($"{result} = alloc %{inst.Operands[0].Name}");
                break;
            case IROpCode.Branch:
                var branchTarget = inst.Operands.Count > 0 ? inst.Operands[^1].Name : "?";
                sb.AppendLine($"br %{branchTarget}");
                break;
            case IROpCode.CondBranch:
                EmitCondBranch(sb, inst);
                break;
            case IROpCode.Return:
                if (inst.Operands.Count > 0)
                    sb.AppendLine($"ret %{inst.Operands[0].Name}");
                else
                    sb.AppendLine("ret");
                break;
            case IROpCode.Call:
                EmitCall(sb, inst, result);
                break;
            case IROpCode.Cast:
                sb.AppendLine($"{result} = cast %{inst.Operands[0].Name} to {IRTypeHelper.ToDisplayName(inst.Result?.Type ?? IRType.Float64)}");
                break;
            case IROpCode.Nop:
                sb.AppendLine("nop");
                break;
            default:
                sb.AppendLine($"{result} = ??? {inst.OpCode}");
                break;
        }
    }

    private static void EmitBinary(StringBuilder sb, IRInstruction inst, string result)
    {
        var op = inst.OpCode switch
        {
            IROpCode.Add => "add",
            IROpCode.Sub => "sub",
            IROpCode.Mul => "mul",
            IROpCode.Div => "div",
            IROpCode.Mod => "mod",
            _ => "???"
        };

        var left = FormatOperand(inst.Operands[0]);
        var right = FormatOperand(inst.Operands[1]);
        sb.AppendLine($"{result} = {op} {left}, {right}");
    }

    private static void EmitCondBranch(StringBuilder sb, IRInstruction inst)
    {
        if (inst.Operands.Count >= 2)
        {
            var cond = FormatOperand(inst.Operands[0]);
            var trueTarget = inst.Operands[1].Name;
            if (inst.Operands.Count >= 3)
            {
                var falseTarget = inst.Operands[2].Name;
                sb.AppendLine($"br_if {cond}, %{trueTarget}, %{falseTarget}");
            }
            else
            {
                sb.AppendLine($"br_if {cond}, %{trueTarget}");
            }
        }
        else
        {
            sb.AppendLine("br_if ???");
        }
    }

    private static void EmitCall(StringBuilder sb, IRInstruction inst, string result)
    {
        var callee = inst.Operands.Count > 0 ? inst.Operands[0].Name : "???";
        sb.Append($"{result} = call @{callee}(");
        for (var i = 1; i < inst.Operands.Count; i++)
        {
            if (i > 1) sb.Append(", ");
            sb.Append(FormatOperand(inst.Operands[i]));
        }
        sb.AppendLine(")");
    }

    private static void EmitPhi(StringBuilder sb, IRPhiNode phi)
    {
        var result = phi.Result != null ? $"%{phi.Result.Name}" : "_";
        sb.Append($"{result} = phi ");
        for (var i = 0; i < phi.IncomingEdges.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append($"[%{phi.IncomingEdges[i].Value.Name} <- %{phi.IncomingEdges[i].Block.Label}]");
        }
        sb.AppendLine();
    }

    private static string FormatOperand(IRValue value)
    {
        if (value.IsConstant && value.ConstantValue.HasValue)
            return value.ConstantValue.Value.ToString("G");
        return $"%{value.Name}";
    }
}
