namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a single neuron node in the network visualization.</summary>
public sealed record NetworkNode
{
    /// <summary>Layer index (column) of this node.</summary>
    public required int LayerIndex { get; init; }

    /// <summary>Index within the layer (row).</summary>
    public required int NodeIndex { get; init; }

    /// <summary>X position in the layout.</summary>
    public required double X { get; init; }

    /// <summary>Y position in the layout.</summary>
    public required double Y { get; init; }

    /// <summary>Label for the node.</summary>
    public string Label { get; init; } = "";
}

/// <summary>Represents a connection between two neurons.</summary>
public sealed record NetworkEdge
{
    /// <summary>Source layer index.</summary>
    public required int FromLayer { get; init; }

    /// <summary>Source node index within the layer.</summary>
    public required int FromNode { get; init; }

    /// <summary>Target layer index.</summary>
    public required int ToLayer { get; init; }

    /// <summary>Target node index within the layer.</summary>
    public required int ToNode { get; init; }
}

/// <summary>Complete plot data for the neural network visualization.</summary>
public sealed record NeuralNetworkPlotData
{
    /// <summary>All neuron nodes with positions.</summary>
    public required IReadOnlyList<NetworkNode> Nodes { get; init; }

    /// <summary>All connections between layers.</summary>
    public required IReadOnlyList<NetworkEdge> Edges { get; init; }

    /// <summary>Layer sizes as specified.</summary>
    public required IReadOnlyList<int> LayerSizes { get; init; }

    /// <summary>Total number of neurons.</summary>
    public required int TotalNeurons { get; init; }

    /// <summary>Total number of connections.</summary>
    public required int TotalConnections { get; init; }
}

/// <summary>Visualizes neural network architecture as a layered graph.</summary>
public sealed class NeuralNetworkVisualizer
{
    /// <summary>
    /// Visualizes a neural network architecture with layers as columns and connections as lines.
    /// </summary>
    /// <param name="layerSizes">Number of neurons in each layer.</param>
    /// <returns>Node positions and edge connections for rendering.</returns>
    public NeuralNetworkPlotData Visualize(int[] layerSizes)
    {
        if (layerSizes == null || layerSizes.Length == 0)
        {
            return new NeuralNetworkPlotData
            {
                Nodes = [],
                Edges = [],
                LayerSizes = [],
                TotalNeurons = 0,
                TotalConnections = 0
            };
        }

        var nodes = new List<NetworkNode>();
        var edges = new List<NetworkEdge>();

        int numLayers = layerSizes.Length;
        double horizontalSpacing = numLayers > 1 ? 1.0 / (double)(numLayers - 1) : 0.5;

        for (int layer = 0; layer < numLayers; layer++)
        {
            int neuronsInLayer = layerSizes[layer];
            double x = (double)layer * horizontalSpacing;

            for (int node = 0; node < neuronsInLayer; node++)
            {
                double y = neuronsInLayer > 1
                    ? (double)node / (double)(neuronsInLayer - 1)
                    : 0.5;

                nodes.Add(new NetworkNode
                {
                    LayerIndex = layer,
                    NodeIndex = node,
                    X = x,
                    Y = y,
                    Label = $"L{layer}:N{node}"
                });
            }
        }

        for (int layer = 0; layer < numLayers - 1; layer++)
        {
            int fromCount = layerSizes[layer];
            int toCount = layerSizes[layer + 1];

            for (int fromNode = 0; fromNode < fromCount; fromNode++)
            {
                for (int toNode = 0; toNode < toCount; toNode++)
                {
                    edges.Add(new NetworkEdge
                    {
                        FromLayer = layer,
                        FromNode = fromNode,
                        ToLayer = layer + 1,
                        ToNode = toNode
                    });
                }
            }
        }

        int totalNeurons = 0;
        for (int i = 0; i < layerSizes.Length; i++)
            totalNeurons += layerSizes[i];

        return new NeuralNetworkPlotData
        {
            Nodes = nodes,
            Edges = edges,
            LayerSizes = layerSizes,
            TotalNeurons = totalNeurons,
            TotalConnections = edges.Count
        };
    }
}
