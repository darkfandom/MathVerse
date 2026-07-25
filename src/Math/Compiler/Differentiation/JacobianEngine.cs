namespace MathVerse.Math.Compiler.Differentiation;

using System;
using System.Collections.Generic;

/// <summary>Computes the Jacobian matrix of a multivariate function using forward-mode automatic differentiation.</summary>
public sealed class JacobianEngine
{
    private readonly ForwardModeAD _forwardAD = new();

    /// <summary>Computes the Jacobian matrix of f: R^n → R^m at the given point.</summary>
    /// <param name="f">The function to differentiate. Takes a DualNumber array and returns a DualNumber array.</param>
    /// <param name="point">The evaluation point (n-dimensional).</param>
    /// <param name="outputDimension">The output dimension m.</param>
    /// <returns>An m×n Jacobian matrix where J[i,j] = ∂f_i/∂x_j.</returns>
    public double[,] Compute(
        Func<DualNumber[], DualNumber[]> f,
        double[] point,
        int outputDimension)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));
        if (outputDimension < 1) throw new ArgumentOutOfRangeException(nameof(outputDimension));

        int n = point.Length;
        var jacobian = new double[outputDimension, n];

        for (int j = 0; j < n; j++)
        {
            var inputs = new DualNumber[n];
            for (int i = 0; i < n; i++)
                inputs[i] = DualNumber.FromValue(point[i]);

            inputs[j] = DualNumber.Create(point[j], 1.0);

            DualNumber[] outputs = f(inputs);

            for (int i = 0; i < Math.Min(outputDimension, outputs.Length); i++)
                jacobian[i, j] = outputs[i].Dual;
        }

        return jacobian;
    }

    /// <summary>Computes the Jacobian using expression AST nodes.</summary>
    public double[,] ComputeFromExpressions(
        IReadOnlyList<Expressions.ExpressionNode> functions,
        string[] variableNames,
        double[] point)
    {
        if (functions is null) throw new ArgumentNullException(nameof(functions));
        if (variableNames is null) throw new ArgumentNullException(nameof(variableNames));
        if (point is null) throw new ArgumentNullException(nameof(point));

        int m = functions.Count;
        int n = variableNames.Length;
        var jacobian = new double[m, n];

        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < m; i++)
            {
                var result = _forwardAD.DifferentiateExpression(functions[i], variableNames[j], point[j]);
                jacobian[i, j] = result.Derivative;
            }
        }

        return jacobian;
    }

    /// <summary>Computes the Jacobian at a single point for a single output function with respect to all variables.</summary>
    public double[] ComputeGradient(
        Func<DualNumber[], DualNumber> f,
        double[] point)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));

        return _forwardAD.Gradient(f, point);
    }

    /// <summary>Verifies a Jacobian computation against numerical finite differences.</summary>
    public (double[,] Analytical, double[,] Numerical, double MaxError) VerifyJacobian(
        Func<DualNumber[], DualNumber[]> f,
        double[] point,
        int outputDimension,
        double epsilon = 1e-7)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        if (point is null) throw new ArgumentNullException(nameof(point));

        int n = point.Length;
        var analytical = Compute(f, point, outputDimension);
        var numerical = new double[outputDimension, n];

        for (int j = 0; j < n; j++)
        {
            double[] pointForward = (double[])point.Clone();
            double[] pointBackward = (double[])point.Clone();
            pointForward[j] += epsilon;
            pointBackward[j] -= epsilon;

            var fwdResult = EvaluateVector(f, pointForward);
            var bwdResult = EvaluateVector(f, pointBackward);

            for (int i = 0; i < outputDimension; i++)
                numerical[i, j] = (fwdResult[i] - bwdResult[i]) / (2.0 * epsilon);
        }

        double maxError = 0;
        for (int i = 0; i < outputDimension; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double error = Math.Abs(analytical[i, j] - numerical[i, j]);
                if (error > maxError) maxError = error;
            }
        }

        return (analytical, numerical, maxError);
    }

    private static double[] EvaluateVector(Func<DualNumber[], DualNumber[]> f, double[] point)
    {
        var inputs = new DualNumber[point.Length];
        for (int i = 0; i < point.Length; i++)
            inputs[i] = DualNumber.FromValue(point[i]);

        DualNumber[] outputs = f(inputs);
        var result = new double[outputs.Length];
        for (int i = 0; i < outputs.Length; i++)
            result[i] = outputs[i].Real;
        return result;
    }
}
