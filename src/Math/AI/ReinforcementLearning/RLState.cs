namespace MathVerse.Math.AI.ReinforcementLearning;
using System.Linq;

/// <summary>Represents a state in a reinforcement learning environment.</summary>
public sealed class RLState
{
    /// <summary>Gets the feature vector describing this state.</summary>
    public double[] Features { get; init; } = [];

    /// <summary>Gets whether this is a terminal (absorbing) state.</summary>
    public bool IsTerminal { get; init; }

    /// <summary>Gets a string key uniquely identifying this state based on its features.</summary>
    public string StateKey => string.Join(",", Features.Select(x => x.ToString("G4")));
}
