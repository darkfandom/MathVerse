namespace MathVerse.Math.AI.ReinforcementLearning;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

/// <summary>
/// On-policy SARSA (State-Action-Reward-State-Action) agent.
/// Updates Q-values using: Q(s,a) += alpha * (r + gamma * Q(s',a') - Q(s,a))
/// where a' is selected from the current policy.
/// </summary>
public sealed class SARSAAgent
{
    private readonly Random _rng;
    private RLValueFunction _qTable = new();

    /// <summary>Initializes a new instance with the default random seed.</summary>
    public SARSAAgent()
    {
        _rng = new Random(42);
    }

    /// <summary>Initializes a new instance with the specified random seed.</summary>
    public SARSAAgent(int seed)
    {
        _rng = new Random(seed);
    }

    /// <summary>
    /// Trains the SARSA agent on the given environment.
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

            RLAction[] validActions = env.GetValidActions(state);
            if (validActions.Length == 0)
            {
                episodeRewards.Add(0.0);
                episodeLengths.Add(0);
                continue;
            }

            RLAction action = SelectAction(state, validActions, _qTable, epsilon);

            while (!state.IsTerminal && steps < 10000)
            {
                (RLState nextState, double reward) = env.Step(state, action);

                RLAction nextAction;
                if (nextState.IsTerminal)
                {
                    nextAction = new RLAction { Id = -1, Name = "terminal" };
                }
                else
                {
                    RLAction[] nextValidActions = env.GetValidActions(nextState);
                    if (nextValidActions.Length == 0)
                    {
                        nextAction = new RLAction { Id = -1, Name = "noop" };
                    }
                    else
                    {
                        nextAction = SelectAction(nextState, nextValidActions, _qTable, epsilon);
                    }
                }

                double currentQ = _qTable.GetValue(state, action);
                double nextQ = nextState.IsTerminal || nextAction.Id < 0
                    ? 0.0
                    : _qTable.GetValue(nextState, nextAction);

                double newQ = currentQ + alpha * (reward + gamma * nextQ - currentQ);
                _qTable.SetValue(state, action, newQ);

                totalReward += reward;
                state = nextState;
                action = nextAction;
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
                .Add("algorithm", 1.0)
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
