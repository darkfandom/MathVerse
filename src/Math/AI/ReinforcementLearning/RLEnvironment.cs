namespace MathVerse.Math.AI.ReinforcementLearning;

/// <summary>Defines a reinforcement learning environment with step, reset, and valid-action logic.</summary>
public sealed class RLEnvironment
{
    /// <summary>Gets or sets the function that advances the environment by one step.</summary>
    public Func<RLState, RLAction, (RLState nextState, double reward)> Step { get; init; } = (_, _) => (new RLState(), 0.0);

    /// <summary>Gets or sets the function that resets the environment to an initial state.</summary>
    public Func<RLState> Reset { get; init; } = () => new RLState();

    /// <summary>Gets or sets the function that returns valid actions for a given state.</summary>
    public Func<RLState, RLAction[]> GetValidActions { get; init; } = _ => [];

    /// <summary>Gets or sets the dimensionality of the state feature vector.</summary>
    public int StateSize { get; init; }

    /// <summary>Gets or sets the total number of possible actions.</summary>
    public int ActionSize { get; init; }
}
