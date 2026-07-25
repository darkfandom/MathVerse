namespace MathVerse.Math.Visualization.SimulationVisualization;

using System.Collections.Generic;

/// <summary>Represents a node (molecule) in the reaction network.</summary>
public sealed record ReactionNode
{
    /// <summary>Name/formula of the species.</summary>
    public required string Name { get; init; }

    /// <summary>X position for layout.</summary>
    public double X { get; init; }

    /// <summary>Y position for layout.</summary>
    public double Y { get; init; }

    /// <summary>Number of connections.</summary>
    public required int ConnectionCount { get; init; }

    /// <summary>Whether this species is a reactant in at least one reaction.</summary>
    public bool IsReactant { get; init; }

    /// <summary>Whether this species is a product in at least one reaction.</summary>
    public bool IsProduct { get; init; }
}

/// <summary>Represents a reaction edge in the network.</summary>
public sealed record ReactionEdge
{
    /// <summary>Source species name (reactant).</summary>
    public required string Reactant { get; init; }

    /// <summary>Target species name (product).</summary>
    public required string Product { get; init; }

    /// <summary>Reaction rate constant.</summary>
    public required double Rate { get; init; }

    /// <summary>Normalized rate for visual scaling (0-1).</summary>
    public required double NormalizedRate { get; init; }
}

/// <summary>Complete data for chemical reaction network visualization.</summary>
public sealed record ChemicalReactionData
{
    /// <summary>Species nodes with layout positions.</summary>
    public required IReadOnlyList<ReactionNode> Nodes { get; init; }

    /// <summary>Reaction edges with rates.</summary>
    public required IReadOnlyList<ReactionEdge> Edges { get; init; }

    /// <summary>Number of unique species.</summary>
    public required int SpeciesCount { get; init; }

    /// <summary>Number of reactions.</summary>
    public required int ReactionCount { get; init; }
}

/// <summary>Visualizes chemical reaction networks as a directed graph.</summary>
public sealed class ChemicalReactionVisualizer
{
    /// <summary>
    /// Visualizes a reaction network, placing nodes in a circular layout and edges with width proportional to rate.
    /// </summary>
    /// <param name="reactions">List of (reactant, product, rate) triples.</param>
    /// <returns>Network nodes and edges with layout positions.</returns>
    public ChemicalReactionData Visualize(
        List<(string reactant, string product, double rate)> reactions)
    {
        if (reactions == null || reactions.Count == 0)
        {
            return new ChemicalReactionData
            {
                Nodes = [],
                Edges = [],
                SpeciesCount = 0,
                ReactionCount = 0
            };
        }

        var speciesSet = new SortedSet<string>();
        var connectionCounts = new Dictionary<string, int>();
        var reactantSet = new HashSet<string>();
        var productSet = new HashSet<string>();

        foreach (var (reactant, product, rate) in reactions)
        {
            speciesSet.Add(reactant);
            speciesSet.Add(product);
            reactantSet.Add(reactant);
            productSet.Add(product);

            if (!connectionCounts.ContainsKey(reactant))
                connectionCounts[reactant] = 0;
            if (!connectionCounts.ContainsKey(product))
                connectionCounts[product] = 0;
            connectionCounts[reactant]++;
            connectionCounts[product]++;
        }

        var speciesList = new List<string>(speciesSet);
        int speciesCount = speciesList.Count;

        double maxRate = 0.0;
        foreach (var (_, _, rate) in reactions)
        {
            if (rate > maxRate) maxRate = rate;
        }

        var nodes = new List<ReactionNode>();
        for (int i = 0; i < speciesCount; i++)
        {
            double angle = speciesCount > 1
                ? 2.0 * System.Math.PI * (double)i / (double)speciesCount
                : 0.0;
            double x = System.Math.Cos(angle);
            double y = System.Math.Sin(angle);

            nodes.Add(new ReactionNode
            {
                Name = speciesList[i],
                X = x,
                Y = y,
                ConnectionCount = connectionCounts.GetValueOrDefault(speciesList[i], 0),
                IsReactant = reactantSet.Contains(speciesList[i]),
                IsProduct = productSet.Contains(speciesList[i])
            });
        }

        var edges = new List<ReactionEdge>();
        foreach (var (reactant, product, rate) in reactions)
        {
            double normalized = maxRate > 1e-15 ? rate / maxRate : 0.0;
            edges.Add(new ReactionEdge
            {
                Reactant = reactant,
                Product = product,
                Rate = rate,
                NormalizedRate = normalized
            });
        }

        return new ChemicalReactionData
        {
            Nodes = nodes,
            Edges = edges,
            SpeciesCount = speciesCount,
            ReactionCount = reactions.Count
        };
    }
}
