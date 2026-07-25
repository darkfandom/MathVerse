namespace MathVerse.Math.DataScience.SignalAnalysis
{
    using System;

    /// <summary>
    /// Provides digital filter design and application including FIR low-pass, high-pass, band-pass filters
    /// using the windowed sinc method, and a simple moving average filter.
    /// </summary>
    public sealed class DigitalFilter
    {
        /// <summary>
        /// Applies a low-pass FIR filter to the signal using a windowed sinc design with a Hamming window.
        /// </summary>
        /// <param name="signal">The input signal.</param>
        /// <param name="cutoff">The cutoff frequency in Hz (must be between 0 and sampleRate/2).</param>
        /// <param name="sampleRate">The sampling rate in Hz.</param>
        /// <param name="order">The filter order (number of taps = order + 1). Default is 10.</param>
        /// <returns>The filtered signal.</returns>
        public static double[] LowPass(double[] signal, double cutoff, double sampleRate, int order = 10)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (cutoff <= 0 || cutoff >= sampleRate / 2.0)
                throw new ArgumentException("Cutoff must be between 0 (exclusive) and sampleRate/2 (exclusive).");
            if (sampleRate <= 0) throw new ArgumentException("Sample rate must be positive.");
            if (order < 1) throw new ArgumentException("Order must be at least 1.");

            double[] coefficients = DesignLowPass(cutoff, sampleRate, order);
            return ApplyFIR(signal, coefficients);
        }

        /// <summary>
        /// Applies a high-pass FIR filter to the signal. Designed as a complement of a low-pass filter.
        /// </summary>
        /// <param name="signal">The input signal.</param>
        /// <param name="cutoff">The cutoff frequency in Hz.</param>
        /// <param name="sampleRate">The sampling rate in Hz.</param>
        /// <param name="order">The filter order. Default is 10.</param>
        /// <returns>The filtered signal.</returns>
        public static double[] HighPass(double[] signal, double cutoff, double sampleRate, int order = 10)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (cutoff <= 0 || cutoff >= sampleRate / 2.0)
                throw new ArgumentException("Cutoff must be between 0 (exclusive) and sampleRate/2 (exclusive).");
            if (sampleRate <= 0) throw new ArgumentException("Sample rate must be positive.");
            if (order < 1) throw new ArgumentException("Order must be at least 1.");

            double[] lowPass = DesignLowPass(cutoff, sampleRate, order);
            int len = order + 1;
            int mid = len / 2;
            double[] highPass = new double[len];
            for (int i = 0; i < len; i++)
            {
                highPass[i] = -lowPass[i];
            }
            highPass[mid] += 1.0;

            return ApplyFIR(signal, highPass);
        }

        /// <summary>
        /// Applies a band-pass FIR filter to the signal. The passband is between lowCutoff and highCutoff.
        /// </summary>
        /// <param name="signal">The input signal.</param>
        /// <param name="lowCutoff">The lower cutoff frequency in Hz.</param>
        /// <param name="highCutoff">The upper cutoff frequency in Hz.</param>
        /// <param name="sampleRate">The sampling rate in Hz.</param>
        /// <param name="order">The filter order. Default is 10.</param>
        /// <returns>The filtered signal.</returns>
        public static double[] BandPass(double[] signal, double lowCutoff, double highCutoff, double sampleRate, int order = 10)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (lowCutoff <= 0) throw new ArgumentException("Low cutoff must be positive.");
            if (highCutoff <= lowCutoff) throw new ArgumentException("High cutoff must be greater than low cutoff.");
            if (highCutoff >= sampleRate / 2.0)
                throw new ArgumentException("High cutoff must be less than sampleRate/2.");
            if (sampleRate <= 0) throw new ArgumentException("Sample rate must be positive.");
            if (order < 1) throw new ArgumentException("Order must be at least 1.");

            double[] lowPassHigh = DesignLowPass(highCutoff, sampleRate, order);
            double[] lowPassLow = DesignLowPass(lowCutoff, sampleRate, order);

            int len = order + 1;
            double[] bandPass = new double[len];
            for (int i = 0; i < len; i++)
            {
                bandPass[i] = lowPassHigh[i] - lowPassLow[i];
            }

            return ApplyFIR(signal, bandPass);
        }

        /// <summary>
        /// Applies a simple moving average filter (boxcar) to the signal.
        /// </summary>
        /// <param name="signal">The input signal.</param>
        /// <param name="windowSize">The size of the averaging window (must be positive).</param>
        /// <returns>The filtered signal of the same length as the input.</returns>
        public static double[] MovingAverage(double[] signal, int windowSize)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (windowSize <= 0) throw new ArgumentException("Window size must be positive.");

            int n = signal.Length;
            double[] result = new double[n];
            double invWindow = 1.0 / windowSize;

            double sum = 0.0;
            for (int i = 0; i < System.Math.Min(windowSize, n); i++)
            {
                sum += signal[i];
            }

            int half = windowSize / 2;
            for (int i = 0; i < n; i++)
            {
                int start = i - half;
                int end = start + windowSize;

                if (i == 0)
                {
                    sum = 0.0;
                    for (int j = System.Math.Max(0, start); j < System.Math.Min(n, end); j++)
                    {
                        sum += signal[j];
                    }
                    int count = System.Math.Min(n, end) - System.Math.Max(0, start);
                    result[i] = sum / count;
                }
                else
                {
                    if (end - 1 < n && end - 1 >= 0)
                    {
                        sum += signal[end - 1];
                    }
                    if (start - 1 >= 0 && start - 1 < n)
                    {
                        sum -= signal[start - 1];
                    }

                    int validStart = System.Math.Max(0, start);
                    int validEnd = System.Math.Min(n, end);
                    int count = validEnd - validStart;
                    result[i] = count > 0 ? sum / count : 0.0;
                }
            }

            return result;
        }

        private static double[] DesignLowPass(double cutoff, double sampleRate, int order)
        {
            int len = order + 1;
            int mid = len / 2;
            double fc = cutoff / sampleRate;
            double[] coefficients = new double[len];

            for (int i = 0; i < len; i++)
            {
                double x = i - mid;
                if (System.Math.Abs(x) < 1e-15)
                {
                    coefficients[i] = 2.0 * fc;
                }
                else
                {
                    coefficients[i] = System.Math.Sin(2.0 * System.Math.PI * fc * x) / (System.Math.PI * x);
                }

                double window = 0.54 - 0.46 * System.Math.Cos(2.0 * System.Math.PI * i / (len - 1));
                coefficients[i] *= window;
            }

            double sum = 0.0;
            for (int i = 0; i < len; i++)
            {
                sum += coefficients[i];
            }
            if (sum != 0.0)
            {
                for (int i = 0; i < len; i++)
                {
                    coefficients[i] /= sum;
                }
            }

            return coefficients;
        }

        private static double[] ApplyFIR(double[] signal, double[] coefficients)
        {
            int n = signal.Length;
            int taps = coefficients.Length;
            double[] result = new double[n];

            for (int i = 0; i < n; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < taps; j++)
                {
                    int idx = i - j;
                    if (idx >= 0)
                    {
                        sum += coefficients[j] * signal[idx];
                    }
                }
                result[i] = sum;
            }

            return result;
        }
    }
}
