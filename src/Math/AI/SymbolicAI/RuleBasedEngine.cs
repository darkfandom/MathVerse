namespace MathVerse.Math.AI.SymbolicAI;

using System;
using System.Collections.Generic;

/// <summary>Rule-based inference engine for mathematical reasoning and simplification.</summary>
public sealed class RuleBasedEngine
{
    private readonly List<InferenceRule> _rules = new();

    /// <summary>Adds an inference rule to the engine.</summary>
    /// <param name="rule">The inference rule to add.</param>
    public void AddRule(InferenceRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        _rules.Add(rule);
    }

    /// <summary>Gets the number of loaded rules.</summary>
    public int RuleCount => _rules.Count;

    /// <summary>Tries each rule on the expression and returns all new expressions produced.</summary>
    /// <param name="expression">The input mathematical expression.</param>
    /// <returns>List of transformed expressions.</returns>
    public List<string> ApplyRules(string expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        List<string> results = new();
        foreach (InferenceRule rule in _rules)
        {
            if (rule.IsApplicable(expression))
            {
                string? transformed = rule.Transform(expression);
                if (transformed != null && transformed != expression)
                    results.Add(transformed);
            }
        }
        return results;
    }

    /// <summary>Checks if any rule can simplify the given expression.</summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>True if at least one rule is applicable; otherwise false.</returns>
    public bool CanSimplify(string expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        foreach (InferenceRule rule in _rules)
        {
            if (rule.IsApplicable(expression))
                return true;
        }
        return false;
    }

    /// <summary>Applies rules iteratively to simplify an expression until no more rules apply or max steps reached.</summary>
    /// <param name="expression">The expression to simplify.</param>
    /// <param name="maxSteps">Maximum number of simplification steps.</param>
    /// <returns>The simplified expression.</returns>
    public string Simplify(string expression, int maxSteps = 10)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        if (maxSteps < 0)
            throw new ArgumentException("Max steps must be non-negative.", nameof(maxSteps));

        string current = expression;
        for (int step = 0; step < maxSteps; step++)
        {
            bool applied = false;
            foreach (InferenceRule rule in _rules)
            {
                if (rule.IsApplicable(current))
                {
                    string? transformed = rule.Transform(current);
                    if (transformed != null && transformed != current)
                    {
                        current = transformed;
                        applied = true;
                        break;
                    }
                }
            }
            if (!applied)
                break;
        }
        return current;
    }

    /// <summary>Attempts to apply the first matching rule to the expression.</summary>
    /// <param name="expression">The expression to transform.</param>
    /// <returns>Tuple of (success, result expression, rule name).</returns>
    public (bool Success, string Result, string RuleName) ApplyFirst(string expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        foreach (InferenceRule rule in _rules)
        {
            if (rule.IsApplicable(expression))
            {
                string? transformed = rule.Transform(expression);
                if (transformed != null)
                    return (true, transformed, rule.Name);
            }
        }
        return (false, expression, "");
    }
}

/// <summary>A named mathematical inference rule with a transformation function.</summary>
public sealed class InferenceRule
{
    /// <summary>Gets the name of the rule.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the transformation function. Returns null if not applicable.</summary>
    public Func<string, string?> Transform { get; init; } = _ => null;

    /// <summary>Checks whether the rule is applicable to the given expression.</summary>
    /// <param name="expression">The mathematical expression.</param>
    /// <returns>True if the rule can be applied.</returns>
    public bool IsApplicable(string expression) => Transform(expression) != null;
}
