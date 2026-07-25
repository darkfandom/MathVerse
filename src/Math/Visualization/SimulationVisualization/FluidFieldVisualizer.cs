namespace MathVerse.Math.Visualization.SimulationVisualization;

using System.Collections.Generic;

/// <summary>Represents a vector field arrow at a position.</summary>
public sealed record VectorFieldArrow
{
    /// <summary>X position.</summary>
    public required double X { get; init; }

    /// <summary>Y position.</summary>
    public required double Y { get; init; }

    /// <summary>Z position (for 3D).</summary>
    public double Z { get; init; }

    /// <summary>X component of the vector.</summary>
    public required double VX { get; init; }

    /// <summary>Y component of the vector.</summary>
    public required double VY { get; init; }

    /// <summary>Z component of the vector.</summary>
    public double VZ { get; init; }

    /// <summary>Magnitude of the vector.</summary>
    public required double Magnitude { get; init; }

    /// <summary>Normalized magnitude (0-1) for color mapping.</summary>
    public required double NormalizedMagnitude { get; init; }
}

/// <summary>Complete data for vector field visualization.</summary>
public sealed record VectorFieldData
{
    /// <summary>Vector arrows at each position.</summary>
    public required IReadOnlyList<VectorFieldArrow> Arrows { get; init; }

    /// <summary>Maximum vector magnitude for normalization.</summary>
    public required double MaxMagnitude { get; init; }
}

/// <summary>Represents a scalar field sample.</summary>
public sealed record ScalarFieldSample
{
    /// <summary>X position.</summary>
    public required double X { get; init; }

    /// <summary>Y position.</summary>
    public required double Y { get; init; }

    /// <summary>Z position (for 3D).</summary>
    public double Z { get; init; }

    /// <summary>Scalar value.</summary>
    public required double Value { get; init; }

    /// <summary>Normalized value (0-1).</summary>
    public required double NormalizedValue { get; init; }

    /// <summary>Mapped color.</summary>
    public required (double R, double G, double B) Color { get; init; }
}

/// <summary>Complete data for scalar field visualization.</summary>
public sealed record ScalarFieldData
{
    /// <summary>Scalar samples with colors.</summary>
    public required IReadOnlyList<ScalarFieldSample> Samples { get; init; }

    /// <summary>Minimum scalar value.</summary>
    public required double MinValue { get; init; }

    /// <summary>Maximum scalar value.</summary>
    public required double MaxValue { get; init; }
}

/// <summary>Visualizes vector and scalar fields from simulation data.</summary>
public sealed class FluidFieldVisualizer
{
    /// <summary>
    /// Creates a vector field visualization with arrows at each position.
    /// </summary>
    /// <param name="positions">Sample positions [x, y] or [x, y, z].</param>
    /// <param name="vectors">Vector values at each position [vx, vy] or [vx, vy, vz].</param>
    /// <param name="scale">Scale factor for arrow lengths.</param>
    /// <returns>Vector field arrow data with normalized magnitudes.</returns>
    public VectorFieldData CreateVectorField(double[][] positions, double[][] vectors, double scale = 1.0)
    {
        if (positions == null || vectors == null || positions.Length == 0 || vectors.Length == 0)
        {
            return new VectorFieldData
            {
                Arrows = [],
                MaxMagnitude = 0.0
            };
        }

        int count = System.Math.Min(positions.Length, vectors.Length);
        var arrows = new List<VectorFieldArrow>();
        double maxMag = 0.0;

        for (int i = 0; i < count; i++)
        {
            double vx = vectors[i].Length > 0 ? vectors[i][0] : 0.0;
            double vy = vectors[i].Length > 1 ? vectors[i][1] : 0.0;
            double vz = vectors[i].Length > 2 ? vectors[i][2] : 0.0;

            double mag = System.Math.Sqrt(vx * vx + vy * vy + vz * vz);
            if (mag > maxMag) maxMag = mag;
        }

        double maxScaled = maxMag * scale;

        for (int i = 0; i < count; i++)
        {
            double x = positions[i].Length > 0 ? positions[i][0] : 0.0;
            double y = positions[i].Length > 1 ? positions[i][1] : 0.0;
            double z = positions[i].Length > 2 ? positions[i][2] : 0.0;
            double vx = vectors[i].Length > 0 ? vectors[i][0] * scale : 0.0;
            double vy = vectors[i].Length > 1 ? vectors[i][1] * scale : 0.0;
            double vz = vectors[i].Length > 2 ? vectors[i][2] * scale : 0.0;

            double mag = System.Math.Sqrt(vx * vx + vy * vy + vz * vz);
            double normalized = maxScaled > 1e-15 ? mag / maxScaled : 0.0;

            arrows.Add(new VectorFieldArrow
            {
                X = x,
                Y = y,
                Z = z,
                VX = vx,
                VY = vy,
                VZ = vz,
                Magnitude = mag,
                NormalizedMagnitude = normalized
            });
        }

        return new VectorFieldData
        {
            Arrows = arrows,
            MaxMagnitude = maxMag
        };
    }

    /// <summary>
    /// Creates a scalar field visualization with color-mapped samples.
    /// </summary>
    /// <param name="positions">Sample positions [x, y] or [x, y, z].</param>
    /// <param name="values">Scalar values at each position.</param>
    /// <param name="colorMap">Color map name.</param>
    /// <returns>Scalar field data with color-mapped samples.</returns>
    public ScalarFieldData CreateScalarField(double[][] positions, double[] values, string colorMap = "Viridis")
    {
        if (positions == null || values == null || positions.Length == 0)
        {
            return new ScalarFieldData
            {
                Samples = [],
                MinValue = 0.0,
                MaxValue = 0.0
            };
        }

        int count = System.Math.Min(positions.Length, values.Length);
        double min = values[0];
        double max = values[0];
        for (int i = 1; i < count; i++)
        {
            if (values[i] < min) min = values[i];
            if (values[i] > max) max = values[i];
        }

        double range = max - min;
        var samples = new List<ScalarFieldSample>();

        for (int i = 0; i < count; i++)
        {
            double x = positions[i].Length > 0 ? positions[i][0] : 0.0;
            double y = positions[i].Length > 1 ? positions[i][1] : 0.0;
            double z = positions[i].Length > 2 ? positions[i][2] : 0.0;
            double val = values[i];
            double normalized = range > 1e-15 ? (val - min) / range : 0.0;

            samples.Add(new ScalarFieldSample
            {
                X = x,
                Y = y,
                Z = z,
                Value = val,
                NormalizedValue = normalized,
                Color = ApplyColorMap(normalized, colorMap)
            });
        }

        return new ScalarFieldData
        {
            Samples = samples,
            MinValue = min,
            MaxValue = max
        };
    }

    private static (double R, double G, double B) ApplyColorMap(double t, string mapName)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        return mapName.ToLowerInvariant() switch
        {
            "viridis" => ViridisColor(t),
            "plasma" => PlasmaColor(t),
            "inferno" => InfernoColor(t),
            "grayscale" => (t, t, t),
            _ => ViridisColor(t)
        };
    }

    private static (double R, double G, double B) ViridisColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.267004, 0.004874, 0.329415],
            [0.1, 0.282327, 0.215072, 0.495441],
            [0.2, 0.194026, 0.407061, 0.557050],
            [0.3, 0.127568, 0.566949, 0.551229],
            [0.4, 0.070600, 0.694293, 0.499732],
            [0.5, 0.119208, 0.786923, 0.397928],
            [0.6, 0.468049, 0.897475, 0.157388],
            [0.7, 0.816658, 0.847280, 0.078447],
            [0.8, 0.934850, 0.788888, 0.147713],
            [0.9, 0.940015, 0.499239, 0.209724],
            [1.0, 0.988362, 0.305861, 0.317971]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double R, double G, double B) PlasmaColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.050383, 0.029803, 0.527975],
            [0.1, 0.284939, 0.013218, 0.553439],
            [0.2, 0.473062, 0.015023, 0.457958],
            [0.3, 0.610617, 0.157349, 0.319777],
            [0.4, 0.703234, 0.324343, 0.184576],
            [0.5, 0.760638, 0.488352, 0.090915],
            [0.6, 0.785105, 0.659710, 0.047982],
            [0.7, 0.781178, 0.841941, 0.026693],
            [0.8, 0.781178, 0.841941, 0.026693],
            [0.9, 0.781178, 0.841941, 0.026693],
            [1.0, 0.781178, 0.841941, 0.026693]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double R, double G, double B) InfernoColor(double t)
    {
        double[][] stops =
        [
            [0.0, 0.001462, 0.000466, 0.013866],
            [0.1, 0.206364, 0.029396, 0.273293],
            [0.2, 0.474024, 0.023244, 0.325859],
            [0.3, 0.703983, 0.147086, 0.135018],
            [0.4, 0.856387, 0.425350, 0.024355],
            [0.5, 0.922470, 0.715256, 0.083681],
            [0.6, 0.933478, 0.835552, 0.176423],
            [0.7, 0.927224, 0.923315, 0.338965],
            [0.8, 0.950532, 0.965310, 0.562702],
            [0.9, 0.988362, 0.998364, 0.644924],
            [1.0, 0.988362, 0.998364, 0.644924]
        ];
        return InterpolateStops(stops, t);
    }

    private static (double R, double G, double B) InterpolateStops(double[][] stops, double t)
    {
        if (t <= stops[0][0])
            return (stops[0][1], stops[0][2], stops[0][3]);
        if (t >= stops[^1][0])
            return (stops[^1][1], stops[^1][2], stops[^1][3]);

        for (int i = 0; i < stops.Length - 1; i++)
        {
            if (t >= stops[i][0] && t <= stops[i + 1][0])
            {
                double span = stops[i + 1][0] - stops[i][0];
                double localT = span > 1e-15 ? (t - stops[i][0]) / span : 0.0;
                return (
                    stops[i][1] + (stops[i + 1][1] - stops[i][1]) * localT,
                    stops[i][2] + (stops[i + 1][2] - stops[i][2]) * localT,
                    stops[i][3] + (stops[i + 1][3] - stops[i][3]) * localT
                );
            }
        }

        return (stops[^1][1], stops[^1][2], stops[^1][3]);
    }
}
