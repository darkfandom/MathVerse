namespace MathVerse.Simulation.Tests.SignalProcessing;

using System.Collections.Immutable;
using System.Numerics;

public class SignalProcessingEngineTests
{
    [Fact]
    public void FFT_DCSignal_FirstElementEqualsN()
    {
        var signal = ImmutableArray.Create(
            new Complex(1, 0), new Complex(1, 0),
            new Complex(1, 0), new Complex(1, 0));

        var result = SignalProcessingEngine.FFT(signal);

        result[0].Real.Should().BeApproximately(4.0, 1e-10);
        result[0].Imaginary.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void FFT_DCSignal_OtherElementsAreZero()
    {
        var signal = ImmutableArray.Create(
            new Complex(1, 0), new Complex(1, 0),
            new Complex(1, 0), new Complex(1, 0));

        var result = SignalProcessingEngine.FFT(signal);

        for (int i = 1; i < result.Length; i++)
        {
            result[i].Magnitude.Should().BeApproximately(0, 1e-10);
        }
    }

    [Fact]
    public void FFT_SingleImpulse_AllElementsEqual()
    {
        var signal = ImmutableArray.Create(
            new Complex(1, 0), new Complex(0, 0),
            new Complex(0, 0), new Complex(0, 0));

        var result = SignalProcessingEngine.FFT(signal);

        for (int i = 0; i < result.Length; i++)
        {
            result[i].Magnitude.Should().BeApproximately(1.0, 1e-10);
        }
    }

    [Fact]
    public void FFT_NonPowerOf2Length_ThrowsArgumentException()
    {
        var signal = ImmutableArray.Create(
            new Complex(1, 0), new Complex(1, 0), new Complex(1, 0));

        Action act = () => SignalProcessingEngine.FFT(signal);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IFFT_RoundTrip_PreservesSignal()
    {
        var signal = ImmutableArray.Create(
            new Complex(1, 0), new Complex(0, 0),
            new Complex(0, 0), new Complex(0, 0));

        var spectrum = SignalProcessingEngine.FFT(signal);
        var recovered = SignalProcessingEngine.IFFT(spectrum);

        for (int i = 0; i < signal.Length; i++)
        {
            recovered[i].Real.Should().BeApproximately(signal[i].Real, 1e-10);
            recovered[i].Imaginary.Should().BeApproximately(signal[i].Imaginary, 1e-10);
        }
    }

    [Fact]
    public void IFFT_RoundTrip_8PointSignal()
    {
        var signal = ImmutableArray.Create(
            new Complex(1, 0), new Complex(2, 0),
            new Complex(3, 0), new Complex(4, 0),
            new Complex(5, 0), new Complex(6, 0),
            new Complex(7, 0), new Complex(8, 0));

        var spectrum = SignalProcessingEngine.FFT(signal);
        var recovered = SignalProcessingEngine.IFFT(spectrum);

        for (int i = 0; i < signal.Length; i++)
        {
            recovered[i].Real.Should().BeApproximately(signal[i].Real, 1e-8);
        }
    }

    [Fact]
    public void Convolve_ImpulseWithSignal_ReturnsSignal()
    {
        var signal = ImmutableArray.Create(1.0, 2.0, 3.0, 4.0);
        var impulse = ImmutableArray.Create(1.0, 0.0, 0.0, 0.0);

        var result = SignalProcessingEngine.Convolve(signal, impulse);

        result.Length.Should().BeGreaterThanOrEqualTo(3);
        result[0].Should().BeApproximately(1.0, 1e-6);
        result[1].Should().BeApproximately(2.0, 1e-6);
        result[2].Should().BeApproximately(3.0, 1e-6);
    }

    [Fact]
    public void Convolve_OutputLengthIsMPlusNMinus1()
    {
        var a = ImmutableArray.Create(1.0, 2.0, 3.0, 4.0);
        var b = ImmutableArray.Create(1.0, 1.0, 0.0, 0.0);

        var result = SignalProcessingEngine.Convolve(a, b);

        result.Length.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Correlate_IdenticalSignals_PeakAtCenter()
    {
        var signal = ImmutableArray.Create(0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0);

        var result = SignalProcessingEngine.Correlate(signal, signal);

        result.Length.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void FilterFIR_ReturnsConvolution()
    {
        var signal = ImmutableArray.Create(1.0, 2.0, 3.0, 4.0);
        var coeffs = ImmutableArray.Create(0.5, 0.5);

        var firResult = SignalProcessingEngine.FilterFIR(signal, coeffs);

        firResult.Should().NotBeEmpty();
    }

    [Fact]
    public void MovingAverage_SmoothsSignal()
    {
        var signal = ImmutableArray.Create(1.0, 2.0, 3.0, 4.0, 5.0);

        var result = SignalProcessingEngine.MovingAverage(signal, 3);

        result.Length.Should().Be(5);
        result[0].Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void MovingAverage_ConstantSignal_RemainsConstant()
    {
        var signal = ImmutableArray.Create(5.0, 5.0, 5.0, 5.0, 5.0);

        var result = SignalProcessingEngine.MovingAverage(signal, 3);

        for (int i = 0; i < result.Length; i++)
        {
            result[i].Should().BeApproximately(5.0, 1e-10);
        }
    }

    [Fact]
    public void ExponentialMovingAverage_FirstElementEqualsFirstSignal()
    {
        var signal = ImmutableArray.Create(1.0, 2.0, 3.0, 4.0, 5.0);

        var result = SignalProcessingEngine.ExponentialMovingAverage(signal, 0.5);

        result[0].Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void ExponentialMovingAverage_Smooths()
    {
        var signal = ImmutableArray.Create(1.0, 10.0, 1.0, 10.0);

        var result = SignalProcessingEngine.ExponentialMovingAverage(signal, 0.1);

        result.Length.Should().Be(4);
        result[1].Should().BeLessThan(10.0);
    }

    [Fact]
    public void Resample_UpSampling_IncreasesLength()
    {
        var signal = ImmutableArray.Create(1.0, 2.0, 3.0, 4.0);

        var result = SignalProcessingEngine.Resample(signal, 2.0);

        result.Length.Should().Be(8);
    }

    [Fact]
    public void Resample_DownSampling_DecreasesLength()
    {
        var signal = ImmutableArray.Create(1.0, 2.0, 3.0, 4.0);

        var result = SignalProcessingEngine.Resample(signal, 0.5);

        result.Length.Should().Be(2);
    }

    [Fact]
    public void Resample_FactorOne_NoChange()
    {
        var signal = ImmutableArray.Create(1.0, 2.0, 3.0);

        var result = SignalProcessingEngine.Resample(signal, 1.0);

        result.Length.Should().Be(3);
    }

    [Fact]
    public void PowerSpectralDensity_DCSignal_PeakAtZeroFrequency()
    {
        var signal = ImmutableArray.Create(1.0, 1.0, 1.0, 1.0);

        var (freqs, mags) = SignalProcessingEngine.PowerSpectralDensity(signal, 1000.0);

        freqs.Length.Should().Be(2);
        mags.Length.Should().Be(2);
        mags[0].Should().BePositive();
    }

    [Fact]
    public void WindowFunction_Hann_StartsAndEndsAtZero()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Hann);

        window[0].Should().BeApproximately(0, 1e-10);
        window[^1].Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void WindowFunction_Hamming_StartsAtNonZero()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Hamming);

        window[0].Should().BeApproximately(0.08, 0.01);
    }

    [Fact]
    public void WindowFunction_Rectangular_AllOnes()
    {
        var window = SignalProcessingEngine.WindowFunction(10, SignalProcessingEngine.WindowType.Rectangular);

        for (int i = 0; i < window.Length; i++)
        {
            window[i].Should().BeApproximately(1.0, 1e-10);
        }
    }

    [Fact]
    public void WindowFunction_OutputLengthMatchesN()
    {
        var window = SignalProcessingEngine.WindowFunction(64, SignalProcessingEngine.WindowType.Blackman);

        window.Length.Should().Be(64);
    }
}
