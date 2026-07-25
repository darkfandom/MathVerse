namespace MathVerse.Math.Visualization.SimulationVisualization;

using System.Collections.Generic;

/// <summary>Represents a node in the biological pathway network.</summary>
public sealed record BiologicalNode
{
    /// <summary>Node name/identifier.</summary>
    public required string Name { get; init; }

    /// <summary>X position for layout.</summary>
    public double X { get; init; }

    /// <summary>Y position for layout.</summary>
    public double Y { get; init; }

    /// <summary>Number of incoming interactions.</summary>
    public int InDegree { get; init; }

    /// <summary>Number of outgoing interactions.</summary>
    public int OutDegree { get; init; }

    /// <summary>Total degree (in + out).</summary>
    public int TotalDegree => InDegree + OutDegree;
}

/// <summary>Represents a directed interaction edge in the pathway.</summary>
public sealed record BiologicalEdge
{
    /// <summary>Source node name.</summary>
    public required string Source { get; init; }

    /// <summary>Target node name.</summary>
    public required string Target { get; init; }

    /// <summary>Interaction type (derived from direction).</summary>
    public required string InteractionType { get; init; }
}

/// <summary>Complete data for biological pathway visualization.</summary>
public sealed record BiologicalSystemData
{
    /// <summary>Pathway nodes with layout positions.</summary>
    public required IReadOnlyList<BiologicalNode> Nodes { get; init; }

    /// <summary>Directed interaction edges.</summary>
    public required IReadOnlyList<BiologicalEdge> Edges { get; init; }

    /// <summary>Number of unique nodes.</summary>
    public required int NodeCount { get; init; }

    /// <summary>Number of interactions.</summary>
    public required int InteractionCount { get; init; }
}

/// <summary>Visualizes biological pathways and protein interaction networks.</summary>
public sealed class BiologicalSystemVisualizer
{
    /// <summary>
    /// Visualizes a biological pathway network with a force-directed-like circular layout.
    /// </summary>
    /// <param name="interactions">Dictionary mapping source to list of target species.</param>
    /// <returns>Network nodes and edges with positions.</returns>
    public BiologicalSystemData Visualize(Dictionary<string, List<string>> interactions)
    {
        if (interactions == null || interactions.Count == 0)
        {
            return new BiologicalSystemData
            {
                Nodes = [],
                Edges = [],
                NodeCount = 0,
                InteractionCount = 0
            };
        }

        var nodeSet = new SortedSet<string>();
        var inDegrees = new Dictionary<string, int>();
        var outDegrees = new Dictionary<string, int>();

        foreach (var (source, targets) in interactions)
        {
            nodeSet.Add(source);
            if (!outDegrees.ContainsKey(source)) outDegrees[source] = 0;
            outDegrees[source] += targets.Count;

            foreach (var target in targets)
            {
                nodeSet.Add(target);
                if (!inDegrees.ContainsKey(target)) inDegrees[target] = 0;
                inDegrees[target]++;
            }
        }

        var nodeList = new List<string>(nodeSet);
        int count = nodeList.Count;

        var nodes = new List<BiologicalNode>();
        for (int i = 0; i < count; i++)
        {
            double angle = count > 1
                ? 2.0 * System.Math.PI * (double)i / (double)count
                : 0.0;

            double layerRadius = 1.0;
            int degree = inDegrees.GetValueOrDefault(nodeList[i], 0) +
                         outDegrees.GetValueOrDefault(nodeList[i], 0);
            if (degree > 3) layerRadius = 0.6;

            nodes.Add(new BiologicalNode
            {
                Name = nodeList[i],
                X = layerRadius * System.Math.Cos(angle),
                Y = layerRadius * System.Math.Sin(angle),
                InDegree = inDegrees.GetValueOrDefault(nodeList[i], 0),
                OutDegree = outDegrees.GetValueOrDefault(nodeList[i], 0)
            });
        }

        var edges = new List<BiologicalEdge>();
        foreach (var (source, targets) in interactions)
        {
            foreach (var target in targets)
            {
                edges.Add(new BiologicalEdge
                {
                    Source = source,
                    Target = target,
                    InteractionType = "directed"
                });
            }
        }

        return new BiologicalSystemData
        {
            Nodes = nodes,
            Edges = edges,
            NodeCount = count,
            InteractionCount = edges.Count
        };
    }
}
