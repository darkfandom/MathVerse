namespace MathVerse.Math.DataScience.AIIntegration;

using System;

/// <summary>
/// Evaluates machine learning models using cross-validation and various scoring metrics.
/// </summary>
public static class ModelEvaluator
{
    /// <summary>
    /// Performs k-fold cross-validation using a custom training function.
    /// </summary>
    /// <param name="X">The feature matrix.</param>
    /// <param name="y">The target vector.</param>
    /// <param name="trainer">A function that takes features and targets, and returns predictions plus a predict function.</param>
    /// <param name="folds">The number of cross-validation folds.</param>
    /// <returns>A <see cref="CrossValidationResult"/> with R² scores per fold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="X"/> or <paramref name="trainer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when data is insufficient for the specified number of folds.</exception>
    public static CrossValidationResult CrossValidate(
        double[][] X,
        double[] y,
        Func<double[][], double[], (double[] predicted, Func<double[], double> predictFunc)> trainer,
        int folds = 5)
    {
        if (X is null) throw new ArgumentNullException(nameof(X));
        if (y is null) throw new ArgumentNullException(nameof(y));
        if (trainer is null) throw new ArgumentNullException(nameof(trainer));
        if (X.Length != y.Length)
            throw new ArgumentException("Feature matrix and target vector must have the same number of rows.");
        if (folds < 2)
            throw new ArgumentOutOfRangeException(nameof(folds), "Folds must be at least 2.");
        if (X.Length < folds)
            throw new ArgumentException($"Insufficient data ({X.Length} rows) for {folds} folds.");

        int foldSize = X.Length / folds;
        double[] foldScores = new double[folds];

        for (int f = 0; f < folds; f++)
        {
            int testStart = f * foldSize;
            int testEnd = (f == folds - 1) ? X.Length : testStart + foldSize;
            int testCount = testEnd - testStart;
            int trainCount = X.Length - testCount;

            double[][] trainX = new double[trainCount][];
            double[] trainY = new double[trainCount];
            double[][] testX = new double[testCount][];
            double[] testY = new double[testCount];

            int trainIdx = 0;
            int testIdx = 0;

            for (int i = 0; i < X.Length; i++)
            {
                if (i >= testStart && i < testEnd)
                {
                    testX[testIdx] = X[i];
                    testY[testIdx] = y[i];
                    testIdx++;
                }
                else
                {
                    trainX[trainIdx] = X[i];
                    trainY[trainIdx] = y[i];
                    trainIdx++;
                }
            }

            var (predicted, predictFunc) = trainer(trainX, trainY);

            double[] predictions = new double[testCount];
            for (int i = 0; i < testCount; i++)
                predictions[i] = predictFunc(testX[i]);

            foldScores[f] = ComputeR2Score(testY, predictions);
        }

        return CrossValidationResult.Create(foldScores, "R²");
    }

    /// <summary>
    /// Computes the R² (coefficient of determination) score between actual and predicted values.
    /// </summary>
    /// <param name="actual">The actual values.</param>
    /// <param name="predicted">The predicted values.</param>
    /// <returns>The R² score in (-inf, 1].</returns>
    /// <exception cref="ArgumentException">Thrown when the arrays have different lengths or are empty.</exception>
    public static double ComputeR2Score(double[] actual, double[] predicted)
    {
        if (actual is null || predicted is null)
            throw new ArgumentNullException(actual is null ? nameof(actual) : nameof(predicted));
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Actual and predicted arrays must have the same length.");
        if (actual.Length == 0)
            throw new ArgumentException("Arrays cannot be empty.");

        double mean = 0.0;
        for (int i = 0; i < actual.Length; i++)
            mean += actual[i];
        mean /= actual.Length;

        double ssRes = 0.0;
        double ssTot = 0.0;
        for (int i = 0; i < actual.Length; i++)
        {
            double diff = actual[i] - predicted[i];
            ssRes += diff * diff;
            double totDiff = actual[i] - mean;
            ssTot += totDiff * totDiff;
        }

        return ssTot < 1e-15 ? 1.0 : 1.0 - (ssRes / ssTot);
    }

    /// <summary>
    /// Computes the Mean Squared Error between actual and predicted values.
    /// </summary>
    /// <param name="actual">The actual values.</param>
    /// <param name="predicted">The predicted values.</param>
    /// <returns>The MSE value.</returns>
    public static double ComputeMSE(double[] actual, double[] predicted)
    {
        if (actual is null || predicted is null)
            throw new ArgumentNullException(actual is null ? nameof(actual) : nameof(predicted));
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Actual and predicted arrays must have the same length.");
        if (actual.Length == 0)
            throw new ArgumentException("Arrays cannot be empty.");

        double sum = 0.0;
        for (int i = 0; i < actual.Length; i++)
        {
            double diff = actual[i] - predicted[i];
            sum += diff * diff;
        }
        return sum / actual.Length;
    }

    /// <summary>
    /// Computes the Root Mean Squared Error between actual and predicted values.
    /// </summary>
    /// <param name="actual">The actual values.</param>
    /// <param name="predicted">The predicted values.</param>
    /// <returns>The RMSE value.</returns>
    public static double ComputeRMSE(double[] actual, double[] predicted)
    {
        return System.Math.Sqrt(ComputeMSE(actual, predicted));
    }

    /// <summary>
    /// Computes the Mean Absolute Error between actual and predicted values.
    /// </summary>
    /// <param name="actual">The actual values.</param>
    /// <param name="predicted">The predicted values.</param>
    /// <returns>The MAE value.</returns>
    public static double ComputeMAE(double[] actual, double[] predicted)
    {
        if (actual is null || predicted is null)
            throw new ArgumentNullException(actual is null ? nameof(actual) : nameof(predicted));
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Actual and predicted arrays must have the same length.");
        if (actual.Length == 0)
            throw new ArgumentException("Arrays cannot be empty.");

        double sum = 0.0;
        for (int i = 0; i < actual.Length; i++)
            sum += System.Math.Abs(actual[i] - predicted[i]);
        return sum / actual.Length;
    }

    /// <summary>
    /// Computes Mean Absolute Percentage Error between actual and predicted values.
    /// </summary>
    /// <param name="actual">The actual values.</param>
    /// <param name="predicted">The predicted values.</param>
    /// <returns>The MAPE value as a fraction (e.g., 0.05 = 5%).</returns>
    public static double ComputeMAPE(double[] actual, double[] predicted)
    {
        if (actual is null || predicted is null)
            throw new ArgumentNullException(actual is null ? nameof(actual) : nameof(predicted));
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Actual and predicted arrays must have the same length.");
        if (actual.Length == 0)
            throw new ArgumentException("Arrays cannot be empty.");

        double sum = 0.0;
        int count = 0;
        for (int i = 0; i < actual.Length; i++)
        {
            if (System.Math.Abs(actual[i]) < 1e-15) continue;
            sum += System.Math.Abs((actual[i] - predicted[i]) / actual[i]);
            count++;
        }

        return count > 0 ? sum / count : 0.0;
    }
}
