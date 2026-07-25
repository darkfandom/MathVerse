namespace MathVerse.Math.AI.Recommendation;

using System.Collections.Immutable;

/// <summary>Recommends mathematical solvers based on problem characteristics.</summary>
public sealed class SolverRecommendationEngine
{
    private readonly List<SolverProfile> _solvers = [];

    /// <summary>Initializes a new instance of the <see cref="SolverRecommendationEngine"/> class with built-in solvers.</summary>
    public SolverRecommendationEngine()
    {
        RegisterSolver(new SolverProfile
        {
            Name = "GaussianElimination",
            SolverType = "LinearSystem",
            Capabilities = ImmutableDictionary<string, double>.Empty
                .Add("precision", 1.0)
                .Add("matrixSize", 0.3)
                .Add("sparseMatrix", 0.1),
            ComputationalCost = 0.8,
            Accuracy = 0.95
        });

        RegisterSolver(new SolverProfile
        {
            Name = "ConjugateGradient",
            SolverType = "LinearSystem",
            Capabilities = ImmutableDictionary<string, double>.Empty
                .Add("precision", 0.85)
                .Add("matrixSize", 0.9)
                .Add("sparseMatrix", 0.95),
            ComputationalCost = 0.4,
            Accuracy = 0.9
        });

        RegisterSolver(new SolverProfile
        {
            Name = "NewtonRaphson",
            SolverType = "Nonlinear",
            Capabilities = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.95)
                .Add("derivativeAvailable", 1.0)
                .Add("dimensionality", 0.5),
            ComputationalCost = 0.6,
            Accuracy = 0.92
        });

        RegisterSolver(new SolverProfile
        {
            Name = "Broyden",
            SolverType = "Nonlinear",
            Capabilities = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.7)
                .Add("derivativeAvailable", 0.3)
                .Add("dimensionality", 0.8),
            ComputationalCost = 0.5,
            Accuracy = 0.88
        });

        RegisterSolver(new SolverProfile
        {
            Name = "RungeKutta4",
            SolverType = "ODE",
            Capabilities = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.9)
                .Add("stiffness", 0.3)
                .Add("accuracy", 0.95),
            ComputationalCost = 0.55,
            Accuracy = 0.94
        });

        RegisterSolver(new SolverProfile
        {
            Name = "RadauIA5",
            SolverType = "ODE",
            Capabilities = ImmutableDictionary<string, double>.Empty
                .Add("smoothness", 0.6)
                .Add("stiffness", 0.95)
                .Add("accuracy", 0.98),
            ComputationalCost = 0.75,
            Accuracy = 0.97
        });
    }

    /// <summary>Registers a new solver profile for consideration during recommendations.</summary>
    /// <param name="profile">The solver profile to register.</param>
    public void RegisterSolver(SolverProfile profile)
    {
        _solvers.Add(profile);
    }

    /// <summary>Recommends solvers ranked by suitability for the given problem characteristics.</summary>
    /// <param name="problemType">The type of problem (e.g., "LinearSystem", "Nonlinear", "ODE").</param>
    /// <param name="characteristics">Key-value pairs describing problem properties and their magnitudes.</param>
    /// <returns>A list of solver recommendations ordered by descending confidence.</returns>
    public List<SolverRecommendation> Recommend(string problemType, ImmutableDictionary<string, double> characteristics)
    {
        var results = new List<SolverRecommendation>();

        foreach (var solver in _solvers)
        {
            if (!solver.SolverType.Equals(problemType, StringComparison.OrdinalIgnoreCase))
                continue;

            double confidence = ComputeConfidence(solver, characteristics);
            if (confidence <= 0.0)
                continue;

            string reason = BuildReason(solver, characteristics, confidence);

            results.Add(new SolverRecommendation
            {
                SolverName = solver.Name,
                Confidence = confidence,
                Reason = reason
            });
        }

        results.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
        return results;
    }

    /// <summary>Computes a confidence score between 0 and 1 for a solver against the given characteristics.</summary>
    /// <param name="solver">The solver profile to score.</param>
    /// <param name="characteristics">Problem characteristics.</param>
    /// <returns>A confidence score between 0 and 1.</returns>
    private static double ComputeConfidence(SolverProfile solver, ImmutableDictionary<string, double> characteristics)
    {
        if (solver.Capabilities.IsEmpty || characteristics.IsEmpty)
        {
            return solver.Accuracy * (1.0 - solver.ComputationalCost * 0.3);
        }

        double totalWeight = 0.0;
        double matchedWeight = 0.0;

        foreach (var kvp in characteristics)
        {
            if (solver.Capabilities.TryGetValue(kvp.Key, out double capabilityStrength))
            {
                double featureIntensity = System.Math.Min(System.Math.Abs(kvp.Value), 1.0);
                double contribution = capabilityStrength * featureIntensity;
                totalWeight += featureIntensity;
                matchedWeight += contribution;
            }
        }

        if (totalWeight < 1e-10)
        {
            return solver.Accuracy * 0.5 * (1.0 - solver.ComputationalCost * 0.2);
        }

        double featureScore = matchedWeight / totalWeight;
        double costPenalty = solver.ComputationalCost * 0.15;
        double accuracyBonus = solver.Accuracy * 0.2;

        double confidence = featureScore * 0.65 + accuracyBonus - costPenalty;
        return System.Math.Max(0.0, System.Math.Min(1.0, confidence));
    }

    /// <summary>Builds a human-readable explanation for why a solver was recommended.</summary>
    /// <param name="solver">The solver profile.</param>
    /// <param name="characteristics">Problem characteristics.</param>
    /// <param name="confidence">Computed confidence score.</param>
    /// <returns>A descriptive reason string.</returns>
    private static string BuildReason(SolverProfile solver, ImmutableDictionary<string, double> characteristics, double confidence)
    {
        var matchedCapabilities = new List<string>();

        foreach (var kvp in characteristics)
        {
            if (solver.Capabilities.TryGetValue(kvp.Key, out double strength) && strength > 0.5)
            {
                matchedCapabilities.Add(kvp.Key);
            }
        }

        string strengthDesc = confidence switch
        {
            >= 0.8 => "excellent",
            >= 0.6 => "good",
            >= 0.4 => "moderate",
            _ => "marginal"
        };

        if (matchedCapabilities.Count > 0)
        {
            return $"{solver.Name} is a {strengthDesc} fit: strong in {string.Join(", ", matchedCapabilities)} with accuracy {solver.Accuracy:F2}.";
        }

        return $"{solver.Name} provides a {strengthDesc} match with general suitability score {confidence:F2}.";
    }
}

/// <summary>Describes a solver's capabilities and cost profile.</summary>
public sealed class SolverProfile
{
    /// <summary>Gets the name of the solver.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the solver type category (e.g., "LinearSystem", "Nonlinear", "ODE").</summary>
    public string SolverType { get; init; } = "";

    /// <summary>Gets the capability strengths mapped by feature name.</summary>
    public ImmutableDictionary<string, double> Capabilities { get; init; } = ImmutableDictionary<string, double>.Empty;

    /// <summary>Gets the relative computational cost between 0 (cheap) and 1 (expensive).</summary>
    public double ComputationalCost { get; init; }

    /// <summary>Gets the expected accuracy between 0 and 1.</summary>
    public double Accuracy { get; init; }
}

/// <summary>Represents a ranked solver recommendation with explanation.</summary>
public sealed class SolverRecommendation
{
    /// <summary>Gets the name of the recommended solver.</summary>
    public string SolverName { get; init; } = "";

    /// <summary>Gets the confidence score between 0 and 1.</summary>
    public double Confidence { get; init; }

    /// <summary>Gets a human-readable explanation for the recommendation.</summary>
    public string Reason { get; init; } = "";
}
