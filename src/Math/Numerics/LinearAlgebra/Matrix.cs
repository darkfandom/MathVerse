namespace MathVerse.Math.Numerics.LinearAlgebra;

using System.Collections.Immutable;
using System.Runtime.CompilerServices;

public sealed record Matrix
{
    internal readonly ImmutableArray<ImmutableArray<double>> _rows;

    public Matrix(ImmutableArray<ImmutableArray<double>> rows)
    {
        if (rows.Length > 0)
        {
            int cols = rows[0].Length;
            if (rows.Any(row => row.Length != cols))
                throw new ArgumentException("All rows must have the same number of columns");
        }
        _rows = rows;
    }

    public Matrix(double[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        var rowData = new ImmutableArray<double>[rows];
        for (int i = 0; i < rows; i++)
        {
            var row = new double[cols];
            for (int j = 0; j < cols; j++) row[j] = array[i, j];
            rowData[i] = row.ToImmutableArray();
        }
        _rows = rowData.ToImmutableArray();
    }

    public Matrix(double[][] jagged)
    {
        var rowData = new ImmutableArray<double>[jagged.Length];
        for (int i = 0; i < jagged.Length; i++) rowData[i] = jagged[i].ToImmutableArray();
        _rows = rowData.ToImmutableArray();
    }

    public int Rows => _rows.Length;

    public int Cols => Rows > 0 ? _rows[0].Length : 0;

    public bool IsSquare => Rows == Cols;

    public bool IsEmpty => Rows == 0 || Cols == 0;

    public double this[int row, int col]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _rows[row][col];
    }

    public ImmutableArray<double> GetRow(int row) => _rows[row];

    public ImmutableArray<double> GetColumn(int col)
    {
        var result = new double[Rows];
        for (int i = 0; i < Rows; i++) result[i] = _rows[i][col];
        return result.ToImmutableArray();
    }

    public static Matrix Zero(int rows, int cols)
    {
        var data = Enumerable.Range(0, rows).Select(_ =>
            Enumerable.Repeat(0.0, cols).ToImmutableArray()
        ).ToImmutableArray();
        return new Matrix(data);
    }

    public static Matrix Identity(int size)
    {
        var data = new ImmutableArray<double>[size];
        for (int i = 0; i < size; i++)
        {
            var row = new double[size];
            row[i] = 1.0;
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public static Matrix Diagonal(Vector diagonal)
    {
        int n = diagonal.Size;
        var data = new ImmutableArray<double>[n];
        for (int i = 0; i < n; i++)
        {
            var row = new double[n];
            row[i] = diagonal[i];
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public static Matrix FromRows(params Vector[] rows)
    {
        if (rows.Length == 0) return new Matrix(ImmutableArray<ImmutableArray<double>>.Empty);
        int cols = rows[0].Size;
        var data = rows.Select(r => r.ToImmutableArray()).ToImmutableArray();
        return new Matrix(data);
    }

    public static Matrix FromColumns(params Vector[] cols)
    {
        if (cols.Length == 0) return new Matrix(ImmutableArray<ImmutableArray<double>>.Empty);
        int rows = cols[0].Size;
        var data = new ImmutableArray<double>[rows];
        for (int i = 0; i < rows; i++)
        {
            var row = new double[cols.Length];
            for (int j = 0; j < cols.Length; j++) row[j] = cols[j][i];
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public Matrix Transpose()
    {
        if (IsEmpty) return this;
        var data = new ImmutableArray<double>[Cols];
        for (int j = 0; j < Cols; j++)
        {
            var col = new double[Rows];
            for (int i = 0; i < Rows; i++) col[i] = _rows[i][j];
            data[j] = col.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public Matrix Add(Matrix other)
    {
        if (Rows != other.Rows || Cols != other.Cols)
            throw new ArgumentException("Matrix dimensions must match");
        var data = new ImmutableArray<double>[Rows];
        for (int i = 0; i < Rows; i++)
        {
            var row = new double[Cols];
            for (int j = 0; j < Cols; j++) row[j] = _rows[i][j] + other._rows[i][j];
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public Matrix Subtract(Matrix other)
    {
        if (Rows != other.Rows || Cols != other.Cols)
            throw new ArgumentException("Matrix dimensions must match");
        var data = new ImmutableArray<double>[Rows];
        for (int i = 0; i < Rows; i++)
        {
            var row = new double[Cols];
            for (int j = 0; j < Cols; j++) row[j] = _rows[i][j] - other._rows[i][j];
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public Matrix Multiply(Matrix other)
    {
        if (Cols != other.Rows)
            throw new ArgumentException($"Cannot multiply {Rows}x{Cols} matrix with {other.Rows}x{other.Cols} matrix");
        var data = new ImmutableArray<double>[Rows];
        for (int i = 0; i < Rows; i++)
        {
            var row = new double[other.Cols];
            for (int j = 0; j < other.Cols; j++)
            {
                double sum = 0;
                for (int k = 0; k < Cols; k++) sum += _rows[i][k] * other._rows[k][j];
                row[j] = sum;
            }
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public Vector Multiply(Vector v)
    {
        if (Cols != v.Size) throw new ArgumentException("Matrix columns must match vector size");
        var result = new double[Rows];
        for (int i = 0; i < Rows; i++)
        {
            double sum = 0;
            for (int j = 0; j < Cols; j++) sum += _rows[i][j] * v[j];
            result[i] = sum;
        }
        return new Vector(result.ToImmutableArray());
    }

    public Matrix Scale(double scalar)
    {
        var data = new ImmutableArray<double>[Rows];
        for (int i = 0; i < Rows; i++)
        {
            var row = new double[Cols];
            for (int j = 0; j < Cols; j++) row[j] = _rows[i][j] * scalar;
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public Matrix Negate() => Scale(-1.0);

    public Matrix ElementWiseMultiply(Matrix other)
    {
        if (Rows != other.Rows || Cols != other.Cols)
            throw new ArgumentException("Matrix dimensions must match");
        var data = new ImmutableArray<double>[Rows];
        for (int i = 0; i < Rows; i++)
        {
            var row = new double[Cols];
            for (int j = 0; j < Cols; j++) row[j] = _rows[i][j] * other._rows[i][j];
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public Matrix ElementWiseDivide(Matrix other)
    {
        if (Rows != other.Rows || Cols != other.Cols)
            throw new ArgumentException("Matrix dimensions must match");
        var data = new ImmutableArray<double>[Rows];
        for (int i = 0; i < Rows; i++)
        {
            var row = new double[Cols];
            for (int j = 0; j < Cols; j++) row[j] = _rows[i][j] / other._rows[i][j];
            data[i] = row.ToImmutableArray();
        }
        return new Matrix(data.ToImmutableArray());
    }

    public double Trace()
    {
        if (!IsSquare) throw new ArgumentException("Trace requires square matrix");
        double sum = 0;
        for (int i = 0; i < Rows; i++) sum += _rows[i][i];
        return sum;
    }

    public double Determinant()
    {
        if (!IsSquare) throw new ArgumentException("Determinant requires square matrix");
        if (Rows == 1) return _rows[0][0];
        if (Rows == 2) return _rows[0][0] * _rows[1][1] - _rows[0][1] * _rows[1][0];

        var lu = LUDecomposition.Compute(this);
        double det = lu.Sign;
        for (int i = 0; i < Rows; i++) det *= lu.U._rows[i][i];
        return det;
    }

    public Matrix Inverse()
    {
        if (!IsSquare) throw new ArgumentException("Inverse requires square matrix");
        return LUDecomposition.Compute(this).Solve(Matrix.Identity(Rows));
    }

    public double NormFrobenius()
    {
        double sum = 0;
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
                sum += _rows[i][j] * _rows[i][j];
        return System.Math.Sqrt(sum);
    }

    public double Norm1()
    {
        double maxColSum = 0;
        for (int j = 0; j < Cols; j++)
        {
            double colSum = 0;
            for (int i = 0; i < Rows; i++) colSum += System.Math.Abs(_rows[i][j]);
            if (colSum > maxColSum) maxColSum = colSum;
        }
        return maxColSum;
    }

    public double NormInf()
    {
        double maxRowSum = 0;
        for (int i = 0; i < Rows; i++)
        {
            double rowSum = 0;
            for (int j = 0; j < Cols; j++) rowSum += System.Math.Abs(_rows[i][j]);
            if (rowSum > maxRowSum) maxRowSum = rowSum;
        }
        return maxRowSum;
    }

    public double[,] ToArray2D()
    {
        var array = new double[Rows, Cols];
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
                array[i, j] = _rows[i][j];
        return array;
    }

    public double[][] ToJaggedArray()
    {
        var result = new double[Rows][];
        for (int i = 0; i < Rows; i++) result[i] = _rows[i].ToArray();
        return result;
    }

    public ImmutableArray<ImmutableArray<double>> ToImmutableRows() => _rows;

    public override string ToString()
    {
        if (IsEmpty) return "Matrix[]";
        var rowsStr = _rows.Select(row => "[" + string.Join(", ", row) + "]");
        return "Matrix[" + string.Join(", ", rowsStr) + "]";
    }

    public static Matrix operator +(Matrix a, Matrix b) => a.Add(b);

    public static Matrix operator -(Matrix a, Matrix b) => a.Subtract(b);

    public static Matrix operator *(Matrix a, Matrix b) => a.Multiply(b);

    public static Vector operator *(Matrix a, Vector v) => a.Multiply(v);

    public static Matrix operator *(Matrix a, double scalar) => a.Scale(scalar);

    public static Matrix operator *(double scalar, Matrix a) => a.Scale(scalar);

    public static Matrix operator -(Matrix a) => a.Negate();
}

public sealed record LUDecomposition
{
    public Matrix L { get; }
    public Matrix U { get; }
    public int[] Pivot { get; }
    public int Sign { get; }

    private LUDecomposition(Matrix l, Matrix u, int[] pivot, int sign)
    {
        L = l; U = u; Pivot = pivot; Sign = sign;
    }

    public static LUDecomposition Compute(Matrix a)
    {
        int n = a.Rows;
        if (!a.IsSquare) throw new ArgumentException("LU decomposition requires square matrix");

        var lData = new double[n][];
        var uData = new double[n][];
        var pivot = Enumerable.Range(0, n).ToArray();
        int sign = 1;

        for (int i = 0; i < n; i++)
        {
            lData[i] = new double[n];
            uData[i] = new double[n];
            lData[i][i] = 1.0;
        }

        var aData = a.ToJaggedArray();

        for (int k = 0; k < n; k++)
        {
            int maxRow = k;
            double maxVal = System.Math.Abs(aData[k][k]);
            for (int i = k + 1; i < n; i++)
            {
                double val = System.Math.Abs(aData[i][k]);
                if (val > maxVal) { maxVal = val; maxRow = i; }
            }

            if (maxVal < 1e-15) throw new InvalidOperationException("Matrix is singular");

            if (maxRow != k)
            {
                (aData[k], aData[maxRow]) = (aData[maxRow], aData[k]);
                (pivot[k], pivot[maxRow]) = (pivot[maxRow], pivot[k]);
                sign = -sign;
            }

            for (int i = k + 1; i < n; i++)
            {
                double factor = aData[i][k] / aData[k][k];
                lData[i][k] = factor;
                for (int j = k; j < n; j++)
                    aData[i][j] -= factor * aData[k][j];
            }
        }

        for (int i = 0; i < n; i++)
            for (int j = i; j < n; j++)
                uData[i][j] = aData[i][j];

        var lRows = lData.Select(r => r.ToImmutableArray()).ToImmutableArray();
        var uRows = uData.Select(r => r.ToImmutableArray()).ToImmutableArray();

        return new LUDecomposition(new Matrix(lRows), new Matrix(uRows), pivot, sign);
    }

    public Vector Solve(Vector b)
    {
        int n = L.Rows;
        var y = new double[n];
        var x = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = b[Pivot[i]];
            for (int j = 0; j < i; j++) sum -= L[i, j] * y[j];
            y[i] = sum;
        }

        for (int i = n - 1; i >= 0; i--)
        {
            double sum = y[i];
            for (int j = i + 1; j < n; j++) sum -= U[i, j] * x[j];
            x[i] = sum / U[i, i];
        }

        return new Vector(x.ToImmutableArray());
    }

    public Matrix Solve(Matrix b)
    {
        var cols = new Vector[b.Cols];
        for (int j = 0; j < b.Cols; j++) cols[j] = Solve(b.GetColumn(j));
        return Matrix.FromColumns(cols);
    }

    public double Determinant()
    {
        double det = Sign;
        for (int i = 0; i < U.Rows; i++) det *= U[i, i];
        return det;
    }
}

public sealed record QRDecomposition
{
    public Matrix Q { get; }
    public Matrix R { get; }

    private QRDecomposition(Matrix q, Matrix r) { Q = q; R = r; }

    public static QRDecomposition Compute(Matrix a)
    {
        int m = a.Rows, n = a.Cols;
        var qData = new double[m][];
        var rData = new double[n][];

        for (int i = 0; i < m; i++) qData[i] = new double[n];
        for (int i = 0; i < n; i++) rData[i] = new double[n];

        var aCols = new double[n][];
        for (int j = 0; j < n; j++)
        {
            aCols[j] = new double[m];
            for (int i = 0; i < m; i++) aCols[j][i] = a[i, j];
        }

        for (int j = 0; j < n; j++)
        {
            var v = aCols[j];
            for (int i = 0; i < j; i++)
            {
                double dot = 0;
                for (int k = 0; k < m; k++) dot += qData[k][i] * v[k];
                rData[i][j] = dot;
                for (int k = 0; k < m; k++) v[k] -= dot * qData[k][i];
            }

            double norm = 0;
            for (int k = 0; k < m; k++) norm += v[k] * v[k];
            norm = System.Math.Sqrt(norm);
            rData[j][j] = norm;

            if (norm > 1e-15)
                for (int k = 0; k < m; k++) qData[k][j] = v[k] / norm;
            else
                for (int k = 0; k < m; k++) qData[k][j] = 0;
        }

        var qRows = qData.Select(r => r.ToImmutableArray()).ToImmutableArray();
        var rRows = rData.Select(r => r.ToImmutableArray()).ToImmutableArray();

        return new QRDecomposition(new Matrix(qRows), new Matrix(rRows));
    }

    public Vector Solve(Vector b)
    {
        var qtb = Q.Transpose().Multiply(b);
        int n = R.Rows;
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = qtb[i];
            for (int j = i + 1; j < n; j++) sum -= R[i, j] * x[j];
            x[i] = sum / R[i, i];
        }
        return new Vector(x.ToImmutableArray());
    }
}

public sealed record SVDDecomposition
{
    public Matrix U { get; }
    public Vector S { get; }
    public Matrix Vt { get; }

    private SVDDecomposition(Matrix u, Vector s, Matrix vt) { U = u; S = s; Vt = vt; }

    public static SVDDecomposition Compute(Matrix a)
    {
        int m = a.Rows, n = a.Cols;
        int minMN = System.Math.Min(m, n);

        var ata = a.Transpose().Multiply(a);
        var (eigVals, eigVecs) = EigenDecomposition.ComputeSymmetric(ata);

        var sData = new double[minMN];
        var vtData = new double[minMN][];
        for (int i = 0; i < minMN; i++)
        {
            sData[i] = System.Math.Sqrt(System.Math.Max(0, eigVals[i]));
            vtData[i] = eigVecs.GetColumn(i).ToArray();
        }

        var uData = new double[m][];
        for (int i = 0; i < m; i++) uData[i] = new double[minMN];

        for (int i = 0; i < minMN; i++)
        {
            if (sData[i] > 1e-15)
            {
                var col = a.Multiply(eigVecs.GetColumn(i)).Scale(1.0 / sData[i]);
                for (int k = 0; k < m; k++) uData[k][i] = col[k];
            }
            else
            {
                for (int k = 0; k < m; k++) uData[k][i] = 0;
            }
        }

        var uRows = uData.Select(r => r.ToImmutableArray()).ToImmutableArray();
        var vtRows = vtData.Select(r => r.ToImmutableArray()).ToImmutableArray();

        return new SVDDecomposition(new Matrix(uRows), new Vector(sData.ToImmutableArray()), new Matrix(vtRows));
    }

    public Vector Solve(Vector b)
    {
        var utb = U.Transpose().Multiply(b);
        var y = new double[S.Size];
        for (int i = 0; i < S.Size; i++) y[i] = S[i] > 1e-15 ? utb[i] / S[i] : 0;
        return Vt.Transpose().Multiply(new Vector(y.ToImmutableArray()));
    }

    public double ConditionNumber() => S.Size > 0 ? S[0] / S[S.Size - 1] : double.PositiveInfinity;

    public int Rank(double tol = 1e-12) => S._values.Count(s => s > tol);
}

public static class EigenDecomposition
{
    public static (Vector Values, Matrix Vectors) ComputeSymmetric(Matrix a)
    {
        if (!a.IsSquare) throw new ArgumentException("Matrix must be square");
        int n = a.Rows;
        var v = a.ToJaggedArray();
        var d = new double[n];
        var e = new double[n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++) d[j] = v[i][j];
            for (int j = 0; j < n - 1; j++) e[j] = v[j][j + 1];
        }

        TQL2(n, d, e, v);

        var indices = Enumerable.Range(0, n).OrderByDescending(i => d[i]).ToArray();
        var sortedD = indices.Select(i => d[i]).ToArray();
        var sortedV = new double[n][];
        for (int i = 0; i < n; i++)
        {
            sortedV[i] = new double[n];
            for (int j = 0; j < n; j++) sortedV[i][j] = v[i][indices[j]];
        }

        return (new Vector(sortedD.ToImmutableArray()), new Matrix(sortedV));
    }

    private static void TQL2(int n, double[] d, double[] e, double[][] v)
    {
        for (int i = 1; i < n; i++) e[i - 1] = e[i];
        e[n - 1] = 0.0;

        for (int l = 0; l < n; l++)
        {
            int iter = 0;
            while (true)
            {
                int m;
                for (m = l; m < n - 1; m++)
                {
                    if (System.Math.Abs(e[m]) + System.Math.Abs(d[m]) + System.Math.Abs(d[m + 1]) == System.Math.Abs(d[m]) + System.Math.Abs(d[m + 1])) break;
                }
                if (m == l) break;

                if (iter++ == 30) throw new InvalidOperationException("Eigenvalue convergence failed");

                double g = (d[l + 1] - d[l]) / (2.0 * e[l]);
                double r = System.Math.Sqrt(g * g + 1.0);
                g = d[m] - d[l] + e[l] / (g + System.Math.Sign(g) * r);

                double s = 1.0, c = 1.0, p = 0.0;
                for (int i = m - 1; i >= l; i--)
                {
                    double f = s * e[i];
                    double b = c * e[i];
                    double r2 = System.Math.Sqrt(f * f + g * g);
                    e[i + 1] = r2;
                    if (r2 == 0) { d[i + 1] -= p; e[m] = 0; break; }
                    s = f / r2; c = g / r2;
                    g = d[i + 1] - p;
                    double r2_2 = (d[i] - g) * s + 2.0 * c * b;
                    p = s * r2_2;
                    d[i + 1] = g + p;
                    g = c * r2_2 - b;

                    for (int k = 0; k < n; k++)
                    {
                        f = v[k][i + 1];
                        v[k][i + 1] = s * v[k][i] + c * f;
                        v[k][i] = c * v[k][i] - s * f;
                    }

                    if (r2_2 == 0 && l <= m)
                    {
                        d[l] -= p; e[l] = g; e[m] = 0.0;
                        break;
                    }
                }
            }
        }
    }
}

public sealed record CholeskyDecomposition
{
    public Matrix L { get; }
    public bool IsPositiveDefinite { get; }

    private CholeskyDecomposition(Matrix l, bool isPD) { L = l; IsPositiveDefinite = isPD; }

    public static CholeskyDecomposition Compute(Matrix a)
    {
        if (!a.IsSquare) throw new ArgumentException("Matrix must be square");
        int n = a.Rows;
        var lData = new double[n][];

        for (int i = 0; i < n; i++) lData[i] = new double[n];

        bool isPD = true;
        var aData = a.ToJaggedArray();

        for (int j = 0; j < n; j++)
        {
            double sum = aData[j][j];
            for (int k = 0; k < j; k++) sum -= lData[j][k] * lData[j][k];

            if (sum <= 0)
            {
                isPD = false;
                lData[j][j] = 0;
            }
            else
            {
                lData[j][j] = System.Math.Sqrt(sum);
            }

            for (int i = j + 1; i < n; i++)
            {
                double sum2 = aData[i][j];
                for (int k = 0; k < j; k++) sum2 -= lData[i][k] * lData[j][k];
                lData[i][j] = isPD && lData[j][j] > 0 ? sum2 / lData[j][j] : 0;
            }
        }

        var lRows = lData.Select(r => r.ToImmutableArray()).ToImmutableArray();
        return new CholeskyDecomposition(new Matrix(lRows), isPD);
    }

    public Vector Solve(Vector b)
    {
        int n = L.Rows;
        var y = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = b[i];
            for (int j = 0; j < i; j++) sum -= L[i, j] * y[j];
            y[i] = L[i, i] > 0 ? sum / L[i, i] : 0;
        }

        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = y[i];
            for (int j = i + 1; j < n; j++) sum -= L[j, i] * x[j];
            x[i] = L[i, i] > 0 ? sum / L[i, i] : 0;
        }
        return new Vector(x.ToImmutableArray());
    }

    public Matrix Solve(Matrix b)
    {
        var cols = new Vector[b.Cols];
        for (int j = 0; j < b.Cols; j++) cols[j] = Solve(b.GetColumn(j));
        return Matrix.FromColumns(cols);
    }

    public Matrix GetInverse() => Solve(Matrix.Identity(L.Rows));
}