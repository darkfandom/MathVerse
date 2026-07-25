namespace MathVerse.Math.AI.Recommendation;

using System.Collections.Immutable;

/// <summary>Recommends numerical methods for integration, differentiation, and root finding based on function properties.</summary>
public sealed class NumericalMethodRecommendation
{
    private readonly List<NumericalMethodProfile> _methods = [];

    /// <summary>Initializes a new instance of the <see cref="NumericalMethodRecommendation"/> class with built-in methods.</summary>
    public NumericalMethodRecommendation()
    {
        RegisterMethod(new NumericalMethodProfile
        {
            Name = "AdaptiveSimpson",
            Category = "Integration",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.9)
                .Add("periodicity", 0.3)
                .Add("singularityCount", 0.1)
                .Add("dimensionality", 0.2),
            ComplexityNote = "Adaptive refinement near discontinuities",
            MinSamplePoints = 5,
            MaxDimensionality = 1
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "GaussLegendre",
            Category = "Integration",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.95)
                .Add("periodicity", 0.4)
                .Add("singularityCount", 0.2)
                .Add("dimensionality", 0.5),
            ComplexityNote = "High accuracy with few function evaluations for smooth integrands",
            MinSamplePoints = 4,
            MaxDimensionality = 3
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "MonteCarlo",
            Category = "Integration",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.2)
                .Add("periodicity", 0.3)
                .Add("singularityCount", 0.6)
                .Add("dimensionality", 0.95),
            ComplexityNote = "Convergence rate independent of dimensionality",
            MinSamplePoints = 1000,
            MaxDimensionality = 1000
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "Romberg",
            Category = "Integration",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.9)
                .Add("periodicity", 0.2)
                .Add("singularityCount", 0.15)
                .Add("dimensionality", 0.15),
            ComplexityNote = "Richardson extrapolation for high-order convergence",
            MinSamplePoints = 8,
            MaxDimensionality = 1
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "CentralDifference",
            Category = "Differentiation",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.8)
                .Add("periodicity", 0.5)
                .Add("singularityCount", 0.1)
                .Add("noiseLevel", 0.2),
            ComplexityNote = "O(h^2) accuracy, sensitive to noise",
            MinSamplePoints = 3,
            MaxDimensionality = 1
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "ComplexStep",
            Category = "Differentiation",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.95)
                .Add("periodicity", 0.5)
                .Add("singularityCount", 0.2)
                .Add("noiseLevel", 0.9),
            ComplexityNote = "Machine-precision accuracy, immune to subtractive cancellation",
            MinSamplePoints = 2,
            MaxDimensionality = 1
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "AutomaticDifferentiation",
            Category = "Differentiation",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.85)
                .Add("periodicity", 0.4)
                .Add("singularityCount", 0.3)
                .Add("dimensionality", 0.9),
            ComplexityNote = "Exact derivatives up to machine precision via dual numbers",
            MinSamplePoints = 1,
            MaxDimensionality = 10000
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "BrentRootFinder",
            Category = "RootFinding",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.7)
                .Add("rootsCount", 0.5)
                .Add("bracketAvailable", 1.0)
                .Add("singularityCount", 0.4),
            ComplexityNote = "Combines bisection, secant, and inverse quadratic interpolation",
            MinSamplePoints = 2,
            MaxDimensionality = 1
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "NewtonRaphson",
            Category = "RootFinding",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.95)
                .Add("rootsCount", 0.3)
                .Add("bracketAvailable", 0.1)
                .Add("derivativeAvailable", 1.0)
                .Add("singularityCount", 0.1),
            ComplexityNote = "Quadratic convergence near simple roots",
            MinSamplePoints = 1,
            MaxDimensionality = 1
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "MullerMethod",
            Category = "RootFinding",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.6)
                .Add("rootsCount", 0.7)
                .Add("bracketAvailable", 0.3)
                .Add("derivativeAvailable", 0.1)
                .Add("singularityCount", 0.3),
            ComplexityNote = "Can find complex roots, no derivative required",
            MinSamplePoints = 3,
            MaxDimensionality = 1
        });

        RegisterMethod(new NumericalMethodProfile
        {
            Name = "BroydenRootFinder",
            Category = "RootFinding",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.7)
                .Add("rootsCount", 0.4)
                .Add("bracketAvailable", 0.1)
                .Add("derivativeAvailable", 0.2)
                .Add("dimensionality", 0.85),
            ComplexityNote = "Quasi-Newton method for systems of nonlinear equations",
            MinSamplePoints = 1,
            MaxDimensionality = 500
        });
    }

    /// <summary>Registers a new numerical method profile for consideration.</summary>
    /// <param name="profile">The method profile to register.</param>
    public void RegisterMethod(NumericalMethodProfile profile)
    {
        _methods.Add(profile);
    }

    /// <summary>Recommends numerical methods for the given category and function properties.</summary>
    /// <param name="category">The method category: "Integration", "Differentiation", or "RootFinding".</param>
    /// <param name="functionProperties">Key-value pairs describing the target function.</param>
    /// <returns>A list of method recommendations ordered by descending confidence.</returns>
    public List<NumericalMethodRecommendationResult> Recommend(string category, ImmutableDictionary<string, double> functionProperties)
    {
        var results = new List<NumericalMethodRecommendationResult>();

        foreach (var method in _methods)
        {
            if (!method.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                continue;

            double confidence = ScoreMethod(method, functionProperties);
            if (confidence <= 0.0)
                continue;

            string reason = BuildReason(method, functionProperties, confidence);

            results.Add(new NumericalMethodRecommendationResult
            {
                MethodName = method.Name,
                Category = method.Category,
                Confidence = confidence,
                ComplexityNote = method.ComplexityNote,
                Reason = reason
            });
        }

        results.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
        return results;
    }

    /// <summary>Computes a suitability score for a method given function properties.</summary>
    /// <param name="method">The method profile.</param>
    /// <param name="properties">Function properties.</param>
    /// <returns>A confidence score between 0 and 1.</returns>
    private static double ScoreMethod(NumericalMethodProfile method, ImmutableDictionary<string, double> properties)
    {
        double totalWeight = 0.0;
        double weightedSum = 0.0;

        foreach (var kvp in properties)
        {
            if (method.Strengths.TryGetValue(kvp.Key, out double strength))
            {
                double value = System.Math.Max(0.0, System.Math.Min(1.0, kvp.Value));
                weightedSum += strength * value;
                totalWeight += value;
            }
        }

        if (totalWeight < 1e-10)
        {
            return 0.15;
        }

        double featureScore = weightedSum / totalWeight;
        return System.Math.Max(0.0, System.Math.Min(1.0, featureScore));
    }

    /// <summary>Builds an explanation for the recommendation based on dominant function properties.</summary>
    /// <param name="method">The method profile.</param>
    /// <param name="properties">Function properties.</param>
    /// <param name="confidence">Computed confidence.</param>
    /// <returns>An explanation string.</returns>
    private static string BuildReason(NumericalMethodProfile method, ImmutableDictionary<string, double> properties, double confidence)
    {
        var strengths = new List<string>();

        foreach (var kvp in properties)
        {
            if (method.Strengths.TryGetValue(kvp.Key, out double strength) && strength > 0.6 && kvp.Value > 0.3)
            {
                strengths.Add($"{kvp.Key}={kvp.Value:F2}");
            }
        }

        string quality = confidence switch
        {
            >= 0.8 => "excellent",
            >= 0.6 => "good",
            >= 0.4 => "adequate",
            _ => "marginal"
        };

        if (strengths.Count > 0)
        {
            return $"{method.Name} is a {quality} match for [{string.Join(", ", strengths)}]. {method.ComplexityNote}.";
        }

        return $"{method.Name} provides a {quality} general match. {method.ComplexityNote}.";
    }
}

/// <summary>Describes a numerical method's strengths and applicability constraints.</summary>
public sealed class NumericalMethodProfile
{
    /// <summary>Gets the method name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the method category: "Integration", "Differentiation", or "RootFinding".</summary>
    public string Category { get; init; } = "";

    /// <summary>Gets the strength profile mapping function properties to method suitability.</summary>
    public ImmutableDictionary<string, double> Strengths { get; init; } = ImmutableDictionary<string, double>.Empty;

    /// <summary>Gets a note describing the method's complexity or convergence characteristics.</summary>
    public string ComplexityNote { get; init; } = "";

    /// <summary>Gets the minimum number of sample points or evaluations required.</summary>
    public int MinSamplePoints { get; init; }

    /// <summary>Gets the maximum dimensionality the method supports.</summary>
    public int MaxDimensionality { get; init; }
}

/// <summary>Represents a ranked numerical method recommendation.</summary>
public sealed class NumericalMethodRecommendationResult
{
    /// <summary>Gets the method name.</summary>
    public string MethodName { get; init; } = "";

    /// <summary>Gets the method category.</summary>
    public string Category { get; init; } = "";

    /// <summary>Gets the confidence score between 0 and 1.</summary>
    public double Confidence { get; init; }

    /// <summary>Gets a note describing the method's complexity or convergence.</summary>
    public string ComplexityNote { get; init; } = "";

    /// <summary>Gets a human-readable explanation for the recommendation.</summary>
    public string Reason { get; init; } = "";
}
