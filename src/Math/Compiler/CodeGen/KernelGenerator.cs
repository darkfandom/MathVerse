namespace MathVerse.Math.Compiler.CodeGen;

using System;
using System.Collections.Generic;
using System.Text;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Generates standalone kernel functions (complete methods with parameters and return types) from an IR module.
/// </summary>
public sealed class KernelGenerator
{
    /// <summary>
    /// Generates a dictionary mapping function names to their complete C# kernel source code.
    /// </summary>
    /// <param name="module">The IR module containing kernel functions.</param>
    /// <returns>A dictionary of function name to generated C# string.</returns>
    public IReadOnlyDictionary<string, string> GenerateKernels(IRModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var results = new Dictionary<string, string>(module.Functions.Count);
        foreach (var func in module.Functions)
        {
            results[func.Name] = GenerateKernel(func);
        }
        return results;
    }

    /// <summary>
    /// Generates a single standalone C# kernel method from an IR function.
    /// </summary>
    /// <param name="function">The IR function to generate a kernel for.</param>
    /// <returns>The complete C# method source code.</returns>
    public string GenerateKernel(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var sb = new StringBuilder(512);
        var csharpReturn = MapType(function.ReturnType);

        sb.Append($"public static {csharpReturn} {SanitizeName(function.Name)}(");
        EmitParameterList(sb, function);
        sb.AppendLine(")");
        sb.AppendLine("{");

        EmitLocalDeclarations(sb, function);
        EmitBlockBody(sb, function);

        sb.Append("}");
        return sb.ToString();
    }

    private static void EmitParameterList(StringBuilder sb, IRFunction function)
    {
        for (var i = 0; i < function.Parameters.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            var p = function.Parameters[i];
            sb.Append($"{MapType(p.Type)} {SanitizeName(p.Name)}");
        }
    }

    private static void EmitLocalDeclarations(StringBuilder sb, IRFunction function)
    {
        var declared = new HashSet<int>();
        foreach (var param in function.Parameters)
            declared.Add(param.Id);

        foreach (var block in function.Blocks)
        {
            foreach (var inst in block.Instructions)
            {
                if (inst.Result != null && declared.Add(inst.Result.Id))
                {
                    sb.AppendLine($"    {MapType(inst.Result.Type)} {SanitizeName(inst.Result.Name)} = default;");
                }
            }
        }
    }

    private static void EmitBlockBody(StringBuilder sb, IRFunction function)
    {
        var singleBlock = function.Blocks.Count <= 1;

        foreach (var block in function.Blocks)
        {
            if (!singleBlock)
            {
                sb.AppendLine();
                sb.AppendLine($"    // Block: {block.Label}");
            }

            foreach (var inst in block.Instructions)
            {
                EmitInstruction(sb, inst, function);
            }

            if (block.Terminator != null && !block.Instructions.Contains(block.Terminator))
            {
                EmitInstruction(sb, block.Terminator, function);
            }
        }
    }

    private static void EmitInstruction(StringBuilder sb, IRInstruction inst, IRFunction function)
    {
        switch (inst.OpCode)
        {
            case IROpCode.Add:
            case IROpCode.Sub:
            case IROpCode.Mul:
            case IROpCode.Div:
            case IROpCode.Mod:
                EmitBinary(sb, inst);
                break;
            case IROpCode.Neg:
                EmitUnary(sb, inst, "-");
                break;
            case IROpCode.Abs:
                EmitIntrinsic(sb, inst, "System.Math.Abs");
                break;
            case IROpCode.Sqrt:
                EmitIntrinsic(sb, inst, "System.Math.Sqrt");
                break;
            case IROpCode.Sin:
                EmitIntrinsic(sb, inst, "System.Math.Sin");
                break;
            case IROpCode.Cos:
                EmitIntrinsic(sb, inst, "System.Math.Cos");
                break;
            case IROpCode.Tan:
                EmitIntrinsic(sb, inst, "System.Math.Tan");
                break;
            case IROpCode.Log:
                EmitIntrinsic(sb, inst, "System.Math.Log");
                break;
            case IROpCode.Exp:
                EmitIntrinsic(sb, inst, "System.Math.Exp");
                break;
            case IROpCode.Pow:
                EmitIntrinsic(sb, inst, "System.Math.Pow");
                break;
            case IROpCode.Fma:
                EmitFma(sb, inst);
                break;
            case IROpCode.Load:
                EmitLoad(sb, inst);
                break;
            case IROpCode.Store:
                EmitStore(sb, inst);
                break;
            case IROpCode.Branch:
                EmitBranch(sb, inst, function);
                break;
            case IROpCode.CondBranch:
                EmitCondBranch(sb, inst, function);
                break;
            case IROpCode.Return:
                EmitReturn(sb, inst);
                break;
            case IROpCode.Call:
                EmitCall(sb, inst);
                break;
            case IROpCode.Cast:
                EmitCast(sb, inst);
                break;
            case IROpCode.Dot:
                EmitIntrinsic(sb, inst, "System.Numerics.Vector.Dot");
                break;
            case IROpCode.Phi:
                EmitPhi(sb, inst);
                break;
            case IROpCode.Nop:
                sb.AppendLine("    // nop");
                break;
            default:
                sb.AppendLine($"    // Unsupported opcode: {inst.OpCode}");
                break;
        }
    }

    private static void EmitBinary(StringBuilder sb, IRInstruction inst)
    {
        var op = inst.OpCode switch
        {
            IROpCode.Add => "+",
            IROpCode.Sub => "-",
            IROpCode.Mul => "*",
            IROpCode.Div => "/",
            IROpCode.Mod => "%",
            _ => "+"
        };

        sb.Append("    ");
        if (inst.Result != null)
            sb.Append($"{SanitizeName(inst.Result.Name)} = ");
        sb.AppendLine($"{FormatOp(inst.Operands[0])} {op} {FormatOp(inst.Operands[1])};");
    }

    private static void EmitUnary(StringBuilder sb, IRInstruction inst, string op)
    {
        sb.Append("    ");
        if (inst.Result != null)
            sb.Append($"{SanitizeName(inst.Result.Name)} = ");
        sb.AppendLine($"{op}{FormatOp(inst.Operands[0])};");
    }

    private static void EmitIntrinsic(StringBuilder sb, IRInstruction inst, string funcName)
    {
        sb.Append("    ");
        if (inst.Result != null)
            sb.Append($"{SanitizeName(inst.Result.Name)} = ");
        sb.Append($"{funcName}(");
        for (var i = 0; i < inst.Operands.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(FormatOp(inst.Operands[i]));
        }
        sb.AppendLine(");");
    }

    private static void EmitFma(StringBuilder sb, IRInstruction inst)
    {
        sb.Append("    ");
        if (inst.Result != null)
            sb.Append($"{SanitizeName(inst.Result.Name)} = ");
        sb.AppendLine($"{FormatOp(inst.Operands[0])} * {FormatOp(inst.Operands[1])} + {FormatOp(inst.Operands[2])};");
    }

    private static void EmitLoad(StringBuilder sb, IRInstruction inst)
    {
        sb.Append("    ");
        if (inst.Result != null)
            sb.Append($"{SanitizeName(inst.Result.Name)} = ");
        sb.AppendLine($"{FormatOp(inst.Operands[0])};");
    }

    private static void EmitStore(StringBuilder sb, IRInstruction inst)
    {
        sb.AppendLine($"    {FormatOp(inst.Operands[0])} = {FormatOp(inst.Operands[1])};");
    }

    private static void EmitBranch(StringBuilder sb, IRInstruction inst, IRFunction function)
    {
        var label = ExtractTargetLabel(inst, inst.Operands.Count - 1, function);
        sb.AppendLine($"    goto {SanitizeName(label)};");
    }

    private static void EmitCondBranch(StringBuilder sb, IRInstruction inst, IRFunction function)
    {
        if (inst.Operands.Count < 2)
        {
            sb.AppendLine("    // malformed cond branch");
            return;
        }

        var cond = FormatOp(inst.Operands[0]);
        var trueLabel = ExtractTargetLabel(inst, 1, function);
        sb.AppendLine($"    if ({cond}) goto {SanitizeName(trueLabel)};");

        if (inst.Operands.Count >= 3)
        {
            var falseLabel = ExtractTargetLabel(inst, 2, function);
            sb.AppendLine($"    goto {SanitizeName(falseLabel)};");
        }
    }

    private static void EmitReturn(StringBuilder sb, IRInstruction inst)
    {
        if (inst.Operands.Count > 0)
            sb.AppendLine($"    return {FormatOp(inst.Operands[0])};");
        else
            sb.AppendLine("    return;");
    }

    private static void EmitCall(StringBuilder sb, IRInstruction inst)
    {
        sb.Append("    ");
        if (inst.Result != null)
            sb.Append($"{SanitizeName(inst.Result.Name)} = ");

        var funcName = inst.Operands.Count > 0 ? FormatOp(inst.Operands[0]) : "Unknown";
        sb.Append($"{funcName}(");
        for (var i = 1; i < inst.Operands.Count; i++)
        {
            if (i > 1) sb.Append(", ");
            sb.Append(FormatOp(inst.Operands[i]));
        }
        sb.AppendLine(");");
    }

    private static void EmitCast(StringBuilder sb, IRInstruction inst)
    {
        var targetType = MapType(inst.Result?.Type ?? IRType.Float64);
        sb.Append("    ");
        if (inst.Result != null)
            sb.Append($"{SanitizeName(inst.Result.Name)} = ");
        sb.AppendLine($"({targetType}){FormatOp(inst.Operands[0])};");
    }

    private static void EmitPhi(StringBuilder sb, IRInstruction inst)
    {
        sb.AppendLine($"    // phi node: {inst.Result?.Name ?? "?"}");
    }

    private static string ExtractTargetLabel(IRInstruction inst, int operandIndex, IRFunction function)
    {
        if (operandIndex < 0 || operandIndex >= inst.Operands.Count)
            return "unknown";

        var operand = inst.Operands[operandIndex];

        foreach (var b in function.Blocks)
        {
            if (b.Label == operand.Name || b.Label.EndsWith("." + operand.Name))
                return b.Label;
        }

        return operand.Name;
    }

    internal static string FormatOp(IRValue value)
    {
        if (value.IsConstant && value.ConstantValue.HasValue)
            return value.ConstantValue.Value.ToString("G");
        return SanitizeName(value.Name);
    }

    internal static string MapType(IRType type)
    {
        return type switch
        {
            IRType.Float32 => "float",
            IRType.Float64 => "double",
            IRType.Int32 => "int",
            IRType.Int64 => "long",
            IRType.Bool => "bool",
            IRType.Void => "void",
            IRType.Pointer => "IntPtr",
            IRType.Vector => "System.Numerics.Vector<double>",
            IRType.Tensor => "double[]",
            _ => "object"
        };
    }

    internal static string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "_";

        var sb = new StringBuilder(name.Length);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsLetterOrDigit(c) || c == '_')
                sb.Append(c);
            else
                sb.Append('_');
        }

        var result = sb.ToString();
        if (char.IsDigit(result[0]))
            result = "_" + result;
        return result;
    }
}
