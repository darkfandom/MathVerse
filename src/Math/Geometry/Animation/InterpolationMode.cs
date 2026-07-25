namespace MathVerse.Math.Geometry.Animation;

/// <summary>Interpolation mode for keyframe animation.</summary>
public enum InterpolationMode
{
    /// <summary>Linear interpolation.</summary>
    Linear,
    
    /// <summary>Smooth step interpolation.</summary>
    SmoothStep,
    
    /// <summary>Step function (no interpolation).</summary>
    Step,
    
    /// <summary>Cubic Hermite interpolation.</summary>
    Cubic
}
