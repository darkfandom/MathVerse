namespace MathVerse.Math.Visualization.SimulationVisualization;

using System.Collections.Generic;

/// <summary>Represents a single sample in a waveform.</summary>
public sealed record WaveformSample
{
    /// <summary>Time value (seconds).</summary>
    public required double Time { get; init; }

    /// <summary>Amplitude value.</summary>
    public required double Amplitude { get; init; }

    /// <summary>Normalized amplitude (0-1) for display.</summary>
    public required double NormalizedAmplitude { get; init; }
}

/// <summary>Complete data for waveform visualization.</summary>
public sealed record WaveformData
{
    /// <summary>Waveform samples with time and amplitude.</summary>
    public required IReadOnlyList<WaveformSample> Samples { get; init; }

    /// <summary>Sample rate in Hz.</summary>
    public required double SampleRate { get; init; }

    /// <summary>Duration in seconds.</summary>
    public required double Duration { get; init; }

    /// <summary>Peak amplitude (absolute maximum).</summary>
    public required double PeakAmplitude { get; init; }

    /// <summary>RMS amplitude.</summary>
    public required double RMSAmplitude { get; init; }
}

/// <summary>Represents a single point in the frequency spectrum.</summary>
public sealed record SpectrumPoint
{
    /// <summary>Frequency in Hz.</summary>
    public required double Frequency { get; init; }

    /// <summary>Magnitude.</summary>
    public required double Magnitude { get; init; }

    /// <summary>Normalized magnitude (0-1) for display.</summary>
    public required double NormalizedMagnitude { get; init; }
}

/// <summary>Complete data for spectrum visualization.</summary>
public sealed record SpectrumData
{
    /// <summary>Spectrum points.</summary>
    public required IReadOnlyList<SpectrumPoint> Points { get; init; }

    /// <summary>Frequency range minimum.</summary>
    public required double MinFrequency { get; init; }

    /// <summary>Frequency range maximum.</summary>
    public required double MaxFrequency { get; init; }

    /// <summary>Dominant frequency (peak).</summary>
    public required double DominantFrequency { get; init; }
}

/// <summary>Visualizes signal processing data as waveforms and frequency spectra.</summary>
public sealed class SignalVisualizer
{
    /// <summary>
    /// Creates a waveform visualization from time-domain samples.
    /// </summary>
    /// <param name="samples">Signal amplitude samples.</param>
    /// <param name="sampleRate">Sample rate in Hz.</param>
    /// <returns>Waveform data with time values and amplitude metrics.</returns>
    public WaveformData CreateWaveform(double[] samples, double sampleRate)
    {
        if (samples == null || samples.Length == 0 || sampleRate <= 0.0)
        {
            return new WaveformData
            {
                Samples = [],
                SampleRate = 0.0,
                Duration = 0.0,
                PeakAmplitude = 0.0,
                RMSAmplitude = 0.0
            };
        }

        int n = samples.Length;
        double duration = (double)(n - 1) / sampleRate;
        double peak = 0.0;
        double sumSquares = 0.0;

        var waveformSamples = new List<WaveformSample>();

        for (int i = 0; i < n; i++)
        {
            double absVal = System.Math.Abs(samples[i]);
            if (absVal > peak) peak = absVal;
            sumSquares += samples[i] * samples[i];

            double time = (double)i / sampleRate;

            waveformSamples.Add(new WaveformSample
            {
                Time = time,
                Amplitude = samples[i],
                NormalizedAmplitude = 0.0
            });
        }

        double rms = System.Math.Sqrt(sumSquares / (double)n);

        for (int i = 0; i < waveformSamples.Count; i++)
        {
            double norm = peak > 1e-15
                ? (waveformSamples[i].Amplitude + peak) / (2.0 * peak)
                : 0.5;

            waveformSamples[i] = new WaveformSample
            {
                Time = waveformSamples[i].Time,
                Amplitude = waveformSamples[i].Amplitude,
                NormalizedAmplitude = norm
            };
        }

        return new WaveformData
        {
            Samples = waveformSamples,
            SampleRate = sampleRate,
            Duration = duration,
            PeakAmplitude = peak,
            RMSAmplitude = rms
        };
    }

    /// <summary>
    /// Creates a frequency spectrum visualization from magnitude data.
    /// </summary>
    /// <param name="magnitudes">Magnitude values per frequency bin.</param>
    /// <param name="frequencies">Corresponding frequency values in Hz.</param>
    /// <returns>Spectrum data with dominant frequency.</returns>
    public SpectrumData CreateSpectrum(double[] magnitudes, double[] frequencies)
    {
        if (magnitudes == null || frequencies == null || magnitudes.Length == 0)
        {
            return new SpectrumData
            {
                Points = [],
                MinFrequency = 0.0,
                MaxFrequency = 0.0,
                DominantFrequency = 0.0
            };
        }

        int count = System.Math.Min(magnitudes.Length, frequencies.Length);
        double maxMag = 0.0;
        double dominantFreq = 0.0;
        double minFreq = frequencies[0];
        double maxFreq = frequencies[0];

        for (int i = 0; i < count; i++)
        {
            if (magnitudes[i] > maxMag)
            {
                maxMag = magnitudes[i];
                dominantFreq = frequencies[i];
            }
            if (frequencies[i] < minFreq) minFreq = frequencies[i];
            if (frequencies[i] > maxFreq) maxFreq = frequencies[i];
        }

        var points = new List<SpectrumPoint>();
        for (int i = 0; i < count; i++)
        {
            points.Add(new SpectrumPoint
            {
                Frequency = frequencies[i],
                Magnitude = magnitudes[i],
                NormalizedMagnitude = maxMag > 1e-15 ? magnitudes[i] / maxMag : 0.0
            });
        }

        return new SpectrumData
        {
            Points = points,
            MinFrequency = minFreq,
            MaxFrequency = maxFreq,
            DominantFrequency = dominantFreq
        };
    }
}
