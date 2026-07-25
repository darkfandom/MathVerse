namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// Defines a noise profile for quantum simulations, specifying error rates for various noise channels.
/// </summary>
public sealed class QuantumNoiseProfile
{
    /// <summary>
    /// Gets the name of this noise profile.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the depolarizing error rate.
    /// </summary>
    public double DepolarizingRate { get; }

    /// <summary>
    /// Gets the bit-flip error rate.
    /// </summary>
    public double BitFlipRate { get; }

    /// <summary>
    /// Gets the phase-flip error rate.
    /// </summary>
    public double PhaseFlipRate { get; }

    /// <summary>
    /// Gets the amplitude damping rate.
    /// </summary>
    public double AmplitudeDampingRate { get; }

    /// <summary>
    /// Gets the phase damping rate.
    /// </summary>
    public double PhaseDampingRate { get; }

    /// <summary>
    /// Gets the readout error rate.
    /// </summary>
    public double ReadoutErrorRate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumNoiseProfile"/> class.
    /// </summary>
    /// <param name="name">The profile name.</param>
    /// <param name="depolarizingRate">The depolarizing error rate.</param>
    /// <param name="bitFlipRate">The bit-flip error rate.</param>
    /// <param name="phaseFlipRate">The phase-flip error rate.</param>
    /// <param name="amplitudeDampingRate">The amplitude damping rate.</param>
    /// <param name="phaseDampingRate">The phase damping rate.</param>
    /// <param name="readoutErrorRate">The readout error rate.</param>
    public QuantumNoiseProfile(
        string name,
        double depolarizingRate,
        double bitFlipRate,
        double phaseFlipRate,
        double amplitudeDampingRate,
        double phaseDampingRate,
        double readoutErrorRate)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DepolarizingRate = depolarizingRate;
        BitFlipRate = bitFlipRate;
        PhaseFlipRate = phaseFlipRate;
        AmplitudeDampingRate = amplitudeDampingRate;
        PhaseDampingRate = phaseDampingRate;
        ReadoutErrorRate = readoutErrorRate;
    }

    /// <summary>
    /// Creates an ideal (noiseless) profile with all rates set to zero.
    /// </summary>
    /// <returns>An ideal <see cref="QuantumNoiseProfile"/>.</returns>
    public static QuantumNoiseProfile CreateIdeal()
    {
        return new QuantumNoiseProfile("Ideal", 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
    }

    /// <summary>
    /// Creates a noisy profile with uniform error rate across all noise channels.
    /// </summary>
    /// <param name="errorRate">The uniform error rate for all noise channels.</param>
    /// <returns>A noisy <see cref="QuantumNoiseProfile"/>.</returns>
    public static QuantumNoiseProfile CreateNoisy(double errorRate)
    {
        return new QuantumNoiseProfile("Noisy", errorRate, errorRate, errorRate, errorRate, errorRate, errorRate);
    }
}
