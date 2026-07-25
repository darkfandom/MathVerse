namespace MathVerse.Math.AI.Recommendation;

using System.Collections.Immutable;

/// <summary>Recommends algorithms based on data characteristics such as size, dimensionality, and sparsity.</summary>
public sealed class AlgorithmRecommendationEngine
{
    private readonly List<AlgorithmProfile> _algorithms = [];

    /// <summary>Initializes a new instance of the <see cref="AlgorithmRecommendationEngine"/> class with built-in algorithms.</summary>
    public AlgorithmRecommendationEngine()
    {
        RegisterAlgorithm(new AlgorithmProfile
        {
            Name = "KMeans",
            Category = "Clustering",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("sampleCount", 0.7)
                .Add("dimensionality", 0.4)
                .Add("sparsity", 0.2)
                .Add("separability", 0.9),
            ComplexityClass = "O(nkd)",
            MinSamples = 10,
            MaxDimensions = 1000
        });

        RegisterAlgorithm(new AlgorithmProfile
        {
            Name = "DBSCAN",
            Category = "Clustering",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("sampleCount", 0.5)
                .Add("dimensionality", 0.3)
                .Add("sparsity", 0.5)
                .Add("separability", 0.6)
                .Add("noiseRatio", 0.9),
            ComplexityClass = "O(n log n)",
            MinSamples = 5,
            MaxDimensions = 500
        });

        RegisterAlgorithm(new AlgorithmProfile
        {
            Name = "LinearRegression",
            Category = "Regression",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("sampleCount", 0.6)
                .Add("dimensionality", 0.5)
                .Add("sparsity", 0.3)
                .Add("linearity", 0.95),
            ComplexityClass = "O(np^2)",
            MinSamples = 20,
            MaxDimensions = 10000
        });

        RegisterAlgorithm(new AlgorithmProfile
        {
            Name = "RidgeRegression",
            Category = "Regression",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("sampleCount", 0.5)
                .Add("dimensionality", 0.8)
                .Add("sparsity", 0.3)
                .Add("linearity", 0.8)
                .Add("multicollinearity", 0.9),
            ComplexityClass = "O(np^2)",
            MinSamples = 15,
            MaxDimensions = 50000
        });

        RegisterAlgorithm(new AlgorithmProfile
        {
            Name = "LassoRegression",
            Category = "Regression",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("sampleCount", 0.5)
                .Add("dimensionality", 0.85)
                .Add("sparsity", 0.9)
                .Add("linearity", 0.75),
            ComplexityClass = "O(np)",
            MinSamples = 20,
            MaxDimensions = 100000
        });

        RegisterAlgorithm(new AlgorithmProfile
        {
            Name = "PolynomialRegression",
            Category = "Regression",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("sampleCount", 0.6)
                .Add("dimensionality", 0.3)
                .Add("sparsity", 0.1)
                .Add("linearity", 0.2)
                .Add("curvature", 0.9),
            ComplexityClass = "O(np^3)",
            MinSamples = 30,
            MaxDimensions = 100
        });

        RegisterAlgorithm(new AlgorithmProfile
        {
            Name = "HierarchicalClustering",
            Category = "Clustering",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("sampleCount", 0.3)
                .Add("dimensionality", 0.4)
                .Add("sparsity", 0.3)
                .Add("structure", 0.8),
            ComplexityClass = "O(n^2 log n)",
            MinSamples = 2,
            MaxDimensions = 200
        });

        RegisterAlgorithm(new AlgorithmProfile
        {
            Name = "RandomForest",
            Category = "Classification",
            Strengths = ImmutableDictionary<string, double>.Empty
                .Add("sampleCount", 0.7)
                .Add("dimensionality", 0.7)
                .Add("sparsity", 0.5)
                .Add("nonlinearity", 0.85)
                .Add("robustness", 0.9),
            ComplexityClass = "O(ntree * n log n)",
            MinSamples = 10,
            MaxDimensions = 5000
        });
    }

    /// <summary>Registers a new algorithm profile for recommendation consideration.</summary>
    /// <param name="profile">The algorithm profile to register.</param>
    public void RegisterAlgorithm(AlgorithmProfile profile)
    {
        _algorithms.Add(profile);
    }

    /// <summary>Recommends algorithms ranked by suitability for the given problem features.</summary>
    /// <param name="problemFeatures">Key-value pairs describing data properties and their magnitudes.</param>
    /// <returns>A list of algorithm recommendations ordered by descending confidence.</returns>
    public List<AlgorithmRecommendation> Recommend(ImmutableDictionary<string, double> problemFeatures)
    {
        var results = new List<AlgorithmRecommendation>();

        foreach (var algo in _algorithms)
        {
            var (confidence, matchedFeatures) = ComputeScore(algo, problemFeatures);
            if (confidence <= 0.0)
                continue;

            string reason = BuildExplanation(algo, matchedFeatures, confidence);

            results.Add(new AlgorithmRecommendation
            {
                AlgorithmName = algo.Name,
                Category = algo.Category,
                Confidence = confidence,
                ComplexityClass = algo.ComplexityClass,
                Reason = reason
            });
        }

        results.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
        return results;
    }

    /// <summary>Computes the score and matched features for an algorithm against problem features.</summary>
    /// <param name="algorithm">The algorithm profile to evaluate.</param>
    /// <param name="features">Problem features.</param>
    /// <returns>A tuple of confidence score and list of matched feature keys.</returns>
    private static (double confidence, List<string> matchedFeatures) ComputeScore(
        AlgorithmProfile algorithm,
        ImmutableDictionary<string, double> features)
    {
        double totalWeight = 0.0;
        double weightedSum = 0.0;
        var matched = new List<string>();

        foreach (var kvp in features)
        {
            if (algorithm.Strengths.TryGetValue(kvp.Key, out double strength))
            {
                double intensity = System.Math.Min(System.Math.Abs(kvp.Value), 1.0);
                weightedSum += strength * intensity;
                totalWeight += intensity;
                if (strength > 0.4)
                {
                    matched.Add(kvp.Key);
                }
            }
        }

        double confidence;
        if (totalWeight < 1e-10)
        {
            confidence = 0.1;
        }
        else
        {
            double matchRatio = weightedSum / totalWeight;
            double coverage = (double)matched.Count / algorithm.Strengths.Count;
            confidence = matchRatio * 0.7 + coverage * 0.3;
        }

        return (System.Math.Max(0.0, System.Math.Min(1.0, confidence)), matched);
    }

    /// <summary>Builds an explanation string listing the algorithm's strengths for the problem.</summary>
    /// <param name="algorithm">The algorithm profile.</param>
    /// <param name="matchedFeatures">Features where the algorithm excels.</param>
    /// <param name="confidence">Computed confidence score.</param>
    /// <returns>An explanation string.</returns>
    private static string BuildExplanation(AlgorithmProfile algorithm, List<string> matchedFeatures, double confidence)
    {
        string quality = confidence switch
        {
            >= 0.8 => "strong",
            >= 0.6 => "good",
            >= 0.4 => "adequate",
            _ => "weak"
        };

        if (matchedFeatures.Count > 0)
        {
            return $"{algorithm.Name} ({algorithm.ComplexityClass}) is a {quality} match excelling in: {string.Join(", ", matchedFeatures)}.";
        }

        return $"{algorithm.Name} ({algorithm.ComplexityClass}) provides a {quality} general match.";
    }
}

/// <summary>Describes an algorithm's strengths, complexity, and applicability constraints.</summary>
public sealed class AlgorithmProfile
{
    /// <summary>Gets the algorithm name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the algorithm category (e.g., "Clustering", "Regression", "Classification").</summary>
    public string Category { get; init; } = "";

    /// <summary>Gets the strength profile mapping feature names to how well the algorithm handles them.</summary>
    public ImmutableDictionary<string, double> Strengths { get; init; } = ImmutableDictionary<string, double>.Empty;

    /// <summary>Gets a description of the algorithm's complexity class.</summary>
    public string ComplexityClass { get; init; } = "";

    /// <summary>Gets the minimum number of samples required.</summary>
    public int MinSamples { get; init; }

    /// <summary>Gets the maximum recommended dimensionality.</summary>
    public int MaxDimensions { get; init; }
}

/// <summary>Represents a ranked algorithm recommendation with reasoning.</summary>
public sealed class AlgorithmRecommendation
{
    /// <summary>Gets the name of the recommended algorithm.</summary>
    public string AlgorithmName { get; init; } = "";

    /// <summary>Gets the algorithm category.</summary>
    public string Category { get; init; } = "";

    /// <summary>Gets the confidence score between 0 and 1.</summary>
    public double Confidence { get; init; }

    /// <summary>Gets the complexity class description.</summary>
    public string ComplexityClass { get; init; } = "";

    /// <summary>Gets a human-readable explanation for the recommendation.</summary>
    public string Reason { get; init; } = "";
}
