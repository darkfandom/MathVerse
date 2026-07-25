namespace MathVerse.Math.AI.MathematicalLearning;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Learns equation patterns from input-output examples and extracts structural information.</summary>
public sealed class EquationPatternLearner
{
    private readonly List<LearnedPattern> _patterns = new();

    /// <summary>Gets the number of learned patterns.</summary>
    public int PatternCount => _patterns.Count;

    /// <summary>Learns equation patterns from input-output example pairs.</summary>
    /// <param name="examples">List of (input, output) expression pairs.</param>
    /// <returns>List of learned patterns describing the relationship.</returns>
    public List<LearnedPattern> LearnFromExamples(List<(string Input, string Output)> examples)
    {
        if (examples == null || examples.Count == 0)
            throw new ArgumentException("Examples cannot be null or empty.", nameof(examples));

        _patterns.Clear();

        List<(double[] Inputs, double Outputs)> numericExamples = new();
        foreach (var (input, output) in examples)
        {
            double[] inputs = ParseNumericVector(input);
            double outputVal = ParseNumericValue(output);
            if (inputs.Length > 0)
                numericExamples.Add((inputs, outputVal));
        }

        if (numericExamples.Count == 0)
            return _patterns;

        int numVars = numericExamples[0].Inputs.Length;

        if (DetectPolynomialPattern(numericExamples, numVars))
            return _patterns;
        if (DetectExponentialPattern(numericExamples))
            return _patterns;
        if (DetectPeriodicPattern(numericExamples))
            return _patterns;
        if (DetectLogarithmicPattern(numericExamples))
            return _patterns;
        if (DetectPowerPattern(numericExamples))
            return _patterns;

        DetectLinearPattern(numericExamples, numVars);

        return _patterns;
    }

    /// <summary>Extracts coefficients and exponents from a polynomial-like relationship.</summary>
    /// <param name="examples">Numeric data points.</param>
    /// <param name="degree">Maximum polynomial degree to attempt.</param>
    /// <returns>Tuple of (coefficients, exponents).</returns>
    public (double[] Coefficients, double[] Exponents) ExtractPolynomialCoefficients(List<(double[] Inputs, double Outputs)> examples, int degree = 3)
    {
        if (examples == null || examples.Count == 0)
            throw new ArgumentException("Examples cannot be null or empty.", nameof(examples));
        if (degree < 1)
            throw new ArgumentException("Degree must be at least 1.", nameof(degree));

        int n = examples.Count;
        int p = degree + 1;

        if (n < p)
            return (Array.Empty<double>(), Array.Empty<double>());

        double[][] X = new double[n][];
        double[] y = new double[n];

        for (int i = 0; i < n; i++)
        {
            X[i] = new double[p];
            double x = examples[i].Inputs.Length > 0 ? examples[i].Inputs[0] : 0.0;
            for (int j = 0; j < p; j++)
                X[i][j] = System.Math.Pow(x, j);
            y[i] = examples[i].Outputs;
        }

        double[] coeffs = SolveLeastSquares(X, y);
        double[] exponents = new double[coeffs.Length];
        for (int i = 0; i < exponents.Length; i++)
            exponents[i] = i;

        return (coeffs, exponents);
    }

    /// <summary>Detects whether data follows an exponential relationship y = a * e^(bx).</summary>
    /// <param name="examples">Numeric data points.</param>
    /// <returns>Tuple of (isExponential, a, b).</returns>
    public (bool IsExponential, double A, double B) DetectExponentialFit(List<(double[] Inputs, double Outputs)> examples)
    {
        if (examples == null || examples.Count < 2)
            return (false, 0.0, 0.0);

        List<(double[] Inputs, double Outputs)> positiveOutputs = new();
        foreach (var ex in examples)
        {
            if (ex.Outputs > 0.0)
                positiveOutputs.Add(ex);
        }

        if (positiveOutputs.Count < 2)
            return (false, 0.0, 0.0);

        List<(double[] Inputs, double Outputs)> logData = new();
        foreach (var ex in positiveOutputs)
        {
            double[] x = new double[] { ex.Inputs.Length > 0 ? ex.Inputs[0] : 0.0 };
            double logY = System.Math.Log(ex.Outputs);
            logData.Add((x, logY));
        }

        var (intercept, slope) = FitLinear(logData);
        double a = System.Math.Exp(intercept);
        double b = slope;

        double r2 = ComputeR2(examples, (x) => a * System.Math.Exp(b * x));
        bool isExponential = r2 > 0.95;

        return (isExponential, a, b);
    }

    private bool DetectPolynomialPattern(List<(double[] Inputs, double Outputs)> examples, int numVars)
    {
        for (int degree = 1; degree <= 4; degree++)
        {
            var (coeffs, _) = ExtractPolynomialCoefficients(examples, degree);
            if (coeffs.Length == 0)
                continue;

            double r2 = ComputeR2(examples, x =>
            {
                double sum = 0.0;
                for (int j = 0; j < coeffs.Length; j++)
                    sum += coeffs[j] * System.Math.Pow(x, j);
                return sum;
            });

            if (r2 > 0.99)
            {
                _patterns.Add(new LearnedPattern
                {
                    Type = "Polynomial",
                    Degree = degree,
                    Coefficients = coeffs,
                    Confidence = r2,
                    Description = $"Polynomial of degree {degree} with R² = {r2:F4}"
                });
                return true;
            }
        }
        return false;
    }

    private bool DetectExponentialPattern(List<(double[] Inputs, double Outputs)> examples)
    {
        var (isExp, a, b) = DetectExponentialFit(examples);
        if (isExp)
        {
            _patterns.Add(new LearnedPattern
            {
                Type = "Exponential",
                Coefficients = new double[] { a, b },
                Confidence = 0.95,
                Description = $"Exponential: {a:F4} * e^({b:F4} * x)"
            });
            return true;
        }
        return false;
    }

    private bool DetectPeriodicPattern(List<(double[] Inputs, double Outputs)> examples)
    {
        if (examples.Count < 4)
            return false;

        int n = examples.Count;
        double[] ys = new double[n];
        for (int i = 0; i < n; i++)
            ys[i] = examples[i].Outputs;

        double mean = 0.0;
        for (int i = 0; i < n; i++)
            mean += ys[i];
        mean /= n;

        double variance = 0.0;
        for (int i = 0; i < n; i++)
        {
            double diff = ys[i] - mean;
            variance += diff * diff;
        }
        variance /= n;

        if (variance < 1e-10)
            return false;

        int signChanges = 0;
        for (int i = 1; i < n; i++)
        {
            double d1 = ys[i - 1] - mean;
            double d2 = ys[i] - mean;
            if (d1 * d2 < 0)
                signChanges++;
        }

        if (signChanges >= 2)
        {
            _patterns.Add(new LearnedPattern
            {
                Type = "Periodic",
                Confidence = System.Math.Min(1.0, signChanges / (double)n),
                Description = $"Periodic pattern detected with {signChanges} sign changes around mean."
            });
            return true;
        }

        return false;
    }

    private bool DetectLogarithmicPattern(List<(double[] Inputs, double Outputs)> examples)
    {
        if (examples.Count < 3)
            return false;

        List<(double[] Inputs, double Outputs)> logData = new();
        foreach (var ex in examples)
        {
            double x = ex.Inputs.Length > 0 ? ex.Inputs[0] : 0.0;
            if (x <= 0)
                continue;
            double[] logX = new double[] { System.Math.Log(x) };
            logData.Add((logX, ex.Outputs));
        }

        if (logData.Count < 3)
            return false;

        var (intercept, slope) = FitLinear(logData);
        double r2 = ComputeR2(examples, x => x > 0 ? intercept + slope * System.Math.Log(x) : 0.0);

        if (r2 > 0.99)
        {
            _patterns.Add(new LearnedPattern
            {
                Type = "Logarithmic",
                Coefficients = new double[] { intercept, slope },
                Confidence = r2,
                Description = $"Logarithmic: {intercept:F4} + {slope:F4} * log(x)"
            });
            return true;
        }

        return false;
    }

    private bool DetectPowerPattern(List<(double[] Inputs, double Outputs)> examples)
    {
        if (examples.Count < 3)
            return false;

        List<(double[] Inputs, double Outputs)> logData = new();
        foreach (var ex in examples)
        {
            double x = ex.Inputs.Length > 0 ? ex.Inputs[0] : 0.0;
            if (x <= 0 || ex.Outputs <= 0)
                continue;
            double[] logX = new double[] { System.Math.Log(x) };
            logData.Add((logX, System.Math.Log(ex.Outputs)));
        }

        if (logData.Count < 3)
            return false;

        var (intercept, slope) = FitLinear(logData);
        double a = System.Math.Exp(intercept);
        double b = slope;

        double r2 = ComputeR2(examples, x => x > 0 ? a * System.Math.Pow(x, b) : 0.0);

        if (r2 > 0.99)
        {
            _patterns.Add(new LearnedPattern
            {
                Type = "Power",
                Coefficients = new double[] { a, b },
                Confidence = r2,
                Description = $"Power: {a:F4} * x^{b:F4}"
            });
            return true;
        }

        return false;
    }

    private void DetectLinearPattern(List<(double[] Inputs, double Outputs)> examples, int numVars)
    {
        var (intercept, slope) = FitLinear(examples);
        double r2 = ComputeR2(examples, x => intercept + slope * x);

        _patterns.Add(new LearnedPattern
        {
            Type = "Linear",
            Coefficients = new double[] { intercept, slope },
            Confidence = r2,
            Description = $"Linear: {intercept:F4} + {slope:F4} * x (R² = {r2:F4})"
        });
    }

    private static (double Intercept, double Slope) FitLinear(List<(double[] Inputs, double Outputs)> data)
    {
        int n = data.Count;
        if (n == 0)
            return (0.0, 0.0);

        double sumX = 0.0, sumY = 0.0, sumXY = 0.0, sumX2 = 0.0;
        foreach (var (inputs, output) in data)
        {
            double x = inputs.Length > 0 ? inputs[0] : 0.0;
            sumX += x;
            sumY += output;
            sumXY += x * output;
            sumX2 += x * x;
        }

        double denom = n * sumX2 - sumX * sumX;
        if (System.Math.Abs(denom) < 1e-15)
            return (sumY / n, 0.0);

        double slope = (n * sumXY - sumX * sumY) / denom;
        double intercept = (sumY - slope * sumX) / n;

        return (intercept, slope);
    }

    private static double[] SolveLeastSquares(double[][] X, double[] y)
    {
        int n = X.Length;
        int p = X[0].Length;

        double[][] Xt = new double[p][];
        for (int j = 0; j < p; j++)
        {
            Xt[j] = new double[n];
            for (int i = 0; i < n; i++)
                Xt[j][i] = X[i][j];
        }

        double[][] XtX = new double[p][];
        for (int i = 0; i < p; i++)
        {
            XtX[i] = new double[p];
            for (int j = 0; j < p; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                    sum += Xt[i][k] * X[k][j];
                XtX[i][j] = sum;
            }
        }

        double[] Xty = new double[p];
        for (int i = 0; i < p; i++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
                sum += Xt[i][k] * y[k];
            Xty[i] = sum;
        }

        double[][] XtXInv = InvertMatrix(XtX);
        double[] coeffs = new double[p];
        for (int i = 0; i < p; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < p; j++)
                sum += XtXInv[i][j] * Xty[j];
            coeffs[i] = sum;
        }

        return coeffs;
    }

    private static double ComputeR2(List<(double[] Inputs, double Outputs)> examples, Func<double, double> predictor)
    {
        double mean = 0.0;
        foreach (var (_, output) in examples)
            mean += output;
        mean /= examples.Count;

        double ssRes = 0.0, ssTot = 0.0;
        foreach (var (inputs, output) in examples)
        {
            double x = inputs.Length > 0 ? inputs[0] : 0.0;
            double predicted = predictor(x);
            ssRes += (output - predicted) * (output - predicted);
            ssTot += (output - mean) * (output - mean);
        }

        if (ssTot < 1e-15)
            return 1.0;

        return 1.0 - ssRes / ssTot;
    }

    private static double[] ParseNumericVector(string s)
    {
        string[] parts = s.Split(new[] { ',', ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
        List<double> values = new();
        foreach (string part in parts)
        {
            if (double.TryParse(part.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                values.Add(val);
        }
        return values.ToArray();
    }

    private static double ParseNumericValue(string s)
    {
        if (double.TryParse(s.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
            return val;
        return 0.0;
    }

    private static double[][] InvertMatrix(double[][] matrix)
    {
        int n = matrix.Length;
        double[][] augmented = new double[n][];
        for (int i = 0; i < n; i++)
        {
            augmented[i] = new double[2 * n];
            for (int j = 0; j < n; j++)
            {
                augmented[i][j] = matrix[i][j];
                augmented[i][n + j] = (i == j) ? 1.0 : 0.0;
            }
        }

        for (int col = 0; col < n; col++)
        {
            int maxRow = col;
            double maxVal = System.Math.Abs(augmented[col][col]);
            for (int row = col + 1; row < n; row++)
            {
                double absVal = System.Math.Abs(augmented[row][col]);
                if (absVal > maxVal) { maxVal = absVal; maxRow = row; }
            }
            if (maxRow != col)
            {
                double[] temp = augmented[col];
                augmented[col] = augmented[maxRow];
                augmented[maxRow] = temp;
            }

            double pivot = augmented[col][col];
            if (System.Math.Abs(pivot) < 1e-12)
                return matrix;

            for (int j = 0; j < 2 * n; j++)
                augmented[col][j] /= pivot;

            for (int row = 0; row < n; row++)
            {
                if (row == col) continue;
                double factor = augmented[row][col];
                for (int j = 0; j < 2 * n; j++)
                    augmented[row][j] -= factor * augmented[col][j];
            }
        }

        double[][] inverse = new double[n][];
        for (int i = 0; i < n; i++)
        {
            inverse[i] = new double[n];
            for (int j = 0; j < n; j++)
                inverse[i][j] = augmented[i][n + j];
        }
        return inverse;
    }
}

/// <summary>Describes a learned pattern in mathematical data.</summary>
public sealed class LearnedPattern
{
    /// <summary>Gets the pattern type (e.g., Polynomial, Exponential, Periodic, Logarithmic, Power, Linear).</summary>
    public string Type { get; init; } = "";

    /// <summary>Gets the polynomial degree (if applicable).</summary>
    public int Degree { get; init; }

    /// <summary>Gets the extracted coefficients.</summary>
    public double[] Coefficients { get; init; } = Array.Empty<double>();

    /// <summary>Gets the confidence level of the pattern match.</summary>
    public double Confidence { get; init; }

    /// <summary>Gets a human-readable description of the pattern.</summary>
    public string Description { get; init; } = "";
}
