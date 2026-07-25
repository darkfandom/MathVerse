namespace MathVerse.Math.Compiler.Differentiation;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>Computes gradients of compiled expressions by traversing IR instructions and applying differentiation rules.</summary>
public sealed class GradientEngine
{
    private readonly ForwardModeAD _forwardAD = new();

    /// <summary>Computes the gradient of the expression in the IRModule with respect to a specified variable.</summary>
    public IRModule Differentiate(IRModule module, string variableName)
    {
        if (module is null) throw new ArgumentNullException(nameof(module));
        if (string.IsNullOrEmpty(variableName)) throw new ArgumentException("Variable name required.", nameof(variableName));

        var gradientModule = new IRModule();
        var derivatives = new Dictionary<string, IROperand>(StringComparer.Ordinal);

        foreach (var instr in module.Instructions)
        {
            switch (instr.Operation)
            {
                case IROperation.LoadConst:
                    {
                        var zeroTemp = gradientModule.CreateTemp("grad_const");
                        gradientModule.Append(IRInstruction.CreateLoadConst(0.0, zeroTemp));
                        derivatives[instr.Destination!.Name] = zeroTemp;
                    }
                    break;

                case IROperation.LoadVar:
                    if (instr.Left is not null && instr.Left.Kind == IROperandKind.Variable)
                    {
                        var gradTemp = gradientModule.CreateTemp("grad_var");
                        if (string.Equals(instr.Left.Name, variableName, StringComparison.Ordinal))
                            gradientModule.Append(IRInstruction.CreateLoadConst(1.0, gradTemp));
                        else
                            gradientModule.Append(IRInstruction.CreateLoadConst(0.0, gradTemp));
                        derivatives[instr.Destination!.Name] = gradTemp;
                    }
                    break;

                case IROperation.Add:
                    EmitBinaryGrad(gradientModule, instr, derivatives,
                        (l, r, dest) => gradientModule.Append(IRInstruction.CreateBinary(IROperation.Add, l, r, dest)));
                    break;

                case IROperation.Sub:
                    EmitBinaryGrad(gradientModule, instr, derivatives,
                        (l, r, dest) => gradientModule.Append(IRInstruction.CreateBinary(IROperation.Sub, l, r, dest)));
                    break;

                case IROperation.Mul:
                    {
                        if (!derivatives.TryGetValue(instr.Left!.Name, out var dLeft)) dLeft = MakeZero(gradientModule);
                        if (!derivatives.TryGetValue(instr.Right!.Name, out var dRight)) dRight = MakeZero(gradientModule);

                        var mul1 = gradientModule.CreateTemp("dmul1");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, dLeft, instr.Right, mul1));

                        var mul2 = gradientModule.CreateTemp("dmul2");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, instr.Left, dRight, mul2));

                        var dest = gradientModule.CreateTemp("dadd");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Add, mul1, mul2, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Div:
                    {
                        if (!derivatives.TryGetValue(instr.Left!.Name, out var dLeft)) dLeft = MakeZero(gradientModule);
                        if (!derivatives.TryGetValue(instr.Right!.Name, out var dRight)) dRight = MakeZero(gradientModule);

                        var numTerm1 = gradientModule.CreateTemp("ddiv1");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, dLeft, instr.Right, numTerm1));

                        var numTerm2 = gradientModule.CreateTemp("ddiv2");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, instr.Left, dRight, numTerm2));

                        var numerator = gradientModule.CreateTemp("ddiv_sub");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Sub, numTerm1, numTerm2, numerator));

                        var denom = gradientModule.CreateTemp("ddiv_sq");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, instr.Right, instr.Right, denom));

                        var dest = gradientModule.CreateTemp("ddiv");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Div, numerator, denom, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Pow:
                    {
                        if (!derivatives.TryGetValue(instr.Left!.Name, out var dLeft)) dLeft = MakeZero(gradientModule);

                        var one = gradientModule.CreateTemp("one");
                        gradientModule.Append(IRInstruction.CreateLoadConst(1.0, one));

                        var expMinusOne = gradientModule.CreateTemp("exp_m1");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Sub, instr.Right!, one, expMinusOne));

                        var coeff = gradientModule.CreateTemp("pow_coeff");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, instr.Right!, dLeft, coeff));

                        var powPart = gradientModule.CreateTemp("pow_part");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Pow, instr.Left, expMinusOne, powPart));

                        var dest = gradientModule.CreateTemp("dpow");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, coeff, powPart, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Neg:
                    {
                        if (!derivatives.TryGetValue(instr.Operand!.Name, out var dOperand)) dOperand = MakeZero(gradientModule);
                        var dest = gradientModule.CreateTemp("dneg");
                        gradientModule.Append(IRInstruction.CreateUnary(IROperation.Neg, dOperand, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Sin:
                    {
                        if (!derivatives.TryGetValue(instr.Operand!.Name, out var dOperand)) dOperand = MakeZero(gradientModule);
                        var cosPart = gradientModule.CreateTemp("dcos");
                        gradientModule.Append(IRInstruction.CreateFunction("cos", [instr.Operand], cosPart));
                        var dest = gradientModule.CreateTemp("dsin");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, dOperand, cosPart, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Cos:
                    {
                        if (!derivatives.TryGetValue(instr.Operand!.Name, out var dOperand)) dOperand = MakeZero(gradientModule);
                        var sinPart = gradientModule.CreateTemp("dsin_val");
                        gradientModule.Append(IRInstruction.CreateFunction("sin", [instr.Operand], sinPart));
                        var negSin = gradientModule.CreateTemp("dneg_sin");
                        gradientModule.Append(IRInstruction.CreateUnary(IROperation.Neg, sinPart, negSin));
                        var dest = gradientModule.CreateTemp("dcos");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, dOperand, negSin, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Exp:
                    {
                        if (!derivatives.TryGetValue(instr.Operand!.Name, out var dOperand)) dOperand = MakeZero(gradientModule);
                        var dest = gradientModule.CreateTemp("dexp");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, dOperand, instr.Destination!, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Ln:
                    {
                        if (!derivatives.TryGetValue(instr.Operand!.Name, out var dOperand)) dOperand = MakeZero(gradientModule);
                        var dest = gradientModule.CreateTemp("dln");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Div, dOperand, instr.Operand, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Sqrt:
                    {
                        if (!derivatives.TryGetValue(instr.Operand!.Name, out var dOperand)) dOperand = MakeZero(gradientModule);
                        var two = gradientModule.CreateTemp("two");
                        gradientModule.Append(IRInstruction.CreateLoadConst(2.0, two));
                        var mul = gradientModule.CreateTemp("dsqrt_mul");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Mul, two, instr.Destination!, mul));
                        var dest = gradientModule.CreateTemp("dsqrt");
                        gradientModule.Append(IRInstruction.CreateBinary(IROperation.Div, dOperand, mul, dest));
                        derivatives[instr.Destination!.Name] = dest;
                    }
                    break;

                case IROperation.Store:
                    {
                        if (instr.Left is not null && derivatives.TryGetValue(instr.Left.Name, out var gradDest))
                        {
                            var gradResult = gradientModule.CreateTemp("grad_result");
                            gradientModule.Append(IRInstruction.CreateStore(gradDest, gradResult));
                        }
                    }
                    break;
            }
        }

        return gradientModule;
    }

    private static IROperand MakeZero(IRModule module)
    {
        var temp = module.CreateTemp("zero");
        module.Append(IRInstruction.CreateLoadConst(0.0, temp));
        return temp;
    }

    private void EmitBinaryGrad(
        IRModule gradientModule,
        IRInstruction instr,
        Dictionary<string, IROperand> derivatives,
        Action<IROperand, IROperand, IROperand> emitOp)
    {
        if (!derivatives.TryGetValue(instr.Left!.Name, out var dLeft)) dLeft = MakeZero(gradientModule);
        if (!derivatives.TryGetValue(instr.Right!.Name, out var dRight)) dRight = MakeZero(gradientModule);

        var dest = gradientModule.CreateTemp("dbinop");
        emitOp(dLeft, dRight, dest);
        derivatives[instr.Destination!.Name] = dest;
    }
}
