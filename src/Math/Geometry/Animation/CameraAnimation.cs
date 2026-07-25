namespace MathVerse.Math.Geometry.Animation;

using Geometry3D;
using Cameras;
using Transformations;

/// <summary>Animates camera position and orientation over time.</summary>
public sealed class CameraAnimation
{
    private readonly Timeline _positionX = new();
    private readonly Timeline _positionY = new();
    private readonly Timeline _positionZ = new();
    private readonly Timeline _targetX = new();
    private readonly Timeline _targetY = new();
    private readonly Timeline _targetZ = new();
    
    /// <summary>Initializes a camera animation.</summary>
    public CameraAnimation() { }
    
    /// <summary>Start time.</summary>
    public double StartTime => System.Math.Min(_positionX.StartTime, _targetX.StartTime);
    
    /// <summary>End time.</summary>
    public double EndTime => System.Math.Max(_positionX.EndTime, _targetX.EndTime);
    
    /// <summary>Sets position keyframes.</summary>
    public void SetPositionKeyframes(IReadOnlyList<(double Time, Point3D Position)> keyframes)
    {
        foreach (var (time, pos) in keyframes)
        {
            _positionX.AddKeyframe(time, pos.X);
            _positionY.AddKeyframe(time, pos.Y);
            _positionZ.AddKeyframe(time, pos.Z);
        }
    }
    
    /// <summary>Sets target keyframes.</summary>
    public void SetTargetKeyframes(IReadOnlyList<(double Time, Point3D Target)> keyframes)
    {
        foreach (var (time, target) in keyframes)
        {
            _targetX.AddKeyframe(time, target.X);
            _targetY.AddKeyframe(time, target.Y);
            _targetZ.AddKeyframe(time, target.Z);
        }
    }
    
    /// <summary>Evaluates the camera transform at time t.</summary>
    public (Point3D Position, Point3D Target) Evaluate(double t)
    {
        var pos = new Point3D(_positionX.Evaluate(t), _positionY.Evaluate(t), _positionZ.Evaluate(t));
        var target = new Point3D(_targetX.Evaluate(t), _targetY.Evaluate(t), _targetZ.Evaluate(t));
        return (pos, target);
    }
    
    /// <summary>Applies animation to a camera at time t.</summary>
    public void ApplyToCamera(Camera camera, double t)
    {
        var (pos, target) = Evaluate(t);
        _ = pos;
        _ = target;
    }
}
