namespace MathVerse.Math.AI.ReinforcementLearning;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

/// <summary>
/// Tabular Q-learning agent implementing off-policy temporal difference learning.
/// Updates Q-values using: Q(s,a) += alpha * (r + gamma * max_a' Q(s',a') - Q(s,a)).
/// </summary>
public sealed class QLearningAgent
{
    private readonly Random _rng;
    private RLValueFunction _qTable = new();

    /// <summary>Initializes a new instance with the default random seed.</summary>
    public QLearningAgent()
    {
        _rng = new Random(42);
    }

    /// <summary>Initializes a new instance with the specified random seed.</summary>
    public QLearningAgent(int seed)
    {
        _rng = new Random(seed);
    }

    /// <summary>
    /// Trains the Q-learning agent on the given environment.
    /// </summary>
    /// <param name="env">The reinforcement learning environment.</param>
    /// <param name="episodes">Number of training episodes.</param>
    /// <param name="alpha">Learning rate.</param>
    /// <param name="gamma">Discount factor.</param>
    /// <param name="epsilon">Exploration rate for epsilon-greedy policy.</param>
    /// <returns>An <see cref="RLTrainingResult"/> with training statistics.</returns>
    public RLTrainingResult Train(
        RLEnvironment env,
        int episodes = 1000,
        double alpha = 0.1,
        double gamma = 0.99,
        double epsilon = 0.1)
    {
        Stopwatch sw = Stopwatch.StartNew();
        _qTable = new RLValueFunction();
        RLPolicy policy = new RLPolicy(epsilon, _rng.Next());
        List<double> episodeRewards = new List<double>();
        List<int> episodeLengths = new List<int>();

        for (int ep = 0; ep < episodes; ep++)
        {
            RLState state = env.Reset();
            double totalReward = 0.0;
            int steps = 0;

            while (!state.IsTerminal && steps < 10000)
            {
                RLAction[] validActions = env.GetValidActions(state);
                if (validActions.Length == 0)
                    break;

                RLAction action = policy.GetAction(state, validActions,
                    (s, a) => _qTable.GetQValues(s, a.Length));

                (RLState nextState, double reward) = env.Step(state, action);

                RLAction[] nextValidActions = env.GetValidActions(nextState);
                double maxNextQ = 0.0;
                if (nextValidActions.Length > 0 && !nextState.IsTerminal)
                {
                    double[] nextQValues = _qTable.GetQValues(nextState, nextValidActions.Length);
                    maxNextQ = nextQValues[0];
                    for (int i = 1; i < nextValidActions.Length; i++)
                    {
                        if (i < nextQValues.Length && nextQValues[i] > maxNextQ)
                            maxNextQ = nextQValues[i];
                    }
                }

                double currentQ = _qTable.GetValue(state, action);
                double newQ = currentQ + alpha * (reward + gamma * maxNextQ - currentQ);
                _qTable.SetValue(state, action, newQ);

                totalReward += reward;
                state = nextState;
                steps++;
            }

            episodeRewards.Add(totalReward);
            episodeLengths.Add(steps);
        }

        sw.Stop();

        int windowSize = System.Math.Min(100, episodeRewards.Count);
        double avgReward = 0.0;
        for (int i = episodeRewards.Count - windowSize; i < episodeRewards.Count; i++)
        {
            avgReward += episodeRewards[i];
        }
        if (windowSize > 0)
            avgReward /= windowSize;

        bool converged = ComputeConvergence(episodeRewards);

        return new RLTrainingResult
        {
            EpisodesCompleted = episodes,
            EpisodeRewards = episodeRewards,
            EpisodeLengths = episodeLengths,
            ValueFunction = _qTable,
            Policy = policy,
            Converged = converged,
            TrainingTime = sw.Elapsed,
            AverageReward = avgReward,
            Metrics = ImmutableDictionary<string, double>.Empty
                .Add("finalEpsilon", epsilon)
                .Add("learningRate", alpha)
                .Add("discountFactor", gamma)
        };
    }

    /// <summary>
    /// Plays one episode in the environment using the trained Q-table and greedy policy.
    /// </summary>
    /// <param name="env">The reinforcement learning environment.</param>
    /// <param name="maxSteps">Maximum steps per episode.</param>
    /// <returns>The total accumulated reward.</returns>
    public double Play(RLEnvironment env, int maxSteps = 100)
    {
        RLState state = env.Reset();
        double totalReward = 0.0;
        int steps = 0;

        while (!state.IsTerminal && steps < maxSteps)
        {
            RLAction[] validActions = env.GetValidActions(state);
            if (validActions.Length == 0)
                break;

            RLAction bestAction = _qTable.GetBestAction(state, validActions);
            (RLState nextState, double reward) = env.Step(state, bestAction);
            totalReward += reward;
            state = nextState;
            steps++;
        }

        return totalReward;
    }

    private static bool ComputeConvergence(List<double> rewards)
    {
        if (rewards.Count < 20)
            return false;

        double recentAvg = 0.0;
        double olderAvg = 0.0;
        int halfWindow = 10;
        for (int i = rewards.Count - halfWindow; i < rewards.Count; i++)
            recentAvg += rewards[i];
        for (int i = rewards.Count - 2 * halfWindow; i < rewards.Count - halfWindow; i++)
            olderAvg += rewards[i];
        recentAvg /= halfWindow;
        olderAvg /= halfWindow;
        return System.Math.Abs(recentAvg - olderAvg) < 0.01 * (System.Math.Abs(olderAvg) + 1e-8);
    }
}
