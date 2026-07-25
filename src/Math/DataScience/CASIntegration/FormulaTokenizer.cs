namespace MathVerse.Math.DataScience.CASIntegration;

using System;

/// <summary>
/// Tokenizes mathematical expressions into an array of tokens for parsing.
/// </summary>
public static class FormulaTokenizer
{
    /// <summary>
    /// Tokenizes a mathematical expression string into individual tokens.
    /// </summary>
    /// <param name="expression">The expression to tokenize.</param>
    /// <returns>An array of tokens.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression contains invalid characters.</exception>
    public static string[] Tokenize(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return Array.Empty<string>();

        var tokens = new System.Collections.Generic.List<string>();
        string s = expression.Trim();
        int i = 0;

        while (i < s.Length)
        {
            char c = s[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (c is '+' or '-' or '*' or '/' or '^' or '(' or ')' or ',')
            {
                tokens.Add(c.ToString());
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                int start = i;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.'))
                    i++;
                if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
                {
                    i++;
                    if (i < s.Length && (s[i] == '+' || s[i] == '-'))
                        i++;
                    while (i < s.Length && char.IsDigit(s[i]))
                        i++;
                }
                tokens.Add(s[start..i]);
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                    i++;
                tokens.Add(s[start..i]);
                continue;
            }

            throw new ArgumentException($"Invalid character '{c}' at position {i} in expression.");
        }

        return tokens.ToArray();
    }
}
