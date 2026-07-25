namespace MathVerse.Math.AI.Probability;

using System;

/// <summary>Linear Kalman filter for state estimation in discrete-time linear dynamical systems.</summary>
public sealed class KalmanFilter
{
    private readonly double[][] _F;
    private readonly double[][] _H;
    private readonly double[][] _Q;
    private readonly int _stateDimension;
    private readonly int _observationDimension;

    /// <summary>Initializes a new Kalman filter with the given state-space model matrices.</summary>
    /// <param name="F">State transition matrix (n x n).</param>
    /// <param name="H">Observation matrix (m x n).</param>
    /// <param name="Q">Process noise covariance (n x n).</param>
    public KalmanFilter(double[][] F, double[][] H, double[][] Q)
    {
        if (F == null || F.Length == 0)
            throw new ArgumentException("State transition matrix cannot be null or empty.", nameof(F));
        if (H == null || H.Length == 0)
            throw new ArgumentException("Observation matrix cannot be null or empty.", nameof(H));
        if (Q == null || Q.Length == 0)
            throw new ArgumentException("Process noise covariance cannot be null or empty.", nameof(Q));

        _F = F;
        _H = H;
        _Q = Q;
        _stateDimension = F.Length;
        _observationDimension = H.Length;
    }

    /// <summary>Gets the state dimension.</summary>
    public int StateDimension => _stateDimension;

    /// <summary>Gets the observation dimension.</summary>
    public int ObservationDimension => _observationDimension;

    /// <summary>Predicts the next state estimate and covariance from the current state.</summary>
    /// <param name="x">Current state estimate.</param>
    /// <param name="P">Current state covariance.</param>
    /// <returns>Tuple of (predicted state, predicted covariance).</returns>
    public (double[] x, double[][] P) Predict(double[] x, double[][] P)
    {
        if (x == null || x.Length != _stateDimension)
            throw new ArgumentException($"State vector must have {_stateDimension} elements.", nameof(x));
        if (P == null || P.Length != _stateDimension)
            throw new ArgumentException($"Covariance matrix must be {_stateDimension}x{_stateDimension}.", nameof(P));

        double[] xPred = MultiplyMatrixVector(_F, x);
        double[][] PPred = AddMatrices(
            MultiplyMatrices(MultiplyMatrices(_F, P), TransposeMatrix(_F)),
            _Q);

        return (xPred, PPred);
    }

    /// <summary>Updates the state estimate and covariance given a measurement.</summary>
    /// <param name="x">Predicted state estimate.</param>
    /// <param name="P">Predicted state covariance.</param>
    /// <param name="z">Measurement vector.</param>
    /// <param name="measurementNoise">Measurement noise variance (scalar, applied to identity).</param>
    /// <returns>Tuple of (updated state, updated covariance).</returns>
    public (double[] x, double[][] P) Update(double[] x, double[][] P, double[] z, double measurementNoise)
    {
        if (x == null || x.Length != _stateDimension)
            throw new ArgumentException($"State vector must have {_stateDimension} elements.", nameof(x));
        if (z == null || z.Length != _observationDimension)
            throw new ArgumentException($"Measurement vector must have {_observationDimension} elements.", nameof(z));

        double[][] R = IdentityMatrix(_observationDimension);
        for (int i = 0; i < _observationDimension; i++)
            R[i][i] = measurementNoise;

        double[][] HT = TransposeMatrix(_H);
        double[][] S = AddMatrices(
            MultiplyMatrices(MultiplyMatrices(_H, P), HT),
            R);
        double[][] SInv = InvertMatrix(S);
        double[][] K = MultiplyMatrices(MultiplyMatrices(P, HT), SInv);

        double[] y = SubtractVectors(z, MultiplyMatrixVector(_H, x));
        double[] xUpdated = AddVectors(x, MultiplyMatrixVector(K, y));

        double[][] IKH = SubtractMatrices(IdentityMatrix(_stateDimension), MultiplyMatrices(K, _H));
        double[][] PUpdated = MultiplyMatrices(IKH, P);

        return (xUpdated, PUpdated);
    }

    /// <summary>Filters a sequence of observations starting from an initial state.</summary>
    /// <param name="initial">Initial state estimate.</param>
    /// <param name="observations">Sequence of measurement vectors.</param>
    /// <param name="measurementNoise">Measurement noise variance.</param>
    /// <returns>Array of filtered state estimates.</returns>
    public double[][] Filter(double[] initial, double[][] observations, double measurementNoise)
    {
        if (initial == null || initial.Length != _stateDimension)
            throw new ArgumentException($"Initial state must have {_stateDimension} elements.", nameof(initial));
        if (observations == null)
            throw new ArgumentNullException(nameof(observations));

        double[] x = (double[])initial.Clone();
        double[][] P = IdentityMatrix(_stateDimension);
        for (int i = 0; i < _stateDimension; i++)
            P[i][i] = 1.0;

        double[][] states = new double[observations.Length + 1][];
        states[0] = (double[])x.Clone();

        for (int t = 0; t < observations.Length; t++)
        {
            var (xPred, PPred) = Predict(x, P);
            var (xUpd, PUpd) = Update(xPred, PPred, observations[t], measurementNoise);
            x = xUpd;
            P = PUpd;
            states[t + 1] = (double[])x.Clone();
        }

        return states;
    }

    /// <summary>Computes the Kalman gain matrix for the given state covariance.</summary>
    /// <param name="P">State covariance matrix.</param>
    /// <returns>Kalman gain matrix.</returns>
    public double[][] ComputeKalmanGain(double[][] P)
    {
        double[][] HT = TransposeMatrix(_H);
        double[][] S = AddMatrices(
            MultiplyMatrices(MultiplyMatrices(_H, P), HT),
            IdentityMatrix(_observationDimension));
        double[][] SInv = InvertMatrix(S);
        return MultiplyMatrices(MultiplyMatrices(P, HT), SInv);
    }

    private static double[] MultiplyMatrixVector(double[][] M, double[] v)
    {
        int rows = M.Length;
        int cols = M[0].Length;
        double[] result = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < cols; j++)
                sum += M[i][j] * v[j];
            result[i] = sum;
        }
        return result;
    }

    private static double[][] MultiplyMatrices(double[][] A, double[][] B)
    {
        int m = A.Length;
        int n = A[0].Length;
        int p = B[0].Length;
        double[][] result = new double[m][];
        for (int i = 0; i < m; i++)
        {
            result[i] = new double[p];
            for (int j = 0; j < p; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                    sum += A[i][k] * B[k][j];
                result[i][j] = sum;
            }
        }
        return result;
    }

    private static double[][] TransposeMatrix(double[][] M)
    {
        int rows = M.Length;
        int cols = M[0].Length;
        double[][] result = new double[cols][];
        for (int j = 0; j < cols; j++)
        {
            result[j] = new double[rows];
            for (int i = 0; i < rows; i++)
                result[j][i] = M[i][j];
        }
        return result;
    }

    private static double[][] AddMatrices(double[][] A, double[][] B)
    {
        int n = A.Length;
        double[][] result = new double[n][];
        for (int i = 0; i < n; i++)
        {
            result[i] = new double[A[i].Length];
            for (int j = 0; j < A[i].Length; j++)
                result[i][j] = A[i][j] + B[i][j];
        }
        return result;
    }

    private static double[][] SubtractMatrices(double[][] A, double[][] B)
    {
        int n = A.Length;
        double[][] result = new double[n][];
        for (int i = 0; i < n; i++)
        {
            result[i] = new double[A[i].Length];
            for (int j = 0; j < A[i].Length; j++)
                result[i][j] = A[i][j] - B[i][j];
        }
        return result;
    }

    private static double[] AddVectors(double[] a, double[] b)
    {
        double[] result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = a[i] + b[i];
        return result;
    }

    private static double[] SubtractVectors(double[] a, double[] b)
    {
        double[] result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = a[i] - b[i];
        return result;
    }

    private static double[][] IdentityMatrix(int n)
    {
        double[][] result = new double[n][];
        for (int i = 0; i < n; i++)
        {
            result[i] = new double[n];
            result[i][i] = 1.0;
        }
        return result;
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
            if (System.Math.Abs(pivot) < 1e-12)
                throw new InvalidOperationException("Matrix is singular.");

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
