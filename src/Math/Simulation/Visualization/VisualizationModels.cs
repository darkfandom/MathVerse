namespace MathVerse.Math.Simulation.Visualization;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public enum PlotType
{
    Line,
    Scatter,
    Heatmap,
    VectorField,
    Streamlines,
    Contour,
    Surface,
    Histogram,
    PhasePortrait
}

public sealed record PlotSeries
{
    public string Name { get; init; } = string.Empty;
    public PlotType Type { get; init; }
    public ImmutableArray<double> X { get; init; }
    public ImmutableArray<double> Y { get; init; }
    public ImmutableArray<double>? Z { get; init; }
    public ImmutableArray<double>? U { get; init; }
    public ImmutableArray<double>? V { get; init; }
    public string Color { get; init; } = string.Empty;
    public string Style { get; init; } = string.Empty;
    public double LineWidth { get; init; }
    public string Marker { get; init; } = string.Empty;
    public ImmutableDictionary<string, object> Metadata { get; init; } = ImmutableDictionary<string, object>.Empty;
}

public sealed record Frame
{
    public double Time { get; init; }
    public ImmutableArray<PlotSeries> Series { get; init; }
    public ImmutableDictionary<string, object> Metadata { get; init; } = ImmutableDictionary<string, object>.Empty;
}

public sealed record Timeline
{
    public ImmutableArray<Frame> Frames { get; init; }
    public double StartTime { get; init; }
    public double EndTime { get; init; }
    public double FrameRate { get; init; }
}

public sealed record HeatmapData
{
    public ImmutableArray<double> X { get; init; }
    public ImmutableArray<double> Y { get; init; }
    public ImmutableArray<double> Values { get; init; }
    public double MinValue { get; init; }
    public double MaxValue { get; init; }
    public string ColorScale { get; init; } = "viridis";
}

public sealed record VectorFieldData
{
    public ImmutableArray<Vector2D> Positions { get; init; }
    public ImmutableArray<Vector2D> Vectors { get; init; }
    public double Scale { get; init; } = 1.0;
}

public sealed record Vector2D
{
    public double X { get; init; }
    public double Y { get; init; }

    public static Vector2D Zero => new() { X = 0, Y = 0 };
}

public sealed record ContourData
{
    public ImmutableArray<double> X { get; init; }
    public ImmutableArray<double> Y { get; init; }
    public ImmutableArray<double> Z { get; init; }
    public ImmutableArray<double> Levels { get; init; }
}

public sealed record StreamlineData
{
    public ImmutableArray<MVVector> Path { get; init; }
    public ImmutableArray<double> Times { get; init; }
    public double StartTime { get; init; }
    public double EndTime { get; init; }
}

public sealed record ParticleSystemData
{
    public ImmutableArray<MVVector> Positions { get; init; }
    public ImmutableArray<MVVector> Velocities { get; init; }
    public ImmutableArray<double> Masses { get; init; }
    public ImmutableArray<ParticleProperties> Properties { get; init; }
}

public sealed record ParticleProperties
{
    public double Radius { get; init; }
    public string Color { get; init; } = string.Empty;
    public double Opacity { get; init; } = 1.0;
    public ImmutableDictionary<string, object> Custom { get; init; } = ImmutableDictionary<string, object>.Empty;
}

public static class VisualizationModels
{
    public static PlotSeries CreateLineSeries(string name, ImmutableArray<double> x, ImmutableArray<double> y, string color = "blue", double lineWidth = 1.0)
        => new() { Name = name, Type = PlotType.Line, X = x, Y = y, Color = color, LineWidth = lineWidth };

    public static PlotSeries CreateScatterSeries(string name, ImmutableArray<double> x, ImmutableArray<double> y, string color = "red", string marker = "circle")
        => new() { Name = name, Type = PlotType.Scatter, X = x, Y = y, Color = color, Marker = marker };

    public static PlotSeries CreateHeatmapSeries(string name, HeatmapData data, string colorScale = "viridis")
    {
        var meta = ImmutableDictionary.CreateBuilder<string, object>();
        meta.Add("heatmap", data);
        return new() { Name = name, Type = PlotType.Heatmap, Color = colorScale, Metadata = meta.ToImmutable() };
    }

    public static PlotSeries CreateVectorFieldSeries(string name, VectorFieldData data, double scale = 1.0, string color = "blue")
    {
        var meta = ImmutableDictionary.CreateBuilder<string, object>();
        meta.Add("vectorField", data);
        meta.Add("scale", scale);
        return new() { Name = name, Type = PlotType.VectorField, Color = color, Metadata = meta.ToImmutable() };
    }

    public static PlotSeries CreateContourSeries(string name, ContourData data, string colorScale = "viridis")
    {
        var meta = ImmutableDictionary.CreateBuilder<string, object>();
        meta.Add("contour", data);
        return new() { Name = name, Type = PlotType.Contour, Color = colorScale, Metadata = meta.ToImmutable() };
    }

    public static PlotSeries CreateStreamlinesSeries(string name, StreamlineData data, string color = "blue")
    {
        var meta = ImmutableDictionary.CreateBuilder<string, object>();
        meta.Add("streamlines", data);
        return new() { Name = name, Type = PlotType.Streamlines, Color = color, Metadata = meta.ToImmutable() };
    }

    public static PlotSeries CreateParticleSeries(string name, ParticleSystemData data, string color = "blue")
    {
        var meta = ImmutableDictionary.CreateBuilder<string, object>();
        meta.Add("particles", data);
        return new() { Name = name, Type = PlotType.PhasePortrait, Color = color, Metadata = meta.ToImmutable() };
    }

    public static Timeline CreateTimeline(ImmutableArray<Frame> frames, double startTime, double endTime, double frameRate)
        => new() { Frames = frames, StartTime = startTime, EndTime = endTime, FrameRate = frameRate };

    public static Frame CreateFrame(double time, ImmutableArray<PlotSeries> series, ImmutableDictionary<string, object>? metadata = null)
        => new() { Time = time, Series = series, Metadata = metadata ?? ImmutableDictionary<string, object>.Empty };

    public static HeatmapData CreateHeatmap(ImmutableArray<double> x, ImmutableArray<double> y, ImmutableArray<double> values, string colorScale = "viridis")
    {
        double min = double.MaxValue, max = double.MinValue;
        foreach (double v in values) { if (v < min) min = v; if (v > max) max = v; }
        return new() { X = x, Y = y, Values = values, MinValue = min, MaxValue = max, ColorScale = colorScale };
    }

    public static VectorFieldData CreateVectorField(ImmutableArray<Vector2D> positions, ImmutableArray<Vector2D> vectors, double scale = 1.0)
        => new() { Positions = positions, Vectors = vectors, Scale = scale };

    public static ContourData CreateContour(ImmutableArray<double> x, ImmutableArray<double> y, ImmutableArray<double> z, ImmutableArray<double> levels)
        => new() { X = x, Y = y, Z = z, Levels = levels };

    public static StreamlineData CreateStreamline(ImmutableArray<MVVector> path, ImmutableArray<double> times, double startTime, double endTime)
        => new() { Path = path, Times = times, StartTime = startTime, EndTime = endTime };

    public static ParticleSystemData CreateParticleSystem(ImmutableArray<MVVector> positions, ImmutableArray<MVVector> velocities, ImmutableArray<double> masses, ImmutableArray<ParticleProperties> properties)
        => new() { Positions = positions, Velocities = velocities, Masses = masses, Properties = properties };
}