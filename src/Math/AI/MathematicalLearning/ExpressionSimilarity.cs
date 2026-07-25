namespace MathVerse.Math.AI.MathematicalLearning;

using System;
using System.Collections.Generic;

/// <summary>Computes structural similarity between mathematical expressions using tree-edit distance.</summary>
public sealed class ExpressionSimilarity
{
    /// <summary>Initializes a new expression similarity calculator.</summary>
    public ExpressionSimilarity()
    {
    }

    /// <summary>Computes the structural similarity between two mathematical expressions (0.0 to 1.0).</summary>
    /// <param name="expr1">First expression.</param>
    /// <param name="expr2">Second expression.</param>
    /// <returns>Similarity score between 0.0 (completely different) and 1.0 (identical).</returns>
    public double ComputeSimilarity(string expr1, string expr2)
    {
        if (string.IsNullOrEmpty(expr1) && string.IsNullOrEmpty(expr2))
            return 1.0;
        if (string.IsNullOrEmpty(expr1) || string.IsNullOrEmpty(expr2))
            return 0.0;

        ExprNode tree1 = ParseExpressionTree(expr1);
        ExprNode tree2 = ParseExpressionTree(expr2);

        int distance = TreeEditDistance(tree1, tree2);
        int maxSize = System.Math.Max(TreeSize(tree1), TreeSize(tree2));

        if (maxSize == 0)
            return 1.0;

        return 1.0 - (double)distance / maxSize;
    }

    /// <summary>Computes the tree-edit distance between two expressions.</summary>
    /// <param name="expr1">First expression.</param>
    /// <param name="expr2">Second expression.</param>
    /// <returns>Minimum number of edit operations to transform expr1 into expr2.</returns>
    public int TreeEditDistance(string expr1, string expr2)
    {
        if (string.IsNullOrEmpty(expr1) && string.IsNullOrEmpty(expr2))
            return 0;
        if (string.IsNullOrEmpty(expr1))
            return TreeSize(ParseExpressionTree(expr2));
        if (string.IsNullOrEmpty(expr2))
            return TreeSize(ParseExpressionTree(expr1));

        ExprNode tree1 = ParseExpressionTree(expr1);
        ExprNode tree2 = ParseExpressionTree(expr2);

        return TreeEditDistance(tree1, tree2);
    }

    /// <summary>Parses a mathematical expression string into a tree node representation.</summary>
    /// <param name="expression">The expression string.</param>
    /// <returns>Root node of the expression tree.</returns>
    public ExprNode ParseExpressionTree(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        string remaining = expression.Trim();
        return ParseExprInternal(ref remaining);
    }

    private int TreeEditDistance(ExprNode t1, ExprNode t2)
    {
        if (t1 == null && t2 == null)
            return 0;
        if (t1 == null)
            return TreeSize(t2);
        if (t2 == null)
            return TreeSize(t1);

        int m = t1.Children.Count;
        int n = t2.Children.Count;

        if (m == 0 && n == 0)
            return t1.Label == t2.Label ? 0 : 1;

        int[][] dp = new int[m + 1][];
        for (int i = 0; i <= m; i++)
        {
            dp[i] = new int[n + 1];
            for (int j = 0; j <= n; j++)
                dp[i][j] = 0;
        }

        for (int j = 0; j <= n; j++)
            dp[0][j] = SubtreeCost(t2, j);
        for (int i = 0; i <= m; i++)
            dp[i][0] = SubtreeCost(t1, i);

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                int relabelCost = (t1.Children[i - 1].Label == t2.Children[j - 1].Label) ? 0 : 1;
                int matchCost = dp[i - 1][j - 1] + relabelCost + TreeEditDistance(t1.Children[i - 1], t2.Children[j - 1]);
                int deleteCost = dp[i - 1][j] + TreeSize(t1.Children[i - 1]);
                int insertCost = dp[i][j - 1] + TreeSize(t2.Children[j - 1]);
                dp[i][j] = System.Math.Min(matchCost, System.Math.Min(deleteCost, insertCost));
            }
        }

        int labelCost = t1.Label == t2.Label ? 0 : 1;
        return labelCost + dp[m][n];
    }

    private static int SubtreeCost(ExprNode node, int childCount)
    {
        int cost = 0;
        for (int i = childCount; i < node.Children.Count; i++)
            cost += TreeSize(node.Children[i]);
        return cost;
    }

    private static int TreeSize(ExprNode node)
    {
        if (node == null)
            return 0;
        int size = 1;
        foreach (ExprNode child in node.Children)
            size += TreeSize(child);
        return size;
    }

    private ExprNode ParseExprInternal(ref string remaining)
    {
        remaining = remaining.TrimStart();
        return ParseAddSubInternal(ref remaining);
    }

    private ExprNode ParseAddSubInternal(ref string remaining)
    {
        ExprNode left = ParseMulDivInternal(ref remaining);

        while (remaining.Length > 0 && (remaining[0] == '+' || remaining[0] == '-'))
        {
            char op = remaining[0];
            remaining = remaining[1..];
            ExprNode right = ParseMulDivInternal(ref remaining);
            left = new ExprNode(op.ToString(), new List<ExprNode> { left, right });
        }

        return left;
    }

    private ExprNode ParseMulDivInternal(ref string remaining)
    {
        ExprNode left = ParseAtomInternal(ref remaining);

        while (remaining.Length > 0 && (remaining[0] == '*' || remaining[0] == '/'))
        {
            char op = remaining[0];
            remaining = remaining[1..];
            ExprNode right = ParseAtomInternal(ref remaining);
            left = new ExprNode(op.ToString(), new List<ExprNode> { left, right });
        }

        return left;
    }

    private ExprNode ParseAtomInternal(ref string remaining)
    {
        remaining = remaining.TrimStart();

        if (remaining.Length > 0 && remaining[0] == '(')
        {
            remaining = remaining[1..];
            ExprNode inner = ParseExprInternal(ref remaining);
            remaining = remaining.TrimStart();
            if (remaining.Length > 0 && remaining[0] == ')')
                remaining = remaining[1..];
            return inner;
        }

        if (remaining.Length > 0 && remaining[0] == '-')
        {
            remaining = remaining[1..];
            ExprNode child = ParseAtomInternal(ref remaining);
            return new ExprNode("neg", new List<ExprNode> { child });
        }

        string[] funcs = ["sin", "cos", "tan", "exp", "log", "sqrt"];
        foreach (string fn in funcs)
        {
            if (remaining.StartsWith(fn + "(", StringComparison.Ordinal))
            {
                remaining = remaining[(fn.Length + 1)..];
                ExprNode arg = ParseExprInternal(ref remaining);
                remaining = remaining.TrimStart();
                if (remaining.Length > 0 && remaining[0] == ')')
                    remaining = remaining[1..];
                return new ExprNode(fn, new List<ExprNode> { arg });
            }
        }

        int start = 0;
        while (remaining.Length > start && (char.IsDigit(remaining[start]) || remaining[start] == '.' || remaining[start] == 'x' || remaining[start] == 'y' || remaining[start] == 'z'))
            start++;

        if (start > 0)
        {
            string token = remaining[..start];
            remaining = remaining[start..];
            return new ExprNode(token, new List<ExprNode>());
        }

        if (remaining.Length > 0)
        {
            char c = remaining[0];
            remaining = remaining[1..];
            return new ExprNode(c.ToString(), new List<ExprNode>());
        }

        return new ExprNode("?", new List<ExprNode>());
    }
}

/// <summary>Represents a node in an expression tree.</summary>
public sealed class ExprNode
{
    /// <summary>Gets the label (operator or operand) of this node.</summary>
    public string Label { get; }

    /// <summary>Gets the child nodes.</summary>
    public List<ExprNode> Children { get; }

    /// <summary>Initializes a new expression tree node.</summary>
    /// <param name="label">The node label.</param>
    /// <param name="children">The child nodes.</param>
    public ExprNode(string label, List<ExprNode> children)
    {
        Label = label;
        Children = children ?? new List<ExprNode>();
    }

    /// <summary>Returns a string representation of this subtree.</summary>
    /// <returns>Expression string.</returns>
    public override string ToString()
    {
        if (Children.Count == 0)
            return Label;
        if (Children.Count == 1)
            return $"{Label}({Children[0]})";
        if (Children.Count == 2)
            return $"({Children[0]} {Label} {Children[1]})";
        string args = string.Join(", ", Children.ConvertAll(c => c.ToString()));
        return $"{Label}({args})";
    }
}
