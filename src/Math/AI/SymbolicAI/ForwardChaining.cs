namespace MathVerse.Math.AI.SymbolicAI;

using System;
using System.Collections.Generic;

/// <summary>Forward chaining inference engine that derives new facts from known facts and rules.</summary>
public sealed class ForwardChaining
{
    private readonly HashSet<string> _facts = new();
    private readonly List<ForwardRule> _rules = new();

    /// <summary>Gets the number of known facts.</summary>
    public int FactCount => _facts.Count;

    /// <summary>Gets the number of rules.</summary>
    public int RuleCount => _rules.Count;

    /// <summary>Adds a fact to the knowledge base.</summary>
    /// <param name="fact">The fact string to add.</param>
    public void AddFact(string fact)
    {
        if (string.IsNullOrEmpty(fact))
            throw new ArgumentException("Fact cannot be null or empty.", nameof(fact));
        _facts.Add(fact);
    }

    /// <summary>Adds a rule for forward chaining.</summary>
    /// <param name="rule">The forward rule.</param>
    public void AddRule(ForwardRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        _rules.Add(rule);
    }

    /// <summary>Runs forward chaining to derive all possible new facts.</summary>
    /// <returns>Set of all newly derived facts (not including the initial facts).</returns>
    public HashSet<string> Run()
    {
        HashSet<string> newFacts = new();
        bool changed = true;

        while (changed)
        {
            changed = false;
            foreach (ForwardRule rule in _rules)
            {
                List<string> results = rule.Apply(_facts);
                foreach (string result in results)
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

    /// <summary>Runs forward chaining for a limited number of iterations.</summary>
    /// <param name="maxIterations">Maximum number of passes through all rules.</param>
    /// <returns>Set of all newly derived facts.</returns>
    public HashSet<string> Run(int maxIterations)
    {
        if (maxIterations < 0)
            throw new ArgumentException("Max iterations must be non-negative.", nameof(maxIterations));

        HashSet<string> newFacts = new();

        for (int iter = 0; iter < maxIterations; iter++)
        {
            bool changed = false;
            foreach (ForwardRule rule in _rules)
            {
                List<string> results = rule.Apply(_facts);
                foreach (string result in results)
                {
                    if (!_facts.Contains(result))
                    {
                        _facts.Add(result);
                        newFacts.Add(result);
                        changed = true;
                    }
                }
            }
            if (!changed)
                break;
        }

        return newFacts;
    }

    /// <summary>Checks whether a specific fact can be derived.</summary>
    /// <param name="goal">The fact to check.</param>
    /// <returns>True if the fact is derivable.</returns>
    public bool CanDerive(string goal)
    {
        HashSet<string> snapshot = new(_facts);
        HashSet<string> derived = Run();
        bool result = _facts.Contains(goal);
        _facts.IntersectWith(snapshot);
        return result || derived.Contains(goal);
    }

    /// <summary>Returns all currently known facts.</summary>
    /// <returns>ReadOnly set of facts.</returns>
    public IReadOnlyCollection<string> GetAllFacts() => _facts.ToList().AsReadOnly();
}

/// <summary>A forward chaining rule with premises and a conclusion.</summary>
public sealed class ForwardRule
{
    /// <summary>Gets the name of the rule.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the required premises (facts that must be present).</summary>
    public List<string> Premises { get; init; } = new();

    /// <summary>Gets the conclusion to derive when all premises are met.</summary>
    public string Conclusion { get; init; } = "";

    /// <summary>Gets an optional custom derivation function.</summary>
    public Func<HashSet<string>, List<string>> CustomDerive { get; init; } = _ => new();

    /// <summary>Applies the rule to the known facts and returns any derivable results.</summary>
    /// <param name="facts">Set of known facts.</param>
    /// <returns>List of facts derived by this rule.</returns>
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

        results.AddRange(CustomDerive(facts));
        return results;
    }
}
