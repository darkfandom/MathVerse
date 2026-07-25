namespace MathVerse.Math.Visualization.Animation;

using System.Collections.Generic;

/// <summary>Represents a multi-dimensional keyframe with time and vector value.</summary>
public sealed record MultiKeyframe
{
    /// <summary>Time of the keyframe.</summary>
    public required double Time { get; init; }

    /// <summary>Values at this keyframe.</summary>
    public required double[] Values { get; init; }
}

/// <summary>Complete data for a keyframe animation evaluation.</summary>
public sealed record KeyframeAnimationData
{
    /// <summary>Evaluated value at the requested time.</summary>
    public required double[] Value { get; init; }

    /// <summary>Number of dimensions.</summary>
    public required int Dimensions { get; init; }

    /// <summary>All keyframes used.</summary>
    public required IReadOnlyList<MultiKeyframe> Keyframes { get; init; }
}

/// <summary>Defines interpolation types for keyframe animations.</summary>
public enum InterpolationType
{
    /// <summary>Linear interpolation between keyframes.</summary>
    Linear,

    /// <summary>Step function (no interpolation).</summary>
    Step,

    /// <summary>Cubic smooth interpolation.</summary>
    CubicSmooth
}

/// <summary>Keyframe animation system for arbitrary multi-dimensional properties.</summary>
public sealed class KeyframeAnimation
{
    private readonly List<MultiKeyframe> _keyframes = [];

    /// <summary>Number of dimensions per keyframe.</summary>
    public int Dimensions { get; private set; }

    /// <summary>Interpolation type used for evaluation.</summary>
    public InterpolationType Interpolation { get; init; } = InterpolationType.Linear;

    /// <summary>
    /// Creates a keyframe animation from time-value pairs.
    /// </summary>
    /// <param name="keyframes">List of (time, values) pairs.</param>
    /// <param name="interpolation">Interpolation method to use.</param>
    /// <returns>A configured KeyframeAnimation instance.</returns>
    public static KeyframeAnimation Create(
        List<(double time, double[] values)> keyframes,
        InterpolationType interpolation)
    {
        var anim = new KeyframeAnimation { Interpolation = interpolation };

        if (keyframes != null)
        {
            foreach (var (time, values) in keyframes)
            {
                anim._keyframes.Add(new MultiKeyframe { Time = time, Values = values });
                if (anim.Dimensions == 0 && values.Length > 0)
                    anim.Dimensions = values.Length;
            }

            anim._keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        return anim;
    }

    /// <summary>Evaluates the animation at the given time.</summary>
    /// <param name="t">Time to evaluate at.</param>
    /// <returns>Interpolated multi-dimensional value.</returns>
    public double[] Evaluate(double t)
    {
        if (Dimensions == 0) return [];
        if (_keyframes.Count == 0) return new double[Dimensions];
        if (_keyframes.Count == 1) return (double[])_keyframes[0].Values.Clone();

        if (t <= _keyframes[0].Time)
            return (double[])_keyframes[0].Values.Clone();
        if (t >= _keyframes[^1].Time)
            return (double[])_keyframes[^1].Values.Clone();

        return Interpolation switch
        {
            InterpolationType.Step => StepInterpolate(t),
            InterpolationType.CubicSmooth => CubicSmoothInterpolate(t),
            _ => LinearInterpolate(t)
        };
    }

    /// <summary>Returns the complete animation data including keyframes.</summary>
    /// <param name="t">Time to evaluate at.</param>
    /// <returns>Full animation data.</returns>
    public KeyframeAnimationData GetData(double t)
    {
        return new KeyframeAnimationData
        {
            Value = Evaluate(t),
            Dimensions = Dimensions,
            Keyframes = _keyframes
        };
    }

    private double[] LinearInterpolate(double t)
    {
        for (int i = 0; i < _keyframes.Count - 1; i++)
        {
            if (t >= _keyframes[i].Time && t <= _keyframes[i + 1].Time)
            {
                double span = _keyframes[i + 1].Time - _keyframes[i].Time;
                double localT = span > 1e-15 ? (t - _keyframes[i].Time) / span : 0.0;

                var result = new double[Dimensions];
                for (int d = 0; d < Dimensions; d++)
                {
                    double a = d < _keyframes[i].Values.Length ? _keyframes[i].Values[d] : 0.0;
                    double b = d < _keyframes[i + 1].Values.Length ? _keyframes[i + 1].Values[d] : 0.0;
                    result[d] = a + (b - a) * localT;
                }
                return result;
            }
        }

        return (double[])_keyframes[^1].Values.Clone();
    }

    private double[] StepInterpolate(double t)
    {
        for (int i = _keyframes.Count - 1; i >= 0; i--)
        {
            if (t >= _keyframes[i].Time)
                return (double[])_keyframes[i].Values.Clone();
        }
        return (double[])_keyframes[0].Values.Clone();
    }

    private double[] CubicSmoothInterpolate(double t)
    {
        for (int i = 0; i < _keyframes.Count - 1; i++)
        {
            if (t >= _keyframes[i].Time && t <= _keyframes[i + 1].Time)
            {
                double span = _keyframes[i + 1].Time - _keyframes[i].Time;
                double localT = span > 1e-15 ? (t - _keyframes[i].Time) / span : 0.0;
                double smoothT = localT * localT * (3.0 - 2.0 * localT);

                var result = new double[Dimensions];
                for (int d = 0; d < Dimensions; d++)
                {
                    double a = d < _keyframes[i].Values.Length ? _keyframes[i].Values[d] : 0.0;
                    double b = d < _keyframes[i + 1].Values.Length ? _keyframes[i + 1].Values[d] : 0.0;
                    result[d] = a + (b - a) * smoothT;
                }
                return result;
            }
        }

        return (double[])_keyframes[^1].Values.Clone();
    }
}
