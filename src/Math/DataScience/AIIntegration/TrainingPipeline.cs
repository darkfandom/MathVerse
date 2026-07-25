namespace MathVerse.Math.DataScience.AIIntegration;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Provides a basic ML training pipeline that executes model training and cross-validation.
/// Supports multiple training methods including linear regression and k-nearest neighbors.
/// </summary>
public static class TrainingPipeline
{
    /// <summary>
    /// Executes a complete training pipeline: preprocess, split, train, and cross-validate.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="targetColumn">The name of the target column.</param>
    /// <param name="method">The training method: "LinearRegression", "KNN", or "RidgeRegression".</param>
    /// <param name="folds">The number of cross-validation folds.</param>
    /// <returns>A <see cref="CrossValidationResult"/> with evaluation metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the method is unknown or data is insufficient.</exception>
    public static CrossValidationResult Execute(
        Dataset ds,
        string targetColumn,
        string method = "LinearRegression",
        int folds = 5)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrWhiteSpace(targetColumn))
            throw new ArgumentException("Target column name cannot be null or empty.", nameof(targetColumn));
        if (folds < 2)
            throw new ArgumentOutOfRangeException(nameof(folds), "Folds must be at least 2.");

        AutomaticPreprocessor.Preprocess(ds);

        (double[][] X, double[] y, _, _) = DatasetPreparer.PrepareTrainingData(ds, targetColumn, 1.0);

        if (X.Length < folds)
            throw new ArgumentException($"Insufficient data points ({X.Length}) for {folds} folds.");

        (double[][] XNorm, _, double[] means, double[] stdDevs) = DatasetPreparer.Normalize(X, Array.Empty<double[]>());

        return method.ToUpperInvariant() switch
        {
            "LINEARREGRESSION" or "LR" => ExecuteLinearRegressionCV(XNorm, y, folds),
            "RIDEREGRESSION" or "RR" => ExecuteRidgeRegressionCV(XNorm, y, folds, 1.0),
            "KNN" or "KNEARESTNEIGHBORS" => ExecuteKNNCV(XNorm, y, folds, 5),
            _ => throw new ArgumentException($"Unknown training method: {method}.")
        };
    }

    private static CrossValidationResult ExecuteLinearRegressionCV(double[][] X, double[] y, int folds)
    {
        double[] foldScores = new double[folds];
        int foldSize = X.Length / folds;

        for (int f = 0; f < folds; f++)
        {
            int testStart = f * foldSize;
            int testEnd = (f == folds - 1) ? X.Length : testStart + foldSize;

            int trainCount = X.Length - (testEnd - testStart);
            double[][] trainX = new double[trainCount][];
            double[] trainY = new double[trainCount];

            int idx = 0;
            for (int i = 0; i < X.Length; i++)
            {
                if (i < testStart || i >= testEnd)
                {
                    trainX[idx] = X[i];
                    trainY[idx] = y[i];
                    idx++;
                }
            }

            double[] weights = SolveLinearRegression(trainX, trainY);

            double sse = 0.0, sst = 0.0;
            double yMean = 0.0;
            for (int i = 0; i < trainY.Length; i++) yMean += trainY[i];
            yMean /= trainY.Length;

            for (int i = testStart; i < testEnd; i++)
            {
                double predicted = PredictLinear(X[i], weights);
                double error = y[i] - predicted;
                sse += error * error;
                sst += (y[i] - yMean) * (y[i] - yMean);
            }

            foldScores[f] = sst > 1e-15 ? 1.0 - (sse / sst) : 1.0;
        }

        return CrossValidationResult.Create(foldScores, "R²");
    }

    private static CrossValidationResult ExecuteRidgeRegressionCV(double[][] X, double[] y, int folds, double alpha)
    {
        double[] foldScores = new double[folds];
        int foldSize = X.Length / folds;

        for (int f = 0; f < folds; f++)
        {
            int testStart = f * foldSize;
            int testEnd = (f == folds - 1) ? X.Length : testStart + foldSize;

            int trainCount = X.Length - (testEnd - testStart);
            double[][] trainX = new double[trainCount][];
            double[] trainY = new double[trainCount];

            int idx = 0;
            for (int i = 0; i < X.Length; i++)
            {
                if (i < testStart || i >= testEnd)
                {
                    trainX[idx] = X[i];
                    trainY[idx] = y[i];
                    idx++;
                }
            }

            double[] weights = SolveRidgeRegression(trainX, trainY, alpha);

            double sse = 0.0, sst = 0.0;
            double yMean = 0.0;
            for (int i = 0; i < trainY.Length; i++) yMean += trainY[i];
            yMean /= trainY.Length;

            for (int i = testStart; i < testEnd; i++)
            {
                double predicted = PredictLinear(X[i], weights);
                double error = y[i] - predicted;
                sse += error * error;
                sst += (y[i] - yMean) * (y[i] - yMean);
            }

            foldScores[f] = sst > 1e-15 ? 1.0 - (sse / sst) : 1.0;
        }

        return CrossValidationResult.Create(foldScores, "R²");
    }

    private static CrossValidationResult ExecuteKNNCV(double[][] X, double[] y, int folds, int k)
    {
        double[] foldScores = new double[folds];
        int foldSize = X.Length / folds;

        for (int f = 0; f < folds; f++)
        {
            int testStart = f * foldSize;
            int testEnd = (f == folds - 1) ? X.Length : testStart + foldSize;

            int trainCount = X.Length - (testEnd - testStart);
            double[][] trainX = new double[trainCount][];
            double[] trainY = new double[trainCount];

            int idx = 0;
            for (int i = 0; i < X.Length; i++)
            {
                if (i < testStart || i >= testEnd)
                {
                    trainX[idx] = X[i];
                    trainY[idx] = y[i];
                    idx++;
                }
            }

            double sse = 0.0, sst = 0.0;
            double yMean = 0.0;
            for (int i = 0; i < trainY.Length; i++) yMean += trainY[i];
            yMean /= trainY.Length;

            for (int i = testStart; i < testEnd; i++)
            {
                double predicted = PredictKNN(trainX, trainY, X[i], k);
                double error = y[i] - predicted;
                sse += error * error;
                sst += (y[i] - yMean) * (y[i] - yMean);
            }

            foldScores[f] = sst > 1e-15 ? 1.0 - (sse / sst) : 1.0;
        }

        return CrossValidationResult.Create(foldScores, "R²");
    }

    /// <summary>
    /// Solves linear regression using the normal equations: w = (X^T X)^-1 X^T y.
    /// </summary>
    private static double[] SolveLinearRegression(double[][] X, double[] y)
    {
        int n = X.Length;
        int d = X[0].Length;

        double[][] XtX = new double[d][];
        for (int i = 0; i < d; i++)
        {
            XtX[i] = new double[d];
            for (int j = 0; j < d; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                    sum += X[k][i] * X[k][j];
                XtX[i][j] = sum;
            }
        }

        double[] Xty = new double[d];
        for (int i = 0; i < d; i++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
                sum += X[k][i] * y[k];
            Xty[i] = sum;
        }

        return SolveLinearSystem(XtX, Xty);
    }

    /// <summary>
    /// Solves ridge regression: w = (X^T X + alpha * I)^-1 X^T y.
    /// </summary>
    private static double[] SolveRidgeRegression(double[][] X, double[] y, double alpha)
    {
        int n = X.Length;
        int d = X[0].Length;

        double[][] XtX = new double[d][];
        for (int i = 0; i < d; i++)
        {
            XtX[i] = new double[d];
            for (int j = 0; j < d; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                    sum += X[k][i] * X[k][j];
                XtX[i][j] = sum;
                if (i == j) XtX[i][j] += alpha;
            }
        }

        double[] Xty = new double[d];
        for (int i = 0; i < d; i++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
                sum += X[k][i] * y[k];
            Xty[i] = sum;
        }

        return SolveLinearSystem(XtX, Xty);
    }

    private static double[] SolveLinearSystem(double[][] A, double[] b)
    {
        int n = b.Length;
        double[][] augmented = new double[n][];
        for (int i = 0; i < n; i++)
        {
            augmented[i] = new double[n + 1];
            Array.Copy(A[i], augmented[i], n);
            augmented[i][n] = b[i];
        }

        for (int col = 0; col < n; col++)
        {
            int maxRow = col;
            for (int row = col + 1; row < n; row++)
            {
                if (System.Math.Abs(augmented[row][col]) > System.Math.Abs(augmented[maxRow][col]))
                    maxRow = row;
            }

            (augmented[col], augmented[maxRow]) = (augmented[maxRow], augmented[col]);

            if (System.Math.Abs(augmented[col][col]) < 1e-15)
                continue;

            for (int row = col + 1; row < n; row++)
            {
                double factor = augmented[row][col] / augmented[col][col];
                for (int j = col; j <= n; j++)
                    augmented[row][j] -= factor * augmented[col][j];
            }
        }

        double[] x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = augmented[i][n];
            for (int j = i + 1; j < n; j++)
                sum -= augmented[i][j] * x[j];
            x[i] = System.Math.Abs(augmented[i][i]) > 1e-15 ? sum / augmented[i][i] : 0.0;
        }

        return x;
    }

    private static double PredictLinear(double[] x, double[] weights)
    {
        double result = 0.0;
        int len = System.Math.Min(x.Length, weights.Length);
        for (int i = 0; i < len; i++)
            result += x[i] * weights[i];
        return result;
    }

    private static double PredictKNN(double[][] trainX, double[] trainY, double[] query, int k)
    {
        int n = trainX.Length;
        int actualK = System.Math.Min(k, n);

        double[] distances = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            int len = System.Math.Min(query.Length, trainX[i].Length);
            for (int j = 0; j < len; j++)
            {
                double d = query[j] - trainX[i][j];
                sum += d * d;
            }
            distances[i] = System.Math.Sqrt(sum);
        }

        int[] indices = new int[n];
        for (int i = 0; i < n; i++) indices[i] = i;
        Array.Sort(distances, indices);

        double sumY = 0.0;
        for (int i = 0; i < actualK; i++)
            sumY += trainY[indices[i]];

        return sumY / actualK;
    }
}
