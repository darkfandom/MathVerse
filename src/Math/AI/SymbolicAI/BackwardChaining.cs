namespace MathVerse.Math.AI.SymbolicAI;

using System;
using System.Collections.Generic;

/// <summary>Backward chaining inference engine that works backward from goals to find supporting facts.</summary>
public sealed class BackwardChaining
{
    private readonly HashSet<string> _facts = new();
    private readonly List<BackwardRule> _rules = new();

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

    /// <summary>Adds a backward chaining rule.</summary>
    /// <param name="rule">The backward rule.</param>
    public void AddRule(BackwardRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        _rules.Add(rule);
    }

    /// <summary>Attempts to prove a goal using backward chaining.</summary>
    /// <param name="goal">The goal fact to prove.</param>
    /// <returns>True if the goal can be proven from known facts and rules.</returns>
    public bool Prove(string goal)
    {
        if (string.IsNullOrEmpty(goal))
            throw new ArgumentException("Goal cannot be null or empty.", nameof(goal));

        HashSet<string> visited = new();
        return ProveGoal(goal, visited);
    }

    /// <summary>Proves a goal and returns the proof trace.</summary>
    /// <param name="goal">The goal fact to prove.</param>
    /// <returns>Tuple of (proved, proof trace steps).</returns>
    public (bool Proved, List<string> Trace) ProveWithTrace(string goal)
    {
        if (string.IsNullOrEmpty(goal))
            throw new ArgumentException("Goal cannot be null or empty.", nameof(goal));

        List<string> trace = new();
        HashSet<string> visited = new();
        bool proved = ProveGoalWithTrace(goal, visited, trace);
        return (proved, trace);
    }

    /// <summary>Returns the set of all goals that can be proven.</summary>
    /// <returns>Set of provable goal strings.</returns>
    public HashSet<string> ProvableGoals()
    {
        HashSet<string> provable = new(_facts);
        bool changed = true;

        while (changed)
        {
            changed = false;
            foreach (BackwardRule rule in _rules)
            {
                if (provable.Contains(rule.Conclusion))
                    continue;

                bool allMet = true;
                foreach (string premise in rule.Premises)
                {
                    if (!provable.Contains(premise))
                    {
                        allMet = false;
                        break;
                    }
                }

                if (allMet)
                {
                    provable.Add(rule.Conclusion);
                    changed = true;
                }
            }
        }

        return provable;
    }

    /// <summary>Finds the shortest proof for a goal.</summary>
    /// <param name="goal">The goal to prove.</param>
    /// <returns>List of rule names in the proof chain, or empty if unprovable.</returns>
    public List<string> ShortestProof(string goal)
    {
        if (string.IsNullOrEmpty(goal))
            throw new ArgumentException("Goal cannot be null or empty.", nameof(goal));

        Dictionary<string, int> depth = new();
        foreach (string fact in _facts)
            depth[fact] = 0;

        Dictionary<string, List<string>> proofChains = new();
        bool changed = true;

        while (changed)
        {
            changed = false;
            foreach (BackwardRule rule in _rules)
            {
                if (depth.ContainsKey(rule.Conclusion))
                    continue;

                int maxDepth = -1;
                bool allMet = true;
                List<string> chain = new();

                foreach (string premise in rule.Premises)
                {
                    if (!depth.TryGetValue(premise, out int d))
                    {
                        allMet = false;
                        break;
                    }
                    if (d > maxDepth)
                        maxDepth = d;
                    chain.Add(premise);
                }

                if (allMet)
                {
                    depth[rule.Conclusion] = maxDepth + 1;
                    chain.Add(rule.Name);
                    proofChains[rule.Conclusion] = chain;
                    changed = true;
                }
            }
        }

        return proofChains.TryGetValue(goal, out List<string>? chainResult) ? chainResult : new List<string>();
    }

    private bool ProveGoal(string goal, HashSet<string> visited)
    {
        if (_facts.Contains(goal))
            return true;

        if (!visited.Add(goal))
            return false;

        foreach (BackwardRule rule in _rules)
        {
            if (rule.Conclusion != goal)
                continue;

            bool allProven = true;
            foreach (string premise in rule.Premises)
            {
                if (!ProveGoal(premise, visited))
                {
                    allProven = false;
                    break;
                }
            }

            if (allProven)
                return true;
        }

        return false;
    }

    private bool ProveGoalWithTrace(string goal, HashSet<string> visited, List<string> trace)
    {
        if (_facts.Contains(goal))
        {
            trace.Add($"Known: {goal}");
            return true;
        }

        if (!visited.Add(goal))
            return false;

        foreach (BackwardRule rule in _rules)
        {
            if (rule.Conclusion != goal)
                continue;

            trace.Add($"Try rule '{rule.Name}': {string.Join(", ", rule.Premises)} => {goal}");
            bool allProven = true;

            foreach (string premise in rule.Premises)
            {
                if (!ProveGoalWithTrace(premise, visited, trace))
                {
                    allProven = false;
                    break;
                }
            }

            if (allProven)
            {
                trace.Add($"Proved: {goal} via {rule.Name}");
                return true;
            }
        }

        trace.Add($"Failed to prove: {goal}");
        return false;
    }
}

/// <summary>A backward chaining rule with premises and a conclusion.</summary>
public sealed class BackwardRule
{
    /// <summary>Gets the name of the rule.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the list of premises required to prove the conclusion.</summary>
    public List<string> Premises { get; init; } = new();

    /// <summary>Gets the conclusion this rule can prove.</summary>
    public string Conclusion { get; init; } = "";

    /// <summary>Gets an optional sub-goal checker for complex premise verification.</summary>
    public Func<HashSet<string>, string, bool>? SubGoalCheck { get; init; }
}
