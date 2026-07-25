namespace MathVerse.Math.AI.ReinforcementLearning;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

/// <summary>
/// First-visit Monte Carlo agent that learns Q-values from complete episode returns.
/// No bootstrapping is used; Q(s,a) is updated as the average of all returns
/// observed after the first visit to (s,a).
/// </summary>
public sealed class MonteCarloAgent
{
    private readonly Random _rng;
    private RLValueFunction _qTable = new();

    /// <summary>Initializes a new instance with the default random seed.</summary>
    public MonteCarloAgent()
    {
        _rng = new Random(42);
    }

    /// <summary>Initializes a new instance with the specified random seed.</summary>
    public MonteCarloAgent(int seed)
    {
        _rng = new Random(seed);
    }

    /// <summary>
    /// Trains the Monte Carlo agent on the given environment.
    /// </summary>
    /// <param name="env">The reinforcement learning environment.</param>
    /// <param name="episodes">Number of training episodes.</param>
    /// <param name="alpha">Learning rate (used for incremental updates).</param>
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
        Dictionary<string, int> visitCount = new Dictionary<string, int>();
        RLPolicy policy = new RLPolicy(epsilon, _rng.Next());
        List<double> episodeRewards = new List<double>();
        List<int> episodeLengths = new List<int>();

        for (int ep = 0; ep < episodes; ep++)
        {
            List<(RLState state, RLAction action)> trajectory = new List<(RLState, RLAction)>();
            List<double> rewards = new List<double>();

            RLState state = env.Reset();
            int steps = 0;

            while (!state.IsTerminal && steps < 10000)
            {
                RLAction[] validActions = env.GetValidActions(state);
                if (validActions.Length == 0)
                    break;

                RLAction action = SelectAction(state, validActions, _qTable, epsilon);
                trajectory.Add((state, action));

                (RLState nextState, double reward) = env.Step(state, action);
                rewards.Add(reward);
                state = nextState;
                steps++;
            }

            double G = 0.0;
            Dictionary<string, double> firstVisit = new Dictionary<string, double>();
            HashSet<string> seen = new HashSet<string>();

            for (int t = trajectory.Count - 1; t >= 0; t--)
            {
                G = gamma * G + rewards[t];
                string saKey = trajectory[t].state.StateKey + "|" + trajectory[t].action.Id;

                if (!seen.Contains(saKey))
                {
                    seen.Add(saKey);

                    double currentQ = _qTable.GetValue(trajectory[t].state, trajectory[t].action);
                    string countKey = saKey;

                    if (!visitCount.TryGetValue(countKey, out int count))
                        count = 0;

                    count++;
                    visitCount[countKey] = count;

                    double newQ = currentQ + alpha * (G - currentQ);
                    _qTable.SetValue(trajectory[t].state, trajectory[t].action, newQ);
                }
            }

            double totalReward = 0.0;
            for (int i = 0; i < rewards.Count; i++)
                totalReward += rewards[i];

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

        bool converged = false;
        if (episodeRewards.Count >= 20)
        {
            double recentAvg = 0.0;
            double olderAvg = 0.0;
            int halfWindow = 10;
            for (int i = episodeRewards.Count - halfWindow; i < episodeRewards.Count; i++)
                recentAvg += episodeRewards[i];
            for (int i = episodeRewards.Count - 2 * halfWindow; i < episodeRewards.Count - halfWindow; i++)
                olderAvg += episodeRewards[i];
            recentAvg /= halfWindow;
            olderAvg /= halfWindow;
            converged = System.Math.Abs(recentAvg - olderAvg) < 0.01 * (System.Math.Abs(olderAvg) + 1e-8);
        }

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
                .Add("algorithm", 2.0)
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

    private RLAction SelectAction(RLState state, RLAction[] validActions, RLValueFunction qTable, double epsilon)
    {
        if (_rng.NextDouble() < epsilon)
        {
            return validActions[_rng.Next(validActions.Length)];
        }

        RLAction best = validActions[0];
        double bestValue = qTable.GetValue(state, best);
        for (int i = 1; i < validActions.Length; i++)
        {
            double val = qTable.GetValue(state, validActions[i]);
            if (val > bestValue)
            {
                bestValue = val;
                best = validActions[i];
            }
        }
        return best;
    }
}
