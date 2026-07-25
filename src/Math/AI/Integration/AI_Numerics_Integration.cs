namespace MathVerse.Math.AI.Integration;

using System.Collections.Immutable;

/// <summary>Intelligent integration between AI and numerical computation subsystems for adaptive solver selection and precision management.</summary>
public sealed class AINumericsIntegration
{
    /// <summary>Recommends the best solver for a function based on its computed properties.</summary>
    /// <param name="isSmooth">Whether the function is smooth and differentiable.</param>
    /// <param name="isPeriodic">Whether the function exhibits periodic behavior.</param>
    /// <param name="hasSingularities">Whether the function has singularities in the domain.</param>
    /// <param name="rootsCount">Estimated number of roots in the interval.</param>
    /// <param name="noiseLevel">Estimated noise level from 0 to 1.</param>
    /// <returns>A <see cref="SolverSelectionResult"/> with recommended method and reasoning.</returns>
    public SolverSelectionResult RecommendSolver(
        bool isSmooth,
        bool isPeriodic,
        bool hasSingularities,
        int rootsCount,
        double noiseLevel)
    {
        if (rootsCount == 0)
        {
            return new SolverSelectionResult
            {
                Method = "BracketSearch",
                Confidence = 0.3,
                Reason = "No roots detected; bracket search may find sign changes.",
                RequiresGradient = false
            };
        }

        if (hasSingularities)
        {
            return new SolverSelectionResult
            {
                Method = "BrentWithBracketing",
                Confidence = 0.85,
                Reason = "Singularities present; bracketed method ensures safety.",
                RequiresGradient = false
            };
        }

        if (noiseLevel > 0.6)
        {
            return new SolverSelectionResult
            {
                Method = "Bisection",
                Confidence = 0.7,
                Reason = $"High noise (level={noiseLevel:F2}); bisection is robust.",
                RequiresGradient = false
            };
        }

        if (isSmooth && rootsCount <= 3)
        {
            return new SolverSelectionResult
            {
                Method = "NewtonRaphson",
                Confidence = 0.9,
                Reason = "Smooth function with few roots; Newton-Raphson converges quadratically.",
                RequiresGradient = true
            };
        }

        if (isPeriodic)
        {
            return new SolverSelectionResult
            {
                Method = "BrentHybrid",
                Confidence = 0.8,
                Reason = "Periodic function; Brent hybrid combines speed with bracket safety.",
                RequiresGradient = false
            };
        }

        if (rootsCount > 5)
        {
            return new SolverSelectionResult
            {
                Method = "MullerMethod",
                Confidence = 0.75,
                Reason = $"Many roots ({rootsCount}); Muller method can find multiple roots.",
                RequiresGradient = false
            };
        }

        return new SolverSelectionResult
        {
            Method = "Brent",
            Confidence = 0.8,
            Reason = "General-purpose robust root finder.",
            RequiresGradient = false
        };
    }

    /// <summary>Recommends the required numerical precision based on an estimated condition number.</summary>
    /// <param name="conditionNumber">The estimated condition number of the problem.</param>
    /// <param name="machineEpsilon">The machine epsilon for double precision (typically 2.2e-16).</param>
    /// <returns>A <see cref="PrecisionRecommendation"/> with the recommended precision level.</returns>
    public PrecisionRecommendation RecommendPrecision(double conditionNumber, double machineEpsilon = 2.2204460492503131e-16)
    {
        if (conditionNumber < 0.0)
        {
            throw new ArgumentException("Condition number must be non-negative.", nameof(conditionNumber));
        }

        double effectiveDigits = -System.Math.Log10(conditionNumber * machineEpsilon);
        double safeDigits = System.Math.Max(1.0, effectiveDigits);

        string precisionLevel;
        string reasoning;

        if (conditionNumber < 1e2)
        {
            precisionLevel = "StandardDouble";
            reasoning = $"Well-conditioned (kappa={conditionNumber:E2}); standard double precision suffices. {safeDigits:F1} significant digits available.";
        }
        else if (conditionNumber < 1e6)
        {
            precisionLevel = "ExtendedPrecision";
            reasoning = $"Moderately ill-conditioned (kappa={conditionNumber:E2}); iterative refinement recommended. {safeDigits:F1} significant digits available.";
        }
        else if (conditionNumber < 1e12)
        {
            precisionLevel = "QuadPrecision";
            reasoning = $"Ill-conditioned (kappa={conditionNumber:E2}); quad precision or compensated summation needed. Only {safeDigits:F1} digits reliable.";
        }
        else
        {
            precisionLevel = "ArbitraryPrecision";
            reasoning = $"Severely ill-conditioned (kappa={conditionNumber:E2}); arbitrary precision arithmetic required. Only {safeDigits:F1} digits reliable in double.";
        }

        return new PrecisionRecommendation
        {
            PrecisionLevel = precisionLevel,
            SafeDigits = safeDigits,
            EffectiveDigits = effectiveDigits,
            Reasoning = reasoning
        };
    }

    /// <summary>Recommends a root-finding approach based on available information about the function.</summary>
    /// <param name="bracketAvailable">Whether a valid bracket [a,b] with sign change is known.</param>
    /// <param name="derivativeAvailable">Whether the function derivative is available.</param>
    /// <param name="functionEvaluationsBudget">Maximum number of function evaluations allowed.</param>
    /// <param name="needsComplexRoots">Whether complex roots need to be found.</param>
    /// <returns>A <see cref="RootFinderRecommendation"/> with the recommended method.</returns>
    public RootFinderRecommendation RecommendRootFinder(
        bool bracketAvailable,
        bool derivativeAvailable,
        int functionEvaluationsBudget,
        bool needsComplexRoots = false)
    {
        if (needsComplexRoots)
        {
            return new RootFinderRecommendation
            {
                Method = "Muller",
                Reason = "Complex roots required; Muller method handles complex arithmetic natively.",
                EstimatedEvaluations = System.Math.Min(functionEvaluationsBudget, 50),
                SafetyLevel = "Medium"
            };
        }

        if (bracketAvailable && derivativeAvailable && functionEvaluationsBudget > 20)
        {
            return new RootFinderRecommendation
            {
                Method = "BrentWithNewton",
                Reason = "Bracket and derivative available; hybrid Brent-Newton provides speed and safety.",
                EstimatedEvaluations = System.Math.Min(functionEvaluationsBudget, 15),
                SafetyLevel = "High"
            };
        }

        if (bracketAvailable)
        {
            if (functionEvaluationsBudget < 10)
            {
                return new RootFinderRecommendation
                {
                    Method = "Bisection",
                    Reason = "Bracket available but very limited evaluations; bisection guarantees convergence.",
                    EstimatedEvaluations = System.Math.Min(functionEvaluationsBudget, 53),
                    SafetyLevel = "VeryHigh"
                };
            }

            return new RootFinderRecommendation
            {
                Method = "Brent",
                Reason = "Bracket available; Brent method is fast and safe.",
                EstimatedEvaluations = System.Math.Min(functionEvaluationsBudget, 20),
                SafetyLevel = "High"
            };
        }

        if (derivativeAvailable && functionEvaluationsBudget >= 30)
        {
            return new RootFinderRecommendation
            {
                Method = "NewtonRaphson",
                Reason = "Derivative available without bracket; Newton-Raphson with line search.",
                EstimatedEvaluations = System.Math.Min(functionEvaluationsBudget, 30),
                SafetyLevel = "Medium"
            };
        }

        return new RootFinderRecommendation
        {
            Method = "SecantWithFallback",
            Reason = "No bracket or derivative; secant method with bisection fallback.",
            EstimatedEvaluations = System.Math.Min(functionEvaluationsBudget, 25),
            SafetyLevel = "Medium"
        };
    }

    /// <summary>Recommends a numerical integration method based on integrand properties.</summary>
    /// <param name="isSmooth">Whether the integrand is smooth.</param>
    /// <param name="dimensionality">Number of dimensions to integrate over.</param>
    /// <param name="hasOscillations">Whether the integrand oscillates rapidly.</param>
    /// <param name="evaluationsBudget">Maximum function evaluations allowed.</param>
    /// <returns>An <see cref="IntegrationMethodRecommendation"/> with the recommended method.</returns>
    public IntegrationMethodRecommendation RecommendIntegrationMethod(
        bool isSmooth,
        int dimensionality,
        bool hasOscillations,
        int evaluationsBudget)
    {
        if (dimensionality > 5)
        {
            return new IntegrationMethodRecommendation
            {
                Method = "QuasiMonteCarlo",
                Reason = $"High dimensionality ({dimensionality}D); Quasi-Monte Carlo with Sobol sequence avoids curse of dimensionality.",
                EstimatedEvaluations = System.Math.Min(evaluationsBudget, 10000),
                ExpectedAccuracy = "Slow convergence O(1/N), but dimension-independent"
            };
        }

        if (dimensionality > 1 && dimensionality <= 5 && isSmooth)
        {
            return new IntegrationMethodRecommendation
            {
                Method = "SmolyakSparseGrid",
                Reason = $"Multi-dimensional ({dimensionality}D) smooth integrand; sparse grid quadrature is efficient.",
                EstimatedEvaluations = System.Math.Min(evaluationsBudget, (int)System.Math.Pow(15, dimensionality)),
                ExpectedAccuracy = "Exponential convergence for smooth integrands"
            };
        }

        if (hasOscillations)
        {
            return new IntegrationMethodRecommendation
            {
                Method = "AdaptiveGaussKronrod",
                Reason = "Oscillatory integrand; adaptive Gauss-Kronrod handles varying frequency.",
                EstimatedEvaluations = System.Math.Min(evaluationsBudget, 500),
                ExpectedAccuracy = "High accuracy with adaptive subdivision"
            };
        }

        if (isSmooth && evaluationsBudget >= 50)
        {
            return new IntegrationMethodRecommendation
            {
                Method = "GaussLegendre",
                Reason = "Smooth integrand with sufficient budget; Gauss-Legendre is highly efficient.",
                EstimatedEvaluations = System.Math.Min(evaluationsBudget, 64),
                ExpectedAccuracy = "Near machine precision for polynomials up to degree 2n-1"
            };
        }

        if (!isSmooth)
        {
            return new IntegrationMethodRecommendation
            {
                Method = "AdaptiveSimpson",
                Reason = "Non-smooth integrand; adaptive Simpson handles local irregularities.",
                EstimatedEvaluations = System.Math.Min(evaluationsBudget, 200),
                ExpectedAccuracy = "O(h^4) convergence with adaptive refinement"
            };
        }

        return new IntegrationMethodRecommendation
        {
            Method = "CompositeTrapezoidal",
            Reason = "Default choice with limited budget; trapezoidal is simple and reliable.",
            EstimatedEvaluations = System.Math.Min(evaluationsBudget, 100),
            ExpectedAccuracy = "O(h^2) convergence"
        };
    }
}

/// <summary>Result of solver selection recommendation.</summary>
public sealed class SolverSelectionResult
{
    /// <summary>Gets the recommended solver method name.</summary>
    public string Method { get; init; } = "";

    /// <summary>Gets the confidence score between 0 and 1.</summary>
    public double Confidence { get; init; }

    /// <summary>Gets a human-readable explanation.</summary>
    public string Reason { get; init; } = "";

    /// <summary>Gets whether the method requires gradient information.</summary>
    public bool RequiresGradient { get; init; }
}

/// <summary>Result of precision recommendation based on condition number analysis.</summary>
public sealed class PrecisionRecommendation
{
    /// <summary>Gets the recommended precision level (e.g., "StandardDouble", "ExtendedPrecision", "QuadPrecision", "ArbitraryPrecision").</summary>
    public string PrecisionLevel { get; init; } = "";

    /// <summary>Gets the number of reliable significant digits at the recommended precision.</summary>
    public double SafeDigits { get; init; }

    /// <summary>Gets the theoretical effective digits at standard double precision.</summary>
    public double EffectiveDigits { get; init; }

    /// <summary>Gets a human-readable explanation.</summary>
    public string Reasoning { get; init; } = "";
}

/// <summary>Result of root-finder recommendation.</summary>
public sealed class RootFinderRecommendation
{
    /// <summary>Gets the recommended root-finder method name.</summary>
    public string Method { get; init; } = "";

    /// <summary>Gets a human-readable explanation.</summary>
    public string Reason { get; init; } = "";

    /// <summary>Gets the estimated number of function evaluations needed.</summary>
    public int EstimatedEvaluations { get; init; }

    /// <summary>Gets the safety level (e.g., "VeryHigh", "High", "Medium", "Low").</summary>
    public string SafetyLevel { get; init; } = "";
}

/// <summary>Result of integration method recommendation.</summary>
public sealed class IntegrationMethodRecommendation
{
    /// <summary>Gets the recommended integration method name.</summary>
    public string Method { get; init; } = "";

    /// <summary>Gets a human-readable explanation.</summary>
    public string Reason { get; init; } = "";

    /// <summary>Gets the estimated number of function evaluations needed.</summary>
    public int EstimatedEvaluations { get; init; }

    /// <summary>Gets a description of the expected accuracy characteristics.</summary>
    public string ExpectedAccuracy { get; init; } = "";
}
