namespace MathVerse.Math.AI.Recommendation;

using System.Collections.Immutable;

/// <summary>Recommends optimization methods based on function properties such as smoothness, dimensionality, constraints, and noise.</summary>
public sealed class OptimizationRecommendationEngine
{
    private readonly List<OptimizationMethodProfile> _methods = [];

    /// <summary>Initializes a new instance of the <see cref="OptimizationRecommendationEngine"/> class with built-in methods.</summary>
    public OptimizationRecommendationEngine()
    {
        RegisterMethod(new OptimizationMethodProfile
        {
            Name = "Adam",
            Family = "GradientBased",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.85)
                .Add("dimensionality", 0.9)
                .Add("noiseLevel", 0.7)
                .Add("constraintCount", 0.2),
            RequiresGradient = true,
            GlobalCapability = 0.3,
            BestForLargeScale = true
        });

        RegisterMethod(new OptimizationMethodProfile
        {
            Name = "L-BFGS",
            Family = "GradientBased",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.95)
                .Add("dimensionality", 0.7)
                .Add("noiseLevel", 0.3)
                .Add("constraintCount", 0.3),
            RequiresGradient = true,
            GlobalCapability = 0.2,
            BestForLargeScale = false
        });

        RegisterMethod(new OptimizationMethodProfile
        {
            Name = "NelderMead",
            Family = "GradientFree",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.3)
                .Add("dimensionality", 0.4)
                .Add("noiseLevel", 0.8)
                .Add("constraintCount", 0.4),
            RequiresGradient = false,
            GlobalCapability = 0.35,
            BestForLargeScale = false
        });

        RegisterMethod(new OptimizationMethodProfile
        {
            Name = "DifferentialEvolution",
            Family = "Evolutionary",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.2)
                .Add("dimensionality", 0.6)
                .Add("noiseLevel", 0.85)
                .Add("constraintCount", 0.7)
                .Add("multimodality", 0.95),
            RequiresGradient = false,
            GlobalCapability = 0.95,
            BestForLargeScale = false
        });

        RegisterMethod(new OptimizationMethodProfile
        {
            Name = "ParticleSwarm",
            Family = "Evolutionary",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.4)
                .Add("dimensionality", 0.7)
                .Add("noiseLevel", 0.7)
                .Add("constraintCount", 0.5)
                .Add("multimodality", 0.85),
            RequiresGradient = false,
            GlobalCapability = 0.85,
            BestForLargeScale = true
        });

        RegisterMethod(new OptimizationMethodProfile
        {
            Name = "SQP",
            Family = "GradientBased",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.9)
                .Add("dimensionality", 0.5)
                .Add("noiseLevel", 0.2)
                .Add("constraintCount", 0.95),
            RequiresGradient = true,
            GlobalCapability = 0.15,
            BestForLargeScale = false
        });

        RegisterMethod(new OptimizationMethodProfile
        {
            Name = "SimulatedAnnealing",
            Family = "Metaheuristic",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.15)
                .Add("dimensionality", 0.5)
                .Add("noiseLevel", 0.9)
                .Add("constraintCount", 0.6)
                .Add("multimodality", 0.9),
            RequiresGradient = false,
            GlobalCapability = 0.8,
            BestForLargeScale = false
        });
    }

    /// <summary>Registers a new optimization method profile for consideration.</summary>
    /// <param name="profile">The optimization method profile to register.</param>
    public void RegisterMethod(OptimizationMethodProfile profile)
    {
        _methods.Add(profile);
    }

    /// <summary>Recommends optimization methods ranked by suitability for the given problem properties.</summary>
    /// <param name="smoothness">How smooth the objective function is (0 = non-smooth, 1 = C-infinity).</param>
    /// <param name="dimensionality">Number of decision variables normalized to [0, 1].</param>
    /// <param name="constraintCount">Number of constraints normalized to [0, 1].</param>
    /// <param name="noiseLevel">Amount of noise in function evaluations (0 = deterministic, 1 = very noisy).</param>
    /// <param name="multimodality">Degree of multimodality (0 = unimodal, 1 = highly multimodal).</param>
    /// <returns>A list of optimization method recommendations ordered by descending confidence.</returns>
    public List<OptimizationMethodRecommendation> Recommend(
        double smoothness,
        double dimensionality,
        double constraintCount,
        double noiseLevel,
        double multimodality = 0.0)
    {
        var characteristics = ImmutableDictionary<string, double>.Empty
            .Add("smoothness", smoothness)
            .Add("dimensionality", dimensionality)
            .Add("constraintCount", constraintCount)
            .Add("noiseLevel", noiseLevel)
            .Add("multimodality", multimodality);

        var results = new List<OptimizationMethodRecommendation>();

        foreach (var method in _methods)
        {
            double confidence = ScoreMethod(method, characteristics);
            if (confidence <= 0.0)
                continue;

            string reason = Explain(method, smoothness, noiseLevel, multimodality, constraintCount, confidence);

            results.Add(new OptimizationMethodRecommendation
            {
                MethodName = method.Name,
                Family = method.Family,
                Confidence = confidence,
                RequiresGradient = method.RequiresGradient,
                Reason = reason
            });
        }

        results.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
        return results;
    }

    /// <summary>Computes a suitability score for a method given the problem characteristics.</summary>
    /// <param name="method">The method profile to score.</param>
    /// <param name="characteristics">Problem characteristics.</param>
    /// <returns>A confidence score between 0 and 1.</returns>
    private static double ScoreMethod(OptimizationMethodProfile method, ImmutableDictionary<string, double> characteristics)
    {
        double totalWeight = 0.0;
        double weightedSum = 0.0;

        foreach (var kvp in characteristics)
        {
            if (method.Strengths.TryGetValue(kvp.Key, out double strength))
            {
                double clampedValue = System.Math.Max(0.0, System.Math.Min(1.0, kvp.Value));
                weightedSum += strength * clampedValue;
                totalWeight += clampedValue;
            }
        }

        if (totalWeight < 1e-10)
        {
            return 0.1;
        }

        double featureScore = weightedSum / totalWeight;
        double globalBonus = method.GlobalCapability * 0.15;
        double result = featureScore * 0.85 + globalBonus;

        return System.Math.Max(0.0, System.Math.Min(1.0, result));
    }

    /// <summary>Builds an explanation for the recommendation based on dominant factors.</summary>
    /// <param name="method">The method profile.</param>
    /// <param name="smoothness">Function smoothness.</param>
    /// <param name="noiseLevel">Noise level.</param>
    /// <param name="multimodality">Multimodality degree.</param>
    /// <param name="constraintCount">Constraint count.</param>
    /// <param name="confidence">Computed confidence.</param>
    /// <returns>An explanation string.</returns>
    private static string Explain(
        OptimizationMethodProfile method,
        double smoothness,
        double noiseLevel,
        double multimodality,
        double constraintCount,
        double confidence)
    {
        var reasons = new List<string>();

        if (smoothness > 0.7 && method.RequiresGradient)
        {
            reasons.Add("function is smooth enabling gradient exploitation");
        }
        else if (smoothness < 0.3 && !method.RequiresGradient)
        {
            reasons.Add("gradient-free approach suits non-smooth objective");
        }

        if (noiseLevel > 0.6)
        {
            reasons.Add("robust to noisy evaluations");
        }

        if (multimodality > 0.6 && method.GlobalCapability > 0.7)
        {
            reasons.Add("strong global search for multimodal landscape");
        }

        if (constraintCount > 0.7)
        {
            reasons.Add("handles constraints well");
        }

        string quality = confidence switch
        {
            >= 0.8 => "excellent",
            >= 0.6 => "good",
            >= 0.4 => "moderate",
            _ => "marginal"
        };

        if (reasons.Count > 0)
        {
            return $"{method.Name} ({method.Family}) is a {quality} fit: {string.Join("; ", reasons)}.";
        }

        return $"{method.Name} ({method.Family}) provides a {quality} general-purpose match.";
    }
}

/// <summary>Describes an optimization method's characteristics and applicability.</summary>
public sealed class OptimizationMethodProfile
{
    /// <summary>Gets the method name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the method family (e.g., "GradientBased", "GradientFree", "Evolutionary", "Metaheuristic").</summary>
    public string Family { get; init; } = "";

    /// <summary>Gets the strength profile mapping problem features to method suitability.</summary>
    public ImmutableDictionary<string, double> Strengths { get; init; } = ImmutableDictionary<string, double>.Empty;

    /// <summary>Gets whether the method requires gradient information.</summary>
    public bool RequiresGradient { get; init; }

    /// <summary>Gets the method's capability for global optimization between 0 and 1.</summary>
    public double GlobalCapability { get; init; }

    /// <summary>Gets whether the method scales well to large problems.</summary>
    public bool BestForLargeScale { get; init; }
}

/// <summary>Represents a ranked optimization method recommendation with reasoning.</summary>
public sealed class OptimizationMethodRecommendation
{
    /// <summary>Gets the name of the recommended method.</summary>
    public string MethodName { get; init; } = "";

    /// <summary>Gets the method family.</summary>
    public string Family { get; init; } = "";

    /// <summary>Gets the confidence score between 0 and 1.</summary>
    public double Confidence { get; init; }

    /// <summary>Gets whether the method requires gradient information.</summary>
    public bool RequiresGradient { get; init; }

    /// <summary>Gets a human-readable explanation for the recommendation.</summary>
    public string Reason { get; init; } = "";
}
