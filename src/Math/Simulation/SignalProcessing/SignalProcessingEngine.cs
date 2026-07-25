namespace MathVerse.Math.Simulation.SignalProcessing;

using System.Collections.Immutable;
using System.Numerics;
using MathVerse.Math.Foundation;
using MathVerse.Math.Numerics.LinearAlgebra;

public static class SignalProcessingEngine
{
    public static ImmutableArray<Complex> FFT(ImmutableArray<Complex> signal)
    {
        int n = signal.Length;
        if ((n & (n - 1)) != 0)
            throw new ArgumentException("Signal length must be power of 2");

        var x = signal.ToArray();
        int logN = BitOperations.Log2((uint)n);

        // Bit-reversal permutation
        for (int i = 0; i < n; i++)
        {
            int j = BitReverse(i, logN);
            if (j > i)
                (x[i], x[j]) = (x[j], x[i]);
        }

        // Cooley-Tukey FFT
        for (int len = 2; len <= n; len <<= 1)
        {
            double angle = -2 * System.Math.PI / len;
            var wlen = new Complex(System.Math.Cos(angle), System.Math.Sin(angle));
            for (int i = 0; i < n; i += len)
            {
                var w = new Complex(1, 0);
                for (int j = 0; j < len / 2; j++)
                {
                    var u = x[i + j];
                    var v = x[i + j + len / 2] * w;
                    x[i + j] = u + v;
                    x[i + j + len / 2] = u - v;
                    w *= wlen;
                }
            }
        }

        return x.ToImmutableArray();
    }

    public static ImmutableArray<Complex> IFFT(ImmutableArray<Complex> spectrum)
    {
        var conj = spectrum.Select(c => Complex.Conjugate(c)).ToImmutableArray();
        var result = FFT(conj);
        return result.Select(c => Complex.Conjugate(c) / result.Length).ToImmutableArray();
    }

    public static ImmutableArray<double> Convolve(ImmutableArray<double> a, ImmutableArray<double> b)
    {
        int n = a.Length + b.Length - 1;
        int size = 1;
        while (size < n) size <<= 1;

        var A = a.Concat(ImmutableArray<double>.Empty).Take(size).Select(x => new Complex(x, 0)).ToImmutableArray();
        var B = b.Concat(ImmutableArray<double>.Empty).Take(size).Select(x => new Complex(x, 0)).ToImmutableArray();

        var FA = FFT(A);
        var FB = FFT(B);
        var FC = FA.Zip(FB, (a, b) => a * b).ToImmutableArray();
        var result = IFFT(FC);

        return result.Take(n).Select(c => c.Real).ToImmutableArray();
    }

    public static ImmutableArray<double> Correlate(ImmutableArray<double> a, ImmutableArray<double> b)
    {
        var reversedB = b.Reverse().ToImmutableArray();
        return Convolve(a, reversedB);
    }

    public static ImmutableArray<double> FilterFIR(ImmutableArray<double> signal, ImmutableArray<double> coefficients)
        => Convolve(signal, coefficients);

    public static ImmutableArray<double> FilterIIR(ImmutableArray<double> signal, ImmutableArray<double> b, ImmutableArray<double> a)
    {
        int n = signal.Length;
        var y = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = 0;
            for (int j = 0; j < b.Length && j <= i; j++)
                sum += b[j] * signal[i - j];
            for (int j = 1; j < a.Length && j <= i; j++)
                sum -= a[j] * y[i - j];
            y[i] = sum / a[0];
        }
        return y.ToImmutableArray();
    }

    public static ImmutableArray<double> MovingAverage(ImmutableArray<double> signal, int window)
    {
        var result = new double[signal.Length];
        for (int i = 0; i < signal.Length; i++)
        {
            double sum = 0;
            int count = 0;
            for (int j = System.Math.Max(0, i - window + 1); j <= i; j++)
            {
                sum += signal[j];
                count++;
            }
            result[i] = sum / count;
        }
        return result.ToImmutableArray();
    }

    public static ImmutableArray<double> ExponentialMovingAverage(ImmutableArray<double> signal, double alpha)
    {
        var result = new double[signal.Length];
        result[0] = signal[0];
        for (int i = 1; i < signal.Length; i++)
            result[i] = alpha * signal[i] + (1 - alpha) * result[i - 1];
        return result.ToImmutableArray();
    }

    public static ImmutableArray<double> Resample(ImmutableArray<double> signal, double factor)
    {
        int newLength = (int)(signal.Length * factor);
        var result = new double[newLength];
        for (int i = 0; i < newLength; i++)
        {
            double srcIndex = i / factor;
            int idx = (int)srcIndex;
            double frac = srcIndex - idx;
            if (idx + 1 < signal.Length)
                result[i] = signal[idx] * (1 - frac) + signal[idx + 1] * frac;
            else
                result[i] = signal[idx];
        }
        return result.ToImmutableArray();
    }

    public static (ImmutableArray<double> frequencies, ImmutableArray<double> magnitudes) PowerSpectralDensity(ImmutableArray<double> signal, double sampleRate)
    {
        var complex = signal.Select(x => new Complex(x, 0)).ToImmutableArray();
        var spectrum = FFT(complex);
        int n = spectrum.Length;
        var freqs = new double[n / 2];
        var mags = new double[n / 2];
        for (int i = 0; i < n / 2; i++)
        {
            freqs[i] = i * sampleRate / spectrum.Length;
            mags[i] = spectrum[i].Magnitude * spectrum[i].Magnitude / spectrum.Length;
        }
        return (freqs.ToImmutableArray(), mags.ToImmutableArray());
    }

    public static ImmutableArray<double> WindowFunction(int n, WindowType type)
    {
        var window = new double[n];
        for (int i = 0; i < n; i++)
        {
            window[i] = type switch
            {
                WindowType.Hann => 0.5 * (1 - System.Math.Cos(2 * System.Math.PI * i / (n - 1))),
                WindowType.Hamming => 0.54 - 0.46 * System.Math.Cos(2 * System.Math.PI * i / (n - 1)),
                WindowType.Blackman => 0.42 - 0.5 * System.Math.Cos(2 * System.Math.PI * i / (n - 1)) + 0.08 * System.Math.Cos(4 * System.Math.PI * i / (n - 1)),
                WindowType.Rectangular => 1.0,
                _ => 1.0
            };
        }
        return window.ToImmutableArray();
    }

    public enum WindowType { Rectangular, Hann, Hamming, Blackman }

    private static int BitReverse(int n, int bits)
    {
        int result = 0;
        for (int i = 0; i < bits; i++)
        {
            result = (result << 1) | (n & 1);
            n >>= 1;
        }
        return result;
    }
}