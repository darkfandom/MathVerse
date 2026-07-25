namespace MathVerse.Math.DataScience.AIIntegration;

using System;
using System.Collections.Generic;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Prepares datasets for machine learning by splitting into training and test sets
/// and extracting feature matrices and target vectors.
/// </summary>
public static class DatasetPreparer
{
    /// <summary>
    /// Splits a dataset into training and test feature matrices (X) and target vectors (y).
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="targetColumn">The name of the target column.</param>
    /// <param name="trainSplit">The fraction of data for training (default 0.8).</param>
    /// <returns>A tuple of (X train, y train, X test, y test).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the target column is missing or the split ratio is invalid.</exception>
    public static (double[][] X, double[] y, double[][] XTest, double[] yTest) PrepareTrainingData(
        Dataset ds,
        string targetColumn,
        double trainSplit = 0.8)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(targetColumn))
            throw new ArgumentException("Target column name cannot be null or empty.", nameof(targetColumn));
        if (trainSplit <= 0.0 || trainSplit >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(trainSplit), "Train split must be between 0 and 1 exclusive.");

        if (ds.Count == 0)
            throw new ArgumentException("Dataset is empty.", nameof(ds));

        var featureColumns = new List<string>();
        foreach (var kvp in ds.Rows[0])
        {
            if (!string.Equals(kvp.Key, targetColumn, StringComparison.OrdinalIgnoreCase)
                && IsNumeric(kvp.Value))
            {
                featureColumns.Add(kvp.Key);
            }
        }

        if (featureColumns.Count == 0)
            throw new ArgumentException("No numeric feature columns found in the dataset.");

        var allX = new List<double[]>();
        var allY = new List<double>();

        foreach (var row in ds.Rows)
        {
            if (!row.TryGetValue(targetColumn, out object? targetVal) || targetVal is null || !IsNumeric(targetVal))
                continue;

            double[] features = new double[featureColumns.Count];
            bool valid = true;

            for (int i = 0; i < featureColumns.Count; i++)
            {
                if (row.TryGetValue(featureColumns[i], out object? val) && val is not null && IsNumeric(val))
                    features[i] = Convert.ToDouble(val);
                else
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                allX.Add(features);
                allY.Add(Convert.ToDouble(targetVal));
            }
        }

        if (allX.Count == 0)
            throw new ArgumentException("No valid data points found after filtering.");

        int trainCount = (int)System.Math.Round(allX.Count * trainSplit);
        trainCount = System.Math.Max(1, System.Math.Min(trainCount, allX.Count - 1));

        double[][] X = new double[trainCount][];
        double[] y = new double[trainCount];
        double[][] XTest = new double[allX.Count - trainCount][];
        double[] yTest = new double[allX.Count - trainCount];

        for (int i = 0; i < trainCount; i++)
        {
            X[i] = allX[i];
            y[i] = allY[i];
        }

        for (int i = 0; i < allX.Count - trainCount; i++)
        {
            XTest[i] = allX[trainCount + i];
            yTest[i] = allY[trainCount + i];
        }

        return (X, y, XTest, yTest);
    }

    /// <summary>
    /// Normalizes feature matrices using z-score normalization across training data.
    /// Applies the same transformation to test data.
    /// </summary>
    /// <param name="XTrain">The training feature matrix.</param>
    /// <param name="XTest">The test feature matrix.</param>
    /// <returns>A tuple of normalized (XTrain, XTest, means, stdDevs).</returns>
    public static (double[][] XTrainNorm, double[][] XTestNorm, double[] Means, double[] StdDevs) Normalize(
        double[][] XTrain,
        double[][] XTest)
    {
        if (XTrain is null || XTrain.Length == 0)
            throw new ArgumentException("Training data cannot be null or empty.", nameof(XTrain));

        int features = XTrain[0].Length;
        double[] means = new double[features];
        double[] stdDevs = new double[features];

        for (int j = 0; j < features; j++)
        {
            double sum = 0.0;
            for (int i = 0; i < XTrain.Length; i++)
                sum += XTrain[i][j];
            means[j] = sum / XTrain.Length;

            double varSum = 0.0;
            for (int i = 0; i < XTrain.Length; i++)
            {
                double d = XTrain[i][j] - means[j];
                varSum += d * d;
            }
            stdDevs[j] = System.Math.Sqrt(varSum / XTrain.Length);
            if (stdDevs[j] < 1e-15) stdDevs[j] = 1.0;
        }

        double[][] XTrainNorm = NormalizeMatrix(XTrain, means, stdDevs);
        double[][] XTestNorm = XTest is not null ? NormalizeMatrix(XTest, means, stdDevs) : Array.Empty<double[]>();

        return (XTrainNorm, XTestNorm, means, stdDevs);
    }

    private static double[][] NormalizeMatrix(double[][] X, double[] means, double[] stdDevs)
    {
        double[][] result = new double[X.Length][];
        for (int i = 0; i < X.Length; i++)
        {
            result[i] = new double[X[i].Length];
            for (int j = 0; j < X[i].Length; j++)
                result[i][j] = (X[i][j] - means[j]) / stdDevs[j];
        }
        return result;
    }

    private static bool IsNumeric(object? value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
