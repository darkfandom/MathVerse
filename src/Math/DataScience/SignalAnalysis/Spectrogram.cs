namespace MathVerse.Math.DataScience.SignalAnalysis
{
    using System;

    /// <summary>
    /// Provides spectrogram computation using short-time Fourier transform (STFT) with windowed FFT.
    /// </summary>
    public sealed class Spectrogram
    {
        /// <summary>
        /// Represents the result of a spectrogram computation.
        /// </summary>
        public sealed class SpectrogramResult
        {
            /// <summary>
            /// Gets the 2D spectrogram array where rows are time frames and columns are frequency bins.
            /// </summary>
            public double[,] Magnitude { get; }

            /// <summary>
            /// Gets the frequency values in Hz for each column.
            /// </summary>
            public double[] Frequencies { get; }

            /// <summary>
            /// Gets the time values in seconds for each row.
            /// </summary>
            public double[] Times { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SpectrogramResult"/> class.
            /// </summary>
            public SpectrogramResult(double[,] magnitude, double[] frequencies, double[] times)
            {
                Magnitude = magnitude;
                Frequencies = frequencies;
                Times = times;
            }
        }

        /// <summary>
        /// Computes the spectrogram of a signal using a sliding Hamming window and FFT.
        /// </summary>
        /// <param name="signal">The input time-domain signal.</param>
        /// <param name="sampleRate">The sampling rate in Hz.</param>
        /// <param name="windowSize">The FFT window size (will be rounded up to the next power of 2). Default is 256.</param>
        /// <param name="overlap">The number of overlapping samples between consecutive windows. Default is 128.</param>
        /// <returns>A <see cref="SpectrogramResult"/> containing the magnitude spectrogram, frequencies, and time values.</returns>
        public static SpectrogramResult Compute(double[] signal, double sampleRate, int windowSize = 256, int overlap = 128)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (sampleRate <= 0) throw new ArgumentException("Sample rate must be positive.");
            if (windowSize <= 0) throw new ArgumentException("Window size must be positive.");
            if (overlap < 0 || overlap >= windowSize)
                throw new ArgumentException("Overlap must be non-negative and less than window size.");

            int fftSize = 1;
            while (fftSize < windowSize)
            {
                fftSize <<= 1;
            }

            double[] window = HammingWindow(windowSize);

            int step = windowSize - overlap;
            int numFrames = (signal.Length - windowSize) / step + 1;
            if (numFrames < 1) numFrames = 1;

            int numFreqBins = fftSize / 2;
            double[] frequencies = new double[numFreqBins];
            double freqResolution = sampleRate / fftSize;
            for (int i = 0; i < numFreqBins; i++)
            {
                frequencies[i] = i * freqResolution;
            }

            double[] times = new double[numFrames];
            for (int i = 0; i < numFrames; i++)
            {
                times[i] = (i * step + windowSize / 2.0) / sampleRate;
            }

            double[,] magnitude = new double[numFrames, numFreqBins];

            for (int frame = 0; frame < numFrames; frame++)
            {
                int start = frame * step;
                double[] real = new double[fftSize];
                double[] imag = new double[fftSize];

                for (int i = 0; i < windowSize && start + i < signal.Length; i++)
                {
                    real[i] = signal[start + i] * window[i];
                }

                FFTUtilities.ForwardFFT(real, imag);

                for (int k = 0; k < numFreqBins; k++)
                {
                    magnitude[frame, k] = System.Math.Sqrt(
                        real[k] * real[k] + imag[k] * imag[k]) / fftSize;
                }
            }

            return new SpectrogramResult(magnitude, frequencies, times);
        }

        private static double[] HammingWindow(int size)
        {
            double[] window = new double[size];
            for (int i = 0; i < size; i++)
            {
                window[i] = 0.54 - 0.46 * System.Math.Cos(2.0 * System.Math.PI * i / (size - 1));
            }
            return window;
        }
    }
}
