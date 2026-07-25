namespace MathVerse.Simulation.Tests.SignalProcessing;

using System.Collections.Immutable;
using SM = global::System.Math;

public sealed class WindowTypeTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    public void Hann_Window_LengthMatchesN(int n)
    {
        var window = SignalProcessingEngine.WindowFunction(n, SignalProcessingEngine.WindowType.Hann);
        window.Length.Should().Be(n);
    }

    [Fact]
    public void Hann_StartsAtZero()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Hann);
        window[0].Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Hann_EndsAtZero()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Hann);
        window[^1].Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Hann_MiddlePeakIsOne()
    {
        int n = 101;
        var window = SignalProcessingEngine.WindowFunction(n, SignalProcessingEngine.WindowType.Hann);
        window[n / 2].Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Hann_Symmetric()
    {
        int n = 50;
        var window = SignalProcessingEngine.WindowFunction(n, SignalProcessingEngine.WindowType.Hann);
        for (int i = 0; i < n / 2; i++)
        {
            window[i].Should().BeApproximately(window[n - 1 - i], 1e-10);
        }
    }

    [Fact]
    public void Hamming_StartsAtNonZero()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Hamming);
        window[0].Should().BeApproximately(0.08, 0.01);
    }

    [Fact]
    public void Hamming_EndsAtNonZero()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Hamming);
        window[^1].Should().BeApproximately(0.08, 0.01);
    }

    [Fact]
    public void Hamming_MiddlePeakIsOne()
    {
        int n = 101;
        var window = SignalProcessingEngine.WindowFunction(n, SignalProcessingEngine.WindowType.Hamming);
        window[n / 2].Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Hamming_AllValuesBetween008And1()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Hamming);
        foreach (var v in window)
        {
            v.Should().BeGreaterThanOrEqualTo(0.08 - 0.01);
            v.Should().BeLessThanOrEqualTo(1.0 + 1e-10);
        }
    }

    [Fact]
    public void Blackman_StartsAtZero()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Blackman);
        window[0].Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Blackman_EndsAtZero()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Blackman);
        window[^1].Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Blackman_MiddlePeakIsOne()
    {
        int n = 101;
        var window = SignalProcessingEngine.WindowFunction(n, SignalProcessingEngine.WindowType.Blackman);
        window[n / 2].Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Rectangular_AllOnes()
    {
        var window = SignalProcessingEngine.WindowFunction(64, SignalProcessingEngine.WindowType.Rectangular);
        foreach (var v in window)
        {
            v.Should().BeApproximately(1.0, 1e-10);
        }
    }

    [Fact]
    public void Rectangular_LengthMatchesN()
    {
        var window = SignalProcessingEngine.WindowFunction(32, SignalProcessingEngine.WindowType.Rectangular);
        window.Length.Should().Be(32);
    }

    [Fact]
    public void Hann_KnownValues_Length10()
    {
        int n = 10;
        var window = SignalProcessingEngine.WindowFunction(n, SignalProcessingEngine.WindowType.Hann);
        for (int i = 0; i < n; i++)
        {
            double expected = 0.5 * (1 - SM.Cos(2 * SM.PI * i / (n - 1)));
            window[i].Should().BeApproximately(expected, 1e-10);
        }
    }

    [Fact]
    public void Hamming_KnownValues_Length10()
    {
        int n = 10;
        var window = SignalProcessingEngine.WindowFunction(n, SignalProcessingEngine.WindowType.Hamming);
        for (int i = 0; i < n; i++)
        {
            double expected = 0.54 - 0.46 * SM.Cos(2 * SM.PI * i / (n - 1));
            window[i].Should().BeApproximately(expected, 1e-10);
        }
    }

    [Fact]
    public void Blackman_KnownValues_Length10()
    {
        int n = 10;
        var window = SignalProcessingEngine.WindowFunction(n, SignalProcessingEngine.WindowType.Blackman);
        for (int i = 0; i < n; i++)
        {
            double expected = 0.42 - 0.5 * SM.Cos(2 * SM.PI * i / (n - 1)) + 0.08 * SM.Cos(4 * SM.PI * i / (n - 1));
            window[i].Should().BeApproximately(expected, 1e-10);
        }
    }

    [Fact]
    public void Hann_NonNegative()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Hann);
        foreach (var v in window)
        {
            v.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void Blackman_NonNegative()
    {
        var window = SignalProcessingEngine.WindowFunction(100, SignalProcessingEngine.WindowType.Blackman);
        foreach (var v in window)
        {
            v.Should().BeGreaterThanOrEqualTo(-1e-14);
        }
    }
}
