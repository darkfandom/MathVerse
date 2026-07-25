namespace MathVerse.Math.AI.Recommendation;

using System.Collections.Immutable;

/// <summary>Recommends CAS simplification strategies based on expression pattern analysis.</summary>
public sealed class SimplificationRecommendationEngine
{
    private readonly List<SimplificationRuleProfile> _rules = [];

    /// <summary>Initializes a new instance of the <see cref="SimplificationRecommendationEngine"/> class with built-in rules.</summary>
    public SimplificationRecommendationEngine()
    {
        RegisterRule(new SimplificationRuleProfile
        {
            Name = "ExpandPolynomials",
            Category = "Expansion",
            Triggers = ["^", "*", "+"],
            NegativeTriggers = ["sin", "cos", "exp"],
            ScorePerTrigger = 0.3,
            OverheadCost = 0.1,
            Description = "Expand polynomial products and powers"
        });

        RegisterRule(new SimplificationRuleProfile
        {
            Name = "FactorCommon",
            Category = "Factoring",
            Triggers = ["gcd", "common", "+", "*"],
            NegativeTriggers = [],
            ScorePerTrigger = 0.35,
            OverheadCost = 0.15,
            Description = "Factor out common subexpressions"
        });

        RegisterRule(new SimplificationRuleProfile
        {
            Name = "TrigSimplify",
            Category = "Trigonometric",
            Triggers = ["sin", "cos", "tan", "sec", "csc", "cot"],
            NegativeTriggers = [],
            ScorePerTrigger = 0.4,
            OverheadCost = 0.2,
            Description = "Apply trigonometric identities and Pythagorean reductions"
        });

        RegisterRule(new SimplificationRuleProfile
        {
            Name = "LogExpSimplify",
            Category = "Logarithmic",
            Triggers = ["log", "ln", "exp"],
            NegativeTriggers = [],
            ScorePerTrigger = 0.45,
            OverheadCost = 0.15,
            Description = "Simplify logarithmic and exponential expressions"
        });

        RegisterRule(new SimplificationRuleProfile
        {
            Name = "RationalCombine",
            Category = "Rational",
            Triggers = ["/", "+", "-"],
            NegativeTriggers = [],
            ScorePerTrigger = 0.25,
            OverheadCost = 0.2,
            Description = "Combine rational fractions over a common denominator"
        });

        RegisterRule(new SimplificationRuleProfile
        {
            Name = "PowerRules",
            Category = "Power",
            Triggers = ["^"],
            NegativeTriggers = [],
            ScorePerTrigger = 0.5,
            OverheadCost = 0.1,
            Description = "Apply power rules: a^m * a^n = a^(m+n), (a^m)^n = a^(mn)"
        });

        RegisterRule(new SimplificationRuleProfile
        {
            Name = "ConstantFold",
            Category = "ConstantFolding",
            Triggers = ["num", "num", "+", "-", "*", "/", "^"],
            NegativeTriggers = [],
            ScorePerTrigger = 0.6,
            OverheadCost = 0.05,
            Description = "Evaluate constant sub-expressions numerically"
        });

        RegisterRule(new SimplificationRuleProfile
        {
            Name = "Substitution",
            Category = "Substitution",
            Triggers = ["let", "sub", "repeat"],
            NegativeTriggers = [],
            ScorePerTrigger = 0.3,
            OverheadCost = 0.25,
            Description = "Apply variable substitution to reduce complexity"
        });
    }

    /// <summary>Registers a new simplification rule profile.</summary>
    /// <param name="profile">The rule profile to register.</param>
    public void RegisterRule(SimplificationRuleProfile profile)
    {
        _rules.Add(profile);
    }

    /// <summary>Recommends simplification strategies for the given expression, ranked by expected reduction benefit.</summary>
    /// <param name="expression">The mathematical expression string to analyze.</param>
    /// <returns>A list of simplification recommendations ordered by descending score.</returns>
    public List<SimplificationRecommendation> Recommend(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return [];
        }

        string normalized = expression.ToUpperInvariant();
        var detectedTokens = Tokenize(normalized);
        var results = new List<SimplificationRecommendation>();

        foreach (var rule in _rules)
        {
            double score = ScoreRule(rule, detectedTokens);
            if (score <= 0.0)
                continue;

            string reason = BuildReason(rule, detectedTokens, score);

            results.Add(new SimplificationRecommendation
            {
                RuleName = rule.Name,
                Category = rule.Category,
                Score = score,
                Reason = reason,
                Description = rule.Description
            });
        }

        results.Sort((a, b) => b.Score.CompareTo(a.Score));
        return results;
    }

    /// <summary>Tokenizes an uppercased expression into recognizable math tokens.</summary>
    /// <param name="expression">Uppercased expression string.</param>
    /// <returns>A set of detected tokens.</returns>
    private static HashSet<string> Tokenize(string expression)
    {
        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string[] knownTokens = ["SIN", "COS", "TAN", "SEC", "CSC", "COT", "LOG", "LN", "EXP",
            "POW", "SQRT", "ABS", "PI", "E", "I"];

        foreach (var token in knownTokens)
        {
            if (expression.Contains(token, StringComparison.Ordinal))
            {
                tokens.Add(token);
            }
        }

        string[] operators = ["^", "*", "/", "+", "-"];
        foreach (var op in operators)
        {
            if (expression.Contains(op, StringComparison.Ordinal))
            {
                tokens.Add(op);
            }
        }

        bool hasNumbers = false;
        for (int i = 0; i < expression.Length; i++)
        {
            if (char.IsDigit(expression[i]))
            {
                hasNumbers = true;
                break;
            }
        }
        if (hasNumbers)
        {
            tokens.Add("num");
        }

        if (expression.Contains('(', StringComparison.Ordinal))
        {
            tokens.Add("paren");
        }

        return tokens;
    }

    /// <summary>Computes a score for a rule based on how well its triggers match detected tokens.</summary>
    /// <param name="rule">The rule profile to score.</param>
    /// <param name="detectedTokens">Tokens detected in the expression.</param>
    /// <returns>A score between 0 and 1.</returns>
    private static double ScoreRule(SimplificationRuleProfile rule, HashSet<string> detectedTokens)
    {
        double positiveScore = 0.0;
        int positiveMatches = 0;

        foreach (var trigger in rule.Triggers)
        {
            if (detectedTokens.Contains(trigger))
            {
                positiveScore += rule.ScorePerTrigger;
                positiveMatches++;
            }
        }

        double negativePenalty = 0.0;
        foreach (var neg in rule.NegativeTriggers)
        {
            if (detectedTokens.Contains(neg))
            {
                negativePenalty += 0.2;
            }
        }

        if (positiveMatches == 0)
        {
            return 0.0;
        }

        double rawScore = positiveScore - negativePenalty - rule.OverheadCost;
        double normalizedScore = System.Math.Max(0.0, System.Math.Min(1.0, rawScore));
        return normalizedScore;
    }

    /// <summary>Builds an explanation for why a rule was recommended.</summary>
    /// <param name="rule">The rule profile.</param>
    /// <param name="detectedTokens">Tokens detected in the expression.</param>
    /// <param name="score">Computed score.</param>
    /// <returns>An explanation string.</returns>
    private static string BuildReason(SimplificationRuleProfile rule, HashSet<string> detectedTokens, double score)
    {
        var matched = new List<string>();
        foreach (var trigger in rule.Triggers)
        {
            if (detectedTokens.Contains(trigger))
            {
                matched.Add(trigger);
            }
        }

        string quality = score switch
        {
            >= 0.7 => "high",
            >= 0.4 => "moderate",
            _ => "low"
        };

        return $"{rule.Description}. Matched tokens: [{string.Join(", ", matched)}]. Expected benefit: {quality}.";
    }
}

/// <summary>Describes a simplification rule's triggers, cost, and applicability.</summary>
public sealed class SimplificationRuleProfile
{
    /// <summary>Gets the rule name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the rule category.</summary>
    public string Category { get; init; } = "";

    /// <summary>Gets the tokens that positively indicate this rule should apply.</summary>
    public List<string> Triggers { get; init; } = [];

    /// <summary>Gets the tokens that penalize this rule's applicability.</summary>
    public List<string> NegativeTriggers { get; init; } = [];

    /// <summary>Gets the score contribution per matched positive trigger.</summary>
    public double ScorePerTrigger { get; init; }

    /// <summary>Gets the computational overhead cost subtracted from the score.</summary>
    public double OverheadCost { get; init; }

    /// <summary>Gets a human-readable description of what the rule does.</summary>
    public string Description { get; init; } = "";
}

/// <summary>Represents a ranked simplification recommendation.</summary>
public sealed class SimplificationRecommendation
{
    /// <summary>Gets the rule name.</summary>
    public string RuleName { get; init; } = "";

    /// <summary>Gets the rule category.</summary>
    public string Category { get; init; } = "";

    /// <summary>Gets the expected benefit score between 0 and 1.</summary>
    public double Score { get; init; }

    /// <summary>Gets a human-readable explanation for the recommendation.</summary>
    public string Reason { get; init; } = "";

    /// <summary>Gets a description of what the simplification rule does.</summary>
    public string Description { get; init; } = "";
}
