namespace MathVerse.Math.Visualization.Animation;

using System.Collections.Generic;

/// <summary>Represents a single playback frame from a simulation recording.</summary>
public sealed record PlaybackFrame
{
    /// <summary>Frame index.</summary>
    public required int FrameIndex { get; init; }

    /// <summary>Time value for this frame.</summary>
    public required double Time { get; init; }

    /// <summary>State vector at this time.</summary>
    public required double[] State { get; init; }
}

/// <summary>Complete data for simulation playback.</summary>
public sealed record SimulationPlaybackData
{
    /// <summary>All playback frames.</summary>
    public required IReadOnlyList<PlaybackFrame> Frames { get; init; }

    /// <summary>Total number of frames.</summary>
    public required int TotalFrames { get; init; }

    /// <summary>Total simulation time.</summary>
    public required double TotalTime { get; init; }

    /// <summary>State dimension.</summary>
    public required int StateDimensions { get; init; }

    /// <summary>Returns the frame closest to the given time.</summary>
    public PlaybackFrame GetFrameAtTime(double t)
    {
        if (Frames.Count == 0)
            return new PlaybackFrame { FrameIndex = 0, Time = 0.0, State = [] };

        double step = TotalFrames > 1 ? TotalTime / (double)(TotalFrames - 1) : 0.0;
        int idx = step > 1e-15
            ? (int)System.Math.Round(t / step)
            : 0;
        idx = System.Math.Clamp(idx, 0, Frames.Count - 1);
        return Frames[idx];
    }
}

/// <summary>Playback system for recorded simulation state histories.</summary>
public sealed class SimulationPlayback
{
    /// <summary>
    /// Creates a playback from recorded simulation state history.
    /// </summary>
    /// <param name="stateHistory">State vectors at each recorded time step.</param>
    /// <param name="timeStep">Time increment between recorded states.</param>
    /// <returns>Playback data with frames for animation.</returns>
    public SimulationPlaybackData Create(List<double[]> stateHistory, double timeStep)
    {
        if (stateHistory == null || stateHistory.Count == 0)
        {
            return new SimulationPlaybackData
            {
                Frames = [],
                TotalFrames = 0,
                TotalTime = 0.0,
                StateDimensions = 0
            };
        }

        int count = stateHistory.Count;
        int dims = stateHistory[0].Length;
        var frames = new List<PlaybackFrame>();

        for (int i = 0; i < count; i++)
        {
            frames.Add(new PlaybackFrame
            {
                FrameIndex = i,
                Time = (double)i * timeStep,
                State = stateHistory[i]
            });
        }

        return new SimulationPlaybackData
        {
            Frames = frames,
            TotalFrames = count,
            TotalTime = (double)(count - 1) * timeStep,
            StateDimensions = dims
        };
    }
}
