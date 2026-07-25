namespace MathVerse.Math.DataScience.UnitsIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides statistical operations that respect physical units and dimensional analysis.
/// Ensures all quantities in a computation are dimensionally compatible.
/// </summary>
public static class UnitAwareStatistics
{
    /// <summary>
    /// Computes the arithmetic mean of a list of physical quantities.
    /// All quantities must share the same dimension; the result uses the first quantity's unit.
    /// </summary>
    /// <param name="values">The list of physical quantities.</param>
    /// <returns>The mean quantity in the first quantity's unit.</returns>
    /// <exception cref="ArgumentException">Thrown when the list is empty or contains incompatible dimensions.</exception>
    public static PhysicalQuantity Mean(List<PhysicalQuantity> values)
    {
        if (values is null || values.Count == 0)
            throw new ArgumentException("Values list cannot be null or empty.", nameof(values));

        Dimension dim = values[0].Dimension;
        string unit = values[0].Unit;

        for (int i = 1; i < values.Count; i++)
        {
            if (!dim.IsEquivalentTo(values[i].Dimension))
                throw new ArgumentException(
                    $"Incompatible dimensions at index {i}: expected {dim}, got {values[i].Dimension}.");
        }

        double sum = 0.0;
        foreach (PhysicalQuantity q in values)
        {
            double valueInBase = UnitConverter.Convert(q.Value, q.Unit, unit);
            sum += valueInBase;
        }

        return new PhysicalQuantity(sum / values.Count, unit, dim);
    }

    /// <summary>
    /// Computes the standard deviation of a list of physical quantities.
    /// All quantities must share the same dimension; the result uses the first quantity's unit.
    /// </summary>
    /// <param name="values">The list of physical quantities.</param>
    /// <returns>The standard deviation quantity.</returns>
    /// <exception cref="ArgumentException">Thrown when the list has fewer than 2 elements or contains incompatible dimensions.</exception>
    public static PhysicalQuantity StdDev(List<PhysicalQuantity> values)
    {
        if (values is null || values.Count < 2)
            throw new ArgumentException("Standard deviation requires at least 2 values.", nameof(values));

        PhysicalQuantity mean = Mean(values);
        string unit = mean.Unit;

        double sumSquaredDiff = 0.0;
        foreach (PhysicalQuantity q in values)
        {
            double diff = UnitConverter.Convert(q.Value, q.Unit, unit) - mean.Value;
            sumSquaredDiff += diff * diff;
        }

        double variance = sumSquaredDiff / (values.Count - 1);
        return new PhysicalQuantity(System.Math.Sqrt(variance), unit, mean.Dimension);
    }

    /// <summary>
    /// Computes the Pearson correlation coefficient between two series of physical quantities.
    /// Both series must have compatible dimensions.
    /// </summary>
    /// <param name="x">The first series of quantities.</param>
    /// <param name="y">The second series of quantities.</param>
    /// <returns>The dimensionless correlation coefficient in [-1, 1].</returns>
    /// <exception cref="ArgumentException">Thrown when series are null, have different lengths, or incompatible dimensions.</exception>
    public static double Correlation(List<PhysicalQuantity> x, List<PhysicalQuantity> y)
    {
        if (x is null || y is null)
            throw new ArgumentNullException(x is null ? nameof(x) : nameof(y));
        if (x.Count != y.Count)
            throw new ArgumentException("Series must have the same length.");
        if (x.Count < 2)
            throw new ArgumentException("Correlation requires at least 2 data points.");

        if (!x[0].Dimension.IsEquivalentTo(y[0].Dimension))
            throw new ArgumentException(
                $"Incompatible dimensions: {x[0].Dimension} vs {y[0].Dimension}.");

        string unit = x[0].Unit;
        int n = x.Count;

        double[] xVals = new double[n];
        double[] yVals = new double[n];

        for (int i = 0; i < n; i++)
        {
            xVals[i] = UnitConverter.Convert(x[i].Value, x[i].Unit, unit);
            yVals[i] = UnitConverter.Convert(y[i].Value, y[i].Unit, unit);
        }

        double xMean = 0.0;
        double yMean = 0.0;
        for (int i = 0; i < n; i++)
        {
            xMean += xVals[i];
            yMean += yVals[i];
        }
        xMean /= n;
        yMean /= n;

        double sumXY = 0.0;
        double sumX2 = 0.0;
        double sumY2 = 0.0;

        for (int i = 0; i < n; i++)
        {
            double dx = xVals[i] - xMean;
            double dy = yVals[i] - yMean;
            sumXY += dx * dy;
            sumX2 += dx * dx;
            sumY2 += dy * dy;
        }

        double denom = System.Math.Sqrt(sumX2 * sumY2);
        if (denom < 1e-15)
            return 0.0;

        return sumXY / denom;
    }

    /// <summary>
    /// Computes the covariance between two series of physical quantities.
    /// Both series must have compatible dimensions.
    /// </summary>
    /// <param name="x">The first series.</param>
    /// <param name="y">The second series.</param>
    /// <returns>The covariance quantity.</returns>
    /// <exception cref="ArgumentException">Thrown when series are null, have different lengths, or incompatible dimensions.</exception>
    public static PhysicalQuantity Covariance(List<PhysicalQuantity> x, List<PhysicalQuantity> y)
    {
        if (x is null || y is null)
            throw new ArgumentNullException(x is null ? nameof(x) : nameof(y));
        if (x.Count != y.Count)
            throw new ArgumentException("Series must have the same length.");
        if (x.Count < 2)
            throw new ArgumentException("Covariance requires at least 2 data points.");

        if (!x[0].Dimension.IsEquivalentTo(y[0].Dimension))
            throw new ArgumentException(
                $"Incompatible dimensions: {x[0].Dimension} vs {y[0].Dimension}.");

        string unit = x[0].Unit;
        int n = x.Count;

        double xMean = 0.0;
        double yMean = 0.0;

        double[] xVals = new double[n];
        double[] yVals = new double[n];

        for (int i = 0; i < n; i++)
        {
            xVals[i] = UnitConverter.Convert(x[i].Value, x[i].Unit, unit);
            yVals[i] = UnitConverter.Convert(y[i].Value, y[i].Unit, unit);
            xMean += xVals[i];
            yMean += yVals[i];
        }

        xMean /= n;
        yMean /= n;

        double sum = 0.0;
        for (int i = 0; i < n; i++)
        {
            sum += (xVals[i] - xMean) * (yVals[i] - yMean);
        }

        double covValue = sum / (n - 1);
        Dimension resultDim = x[0].Dimension * y[0].Dimension;
        return new PhysicalQuantity(covValue, unit, resultDim);
    }
}
