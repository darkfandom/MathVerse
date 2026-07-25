namespace MathVerse.Math.Numerics.Sparse;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum SparseFormat
{
    CSR,
    CSC,
    COO
}

public sealed record SparseMatrix
{
    public int Rows { get; }
    public int Cols { get; }
    public int NonZeros { get; }
    public SparseFormat Format { get; }
    public ImmutableArray<double> Values { get; }
    public ImmutableArray<int> RowPtr { get; }
    public ImmutableArray<int> ColIndices { get; }
    public ImmutableArray<int> ColPtr { get; }
    public ImmutableArray<int> RowIndices { get; }

    internal SparseMatrix(
        int rows, int cols, int nnz, SparseFormat format,
        ImmutableArray<double> values,
        ImmutableArray<int> rowPtr,
        ImmutableArray<int> colIndices,
        ImmutableArray<int> colPtr,
        ImmutableArray<int> rowIndices)
    {
        Rows = rows;
        Cols = cols;
        NonZeros = nnz;
        Format = format;
        Values = values;
        RowPtr = rowPtr;
        ColIndices = colIndices;
        ColPtr = colPtr;
        RowIndices = rowIndices;
    }

    public bool IsSquare => Rows == Cols;

    public static SparseMatrix FromDense(Matrix dense, SparseFormat format = SparseFormat.CSR)
    {
        int rows = dense.Rows, cols = dense.Cols;
        var entries = new List<(int row, int col, double val)>();

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                double val = dense[i, j];
                if (System.Math.Abs(val) > 1e-15)
                    entries.Add((i, j, val));
            }

        return format switch
        {
            SparseFormat.CSR => BuildCSR(rows, cols, entries),
            SparseFormat.CSC => BuildCSC(rows, cols, entries),
            SparseFormat.COO => BuildCOO(rows, cols, entries),
            _ => throw new ArgumentException("Unknown format")
        };
    }

    private static SparseMatrix BuildCSR(int rows, int cols, List<(int row, int col, double val)> entries)
    {
        entries.Sort((a, b) => a.row != b.row ? a.row.CompareTo(b.row) : a.col.CompareTo(b.col));

        var values = new double[entries.Count];
        var colIndices = new int[entries.Count];
        var rowPtr = new int[rows + 1];

        int currentRow = 0;
        rowPtr[0] = 0;

        for (int k = 0; k < entries.Count; k++)
        {
            var (row, col, val) = entries[k];
            while (currentRow < row)
            {
                rowPtr[++currentRow] = k;
            }
            values[k] = val;
            colIndices[k] = col;
        }
        while (currentRow < rows) rowPtr[++currentRow] = entries.Count;

        return new SparseMatrix(rows, cols, entries.Count, SparseFormat.CSR,
            values.ToImmutableArray(), rowPtr.ToImmutableArray(), colIndices.ToImmutableArray(),
            ImmutableArray<int>.Empty, ImmutableArray<int>.Empty);
    }

    private static SparseMatrix BuildCSC(int rows, int cols, List<(int row, int col, double val)> entries)
    {
        entries.Sort((a, b) => a.col != b.col ? a.col.CompareTo(b.col) : a.row.CompareTo(b.row));

        var values = new double[entries.Count];
        var rowIndices = new int[entries.Count];
        var colPtr = new int[cols + 1];

        int currentCol = 0;
        colPtr[0] = 0;

        for (int k = 0; k < entries.Count; k++)
        {
            var (row, col, val) = entries[k];
            while (currentCol < col)
            {
                colPtr[++currentCol] = k;
            }
            values[k] = val;
            rowIndices[k] = row;
        }
        while (currentCol < cols) colPtr[++currentCol] = entries.Count;

        return new SparseMatrix(rows, cols, entries.Count, SparseFormat.CSC,
            values.ToImmutableArray(), ImmutableArray<int>.Empty, ImmutableArray<int>.Empty,
            colPtr.ToImmutableArray(), rowIndices.ToImmutableArray());
    }

    private static SparseMatrix BuildCOO(int rows, int cols, List<(int row, int col, double val)> entries)
    {
        var values = new double[entries.Count];
        var rowIndices = new int[entries.Count];
        var colIndices = new int[entries.Count];

        for (int k = 0; k < entries.Count; k++)
        {
            values[k] = entries[k].val;
            rowIndices[k] = entries[k].row;
            colIndices[k] = entries[k].col;
        }

        return new SparseMatrix(rows, cols, entries.Count, SparseFormat.COO,
            values.ToImmutableArray(), ImmutableArray<int>.Empty, colIndices.ToImmutableArray(),
            ImmutableArray<int>.Empty, rowIndices.ToImmutableArray());
    }

    public Matrix ToDense()
    {
        var data = new double[Rows][];
        for (int i = 0; i < Rows; i++) data[i] = new double[Cols];

        switch (Format)
        {
            case SparseFormat.CSR:
                for (int i = 0; i < Rows; i++)
                    for (int k = RowPtr[i]; k < RowPtr[i + 1]; k++)
                        data[i][ColIndices[k]] = Values[k];
                break;
            case SparseFormat.CSC:
                for (int j = 0; j < Cols; j++)
                    for (int k = ColPtr[j]; k < ColPtr[j + 1]; k++)
                        data[RowIndices[k]][j] = Values[k];
                break;
            case SparseFormat.COO:
                for (int k = 0; k < NonZeros; k++)
                    data[RowIndices[k]][ColIndices[k]] = Values[k];
                break;
        }
        return new Matrix(data);
    }

    public SparseMatrix Transpose()
    {
        return Format switch
        {
            SparseFormat.CSR => ToCSC(),
            SparseFormat.CSC => ToCSR(),
            SparseFormat.COO => new SparseMatrix(Cols, Rows, NonZeros, SparseFormat.COO,
                Values, ImmutableArray<int>.Empty, RowIndices, ImmutableArray<int>.Empty, ColIndices),
            _ => throw new InvalidOperationException()
        };
    }

    public SparseMatrix ToCSR()
    {
        if (Format == SparseFormat.CSR) return this;
        return FromDense(ToDense(), SparseFormat.CSR);
    }

    public SparseMatrix ToCSC()
    {
        if (Format == SparseFormat.CSC) return this;
        return FromDense(ToDense(), SparseFormat.CSC);
    }

    public Vector Multiply(Vector v)
    {
        if (Cols != v.Size) throw new ArgumentException("Matrix columns must match vector size");
        var result = new double[Rows];

        switch (Format)
        {
            case SparseFormat.CSR:
                for (int i = 0; i < Rows; i++)
                {
                    double sum = 0;
                    for (int k = RowPtr[i]; k < RowPtr[i + 1]; k++)
                        sum += Values[k] * v[ColIndices[k]];
                    result[i] = sum;
                }
                break;
            case SparseFormat.CSC:
                for (int j = 0; j < Cols; j++)
                {
                    double vj = v[j];
                    for (int k = ColPtr[j]; k < ColPtr[j + 1]; k++)
                        result[RowIndices[k]] += Values[k] * vj;
                }
                break;
            case SparseFormat.COO:
                for (int k = 0; k < NonZeros; k++)
                    result[RowIndices[k]] += Values[k] * v[ColIndices[k]];
                break;
        }
        return new Vector(result.ToImmutableArray());
    }

    public SparseMatrix Add(SparseMatrix other)
    {
        if (Rows != other.Rows || Cols != other.Cols)
            throw new ArgumentException("Matrix dimensions must match");

        var a = Format == SparseFormat.CSR ? this : ToCSR();
        var b = other.Format == SparseFormat.CSR ? other : other.ToCSR();

        var entries = new List<(int row, int col, double val)>();

        for (int i = 0; i < Rows; i++)
        {
            int pa = a.RowPtr[i], pb = b.RowPtr[i];
            int ea = a.RowPtr[i + 1], eb = b.RowPtr[i + 1];

            while (pa < ea && pb < eb)
            {
                int ca = a.ColIndices[pa], cb = b.ColIndices[pb];
                if (ca < cb)
                {
                    entries.Add((i, ca, a.Values[pa]));
                    pa++;
                }
                else if (ca > cb)
                {
                    entries.Add((i, cb, b.Values[pb]));
                    pb++;
                }
                else
                {
                    double sum = a.Values[pa] + b.Values[pb];
                    if (System.Math.Abs(sum) > 1e-15)
                        entries.Add((i, ca, sum));
                    pa++; pb++;
                }
            }
            while (pa < ea) { entries.Add((i, a.ColIndices[pa], a.Values[pa])); pa++; }
            while (pb < eb) { entries.Add((i, b.ColIndices[pb], b.Values[pb])); pb++; }
        }

        return BuildCSR(Rows, Cols, entries);
    }

    public SparseMatrix Multiply(SparseMatrix other)
    {
        if (Cols != other.Rows)
            throw new ArgumentException($"Cannot multiply {Rows}x{Cols} with {other.Rows}x{other.Cols}");

        var a = Format == SparseFormat.CSR ? this : ToCSR();
        var b = other.Format == SparseFormat.CSC ? other : other.ToCSC();

        var entries = new List<(int row, int col, double val)>();

        for (int i = 0; i < Rows; i++)
        {
            var colAccum = new Dictionary<int, double>();

            for (int ka = a.RowPtr[i]; ka < a.RowPtr[i + 1]; ka++)
            {
                int k = a.ColIndices[ka];
                double av = a.Values[ka];

                for (int kb = b.ColPtr[k]; kb < b.ColPtr[k + 1]; kb++)
                {
                    int j = b.RowIndices[kb];
                    double bv = b.Values[kb];
                    double prod = av * bv;
                    if (colAccum.TryGetValue(j, out double existing))
                        colAccum[j] = existing + prod;
                    else
                        colAccum[j] = prod;
                }
            }

            foreach (var (col, val) in colAccum)
                if (System.Math.Abs(val) > 1e-15)
                    entries.Add((i, col, val));
        }

        return BuildCSR(Rows, other.Cols, entries);
    }

    public double this[int row, int col]
    {
        get
        {
            switch (Format)
            {
                case SparseFormat.CSR:
                    for (int k = RowPtr[row]; k < RowPtr[row + 1]; k++)
                        if (ColIndices[k] == col) return Values[k];
                    return 0;
                case SparseFormat.CSC:
                    for (int k = ColPtr[col]; k < ColPtr[col + 1]; k++)
                        if (RowIndices[k] == row) return Values[k];
                    return 0;
                case SparseFormat.COO:
                    for (int k = 0; k < NonZeros; k++)
                        if (RowIndices[k] == row && ColIndices[k] == col) return Values[k];
                    return 0;
                default: return 0;
            }
        }
    }
}