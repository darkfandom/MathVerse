namespace MathVerse.Math.AI.ReinforcementLearning;
using System;

/// <summary>Policy for selecting actions using epsilon-greedy strategy in reinforcement learning.</summary>
public sealed class RLPolicy
{
    private readonly Random _rng;

    /// <summary>Gets the exploration rate (probability of choosing a random action).</summary>
    public double Epsilon { get; init; } = 0.1;

    /// <summary>Gets the random seed used for action selection.</summary>
    public int RandomSeed { get; init; } = 42;

    /// <summary>Initializes a new instance with the configured epsilon and random seed.</summary>
    public RLPolicy()
    {
        _rng = new Random(RandomSeed);
    }

    /// <summary>Initializes a new instance with the specified epsilon and random seed.</summary>
    /// <param name="epsilon">Exploration rate.</param>
    /// <param name="seed">Random seed.</param>
    public RLPolicy(double epsilon, int seed)
    {
        Epsilon = epsilon;
        RandomSeed = seed;
        _rng = new Random(seed);
    }

    /// <summary>
    /// Selects an action for the given state using epsilon-greedy strategy.
    /// With probability epsilon a random valid action is chosen; otherwise the greedy action is chosen.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="validActions">The set of valid actions in the current state.</param>
    /// <param name="qValues">Function returning Q-values for the state across all actions.</param>
    /// <returns>The selected action.</returns>
    public RLAction GetAction(RLState state, RLAction[] validActions, Func<RLState, RLAction[], double[]> qValues)
    {
        if (validActions.Length == 0)
            return new RLAction { Id = -1, Name = "noop" };

        if (_rng.NextDouble() < Epsilon)
        {
            int randomIdx = _rng.Next(validActions.Length);
            return validActions[randomIdx];
        }

        double[] qVals = qValues(state, validActions);
        int bestIdx = 0;
        for (int i = 1; i < qVals.Length; i++)
        {
            if (qVals[i] > qVals[bestIdx])
                bestIdx = i;
        }
        return validActions[bestIdx];
    }
}
