namespace MathVerse.Math.AI.MathematicalLearning;

using System;
using System.Collections.Generic;

/// <summary>Fixed-size vector embedding of mathematical expressions using bag-of-operators and positional encoding.</summary>
public sealed class MathematicalEmbedding
{
    private static readonly Dictionary<string, int> OperatorIndex = new()
    {
        ["+"] = 0, ["-"] = 1, ["*"] = 2, ["/"] = 3, ["^"] = 4,
        ["sin"] = 5, ["cos"] = 6, ["tan"] = 7, ["exp"] = 8, ["log"] = 9,
        ["sqrt"] = 10, ["("] = 11, [")"] = 12, ["neg"] = 13
    };

    private const int NumOperators = 14;

    /// <summary>Initializes a new mathematical embedding calculator.</summary>
    public MathematicalEmbedding()
    {
    }

    /// <summary>Computes a fixed-size embedding vector for a mathematical expression.</summary>
    /// <param name="expression">The expression to embed.</param>
    /// <param name="dimensions">Embedding dimension (default 64).</param>
    /// <returns>Embedding vector of the specified dimension.</returns>
    public double[] Embed(string expression, int dimensions = 64)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));
        if (dimensions <= 0)
            throw new ArgumentException("Dimensions must be positive.", nameof(dimensions));

        double[] embedding = new double[dimensions];
        string[] tokens = Tokenize(expression);

        BagOfOperatorsEmbed(tokens, embedding, dimensions);
        PositionalEncoding(tokens, embedding, dimensions);
        StructuralFeaturesEmbed(expression, embedding, dimensions);

        double norm = 0.0;
        for (int i = 0; i < dimensions; i++)
            norm += embedding[i] * embedding[i];
        norm = System.Math.Sqrt(norm);
        if (norm > 1e-10)
        {
            for (int i = 0; i < dimensions; i++)
                embedding[i] /= norm;
        }

        return embedding;
    }

    /// <summary>Computes cosine similarity between two expression embeddings.</summary>
    /// <param name="expr1">First expression.</param>
    /// <param name="expr2">Second expression.</param>
    /// <param name="dimensions">Embedding dimension.</param>
    /// <returns>Cosine similarity between -1 and 1.</returns>
    public double CosineSimilarity(string expr1, string expr2, int dimensions = 64)
    {
        if (string.IsNullOrEmpty(expr1) || string.IsNullOrEmpty(expr2))
            return 0.0;

        double[] emb1 = Embed(expr1, dimensions);
        double[] emb2 = Embed(expr2, dimensions);

        double dot = 0.0, norm1 = 0.0, norm2 = 0.0;
        for (int i = 0; i < dimensions; i++)
        {
            dot += emb1[i] * emb2[i];
            norm1 += emb1[i] * emb1[i];
            norm2 += emb2[i] * emb2[i];
        }

        double denom = System.Math.Sqrt(norm1) * System.Math.Sqrt(norm2);
        if (denom < 1e-10)
            return 0.0;

        return dot / denom;
    }

    /// <summary>Computes the bag-of-operators frequency vector.</summary>
    /// <param name="expression">The expression.</param>
    /// <returns>Frequency vector of length 14 (one per operator type).</returns>
    public double[] BagOfOperators(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        string[] tokens = Tokenize(expression);
        double[] freq = new double[NumOperators];

        foreach (string token in tokens)
        {
            if (OperatorIndex.TryGetValue(token, out int idx))
                freq[idx] += 1.0;
        }

        double total = 0.0;
        for (int i = 0; i < NumOperators; i++)
            total += freq[i];

        if (total > 0.0)
        {
            for (int i = 0; i < NumOperators; i++)
                freq[i] /= total;
        }

        return freq;
    }

    /// <summary>Tokenizes a mathematical expression into a list of tokens.</summary>
    /// <param name="expression">The expression to tokenize.</param>
    /// <returns>List of tokens.</returns>
    public string[] Tokenize(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        List<string> tokens = new();
        string current = "";
        int i = 0;

        while (i < expression.Length)
        {
            char c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current);
                    current = "";
                }
                i++;
                continue;
            }

            if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '(' || c == ')')
            {
                if (current.Length > 0)
                {
                    tokens.Add(current);
                    current = "";
                }
                tokens.Add(c.ToString());
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                current += c;
                i++;
                continue;
            }

            if (char.IsLetter(c))
            {
                current += c;
                i++;
                while (i < expression.Length && char.IsLetterOrDigit(expression[i]))
                {
                    current += expression[i];
                    i++;
                }

                string[] funcs = ["sin", "cos", "tan", "exp", "log", "sqrt"];
                string lowered = current.ToLowerInvariant();
                bool isFunc = false;
                foreach (string fn in funcs)
                {
                    if (lowered == fn)
                    {
                        isFunc = true;
                        break;
                    }
                }

                if (isFunc)
                    tokens.Add(lowered);
                else
                    tokens.Add(current);

                current = "";
                continue;
            }

            i++;
        }

        if (current.Length > 0)
            tokens.Add(current);

        return tokens.ToArray();
    }

    private void BagOfOperatorsEmbed(string[] tokens, double[] embedding, int dimensions)
    {
        int baseOffset = 0;
        int slotSize = System.Math.Min(NumOperators, dimensions - baseOffset);

        double[] freq = new double[NumOperators];
        foreach (string token in tokens)
        {
            if (OperatorIndex.TryGetValue(token, out int idx))
                freq[idx] += 1.0;
        }

        double total = 0.0;
        for (int i = 0; i < NumOperators; i++)
            total += freq[i];

        for (int i = 0; i < slotSize; i++)
        {
            if (total > 0.0)
                embedding[baseOffset + i] = freq[i] / total;
            else
                embedding[baseOffset + i] = 0.0;
        }
    }

    private void PositionalEncoding(string[] tokens, double[] embedding, int dimensions)
    {
        int baseOffset = NumOperators;
        int available = dimensions - baseOffset;

        if (available <= 0)
            return;

        for (int pos = 0; pos < tokens.Length && pos < available; pos++)
        {
            for (int d = 0; d < 2 && baseOffset + pos * 2 + d < dimensions; d++)
            {
                double angle = pos / System.Math.Pow(10000.0, 2.0 * d / dimensions);
                if (d == 0)
                    embedding[baseOffset + pos * 2] = System.Math.Sin(angle);
                else
                    embedding[baseOffset + pos * 2 + 1] = System.Math.Cos(angle);
            }
        }
    }

    private void StructuralFeaturesEmbed(string expression, double[] embedding, int dimensions)
    {
        int baseOffset = dimensions - 4;
        if (baseOffset < NumOperators)
            baseOffset = NumOperators;

        int depth = 0;
        int maxDepth = 0;
        foreach (char c in expression)
        {
            if (c == '(') { depth++; if (depth > maxDepth) maxDepth = depth; }
            else if (c == ')') depth--;
        }

        int numTerms = 1;
        foreach (char c in expression)
        {
            if (c == '+' || c == '-') numTerms++;
        }

        int numOps = 0;
        foreach (char c in expression)
        {
            if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^')
                numOps++;
        }

        double lengthNorm = System.Math.Min(1.0, expression.Length / 100.0);

        if (baseOffset < dimensions)
            embedding[baseOffset] = maxDepth / 10.0;
        if (baseOffset + 1 < dimensions)
            embedding[baseOffset + 1] = numTerms / 20.0;
        if (baseOffset + 2 < dimensions)
            embedding[baseOffset + 2] = numOps / 20.0;
        if (baseOffset + 3 < dimensions)
            embedding[baseOffset + 3] = lengthNorm;
    }
}
