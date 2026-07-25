namespace MathVerse.Math.Quantum.Variational;

using System;

/// <summary>
/// Generic classical optimizer for variational quantum algorithms, supporting both
/// gradient-based (BFGS-like) and gradient-free (SPSA) optimization strategies.
/// </summary>
public sealed class QuantumOptimizer
{
    private readonly Func<double[], double> _objective;
    private readonly int _numParameters;

    /// <summary>Gets the objective function being minimized.</summary>
    public Func<double[], double> Objective => _objective;

    /// <summary>Gets the number of parameters.</summary>
    public int NumParameters => _numParameters;

    /// <summary>Creates a quantum optimizer for the specified objective function.</summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="numParameters">The number of parameters in the objective.</param>
    public QuantumOptimizer(Func<double[], double> objective, int numParameters)
    {
        _objective = objective ?? throw new ArgumentNullException(nameof(objective));
        if (numParameters < 1) throw new ArgumentOutOfRangeException(nameof(numParameters));
        _numParameters = numParameters;
    }

    /// <summary>
    /// Optimizes using a simplified BFGS quasi-Newton method with parameter-shift gradients.
    /// </summary>
    /// <param name="initialParams">Initial parameter values.</param>
    /// <param name="maxIterations">Maximum number of iterations.</param>
    /// <returns>The optimization result.</returns>
    public OptimizerResult OptimizeBFGS(double[] initialParams, int maxIterations)
    {
        if (initialParams == null) throw new ArgumentNullException(nameof(initialParams));
        if (maxIterations < 1) throw new ArgumentOutOfRangeException(nameof(maxIterations));

        int n = _numParameters;
        var x = (double[])initialParams.Clone();
        var H = IdentityMatrix(n);
        double fPrev = _objective(x);
        double fBest = fPrev;
        var xBest = (double[])x.Clone();

        for (int iter = 0; iter < maxIterations; iter++)
        {
            var g = ComputeGradient(x);
            var direction = MatrixVectorMultiply(H, g);
            for (int i = 0; i < n; i++) direction[i] = -direction[i];

            double step = BacktrackingLineSearch(x, direction, g, fPrev);
            var xNew = new double[n];
            for (int i = 0; i < n; i++) xNew[i] = x[i] + step * direction[i];

            double fNew = _objective(xNew);
            if (fNew < fBest) { fBest = fNew; xBest = (double[])xNew.Clone(); }

            var gNew = ComputeGradient(xNew);
            var s = new double[n];
            var y = new double[n];
            for (int i = 0; i < n; i++) { s[i] = xNew[i] - x[i]; y[i] = gNew[i] - g[i]; }

            double dotSY = DotProduct(s, y);
            if (dotSY > 1e-15)
            {
                var Hs = MatrixVectorMultiply(H, s);
                double rho = 1.0 / dotSY;
                H = UpdateBFGS(H, s, y, Hs, rho);
            }

            x = xNew;
            fPrev = fNew;

            if (System.Math.Abs(fNew - fBest) < 1e-10 && iter > 0)
                return new OptimizerResult(fBest, xBest, iter + 1, true);
        }

        return new OptimizerResult(fBest, xBest, maxIterations, false);
    }

    /// <summary>
    /// Optimizes using the Simultaneous Perturbation Stochastic Approximation (SPSA) algorithm.
    /// </summary>
    /// <param name="initialParams">Initial parameter values.</param>
    /// <param name="maxIterations">Maximum number of iterations.</param>
    /// <param name="a">The learning rate scaling parameter.</param>
    /// <param name="c">The gradient estimation perturbation scaling parameter.</param>
    /// <returns>The optimization result.</returns>
    public OptimizerResult OptimizeSPSA(double[] initialParams, int maxIterations, double a = 0.1, double c = 0.1)
    {
        if (initialParams == null) throw new ArgumentNullException(nameof(initialParams));
        if (maxIterations < 1) throw new ArgumentOutOfRangeException(nameof(maxIterations));

        int n = _numParameters;
        var x = (double[])initialParams.Clone();
        var rng = new Random(42);
        double fBest = _objective(x);
        var xBest = (double[])x.Clone();

        for (int iter = 0; iter < maxIterations; iter++)
        {
            double ak = a / System.Math.Pow(iter + 1.0, 0.602);
            double ck = c / System.Math.Pow(iter + 1.0, 0.101);

            var delta = new double[n];
            for (int i = 0; i < n; i++)
                delta[i] = rng.NextDouble() < 0.5 ? 1.0 : -1.0;

            var xPlus = new double[n];
            var xMinus = new double[n];
            for (int i = 0; i < n; i++)
            {
                xPlus[i] = x[i] + ck * delta[i];
                xMinus[i] = x[i] - ck * delta[i];
            }

            double fPlus = _objective(xPlus);
            double fMinus = _objective(xMinus);
            double gHat = (fPlus - fMinus) / (2.0 * ck);

            for (int i = 0; i < n; i++)
                x[i] -= ak * gHat * delta[i];

            double fCurrent = _objective(x);
            if (fCurrent < fBest) { fBest = fCurrent; xBest = (double[])x.Clone(); }
        }

        return new OptimizerResult(fBest, xBest, maxIterations, false);
    }

    private double[] ComputeGradient(double[] x)
    {
        return ParameterShiftGradient.ComputeGradient(_objective, x);
    }

    private double BacktrackingLineSearch(double[] x, double[] direction, double[] gradient, double fCurrent)
    {
        double step = 1.0;
        double c1 = 1e-4;
        double decrease = 0.5;
        int maxLineSearch = 20;

        double directional = DotProduct(gradient, direction);
        for (int i = 0; i < maxLineSearch; i++)
        {
            var xTrial = new double[x.Length];
            for (int j = 0; j < x.Length; j++) xTrial[j] = x[j] + step * direction[j];
            double fTrial = _objective(xTrial);
            if (fTrial <= fCurrent + c1 * step * directional) break;
            step *= decrease;
        }
        return step;
    }

    private static double[] IdentityMatrix(int n)
    {
        var result = new double[n * n];
        for (int i = 0; i < n; i++) result[i * n + i] = 1.0;
        return result;
    }

    private static double[] MatrixVectorMultiply(double[] matrix, double[] vector)
    {
        int n = vector.Length;
        var result = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
                sum += matrix[i * n + j] * vector[j];
            result[i] = sum;
        }
        return result;
    }

    private static double[] UpdateBFGS(double[] H, double[] s, double[] y, double[] Hs, double rho)
    {
        int n = s.Length;
        var result = new double[n * n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i * n + j] = H[i * n + j]
                    + rho * y[i] * y[j]
                    - rho * Hs[i] * Hs[j];
        return result;
    }

    private static double DotProduct(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
        return sum;
    }
}

/// <summary>
/// Represents the result of a classical optimization.
/// </summary>
public sealed class OptimizerResult
{
    /// <summary>Gets the optimal objective value found.</summary>
    public double OptimalValue { get; }

    /// <summary>Gets the optimal parameter values.</summary>
    public double[] OptimalParameters { get; }

    /// <summary>Gets the number of iterations performed.</summary>
    public int Iterations { get; }

    /// <summary>Gets whether the optimizer converged.</summary>
    public bool Converged { get; }

    /// <summary>Creates an optimizer result.</summary>
    public OptimizerResult(double optimalValue, double[] optimalParameters, int iterations, bool converged)
    {
        OptimalValue = optimalValue;
        OptimalParameters = optimalParameters ?? throw new ArgumentNullException(nameof(optimalParameters));
        Iterations = iterations;
        Converged = converged;
    }
}
