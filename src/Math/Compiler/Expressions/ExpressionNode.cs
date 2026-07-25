namespace MathVerse.Math.Compiler.Expressions;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>Enumerates the kinds of expression AST nodes.</summary>
public enum ExpressionNodeType
{
    /// <summary>A numeric literal.</summary>
    Number,

    /// <summary>A variable reference.</summary>
    Variable,

    /// <summary>A binary operation.</summary>
    BinaryOp,

    /// <summary>A unary operation.</summary>
    UnaryOp,

    /// <summary>A function call.</summary>
    Function,
}

/// <summary>Enumerates binary operators in mathematical expressions.</summary>
public enum BinaryOperator
{
    /// <summary>Addition (+).</summary>
    Add,

    /// <summary>Subtraction (-).</summary>
    Subtract,

    /// <summary>Multiplication (*).</summary>
    Multiply,

    /// <summary>Division (/).</summary>
    Divide,

    /// <summary>Exponentiation (^).</summary>
    Power,
}

/// <summary>Enumerates unary operators in mathematical expressions.</summary>
public enum UnaryOperator
{
    /// <summary>Negation (-x).</summary>
    Negate,

    /// <summary>Positive (+x).</summary>
    Positive,
}

/// <summary>Abstract base class for all AST nodes in a mathematical expression.</summary>
public abstract class ExpressionNode
{
    /// <summary>The kind of this node.</summary>
    public abstract ExpressionNodeType NodeType { get; }

    /// <summary>Computes a hash code for this node subtree.</summary>
    public abstract override int GetHashCode();

    /// <summary>Display representation.</summary>
    public abstract override string ToString();

    /// <summary>Checks structural equality with another node.</summary>
    public abstract bool StructuralEquals(ExpressionNode? other);
}

/// <summary>A numeric literal node.</summary>
public sealed class NumberNode : ExpressionNode
{
    /// <summary>The numeric value.</summary>
    public double Value { get; }

    /// <summary>Creates a new number node.</summary>
    public NumberNode(double value) => Value = value;

    /// <inheritdoc />
    public override ExpressionNodeType NodeType => ExpressionNodeType.Number;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(NodeType, Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString("G");

    /// <inheritdoc />
    public override bool StructuralEquals(ExpressionNode? other) =>
        other is NumberNode n && n.Value.Equals(Value);
}

/// <summary>A variable reference node.</summary>
public sealed class VariableNode : ExpressionNode
{
    /// <summary>The variable name.</summary>
    public string Name { get; }

    /// <summary>Creates a new variable node.</summary>
    public VariableNode(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

    /// <inheritdoc />
    public override ExpressionNodeType NodeType => ExpressionNodeType.Variable;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(NodeType, Name);

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    public override bool StructuralEquals(ExpressionNode? other) =>
        other is VariableNode v && string.Equals(v.Name, Name, StringComparison.Ordinal);
}

/// <summary>A binary operation node (left op right).</summary>
public sealed class BinaryOpNode : ExpressionNode
{
    /// <summary>The left operand.</summary>
    public ExpressionNode Left { get; }

    /// <summary>The binary operator.</summary>
    public BinaryOperator Op { get; }

    /// <summary>The right operand.</summary>
    public ExpressionNode Right { get; }

    /// <summary>Creates a new binary operation node.</summary>
    public BinaryOpNode(ExpressionNode left, BinaryOperator op, ExpressionNode right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Op = op;
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <inheritdoc />
    public override ExpressionNodeType NodeType => ExpressionNodeType.BinaryOp;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(NodeType, Op, Left.GetHashCode(), Right.GetHashCode());

    /// <inheritdoc />
    public override string ToString()
    {
        string opStr = Op switch
        {
            BinaryOperator.Add => "+",
            BinaryOperator.Subtract => "-",
            BinaryOperator.Multiply => "*",
            BinaryOperator.Divide => "/",
            BinaryOperator.Power => "^",
            _ => "?",
        };
        return $"({Left} {opStr} {Right})";
    }

    /// <inheritdoc />
    public override bool StructuralEquals(ExpressionNode? other) =>
        other is BinaryOpNode b &&
        b.Op == Op &&
        Left.StructuralEquals(b.Left) &&
        Right.StructuralEquals(b.Right);
}

/// <summary>A unary operation node (op operand).</summary>
public sealed class UnaryOpNode : ExpressionNode
{
    /// <summary>The unary operator.</summary>
    public UnaryOperator Op { get; }

    /// <summary>The operand.</summary>
    public ExpressionNode Operand { get; }

    /// <summary>Creates a new unary operation node.</summary>
    public UnaryOpNode(UnaryOperator op, ExpressionNode operand)
    {
        Op = op;
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
    }

    /// <inheritdoc />
    public override ExpressionNodeType NodeType => ExpressionNodeType.UnaryOp;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(NodeType, Op, Operand.GetHashCode());

    /// <inheritdoc />
    public override string ToString() =>
        Op == UnaryOperator.Negate ? $"(-{Operand})" : $"(+{Operand})";

    /// <inheritdoc />
    public override bool StructuralEquals(ExpressionNode? other) =>
        other is UnaryOpNode u &&
        u.Op == Op &&
        Operand.StructuralEquals(u.Operand);
}

/// <summary>A function call node with a name and arguments.</summary>
public sealed class FunctionNode : ExpressionNode
{
    /// <summary>The function name.</summary>
    public string FunctionName { get; }

    /// <summary>The argument list.</summary>
    public IReadOnlyList<ExpressionNode> Arguments { get; }

    /// <summary>Creates a new function call node.</summary>
    public FunctionNode(string functionName, IReadOnlyList<ExpressionNode> arguments)
    {
        FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    /// <inheritdoc />
    public override ExpressionNodeType NodeType => ExpressionNodeType.Function;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(NodeType);
        hash.Add(FunctionName);
        foreach (var arg in Arguments)
            hash.Add(arg.GetHashCode());
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(FunctionName).Append('(');
        for (int i = 0; i < Arguments.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(Arguments[i]);
        }
        sb.Append(')');
        return sb.ToString();
    }

    /// <inheritdoc />
    public override bool StructuralEquals(ExpressionNode? other)
    {
        if (other is not FunctionNode f) return false;
        if (!string.Equals(FunctionName, f.FunctionName, StringComparison.Ordinal)) return false;
        if (Arguments.Count != f.Arguments.Count) return false;
        for (int i = 0; i < Arguments.Count; i++)
        {
            if (!Arguments[i].StructuralEquals(f.Arguments[i])) return false;
        }
        return true;
    }
}
