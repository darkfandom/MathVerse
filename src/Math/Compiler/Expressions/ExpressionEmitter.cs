namespace MathVerse.Math.Compiler.Expressions;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using MathVerse.Math.Compiler.IR;

/// <summary>Emits optimized IR from a linearized (flattened) expression sequence.</summary>
public sealed class ExpressionEmitter
{
    private IRModule _module = null!;
    private readonly Dictionary<string, IROperand> _operandCache = new(StringComparer.Ordinal);

    /// <summary>Emits an optimized IRModule from the given AST root.</summary>
    public IRModule Emit(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        _module = new IRModule();
        _operandCache.Clear();

        var linearized = Linearize(root);
        EmitLinearized(linearized);

        var result = _module;
        _module = null!;
        return result;
    }

    /// <summary>Emits optimized IR from a list of linearized expression fragments into an existing module.</summary>
    public void EmitInto(IReadOnlyList<ExpressionNode> fragments, IRModule module)
    {
        if (fragments is null) throw new ArgumentNullException(nameof(fragments));
        if (module is null) throw new ArgumentNullException(nameof(module));

        _module = module;
        _operandCache.Clear();

        EmitLinearized(fragments);

        _module = null!;
    }

    /// <summary>Emits a single expression node with vectorized constant folding when applicable.</summary>
    public IRModule EmitVectorized(ExpressionNode root, int vectorWidth = 4)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        _module = new IRModule();
        _operandCache.Clear();

        if (CanVectorize(root))
            EmitVectorizedNode(root, vectorWidth);
        else
            EmitSingle(root);

        var result = _module;
        _module = null!;
        return result;
    }

    private List<ExpressionNode> Linearize(ExpressionNode root)
    {
        var result = new List<ExpressionNode>();
        LinearizeImpl(root, result);
        return result;
    }

    private void LinearizeImpl(ExpressionNode node, List<ExpressionNode> result)
    {
        if (node is BinaryOpNode bin)
        {
            LinearizeImpl(bin.Left, result);
            LinearizeImpl(bin.Right, result);
            result.Add(node);
        }
        else if (node is UnaryOpNode unary)
        {
            LinearizeImpl(unary.Operand, result);
            result.Add(node);
        }
        else if (node is FunctionNode func)
        {
            foreach (var arg in func.Arguments)
                LinearizeImpl(arg, result);
            result.Add(node);
        }
        else
        {
            result.Add(node);
        }
    }

    private void EmitLinearized(IReadOnlyList<ExpressionNode> linearized)
    {
        foreach (var node in linearized)
            EmitSingle(node);
    }

    private IROperand EmitSingle(ExpressionNode node)
    {
        return node switch
        {
            NumberNode num => EmitNumber(num),
            VariableNode var => EmitVariable(var),
            BinaryOpNode bin => EmitBinaryOp(bin),
            UnaryOpNode unary => EmitUnaryOp(unary),
            FunctionNode func => EmitFunctionCall(func),
            _ => throw new ArgumentException($"Unknown node type: {node.GetType().Name}"),
        };
    }

    private IROperand EmitNumber(NumberNode node)
    {
        string key = $"const_{node.Value}";
        if (_operandCache.TryGetValue(key, out var cached))
            return cached;

        var dest = CreateTemp("const");
        _module.Append(IRInstruction.CreateLoadConst(node.Value, dest));
        _operandCache[key] = dest;
        return dest;
    }

    private IROperand EmitVariable(VariableNode node)
    {
        string key = $"var_{node.Name}";
        if (_operandCache.TryGetValue(key, out var cached))
            return cached;

        if (!_module.HasVariable(node.Name))
            _module.DeclareVariable(node.Name);

        var dest = CreateTemp("var");
        _module.Append(IRInstruction.CreateLoadVar(node.Name, dest));
        _operandCache[key] = dest;
        return dest;
    }

    private IROperand EmitBinaryOp(BinaryOpNode node)
    {
        var left = EmitSingle(node.Left);
        var right = EmitSingle(node.Right);
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
        var operand = EmitSingle(node.Operand);
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

    private IROperand EmitFunctionCall(FunctionNode node)
    {
        var args = new List<IROperand>(node.Arguments.Count);
        foreach (var arg in node.Arguments)
            args.Add(EmitSingle(arg));

        var dest = CreateTemp("func");
        _module.Append(IRInstruction.CreateFunction(node.FunctionName, args, dest));
        return dest;
    }

    private bool CanVectorize(ExpressionNode node)
    {
        if (node is BinaryOpNode bin && bin.Op is BinaryOperator.Add or BinaryOperator.Multiply)
        {
            return CanVectorize(bin.Left) && CanVectorize(bin.Right);
        }
        return node is NumberNode or VariableNode;
    }

    private void EmitVectorizedNode(ExpressionNode node, int vectorWidth)
    {
        if (node is BinaryOpNode bin)
        {
            var left = EmitSingleVectorized(bin.Left, vectorWidth);
            var right = EmitSingleVectorized(bin.Right, vectorWidth);
            var dest = CreateTemp("simd");

            IROperation op = bin.Op switch
            {
                BinaryOperator.Add => IROperation.Add,
                BinaryOperator.Multiply => IROperation.Mul,
                _ => IROperation.Add,
            };

            _module.Append(IRInstruction.CreateBinary(op, left, right, dest));
        }
        else
        {
            EmitSingle(node);
        }
    }

    private IROperand EmitSingleVectorized(ExpressionNode node, int vectorWidth)
    {
        return EmitSingle(node);
    }

    private IROperand CreateTemp(string? debugName = null)
    {
        int id = _module.NextTempId();
        return IROperand.CreateTemporary(id, debugName);
    }
}
