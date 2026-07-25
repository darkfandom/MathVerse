namespace MathVerse.Math.Geometry.Animation;

using SceneGraph;
using Transformations;

/// <summary>Animates scene node transforms over time.</summary>
public sealed class SceneAnimation
{
    private readonly Dictionary<string, Timeline> _channels = [];
    private readonly Dictionary<string, SceneNode> _targetNodes = [];
    
    /// <summary>Initializes a scene animation.</summary>
    public SceneAnimation() { }
    
    /// <summary>Start time.</summary>
    public double StartTime => _channels.Count > 0 ? _channels.Values.Min(c => c.StartTime) : 0;
    
    /// <summary>End time.</summary>
    public double EndTime => _channels.Count > 0 ? _channels.Values.Max(c => c.EndTime) : 0;
    
    /// <summary>Channel count.</summary>
    public int ChannelCount => _channels.Count;
    
    /// <summary>Adds an animation channel for a node's translate X.</summary>
    public void AddTranslationChannel(string nodeName, SceneNode node, Timeline x, Timeline y, Timeline z)
    {
        _targetNodes[nodeName + "_tx"] = node;
        _channels[nodeName + "_tx"] = x;
        _targetNodes[nodeName + "_ty"] = node;
        _channels[nodeName + "_ty"] = y;
        _targetNodes[nodeName + "_tz"] = node;
        _channels[nodeName + "_tz"] = z;
    }
    
    /// <summary>Evaluates and applies animation at time t.</summary>
    public void Evaluate(double t)
    {
        var nodeTransforms = new Dictionary<SceneNode, (double tx, double ty, double tz)>();
        
        foreach (var kvp in _channels)
        {
            if (_targetNodes.TryGetValue(kvp.Key, out var node))
            {
                if (!nodeTransforms.ContainsKey(node))
                    nodeTransforms[node] = (0, 0, 0);
                
                double val = kvp.Value.Evaluate(t);
                if (kvp.Key.EndsWith("_tx"))
                    nodeTransforms[node] = (val, nodeTransforms[node].ty, nodeTransforms[node].tz);
                else if (kvp.Key.EndsWith("_ty"))
                    nodeTransforms[node] = (nodeTransforms[node].tx, val, nodeTransforms[node].tz);
                else if (kvp.Key.EndsWith("_tz"))
                    nodeTransforms[node] = (nodeTransforms[node].tx, nodeTransforms[node].ty, val);
            }
        }
        
        foreach (var kvp in nodeTransforms)
        {
            kvp.Key.LocalTransform = Transform3D.Translation(kvp.Value.tx, kvp.Value.ty, kvp.Value.tz);
        }
    }
}
