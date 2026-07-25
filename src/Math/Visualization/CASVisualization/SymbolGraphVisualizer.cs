namespace MathVerse.Math.Visualization.CASVisualization;

using System.Collections.Immutable;

/// <summary>Visualizes symbol dependency graphs showing how symbols depend on each other in a mathematical system.</summary>
public sealed class SymbolGraphVisualizer
{
    private const double NodeRadius = 1.5;
    private const int LayoutIterations = 100;
    private const double RepulsionForce = 5.0;
    private const double AttractionForce = 0.1;
    private const double DampingFactor = 0.9;

    /// <summary>Visualizes a symbol dependency graph as a force-directed layout with nodes and edges.</summary>
    /// <param name="dependencies">A dictionary mapping each symbol to the list of symbols it depends on.</param>
    /// <returns>An <see cref="ExpressionTreeResult"/> containing positioned nodes and dependency edges.</returns>
    public ExpressionTreeResult Visualize(Dictionary<string, List<string>> dependencies)
    {
        var result = new ExpressionTreeResult();

        var symbols = new List<string>(dependencies.Keys);
        foreach (var deps in dependencies.Values)
        {
            foreach (var dep in deps)
            {
                if (!symbols.Contains(dep))
                    symbols.Add(dep);
            }
        }

        var positions = InitializePositions(symbols.Count);
        var velocities = new double[symbols.Count, 2];

        // Force-directed layout
        for (int iter = 0; iter < LayoutIterations; iter++)
        {
            var forces = new double[symbols.Count, 2];

            // Repulsion between all pairs
            for (int i = 0; i < symbols.Count; i++)
            {
                for (int j = i + 1; j < symbols.Count; j++)
                {
                    double dx = positions[i][0] - positions[j][0];
                    double dy = positions[i][1] - positions[j][1];
                    double dist = System.Math.Sqrt(dx * dx + dy * dy) + 1e-10;
                    double force = RepulsionForce / (dist * dist);
                    double fx = force * dx / dist;
                    double fy = force * dy / dist;

                    forces[i, 0] += fx;
                    forces[i, 1] += fy;
                    forces[j, 0] -= fx;
                    forces[j, 1] -= fy;
                }
            }

            // Attraction along edges
            for (int i = 0; i < symbols.Count; i++)
            {
                string sym = symbols[i];
                if (dependencies.TryGetValue(sym, out var deps))
                {
                    foreach (var dep in deps)
                    {
                        int j = symbols.IndexOf(dep);
                        if (j < 0) continue;

                        double dx = positions[j][0] - positions[i][0];
                        double dy = positions[j][1] - positions[i][1];
                        double dist = System.Math.Sqrt(dx * dx + dy * dy) + 1e-10;
                        double force = AttractionForce * (dist - NodeRadius);
                        double fx = force * dx / dist;
                        double fy = force * dy / dist;

                        forces[i, 0] += fx;
                        forces[i, 1] += fy;
                        forces[j, 0] -= fx;
                        forces[j, 1] -= fy;
                    }
                }
            }

            // Apply forces with damping
            for (int i = 0; i < symbols.Count; i++)
            {
                velocities[i, 0] = (velocities[i, 0] + forces[i, 0]) * DampingFactor;
                velocities[i, 1] = (velocities[i, 1] + forces[i, 1]) * DampingFactor;
                positions[i][0] += velocities[i, 0];
                positions[i][1] += velocities[i, 1];
            }
        }

        // Assign nodes
        var symbolColors = new Dictionary<string, string>();
        var colorPalette = new[] { "#E74C3C", "#3498DB", "#2ECC71", "#F39C12", "#9B59B6", "#1ABC9C", "#E67E22", "#2C3E50" };
        int colorIdx = 0;

        for (int i = 0; i < symbols.Count; i++)
        {
            bool isSource = dependencies.ContainsKey(symbols[i]);
            if (isSource && !symbolColors.ContainsKey(symbols[i]))
            {
                symbolColors[symbols[i]] = colorPalette[colorIdx % colorPalette.Length];
                colorIdx++;
            }
            else if (!symbolColors.ContainsKey(symbols[i]))
            {
                symbolColors[symbols[i]] = "#7F8C8D";
            }

            bool isRoot = isSource && (!dependencies.TryGetValue(symbols[i], out var d) || d.Count == 0);

            result.Nodes.Add(new ExpressionTreeNode
            {
                Id = i,
                Label = symbols[i],
                NodeType = isRoot ? "root-symbol" : (isSource ? "symbol" : "dependency"),
                X = positions[i][0],
                Y = positions[i][1],
                Color = symbolColors[symbols[i]]
            });
        }

        // Assign edges
        var edgeSet = new HashSet<(int, int)>();
        for (int i = 0; i < symbols.Count; i++)
        {
            string sym = symbols[i];
            if (!dependencies.TryGetValue(sym, out var deps)) continue;

            foreach (var dep in deps)
            {
                int j = symbols.IndexOf(dep);
                if (j < 0) continue;

                var edge = i < j ? (i, j) : (j, i);
                if (edgeSet.Add(edge))
                {
                    result.Edges.Add(new ExpressionTreeEdge
                    {
                        FromNodeId = i,
                        ToNodeId = j,
                        Label = ""
                    });
                }
            }
        }

        double minX = positions.Min(p => p[0]);
        double minY = positions.Min(p => p[1]);
        double maxX = positions.Max(p => p[0]);
        double maxY = positions.Max(p => p[1]);

        result.Width = (int)(maxX - minX + 4);
        result.Height = (int)(maxY - minY + 4);
        return result;
    }

    private static double[][] InitializePositions(int count)
    {
        var positions = new double[count][];
        for (int i = 0; i < count; i++)
        {
            double angle = 2.0 * System.Math.PI * i / count;
            double radius = System.Math.Sqrt(count) * 2.0;
            positions[i] = [radius * System.Math.Cos(angle), radius * System.Math.Sin(angle)];
        }
        return positions;
    }
}
