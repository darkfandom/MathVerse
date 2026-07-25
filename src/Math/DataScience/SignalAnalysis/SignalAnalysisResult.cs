namespace MathVerse.Math.DataScience.SignalAnalysis
{
    using System;

    /// <summary>
    /// Represents the combined result of signal analysis including frequency, power spectrum, peaks, and noise metrics.
    /// </summary>
    public sealed class SignalAnalysisResult
    {
        /// <summary>
        /// Gets the frequency values in Hz.
        /// </summary>
        public double[] Frequencies { get; }

        /// <summary>
        /// Gets the power spectrum values.
        /// </summary>
        public double[] PowerSpectrum { get; }

        /// <summary>
        /// Gets the indices of detected peaks in the signal.
        /// </summary>
        public int[] Peaks { get; }

        /// <summary>
        /// Gets the estimated noise level (standard deviation).
        /// </summary>
        public double NoiseLevel { get; }

        /// <summary>
        /// Gets the estimated signal-to-noise ratio in dB.
        /// </summary>
        public double SNR { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalAnalysisResult"/> class.
        /// </summary>
        /// <param name="frequencies">The frequency array.</param>
        /// <param name="powerSpectrum">The power spectrum array.</param>
        /// <param name="peaks">The detected peak indices.</param>
        /// <param name="noiseLevel">The estimated noise level.</param>
        /// <param name="snr">The signal-to-noise ratio in dB.</param>
        public SignalAnalysisResult(double[] frequencies, double[] powerSpectrum, int[] peaks, double noiseLevel, double snr)
        {
            Frequencies = frequencies ?? throw new ArgumentNullException(nameof(frequencies));
            PowerSpectrum = powerSpectrum ?? throw new ArgumentNullException(nameof(powerSpectrum));
            Peaks = peaks ?? throw new ArgumentNullException(nameof(peaks));
            NoiseLevel = noiseLevel;
            SNR = snr;
        }
    }
}
