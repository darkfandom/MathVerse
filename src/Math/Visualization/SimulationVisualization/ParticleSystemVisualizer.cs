namespace MathVerse.Math.Visualization.SimulationVisualization;

using System.Collections.Generic;

/// <summary>Represents a single particle in the visualization.</summary>
public sealed record ParticleData
{
    /// <summary>Particle index.</summary>
    public required int Index { get; init; }

    /// <summary>X position.</summary>
    public required double X { get; init; }

    /// <summary>Y position.</summary>
    public required double Y { get; init; }

    /// <summary>Z position (for 3D).</summary>
    public double Z { get; init; }

    /// <summary>Particle size.</summary>
    public required double Size { get; init; }

    /// <summary>Red color component (0-1).</summary>
    public double R { get; init; }

    /// <summary>Green color component (0-1).</summary>
    public double G { get; init; }

    /// <summary>Blue color component (0-1).</summary>
    public double B { get; init; }
}

/// <summary>Complete data for particle system visualization.</summary>
public sealed record ParticleVisualizationData
{
    /// <summary>All particles with positions, sizes, and colors.</summary>
    public required IReadOnlyList<ParticleData> Particles { get; init; }

    /// <summary>Bounding box minimum corner.</summary>
    public required (double X, double Y, double Z) BoundsMin { get; init; }

    /// <summary>Bounding box maximum corner.</summary>
    public required (double X, double Y, double Z) BoundsMax { get; init; }

    /// <summary>Total number of particles.</summary>
    public required int ParticleCount { get; init; }
}

/// <summary>Visualizes particle data with positions, sizes, and colors.</summary>
public sealed class ParticleSystemVisualizer
{
    /// <summary>
    /// Creates a particle system visualization with positions, sizes, and optional colors.
    /// </summary>
    /// <param name="positions">Particle positions (each row is [x, y] or [x, y, z]).</param>
    /// <param name="sizes">Optional per-particle sizes.</param>
    /// <param name="colors">Optional per-particle colors as packed RGB values (0-1).</param>
    /// <returns>Particle visualization data with bounding box.</returns>
    public ParticleVisualizationData Create(double[][] positions, double[]? sizes = null, double[]? colors = null)
    {
        if (positions == null || positions.Length == 0)
        {
            return new ParticleVisualizationData
            {
                Particles = [],
                BoundsMin = (0, 0, 0),
                BoundsMax = (0, 0, 0),
                ParticleCount = 0
            };
        }

        var particles = new List<ParticleData>();
        double minX = positions[0][0];
        double minY = positions[0].Length > 1 ? positions[0][1] : 0.0;
        double minZ = positions[0].Length > 2 ? positions[0][2] : 0.0;
        double maxX = minX;
        double maxY = minY;
        double maxZ = minZ;

        for (int i = 0; i < positions.Length; i++)
        {
            double x = positions[i][0];
            double y = positions[i].Length > 1 ? positions[i][1] : 0.0;
            double z = positions[i].Length > 2 ? positions[i][2] : 0.0;

            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (z < minZ) minZ = z;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            if (z > maxZ) maxZ = z;

            double size = sizes != null && i < sizes.Length ? sizes[i] : 1.0;

            double r = 0.2, g = 0.5, b = 0.8;
            if (colors != null)
            {
                int colorIdx = i * 3;
                if (colorIdx + 2 < colors.Length)
                {
                    r = colors[colorIdx];
                    g = colors[colorIdx + 1];
                    b = colors[colorIdx + 2];
                }
            }
            else
            {
                double t = positions.Length > 1 ? (double)i / (double)(positions.Length - 1) : 0.0;
                (r, g, b) = DefaultParticleColor(t);
            }

            particles.Add(new ParticleData
            {
                Index = i,
                X = x,
                Y = y,
                Z = z,
                Size = size,
                R = r,
                G = g,
                B = b
            });
        }

        return new ParticleVisualizationData
        {
            Particles = particles,
            BoundsMin = (minX, minY, minZ),
            BoundsMax = (maxX, maxY, maxZ),
            ParticleCount = particles.Count
        };
    }

    private static (double r, double g, double b) DefaultParticleColor(double t)
    {
        double h = t * 270.0 / 360.0;
        double s = 0.8;
        double v = 0.9;

        double c = v * s;
        double x = c * (1.0 - System.Math.Abs((h * 6.0) % 2.0 - 1.0));
        double m = v - c;

        double r1, g1, b1;
        int sector = (int)(h * 6.0);
        switch (sector)
        {
            case 0: r1 = c; g1 = x; b1 = 0.0; break;
            case 1: r1 = x; g1 = c; b1 = 0.0; break;
            case 2: r1 = 0.0; g1 = c; b1 = x; break;
            case 3: r1 = 0.0; g1 = x; b1 = c; break;
            case 4: r1 = x; g1 = 0.0; b1 = c; break;
            default: r1 = c; g1 = 0.0; b1 = x; break;
        }

        return (r1 + m, g1 + m, b1 + m);
    }
}
