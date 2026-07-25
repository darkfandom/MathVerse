namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel Fast Fourier Transform using Cooley-Tukey algorithm.
    /// </summary>
    public sealed class ParallelFFT
    {
        /// <summary>
        /// Computes the forward FFT in parallel.
        /// </summary>
        /// <param name="real">Real part array (modified in-place).</param>
        /// <param name="imag">Imaginary part array (modified in-place).</param>
        public void ForwardFFT(double[] real, double[] imag)
        {
            if (real == null)
                throw new ArgumentNullException(nameof(real));
            if (imag == null)
                throw new ArgumentNullException(nameof(imag));
            if (real.Length != imag.Length)
                throw new ArgumentException("Real and imaginary arrays must have the same length.");

            int n = real.Length;
            if ((n & (n - 1)) != 0)
                throw new ArgumentException("Length must be a power of 2.");

            BitReversePermute(real, imag);
            ComputeFFT(real, imag, n, inverse: false);
        }

        /// <summary>
        /// Computes the inverse FFT in parallel.
        /// </summary>
        /// <param name="real">Real part array (modified in-place).</param>
        /// <param name="imag">Imaginary part array (modified in-place).</param>
        public void InverseFFT(double[] real, double[] imag)
        {
            if (real == null)
                throw new ArgumentNullException(nameof(real));
            if (imag == null)
                throw new ArgumentNullException(nameof(imag));
            if (real.Length != imag.Length)
                throw new ArgumentException("Real and imaginary arrays must have the same length.");

            int n = real.Length;
            if ((n & (n - 1)) != 0)
                throw new ArgumentException("Length must be a power of 2.");

            BitReversePermute(real, imag);
            ComputeFFT(real, imag, n, inverse: true);

            double invN = 1.0 / n;
            for (int i = 0; i < n; i++)
            {
                real[i] *= invN;
                imag[i] *= invN;
            }
        }

        private static void BitReversePermute(double[] real, double[] imag)
        {
            int n = real.Length;
            int bits = 0;
            int temp = n;
            while (temp > 1) { temp >>= 1; bits++; }

            for (int i = 0; i < n; i++)
            {
                int j = BitReverse(i, bits);
                if (i < j)
                {
                    (real[i], real[j]) = (real[j], real[i]);
                    (imag[i], imag[j]) = (imag[j], imag[i]);
                }
            }
        }

        private static int BitReverse(int x, int bits)
        {
            int result = 0;
            for (int i = 0; i < bits; i++)
            {
                result = (result << 1) | (x & 1);
                x >>= 1;
            }
            return result;
        }

        private static void ComputeFFT(double[] real, double[] imag, int n, bool inverse)
        {
            for (int size = 2; size <= n; size *= 2)
            {
                int halfSize = size / 2;
                double angle = (inverse ? 2.0 : -2.0) * System.Math.PI / size;
                double wReal = System.Math.Cos(angle);
                double wImag = System.Math.Sin(angle);

                int parallelThreshold = 64;
                int blockCount = n / size;

                if (blockCount >= parallelThreshold)
                {
                    Parallel.For(0, blockCount, block =>
                    {
                        ProcessFFTBlock(real, imag, block, size, halfSize, wReal, wImag);
                    });
                }
                else
                {
                    for (int block = 0; block < blockCount; block++)
                    {
                        ProcessFFTBlock(real, imag, block, size, halfSize, wReal, wImag);
                    }
                }
            }
        }

        private static void ProcessFFTBlock(double[] real, double[] imag, int block, int size, int halfSize, double wReal, double wImag)
        {
            int start = block * size;
            double curWReal = 1;
            double curWImag = 0;

            for (int j = 0; j < halfSize; j++)
            {
                int uIdx = start + j;
                int tIdx = start + j + halfSize;

                double tReal = curWReal * real[tIdx] - curWImag * imag[tIdx];
                double tImag = curWReal * imag[tIdx] + curWImag * real[tIdx];

                real[tIdx] = real[uIdx] - tReal;
                imag[tIdx] = imag[uIdx] - tImag;
                real[uIdx] += tReal;
                imag[uIdx] += tImag;

                double newWReal = curWReal * wReal - curWImag * wImag;
                double newWImag = curWReal * wImag + curWImag * wReal;
                curWReal = newWReal;
                curWImag = newWImag;
            }
        }
    }
}
