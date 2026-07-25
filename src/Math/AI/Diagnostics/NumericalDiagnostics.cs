namespace MathVerse.Math.AI.Diagnostics;

using System.Collections.Immutable;

/// <summary>Numerical diagnostics for condition number estimation, stability analysis, and error bound computation.</summary>
public sealed class NumericalDiagnostics
{
    /// <summary>Estimates the condition number of a square matrix using the ratio of estimated largest to smallest singular values.</summary>
    /// <param name="matrix">The square matrix to analyze (array of rows).</param>
    /// <returns>A <see cref="ConditionNumberResult"/> with the estimated condition number and diagnostics.</returns>
    /// <exception cref="ArgumentException">Thrown when the matrix is null, empty, or non-square.</exception>
    public ConditionNumberResult Analyze(double[][] matrix)
    {
        if (matrix == null || matrix.Length == 0)
        {
            throw new ArgumentException("Matrix cannot be null or empty.", nameof(matrix));
        }

        int n = matrix.Length;
        for (int i = 0; i < n; i++)
        {
            if (matrix[i] == null || matrix[i].Length != n)
            {
                throw new ArgumentException($"Row {i} must have length {n} (matrix must be square).");
            }
        }

        double normInf = ComputeInfinityNorm(matrix);
        double normInfInv = ComputeInfinityNorm(InvertMatrix(matrix));

        double conditionNumber = normInf * normInfInv;

        string classification;
        if (conditionNumber < 1e2)
        {
            classification = "Well-conditioned";
        }
        else if (conditionNumber < 1e6)
        {
            classification = "Moderately ill-conditioned";
        }
        else if (conditionNumber < 1e12)
        {
            classification = "Ill-conditioned";
        }
        else
        {
            classification = "Severely ill-conditioned";
        }

        double digitsLost = System.Math.Log10(conditionNumber);
        string recommendation;

        if (conditionNumber > 1e10)
        {
            recommendation = "Use iterative refinement or arbitrary precision arithmetic.";
        }
        else if (conditionNumber > 1e6)
        {
            recommendation = "Use pivoting and consider compensated summation.";
        }
        else if (conditionNumber > 1e3)
        {
            recommendation = "Standard double precision is adequate with careful implementation.";
        }
        else
        {
            recommendation = "No special precautions needed.";
        }

        return new ConditionNumberResult
        {
            ConditionNumber = conditionNumber,
            InfinityNorm = normInf,
            InverseInfinityNorm = normInfInv,
            Classification = classification,
            DigitsOfAccuracyLost = digitsLost,
            Recommendation = recommendation
        };
    }

    /// <summary>Analyzes the local stability of a dynamical system by estimating the Jacobian eigenvalue spectrum at a given point.</summary>
    /// <param name="func">The system function f(x) whose Jacobian is to be estimated.</param>
    /// <param name="point">The state vector at which to evaluate.</param>
    /// <returns>A <see cref="StabilityAnalysisResult"/> with eigenvalue estimates and stability assessment.</returns>
    /// <exception cref="ArgumentException">Thrown when the function or point is null or empty.</exception>
    public StabilityAnalysisResult AnalyzeStability(Func<double[], double> func, double[] point)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));
        if (point == null || point.Length == 0)
            throw new ArgumentException("Point cannot be null or empty.", nameof(point));

        int n = point.Length;
        double[][] jacobian = new double[n][];
        double epsilon = System.Math.Sqrt(2.2204460492503131e-16);

        double[] f0 = new double[n];
        for (int i = 0; i < n; i++)
        {
            f0[i] = func(point);
        }

        for (int j = 0; j < n; j++)
        {
            jacobian[j] = new double[n];
            double[] perturbed = (double[])point.Clone();
            double h = epsilon * System.Math.Max(System.Math.Abs(point[j]), 1.0);
            perturbed[j] += h;

            for (int i = 0; i < n; i++)
            {
                double[] evalPoint = (double[])point.Clone();
                evalPoint[j] = perturbed[j];
                double fPlus = func(evalPoint);
                jacobian[j][i] = (fPlus - f0[i]) / h;
            }
        }

        double maxDiagReal = 0.0;
        double minDiagReal = 0.0;

        for (int i = 0; i < n; i++)
        {
            double real = jacobian[i][i];
            if (i == 0 || real > maxDiagReal) maxDiagReal = real;
            if (i == 0 || real < minDiagReal) minDiagReal = real;
        }

        double spectralRadius = System.Math.Max(System.Math.Abs(maxDiagReal), System.Math.Abs(minDiagReal));
        bool isStable = maxDiagReal < 0.0;

        double stiffnessRatio = 1.0;
        double minAbs = System.Math.Abs(minDiagReal);
        double maxAbs = System.Math.Abs(maxDiagReal);
        if (minAbs > 1e-15)
        {
            stiffnessRatio = maxAbs / minAbs;
        }

        string classification;
        if (isStable && stiffnessRatio < 10.0)
        {
            classification = "Stable and non-stiff";
        }
        else if (isStable && stiffnessRatio < 1e4)
        {
            classification = "Stable and moderately stiff";
        }
        else if (isStable)
        {
            classification = "Stable but very stiff";
        }
        else if (!isStable && stiffnessRatio < 10.0)
        {
            classification = "Unstable and non-stiff";
        }
        else
        {
            classification = "Unstable and stiff";
        }

        return new StabilityAnalysisResult
        {
            IsStable = isStable,
            SpectralRadius = spectralRadius,
            MaxRealEigenvalue = maxDiagReal,
            MinRealEigenvalue = minDiagReal,
            StiffnessRatio = stiffnessRatio,
            Classification = classification
        };
    }

    /// <summary>Estimates forward error bounds for a linear system Ax=b given a computed solution.</summary>
    /// <param name="matrix">The coefficient matrix A.</param>
    /// <param name="rhs">The right-hand side vector b.</param>
    /// <param name="computedSolution">The computed solution x.</param>
    /// <returns>An <see cref="ErrorBoundResult"/> with forward and backward error estimates.</returns>
    public ErrorBoundResult EstimateErrorBounds(double[][] matrix, double[] rhs, double[] computedSolution)
    {
        if (matrix == null || matrix.Length == 0)
            throw new ArgumentException("Matrix cannot be null or empty.", nameof(matrix));
        if (rhs == null || rhs.Length == 0)
            throw new ArgumentException("RHS cannot be null or empty.", nameof(rhs));
        if (computedSolution == null || computedSolution.Length == 0)
            throw new ArgumentException("Solution cannot be null or empty.", nameof(computedSolution));

        int n = matrix.Length;
        double[] residual = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
            {
                sum += matrix[i][j] * computedSolution[j];
            }
            residual[i] = rhs[i] - sum;
        }

        double residualNorm = VectorInfinityNorm(residual);
        double rhsNorm = VectorInfinityNorm(rhs);

        double backwardError = rhsNorm > 1e-15 ? residualNorm / rhsNorm : residualNorm;

        var condResult = Analyze(matrix);
        double forwardErrorBound = condResult.ConditionNumber * backwardError;

        return new ErrorBoundResult
        {
            BackwardError = backwardError,
            ForwardErrorBound = forwardErrorBound,
            ResidualNorm = residualNorm,
            ConditionNumber = condResult.ConditionNumber,
            Assessment = $"Backward error={backwardError:E4}, estimated forward error bound={forwardErrorBound:E4}."
        };
    }

    /// <summary>Computes the infinity norm (maximum absolute row sum) of a square matrix.</summary>
    /// <param name="matrix">The square matrix.</param>
    /// <returns>The infinity norm.</returns>
    private static double ComputeInfinityNorm(double[][] matrix)
    {
        int n = matrix.Length;
        double maxSum = 0.0;

        for (int i = 0; i < n; i++)
        {
            double rowSum = 0.0;
            for (int j = 0; j < n; j++)
            {
                rowSum += System.Math.Abs(matrix[i][j]);
            }
            if (rowSum > maxSum) maxSum = rowSum;
        }

        return maxSum;
    }

    /// <summary>Computes the infinity norm of a vector.</summary>
    /// <param name="v">The vector.</param>
    /// <returns>The infinity norm.</returns>
    private static double VectorInfinityNorm(double[] v)
    {
        double max = 0.0;
        for (int i = 0; i < v.Length; i++)
        {
            double abs = System.Math.Abs(v[i]);
            if (abs > max) max = abs;
        }
        return max;
    }

    /// <summary>Inverts a square matrix using Gaussian elimination with partial pivoting.</summary>
    /// <param name="matrix">The matrix to invert.</param>
    /// <returns>The inverted matrix.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the matrix is singular.</exception>
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
                if (absVal > maxVal)
                {
                    maxVal = absVal;
                    maxRow = row;
                }
            }

            if (maxRow != col)
            {
                double[] temp = augmented[col];
                augmented[col] = augmented[maxRow];
                augmented[maxRow] = temp;
            }

            double pivot = augmented[col][col];
            if (System.Math.Abs(pivot) < 1e-15)
                throw new InvalidOperationException("Matrix is singular or nearly singular.");

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
            {
                inverse[i][j] = augmented[i][n + j];
            }
        }

        return inverse;
    }
}

/// <summary>Result of condition number analysis.</summary>
public sealed class ConditionNumberResult
{
    /// <summary>Gets the estimated condition number.</summary>
    public double ConditionNumber { get; init; }

    /// <summary>Gets the infinity norm of the matrix.</summary>
    public double InfinityNorm { get; init; }

    /// <summary>Gets the infinity norm of the inverse.</summary>
    public double InverseInfinityNorm { get; init; }

    /// <summary>Gets the condition classification (e.g., "Well-conditioned", "Ill-conditioned").</summary>
    public string Classification { get; init; } = "";

    /// <summary>Gets the estimated number of digits of accuracy lost.</summary>
    public double DigitsOfAccuracyLost { get; init; }

    /// <summary>Gets a recommendation for handling the conditioning.</summary>
    public string Recommendation { get; init; } = "";
}

/// <summary>Result of local stability analysis.</summary>
public sealed class StabilityAnalysisResult
{
    /// <summary>Gets whether the system is locally stable (all eigenvalues have negative real parts).</summary>
    public bool IsStable { get; init; }

    /// <summary>Gets the spectral radius of the Jacobian.</summary>
    public double SpectralRadius { get; init; }

    /// <summary>Gets the maximum real part of the diagonal Jacobian elements.</summary>
    public double MaxRealEigenvalue { get; init; }

    /// <summary>Gets the minimum real part of the diagonal Jacobian elements.</summary>
    public double MinRealEigenvalue { get; init; }

    /// <summary>Gets the stiffness ratio.</summary>
    public double StiffnessRatio { get; init; }

    /// <summary>Gets the stability and stiffness classification.</summary>
    public string Classification { get; init; } = "";
}

/// <summary>Result of error bound estimation for a linear system.</summary>
public sealed class ErrorBoundResult
{
    /// <summary>Gets the backward error (residual norm / rhs norm).</summary>
    public double BackwardError { get; init; }

    /// <summary>Gets the estimated forward error bound.</summary>
    public double ForwardErrorBound { get; init; }

    /// <summary>Gets the residual norm.</summary>
    public double ResidualNorm { get; init; }

    /// <summary>Gets the condition number used in the bound computation.</summary>
    public double ConditionNumber { get; init; }

    /// <summary>Gets a human-readable assessment.</summary>
    public string Assessment { get; init; } = "";
}
