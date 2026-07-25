namespace MathVerse.Math.Quantum.Variational;

using System;

/// <summary>
/// Provides gradient and Hessian computation for quantum cost functions using the
/// parameter-shift rule, as well as gradient descent optimization.
/// </summary>
public static class ParameterShiftGradient
{
    /// <summary>
    /// Computes the gradient of a cost function using the parameter-shift rule.
    /// For each parameter θᵢ: ∂f/∂θᵢ = [f(θᵢ + π/2) − f(θᵢ − π/2)] / 2.
    /// </summary>
    /// <param name="costFunction">The cost function to differentiate.</param>
    /// <param name="parameters">The current parameter values.</param>
    /// <param name="shift">The shift angle (default π/2).</param>
    /// <returns>The gradient vector.</returns>
    public static double[] ComputeGradient(Func<double[], double> costFunction, double[] parameters, double shift = System.Math.PI / 2.0)
    {
        if (costFunction == null) throw new ArgumentNullException(nameof(costFunction));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        int n = parameters.Length;
        var gradient = new double[n];

        for (int i = 0; i < n; i++)
        {
            var forward = (double[])parameters.Clone();
            var backward = (double[])parameters.Clone();
            forward[i] += shift;
            backward[i] -= shift;
            gradient[i] = (costFunction(forward) - costFunction(backward)) / (2.0 * System.Math.Sin(shift));
        }

        return gradient;
    }

    /// <summary>
    /// Computes the Hessian matrix of a cost function using second-order parameter-shift rules.
    /// </summary>
    /// <param name="costFunction">The cost function to differentiate.</param>
    /// <param name="parameters">The current parameter values.</param>
    /// <param name="shift">The shift angle (default π/2).</param>
    /// <returns>The Hessian matrix.</returns>
    public static double[,] ComputeHessian(Func<double[], double> costFunction, double[] parameters, double shift = System.Math.PI / 2.0)
    {
        if (costFunction == null) throw new ArgumentNullException(nameof(costFunction));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        int n = parameters.Length;
        var hessian = new double[n, n];
        double f0 = costFunction(parameters);

        for (int i = 0; i < n; i++)
        {
            var fwd = (double[])parameters.Clone();
            var bwd = (double[])parameters.Clone();
            fwd[i] += shift;
            bwd[i] -= shift;
            double fP = costFunction(fwd);
            double fM = costFunction(bwd);
            hessian[i, i] = (fP - 2.0 * f0 + fM) / (System.Math.Sin(shift) * System.Math.Sin(shift));

            for (int j = i + 1; j < n; j++)
            {
                var pp = (double[])parameters.Clone();
                var mm = (double[])parameters.Clone();
                var pm = (double[])parameters.Clone();
                var mp = (double[])parameters.Clone();
                pp[i] += shift; pp[j] += shift;
                mm[i] -= shift; mm[j] -= shift;
                pm[i] += shift; pm[j] -= shift;
                mp[i] -= shift; mp[j] += shift;
                hessian[i, j] = (costFunction(pp) - costFunction(pm) - costFunction(mp) + costFunction(mm)) /
                                (4.0 * System.Math.Sin(shift) * System.Math.Sin(shift));
                hessian[j, i] = hessian[i, j];
            }
        }

        return hessian;
    }

    /// <summary>
    /// Performs gradient descent optimization on the cost function.
    /// </summary>
    /// <param name="costFunction">The cost function to minimize.</param>
    /// <param name="initialParams">The initial parameter values.</param>
    /// <param name="iterations">The number of gradient descent iterations.</param>
    /// <param name="learningRate">The learning rate (step size).</param>
    /// <returns>The optimized parameters.</returns>
    public static double[] GradientDescent(Func<double[], double> costFunction, double[] initialParams, int iterations, double learningRate = 0.1)
    {
        if (costFunction == null) throw new ArgumentNullException(nameof(costFunction));
        if (initialParams == null) throw new ArgumentNullException(nameof(initialParams));
        if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations));

        var current = (double[])initialParams.Clone();

        for (int iter = 0; iter < iterations; iter++)
        {
            var gradient = ComputeGradient(costFunction, current);
            for (int i = 0; i < current.Length; i++)
                current[i] -= learningRate * gradient[i];
        }

        return current;
    }
}
