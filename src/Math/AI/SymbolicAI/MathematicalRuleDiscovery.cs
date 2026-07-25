namespace MathVerse.Math.AI.SymbolicAI;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Discovers mathematical patterns, common factors, and simplification opportunities in expressions.</summary>
public sealed class MathematicalRuleDiscovery
{
    /// <summary>Initializes a new mathematical rule discovery instance.</summary>
    public MathematicalRuleDiscovery()
    {
    }

    /// <summary>Analyzes a collection of expressions and discovers frequently occurring sub-expressions.</summary>
    /// <param name="expressions">List of mathematical expression strings.</param>
    /// <param name="minFrequency">Minimum frequency for a sub-expression to be considered common.</param>
    /// <returns>Dictionary of sub-expression to its frequency count.</returns>
    public Dictionary<string, int> DiscoverFrequentSubExpressions(List<string> expressions, int minFrequency = 2)
    {
        if (expressions == null)
            throw new ArgumentNullException(nameof(expressions));
        if (minFrequency < 1)
            throw new ArgumentException("Minimum frequency must be at least 1.", nameof(minFrequency));

        Dictionary<string, int> frequency = new();

        foreach (string expr in expressions)
        {
            HashSet<string> subExprs = ExtractSubExpressions(expr);
            foreach (string sub in subExprs)
            {
                if (frequency.ContainsKey(sub))
                    frequency[sub]++;
                else
                    frequency[sub] = 1;
            }
        }

        Dictionary<string, int> result = new();
        foreach (KeyValuePair<string, int> kv in frequency)
        {
            if (kv.Value >= minFrequency)
                result[kv.Key] = kv.Value;
        }

        return result;
    }

    /// <summary>Extracts all sub-expressions from a mathematical expression string.</summary>
    /// <param name="expression">The expression to decompose.</param>
    /// <returns>Set of sub-expression strings.</returns>
    public HashSet<string> ExtractSubExpressions(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        HashSet<string> subExprs = new();
        ExtractSubExprHelper(expression.Trim(), subExprs);
        return subExprs;
    }

    /// <summary>Identifies common factors in a list of polynomial-like expressions.</summary>
    /// <param name="expressions">List of expressions.</param>
    /// <returns>List of common factor strings found across expressions.</returns>
    public List<string> FindCommonFactors(List<string> expressions)
    {
        if (expressions == null)
            throw new ArgumentNullException(nameof(expressions));

        List<List<string>> termLists = new();
        foreach (string expr in expressions)
            termLists.Add(ExtractTerms(expr));

        if (termLists.Count == 0)
            return new List<string>();

        HashSet<string> commonTerms = new(termLists[0]);
        foreach (List<string> terms in termLists)
            commonTerms.IntersectWith(terms);

        return commonTerms.ToList();
    }

    /// <summary>Detects structural patterns in an expression (polynomial, exponential, periodic, rational).</summary>
    /// <param name="expression">The expression to classify.</param>
    /// <returns>List of detected pattern types.</returns>
    public List<ExpressionPattern> DetectPatterns(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        List<ExpressionPattern> patterns = new();
        string normalized = expression.Trim().ToLowerInvariant();

        if (IsPolynomial(normalized))
            patterns.Add(new ExpressionPattern { Type = "Polynomial", Description = "Expression is a polynomial." });

        if (HasExponential(normalized))
            patterns.Add(new ExpressionPattern { Type = "Exponential", Description = "Expression contains exponential terms." });

        if (HasTrigonometric(normalized))
            patterns.Add(new ExpressionPattern { Type = "Periodic", Description = "Expression contains trigonometric (periodic) terms." });

        if (IsRational(normalized))
            patterns.Add(new ExpressionPattern { Type = "Rational", Description = "Expression is a rational function." });

        if (HasLogarithm(normalized))
            patterns.Add(new ExpressionPattern { Type = "Logarithmic", Description = "Expression contains logarithmic terms." });

        if (HasNested(normalized))
            patterns.Add(new ExpressionPattern { Type = "Nested", Description = "Expression has nested function calls." });

        return patterns;
    }

    /// <summary>Finds simplification opportunities in an expression.</summary>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns>List of simplification suggestions.</returns>
    public List<string> FindSimplificationOpportunities(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        List<string> opportunities = new();
        string e = expression.Trim();

        if (e.Contains("x * 0") || e.Contains("0 * x") || e.Contains("x*0") || e.Contains("0*x"))
            opportunities.Add("Multiply by zero: entire term vanishes.");

        if (e.Contains("x + 0") || e.Contains("0 + x") || e.Contains("x+0") || e.Contains("0+x"))
            opportunities.Add("Additive identity: adding zero can be removed.");

        if (e.Contains("x * 1") || e.Contains("1 * x") || e.Contains("x*1") || e.Contains("1*x"))
            opportunities.Add("Multiplicative identity: multiplying by 1 is redundant.");

        if (e.Contains("x^1") || e.Contains("x ** 1"))
            opportunities.Add("Power of 1: x^1 simplifies to x.");

        if (e.Contains("x^0") || e.Contains("x ** 0"))
            opportunities.Add("Power of 0: x^0 simplifies to 1.");

        if (e.Contains("sin(x)^2") && e.Contains("cos(x)^2"))
            opportunities.Add("Pythagorean identity: sin^2(x) + cos^2(x) = 1.");

        if (e.Contains("log(exp(") || e.Contains("exp(log("))
            opportunities.Add("Inverse functions: log(exp(x)) = x and exp(log(x)) = x.");

        if (CountOccurrences(e, '+') > 2)
            opportunities.Add("Multiple additions: consider factoring or grouping.");

        if (HasDuplicateTerms(e))
            opportunities.Add("Duplicate terms detected: combine like terms.");

        return opportunities;
    }

    /// <summary>Extracts terms from a sum expression by splitting on + and -.</summary>
    /// <param name="expression">The expression to decompose.</param>
    /// <returns>List of individual term strings.</returns>
    public List<string> ExtractTerms(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        List<string> terms = new();
        string current = "";
        foreach (char c in expression)
        {
            if ((c == '+' || c == '-') && current.Length > 0)
            {
                terms.Add(current.Trim());
                current = c.ToString();
            }
            else
            {
                current += c;
            }
        }
        if (current.Length > 0)
            terms.Add(current.Trim());

        return terms.Where(t => t.Length > 0).ToList();
    }

    private void ExtractSubExprHelper(string expr, HashSet<string> collected)
    {
        if (expr.Length < 2)
            return;

        collected.Add(expr);

        int depth = 0;
        int start = -1;
        for (int i = 0; i < expr.Length; i++)
        {
            if (expr[i] == '(')
            {
                if (depth == 0)
                    start = i;
                depth++;
            }
            else if (expr[i] == ')')
            {
                depth--;
                if (depth == 0 && start >= 0)
                {
                    string inner = expr[(start + 1)..i];
                    if (inner.Length > 1)
                    {
                        collected.Add(inner);
                        ExtractSubExprHelper(inner, collected);
                    }
                    start = -1;
                }
            }
        }
    }

    private static bool IsPolynomial(string expr)
    {
        string[] operators = ["+", "-", "*", "^"];
        foreach (char c in expr)
        {
            if (char.IsLetter(c) && !IsInSingleCharContext(expr, c, "x"))
            {
                string[] mathFuncs = ["sin", "cos", "tan", "exp", "log", "sqrt"];
                foreach (string fn in mathFuncs)
                {
                    if (expr.Contains(fn))
                        return false;
                }
            }
        }
        return true;
    }

    private static bool IsInSingleCharContext(string expr, char c, string target)
    {
        return c == target[0];
    }

    private static bool HasExponential(string expr)
    {
        return expr.Contains("exp(") || expr.Contains("e^");
    }

    private static bool HasTrigonometric(string expr)
    {
        return expr.Contains("sin(") || expr.Contains("cos(") || expr.Contains("tan(") ||
               expr.Contains("asin(") || expr.Contains("acos(") || expr.Contains("atan(");
    }

    private static bool IsRational(string expr)
    {
        return expr.Contains('/');
    }

    private static bool HasLogarithm(string expr)
    {
        return expr.Contains("log(") || expr.Contains("ln(");
    }

    private static bool HasNested(string expr)
    {
        int maxDepth = 0;
        int depth = 0;
        foreach (char c in expr)
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
        return maxDepth >= 2;
    }

    private static int CountOccurrences(string text, char target)
    {
        int count = 0;
        foreach (char c in text)
        {
            if (c == target)
                count++;
        }
        return count;
    }

    private static bool HasDuplicateTerms(string expr)
    {
        List<string> terms = new();
        string current = "";
        foreach (char c in expr)
        {
            if (c == '+' || c == '-')
            {
                if (current.Length > 0)
                    terms.Add(current.Trim());
                current = c.ToString();
            }
            else
            {
                current += c;
            }
        }
        if (current.Length > 0)
            terms.Add(current.Trim());

        HashSet<string> seen = new();
        foreach (string term in terms)
        {
            string normalized = term.TrimStart('+', ' ').Trim();
            if (normalized.Length > 0 && !seen.Add(normalized))
                return true;
        }
        return false;
    }
}

/// <summary>Describes a detected structural pattern in an expression.</summary>
public sealed class ExpressionPattern
{
    /// <summary>Gets the pattern type name (e.g., Polynomial, Exponential).</summary>
    public string Type { get; init; } = "";

    /// <summary>Gets a human-readable description of the pattern.</summary>
    public string Description { get; init; } = "";
}
