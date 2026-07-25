namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class IRPhiNode : IRInstruction
{
    public IReadOnlyList<(IRValue Value, IRBlock Block)> IncomingEdges { get; }

    public IRPhiNode(IRValue result, IReadOnlyList<(IRValue Value, IRBlock Block)> incomingEdges)
        : base(IROpCode.Phi, result, incomingEdges.Select(e => e.Value).ToArray())
    {
        IncomingEdges = incomingEdges;
    }

    public override bool HasSideEffects => false;
    public override bool IsTerminator => false;

    public IRValue GetValueForBlock(IRBlock block)
    {
        foreach (var (value, b) in IncomingEdges)
        {
            if (b == block)
                return value;
        }
        throw new InvalidOperationException($"No incoming value for block {block.Label}");
    }

    public IRPhiNode ReplaceOperand(IRValue oldVal, IRValue newVal)
    {
        var newEdges = IncomingEdges
            .Select(e => e.Value == oldVal ? (newVal, e.Block) : e)
            .ToArray();
        return new IRPhiNode(Result!, newEdges);
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        if (Result != null)
            sb.Append($"{Result} = ");
        sb.Append("phi(");
        for (var i = 0; i < IncomingEdges.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append($"[{IncomingEdges[i].Value} <- {IncomingEdges[i].Block.Label}]");
        }
        sb.Append(')');
        return sb.ToString();
    }
}
