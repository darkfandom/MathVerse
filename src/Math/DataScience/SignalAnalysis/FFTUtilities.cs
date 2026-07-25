namespace MathVerse.Math.DataScience.SignalAnalysis
{
    using System;

    /// <summary>
    /// Provides Fast Fourier Transform (FFT) and Inverse FFT utilities using the Cooley-Tukey radix-2 decimation-in-time algorithm.
    /// Input lengths must be powers of 2.
    /// </summary>
    public sealed class FFTUtilities
    {
        /// <summary>
        /// Computes the forward Discrete Fourier Transform using the Cooley-Tukey radix-2 DIT algorithm.
        /// The input arrays are modified in-place to contain the result.
        /// </summary>
        /// <param name="real">The real parts of the input signal. Length must be a power of 2.</param>
        /// <param name="imag">The imaginary parts of the input signal. Length must equal <paramref name="real"/> length.</param>
        public static void ForwardFFT(double[] real, double[] imag)
        {
            if (real == null) throw new ArgumentNullException(nameof(real));
            if (imag == null) throw new ArgumentNullException(nameof(imag));
            if (real.Length != imag.Length)
                throw new ArgumentException("Real and imaginary arrays must have the same length.");
            if (real.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");
            if ((real.Length & (real.Length - 1)) != 0)
                throw new ArgumentException("Length must be a power of 2.");

            int n = real.Length;

            BitReversePermute(real, imag, n);

            for (int len = 2; len <= n; len <<= 1)
            {
                int half = len >> 1;
                double angle = -2.0 * System.Math.PI / len;
                double wlenReal = System.Math.Cos(angle);
                double wlenImag = System.Math.Sin(angle);

                for (int i = 0; i < n; i += len)
                {
                    double wReal = 1.0;
                    double wImag = 0.0;

                    for (int j = 0; j < half; j++)
                    {
                        int uIdx = i + j;
                        int tIdx = i + j + half;

                        double tReal = wReal * real[tIdx] - wImag * imag[tIdx];
                        double tImag = wReal * imag[tIdx] + wImag * real[tIdx];

                        real[tIdx] = real[uIdx] - tReal;
                        imag[tIdx] = imag[uIdx] - tImag;

                        real[uIdx] += tReal;
                        imag[uIdx] += tImag;

                        double newWReal = wReal * wlenReal - wImag * wlenImag;
                        double newWImag = wReal * wlenImag + wImag * wlenReal;
                        wReal = newWReal;
                        wImag = newWImag;
                    }
                }
            }
        }

        /// <summary>
        /// Computes the inverse Discrete Fourier Transform using the Cooley-Tukey algorithm.
        /// The input arrays are modified in-place to contain the result (divided by N).
        /// </summary>
        /// <param name="real">The real parts of the frequency-domain signal. Length must be a power of 2.</param>
        /// <param name="imag">The imaginary parts of the frequency-domain signal. Length must equal <paramref name="real"/> length.</param>
        public static void InverseFFT(double[] real, double[] imag)
        {
            if (real == null) throw new ArgumentNullException(nameof(real));
            if (imag == null) throw new ArgumentNullException(nameof(imag));
            if (real.Length != imag.Length)
                throw new ArgumentException("Real and imaginary arrays must have the same length.");
            if (real.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");
            if ((real.Length & (real.Length - 1)) != 0)
                throw new ArgumentException("Length must be a power of 2.");

            int n = real.Length;

            for (int i = 0; i < n; i++)
            {
                imag[i] = -imag[i];
            }

            ForwardFFT(real, imag);

            for (int i = 0; i < n; i++)
            {
                real[i] /= n;
                imag[i] = -imag[i] / n;
            }
        }

        /// <summary>
        /// Computes the magnitude spectrum |X(k)| from the real and imaginary parts of the FFT.
        /// </summary>
        /// <param name="real">The real parts of the FFT output.</param>
        /// <param name="imag">The imaginary parts of the FFT output.</param>
        /// <returns>An array of magnitudes of the same length as the input.</returns>
        public static double[] MagnitudeSpectrum(double[] real, double[] imag)
        {
            if (real == null) throw new ArgumentNullException(nameof(real));
            if (imag == null) throw new ArgumentNullException(nameof(imag));
            if (real.Length != imag.Length)
                throw new ArgumentException("Real and imaginary arrays must have the same length.");

            int n = real.Length;
            double[] magnitude = new double[n];
            for (int i = 0; i < n; i++)
            {
                magnitude[i] = System.Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
            }
            return magnitude;
        }

        /// <summary>
        /// Computes the phase spectrum arg(X(k)) from the real and imaginary parts of the FFT.
        /// </summary>
        /// <param name="real">The real parts of the FFT output.</param>
        /// <param name="imag">The imaginary parts of the FFT output.</param>
        /// <returns>An array of phase angles in radians, in the range [-pi, pi].</returns>
        public static double[] PhaseSpectrum(double[] real, double[] imag)
        {
            if (real == null) throw new ArgumentNullException(nameof(real));
            if (imag == null) throw new ArgumentNullException(nameof(imag));
            if (real.Length != imag.Length)
                throw new ArgumentException("Real and imaginary arrays must have the same length.");

            int n = real.Length;
            double[] phase = new double[n];
            for (int i = 0; i < n; i++)
            {
                phase[i] = System.Math.Atan2(imag[i], real[i]);
            }
            return phase;
        }

        private static void BitReversePermute(double[] real, double[] imag, int n)
        {
            int j = 0;
            for (int i = 0; i < n - 1; i++)
            {
                if (i < j)
                {
                    double tempReal = real[i];
                    real[i] = real[j];
                    real[j] = tempReal;

                    double tempImag = imag[i];
                    imag[i] = imag[j];
                    imag[j] = tempImag;
                }

                int bit = n >> 1;
                while (j >= bit)
                {
                    j -= bit;
                    bit >>= 1;
                }
                j += bit;
            }
        }
    }
}
