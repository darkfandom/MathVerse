using BenchmarkDotNet.Attributes;
using System.Numerics;

namespace MathVerse.Performance.Tests.Simulation;

[MemoryDiagnoser]
public class SignalProcessingBenchmarks
{
    private ImmutableArray<Complex> _signal8;
    private ImmutableArray<Complex> _signal16;
    private ImmutableArray<Complex> _signal64;
    private ImmutableArray<Complex> _signal128;
    private ImmutableArray<Complex> _signal256;
    private ImmutableArray<Complex> _signal1024;
    private ImmutableArray<double> _realSignal16;
    private ImmutableArray<double> _realSignal64;
    private ImmutableArray<double> _realSignal256;
    private ImmutableArray<double> _filterCoeffs;

    [GlobalSetup]
    public void Setup()
    {
        _signal8 = CreateComplexSignal(8);
        _signal16 = CreateComplexSignal(16);
        _signal64 = CreateComplexSignal(64);
        _signal128 = CreateComplexSignal(128);
        _signal256 = CreateComplexSignal(256);
        _signal1024 = CreateComplexSignal(1024);
        _realSignal16 = CreateRealSignal(16);
        _realSignal64 = CreateRealSignal(64);
        _realSignal256 = CreateRealSignal(256);
        _filterCoeffs = ImmutableArray.Create(0.1, 0.2, 0.4, 0.2, 0.1);
    }

    private static ImmutableArray<Complex> CreateComplexSignal(int n)
    {
        var builder = ImmutableArray.CreateBuilder<Complex>(n);
        for (int i = 0; i < n; i++)
            builder.Add(new Complex(System.Math.Sin(2 * System.Math.PI * i / n), System.Math.Cos(2 * System.Math.PI * i / n)));
        return builder.MoveToImmutable();
    }

    private static ImmutableArray<double> CreateRealSignal(int n)
    {
        var builder = ImmutableArray.CreateBuilder<double>(n);
        for (int i = 0; i < n; i++)
            builder.Add(System.Math.Sin(2 * System.Math.PI * i / n) + 0.5 * System.Math.Sin(4 * System.Math.PI * i / n));
        return builder.MoveToImmutable();
    }

    [Benchmark]
    public ImmutableArray<Complex> FFT_Size8() => SignalProcessingEngine.FFT(_signal8);

    [Benchmark]
    public ImmutableArray<Complex> FFT_Size16() => SignalProcessingEngine.FFT(_signal16);

    [Benchmark]
    public ImmutableArray<Complex> FFT_Size64() => SignalProcessingEngine.FFT(_signal64);

    [Benchmark]
    public ImmutableArray<Complex> FFT_Size128() => SignalProcessingEngine.FFT(_signal128);

    [Benchmark]
    public ImmutableArray<Complex> FFT_Size256() => SignalProcessingEngine.FFT(_signal256);

    [Benchmark]
    public ImmutableArray<Complex> FFT_Size1024() => SignalProcessingEngine.FFT(_signal1024);

    [Benchmark]
    public ImmutableArray<Complex> IFFT_Size16() => SignalProcessingEngine.IFFT(_signal16);

    [Benchmark]
    public ImmutableArray<Complex> IFFT_Size64() => SignalProcessingEngine.IFFT(_signal64);

    [Benchmark]
    public ImmutableArray<Complex> IFFT_Size256() => SignalProcessingEngine.IFFT(_signal256);

    [Benchmark]
    public ImmutableArray<Complex> FFT_RoundTrip_Size16()
    {
        var freq = SignalProcessingEngine.FFT(_signal16);
        return SignalProcessingEngine.IFFT(freq);
    }

    [Benchmark]
    public ImmutableArray<Complex> FFT_RoundTrip_Size64()
    {
        var freq = SignalProcessingEngine.FFT(_signal64);
        return SignalProcessingEngine.IFFT(freq);
    }

    [Benchmark]
    public ImmutableArray<double> Convolve_Size16() => SignalProcessingEngine.Convolve(_realSignal16, _filterCoeffs);

    [Benchmark]
    public ImmutableArray<double> Convolve_Size64() => SignalProcessingEngine.Convolve(_realSignal64, _filterCoeffs);

    [Benchmark]
    public ImmutableArray<double> Convolve_Size256() => SignalProcessingEngine.Convolve(_realSignal256, _filterCoeffs);

    [Benchmark]
    public ImmutableArray<double> MovingAverage_Size64_Window3() => SignalProcessingEngine.MovingAverage(_realSignal64, 3);

    [Benchmark]
    public ImmutableArray<double> MovingAverage_Size64_Window10() => SignalProcessingEngine.MovingAverage(_realSignal64, 10);

    [Benchmark]
    public ImmutableArray<double> MovingAverage_Size256_Window5() => SignalProcessingEngine.MovingAverage(_realSignal256, 5);

    [Benchmark]
    public ImmutableArray<double> ExponentialMovingAverage_Size64() => SignalProcessingEngine.ExponentialMovingAverage(_realSignal64, 0.3);

    [Benchmark]
    public ImmutableArray<double> ExponentialMovingAverage_Size256() => SignalProcessingEngine.ExponentialMovingAverage(_realSignal256, 0.3);

    [Benchmark]
    public ImmutableArray<double> Resample_Size64_2x() => SignalProcessingEngine.Resample(_realSignal64, 2.0);

    [Benchmark]
    public ImmutableArray<double> Resample_Size64_05x() => SignalProcessingEngine.Resample(_realSignal64, 0.5);

    [Benchmark]
    public ImmutableArray<double> Resample_Size256_3x() => SignalProcessingEngine.Resample(_realSignal256, 3.0);

    [Benchmark]
    public ImmutableArray<double> WindowFunction_Hann_64() => SignalProcessingEngine.WindowFunction(64, SignalProcessingEngine.WindowType.Hann);

    [Benchmark]
    public ImmutableArray<double> WindowFunction_Hamming_64() => SignalProcessingEngine.WindowFunction(64, SignalProcessingEngine.WindowType.Hamming);

    [Benchmark]
    public ImmutableArray<double> WindowFunction_Blackman_64() => SignalProcessingEngine.WindowFunction(64, SignalProcessingEngine.WindowType.Blackman);

    [Benchmark]
    public ImmutableArray<double> WindowFunction_Rectangular_64() => SignalProcessingEngine.WindowFunction(64, SignalProcessingEngine.WindowType.Rectangular);

    [Benchmark]
    public ImmutableArray<double> WindowFunction_Hann_256() => SignalProcessingEngine.WindowFunction(256, SignalProcessingEngine.WindowType.Hann);

    [Benchmark]
    public (ImmutableArray<double> frequencies, ImmutableArray<double> magnitudes) PowerSpectralDensity_64() => SignalProcessingEngine.PowerSpectralDensity(_realSignal64, 1000.0);

    [Benchmark]
    public (ImmutableArray<double> frequencies, ImmutableArray<double> magnitudes) PowerSpectralDensity_256() => SignalProcessingEngine.PowerSpectralDensity(_realSignal256, 1000.0);

    [Benchmark]
    public ImmutableArray<double> Correlate_Size64() => SignalProcessingEngine.Correlate(_realSignal64, _filterCoeffs);
}
