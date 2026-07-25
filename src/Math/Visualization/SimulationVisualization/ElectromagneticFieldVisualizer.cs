namespace MathVerse.Math.Visualization.SimulationVisualization;

using System;
using System.Collections.Generic;

/// <summary>Represents a single point along a field line.</summary>
public sealed record FieldLinePoint
{
    /// <summary>X position along the field line.</summary>
    public required double X { get; init; }

    /// <summary>Y position along the field line.</summary>
    public required double Y { get; init; }

    /// <summary>Z position along the field line.</summary>
    public double Z { get; init; }

    /// <summary>X component of the field at this point.</summary>
    public double FX { get; init; }

    /// <summary>Y component of the field at this point.</summary>
    public double FY { get; init; }

    /// <summary>Z component of the field at this point.</summary>
    public double FZ { get; init; }
}

/// <summary>Represents a complete field line from a starting point.</summary>
public sealed record FieldLine
{
    /// <summary>Ordered points along the field line.</summary>
    public required IReadOnlyList<FieldLinePoint> Points { get; init; }

    /// <summary>The starting point index.</summary>
    public required int StartPointIndex { get; init; }
}

/// <summary>Complete data for electromagnetic field line visualization.</summary>
public sealed record FieldLineVisualizationData
{
    /// <summary>Computed field lines.</summary>
    public required IReadOnlyList<FieldLine> FieldLines { get; init; }

    /// <summary>Total number of points across all field lines.</summary>
    public required int TotalPoints { get; init; }

    /// <summary>Step size used for integration.</summary>
    public required double StepSize { get; init; }
}

/// <summary>Visualizes electromagnetic field lines using numerical integration.</summary>
public sealed class ElectromagneticFieldVisualizer
{
    /// <summary>
    /// Creates field lines by numerically integrating the field function from each starting point.
    /// Uses a simple Euler method with the given step size.
    /// </summary>
    /// <param name="startPoints">Starting points for field lines [x, y] or [x, y, z].</param>
    /// <param name="fieldFunc">Function that returns the field vector at a given position.</param>
    /// <param name="stepSize">Integration step size.</param>
    /// <param name="maxSteps">Maximum number of integration steps per field line.</param>
    /// <returns>Field line data with all computed points.</returns>
    public FieldLineVisualizationData CreateFieldLines(
        double[][] startPoints,
        Func<double[], double[]> fieldFunc,
        double stepSize = 0.1,
        int maxSteps = 100)
    {
        if (startPoints == null || startPoints.Length == 0 || fieldFunc == null)
        {
            return new FieldLineVisualizationData
            {
                FieldLines = [],
                TotalPoints = 0,
                StepSize = stepSize
            };
        }

        var fieldLines = new List<FieldLine>();
        int totalPoints = 0;

        for (int s = 0; s < startPoints.Length; s++)
        {
            var points = new List<FieldLinePoint>();
            double[] pos = new double[startPoints[s].Length];
            System.Array.Copy(startPoints[s], pos, pos.Length);

            for (int step = 0; step < maxSteps; step++)
            {
                double[] field = fieldFunc(pos);
                if (field == null || field.Length < 2) break;

                double mag = 0.0;
                for (int d = 0; d < field.Length; d++)
                    mag += field[d] * field[d];
                mag = System.Math.Sqrt(mag);

                if (mag < 1e-12) break;

                points.Add(new FieldLinePoint
                {
                    X = pos[0],
                    Y = pos[1],
                    Z = pos.Length > 2 ? pos[2] : 0.0,
                    FX = field[0],
                    FY = field[1],
                    FZ = field.Length > 2 ? field[2] : 0.0
                });

                for (int d = 0; d < System.Math.Min(pos.Length, field.Length); d++)
                    pos[d] += field[d] * stepSize / mag;
            }

            totalPoints += points.Count;

            fieldLines.Add(new FieldLine
            {
                Points = points,
                StartPointIndex = s
            });
        }

        return new FieldLineVisualizationData
        {
            FieldLines = fieldLines,
            TotalPoints = totalPoints,
            StepSize = stepSize
        };
    }
}
