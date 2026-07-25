namespace MathVerse.Math.Numerics.Sparse;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed record SparseCholeskyDecomposition
{
    public SparseMatrix L { get; }
    public bool IsPositiveDefinite { get; }

    private SparseCholeskyDecomposition(SparseMatrix l, bool isPD)
    {
        L = l; IsPositiveDefinite = isPD;
    }

    public static SparseCholeskyDecomposition Compute(SparseMatrix a)
    {
        if (!a.IsSquare) throw new ArgumentException("Matrix must be square");
        var dense = a.ToDense();
        var chol = CholeskyDecomposition.Compute(dense);
        return new SparseCholeskyDecomposition(
            SparseMatrix.FromDense(chol.L),
            chol.IsPositiveDefinite);
    }

    public Vector Solve(Vector b)
    {
        var denseL = L.ToDense();
        int n = denseL.Rows;
        var y = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = b[i];
            for (int j = 0; j < i; j++) sum -= denseL[i, j] * y[j];
            y[i] = denseL[i, i] > 0 ? sum / denseL[i, i] : 0;
        }

        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = y[i];
            for (int j = i + 1; j < n; j++) sum -= denseL[j, i] * x[j];
            x[i] = denseL[i, i] > 0 ? sum / denseL[i, i] : 0;
        }
        return new Vector(x.ToImmutableArray());
    }
}