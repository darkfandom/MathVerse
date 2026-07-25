namespace MathVerse.Math.AI.Integration;

using System.Collections.Immutable;

/// <summary>Intelligent integration between AI and simulation subsystems for adaptive timestep, ODE solver selection, stability prediction, and error control.</summary>
public sealed class AISimulationIntegration
{
    /// <summary>Predicts an optimal adaptive timestep based on derivative estimates.</summary>
    /// <param name="currentState">The current state vector.</param>
    /// <param name="derivativeEstimate">The estimated derivative at the current state.</param>
    /// <param name="currentTimestep">The current timestep being used.</param>
    /// <param name="targetError">The desired error tolerance per step.</param>
    /// <returns>A <see cref="TimestepPrediction"/> with the recommended timestep and reasoning.</returns>
    public TimestepPrediction PredictAdaptiveTimestep(
        double[] currentState,
        double[] derivativeEstimate,
        double currentTimestep,
        double targetError)
    {
        if (currentState.Length != derivativeEstimate.Length)
        {
            throw new ArgumentException("State and derivative vectors must have the same length.");
        }

        if (currentTimestep <= 0.0)
        {
            throw new ArgumentException("Timestep must be positive.", nameof(currentTimestep));
        }

        if (targetError <= 0.0)
        {
            throw new ArgumentException("Target error must be positive.", nameof(targetError));
        }

        int n = currentState.Length;
        double maxDerivMagnitude = 0.0;
        double stateMagnitude = 0.0;

        for (int i = 0; i < n; i++)
        {
            double derivAbs = System.Math.Abs(derivativeEstimate[i]);
            if (derivAbs > maxDerivMagnitude)
            {
                maxDerivMagnitude = derivAbs;
            }

            double stateAbs = System.Math.Abs(currentState[i]);
            if (stateAbs > stateMagnitude)
            {
                stateMagnitude = stateAbs;
            }
        }

        double scale = System.Math.Max(stateMagnitude, 1e-10);
        double normalizedDerivative = maxDerivMagnitude / scale;

        double safeTimestep;
        string reason;

        if (normalizedDerivative < 1e-8)
        {
            safeTimestep = currentTimestep * 2.0;
            reason = "Near-equilibrium state; safe to increase timestep.";
        }
        else if (normalizedDerivative > 1e6)
        {
            safeTimestep = currentTimestep * 0.1;
            reason = "Extremely rapid dynamics; timestep must decrease dramatically.";
        }
        else if (normalizedDerivative > 1.0)
        {
            double factor = targetError / (normalizedDerivative * currentTimestep);
            safeTimestep = currentTimestep * System.Math.Max(0.1, System.Math.Min(2.0, System.Math.Sqrt(factor)));
            reason = $"Rapid dynamics (norm deriv={normalizedDerivative:E2}); adjusting timestep for stability.";
        }
        else
        {
            double factor = targetError / System.Math.Max(normalizedDerivative * currentTimestep, 1e-15);
            double clamped = System.Math.Max(0.5, System.Math.Min(2.0, System.Math.Pow(factor, 0.2)));
            safeTimestep = currentTimestep * clamped;
            reason = $"Moderate dynamics; gradual timestep adjustment by factor {clamped:F3}.";
        }

        double changeRatio = safeTimestep / currentTimestep;
        if (changeRatio > 1.5)
        {
            safeTimestep = currentTimestep * 1.5;
            reason += " Capped increase at 50% to prevent overshoot.";
        }
        else if (changeRatio < 0.5)
        {
            safeTimestep = currentTimestep * 0.5;
            reason += " Capped decrease at 50% to prevent excessive refinement.";
        }

        return new TimestepPrediction
        {
            RecommendedTimestep = safeTimestep,
            ChangeRatio = safeTimestep / currentTimestep,
            Reason = reason
        };
    }

    /// <summary>Recommends an ODE solver based on problem stiffness and other characteristics.</summary>
    /// <param name="isStiff">Whether the system is estimated to be stiff.</param>
    /// <param name="stiffnessRatio">Estimated ratio of largest to smallest eigenvalue magnitude.</param>
    /// <param name="hasConstraints">Whether the system has algebraic constraints (DAE).</param>
    /// <param name="needsEventDetection">Whether discontinuity or event detection is required.</param>
    /// <param name="dimensionality">Number of state variables.</param>
    /// <returns>An <see cref="ODESolverRecommendation"/> with the recommended solver.</returns>
    public ODESolverRecommendation RecommendODESolver(
        bool isStiff,
        double stiffnessRatio,
        bool hasConstraints,
        bool needsEventDetection,
        int dimensionality)
    {
        if (hasConstraints)
        {
            return new ODESolverRecommendation
            {
                Solver = "IDA",
                Order = 5,
                Implicit = true,
                Reason = "Algebraic constraints present; IDA handles DAE systems (index-1).",
                StabilityRegion = "A-stable, L-stable"
            };
        }

        if (needsEventDetection)
        {
            if (isStiff)
            {
                return new ODESolverRecommendation
                {
                    Solver = "RadauIA5WithEvents",
                    Order = 5,
                    Implicit = true,
                    Reason = "Stiff system with event detection; Radau IIA with root-finding.",
                    StabilityRegion = "A-stable, L-stable"
                };
            }

            return new ODESolverRecommendation
            {
                Solver = "DOP853WithEvents",
                Order = 8,
                Implicit = false,
                Reason = "Non-stiff system with event detection; DOP853 provides high-order accuracy.",
                StabilityRegion = "Absolute stability for moderate step sizes"
            };
        }

        if (isStiff && stiffnessRatio > 1e6)
        {
            if (dimensionality > 1000)
            {
                return new ODESolverRecommendation
                {
                    Solver = "BDF",
                    Order = 5,
                    Implicit = true,
                    Reason = $"Extremely stiff ({stiffnessRatio:E0}) with large system ({dimensionality}D); BDF with sparse linear solver.",
                    StabilityRegion = "A-stable (orders 1-2), stiffly accurate (orders 3-6)"
                };
            }

            return new ODESolverRecommendation
            {
                Solver = "RadauIA5",
                Order = 5,
                Implicit = true,
                Reason = $"Very stiff system (ratio={stiffnessRatio:E0}); Radau IIA is L-stable.",
                StabilityRegion = "A-stable, L-stable"
            };
        }

        if (isStiff)
        {
            return new ODESolverRecommendation
            {
                Solver = "SDIRK",
                Order = 3,
                Implicit = true,
                Reason = $"Moderately stiff (ratio={stiffnessRatio:E1}); SDIRK avoids反复 factorization.",
                StabilityRegion = "A-stable"
            };
        }

        if (dimensionality > 500)
        {
            return new ODESolverRecommendation
            {
                Solver = "VernerRK",
                Order = 8,
                Implicit = false,
                Reason = $"Large non-stiff system ({dimensionality}D); Verner's method is efficient.",
                StabilityRegion = "Moderate absolute stability region"
            };
        }

        return new ODESolverRecommendation
        {
            Solver = "DOPRI5",
            Order = 5,
            Implicit = false,
            Reason = "Non-stiff system; Dormand-Prince 5 with adaptive stepping.",
            StabilityRegion = "Moderate absolute stability region"
        };
    }

    /// <summary>Predicts numerical stability by estimating eigenvalue characteristics of the Jacobian.</summary>
    /// <param name="jacobianDiagonal">Diagonal elements of the Jacobian matrix approximation.</param>
    /// <param name="timestep">The proposed integration timestep.</param>
    /// <returns>A <see cref="StabilityPrediction"/> with eigenvalue estimates and stability assessment.</returns>
    public StabilityPrediction PredictStability(double[] jacobianDiagonal, double timestep)
    {
        if (jacobianDiagonal.Length == 0)
        {
            return new StabilityPrediction
            {
                IsStable = true,
                MaxEigenvalueReal = 0.0,
                MinEigenvalueReal = 0.0,
                SpectralRadius = 0.0,
                StiffnessRatio = 1.0,
                Assessment = "Empty Jacobian; assuming stability."
            };
        }

        double maxReal = jacobianDiagonal[0];
        double minReal = jacobianDiagonal[0];

        for (int i = 1; i < jacobianDiagonal.Length; i++)
        {
            if (jacobianDiagonal[i] > maxReal) maxReal = jacobianDiagonal[i];
            if (jacobianDiagonal[i] < minReal) minReal = jacobianDiagonal[i];
        }

        double spectralRadius = System.Math.Max(System.Math.Abs(maxReal), System.Math.Abs(minReal));
        double scaledRadius = spectralRadius * timestep;

        double stiffnessRatio = 1.0;
        double minAbs = System.Math.Abs(minReal);
        double maxAbs = System.Math.Abs(maxReal);
        if (minAbs > 1e-15)
        {
            stiffnessRatio = maxAbs / minAbs;
        }

        bool isStable = scaledRadius < 2.5;
        string assessment;

        if (scaledRadius < 0.5)
        {
            assessment = $"Very stable (scaled radius={scaledRadius:F3}). Large timestep margin available.";
        }
        else if (scaledRadius < 1.5)
        {
            assessment = $"Stable (scaled radius={scaledRadius:F3}). Moderate timestep margin.";
        }
        else if (scaledRadius < 2.5)
        {
            assessment = $"Marginally stable (scaled radius={scaledRadius:F3}). Timestep near stability limit.";
        }
        else
        {
            assessment = $"Unstable (scaled radius={scaledRadius:F3}). Reduce timestep or use implicit method.";
        }

        return new StabilityPrediction
        {
            IsStable = isStable,
            MaxEigenvalueReal = maxReal,
            MinEigenvalueReal = minReal,
            SpectralRadius = spectralRadius,
            StiffnessRatio = stiffnessRatio,
            Assessment = assessment
        };
    }

    /// <summary>Adjusts error tolerance automatically based on solver performance and error estimates.</summary>
    /// <param name="currentTolerance">The current error tolerance.</param>
    /// <param name="estimatedError">The estimated local truncation error from the last step.</param>
    /// <param name="acceptanceRate">Fraction of recently accepted steps (0 to 1).</param>
    /// <param name="minTolerance">Minimum allowed tolerance.</param>
    /// <param name="maxTolerance">Maximum allowed tolerance.</param>
    /// <returns>An <see cref="ErrorToleranceAdjustment"/> with the adjusted tolerance and reasoning.</returns>
    public ErrorToleranceAdjustment AdjustErrorTolerance(
        double currentTolerance,
        double estimatedError,
        double acceptanceRate,
        double minTolerance,
        double maxTolerance)
    {
        if (currentTolerance <= 0.0 || minTolerance <= 0.0 || maxTolerance <= 0.0)
        {
            throw new ArgumentException("Tolerances must be positive.");
        }

        if (minTolerance > maxTolerance)
        {
            throw new ArgumentException("Min tolerance must be less than or equal to max tolerance.");
        }

        double adjustedTolerance = currentTolerance;
        string reason;

        if (estimatedError < currentTolerance * 0.01)
        {
            adjustedTolerance = System.Math.Min(maxTolerance, currentTolerance * 1.5);
            reason = $"Error ({estimatedError:E2}) much smaller than tolerance; relaxing to {adjustedTolerance:E2} for efficiency.";
        }
        else if (estimatedError > currentTolerance * 10.0)
        {
            adjustedTolerance = System.Math.Max(minTolerance, currentTolerance * 0.5);
            reason = $"Error ({estimatedError:E2}) exceeds tolerance; tightening to {adjustedTolerance:E2}.";
        }
        else if (acceptanceRate < 0.5)
        {
            adjustedTolerance = System.Math.Max(minTolerance, currentTolerance * 0.7);
            reason = $"Low acceptance rate ({acceptanceRate:P0}); relaxing tolerance to {adjustedTolerance:E2} to improve acceptance.";
        }
        else if (acceptanceRate > 0.95 && estimatedError < currentTolerance * 0.3)
        {
            adjustedTolerance = System.Math.Min(maxTolerance, currentTolerance * 1.2);
            reason = $"High acceptance rate ({acceptanceRate:P0}) with low error; slight relaxation to {adjustedTolerance:E2}.";
        }
        else
        {
            reason = $"Current tolerance {currentTolerance:E2} is appropriate (error={estimatedError:E2}, acceptance={acceptanceRate:P0}).";
        }

        adjustedTolerance = System.Math.Max(minTolerance, System.Math.Min(maxTolerance, adjustedTolerance));

        return new ErrorToleranceAdjustment
        {
            AdjustedTolerance = adjustedTolerance,
            ToleranceRatio = adjustedTolerance / currentTolerance,
            Reason = reason
        };
    }
}

/// <summary>Result of adaptive timestep prediction.</summary>
public sealed class TimestepPrediction
{
    /// <summary>Gets the recommended timestep.</summary>
    public double RecommendedTimestep { get; init; }

    /// <summary>Gets the ratio of recommended to current timestep.</summary>
    public double ChangeRatio { get; init; }

    /// <summary>Gets a human-readable explanation.</summary>
    public string Reason { get; init; } = "";
}

/// <summary>Result of ODE solver recommendation.</summary>
public sealed class ODESolverRecommendation
{
    /// <summary>Gets the recommended ODE solver name.</summary>
    public string Solver { get; init; } = "";

    /// <summary>Gets the method order.</summary>
    public int Order { get; init; }

    /// <summary>Gets whether the method is implicit.</summary>
    public bool Implicit { get; init; }

    /// <summary>Gets a human-readable explanation.</summary>
    public string Reason { get; init; } = "";

    /// <summary>Gets a description of the method's stability region.</summary>
    public string StabilityRegion { get; init; } = "";
}

/// <summary>Result of stability prediction from eigenvalue analysis.</summary>
public sealed class StabilityPrediction
{
    /// <summary>Gets whether the system is predicted to be stable at the given timestep.</summary>
    public bool IsStable { get; init; }

    /// <summary>Gets the largest real part of the estimated eigenvalues.</summary>
    public double MaxEigenvalueReal { get; init; }

    /// <summary>Gets the smallest real part of the estimated eigenvalues.</summary>
    public double MinEigenvalueReal { get; init; }

    /// <summary>Gets the spectral radius of the Jacobian.</summary>
    public double SpectralRadius { get; init; }

    /// <summary>Gets the stiffness ratio (max/min eigenvalue magnitude).</summary>
    public double StiffnessRatio { get; init; }

    /// <summary>Gets a human-readable stability assessment.</summary>
    public string Assessment { get; init; } = "";
}

/// <summary>Result of error tolerance adjustment.</summary>
public sealed class ErrorToleranceAdjustment
{
    /// <summary>Gets the adjusted error tolerance.</summary>
    public double AdjustedTolerance { get; init; }

    /// <summary>Gets the ratio of adjusted to original tolerance.</summary>
    public double ToleranceRatio { get; init; }

    /// <summary>Gets a human-readable explanation.</summary>
    public string Reason { get; init; } = "";
}
