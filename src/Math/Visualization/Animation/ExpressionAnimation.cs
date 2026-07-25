namespace MathVerse.Math.Visualization.Animation;

using System;
using System.Collections.Generic;

/// <summary>Represents a single evaluated frame of an expression animation.</summary>
public sealed record ExpressionFrame
{
    /// <summary>Time value.</summary>
    public required double Time { get; init; }

    /// <summary>Function value at this time.</summary>
    public required double Value { get; init; }

    /// <summary>X coordinate for plotting (normalized).</summary>
    public double X { get; init; }

    /// <summary>Y coordinate for plotting (normalized).</summary>
    public double Y { get; init; }
}

/// <summary>Complete data for expression animation visualization.</summary>
public sealed record ExpressionAnimationData
{
    /// <summary>Evaluated frames over the time range.</summary>
    public required IReadOnlyList<ExpressionFrame> Frames { get; init; }

    /// <summary>Minimum time value.</summary>
    public required double TMin { get; init; }

    /// <summary>Maximum time value.</summary>
    public required double TMax { get; init; }

    /// <summary>Animation duration.</summary>
    public required double AnimTime { get; init; }

    /// <summary>Minimum function value.</summary>
    public required double MinValue { get; init; }

    /// <summary>Maximum function value.</summary>
    public required double MaxValue { get; init; }
}

/// <summary>Animates mathematical expressions over time.</summary>
public sealed class ExpressionAnimation
{
    /// <summary>
    /// Creates an animation from a time-varying mathematical function.
    /// Evaluates the function at many points across [tMin, tMax] and scales to animation time.
    /// </summary>
    /// <param name="expression">Function of time f(t) → value.</param>
    /// <param name="tMin">Minimum time domain value.</param>
    /// <param name="tMax">Maximum time domain value.</param>
    /// <param name="animTime">Total animation duration in seconds.</param>
    /// <param name="resolution">Number of evaluation points.</param>
    /// <returns>Expression animation data with frames.</returns>
    public ExpressionAnimationData Create(
        Func<double, double> expression,
        double tMin,
        double tMax,
        double animTime,
        int resolution = 200)
    {
        if (expression == null || tMin >= tMax || resolution < 2)
        {
            return new ExpressionAnimationData
            {
                Frames = [],
                TMin = tMin,
                TMax = tMax,
                AnimTime = animTime,
                MinValue = 0.0,
                MaxValue = 0.0
            };
        }

        var frames = new List<ExpressionFrame>();
        double minVal = double.MaxValue;
        double maxVal = double.MinValue;
        double tSpan = tMax - tMin;

        for (int i = 0; i < resolution; i++)
        {
            double t = tMin + (double)i / (double)(resolution - 1) * tSpan;
            double value = expression(t);

            if (value < minVal) minVal = value;
            if (value > maxVal) maxVal = value;

            double normalizedX = (double)i / (double)(resolution - 1);
            double normalizedY = 0.0;

            frames.Add(new ExpressionFrame
            {
                Time = t,
                Value = value,
                X = normalizedX,
                Y = normalizedY
            });
        }

        double valRange = maxVal - minVal;
        for (int i = 0; i < frames.Count; i++)
        {
            double normY = valRange > 1e-15
                ? (frames[i].Value - minVal) / valRange
                : 0.5;

            frames[i] = new ExpressionFrame
            {
                Time = frames[i].Time,
                Value = frames[i].Value,
                X = frames[i].X,
                Y = normY
            };
        }

        return new ExpressionAnimationData
        {
            Frames = frames,
            TMin = tMin,
            TMax = tMax,
            AnimTime = animTime,
            MinValue = minVal,
            MaxValue = maxVal
        };
    }
}
