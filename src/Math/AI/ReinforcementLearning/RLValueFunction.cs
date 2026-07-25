namespace MathVerse.Math.AI.ReinforcementLearning;
using System;
using System.Collections.Generic;

/// <summary>Tabular Q-value function for reinforcement learning.</summary>
public sealed class RLValueFunction
{
    private readonly Dictionary<string, double[]> _qTable = new();

    /// <summary>
    /// Gets the Q-value for the given state-action pair.
    /// Returns 0.0 if the state has not been visited.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>The Q-value Q(s, a).</returns>
    public double GetValue(RLState state, RLAction action)
    {
        string key = state.StateKey;
        if (_qTable.TryGetValue(key, out double[]? values) && action.Id >= 0 && action.Id < values.Length)
        {
            return values[action.Id];
        }
        return 0.0;
    }

    /// <summary>
    /// Sets the Q-value for the given state-action pair.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <param name="value">The new Q-value.</param>
    public void SetValue(RLState state, RLAction action, double value)
    {
        string key = state.StateKey;
        if (!_qTable.TryGetValue(key, out double[]? values))
        {
            int size = System.Math.Max(action.Id + 1, 10);
            values = new double[size];
            _qTable[key] = values;
        }
        if (action.Id >= values.Length)
        {
            double[] newValues = new double[action.Id + 1];
            Array.Copy(values, newValues, values.Length);
            values = newValues;
            _qTable[key] = values;
        }
        values[action.Id] = value;
    }

    /// <summary>
    /// Gets all Q-values for the given state, returning an array indexed by action ID.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="numActions">The total number of possible actions.</param>
    /// <returns>An array of Q-values indexed by action ID.</returns>
    public double[] GetQValues(RLState state, int numActions)
    {
        string key = state.StateKey;
        if (_qTable.TryGetValue(key, out double[]? values))
        {
            if (values.Length >= numActions)
                return values;

            double[] padded = new double[numActions];
            Array.Copy(values, padded, values.Length);
            return padded;
        }

        return new double[numActions];
    }

    /// <summary>
    /// Returns the action with the highest Q-value among the valid actions for the given state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="validActions">The set of valid actions.</param>
    /// <returns>The best action according to current Q-values.</returns>
    public RLAction GetBestAction(RLState state, RLAction[] validActions)
    {
        if (validActions.Length == 0)
            return new RLAction { Id = -1, Name = "noop" };

        RLAction best = validActions[0];
        double bestValue = GetValue(state, best);

        for (int i = 1; i < validActions.Length; i++)
        {
            double val = GetValue(state, validActions[i]);
            if (val > bestValue)
            {
                bestValue = val;
                best = validActions[i];
            }
        }

        return best;
    }

    /// <summary>Gets the total number of states stored in the Q-table.</summary>
    public int StateCount => _qTable.Count;
}
