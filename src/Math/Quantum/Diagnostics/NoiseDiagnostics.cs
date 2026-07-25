namespace MathVerse.Math.Quantum.Diagnostics;

/// <summary>
/// Provides diagnostic information about noise in quantum simulations.
/// </summary>
public sealed class NoiseDiagnostics
{
    /// <summary>
    /// Gets the estimated fidelity of the noisy operation.
    /// </summary>
    public double FidelityEstimate { get; }

    /// <summary>
    /// Gets the overall error rate.
    /// </summary>
    public double ErrorRate { get; }

    /// <summary>
    /// Gets the number of noisy gates applied.
    /// </summary>
    public int NoisyGatesApplied { get; }

    /// <summary>
    /// Gets the noise parameters and their values.
    /// </summary>
    public Dictionary<string, double> NoiseParameters { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoiseDiagnostics"/> class.
    /// </summary>
    /// <param name="fidelityEstimate">The fidelity estimate.</param>
    /// <param name="errorRate">The error rate.</param>
    /// <param name="noisyGatesApplied">The number of noisy gates applied.</param>
    /// <param name="noiseParameters">The noise parameter dictionary.</param>
    public NoiseDiagnostics(
        double fidelityEstimate,
        double errorRate,
        int noisyGatesApplied,
        Dictionary<string, double> noiseParameters)
    {
        FidelityEstimate = fidelityEstimate;
        ErrorRate = errorRate;
        NoisyGatesApplied = noisyGatesApplied;
        NoiseParameters = noiseParameters ?? throw new ArgumentNullException(nameof(noiseParameters));
    }

    /// <summary>
    /// Returns a summary string of the noise diagnostics.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public string GetSummary()
    {
        return $"Noise: fidelity {FidelityEstimate:F4}, error rate {ErrorRate:F4}, {NoisyGatesApplied} noisy gates";
    }
}
