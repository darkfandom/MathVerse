namespace MathVerse.Math.Geometry.Animation;

/// <summary>Represents a keyframe with time and value.</summary>
public readonly record struct Keyframe(double Time, double Value)
{
    /// <summary>Keyframe time.</summary>
    public double Time { get; } = Time;
    
    /// <summary>Keyframe value.</summary>
    public double Value { get; } = Value;
}
