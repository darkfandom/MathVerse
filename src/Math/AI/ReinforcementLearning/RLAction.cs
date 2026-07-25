namespace MathVerse.Math.AI.ReinforcementLearning;

/// <summary>Represents an action in a reinforcement learning environment.</summary>
public sealed class RLAction
{
    /// <summary>Gets the unique integer identifier for this action.</summary>
    public int Id { get; init; }

    /// <summary>Gets the human-readable name of this action.</summary>
    public string Name { get; init; } = "";
}
