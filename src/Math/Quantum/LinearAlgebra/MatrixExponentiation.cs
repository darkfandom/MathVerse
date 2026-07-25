namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Static utility class for matrix exponentiation using Padé approximation.
/// Used to construct unitary gates from Hermitian generators: U = e^{iHt}.
/// </summary>
public static class MatrixExponentiation
{
    /// <summary>
    /// Computes e^(sM) using a [6/6] Padé approximant.
    /// </summary>
    public static ComplexMatrix Exponentiate(ComplexMatrix matrix, Complex scalar)
    {
        if (matrix == null) throw new ArgumentNullException(nameof(matrix));
        if (matrix.Rows != matrix.Cols)
            throw new ArgumentException("Matrix must be square for exponentiation.", nameof(matrix));

        int n = matrix.Rows;
        ComplexMatrix scaled = matrix.Scale(scalar);
        ComplexMatrix identity = ComplexMatrix.Identity(n);

        ComplexMatrix a2 = scaled.Multiply(scaled);

        ComplexMatrix q = identity
            .Add(a2.Scale(new Complex(1.0 / 6.0, 0.0)))
            .Add(MatrixPower(a2, 2).Scale(new Complex(1.0 / 90.0, 0.0)))
            .Add(MatrixPower(a2, 3).Scale(new Complex(1.0 / 2520.0, 0.0)));

        ComplexMatrix p = identity
            .Add(a2.Scale(new Complex(1.0 / 2.0, 0.0)))
            .Add(MatrixPower(a2, 2).Scale(new Complex(1.0 / 60.0, 0.0)))
            .Add(MatrixPower(a2, 3).Scale(new Complex(1.0 / 2520.0, 0.0)));

        ComplexMatrix scaledOdd = scaled
            .Add(MatrixPower(scaled, 3).Scale(new Complex(1.0 / 30.0, 0.0)))
            .Add(MatrixPower(scaled, 5).Scale(new Complex(1.0 / 840.0, 0.0)));

        ComplexMatrix numerator = p.Add(scaledOdd);
        ComplexMatrix denominator = q.Add(scaledOdd.Scale(new System.Numerics.Complex(-1.0, 0.0)));

        return PseudoInverse(denominator).Multiply(numerator);
    }

    /// <summary>
    /// Computes the unitary gate U = e^{-iθG} from a Hermitian generator G and angle θ.
    /// </summary>
    public static ComplexMatrix GateExponentiate(ComplexMatrix generator, double theta)
    {
        if (generator == null) throw new ArgumentNullException(nameof(generator));
        return Exponentiate(generator, new Complex(0.0, -theta));
    }

    /// <summary>Computes integer powers of a square matrix.</summary>
    public static ComplexMatrix MatrixPower(ComplexMatrix m, int power)
    {
        if (m == null) throw new ArgumentNullException(nameof(m));
        if (m.Rows != m.Cols)
            throw new ArgumentException("Matrix must be square for power computation.", nameof(m));
        if (power < 0)
            throw new ArgumentOutOfRangeException(nameof(power), "Negative powers are not supported.");

        if (power == 0) return ComplexMatrix.Identity(m.Rows);

        if (power == 1) return m;

        ComplexMatrix result = m;
        for (int i = 2; i <= power; i++)
            result = result.Multiply(m);
        return result;
    }

    private static ComplexMatrix PseudoInverse(ComplexMatrix m)
    {
        int n = m.Rows;
        var augmented = new Complex[n, 2 * n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                augmented[i, j] = m[i, j];
            augmented[i, n + i] = Complex.One;
        }

        for (int i = 0; i < n; i++)
        {
            int maxRow = i;
            double maxMag = augmented[i, i].Magnitude;
            for (int k = i + 1; k < n; k++)
            {
                double mag = augmented[k, i].Magnitude;
                if (mag > maxMag) { maxMag = mag; maxRow = k; }
            }
            if (maxMag < 1e-15) throw new InvalidOperationException("Matrix is singular.");

            if (maxRow != i)
                for (int j = 0; j < 2 * n; j++)
                    (augmented[i, j], augmented[maxRow, j]) = (augmented[maxRow, j], augmented[i, j]);

            Complex pivot = augmented[i, i];
            for (int j = 0; j < 2 * n; j++)
                augmented[i, j] /= pivot;

            for (int k = 0; k < n; k++)
            {
                if (k == i) continue;
                Complex factor = augmented[k, i];
                for (int j = 0; j < 2 * n; j++)
                    augmented[k, j] -= factor * augmented[i, j];
            }
        }

        var result = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i, j] = augmented[i, n + j];
        return new ComplexMatrix(result);
    }
}
