namespace MathVerse.Math.AI.MachineLearning.Regression;

using System;

/// <summary>
/// Implements polynomial regression by expanding features to polynomial degree and applying linear regression.
/// </summary>
public sealed class PolynomialRegression
{
    private readonly LinearRegression _linearRegression;
    private int _degree;

    /// <summary>
    /// Gets the polynomial degree used for feature expansion.
    /// </summary>
    public int Degree => _degree;

    /// <summary>
    /// Gets the model coefficients after training.
    /// </summary>
    public double[] Coefficients => _linearRegression.Coefficients;

    /// <summary>
    /// Initializes a new instance of the PolynomialRegression class.
    /// </summary>
    /// <param name="degree">The polynomial degree for feature expansion.</param>
    /// <exception cref="ArgumentException">Thrown when degree is less than 1.</exception>
    public PolynomialRegression(int degree = 2)
    {
        if (degree < 1)
            throw new ArgumentException("Degree must be at least 1.", nameof(degree));

        _degree = degree;
        _linearRegression = new LinearRegression();
    }

    /// <summary>
    /// Trains the polynomial regression model by expanding features and applying linear regression.
    /// </summary>
    /// <param name="X">Training feature matrix (samples x features).</param>
    /// <param name="y">Target values.</param>
    /// <exception cref="ArgumentException">Thrown when input data is invalid.</exception>
    public void Train(double[][] X, double[] y)
    {
        double[][] expandedX = ExpandFeatures(X, _degree);
        _linearRegression.Train(expandedX, y);
    }

    /// <summary>
    /// Predicts target values for the given feature matrix.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Array of predicted values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when model has not been trained.</exception>
    public double[] Predict(double[][] X)
    {
        double[][] expandedX = ExpandFeatures(X, _degree);
        return _linearRegression.Predict(expandedX);
    }

    /// <summary>
    /// Calculates the R-squared (coefficient of determination) score.
    /// </summary>
    /// <param name="actual">Actual target values.</param>
    /// <param name="predicted">Predicted values.</param>
    /// <returns>R-squared score.</returns>
    public double R2Score(double[] actual, double[] predicted)
    {
        return _linearRegression.R2Score(actual, predicted);
    }

    /// <summary>
    /// Calculates the mean squared error between actual and predicted values.
    /// </summary>
    /// <param name="actual">Actual target values.</param>
    /// <param name="predicted">Predicted values.</param>
    /// <returns>Mean squared error value.</returns>
    public double MeanSquaredError(double[] actual, double[] predicted)
    {
        return _linearRegression.MeanSquaredError(actual, predicted);
    }

    /// <summary>
    /// Expands features to polynomial combinations up to the specified degree.
    /// </summary>
    /// <param name="X">Original feature matrix.</param>
    /// <param name="degree">Polynomial degree.</param>
    /// <returns>Expanded feature matrix with polynomial terms.</returns>
    private static double[][] ExpandFeatures(double[][] X, int degree)
    {
        int n = X.Length;
        int originalFeatures = X[0].Length;

        int totalFeatures = 0;
        for (int d = 1; d <= degree; d++)
        {
            totalFeatures += BinomialCoefficient(originalFeatures + d - 1, d);
        }

        double[][] expanded = new double[n][];
        for (int i = 0; i < n; i++)
        {
            expanded[i] = new double[totalFeatures];
            int featureIndex = 0;

            for (int d = 1; d <= degree; d++)
            {
                featureIndex = GenerateCombinations(X[i], d, expanded[i], featureIndex);
            }
        }

        return expanded;
    }

    /// <summary>
    /// Calculates binomial coefficient C(n, k).
    /// </summary>
    /// <param name="n">Total number of items.</param>
    /// <param name="k">Number of items to choose.</param>
    /// <returns>Binomial coefficient value.</returns>
    private static int BinomialCoefficient(int n, int k)
    {
        if (k < 0 || k > n)
            return 0;
        if (k == 0 || k == n)
            return 1;

        int result = 1;
        for (int i = 0; i < System.Math.Min(k, n - k); i++)
        {
            result = result * (n - i) / (i + 1);
        }

        return result;
    }

    /// <summary>
    /// Generates all combinations of features for a given degree.
    /// </summary>
    /// <param name="features">Original feature values.</param>
    /// <param name="degree">Current degree.</param>
    /// <param name="output">Output array to fill.</param>
    /// <param name="startIndex">Starting index in output array.</param>
    /// <returns>Next available index in output array.</returns>
    private static int GenerateCombinations(double[] features, int degree, double[] output, int startIndex)
    {
        int n = features.Length;
        int[] indices = new int[degree];
        int currentIndex = startIndex;

        GenerateCombinationsRecursive(features, degree, 0, indices, output, ref currentIndex);

        return currentIndex;
    }

    /// <summary>
    /// Recursively generates combinations of features.
    /// </summary>
    /// <param name="features">Original feature values.</param>
    /// <param name="degree">Remaining degree to generate.</param>
    /// <param name="start">Start index for feature selection.</param>
    /// <param name="indices">Current combination indices.</param>
    /// <param name="output">Output array to fill.</param>
    /// <param name="currentIndex">Current index in output array.</param>
    private static void GenerateCombinationsRecursive(double[] features, int degree, int start,
        int[] indices, double[] output, ref int currentIndex)
    {
        int n = features.Length;

        if (degree == 0)
        {
            double product = 1.0;
            for (int i = 0; i < indices.Length; i++)
            {
                product *= features[indices[i]];
            }
            output[currentIndex] = product;
            currentIndex++;
            return;
        }

        for (int i = start; i < n; i++)
        {
            indices[indices.Length - degree] = i;
            GenerateCombinationsRecursive(features, degree - 1, i, indices, output, ref currentIndex);
        }
    }
}
