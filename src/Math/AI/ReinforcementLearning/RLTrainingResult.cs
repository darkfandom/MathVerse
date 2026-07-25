namespace MathVerse.Math.AI.ReinforcementLearning;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

/// <summary>Result of a reinforcement learning training session.</summary>
public sealed class RLTrainingResult
{
    /// <summary>Gets the total number of episodes trained.</summary>
    public int EpisodesCompleted { get; init; }

    /// <summary>Gets the list of total rewards per episode.</summary>
    public List<double> EpisodeRewards { get; init; } = [];

    /// <summary>Gets the list of episode lengths (steps per episode).</summary>
    public List<int> EpisodeLengths { get; init; } = [];

    /// <summary>Gets the trained Q-value function.</summary>
    public RLValueFunction ValueFunction { get; init; } = new();

    /// <summary>Gets the trained policy.</summary>
    public RLPolicy Policy { get; init; } = new();

    /// <summary>Gets whether training converged.</summary>
    public bool Converged { get; init; }

    /// <summary>Gets the total training time.</summary>
    public TimeSpan TrainingTime { get; init; }

    /// <summary>Gets the average reward over the last window of episodes.</summary>
    public double AverageReward { get; init; }

    /// <summary>Gets additional metrics from training.</summary>
    public ImmutableDictionary<string, double> Metrics { get; init; } = ImmutableDictionary<string, double>.Empty;
}
