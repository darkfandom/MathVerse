namespace MathVerse.Math.DataScience.SignalAnalysis
{
    using System;

    /// <summary>
    /// Provides power spectrum computation using FFT analysis.
    /// </summary>
    public sealed class PowerSpectrum
    {
        /// <summary>
        /// Represents the result of a power spectrum computation.
        /// </summary>
        public sealed class PowerSpectrumResult
        {
            /// <summary>
            /// Gets the frequency values in Hz.
            /// </summary>
            public double[] Frequencies { get; }

            /// <summary>
            /// Gets the power spectral density values.
            /// </summary>
            public double[] Power { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PowerSpectrumResult"/> class.
            /// </summary>
            public PowerSpectrumResult(double[] frequencies, double[] power)
            {
                Frequencies = frequencies;
                Power = power;
            }
        }

        /// <summary>
        /// Computes the one-sided power spectrum of a signal using FFT.
        /// </summary>
        /// <param name="signal">The input time-domain signal. Length will be zero-padded to the next power of 2 if necessary.</param>
        /// <param name="sampleRate">The sampling rate in Hz.</param>
        /// <returns>A <see cref="PowerSpectrumResult"/> containing the frequencies and power values.</returns>
        public static PowerSpectrumResult Compute(double[] signal, double sampleRate)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (sampleRate <= 0) throw new ArgumentException("Sample rate must be positive.");

            int n = 1;
            while (n < signal.Length)
            {
                n <<= 1;
            }

            double[] real = new double[n];
            double[] imag = new double[n];

            for (int i = 0; i < signal.Length; i++)
            {
                real[i] = signal[i];
            }

            FFTUtilities.ForwardFFT(real, imag);

            int halfN = n / 2;
            double[] frequencies = new double[halfN];
            double[] power = new double[halfN];

            double freqResolution = sampleRate / n;

            for (int i = 0; i < halfN; i++)
            {
                frequencies[i] = i * freqResolution;
                power[i] = (real[i] * real[i] + imag[i] * imag[i]) / n;
                if (i > 0)
                {
                    power[i] *= 2.0;
                }
            }

            return new PowerSpectrumResult(frequencies, power);
        }
    }
}
