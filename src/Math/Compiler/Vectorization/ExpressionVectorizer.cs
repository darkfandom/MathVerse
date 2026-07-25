namespace MathVerse.Math.Compiler.Vectorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Vectorizes expression trees by grouping compatible scalar operations into
/// SIMD lanes. Analyzes data dependencies within expression trees and maps
/// independent computations to parallel SIMD execution lanes.
/// </summary>
public sealed class ExpressionVectorizer : IVectorizationPass
{
    private readonly int _maxLanes;

    /// <summary>
    /// Initializes the expression vectorizer.
    /// </summary>
    /// <param name="maxLanes">Maximum number of SIMD lanes to use.</param>
    public ExpressionVectorizer(int maxLanes = 8)
    {
        _maxLanes = maxLanes;
    }

    /// <inheritdoc />
    public string Name => "ExpressionVectorizer";

    /// <inheritdoc />
    public IRModule Vectorize(IRModule module)
    {
        foreach (var function in module.Functions)
            VectorizeFunction(function);
        return module;
    }

    private void VectorizeFunction(IRFunction function)
    {
        foreach (var block in function.Blocks)
            VectorizeBlock(block);
    }

    private void VectorizeBlock(IRBlock block)
    {
        var trees = ExtractExpressionTrees(block);

        foreach (var tree in trees)
        {
            if (tree.Nodes.Count < 2)
                continue;

            var compatibleSets = FindCompatibleSets(tree);
            foreach (var set in compatibleSets)
            {
                if (set.Count < 2 || set.Count > _maxLanes)
                    continue;

                VectorizeExpressionSet(block, set, tree);
            }
        }
    }

    private static List<ExpressionTree> ExtractExpressionTrees(IRBlock block)
    {
        var trees = new List<ExpressionTree>();
        var processed = new HashSet<int>();

        for (var i = 0; i < block.Instructions.Count; i++)
        {
            if (processed.Contains(i))
                continue;

            var inst = block.Instructions[i];
            if (inst is IRPhiNode)
                continue;
            if (inst.HasSideEffects)
                continue;
            if (inst.IsTerminator)
                continue;
            if (inst.Result == null)
                continue;
            if (!IsVectorizableOp(inst.OpCode))
                continue;

            var tree = new ExpressionTree();
            BuildExpressionTree(block, i, tree, processed, new HashSet<int>());

            if (tree.Nodes.Count >= 2)
                trees.Add(tree);
        }

        return trees;
    }

    private static void BuildExpressionTree(
        IRBlock block,
        int startIdx,
        ExpressionTree tree,
        HashSet<int> globalProcessed,
        HashSet<int> localVisited)
    {
        if (!localVisited.Add(startIdx))
            return;

        var inst = block.Instructions[startIdx];
        tree.Nodes.Add(new TreeNode(startIdx, inst));
        globalProcessed.Add(startIdx);

        for (var opIdx = 0; opIdx < inst.Operands.Count; opIdx++)
        {
            var operand = inst.Operands[opIdx];
            if (operand.IsConstant)
                continue;

            var defIdx = FindDefinition(block, operand);
            if (defIdx >= 0 && !globalProcessed.Contains(defIdx))
            {
                var defInst = block.Instructions[defIdx];
                if (IsVectorizableOp(defInst.OpCode) && defInst.Result != null)
                {
                    BuildExpressionTree(block, defIdx, tree, globalProcessed, localVisited);
                }
            }
        }
    }

    private static int FindDefinition(IRBlock block, IRValue value)
    {
        for (var i = 0; i < block.Instructions.Count; i++)
        {
            var inst = block.Instructions[i];
            if (inst.Result != null && inst.Result.Id == value.Id)
                return i;
        }
        return -1;
    }

    private static bool IsVectorizableOp(IROpCode opCode)
    {
        return opCode is
            IROpCode.Add or IROpCode.Sub or IROpCode.Mul or
            IROpCode.Div or IROpCode.Neg or IROpCode.Fma or
            IROpCode.Abs;
    }

    private static List<List<TreeNode>> FindCompatibleSets(ExpressionTree tree)
    {
        var sets = new List<List<TreeNode>>();
        var used = new HashSet<int>();

        for (var i = 0; i < tree.Nodes.Count; i++)
        {
            if (used.Contains(i))
                continue;

            var node = tree.Nodes[i];
            var set = new List<TreeNode> { node };
            used.Add(i);

            for (var j = i + 1; j < tree.Nodes.Count; j++)
            {
                if (used.Contains(j))
                    continue;

                var candidate = tree.Nodes[j];
                if (AreCompatible(node, candidate, tree))
                {
                    set.Add(candidate);
                    used.Add(j);
                }
            }

            if (set.Count >= 2)
                sets.Add(set);
        }

        return sets;
    }

    private static bool AreCompatible(TreeNode a, TreeNode b, ExpressionTree tree)
    {
        if (a.Instruction.OpCode != b.Instruction.OpCode)
            return false;

        if (a.Instruction.Operands.Count != b.Instruction.Operands.Count)
            return false;

        if (a.Instruction.Result?.Type != b.Instruction.Result?.Type)
            return false;

        if (IsAncestor(a, b, tree) || IsAncestor(b, a, tree))
            return false;

        return true;
    }

    private static bool IsAncestor(TreeNode potentialAncestor, TreeNode descendant, ExpressionTree tree)
    {
        var nodeMap = new Dictionary<int, TreeNode>();
        foreach (var n in tree.Nodes)
            nodeMap[n.Index] = n;

        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(descendant.Index);

        while (queue.Count > 0)
        {
            var currentIdx = queue.Dequeue();
            if (!visited.Add(currentIdx))
                continue;

            if (currentIdx == potentialAncestor.Index)
                return true;

            if (!nodeMap.TryGetValue(currentIdx, out var currentNode))
                continue;

            foreach (var operand in currentNode.Instruction.Operands)
            {
                if (operand.IsConstant)
                    continue;

                for (var i = 0; i < tree.Nodes.Count; i++)
                {
                    var n = tree.Nodes[i];
                    if (n.Instruction.Result != null && n.Instruction.Result.Id == operand.Id)
                    {
                        queue.Enqueue(n.Index);
                    }
                }
            }
        }

        return false;
    }

    private static void VectorizeExpressionSet(
        IRBlock block,
        List<TreeNode> set,
        ExpressionTree tree)
    {
        var vectorWidth = Vector<float>.Count;
        var numLanes = Math.Min(set.Count, vectorWidth);

        for (var i = 0; i < numLanes; i++)
        {
            var node = set[i];
            var inst = node.Instruction;
            var idx = block.Instructions.IndexOf(inst);
            if (idx < 0)
                continue;

            var vectorResult = IRValue.CreateRegister(
                $"exprv_{inst.Result!.Name}", IRType.Vector);

            var vectorOperands = inst.Operands
                .Select(o => o.IsConstant
                    ? IRValue.CreateConstant($"exprv_{o.Name}", o.ConstantValue ?? 0, IRType.Vector)
                    : IRValue.CreateRegister($"exprv_{o.Name}", IRType.Vector))
                .ToList();

            var vectorInst = new IRInstruction(IROpCode.VectorOp, vectorResult, vectorOperands);
            block.Instructions[idx] = vectorInst;
            vectorInst.ParentBlock = block;
            vectorInst.SequenceIndex = idx;
        }
    }

    private sealed class ExpressionTree
    {
        public List<TreeNode> Nodes { get; } = new();
    }

    private sealed class TreeNode
    {
        public int Index { get; }
        public IRInstruction Instruction { get; }

        public TreeNode(int index, IRInstruction instruction)
        {
            Index = index;
            Instruction = instruction;
        }
    }
}
