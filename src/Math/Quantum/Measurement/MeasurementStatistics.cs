namespace MathVerse.Math.Quantum.Measurement;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Aggregates measurement outcomes from multiple shots of a quantum circuit.
/// </summary>
public sealed class MeasurementStatistics
{
    private readonly Dictionary<string, int> _counts;

    /// <summary>Gets the total number of measurement shots.</summary>
    public int Shots { get; }

    /// <summary>Gets the count for each observed basis state.</summary>
    public IReadOnlyDictionary<string, int> Counts => _counts;

    /// <summary>Creates measurement statistics from a dictionary of counts.</summary>
    public MeasurementStatistics(Dictionary<string, int> counts)
    {
        _counts = counts ?? throw new ArgumentNullException(nameof(counts));
        Shots = counts.Values.Sum();
    }

    /// <summary>Gets the empirical probability of observing the specified basis state.</summary>
    public double Probability(string basisState)
    {
        if (string.IsNullOrEmpty(basisState))
            throw new ArgumentException("Basis state cannot be null or empty.", nameof(basisState));
        return _counts.TryGetValue(basisState, out int count) ? (double)count / Shots : 0.0;
    }

    /// <summary>Returns the most frequently observed basis state.</summary>
    public string MostLikely()
    {
        if (_counts.Count == 0) throw new InvalidOperationException("No measurement results available.");
        return _counts.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    /// <summary>Gets the count for the specified basis state.</summary>
    public int GetCount(string basisState)
    {
        if (string.IsNullOrEmpty(basisState))
            throw new ArgumentException("Basis state cannot be null or empty.", nameof(basisState));
        return _counts.TryGetValue(basisState, out int count) ? count : 0;
    }
}
