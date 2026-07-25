namespace MathVerse.Math.Visualization.CASVisualization;

using System.Collections.Immutable;

/// <summary>Visualizes algebraic simplification rewrite steps as a sequence of expression trees.</summary>
public sealed class RewriteTreeVisualizer
{
    private readonly ExpressionTreeVisualizer _treeVisualizer = new();

    /// <summary>Visualizes a chain of expression rewrites, producing a tree for each step.</summary>
    /// <param name="original">The original expression string.</param>
    /// <param name="steps">The ordered list of rewritten expression strings after each step.</param>
    /// <returns>An <see cref="ExpressionTreeResult"/> containing trees for all steps connected as a rewrite chain.</returns>
    public ExpressionTreeResult Visualize(string original, List<string> steps)
    {
        var result = new ExpressionTreeResult();
        var allExpressions = new List<string> { original };
        allExpressions.AddRange(steps);

        var stepTrees = new List<ExpressionTreeResult>();
        foreach (var expr in allExpressions)
        {
            var tree = _treeVisualizer.Visualize(expr);
            stepTrees.Add(tree);
        }

        if (stepTrees.Count == 0) return result;

        double xOffset = 0;
        int globalIdOffset = 0;

        for (int s = 0; s < stepTrees.Count; s++)
        {
            var tree = stepTrees[s];
            int maxIdInTree = tree.Nodes.Count > 0 ? tree.Nodes.Max(n => n.Id) : 0;

            foreach (var node in tree.Nodes)
            {
                result.Nodes.Add(new ExpressionTreeNode
                {
                    Id = node.Id + globalIdOffset,
                    Label = node.Label,
                    NodeType = node.NodeType,
                    X = node.X + xOffset,
                    Y = node.Y + s * 3.0,
                    Color = node.Color
                });
            }

            foreach (var edge in tree.Edges)
            {
                result.Edges.Add(new ExpressionTreeEdge
                {
                    FromNodeId = edge.FromNodeId + globalIdOffset,
                    ToNodeId = edge.ToNodeId + globalIdOffset,
                    Label = edge.Label
                });
            }

            // Add step label node
            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = globalIdOffset + maxIdInTree + 1,
                Label = s == 0 ? "Original" : $"Step {s}",
                NodeType = "label",
                X = -1.5 + xOffset,
                Y = 0.5 + s * 3.0,
                Color = s == 0 ? "#34495E" : "#E67E22"
            });

            xOffset += tree.Width + 3.0;
            globalIdOffset += maxIdInTree + 2;

            // Add rewrite arrow edge between steps
            if (s > 0)
            {
                int prevStepLabelId = globalIdOffset - (maxIdInTree + 2) - 1;
                int currLabelId = globalIdOffset - 1;
                result.Edges.Add(new ExpressionTreeEdge
                {
                    FromNodeId = prevStepLabelId,
                    ToNodeId = currLabelId,
                    Label = "→"
                });
            }
        }

        result.Width = (int)xOffset;
        result.Height = stepTrees.Count * 3;
        return result;
    }
}
