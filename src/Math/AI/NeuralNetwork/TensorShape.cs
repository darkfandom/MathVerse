namespace MathVerse.Math.AI.NeuralNetwork;
using System;

/// <summary>Describes the shape of a tensor with validation.</summary>
public sealed class TensorShape
{
    /// <summary>Gets the dimensions of this shape.</summary>
    public int[] Dimensions { get; }

    /// <summary>Gets the rank (number of dimensions) of this shape.</summary>
    public int Rank => Dimensions.Length;

    /// <summary>Gets the total number of elements described by this shape.</summary>
    public int TotalSize { get; }

    /// <summary>Initializes a new tensor shape with the specified dimensions.</summary>
    /// <param name="dimensions">The size of each dimension. All values must be greater than zero.</param>
    public TensorShape(params int[] dimensions)
    {
        if (dimensions.Length == 0)
        {
            throw new ArgumentException("At least one dimension must be specified.");
        }
        for (int i = 0; i < dimensions.Length; i++)
        {
            if (dimensions[i] <= 0)
            {
                throw new ArgumentException(
                    $"Dimension {i} has invalid size {dimensions[i]}. All dimensions must be greater than zero.");
            }
        }
        Dimensions = (int[])dimensions.Clone();
        int total = 1;
        for (int i = 0; i < dimensions.Length; i++)
        {
            total *= dimensions[i];
        }
        TotalSize = total;
    }

    /// <summary>Gets a scalar shape (single element).</summary>
    public static TensorShape Scalar => new(1);

    /// <summary>Creates a 1D vector shape.</summary>
    /// <param name="size">The number of elements in the vector.</param>
    /// <returns>A new TensorShape representing a vector.</returns>
    public static TensorShape Vector(int size) => new(size);

    /// <summary>Creates a 2D matrix shape.</summary>
    /// <param name="rows">The number of rows.</param>
    /// <param name="cols">The number of columns.</param>
    /// <returns>A new TensorShape representing a matrix.</returns>
    public static TensorShape Matrix(int rows, int cols) => new(rows, cols);

    /// <summary>Checks whether this shape is compatible with another shape for broadcasting.</summary>
    /// <param name="other">The other shape to check compatibility with.</param>
    /// <returns>True if the shapes are compatible for broadcasting operations.</returns>
    public bool IsCompatibleWith(TensorShape other)
    {
        int maxRank = Rank > other.Rank ? Rank : other.Rank;
        for (int i = 0; i < maxRank; i++)
        {
            int dimA = i < maxRank - Rank ? 1 : Dimensions[i - (maxRank - Rank)];
            int dimB = i < maxRank - other.Rank ? 1 : other.Dimensions[i - (maxRank - other.Rank)];
            if (dimA != dimB && dimA != 1 && dimB != 1)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>Returns a string representation of this shape.</summary>
    /// <returns>A string in the format [d1, d2, ...].</returns>
    public override string ToString() => $"[{string.Join(", ", Dimensions)}]";

    /// <summary>Determines whether this shape equals another shape.</summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the other object is a TensorShape with identical dimensions.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not TensorShape other) return false;
        if (Rank != other.Rank) return false;
        for (int i = 0; i < Rank; i++)
        {
            if (Dimensions[i] != other.Dimensions[i]) return false;
        }
        return true;
    }

    /// <summary>Gets the hash code for this shape.</summary>
    /// <returns>A hash code based on all dimensions.</returns>
    public override int GetHashCode()
    {
        int hash = 17;
        for (int i = 0; i < Rank; i++)
        {
            hash = hash * 31 + Dimensions[i];
        }
        return hash;
    }
}
