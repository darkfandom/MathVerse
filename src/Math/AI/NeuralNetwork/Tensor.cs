namespace MathVerse.Math.AI.NeuralNetwork;
using System;

/// <summary>N-dimensional array for neural network computations.</summary>
public sealed class Tensor
{
    private readonly double[] _data;

    /// <summary>Gets the shape of this tensor.</summary>
    public int[] Shape { get; }

    /// <summary>Gets the total number of elements in this tensor.</summary>
    public int TotalSize => _data.Length;

    /// <summary>Gets the rank (number of dimensions) of this tensor.</summary>
    public int Rank => Shape.Length;

    /// <summary>Initializes a new tensor with the specified shape, filled with zeros.</summary>
    /// <param name="shape">The dimensions of the tensor.</param>
    public Tensor(int[] shape)
    {
        Shape = (int[])shape.Clone();
        int size = 1;
        for (int i = 0; i < shape.Length; i++)
        {
            size *= shape[i];
        }
        _data = new double[size];
    }

    /// <summary>Initializes a new tensor with the specified shape and data.</summary>
    /// <param name="shape">The dimensions of the tensor.</param>
    /// <param name="data">The backing data array.</param>
    public Tensor(int[] shape, double[] data)
    {
        Shape = (int[])shape.Clone();
        _data = (double[])data.Clone();
    }

    /// <summary>Initializes a 2D tensor with the specified rows and columns.</summary>
    /// <param name="rows">The number of rows.</param>
    /// <param name="cols">The number of columns.</param>
    public Tensor(int rows, int cols) : this([rows, cols])
    {
    }

    /// <summary>Gets or sets the element at the specified indices.</summary>
    /// <param name="indices">The indices into each dimension.</param>
    /// <returns>The value at the specified position.</returns>
    public double this[params int[] indices]
    {
        get => _data[FlatIndex(indices)];
        set => _data[FlatIndex(indices)] = value;
    }

    /// <summary>Gets the underlying data array.</summary>
    public double[] Data => _data;

    /// <summary>Creates a tensor of the specified shape filled with zeros.</summary>
    /// <param name="shape">The dimensions of the tensor.</param>
    /// <returns>A new zero-filled tensor.</returns>
    public static Tensor Zeros(int[] shape) => new(shape);

    /// <summary>Creates a tensor of the specified shape filled with ones.</summary>
    /// <param name="shape">The dimensions of the tensor.</param>
    /// <returns>A new one-filled tensor.</returns>
    public static Tensor Ones(int[] shape)
    {
        int size = 1;
        for (int i = 0; i < shape.Length; i++)
        {
            size *= shape[i];
        }
        double[] data = new double[size];
        for (int i = 0; i < size; i++)
        {
            data[i] = 1.0;
        }
        return new Tensor(shape, data);
    }

    /// <summary>Creates a tensor of the specified shape filled with random values from a normal distribution.</summary>
    /// <param name="shape">The dimensions of the tensor.</param>
    /// <param name="seed">The random seed for reproducibility.</param>
    /// <returns>A new randomly filled tensor.</returns>
    public static Tensor Random(int[] shape, int seed = 42)
    {
        int size = 1;
        for (int i = 0; i < shape.Length; i++)
        {
            size *= shape[i];
        }
        var rng = new Random(seed);
        double[] data = new double[size];
        for (int i = 0; i < size; i++)
        {
            data[i] = NormalRandom(rng);
        }
        return new Tensor(shape, data);
    }

    /// <summary>Reshapes this tensor to a new shape without changing the data.</summary>
    /// <param name="newShape">The new dimensions. The total size must match.</param>
    /// <returns>A new tensor with the specified shape.</returns>
    public Tensor Reshape(int[] newShape)
    {
        int newSize = 1;
        for (int i = 0; i < newShape.Length; i++)
        {
            newSize *= newShape[i];
        }
        if (newSize != TotalSize)
        {
            throw new ArgumentException(
                $"Cannot reshape tensor of size {TotalSize} into shape [{string.Join(", ", newShape)}] (size {newSize}).");
        }
        return new Tensor(newShape, (double[])_data.Clone());
    }

    /// <summary>Transposes a 2D tensor by swapping rows and columns.</summary>
    /// <returns>A new tensor with dimensions swapped.</returns>
    public Tensor Transpose()
    {
        if (Rank != 2)
        {
            throw new InvalidOperationException("Transpose is only supported for 2D tensors.");
        }
        int rows = Shape[0];
        int cols = Shape[1];
        double[] newData = new double[TotalSize];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                newData[c * rows + r] = _data[r * cols + c];
            }
        }
        return new Tensor([cols, rows], newData);
    }

    /// <summary>Computes the sum of all elements in this tensor.</summary>
    /// <returns>The sum of all elements.</returns>
    public double Sum()
    {
        double s = 0.0;
        for (int i = 0; i < _data.Length; i++)
        {
            s += _data[i];
        }
        return s;
    }

    /// <summary>Computes the mean of all elements in this tensor.</summary>
    /// <returns>The mean of all elements.</returns>
    public double Mean()
    {
        return Sum() / _data.Length;
    }

    /// <summary>Returns the maximum element in this tensor.</summary>
    /// <returns>The maximum value.</returns>
    public double Max()
    {
        if (_data.Length == 0) throw new InvalidOperationException("Tensor is empty.");
        double m = _data[0];
        for (int i = 1; i < _data.Length; i++)
        {
            if (_data[i] > m) m = _data[i];
        }
        return m;
    }

    /// <summary>Returns the minimum element in this tensor.</summary>
    /// <returns>The minimum value.</returns>
    public double Min()
    {
        if (_data.Length == 0) throw new InvalidOperationException("Tensor is empty.");
        double m = _data[0];
        for (int i = 1; i < _data.Length; i++)
        {
            if (_data[i] < m) m = _data[i];
        }
        return m;
    }

    /// <summary>Creates a deep copy of this tensor.</summary>
    /// <returns>A new tensor with identical shape and data.</returns>
    public Tensor Clone()
    {
        return new Tensor((int[])Shape.Clone(), (double[])_data.Clone());
    }

    /// <summary>Computes the row-major flat index from multi-dimensional indices.</summary>
    /// <param name="indices">The indices into each dimension.</param>
    /// <returns>The flat index into the backing array.</returns>
    private int FlatIndex(int[] indices)
    {
        if (indices.Length != Shape.Length)
        {
            throw new ArgumentException(
                $"Expected {Shape.Length} indices but got {indices.Length}.");
        }
        int flat = 0;
        int stride = 1;
        for (int i = Shape.Length - 1; i >= 0; i--)
        {
            if (indices[i] < 0 || indices[i] >= Shape[i])
            {
                throw new IndexOutOfRangeException(
                    $"Index {indices[i]} is out of range for dimension {i} (size {Shape[i]}).");
            }
            flat += indices[i] * stride;
            stride *= Shape[i];
        }
        return flat;
    }

    /// <summary>Generates a standard normal random value using the Box-Muller transform.</summary>
    /// <param name="rng">The random number generator.</param>
    /// <returns>A value from the standard normal distribution.</returns>
    private static double NormalRandom(Random rng)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);
    }
}
