namespace MathVerse.Math.Numerics.Sparse;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed record SparseLUDecomposition
{
    public SparseMatrix L { get; }
    public SparseMatrix U { get; }
    public ImmutableArray<int> Pivot { get; }
    public int Sign { get; }

    private SparseLUDecomposition(SparseMatrix l, SparseMatrix u, ImmutableArray<int> pivot, int sign)
    {
        L = l; U = u; Pivot = pivot; Sign = sign;
    }

    public static SparseLUDecomposition Compute(SparseMatrix a)
    {
        if (!a.IsSquare) throw new ArgumentException("Matrix must be square");
        var dense = a.ToDense();
        var lu = LUDecomposition.Compute(dense);
        return new SparseLUDecomposition(
            SparseMatrix.FromDense(lu.L),
            SparseMatrix.FromDense(lu.U),
            lu.Pivot.ToImmutableArray(),
            lu.Sign);
    }

    public Vector Solve(Vector b)
    {
        var denseL = L.ToDense();
        var denseU = U.ToDense();
        int n = denseL.Rows;
        var y = new double[n];
        var x = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = b[Pivot[i]];
            for (int j = 0; j < i; j++) sum -= denseL[i, j] * y[j];
            y[i] = sum;
        }

        for (int i = n - 1; i >= 0; i--)
        {
            double sum = y[i];
            for (int j = i + 1; j < n; j++) sum -= denseU[i, j] * x[j];
            x[i] = sum / denseU[i, i];
        }

        return new Vector(x.ToImmutableArray());
    }
}