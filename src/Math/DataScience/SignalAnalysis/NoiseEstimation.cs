namespace MathVerse.Math.DataScience.SignalAnalysis
{
    using System;

    /// <summary>
    /// Provides noise estimation, signal-to-noise ratio computation, and basic noise removal utilities.
    /// </summary>
    public sealed class NoiseEstimation
    {
        /// <summary>
        /// Estimates the noise level of a signal using the Median Absolute Deviation (MAD) of the
        /// high-frequency wavelet coefficients, scaled by 0.6745 for Gaussian noise.
        /// Falls back to standard deviation of first differences for short signals.
        /// </summary>
        /// <param name="signal">The input signal.</param>
        /// <returns>An estimate of the noise standard deviation.</returns>
        public static double EstimateNoiseLevel(double[] signal)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length < 2) throw new ArgumentException("Signal must have at least 2 samples.");

            if (signal.Length < 8)
            {
                return EstimateNoiseFromDifferences(signal);
            }

            int maxPow2 = 1;
            while (maxPow2 * 2 <= signal.Length)
            {
                maxPow2 <<= 1;
            }

            double[] truncated = new double[maxPow2];
            for (int i = 0; i < maxPow2; i++)
            {
                truncated[i] = signal[i];
            }

            var wavelet = WaveletTransform.DiscreteWaveletTransform(truncated, 1);
            double[] detail = wavelet.Details[0];

            double[] absDetail = new double[detail.Length];
            for (int i = 0; i < detail.Length; i++)
            {
                absDetail[i] = System.Math.Abs(detail[i]);
            }

            double median = ComputeMedian(absDetail);
            return median / 0.6745;
        }

        /// <summary>
        /// Estimates the signal-to-noise ratio (SNR) of a noisy signal compared to a known clean reference.
        /// </summary>
        /// <param name="signal">The noisy signal.</param>
        /// <param name="cleanSignal">The clean reference signal.</param>
        /// <returns>The SNR in decibels (dB).</returns>
        public static double EstimateSNR(double[] signal, double[] cleanSignal)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (cleanSignal == null) throw new ArgumentNullException(nameof(cleanSignal));
            if (signal.Length != cleanSignal.Length)
                throw new ArgumentException("Signal and clean signal must have the same length.");
            if (signal.Length == 0)
                throw new ArgumentException("Signals must not be empty.");

            double signalPower = 0.0;
            double noisePower = 0.0;

            for (int i = 0; i < signal.Length; i++)
            {
                signalPower += cleanSignal[i] * cleanSignal[i];
                double noise = signal[i] - cleanSignal[i];
                noisePower += noise * noise;
            }

            if (noisePower < 1e-30)
                return double.PositiveInfinity;

            return 10.0 * System.Math.Log10(signalPower / noisePower);
        }

        /// <summary>
        /// Removes noise from a signal by subtracting an estimate of the noise.
        /// Uses soft thresholding on wavelet detail coefficients.
        /// </summary>
        /// <param name="signal">The noisy input signal.</param>
        /// <param name="noiseLevel">The estimated noise standard deviation.</param>
        /// <returns>The denoised signal.</returns>
        public static double[] RemoveNoise(double[] signal, double noiseLevel)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (noiseLevel < 0) throw new ArgumentException("Noise level must be non-negative.");

            if (noiseLevel < 1e-15)
            {
                double[] copy = new double[signal.Length];
                for (int i = 0; i < signal.Length; i++) copy[i] = signal[i];
                return copy;
            }

            int maxPow2 = 1;
            while (maxPow2 * 2 <= signal.Length)
            {
                maxPow2 <<= 1;
            }

            int levels = 1;
            int temp = maxPow2;
            while (temp >= 4)
            {
                levels++;
                temp >>= 1;
            }
            if (levels > 5) levels = 5;

            double[] truncated = new double[maxPow2];
            for (int i = 0; i < maxPow2; i++)
            {
                truncated[i] = signal[i];
            }

            var wavelet = WaveletTransform.DiscreteWaveletTransform(truncated, levels);

            double threshold = noiseLevel * System.Math.Sqrt(2.0 * System.Math.Log(maxPow2));

            for (int level = 0; level < levels; level++)
            {
                double[] detail = wavelet.Details[level];
                for (int i = 0; i < detail.Length; i++)
                {
                    detail[i] = SoftThreshold(detail[i], threshold);
                }
            }

            double[] reconstructed = WaveletTransform.InverseDiscreteWaveletTransform(wavelet, signal.Length);

            return reconstructed;
        }

        private static double EstimateNoiseFromDifferences(double[] signal)
        {
            double[] diffs = new double[signal.Length - 1];
            for (int i = 0; i < signal.Length - 1; i++)
            {
                diffs[i] = signal[i + 1] - signal[i];
            }

            double mean = 0.0;
            for (int i = 0; i < diffs.Length; i++)
            {
                mean += diffs[i];
            }
            mean /= diffs.Length;

            double variance = 0.0;
            for (int i = 0; i < diffs.Length; i++)
            {
                double d = diffs[i] - mean;
                variance += d * d;
            }
            variance /= diffs.Length;

            return System.Math.Sqrt(variance) / System.Math.Sqrt(2.0);
        }

        private static double ComputeMedian(double[] sorted_data)
        {
            double[] arr = new double[sorted_data.Length];
            for (int i = 0; i < sorted_data.Length; i++)
            {
                arr[i] = sorted_data[i];
            }

            Array.Sort(arr);

            int n = arr.Length;
            if (n % 2 == 1)
            {
                return arr[n / 2];
            }
            else
            {
                return (arr[n / 2 - 1] + arr[n / 2]) / 2.0;
            }
        }

        private static double SoftThreshold(double value, double threshold)
        {
            if (value > threshold)
            {
                return value - threshold;
            }
            else if (value < -threshold)
            {
                return value + threshold;
            }
            return 0.0;
        }
    }
}
