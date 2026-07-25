namespace MathVerse.Math.DataScience.Core;

using System;

/// <summary>
/// Result of evaluating a model's predictions against actual values.
/// </summary>
public sealed class ModelEvaluationResult
{
    /// <summary>
    /// Gets or sets the mean absolute error.
    /// </summary>
    public double MeanAbsoluteError { get; set; }

    /// <summary>
    /// Gets or sets the mean squared error.
    /// </summary>
    public double MeanSquaredError { get; set; }

    /// <summary>
    /// Gets or sets the root mean squared error.
    /// </summary>
    public double RootMeanSquaredError { get; set; }

    /// <summary>
    /// Gets or sets the R-squared value.
    /// </summary>
    public double RSquared { get; set; }

    /// <summary>
    /// Gets or sets the mean absolute percentage error.
    /// </summary>
    public double MeanAbsolutePercentageError { get; set; }

    /// <summary>
    /// Gets or sets the number of predictions evaluated.
    /// </summary>
    public int SampleCount { get; set; }

    /// <summary>
    /// Creates a new <see cref="ModelEvaluationResult"/> instance by computing metrics from actual and predicted values.
    /// </summary>
    /// <param name="actual">The actual values.</param>
    /// <param name="predicted">The predicted values.</param>
    /// <returns>A new model evaluation result.</returns>
    public static ModelEvaluationResult Create(double[] actual, double[] predicted)
    {
        int n = System.Math.Min(actual.Length, predicted.Length);
        double sumAbsError = 0.0;
        double sumSquaredError = 0.0;
        double sumAbsPctError = 0.0;
        double sumActual = 0.0;
        double sumSquaredActual = 0.0;

        for (int i = 0; i < n; i++)
        {
            double diff = actual[i] - predicted[i];
            sumAbsError += System.Math.Abs(diff);
            sumSquaredError += diff * diff;
            if (System.Math.Abs(actual[i]) > 1e-10)
            {
                sumAbsPctError += System.Math.Abs(diff / actual[i]);
            }
            sumActual += actual[i];
            sumSquaredActual += actual[i] * actual[i];
        }

        double mae = sumAbsError / n;
        double mse = sumSquaredError / n;
        double rmse = System.Math.Sqrt(mse);
        double meanActual = sumActual / n;
        double ssRes = sumSquaredError;
        double ssTot = sumSquaredActual - n * meanActual * meanActual;
        double r2 = ssTot > 1e-10 ? 1.0 - (ssRes / ssTot) : 0.0;
        double mape = (sumAbsPctError / n) * 100.0;

        return new ModelEvaluationResult
        {
            MeanAbsoluteError = mae,
            MeanSquaredError = mse,
            RootMeanSquaredError = rmse,
            RSquared = r2,
            MeanAbsolutePercentageError = mape,
            SampleCount = n
        };
    }
}