namespace MathVerse.Math.Visualization.CASVisualization;

/// <summary>Visualizes a simplification pipeline showing before/after expressions and the applied rule at each step.</summary>
public sealed class SimplificationStepVisualizer
{
    private const double RowSpacing = 2.0;
    private const double ColSpacing = 4.0;

    /// <summary>Visualizes a sequence of simplification steps as a linear pipeline of expression nodes.</summary>
    /// <param name="steps">A list of (before expression, after expression, rule name) tuples.</param>
    /// <returns>An <see cref="ExpressionTreeResult"/> containing expression nodes connected by rule-labeled edges.</returns>
    public ExpressionTreeResult Visualize(List<(string Before, string After, string RuleName)> steps)
    {
        var result = new ExpressionTreeResult();
        int nodeId = 0;

        for (int i = 0; i < steps.Count; i++)
        {
            var (before, after, ruleName) = steps[i];

            // Before node
            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = nodeId,
                Label = before,
                NodeType = "expression",
                X = i * ColSpacing,
                Y = 0,
                Color = i == 0 ? "#3498DB" : "#95A5A6"
            });

            int beforeId = nodeId;
            nodeId++;

            // Rule label node
            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = nodeId,
                Label = ruleName,
                NodeType = "rule",
                X = i * ColSpacing + ColSpacing * 0.5,
                Y = RowSpacing * 0.5,
                Color = "#E67E22"
            });

            int ruleId = nodeId;
            nodeId++;

            // After node
            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = nodeId,
                Label = after,
                NodeType = "expression",
                X = (i + 1) * ColSpacing,
                Y = 0,
                Color = i == steps.Count - 1 ? "#2ECC71" : "#95A5A6"
            });

            int afterId = nodeId;
            nodeId++;

            // Edges: before -> rule -> after
            result.Edges.Add(new ExpressionTreeEdge
            {
                FromNodeId = beforeId,
                ToNodeId = ruleId,
                Label = ""
            });

            result.Edges.Add(new ExpressionTreeEdge
            {
                FromNodeId = ruleId,
                ToNodeId = afterId,
                Label = ""
            });

            // Chain consecutive before nodes
            if (i > 0)
            {
                int prevAfterId = nodeId - 3 - 3 + 1;
                result.Edges.Add(new ExpressionTreeEdge
                {
                    FromNodeId = prevAfterId,
                    ToNodeId = beforeId,
                    Label = "..."
                });
            }
        }

        result.Width = (int)((steps.Count + 1) * ColSpacing);
        result.Height = (int)(RowSpacing * 2);
        return result;
    }
}
