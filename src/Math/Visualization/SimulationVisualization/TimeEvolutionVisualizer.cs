namespace MathVerse.Math.Visualization.SimulationVisualization;

using System.Collections.Generic;

/// <summary>Represents a single time step in the evolution overlay.</summary>
public sealed record TimeSnapshot
{
    /// <summary>Time value.</summary>
    public required double Time { get; init; }

    /// <summary>Frame index.</summary>
    public required int FrameIndex { get; init; }

    /// <summary>State data at this time step.</summary>
    public required double[] State { get; init; }

    /// <summary>Normalized opacity for overlay (newer frames more opaque).</summary>
    public required double Opacity { get; init; }
}

/// <summary>Represents a trace (trajectory) of a single variable over time.</summary>
public sealed record TimeTrace
{
    /// <summary>Variable index.</summary>
    public required int VariableIndex { get; init; }

    /// <summary>Values over all time steps.</summary>
    public required IReadOnlyList<(double Time, double Value)> Values { get; init; }

    /// <summary>Minimum value across all time steps.</summary>
    public required double MinValue { get; init; }

    /// <summary>Maximum value across all time steps.</summary>
    public required double MaxValue { get; init; }
}

/// <summary>Complete data for time evolution visualization.</summary>
public sealed record TimeEvolutionData
{
    /// <summary>Snapshots at each time step.</summary>
    public required IReadOnlyList<TimeSnapshot> Snapshots { get; init; }

    /// <summary>Traces for each variable dimension.</summary>
    public required IReadOnlyList<TimeTrace> Traces { get; init; }

    /// <summary>Total time span.</summary>
    public required double TimeSpan { get; init; }

    /// <summary>Number of state dimensions.</summary>
    public required int StateDimensions { get; init; }
}

/// <summary>Visualizes time evolution by overlaying multiple simulation snapshots.</summary>
public sealed class TimeEvolutionVisualizer
{
    /// <summary>
    /// Creates an overlay of simulation state snapshots over time.
    /// Computes per-dimension traces with min/max ranges.
    /// </summary>
    /// <param name="snapshots">State vectors at each time step.</param>
    /// <param name="timeStep">Time increment between snapshots.</param>
    /// <returns>Time evolution data with snapshots and traces.</returns>
    public TimeEvolutionData Create(List<double[]> snapshots, double timeStep)
    {
        if (snapshots == null || snapshots.Count == 0)
        {
            return new TimeEvolutionData
            {
                Snapshots = [],
                Traces = [],
                TimeSpan = 0.0,
                StateDimensions = 0
            };
        }

        int frameCount = snapshots.Count;
        int dims = snapshots[0].Length;
        double totalTime = (double)(frameCount - 1) * timeStep;

        var snapList = new List<TimeSnapshot>();
        for (int i = 0; i < frameCount; i++)
        {
            double time = (double)i * timeStep;
            double opacity = frameCount > 1
                ? (double)i / (double)(frameCount - 1) * 0.8 + 0.2
                : 1.0;

            snapList.Add(new TimeSnapshot
            {
                Time = time,
                FrameIndex = i,
                State = snapshots[i],
                Opacity = opacity
            });
        }

        var traces = new List<TimeTrace>();
        for (int d = 0; d < dims; d++)
        {
            var values = new List<(double Time, double Value)>();
            double minVal = double.MaxValue;
            double maxVal = double.MinValue;

            for (int i = 0; i < frameCount; i++)
            {
                double time = (double)i * timeStep;
                double val = d < snapshots[i].Length ? snapshots[i][d] : 0.0;

                values.Add((time, val));
                if (val < minVal) minVal = val;
                if (val > maxVal) maxVal = val;
            }

            traces.Add(new TimeTrace
            {
                VariableIndex = d,
                Values = values,
                MinValue = minVal,
                MaxValue = maxVal
            });
        }

        return new TimeEvolutionData
        {
            Snapshots = snapList,
            Traces = traces,
            TimeSpan = totalTime,
            StateDimensions = dims
        };
    }
}
