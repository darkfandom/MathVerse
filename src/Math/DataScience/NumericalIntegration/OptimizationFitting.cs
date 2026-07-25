namespace MathVerse.Math.DataScience.NumericalIntegration;

using System;

/// <summary>
/// Fits model parameters to data using gradient descent optimization.
/// </summary>
public static class OptimizationFitting
{
    /// <summary>
    /// Fits a parametric model to data using gradient descent optimization.
    /// Minimizes the sum of squared residuals between the model and observed data.
    /// </summary>
    /// <param name="model">The model function: f(x, params) -> y.</param>
    /// <param name="x">The independent variable data.</param>
    /// <param name="y">The observed dependent variable data.</param>
    /// <param name="initialParams">The initial parameter guesses.</param>
    /// <param name="maxIterations">The maximum number of gradient descent iterations.</param>
    /// <param name="learningRate">The learning rate (step size) for gradient descent.</param>
    /// <param name="tolerance">Convergence tolerance for the parameter update norm.</param>
    /// <returns>The optimized parameter array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when data lengths are mismatched.</exception>
    public static double[] CurveFit(
        Func<double, double[], double> model,
        double[] x,
        double[] y,
        double[] initialParams,
        int maxIterations = 100,
        double learningRate = 0.001,
        double tolerance = 1e-12)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        if (x is null) throw new ArgumentNullException(nameof(x));
        if (y is null) throw new ArgumentNullException(nameof(y));
        if (initialParams is null) throw new ArgumentNullException(nameof(initialParams));
        if (x.Length != y.Length)
            throw new ArgumentException("x and y arrays must have the same length.");
        if (x.Length == 0)
            throw new ArgumentException("Data arrays cannot be empty.");

        int paramCount = initialParams.Length;
        double[] parameters = new double[paramCount];
        Array.Copy(initialParams, parameters, paramCount);

        int n = x.Length;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            double[] gradients = new double[paramCount];

            for (int i = 0; i < n; i++)
            {
                double predicted = EvalModel(model, x[i], parameters);
                double error = predicted - y[i];

                for (int p = 0; p < paramCount; p++)
                {
                    double grad = ComputePartialDerivative(model, x[i], parameters, p);
                    gradients[p] += 2.0 * error * grad;
                }
            }

            double updateNorm = 0.0;
            for (int p = 0; p < paramCount; p++)
            {
                double update = learningRate * gradients[p] / n;
                parameters[p] -= update;
                updateNorm += update * update;
            }

            if (System.Math.Sqrt(updateNorm) < tolerance)
                break;
        }

        return parameters;
    }

    /// <summary>
    /// Fits a polynomial of specified degree to data using gradient descent.
    /// </summary>
    /// <param name="x">The independent variable data.</param>
    /// <param name="y">The observed dependent variable data.</param>
    /// <param name="degree">The polynomial degree.</param>
    /// <param name="maxIterations">Maximum gradient descent iterations.</param>
    /// <param name="learningRate">The learning rate.</param>
    /// <returns>The polynomial coefficients [a0, a1, ..., a_degree].</returns>
    public static double[] PolynomialFit(
        double[] x,
        double[] y,
        int degree,
        int maxIterations = 500,
        double learningRate = 0.0001)
    {
        if (x is null) throw new ArgumentNullException(nameof(x));
        if (y is null) throw new ArgumentNullException(nameof(y));
        if (x.Length != y.Length)
            throw new ArgumentException("x and y arrays must have the same length.");
        if (degree < 1)
            throw new ArgumentOutOfRangeException(nameof(degree), "Degree must be at least 1.");

        double[] initialParams = new double[degree + 1];
        Func<double, double[], double> polyModel = (xi, p) =>
        {
            double result = 0.0;
            double xPow = 1.0;
            for (int d = 0; d < p.Length; d++)
            {
                result += p[d] * xPow;
                xPow *= xi;
            }
            return result;
        };

        return CurveFit(polyModel, x, y, initialParams, maxIterations, learningRate);
    }

    /// <summary>
    /// Computes the sum of squared errors for a model with given parameters.
    /// </summary>
    /// <param name="model">The model function.</param>
    /// <param name="x">The independent variable data.</param>
    /// <param name="y">The observed data.</param>
    /// <param name="parameters">The model parameters.</param>
    /// <returns>The sum of squared errors.</returns>
    public static double ComputeSSE(
        Func<double, double[], double> model,
        double[] x,
        double[] y,
        double[] parameters)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        if (x is null) throw new ArgumentNullException(nameof(x));
        if (y is null) throw new ArgumentNullException(nameof(y));
        if (parameters is null) throw new ArgumentNullException(nameof(parameters));

        double sse = 0.0;
        for (int i = 0; i < x.Length; i++)
        {
            double predicted = EvalModel(model, x[i], parameters);
            double error = predicted - y[i];
            sse += error * error;
        }
        return sse;
    }

    /// <summary>
    /// Computes R² (coefficient of determination) for a model.
    /// </summary>
    /// <param name="model">The model function.</param>
    /// <param name="x">The independent variable data.</param>
    /// <param name="y">The observed data.</param>
    /// <param name="parameters">The model parameters.</param>
    /// <returns>The R² score.</returns>
    public static double ComputeR2(
        Func<double, double[], double> model,
        double[] x,
        double[] y,
        double[] parameters)
    {
        if (y is null || y.Length == 0) return 0.0;

        double mean = 0.0;
        for (int i = 0; i < y.Length; i++) mean += y[i];
        mean /= y.Length;

        double ssTot = 0.0;
        double ssRes = 0.0;
        for (int i = 0; i < y.Length; i++)
        {
            double predicted = EvalModel(model, x[i], parameters);
            double totDiff = y[i] - mean;
            ssTot += totDiff * totDiff;
            double resDiff = y[i] - predicted;
            ssRes += resDiff * resDiff;
        }

        return ssTot < 1e-15 ? 1.0 : 1.0 - (ssRes / ssTot);
    }

    private static double EvalModel(Func<double, double[], double> model, double x, double[] parameters)
    {
        return model(x, parameters);
    }

    private static double EvalModelSimple(Func<double, double[], double> model, double x, double param, int paramIndex, double[] allParams)
    {
        double[] modified = new double[allParams.Length];
        Array.Copy(allParams, modified, allParams.Length);
        modified[paramIndex] = param;
        return model(x, modified);
    }

    private static double ComputePartialDerivative(Func<double, double[], double> model, double x, double[] parameters, int paramIndex)
    {
        double h = 1e-8;
        double[] paramsPlus = new double[parameters.Length];
        double[] paramsMinus = new double[parameters.Length];
        Array.Copy(parameters, paramsPlus, parameters.Length);
        Array.Copy(parameters, paramsMinus, parameters.Length);

        paramsPlus[paramIndex] += h;
        paramsMinus[paramIndex] -= h;

        double valPlus = model(x, paramsPlus);
        double valMinus = model(x, paramsMinus);

        return (valPlus - valMinus) / (2.0 * h);
    }
}
