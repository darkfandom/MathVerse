namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Static class for eigenvalue decomposition of Hermitian matrices using the Jacobi eigenvalue algorithm.
/// </summary>
public static class EigenSolver
{
    private const int MaxIterations = 1000;

    /// <summary>
    /// Computes eigenvalues and eigenvectors of a Hermitian matrix using the Jacobi algorithm.
    /// </summary>
    public static (double[] eigenvalues, ComplexMatrix eigenvectors) Decompose(ComplexMatrix hermitian)
    {
        if (hermitian == null) throw new ArgumentNullException(nameof(hermitian));
        if (hermitian.Rows != hermitian.Cols)
            throw new ArgumentException("Matrix must be square.", nameof(hermitian));

        int n = hermitian.Rows;

        var a = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                a[i, j] = hermitian[i, j];

        var v = new Complex[n, n];
        for (int i = 0; i < n; i++)
            v[i, i] = Complex.One;

        for (int iter = 0; iter < MaxIterations; iter++)
        {
            double offDiagNorm = 0.0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    offDiagNorm += a[i, j].Magnitude * a[i, j].Magnitude;
            offDiagNorm = System.Math.Sqrt(2.0 * offDiagNorm);

            if (offDiagNorm < 1e-14) break;

            int p = 0, q = 1;
            double maxVal = a[0, 1].Magnitude;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                {
                    double val = a[i, j].Magnitude;
                    if (val > maxVal) { maxVal = val; p = i; q = j; }
                }

            JacobiRotation(a, v, p, q, n);
        }

        double[] eigenvalues = new double[n];
        for (int i = 0; i < n; i++)
            eigenvalues[i] = a[i, i].Real;

        var eigenvectors = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                eigenvectors[i, j] = v[i, j];

        return (eigenvalues, new ComplexMatrix(eigenvectors));
    }

    /// <summary>Computes only the eigenvalues of a Hermitian matrix.</summary>
    public static double[] Eigenvalues(ComplexMatrix hermitian)
    {
        var (eigenvalues, _) = Decompose(hermitian);
        return eigenvalues;
    }

    /// <summary>Computes only the eigenvectors of a Hermitian matrix.</summary>
    public static ComplexMatrix Eigenvectors(ComplexMatrix hermitian)
    {
        var (_, eigenvectors) = Decompose(hermitian);
        return eigenvectors;
    }

    private static void JacobiRotation(Complex[,] a, Complex[,] v, int p, int q, int n)
    {
        if (a[p, q].Magnitude < 1e-15) return;

        double app = a[p, p].Real;
        double aqq = a[q, q].Real;
        double apq = a[p, q].Real;

        double tau = (aqq - app) / (2.0 * apq);
        double t;
        if (tau >= 0)
            t = 1.0 / (tau + System.Math.Sqrt(1.0 + tau * tau));
        else
            t = -1.0 / (-tau + System.Math.Sqrt(1.0 + tau * tau));

        double c = 1.0 / System.Math.Sqrt(1.0 + t * t);
        double s = t * c;

        for (int i = 0; i < n; i++)
        {
            Complex aip = a[i, p];
            Complex aiq = a[i, q];
            a[i, p] = c * aip - s * aiq;
            a[i, q] = s * aip + c * aiq;
        }

        for (int j = 0; j < n; j++)
        {
            Complex apj = a[p, j];
            Complex aqj = a[q, j];
            a[p, j] = c * apj - s * aqj;
            a[q, j] = s * apj + c * aqj;
        }

        for (int i = 0; i < n; i++)
        {
            Complex vip = v[i, p];
            Complex viq = v[i, q];
            v[i, p] = c * vip - s * viq;
            v[i, q] = s * vip + c * viq;
        }
    }
}
