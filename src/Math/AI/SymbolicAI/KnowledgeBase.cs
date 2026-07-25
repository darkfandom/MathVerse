namespace MathVerse.Math.AI.SymbolicAI;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Stores mathematical facts and rules with forward and backward chaining inference.</summary>
public sealed class KnowledgeBase
{
    private readonly HashSet<string> _facts = new();
    private readonly List<MathRule> _rules = new();

    /// <summary>Gets the number of known facts.</summary>
    public int FactCount => _facts.Count;

    /// <summary>Gets the number of rules.</summary>
    public int RuleCount => _rules.Count;

    /// <summary>Adds a fact to the knowledge base.</summary>
    /// <param name="fact">The mathematical fact string.</param>
    public void AddFact(string fact)
    {
        if (string.IsNullOrEmpty(fact))
            throw new ArgumentException("Fact cannot be null or empty.", nameof(fact));
        _facts.Add(fact);
    }

    /// <summary>Adds a rule to the knowledge base.</summary>
    /// <param name="rule">The mathematical rule.</param>
    public void AddRule(MathRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        _rules.Add(rule);
    }

    /// <summary>Checks whether a fact is known.</summary>
    /// <param name="fact">The fact to check.</param>
    /// <returns>True if the fact is known.</returns>
    public bool Knows(string fact) => _facts.Contains(fact);

    /// <summary>Retrieves all known facts.</summary>
    /// <returns>ReadOnly collection of facts.</returns>
    public IReadOnlyCollection<string> GetAllFacts() => _facts.ToList().AsReadOnly();

    /// <summary>Applies forward chaining: repeatedly applies rules to known facts to derive new facts until no new facts are produced.</summary>
    /// <returns>Set of all newly derived facts.</returns>
    public HashSet<string> ForwardChain()
    {
        HashSet<string> newFacts = new();
        bool changed = true;

        while (changed)
        {
            changed = false;
            foreach (MathRule rule in _rules)
            {
                foreach (string result in rule.Apply(_facts))
                {
                    if (!_facts.Contains(result))
                    {
                        _facts.Add(result);
                        newFacts.Add(result);
                        changed = true;
                    }
                }
            }
        }

        return newFacts;
    }

    /// <summary>Uses backward chaining to determine if a goal can be derived from known facts.</summary>
    /// <param name="goal">The goal fact to prove.</param>
    /// <returns>True if the goal can be derived.</returns>
    public bool BackwardChain(string goal)
    {
        if (_facts.Contains(goal))
            return true;

        HashSet<string> visited = new();
        return Prove(goal, visited);
    }

    /// <summary>Finds the derivation path for a goal using backward chaining.</summary>
    /// <param name="goal">The goal to prove.</param>
    /// <returns>List of steps in the derivation, or empty if unprovable.</returns>
    public List<string> FindDerivation(string goal)
    {
        List<string> path = new();
        HashSet<string> visited = new();
        if (ProveWithTrace(goal, visited, path))
            return path;
        return new List<string>();
    }

    /// <summary>Pattern matches facts against a template string.</summary>
    /// <param name="template">Template with '*' wildcards.</param>
    /// <returns>Facts matching the template.</returns>
    public List<string> PatternMatch(string template)
    {
        if (string.IsNullOrEmpty(template))
            throw new ArgumentException("Template cannot be null or empty.", nameof(template));

        List<string> matches = new();
        string pattern = "^" + System.Text.RegularExpressions.Regex.Escape(template).Replace("\\*", ".*") + "$";
        var regex = new System.Text.RegularExpressions.Regex(pattern);

        foreach (string fact in _facts)
        {
            if (regex.IsMatch(fact))
                matches.Add(fact);
        }
        return matches;
    }

    /// <summary>Removes a fact from the knowledge base.</summary>
    /// <param name="fact">The fact to remove.</param>
    /// <returns>True if the fact was found and removed.</returns>
    public bool RemoveFact(string fact) => _facts.Remove(fact);

    /// <summary>Clears all facts and rules.</summary>
    public void Clear()
    {
        _facts.Clear();
        _rules.Clear();
    }

    private bool Prove(string goal, HashSet<string> visited)
    {
        if (_facts.Contains(goal))
            return true;
        if (!visited.Add(goal))
            return false;

        foreach (MathRule rule in _rules)
        {
            if (rule.ConclusionMatches(goal))
            {
                bool allPremisesProven = true;
                foreach (string premise in rule.Premises)
                {
                    if (!Prove(premise, visited))
                    {
                        allPremisesProven = false;
                        break;
                    }
                }

                if (allPremisesProven)
                    return true;
            }
        }

        return false;
    }

    private bool ProveWithTrace(string goal, HashSet<string> visited, List<string> path)
    {
        if (_facts.Contains(goal))
        {
            path.Add($"Known: {goal}");
            return true;
        }
        if (!visited.Add(goal))
            return false;

        foreach (MathRule rule in _rules)
        {
            if (rule.ConclusionMatches(goal))
            {
                bool allProven = true;
                foreach (string premise in rule.Premises)
                {
                    if (!ProveWithTrace(premise, visited, path))
                    {
                        allProven = false;
                        break;
                    }
                }

                if (allProven)
                {
                    path.Add($"Apply {rule.Name}: {string.Join(", ", rule.Premises)} => {goal}");
                    return true;
                }
            }
        }

        return false;
    }
}

/// <summary>A mathematical inference rule with premises and a conclusion.</summary>
public sealed class MathRule
{
    /// <summary>Gets the name of the rule.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the list of premises (antecedents) required.</summary>
    public List<string> Premises { get; init; } = new();

    /// <summary>Gets the conclusion (consequent) of the rule.</summary>
    public string Conclusion { get; init; } = "";

    /// <summary>Gets the transformation function that produces derived facts from existing facts.</summary>
    public Func<HashSet<string>, List<string>> Derive { get; init; } = _ => new();

    /// <summary>Applies the rule to the known facts and returns all derivable results.</summary>
    /// <param name="facts">Set of known facts.</param>
    /// <returns>List of new facts that can be derived.</returns>
    public List<string> Apply(HashSet<string> facts)
    {
        List<string> results = new();
        bool allMet = true;
        foreach (string premise in Premises)
        {
            if (!facts.Contains(premise))
            {
                allMet = false;
                break;
            }
        }

        if (allMet && !string.IsNullOrEmpty(Conclusion))
            results.Add(Conclusion);

        results.AddRange(Derive(facts));
        return results;
    }

    /// <summary>Checks whether the rule's conclusion matches a given goal.</summary>
    /// <param name="goal">The goal to match against.</param>
    /// <returns>True if the conclusion matches.</returns>
    public bool ConclusionMatches(string goal) => Conclusion == goal;
}
