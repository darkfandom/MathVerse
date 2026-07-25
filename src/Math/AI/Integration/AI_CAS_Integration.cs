namespace MathVerse.Math.AI.Integration;

using System.Collections.Immutable;

/// <summary>Intelligent integration between AI and Computer Algebra System for expression simplification and rewriting.</summary>
public sealed class AICASIntegration
{
    private static readonly ImmutableDictionary<string, double> RuleWeights = ImmutableDictionary<string, double>.Empty
        .Add("PowerSimplify", 0.85)
        .Add("TrigIdentity", 0.8)
        .Add("LogCombine", 0.75)
        .Add("FactorCommon", 0.7)
        .Add("ExpandProducts", 0.65)
        .Add("CancelCommon", 0.9)
        .Add("PartialFractions", 0.6)
        .Add("Substitution", 0.55)
        .Add("Rationalize", 0.5)
        .Add("CombineFractions", 0.72);

    /// <summary>Ranks CAS simplification rules by expected reduction for the given expression.</summary>
    /// <param name="expression">The mathematical expression to analyze.</param>
    /// <returns>A list of rule names ordered by expected reduction benefit, highest first.</returns>
    public List<string> RankSimplificationRules(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return [];
        }

        var scores = new List<(string rule, double score)>();

        foreach (var kvp in RuleWeights)
        {
            double relevance = ComputeRuleRelevance(kvp.Key, expression);
            double combinedScore = kvp.Value * relevance;

            if (combinedScore > 0.01)
            {
                scores.Add((kvp.Key, combinedScore));
            }
        }

        scores.Sort((a, b) => b.score.CompareTo(a.score));

        var result = new List<string>();
        foreach (var s in scores)
        {
            result.Add(s.rule);
        }

        return result;
    }

    /// <summary>Predicts the optimal simplification strategy for the given expression.</summary>
    /// <param name="expression">The mathematical expression to simplify.</param>
    /// <returns>The name of the recommended CAS strategy.</returns>
    public string PredictOptimalSimplification(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return "Simplify";
        }

        string upper = expression.ToUpperInvariant();

        int trigCount = CountOccurrences(upper, "SIN") + CountOccurrences(upper, "COS") + CountOccurrences(upper, "TAN");
        int logCount = CountOccurrences(upper, "LOG") + CountOccurrences(upper, "LN");
        int expCount = CountOccurrences(upper, "EXP");
        int polyCount = CountOccurrences(upper, "^") + CountOccurrences(upper, "*");
        int rationalCount = CountOccurrences(upper, "/");

        int maxCount = trigCount;
        string bestStrategy = "TrigSimplify";

        if (logCount + expCount > maxCount)
        {
            maxCount = logCount + expCount;
            bestStrategy = "LogExpSimplify";
        }

        if (polyCount > maxCount)
        {
            maxCount = polyCount;
            bestStrategy = "FactorPolynomials";
        }

        if (rationalCount > maxCount)
        {
            maxCount = rationalCount;
            bestStrategy = "RationalCombine";
        }

        if (maxCount == 0)
        {
            return "AlgebraicSimplify";
        }

        return bestStrategy;
    }

    /// <summary>Predicts a chain of rewrite steps to transform an expression into a target form.</summary>
    /// <param name="expression">The source expression.</param>
    /// <param name="targetForm">A description of the desired target form (e.g., "factored", "expanded", "simplified").</param>
    /// <returns>An ordered list of rewrite step descriptions.</returns>
    public List<string> PredictRewriteSteps(string expression, string targetForm)
    {
        var steps = new List<string>();

        if (string.IsNullOrWhiteSpace(expression) || string.IsNullOrWhiteSpace(targetForm))
        {
            return steps;
        }

        string upper = expression.ToUpperInvariant();
        string target = targetForm.ToUpperInvariant();

        if (target.Contains("FACTOR", StringComparison.Ordinal))
        {
            if (HasAddition(upper))
            {
                steps.Add("Identify common factors in additive terms");
                steps.Add("Apply GCD-based factoring");
                steps.Add("Factor out greatest common divisor");
            }

            if (HasPowers(upper))
            {
                steps.Add("Apply difference of squares where applicable");
                steps.Add("Apply sum/difference of cubes patterns");
            }
        }
        else if (target.Contains("EXPAND", StringComparison.Ordinal))
        {
            if (HasProducts(upper))
            {
                steps.Add("Distribute multiplication over addition");
                steps.Add("Apply binomial expansion for power terms");
                steps.Add("Collect and combine like terms");
            }

            if (HasPowers(upper))
            {
                steps.Add("Expand power expressions using binomial theorem");
            }
        }
        else if (target.Contains("SIMPLIF", StringComparison.Ordinal))
        {
            steps.Add("Combine like terms");
            steps.Add("Cancel common factors in fractions");
            steps.Add("Apply algebraic identities");
            steps.Add("Reduce to canonical form");
        }
        else if (target.Contains("TRIG", StringComparison.Ordinal))
        {
            if (HasTrigFunctions(upper))
            {
                steps.Add("Apply Pythagorean identities");
                steps.Add("Convert to standard angle form");
                steps.Add("Use double-angle or half-angle identities");
                steps.Add("Collect trigonometric terms");
            }
        }
        else if (target.Contains("PARTIAL", StringComparison.Ordinal))
        {
            if (HasRationalExpression(upper))
            {
                steps.Add("Ensure proper fraction form via polynomial long division");
                steps.Add("Factor the denominator completely");
                steps.Add("Set up partial fraction decomposition template");
                steps.Add("Solve for coefficients via undetermined coefficients");
            }
        }

        if (steps.Count == 0)
        {
            steps.Add($"Normalize expression for {targetForm} form");
            steps.Add("Apply standard algebraic simplification");
        }

        return steps;
    }

    /// <summary>Selects the best CAS strategy based on expression analysis.</summary>
    /// <param name="expression">The mathematical expression to analyze.</param>
    /// <returns>The name of the selected CAS strategy.</returns>
    public string SelectStrategy(string expression)
    {
        return PredictOptimalSimplification(expression);
    }

    /// <summary>Computes how relevant a specific rule is for the given expression.</summary>
    /// <param name="ruleName">The rule name.</param>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns>A relevance score between 0 and 1.</returns>
    private static double ComputeRuleRelevance(string ruleName, string expression)
    {
        string upper = expression.ToUpperInvariant();

        return ruleName switch
        {
            "PowerSimplify" => HasPowers(upper) ? 0.8 + CountOccurrences(upper, "^") * 0.05 : 0.0,
            "TrigIdentity" => HasTrigFunctions(upper) ? 0.85 : 0.0,
            "LogCombine" => HasLogFunctions(upper) ? 0.8 : 0.0,
            "FactorCommon" => HasAddition(upper) && HasProducts(upper) ? 0.75 : 0.1,
            "ExpandProducts" => HasProducts(upper) && HasAddition(upper) ? 0.7 : 0.1,
            "CancelCommon" => HasRationalExpression(upper) ? 0.9 : 0.0,
            "PartialFractions" => HasRationalExpression(upper) ? 0.65 : 0.0,
            "Substitution" => HasNestedExpressions(upper) ? 0.5 : 0.1,
            "Rationalize" => upper.Contains("SQRT", StringComparison.Ordinal) && HasRationalExpression(upper) ? 0.7 : 0.0,
            "CombineFractions" => CountOccurrences(upper, "/") > 1 ? 0.75 : 0.0,
            _ => 0.1
        };
    }

    private static bool HasPowers(string upper) => upper.Contains('^');
    private static bool HasAddition(string upper) => upper.Contains('+');
    private static bool HasProducts(string upper) => upper.Contains('*');
    private static bool HasRationalExpression(string upper) => upper.Contains('/');
    private static bool HasNestedExpressions(string upper) => upper.Contains('(');
    private static bool HasTrigFunctions(string upper) =>
        upper.Contains("SIN") || upper.Contains("COS") || upper.Contains("TAN") ||
        upper.Contains("SEC") || upper.Contains("CSC") || upper.Contains("COT");

    private static bool HasLogFunctions(string upper) => upper.Contains("LOG") || upper.Contains("LN");

    private static int CountOccurrences(string source, string substring)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(substring, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += substring.Length;
        }
        return count;
    }
}
