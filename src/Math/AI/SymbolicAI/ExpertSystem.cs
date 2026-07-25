namespace MathVerse.Math.AI.SymbolicAI;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Mathematical expert system with confidence factors for probabilistic reasoning.</summary>
public sealed class ExpertSystem
{
    private readonly List<ExpertRule> _rules = new();
    private readonly Dictionary<string, double> _knownFacts = new();

    /// <summary>Gets the number of loaded rules.</summary>
    public int RuleCount => _rules.Count;

    /// <summary>Gets the number of known facts.</summary>
    public int FactCount => _knownFacts.Count;

    /// <summary>Adds an expert rule to the system.</summary>
    /// <param name="rule">The expert rule with confidence factor.</param>
    public void AddRule(ExpertRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        _rules.Add(rule);
    }

    /// <summary>Asserts a fact with a confidence level.</summary>
    /// <param name="fact">The fact string.</param>
    /// <param name="confidence">Confidence level from 0.0 to 1.0.</param>
    public void AssertFact(string fact, double confidence)
    {
        if (string.IsNullOrEmpty(fact))
            throw new ArgumentException("Fact cannot be null or empty.", nameof(fact));
        if (confidence < 0.0 || confidence > 1.0)
            throw new ArgumentException("Confidence must be between 0.0 and 1.0.", nameof(confidence));

        _knownFacts[fact] = confidence;
    }

    /// <summary>Retrieves the confidence of a known fact.</summary>
    /// <param name="fact">The fact to look up.</param>
    /// <returns>Confidence value, or 0.0 if not known.</returns>
    public double GetConfidence(string fact)
    {
        return _knownFacts.TryGetValue(fact, out double conf) ? conf : 0.0;
    }

    /// <summary>Diagnoses a problem description and returns ranked suggestions with confidence.</summary>
    /// <param name="problemDescription">Description of the mathematical problem.</param>
    /// <returns>List of (suggestion, confidence) pairs ranked by confidence.</returns>
    public List<(string Suggestion, double Confidence)> Diagnose(string problemDescription)
    {
        if (string.IsNullOrEmpty(problemDescription))
            throw new ArgumentException("Problem description cannot be null or empty.", nameof(problemDescription));

        List<(string, double)> suggestions = new();
        HashSet<string> activated = new();

        bool changed = true;
        int maxIterations = 100;
        int iter = 0;

        while (changed && iter < maxIterations)
        {
            changed = false;
            iter++;

            foreach (ExpertRule rule in _rules)
            {
                if (activated.Contains(rule.Name))
                    continue;

                if (rule.MatchesProblem(problemDescription, _knownFacts))
                {
                    double chainConfidence = ComputeChainConfidence(rule);
                    if (chainConfidence > 0.0)
                    {
                        suggestions.Add((rule.Conclusion, chainConfidence));
                        _knownFacts[rule.Conclusion] = chainConfidence;
                        activated.Add(rule.Name);
                        changed = true;
                    }
                }
            }
        }

        suggestions.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        return suggestions;
    }

    /// <summary>Computes the combined confidence through a chain of rules.</summary>
    /// <param name="rule">The rule whose chain confidence to compute.</param>
    /// <returns>Combined confidence value.</returns>
    public double ComputeChainConfidence(ExpertRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        double minConfidence = rule.Confidence;
        foreach (string premise in rule.Premises)
        {
            if (_knownFacts.TryGetValue(premise, out double premConf))
            {
                if (premConf < minConfidence)
                    minConfidence = premConf;
            }
            else
            {
                return 0.0;
            }
        }

        return minConfidence;
    }

    /// <summary>Retrieves all rules that can be triggered by the current known facts.</summary>
    /// <returns>List of applicable rules with their computed confidences.</returns>
    public List<(ExpertRule Rule, double Confidence)> GetApplicableRules()
    {
        List<(ExpertRule, double)> applicable = new();
        foreach (ExpertRule rule in _rules)
        {
            double conf = ComputeChainConfidence(rule);
            if (conf > 0.0)
                applicable.Add((rule, conf));
        }
        return applicable;
    }

    /// <summary>Clears all known facts.</summary>
    public void ClearFacts() => _knownFacts.Clear();
}

/// <summary>An expert system rule with premises, conclusion, and confidence factor.</summary>
public sealed class ExpertRule
{
    /// <summary>Gets the unique name of the rule.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the list of premise fact strings required.</summary>
    public List<string> Premises { get; init; } = new();

    /// <summary>Gets the conclusion fact produced by this rule.</summary>
    public string Conclusion { get; init; } = "";

    /// <summary>Gets the base confidence factor for this rule (0.0 to 1.0).</summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>Gets the list of keywords for problem matching.</summary>
    public List<string> Keywords { get; init; } = new();

    /// <summary>Checks whether the rule matches a problem description and the known facts satisfy its premises.</summary>
    /// <param name="problemDescription">The problem description to match against keywords.</param>
    /// <param name="knownFacts">Currently known facts with confidence levels.</param>
    /// <returns>True if the rule is applicable.</returns>
    public bool MatchesProblem(string problemDescription, Dictionary<string, double> knownFacts)
    {
        if (string.IsNullOrEmpty(problemDescription))
            return false;

        bool keywordMatch = Keywords.Count == 0;
        foreach (string keyword in Keywords)
        {
            if (problemDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                keywordMatch = true;
                break;
            }
        }

        if (!keywordMatch)
            return false;

        foreach (string premise in Premises)
        {
            if (!knownFacts.ContainsKey(premise))
                return false;
        }

        return true;
    }
}
