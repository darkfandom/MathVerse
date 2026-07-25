namespace MathVerse.Math.Visualization.Animation;

using System.Collections.Generic;

/// <summary>Timeline for managing animation keyframes and playback.</summary>
public sealed class AnimationTimeline
{
    /// <summary>Keyframes stored on this timeline.</summary>
    private readonly List<AnimationKeyframe> _keyframes = [];

    /// <summary>Name of the animation.</summary>
    public string Name { get; init; } = "";

    /// <summary>Total duration of the animation in seconds.</summary>
    public double Duration { get; init; } = 1.0;

    /// <summary>Whether the animation loops.</summary>
    public bool Loop { get; init; }

    /// <summary>Playback mode (Forward, PingPong, Loop).</summary>
    public AnimationPlayMode PlayMode { get; init; } = AnimationPlayMode.Forward;

    /// <summary>Read-only access to the sorted keyframes.</summary>
    public IReadOnlyList<AnimationKeyframe> Keyframes => _keyframes;

    /// <summary>Adds a keyframe at the given time with the specified value.</summary>
    /// <param name="time">Time of the keyframe.</param>
    /// <param name="value">Value at the keyframe.</param>
    public void AddKeyframe(double time, double value)
    {
        _keyframes.Add(new AnimationKeyframe(time, value));
    }

    /// <summary>Sorts keyframes by time in ascending order.</summary>
    public void SortKeyframes()
    {
        _keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));
    }

    /// <summary>
    /// Evaluates the animation at time t using linear interpolation between keyframes.
    /// Handles loop and ping-pong modes.
    /// </summary>
    /// <param name="t">Time to evaluate at.</param>
    /// <returns>Interpolated value.</returns>
    public double Evaluate(double t)
    {
        if (_keyframes.Count == 0) return 0.0;
        if (_keyframes.Count == 1) return _keyframes[0].Value;

        t = MapTime(t);

        if (t <= _keyframes[0].Time) return _keyframes[0].Value;
        if (t >= _keyframes[^1].Time) return _keyframes[^1].Value;

        for (int i = 0; i < _keyframes.Count - 1; i++)
        {
            if (t >= _keyframes[i].Time && t <= _keyframes[i + 1].Time)
            {
                double span = _keyframes[i + 1].Time - _keyframes[i].Time;
                double localT = span > 1e-15
                    ? (t - _keyframes[i].Time) / span
                    : 0.0;
                return _keyframes[i].Value + (_keyframes[i + 1].Value - _keyframes[i].Value) * localT;
            }
        }

        return _keyframes[^1].Value;
    }

    private double MapTime(double t)
    {
        if (Duration <= 1e-15) return 0.0;

        return PlayMode switch
        {
            AnimationPlayMode.Forward => Loop
                ? t - System.Math.Floor(t / Duration) * Duration
                : System.Math.Clamp(t, 0.0, Duration),

            AnimationPlayMode.Loop => t - System.Math.Floor(t / Duration) * Duration,

            AnimationPlayMode.PingPong =>
                PingPong(t),

            _ => t
        };
    }

    private double PingPong(double t)
    {
        if (Duration <= 1e-15) return 0.0;

        double period = Duration * 2.0;
        double modT = t - System.Math.Floor(t / period) * period;

        return modT <= Duration ? modT : period - modT;
    }
}

/// <summary>Represents a single keyframe with time and value.</summary>
/// <param name="Time">Time of the keyframe.</param>
/// <param name="Value">Value at the keyframe.</param>
public readonly record struct AnimationKeyframe(double Time, double Value);

/// <summary>Defines the playback mode for animations.</summary>
public enum AnimationPlayMode
{
    /// <summary>Play from start to end once.</summary>
    Forward,

    /// <summary>Play forward then backward repeatedly.</summary>
    PingPong,

    /// <summary>Loop from start to end repeatedly.</summary>
    Loop
}
