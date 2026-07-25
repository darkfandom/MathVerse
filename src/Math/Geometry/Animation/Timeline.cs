namespace MathVerse.Math.Geometry.Animation;

using System.Collections.Immutable;

/// <summary>Represents a timeline of keyframes for animation.</summary>
public sealed class Timeline
{
    private readonly List<Keyframe> _keyframes = [];
    private InterpolationMode _mode = InterpolationMode.Linear;
    
    /// <summary>Initializes an empty timeline.</summary>
    public Timeline() { }
    
    /// <summary>Initializes a timeline with keyframes.</summary>
    public Timeline(IEnumerable<Keyframe> keyframes)
    {
        _keyframes.AddRange(keyframes);
        _keyframes.Sort(Comparer<Keyframe>.Create((a, b) => a.Time.CompareTo(b.Time)));
    }
    
    /// <summary>Interpolation mode.</summary>
    public InterpolationMode Mode
    {
        get => _mode;
        set => _mode = value;
    }
    
    /// <summary>Keyframe count.</summary>
    public int Count => _keyframes.Count;
    
    /// <summary>Start time.</summary>
    public double StartTime => _keyframes.Count > 0 ? _keyframes[0].Time : 0;
    
    /// <summary>End time.</summary>
    public double EndTime => _keyframes.Count > 0 ? _keyframes[^1].Time : 0;
    
    /// <summary>Duration.</summary>
    public double Duration => EndTime - StartTime;
    
    /// <summary>Adds a keyframe.</summary>
    public void AddKeyframe(double time, double value)
    {
        _keyframes.Add(new Keyframe(time, value));
        _keyframes.Sort(Comparer<Keyframe>.Create((a, b) => a.Time.CompareTo(b.Time)));
    }
    
    /// <summary>Evaluates the timeline at time t.</summary>
    public double Evaluate(double t)
    {
        if (_keyframes.Count == 0) return 0;
        if (_keyframes.Count == 1) return _keyframes[0].Value;
        
        if (t <= _keyframes[0].Time) return _keyframes[0].Value;
        if (t >= _keyframes[^1].Time) return _keyframes[^1].Value;
        
        int idx = 0;
        for (int i = 0; i < _keyframes.Count - 1; i++)
        {
            if (t >= _keyframes[i].Time && t <= _keyframes[i + 1].Time) { idx = i; break; }
        }
        
        double t0 = _keyframes[idx].Time;
        double t1 = _keyframes[idx + 1].Time;
        double v0 = _keyframes[idx].Value;
        double v1 = _keyframes[idx + 1].Value;
        double localT = (t1 - t0) > 1e-15 ? (t - t0) / (t1 - t0) : 0;
        
        return _mode switch
        {
            InterpolationMode.Linear => v0 + (v1 - v0) * localT,
            InterpolationMode.SmoothStep => v0 + (v1 - v0) * localT * localT * (3 - 2 * localT),
            InterpolationMode.Step => v0,
            InterpolationMode.Cubic => v0 + (v1 - v0) * localT * localT * localT * (localT * (6 * localT - 15) + 10),
            _ => v0 + (v1 - v0) * localT
        };
    }
    
    /// <summary>Returns all keyframes.</summary>
    public IReadOnlyList<Keyframe> GetKeyframes() => _keyframes.ToArray();
}
