namespace MathVerse.Math.DataScience.NumericalIntegration;

using System;

/// <summary>
/// Provides sparse matrix operations using Compressed Sparse Row (CSR) format.
/// </summary>
public static class SparseAnalytics
{
    /// <summary>
    /// Multiplies a sparse matrix (in CSR format) by a dense vector.
    /// Result[i] = sum(values[k] * vector[colInd[k]]) for k in rowPtr[i]..rowPtr[i+1].
    /// </summary>
    /// <param name="rowPtr">CSR row pointer array of length (rows + 1).</param>
    /// <param name="colInd">CSR column index array.</param>
    /// <param name="values">CSR non-zero values array.</param>
    /// <param name="vector">The dense vector to multiply.</param>
    /// <returns>The result vector.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when array lengths are inconsistent.</exception>
    public static double[] MultiplySparse(int[] rowPtr, int[] colInd, double[] values, double[] vector)
    {
        if (rowPtr is null) throw new ArgumentNullException(nameof(rowPtr));
        if (colInd is null) throw new ArgumentNullException(nameof(colInd));
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (vector is null) throw new ArgumentNullException(nameof(vector));

        if (colInd.Length != values.Length)
            throw new ArgumentException("colInd and values must have the same length.");

        int rows = rowPtr.Length - 1;
        if (rows < 0)
            throw new ArgumentException("rowPtr must have at least one element.");

        double[] result = new double[rows];

        for (int i = 0; i < rows; i++)
        {
            double sum = 0.0;
            for (int k = rowPtr[i]; k < rowPtr[i + 1]; k++)
            {
                int col = colInd[k];
                if (col >= 0 && col < vector.Length)
                    sum += values[k] * vector[col];
            }
            result[i] = sum;
        }

        return result;
    }

    /// <summary>
    /// Transposes a sparse matrix stored in CSR format, returning a new CSR representation.
    /// </summary>
    /// <param name="rowPtr">CSR row pointer array of length (rows + 1).</param>
    /// <param name="colInd">CSR column index array.</param>
    /// <param name="values">CSR non-zero values array.</param>
    /// <param name="rows">The number of rows in the original matrix.</param>
    /// <param name="cols">The number of columns in the original matrix.</param>
    /// <returns>A tuple of (newRowPtr, newColInd, newValues) for the transposed matrix.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when array lengths are inconsistent.</exception>
    public static (int[] newRowPtr, int[] newColInd, double[] newValues) TransposeSparse(
        int[] rowPtr, int[] colInd, double[] values, int rows, int cols)
    {
        if (rowPtr is null) throw new ArgumentNullException(nameof(rowPtr));
        if (colInd is null) throw new ArgumentNullException(nameof(colInd));
        if (values is null) throw new ArgumentNullException(nameof(values));

        if (colInd.Length != values.Length)
            throw new ArgumentException("colInd and values must have the same length.");

        int nnz = values.Length;
        int[] newRowPtr = new int[cols + 1];
        int[] newColInd = new int[nnz];
        double[] newValues = new double[nnz];

        // Count non-zeros per column (which becomes row in transpose)
        for (int k = 0; k < nnz; k++)
            newRowPtr[colInd[k] + 1]++;

        // Prefix sum
        for (int i = 1; i <= cols; i++)
            newRowPtr[i] += newRowPtr[i - 1];

        // Fill transposed data
        int[] tempCount = new int[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int k = rowPtr[i]; k < rowPtr[i + 1]; k++)
            {
                int col = colInd[k];
                int destIdx = newRowPtr[col] + tempCount[col];
                newColInd[destIdx] = i;
                newValues[destIdx] = values[k];
                tempCount[col]++;
            }
        }

        return (newRowPtr, newColInd, newValues);
    }

    /// <summary>
    /// Computes the dot product of two sparse vectors stored in COO format.
    /// </summary>
    /// <param name="indices1">Index array of the first sparse vector.</param>
    /// <param name="values1">Value array of the first sparse vector.</param>
    /// <param name="indices2">Index array of the second sparse vector.</param>
    /// <param name="values2">Value array of the second sparse vector.</param>
    /// <param name="dimension">The dimension of the vectors.</param>
    /// <returns>The dot product value.</returns>
    public static double SparseDotProduct(int[] indices1, double[] values1, int[] indices2, double[] values2, int dimension)
    {
        if (indices1 is null) throw new ArgumentNullException(nameof(indices1));
        if (values1 is null) throw new ArgumentNullException(nameof(values1));
        if (indices2 is null) throw new ArgumentNullException(nameof(indices2));
        if (values2 is null) throw new ArgumentNullException(nameof(values2));

        double result = 0.0;
        int i = 0, j = 0;

        while (i < indices1.Length && j < indices2.Length)
        {
            if (indices1[i] == indices2[j])
            {
                result += values1[i] * values2[j];
                i++;
                j++;
            }
            else if (indices1[i] < indices2[j])
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        return result;
    }

    /// <summary>
    /// Computes the 2-norm of a sparse vector in CSR format.
    /// </summary>
    /// <param name="rowPtr">Row pointer array.</param>
    /// <param name="colInd">Column index array.</param>
    /// <param name="values">Non-zero values array.</param>
    /// <returns>The Euclidean norm.</returns>
    public static double SparseNorm2(int[] rowPtr, int[] colInd, double[] values)
    {
        if (rowPtr is null) throw new ArgumentNullException(nameof(rowPtr));
        if (values is null) throw new ArgumentNullException(nameof(values));

        double sumSq = 0.0;
        for (int k = 0; k < values.Length; k++)
            sumSq += values[k] * values[k];

        return System.Math.Sqrt(sumSq);
    }

    /// <summary>
    /// Scales all non-zero values of a sparse matrix by a scalar factor.
    /// </summary>
    /// <param name="values">The non-zero values array (modified in-place).</param>
    /// <param name="scalar">The scalar multiplier.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public static void ScaleSparse(double[] values, double scalar)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));

        for (int i = 0; i < values.Length; i++)
            values[i] *= scalar;
    }

    /// <summary>
    /// Computes the number of non-zero elements per row from a CSR representation.
    /// </summary>
    /// <param name="rowPtr">The CSR row pointer array.</param>
    /// <returns>An array containing the number of non-zeros per row.</returns>
    public static int[] NonZerosPerRow(int[] rowPtr)
    {
        if (rowPtr is null) throw new ArgumentNullException(nameof(rowPtr));

        int rows = rowPtr.Length - 1;
        if (rows < 0) return Array.Empty<int>();

        int[] result = new int[rows];
        for (int i = 0; i < rows; i++)
            result[i] = rowPtr[i + 1] - rowPtr[i];

        return result;
    }
}
