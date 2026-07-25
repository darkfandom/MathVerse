namespace MathVerse.Math.AI.Optimization;

/// <summary>Factory for creating optimizer instances by name.</summary>
public static class OptimizerFactory
{
    /// <summary>Creates an optimizer by its short name.</summary>
    /// <param name="name">The optimizer name (GD, SGD, Momentum, Nesterov, Adam, AdamW, RMSProp, AdaGrad, AdaDelta, LBFGS, Newton, TrustRegion, CoordinateDescent, SA, PSO, GA).</param>
    /// <returns>A new optimizer instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the name is not recognized.</exception>
    public static IOptimizer Create(string name)
    {
        return name switch
        {
            "GD" => new GradientDescentOptimizer(),
            "SGD" => new StochasticGradientDescentOptimizer(),
            "Momentum" => new MomentumOptimizer(),
            "Nesterov" => new NesterovOptimizer(),
            "Adam" => new AdamOptimizer(),
            "AdamW" => new AdamWOptimizer(),
            "RMSProp" => new RMSPropOptimizer(),
            "AdaGrad" => new AdaGradOptimizer(),
            "AdaDelta" => new AdaDeltaOptimizer(),
            "LBFGS" => new LBFGSOptimizer(),
            "Newton" => new NewtonOptimizer(),
            "TrustRegion" => new TrustRegionOptimizer(),
            "CoordinateDescent" => new CoordinateDescentOptimizer(),
            "SA" => new SimulatedAnnealingOptimizer(),
            "PSO" => new ParticleSwarmOptimizer(),
            "GA" => new GeneticOptimizer(),
            _ => throw new ArgumentException($"Unknown optimizer: '{name}'.", nameof(name))
        };
    }

    /// <summary>Gets all supported optimizer names.</summary>
    /// <returns>An array of supported optimizer short names.</returns>
    public static string[] GetSupportedNames()
    {
        return ["GD", "SGD", "Momentum", "Nesterov", "Adam", "AdamW", "RMSProp",
                "AdaGrad", "AdaDelta", "LBFGS", "Newton", "TrustRegion",
                "CoordinateDescent", "SA", "PSO", "GA"];
    }
}
