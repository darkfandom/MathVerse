namespace MathVerse.Math.DataScience.SignalAnalysis
{
    using System;

    /// <summary>
    /// Represents the result of a discrete wavelet transform containing approximation and detail coefficients.
    /// </summary>
    public sealed class WaveletResult
    {
        /// <summary>
        /// Gets the approximation coefficients at each decomposition level.
        /// Index 0 is the finest level, increasing indices are coarser levels.
        /// </summary>
        public double[][] Approximations { get; }

        /// <summary>
        /// Gets the detail coefficients at each decomposition level.
        /// Index 0 is the finest level, increasing indices are coarser levels.
        /// </summary>
        public double[][] Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveletResult"/> class.
        /// </summary>
        /// <param name="approximations">The approximation coefficients at each level.</param>
        /// <param name="details">The detail coefficients at each level.</param>
        public WaveletResult(double[][] approximations, double[][] details)
        {
            Approximations = approximations ?? throw new ArgumentNullException(nameof(approximations));
            Details = details ?? throw new ArgumentNullException(nameof(details));
        }
    }

    /// <summary>
    /// Provides discrete wavelet transform (DWT) computation using the Haar wavelet.
    /// The Haar wavelet decomposes a signal into approximation (average) and detail (difference) coefficients.
    /// </summary>
    public sealed class WaveletTransform
    {
        private static readonly double Sqrt2Inv = 1.0 / System.Math.Sqrt(2.0);

        /// <summary>
        /// Computes the forward discrete wavelet transform using the Haar wavelet at the specified number of decomposition levels.
        /// </summary>
        /// <param name="signal">The input signal. Length should be a power of 2 for best results; non-power-of-2 signals are truncated to the largest power of 2.</param>
        /// <param name="levels">The number of decomposition levels (default is 4). Must not exceed log2(signal.Length).</param>
        /// <returns>A <see cref="WaveletResult"/> containing approximation and detail coefficients at each level.</returns>
        public static WaveletResult DiscreteWaveletTransform(double[] signal, int levels = 4)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (levels <= 0) throw new ArgumentException("Number of levels must be positive.");

            int n = signal.Length;
            int maxSize = 1;
            while (maxSize <= n / 2)
            {
                maxSize <<= 1;
            }
            int maxLevels = 0;
            int temp = n;
            while (temp >= 2 && maxLevels < maxSize)
            {
                temp >>= 1;
                maxLevels++;
            }
            if (levels > maxLevels)
                throw new ArgumentException($"Levels must not exceed {maxLevels} for a signal of length {n}.");

            double[][] approximations = new double[levels][];
            double[][] details = new double[levels][];

            double[] current = new double[n];
            for (int i = 0; i < n; i++)
            {
                current[i] = signal[i];
            }

            for (int level = 0; level < levels; level++)
            {
                int len = current.Length;
                if (len < 2) break;
                int half = len / 2;

                double[] approx = new double[half];
                double[] detail = new double[half];

                for (int i = 0; i < half; i++)
                {
                    approx[i] = (current[2 * i] + current[2 * i + 1]) * Sqrt2Inv;
                    detail[i] = (current[2 * i] - current[2 * i + 1]) * Sqrt2Inv;
                }

                approximations[level] = approx;
                details[level] = detail;
                current = approx;
            }

            return new WaveletResult(approximations, details);
        }

        /// <summary>
        /// Reconstructs a signal from its Haar wavelet decomposition using the inverse DWT.
        /// </summary>
        /// <param name="result">The wavelet decomposition result from <see cref="DiscreteWaveletTransform"/>.</param>
        /// <param name="originalLength">The desired output length (truncates or zero-pads as needed).</param>
        /// <returns>The reconstructed signal.</returns>
        public static double[] InverseDiscreteWaveletTransform(WaveletResult result, int originalLength)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            int levels = result.Approximations.Length;
            if (levels == 0) return new double[originalLength];

            double[] current = result.Approximations[levels - 1];
            if (current == null || current.Length == 0)
                return new double[originalLength];

            for (int level = levels - 1; level >= 0; level--)
            {
                int len = current.Length;
                int outputLen = len * 2;
                double[] reconstructed = new double[outputLen];

                double[] approx = result.Approximations[level];
                double[] detail = result.Details[level];

                for (int i = 0; i < len; i++)
                {
                    reconstructed[2 * i] = (approx[i] + detail[i]) * Sqrt2Inv;
                    reconstructed[2 * i + 1] = (approx[i] - detail[i]) * Sqrt2Inv;
                }

                current = reconstructed;
            }

            double[] output = new double[originalLength];
            int copyLen = System.Math.Min(current.Length, originalLength);
            for (int i = 0; i < copyLen; i++)
            {
                output[i] = current[i];
            }
            return output;
        }
    }
}
