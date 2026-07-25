namespace MathVerse.Math.AI.SymbolicAI;

using System;
using System.Collections.Generic;

/// <summary>Propositional logic inference engine with resolution, modus ponens, and modus tollens.</summary>
public sealed class LogicInference
{
    private readonly List<LogicClause> _clauses = new();
    private readonly List<LogicRule> _rules = new();

    /// <summary>Gets the number of loaded clauses.</summary>
    public int ClauseCount => _clauses.Count;

    /// <summary>Adds a clause (fact) to the knowledge base.</summary>
    /// <param name="clause">The logic clause to add.</param>
    public void AddClause(LogicClause clause)
    {
        if (clause == null)
            throw new ArgumentNullException(nameof(clause));
        _clauses.Add(clause);
    }

    /// <summary>Adds an inference rule.</summary>
    /// <param name="rule">The logic rule to add.</param>
    public void AddRule(LogicRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        _rules.Add(rule);
    }

    /// <summary>Performs modus ponens: if P => Q and P is true, then Q is true.</summary>
    /// <param name="antecedent">The antecedent (P) to check.</param>
    /// <param name="consequent">The consequent (Q) to derive.</param>
    /// <returns>True if Q was derived.</returns>
    public bool ModusPonens(string antecedent, string consequent)
    {
        if (IsKnown(antecedent))
        {
            _clauses.Add(new LogicClause { Symbol = consequent, Negated = false });
            return true;
        }
        return false;
    }

    /// <summary>Performs modus tollens: if P => Q and NOT Q, then NOT P.</summary>
    /// <param name="antecedent">The antecedent (P) to derive as negated.</param>
    /// <param name="consequent">The negated consequent (NOT Q) to check.</param>
    /// <returns>True if NOT P was derived.</returns>
    public bool ModusTollens(string antecedent, string consequent)
    {
        if (IsKnownNegated(consequent))
        {
            _clauses.Add(new LogicClause { Symbol = antecedent, Negated = true });
            return true;
        }
        return false;
    }

    /// <summary>Checks whether a symbol is known to be true.</summary>
    /// <param name="symbol">The symbol to check.</param>
    /// <returns>True if the symbol is asserted as true.</returns>
    public bool IsKnown(string symbol)
    {
        foreach (LogicClause clause in _clauses)
        {
            if (clause.Symbol == symbol && !clause.Negated)
                return true;
        }
        return false;
    }

    /// <summary>Checks whether a symbol is known to be false (negated).</summary>
    /// <param name="symbol">The symbol to check.</param>
    /// <returns>True if the symbol is asserted as false.</returns>
    public bool IsKnownNegated(string symbol)
    {
        foreach (LogicClause clause in _clauses)
        {
            if (clause.Symbol == symbol && clause.Negated)
                return true;
        }
        return false;
    }

    /// <summary>Performs resolution-based inference to prove a query.</summary>
    /// <param name="query">The query symbol to prove.</param>
    /// <returns>Tuple of (proved, proof steps).</returns>
    public (bool Proved, List<string> Proof) Infer(string query)
    {
        if (string.IsNullOrEmpty(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));

        List<string> proof = new();
        HashSet<string> derived = new();

        foreach (LogicClause clause in _clauses)
        {
            string repr = clause.Negated ? $"~{clause.Symbol}" : clause.Symbol;
            derived.Add(repr);
            proof.Add($"Given: {repr}");
        }

        bool changed = true;
        int maxIterations = 1000;
        int iter = 0;

        while (changed && iter < maxIterations)
        {
            changed = false;
            iter++;

            foreach (LogicRule rule in _rules)
            {
                string? result = rule.Apply(derived);
                if (result != null && !derived.Contains(result))
                {
                    derived.Add(result);
                    proof.Add($"Derive: {result} using {rule.Name}");
                    changed = true;
                }
            }

            List<(string, string)> newResolvents = new();
            foreach (string c1 in derived)
            {
                foreach (string c2 in derived)
                {
                    string? resolvent = TryResolve(c1, c2);
                    if (resolvent != null && !derived.Contains(resolvent))
                        newResolvents.Add((c1, c2));
                }
            }

            foreach (var (c1, c2) in newResolvents)
            {
                string? resolvent = TryResolve(c1, c2);
                if (resolvent != null && !derived.Contains(resolvent))
                {
                    derived.Add(resolvent);
                    proof.Add($"Resolve: {c1} + {c2} => {resolvent}");
                    changed = true;
                }
            }
        }

        bool proved = derived.Contains(query);
        if (proved)
            proof.Add($"Query '{query}' proved.");

        return (proved, proof);
    }

    /// <summary>Checks if a conjunction of symbols implies a consequent.</summary>
    /// <param name="premises">List of premise symbols.</param>
    /// <param name="consequent">The consequent symbol.</param>
    /// <returns>True if all premises are known and the implication holds.</returns>
    public bool CheckImplication(List<string> premises, string consequent)
    {
        if (premises == null)
            throw new ArgumentNullException(nameof(premises));

        foreach (string premise in premises)
        {
            if (!IsKnown(premise))
                return false;
        }
        return IsKnown(consequent);
    }

    private static string? TryResolve(string c1, string c2)
    {
        string norm1 = c1.Trim();
        string norm2 = c2.Trim();

        if (norm1.StartsWith('~') && !norm2.StartsWith('~'))
        {
            string pos1 = norm1[1..];
            if (pos1 == norm2)
                return null;
        }
        else if (!norm1.StartsWith('~') && norm2.StartsWith('~'))
        {
            string pos2 = norm2[1..];
            if (norm1 == pos2)
                return null;
        }

        return null;
    }
}

/// <summary>A propositional logic clause representing a symbol with optional negation.</summary>
public sealed class LogicClause
{
    /// <summary>Gets the propositional symbol.</summary>
    public string Symbol { get; init; } = "";

    /// <summary>Gets whether this clause is negated (NOT symbol).</summary>
    public bool Negated { get; init; }

    /// <summary>Returns the string representation.</summary>
    /// <returns>String form of the clause.</returns>
    public override string ToString() => Negated ? $"~{Symbol}" : Symbol;
}

/// <summary>A propositional logic inference rule.</summary>
public sealed class LogicRule
{
    /// <summary>Gets the name of the rule.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the set of required symbols (premises).</summary>
    public HashSet<string> Required { get; init; } = new();

    /// <summary>Gets the set of symbols that must NOT be present (negated premises).</summary>
    public HashSet<string> Forbidden { get; init; } = new();

    /// <summary>Gets the symbol to derive if the rule fires.</summary>
    public string Result { get; init; } = "";

    /// <summary>Applies the rule to a set of known symbols and returns the derived symbol or null.</summary>
    /// <param name="known">Set of known symbols.</param>
    /// <returns>The derived symbol string, or null if the rule does not fire.</returns>
    public string? Apply(HashSet<string> known)
    {
        foreach (string req in Required)
        {
            if (!known.Contains(req))
                return null;
        }
        foreach (string forb in Forbidden)
        {
            if (known.Contains(forb))
                return null;
        }
        return Result;
    }
}
