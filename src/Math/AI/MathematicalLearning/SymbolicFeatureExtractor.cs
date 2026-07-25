namespace MathVerse.Math.AI.MathematicalLearning;

using System;
using System.Collections.Generic;

/// <summary>Extracts numerical features from mathematical expressions: depth, width, operator count, variable count, and complexity.</summary>
public sealed class SymbolicFeatureExtractor
{
    private static readonly HashSet<string> Operators = new() { "+", "-", "*", "/", "^" };
    private static readonly HashSet<string> Functions = new() { "sin", "cos", "tan", "exp", "log", "sqrt", "asin", "acos", "atan" };
    private static readonly HashSet<string> Variables = new() { "x", "y", "z", "a", "b", "c", "n", "t" };

    /// <summary>Initializes a new symbolic feature extractor.</summary>
    public SymbolicFeatureExtractor()
    {
    }

    /// <summary>Extracts a feature vector from a mathematical expression.</summary>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns>Array of features: [depth, width, numOperators, numFunctions, numVariables, numConstants, complexityScore, nestingDepth, termCount].</returns>
    public double[] ExtractFeatures(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        ExprNode tree = ParseToTree(expression);

        int depth = ComputeDepth(tree);
        int width = ComputeWidth(tree);
        int numOperators = CountNodes(tree, Operators);
        int numFunctions = CountNodes(tree, Functions);
        int numVariables = CountVariables(expression);
        int numConstants = CountConstants(expression);
        double complexity = ComputeComplexityScore(tree);
        int nestingDepth = ComputeNestingDepth(expression);
        int termCount = CountTerms(expression);

        return new double[]
        {
            depth,
            width,
            numOperators,
            numFunctions,
            numVariables,
            numConstants,
            complexity,
            nestingDepth,
            termCount
        };
    }

    /// <summary>Computes the depth (height) of the expression tree.</summary>
    /// <param name="expression">The expression string.</param>
    /// <returns>Tree depth.</returns>
    public int ComputeTreeDepth(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return 0;
        ExprNode tree = ParseToTree(expression);
        return ComputeDepth(tree);
    }

    /// <summary>Computes the width (maximum number of nodes at any level) of the expression tree.</summary>
    /// <param name="expression">The expression string.</param>
    /// <returns>Tree width.</returns>
    public int ComputeTreeWidth(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return 0;
        ExprNode tree = ParseToTree(expression);
        return ComputeWidth(tree);
    }

    /// <summary>Counts the number of operators in the expression.</summary>
    /// <param name="expression">The expression string.</param>
    /// <returns>Operator count.</returns>
    public int CountOperators(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return 0;

        int count = 0;
        foreach (char c in expression)
        {
            if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^')
                count++;
        }
        return count;
    }

    /// <summary>Counts the number of distinct variables in the expression.</summary>
    /// <param name="expression">The expression string.</param>
    /// <returns>Distinct variable count.</returns>
    public int CountDistinctVariables(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return 0;

        HashSet<string> found = new();
        foreach (char c in expression)
        {
            string s = c.ToString();
            if (Variables.Contains(s))
                found.Add(s);
        }
        return found.Count;
    }

    /// <summary>Computes the syntactic complexity score of the expression.</summary>
    /// <param name="expression">The expression string.</param>
    /// <returns>Complexity score (higher means more complex).</returns>
    public double ComputeSyntacticComplexity(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return 0.0;

        ExprNode tree = ParseToTree(expression);
        return ComputeComplexityScore(tree);
    }

    /// <summary>Identifies the dominant operator in the expression (the one closest to the root).</summary>
    /// <param name="expression">The expression string.</param>
    /// <returns>The dominant operator string.</returns>
    public string GetDominantOperator(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return "";

        ExprNode tree = ParseToTree(expression);
        return tree.Label;
    }

    private int ComputeDepth(ExprNode node)
    {
        if (node == null || node.Children.Count == 0)
            return 1;

        int maxChild = 0;
        foreach (ExprNode child in node.Children)
        {
            int d = ComputeDepth(child);
            if (d > maxChild)
                maxChild = d;
        }
        return 1 + maxChild;
    }

    private int ComputeWidth(ExprNode node)
    {
        if (node == null)
            return 0;
        if (node.Children.Count == 0)
            return 1;

        int width = 0;
        Queue<ExprNode> queue = new();
        queue.Enqueue(node);

        while (queue.Count > 0)
        {
            int levelSize = queue.Count;
            if (levelSize > width)
                width = levelSize;

            for (int i = 0; i < levelSize; i++)
            {
                ExprNode current = queue.Dequeue();
                foreach (ExprNode child in current.Children)
                    queue.Enqueue(child);
            }
        }

        return width;
    }

    private static int CountNodes(ExprNode node, HashSet<string> labels)
    {
        if (node == null)
            return 0;

        int count = labels.Contains(node.Label) ? 1 : 0;
        foreach (ExprNode child in node.Children)
            count += CountNodes(child, labels);
        return count;
    }

    private static int CountVariables(string expression)
    {
        int count = 0;
        foreach (char c in expression)
        {
            if (Variables.Contains(c.ToString()))
                count++;
        }
        return count;
    }

    private static int CountConstants(string expression)
    {
        int count = 0;
        bool inNumber = false;
        foreach (char c in expression)
        {
            if (char.IsDigit(c) || c == '.')
            {
                if (!inNumber)
                {
                    inNumber = true;
                    count++;
                }
            }
            else
            {
                inNumber = false;
            }
        }
        return count;
    }

    private double ComputeComplexityScore(ExprNode node)
    {
        if (node == null)
            return 0.0;

        double score = 1.0;
        if (Operators.Contains(node.Label))
            score += 1.0;
        if (Functions.Contains(node.Label))
            score += 2.0;

        foreach (ExprNode child in node.Children)
            score += ComputeComplexityScore(child);

        return score;
    }

    private static int ComputeNestingDepth(string expression)
    {
        int maxDepth = 0;
        int depth = 0;
        foreach (char c in expression)
        {
            if (c == '(')
            {
                depth++;
                if (depth > maxDepth)
                    maxDepth = depth;
            }
            else if (c == ')')
                depth--;
        }
        return maxDepth;
    }

    private static int CountTerms(string expression)
    {
        int count = 1;
        foreach (char c in expression)
        {
            if (c == '+' || c == '-')
                count++;
        }
        return count;
    }

    private ExprNode ParseToTree(string expression)
    {
        string remaining = expression.Trim();
        return ParseAddSub(ref remaining);
    }

    private ExprNode ParseAddSub(ref string remaining)
    {
        remaining = remaining.TrimStart();
        ExprNode left = ParseMulDiv(ref remaining);

        while (remaining.Length > 0 && (remaining[0] == '+' || remaining[0] == '-'))
        {
            char op = remaining[0];
            remaining = remaining[1..];
            ExprNode right = ParseMulDiv(ref remaining);
            left = new ExprNode(op.ToString(), new List<ExprNode> { left, right });
        }

        return left;
    }

    private ExprNode ParseMulDiv(ref string remaining)
    {
        remaining = remaining.TrimStart();
        ExprNode left = ParsePower(ref remaining);

        while (remaining.Length > 0 && (remaining[0] == '*' || remaining[0] == '/'))
        {
            char op = remaining[0];
            remaining = remaining[1..];
            ExprNode right = ParsePower(ref remaining);
            left = new ExprNode(op.ToString(), new List<ExprNode> { left, right });
        }

        return left;
    }

    private ExprNode ParsePower(ref string remaining)
    {
        remaining = remaining.TrimStart();
        ExprNode left = ParseAtom(ref remaining);

        if (remaining.Length > 0 && remaining[0] == '^')
        {
            remaining = remaining[1..];
            ExprNode right = ParsePower(ref remaining);
            left = new ExprNode("^", new List<ExprNode> { left, right });
        }

        return left;
    }

    private ExprNode ParseAtom(ref string remaining)
    {
        remaining = remaining.TrimStart();

        if (remaining.Length > 0 && remaining[0] == '(')
        {
            remaining = remaining[1..];
            ExprNode inner = ParseAddSub(ref remaining);
            remaining = remaining.TrimStart();
            if (remaining.Length > 0 && remaining[0] == ')')
                remaining = remaining[1..];
            return inner;
        }

        if (remaining.Length > 0 && remaining[0] == '-')
        {
            remaining = remaining[1..];
            ExprNode child = ParseAtom(ref remaining);
            return new ExprNode("neg", new List<ExprNode> { child });
        }

        string[] funcs = ["sin", "cos", "tan", "exp", "log", "sqrt"];
        foreach (string fn in funcs)
        {
            if (remaining.StartsWith(fn + "(", StringComparison.Ordinal))
            {
                remaining = remaining[(fn.Length + 1)..];
                ExprNode arg = ParseAddSub(ref remaining);
                remaining = remaining.TrimStart();
                if (remaining.Length > 0 && remaining[0] == ')')
                    remaining = remaining[1..];
                return new ExprNode(fn, new List<ExprNode> { arg });
            }
        }

        int start = 0;
        while (remaining.Length > start && (char.IsDigit(remaining[start]) || remaining[start] == '.' ||
               remaining[start] == 'x' || remaining[start] == 'y' || remaining[start] == 'z'))
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
