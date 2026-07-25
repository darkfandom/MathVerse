namespace MathVerse.Math.Compiler.Expressions;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>Lowers an ExpressionNode AST into an IRModule by traversing the tree and emitting instructions.</summary>
public sealed class ExpressionLowering
{
    private IRModule _module = null!;
    private readonly Dictionary<string, IROperation> _functionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sin"] = IROperation.Sin,
        ["cos"] = IROperation.Cos,
        ["tan"] = IROperation.Tan,
        ["asin"] = IROperation.Asin,
        ["acos"] = IROperation.Acos,
        ["atan"] = IROperation.Atan,
        ["ln"] = IROperation.Ln,
        ["log"] = IROperation.Log,
        ["exp"] = IROperation.Exp,
        ["sqrt"] = IROperation.Sqrt,
        ["abs"] = IROperation.Abs,
        ["ceil"] = IROperation.Ceil,
        ["floor"] = IROperation.Floor,
    };

    /// <summary>Lowers the given AST root into a new IRModule.</summary>
    public IRModule Lower(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        _module = new IRModule();

        var result = EmitNode(root);

        var outputTemp = CreateTemp("result");
        _module.Append(IRInstruction.CreateStore(result, outputTemp));

        var final = _module;
        _module = null!;
        return final;
    }

    /// <summary>Lowers the given AST into an existing IRModule, appending instructions.</summary>
    public void LowerInto(ExpressionNode root, IRModule module)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));
        if (module is null) throw new ArgumentNullException(nameof(module));

        _module = module;
        EmitNode(root);
        _module = null!;
    }

    private IROperand EmitNode(ExpressionNode node)
    {
        return node switch
        {
            NumberNode num => EmitNumber(num),
            VariableNode var => EmitVariable(var),
            BinaryOpNode bin => EmitBinaryOp(bin),
            UnaryOpNode unary => EmitUnaryOp(unary),
            FunctionNode func => EmitFunction(func),
            _ => throw new ArgumentException($"Unknown node type: {node.GetType().Name}"),
        };
    }

    private IROperand EmitNumber(NumberNode node)
    {
        var dest = CreateTemp("const");
        _module.Append(IRInstruction.CreateLoadConst(node.Value, dest));
        return dest;
    }

    private IROperand EmitVariable(VariableNode node)
    {
        if (!_module.HasVariable(node.Name))
            _module.DeclareVariable(node.Name);

        var dest = CreateTemp("var");
        _module.Append(IRInstruction.CreateLoadVar(node.Name, dest));
        return dest;
    }

    private IROperand EmitBinaryOp(BinaryOpNode node)
    {
        var left = EmitNode(node.Left);
        var right = EmitNode(node.Right);
        var dest = CreateTemp("binop");

        IROperation op = node.Op switch
        {
            BinaryOperator.Add => IROperation.Add,
            BinaryOperator.Subtract => IROperation.Sub,
            BinaryOperator.Multiply => IROperation.Mul,
            BinaryOperator.Divide => IROperation.Div,
            BinaryOperator.Power => IROperation.Pow,
            _ => throw new ArgumentException($"Unknown binary operator: {node.Op}"),
        };

        _module.Append(IRInstruction.CreateBinary(op, left, right, dest));
        return dest;
    }

    private IROperand EmitUnaryOp(UnaryOpNode node)
    {
        var operand = EmitNode(node.Operand);
        var dest = CreateTemp("unary");

        IROperation op = node.Op switch
        {
            UnaryOperator.Negate => IROperation.Neg,
            UnaryOperator.Positive => IROperation.Pos,
            _ => throw new ArgumentException($"Unknown unary operator: {node.Op}"),
        };

        _module.Append(IRInstruction.CreateUnary(op, operand, dest));
        return dest;
    }

    private IROperand EmitFunction(FunctionNode node)
    {
        var args = new List<IROperand>(node.Arguments.Count);
        foreach (var arg in node.Arguments)
            args.Add(EmitNode(arg));

        var dest = CreateTemp("func");

        if (_functionMap.TryGetValue(node.FunctionName, out IROperation mathOp))
        {
            if (args.Count == 1)
            {
                var funcDest = CreateTemp("mathfn");
                _module.Append(IRInstruction.CreateFunction(node.FunctionName, args, funcDest));
                return funcDest;
            }
        }

        _module.Append(IRInstruction.CreateFunction(node.FunctionName, args, dest));
        return dest;
    }

    private IROperand CreateTemp(string? debugName = null)
    {
        int id = _module.NextTempId();
        return IROperand.CreateTemporary(id, debugName);
    }
}
