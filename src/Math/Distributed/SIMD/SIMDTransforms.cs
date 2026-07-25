namespace MathVerse.Math.Distributed.SIMD
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Provides SIMD-accelerated Fast Fourier Transform (FFT) operations using the
    /// Cooley-Tukey radix-2 algorithm with vectorized butterfly computations.
    /// </summary>
    public sealed class SIMDTransforms
    {
        /// <summary>
        /// Computes the forward (direct) FFT on real and imaginary input arrays.
        /// Both arrays must have the same length, which must be a power of two.
        /// </summary>
        /// <param name="real">The real parts of the input signal. Modified in place.</param>
        /// <param name="imag">The imaginary parts of the input signal. Modified in place.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="real"/> or <paramref name="imag"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when lengths differ or are not a power of two.
        /// </exception>
        public static void ForwardFFT(double[] real, double[] imag)
        {
            ValidateInputs(real, imag);
            int n = real.Length;

            if (n == 0)
                return;

            BitReversePermutation(real, imag);

            for (int stageSize = 2; stageSize <= n; stageSize *= 2)
            {
                int halfStage = stageSize / 2;
                double angle = -2.0 * System.Math.PI / stageSize;
                double wReal = System.Math.Cos(angle);
                double wImag = System.Math.Sin(angle);

                ButterflyStage(real, imag, n, stageSize, halfStage, wReal, wImag);
            }
        }

        /// <summary>
        /// Computes the inverse (reverse) FFT on real and imaginary input arrays.
        /// Both arrays must have the same length, which must be a power of two.
        /// </summary>
        /// <param name="real">The real parts of the frequency-domain signal. Modified in place.</param>
        /// <param name="imag">The imaginary parts of the frequency-domain signal. Modified in place.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="real"/> or <paramref name="imag"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when lengths differ or are not a power of two.
        /// </exception>
        public static void InverseFFT(double[] real, double[] imag)
        {
            ValidateInputs(real, imag);
            int n = real.Length;

            if (n == 0)
                return;

            BitReversePermutation(real, imag);

            for (int stageSize = 2; stageSize <= n; stageSize *= 2)
            {
                int halfStage = stageSize / 2;
                double angle = 2.0 * System.Math.PI / stageSize;
                double wReal = System.Math.Cos(angle);
                double wImag = System.Math.Sin(angle);

                ButterflyStage(real, imag, n, stageSize, halfStage, wReal, wImag);
            }

            double invN = 1.0 / n;
            for (int i = 0; i < n; i++)
            {
                real[i] *= invN;
                imag[i] *= invN;
            }
        }

        /// <summary>
        /// Performs a single butterfly stage of the FFT using SIMD when available.
        /// </summary>
        /// <param name="real">The real array.</param>
        /// <param name="imag">The imaginary array.</param>
        /// <param name="n">The total FFT size.</param>
        /// <param name="stageSize">The current stage size (2, 4, 8, ...).</param>
        /// <param name="halfStage">Half of the stage size.</param>
        /// <param name="wReal">The real part of the twiddle factor.</param>
        /// <param name="wImag">The imaginary part of the twiddle factor.</param>
        private static void ButterflyStage(
            double[] real, double[] imag, int n,
            int stageSize, int halfStage,
            double wReal, double wImag)
        {
            for (int groupStart = 0; groupStart < n; groupStart += stageSize)
            {
                int butterflyTop = groupStart;
                int butterflyBottom = groupStart + halfStage;

                double curWReal = 1.0;
                double curWImag = 0.0;

                if (Vector.IsHardwareAccelerated && halfStage >= Vector<double>.Count)
                {
                    int vectorSize = Vector<double>.Count;
                    int k = 0;

                    for (; k <= halfStage - vectorSize; k += vectorSize)
                    {
                        int topIdx = butterflyTop + k;
                        int botIdx = butterflyBottom + k;

                        Vector<double> vTopReal = new Vector<double>(real, topIdx);
                        Vector<double> vTopImag = new Vector<double>(imag, topIdx);
                        Vector<double> vBotReal = new Vector<double>(real, botIdx);
                        Vector<double> vBotImag = new Vector<double>(imag, botIdx);

                        Vector<double> vWReal = new Vector<double>(curWReal);
                        Vector<double> vWImag = new Vector<double>(curWImag);

                        Vector<double> tReal = vBotReal * vWReal - vBotImag * vWImag;
                        Vector<double> tImag = vBotReal * vWImag + vBotImag * vWReal;

                        (vTopReal + tReal).CopyTo(real, topIdx);
                        (vTopImag + tImag).CopyTo(imag, topIdx);
                        (vTopReal - tReal).CopyTo(real, botIdx);
                        (vTopImag - tImag).CopyTo(imag, botIdx);

                        double tempWReal = curWReal * wReal - curWImag * wImag;
                        curWImag = curWReal * wImag + curWImag * wReal;
                        curWReal = tempWReal;

                        for (int lane = 1; lane < vectorSize; lane++)
                        {
                            double nextWReal = curWReal * wReal - curWImag * wImag;
                            curWImag = curWReal * wImag + curWImag * wReal;
                            curWReal = nextWReal;
                        }
                    }

                    for (; k < halfStage; k++)
                    {
                        int topIdx = butterflyTop + k;
                        int botIdx = butterflyBottom + k;

                        double tReal = real[botIdx] * curWReal - imag[botIdx] * curWImag;
                        double tImag = real[botIdx] * curWImag + imag[botIdx] * curWReal;

                        real[topIdx] += tReal;
                        imag[topIdx] += tImag;
                        real[botIdx] = real[topIdx] - 2.0 * tReal;
                        imag[botIdx] = imag[topIdx] - 2.0 * tImag;

                        double nextWReal = curWReal * wReal - curWImag * wImag;
                        curWImag = curWReal * wImag + curWImag * wReal;
                        curWReal = nextWReal;
                    }
                }
                else
                {
                    for (int k = 0; k < halfStage; k++)
                    {
                        int topIdx = butterflyTop + k;
                        int botIdx = butterflyBottom + k;

                        double tReal = real[botIdx] * curWReal - imag[botIdx] * curWImag;
                        double tImag = real[botIdx] * curWImag + imag[botIdx] * curWReal;

                        real[topIdx] += tReal;
                        imag[topIdx] += tImag;
                        real[botIdx] = real[topIdx] - 2.0 * tReal;
                        imag[botIdx] = imag[topIdx] - 2.0 * tImag;

                        double nextWReal = curWReal * wReal - curWImag * wImag;
                        curWImag = curWReal * wImag + curWImag * wReal;
                        curWReal = nextWReal;
                    }
                }
            }
        }

        /// <summary>
        /// Applies the bit-reverse permutation to reorder arrays for in-place FFT.
        /// </summary>
        /// <param name="real">The real array.</param>
        /// <param name="imag">The imaginary array.</param>
        private static void BitReversePermutation(double[] real, double[] imag)
        {
            int n = real.Length;
            int bits = CountBits(n) - 1;

            for (int i = 0; i < n; i++)
            {
                int j = BitReverse(i, bits);
                if (i < j)
                {
                    double tempReal = real[i];
                    real[i] = real[j];
                    real[j] = tempReal;

                    double tempImag = imag[i];
                    imag[i] = imag[j];
                    imag[j] = tempImag;
                }
            }
        }

        /// <summary>
        /// Counts the number of bits needed to represent the value.
        /// </summary>
        /// <param name="n">The value to measure.</param>
        /// <returns>The number of significant bits.</returns>
        private static int CountBits(int n)
        {
            int count = 0;
            while (n > 0)
            {
                n >>= 1;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Reverses the bits of the given index within the specified bit width.
        /// </summary>
        /// <param name="value">The value whose bits to reverse.</param>
        /// <param name="bits">The number of bits to consider.</param>
        /// <returns>The bit-reversed value.</returns>
        private static int BitReverse(int value, int bits)
        {
            int result = 0;
            for (int i = 0; i < bits; i++)
            {
                result = (result << 1) | (value & 1);
                value >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Validates that the input arrays are compatible for FFT processing.
        /// </summary>
        /// <param name="real">The real array.</param>
        /// <param name="imag">The imaginary array.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="real"/> or <paramref name="imag"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when lengths differ or are not a power of two.
        /// </exception>
        private static void ValidateInputs(double[] real, double[] imag)
        {
            if (real is null)
                throw new ArgumentNullException(nameof(real));
            if (imag is null)
                throw new ArgumentNullException(nameof(imag));
            if (real.Length != imag.Length)
                throw new ArgumentException("Real and imaginary arrays must have the same length.");
            if (real.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");
            if ((real.Length & (real.Length - 1)) != 0)
                throw new ArgumentException("Array length must be a power of two.");
        }
    }
}
