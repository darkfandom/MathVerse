namespace MathVerse.Math.AI.NeuralNetwork;
using System;

/// <summary>Static operations on tensors for neural network computation.</summary>
public sealed class TensorOperations
{
    /// <summary>Performs element-wise addition of two tensors.</summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns>A new tensor containing the element-wise sum.</returns>
    public Tensor Add(Tensor a, Tensor b)
    {
        if (a.TotalSize != b.TotalSize)
        {
            throw new ArgumentException("Tensors must have the same total size for addition.");
        }
        double[] result = new double[a.TotalSize];
        double[] dataA = a.Data;
        double[] dataB = b.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = dataA[i] + dataB[i];
        }
        return new Tensor(a.Shape, result);
    }

    /// <summary>Performs element-wise subtraction of two tensors.</summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns>A new tensor containing a - b element-wise.</returns>
    public Tensor Subtract(Tensor a, Tensor b)
    {
        if (a.TotalSize != b.TotalSize)
        {
            throw new ArgumentException("Tensors must have the same total size for subtraction.");
        }
        double[] result = new double[a.TotalSize];
        double[] dataA = a.Data;
        double[] dataB = b.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = dataA[i] - dataB[i];
        }
        return new Tensor(a.Shape, result);
    }

    /// <summary>Performs element-wise multiplication (Hadamard product) of two tensors.</summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns>A new tensor containing the element-wise product.</returns>
    public Tensor Multiply(Tensor a, Tensor b)
    {
        if (a.TotalSize != b.TotalSize)
        {
            throw new ArgumentException("Tensors must have the same total size for multiplication.");
        }
        double[] result = new double[a.TotalSize];
        double[] dataA = a.Data;
        double[] dataB = b.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = dataA[i] * dataB[i];
        }
        return new Tensor(a.Shape, result);
    }

    /// <summary>Multiplies every element of a tensor by a scalar value.</summary>
    /// <param name="a">The tensor.</param>
    /// <param name="scalar">The scalar multiplier.</param>
    /// <returns>A new scaled tensor.</returns>
    public Tensor Scale(Tensor a, double scalar)
    {
        double[] result = new double[a.TotalSize];
        double[] dataA = a.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = dataA[i] * scalar;
        }
        return new Tensor(a.Shape, result);
    }

    /// <summary>Performs matrix multiplication of two 2D tensors.</summary>
    /// <param name="a">The left matrix tensor of shape [M, K].</param>
    /// <param name="b">The right matrix tensor of shape [K, N].</param>
    /// <returns>A new tensor of shape [M, N] containing the matrix product.</returns>
    public Tensor MatMul(Tensor a, Tensor b)
    {
        if (a.Rank != 2 || b.Rank != 2)
        {
            throw new ArgumentException("MatMul requires 2D tensors.");
        }
        if (a.Shape[1] != b.Shape[0])
        {
            throw new ArgumentException(
                $"Incompatible dimensions for MatMul: [{a.Shape[0]}, {a.Shape[1]}] x [{b.Shape[0]}, {b.Shape[1]}].");
        }
        int m = a.Shape[0];
        int k = a.Shape[1];
        int n = b.Shape[1];
        double[] result = new double[m * n];
        double[] dataA = a.Data;
        double[] dataB = b.Data;
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double sum = 0.0;
                for (int l = 0; l < k; l++)
                {
                    sum += dataA[i * k + l] * dataB[l * n + j];
                }
                result[i * n + j] = sum;
            }
        }
        return new Tensor([m, n], result);
    }

    /// <summary>Computes the dot product of two 1D tensors.</summary>
    /// <param name="a">The first vector tensor.</param>
    /// <param name="b">The second vector tensor.</param>
    /// <returns>A scalar tensor containing the dot product.</returns>
    public Tensor Dot(Tensor a, Tensor b)
    {
        if (a.Rank != 1 || b.Rank != 1)
        {
            throw new ArgumentException("Dot product requires 1D tensors.");
        }
        if (a.TotalSize != b.TotalSize)
        {
            throw new ArgumentException("Vectors must have the same length for dot product.");
        }
        double sum = 0.0;
        double[] dataA = a.Data;
        double[] dataB = b.Data;
        for (int i = 0; i < a.TotalSize; i++)
        {
            sum += dataA[i] * dataB[i];
        }
        return new Tensor([1], [sum]);
    }

    /// <summary>Reduces a tensor by summing along the specified axis.</summary>
    /// <param name="a">The input tensor.</param>
    /// <param name="axis">The axis along which to reduce.</param>
    /// <returns>A new tensor with the specified axis removed.</returns>
    public Tensor Sum(Tensor a, int axis)
    {
        if (axis < 0 || axis >= a.Rank)
        {
            throw new ArgumentException($"Axis {axis} is out of range for tensor of rank {a.Rank}.");
        }
        int[] newShape = new int[a.Rank - 1];
        int idx = 0;
        for (int i = 0; i < a.Rank; i++)
        {
            if (i != axis) newShape[idx++] = a.Shape[i];
        }
        if (newShape.Length == 0) newShape = [1];
        double[] result = new double[Tensor.Zeros(newShape).TotalSize];
        int[] srcIdx = new int[a.Rank];
        int outerSize = 1;
        int axisSize = a.Shape[axis];
        int innerSize = 1;
        for (int i = 0; i < axis; i++) outerSize *= a.Shape[i];
        for (int i = axis + 1; i < a.Rank; i++) innerSize *= a.Shape[i];
        for (int o = 0; o < outerSize; o++)
        {
            for (int inner = 0; inner < innerSize; inner++)
            {
                double sum = 0.0;
                for (int a2 = 0; a2 < axisSize; a2++)
                {
                    int srcFlat = 0;
                    int stride = 1;
                    for (int d = a.Rank - 1; d >= 0; d--)
                    {
                        int idxVal;
                        if (d < axis)
                        {
                            idxVal = o / 1;
                            int tmpO = o;
                            for (int dd = 0; dd < d; dd++) tmpO /= a.Shape[dd];
                            idxVal = tmpO % a.Shape[d];
                        }
                        else if (d == axis)
                        {
                            idxVal = a2;
                        }
                        else
                        {
                            idxVal = inner / 1;
                            int tmpI = inner;
                            for (int dd = axis + 1; dd < d; dd++) tmpI /= a.Shape[dd];
                            idxVal = tmpI % a.Shape[d];
                        }
                        srcFlat += idxVal * stride;
                        stride *= a.Shape[d];
                    }
                    sum += a.Data[srcFlat];
                }
                int resFlat = o * innerSize + inner;
                if (resFlat < result.Length)
                {
                    result[resFlat] = sum;
                }
            }
        }
        return new Tensor(newShape, result);
    }

    /// <summary>Reduces a tensor by computing the mean along the specified axis.</summary>
    /// <param name="a">The input tensor.</param>
    /// <param name="axis">The axis along which to reduce.</param>
    /// <returns>A new tensor with the specified axis removed, containing mean values.</returns>
    public Tensor Mean(Tensor a, int axis)
    {
        Tensor sumResult = Sum(a, axis);
        double divisor = a.Shape[axis];
        return Scale(sumResult, 1.0 / divisor);
    }

    /// <summary>Broadcasts a tensor to the target shape following NumPy broadcasting rules.</summary>
    /// <param name="a">The tensor to broadcast.</param>
    /// <param name="targetShape">The target shape to broadcast to.</param>
    /// <returns>A new tensor with the target shape.</returns>
    public Tensor Broadcast(Tensor a, int[] targetShape)
    {
        if (targetShape.Length < a.Rank)
        {
            throw new ArgumentException("Target shape must have at least as many dimensions as the source.");
        }
        int[] resultShape = new int[targetShape.Length];
        int padCount = targetShape.Length - a.Rank;
        for (int i = 0; i < targetShape.Length; i++)
        {
            if (i < padCount)
            {
                resultShape[i] = targetShape[i];
            }
            else
            {
                int srcDim = a.Shape[i - padCount];
                if (srcDim == targetShape[i] || srcDim == 1)
                {
                    resultShape[i] = targetShape[i];
                }
                else
                {
                    throw new ArgumentException(
                        $"Cannot broadcast dimension {srcDim} to {targetShape[i]} at axis {i}.");
                }
            }
        }
        int totalSize = 1;
        for (int i = 0; i < resultShape.Length; i++)
        {
            totalSize *= resultShape[i];
        }
        double[] result = new double[totalSize];
        int[] srcShape = a.Shape;
        int srcRank = srcShape.Length;
        for (int flat = 0; flat < totalSize; flat++)
        {
            int tmp = flat;
            int[] dstIdx = new int[resultShape.Length];
            for (int d = resultShape.Length - 1; d >= 0; d--)
            {
                dstIdx[d] = tmp % resultShape[d];
                tmp /= resultShape[d];
            }
            int srcFlat = 0;
            int stride = 1;
            for (int d = srcRank - 1; d >= 0; d--)
            {
                int srcDim = dstIdx[d + padCount];
                if (srcShape[d] == 1)
                {
                    srcDim = 0;
                }
                srcFlat += srcDim * stride;
                stride *= srcShape[d];
            }
            result[flat] = a.Data[srcFlat];
        }
        return new Tensor(resultShape, result);
    }

    /// <summary>Computes the softmax of a 1D tensor.</summary>
    /// <param name="a">The input tensor.</param>
    /// <returns>A new tensor with softmax applied (values sum to 1).</returns>
    public Tensor Softmax(Tensor a)
    {
        if (a.Rank != 1)
        {
            throw new ArgumentException("Softmax is only supported for 1D tensors.");
        }
        int n = a.TotalSize;
        double[] result = new double[n];
        double maxVal = a.Max();
        double expSum = 0.0;
        for (int i = 0; i < n; i++)
        {
            result[i] = System.Math.Exp(a.Data[i] - maxVal);
            expSum += result[i];
        }
        for (int i = 0; i < n; i++)
        {
            result[i] /= expSum;
        }
        return new Tensor([n], result);
    }

    /// <summary>Computes the element-wise natural logarithm of a tensor.</summary>
    /// <param name="a">The input tensor.</param>
    /// <returns>A new tensor with ln applied to each element.</returns>
    public Tensor Log(Tensor a)
    {
        double[] result = new double[a.TotalSize];
        double[] data = a.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = System.Math.Log(data[i]);
        }
        return new Tensor(a.Shape, result);
    }

    /// <summary>Computes the element-wise exponential of a tensor.</summary>
    /// <param name="a">The input tensor.</param>
    /// <returns>A new tensor with exp applied to each element.</returns>
    public Tensor Exp(Tensor a)
    {
        double[] result = new double[a.TotalSize];
        double[] data = a.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = System.Math.Exp(data[i]);
        }
        return new Tensor(a.Shape, result);
    }

    /// <summary>Computes the element-wise absolute value of a tensor.</summary>
    /// <param name="a">The input tensor.</param>
    /// <returns>A new tensor with absolute values.</returns>
    public Tensor Abs(Tensor a)
    {
        double[] result = new double[a.TotalSize];
        double[] data = a.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = System.Math.Abs(data[i]);
        }
        return new Tensor(a.Shape, result);
    }

    /// <summary>Clips all values in a tensor to the specified range.</summary>
    /// <param name="a">The input tensor.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>A new tensor with values clipped to [min, max].</returns>
    public Tensor Clip(Tensor a, double min, double max)
    {
        double[] result = new double[a.TotalSize];
        double[] data = a.Data;
        for (int i = 0; i < result.Length; i++)
        {
            double v = data[i];
            if (v < min) v = min;
            else if (v > max) v = max;
            result[i] = v;
        }
        return new Tensor(a.Shape, result);
    }
}
