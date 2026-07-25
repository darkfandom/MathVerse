namespace MathVerse.Math.Distributed.IncrementalComputation;

/// <summary>Recomputes only the changed portions of a result array using an incremental update function.</summary>
public sealed class PartialRecompute
{
    private long _totalRecomputations;
    private long _totalValuesUpdated;

    /// <summary>Gets the total number of partial recomputation operations performed.</summary>
    public long TotalRecomputations => Interlocked.Read(ref _totalRecomputations);

    /// <summary>Gets the total number of individual values updated across all recomputations.</summary>
    public long TotalValuesUpdated => Interlocked.Read(ref _totalValuesUpdated);

    /// <summary>Recomputes only the specified indices of the previous result using the update function.</summary>
    /// <param name="previousResult">The previous result array (not modified).</param>
    /// <param name="updateFunc">
    /// A function that receives the index and the previous value, returning the updated value.
    /// </param>
    /// <param name="changedIndices">The indices whose values should be recomputed.</param>
    /// <returns>A new array with only the specified indices updated.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="previousResult"/> or <paramref name="updateFunc"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an index is outside the array bounds.</exception>
    public double[] Recompute(double[] previousResult, Func<int, double, double> updateFunc, int[] changedIndices)
    {
        if (previousResult is null)
        {
            throw new ArgumentNullException(nameof(previousResult));
        }

        if (updateFunc is null)
        {
            throw new ArgumentNullException(nameof(updateFunc));
        }

        if (changedIndices is null || changedIndices.Length == 0)
        {
            return (double[])previousResult.Clone();
        }

        double[] result = new double[previousResult.Length];
        Array.Copy(previousResult, result, previousResult.Length);

        foreach (int index in changedIndices)
        {
            if (index < 0 || index >= result.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(changedIndices),
                    index,
                    $"Index {index} is out of range for array of length {result.Length}.");
            }

            result[index] = updateFunc(index, previousResult[index]);
        }

        Interlocked.Increment(ref _totalRecomputations);
        Interlocked.Add(ref _totalValuesUpdated, changedIndices.Length);

        return result;
    }

    /// <summary>Recomputes only the indices where the input has changed, using equality comparison.</summary>
    /// <param name="previousResult">The previous result array.</param>
    /// <param name="previousInput">The previous input array.</param>
    /// <param name="currentInput">The current input array.</param>
    /// <param name="computeSingle">A function that computes a single output value from the current input.</param>
    /// <returns>A new array with only the changed indices updated.</returns>
    /// <exception cref="ArgumentException">Thrown when input arrays differ in length.</exception>
    public double[] RecomputeFromInputChanges(
        double[] previousResult,
        double[] previousInput,
        double[] currentInput,
        Func<double[], int, double> computeSingle)
    {
        if (previousResult is null)
        {
            throw new ArgumentNullException(nameof(previousResult));
        }

        if (previousInput is null)
        {
            throw new ArgumentNullException(nameof(previousInput));
        }

        if (currentInput is null)
        {
            throw new ArgumentNullException(nameof(currentInput));
        }

        if (previousInput.Length != currentInput.Length)
        {
            throw new ArgumentException(
                $"Input arrays must have the same length. Previous: {previousInput.Length}, Current: {currentInput.Length}.");
        }

        int resultLength = previousResult.Length;
        double[] result = new double[resultLength];
        Array.Copy(previousResult, result, resultLength);

        int maxLength = System.Math.Min(currentInput.Length, resultLength);
        int updatedCount = 0;

        for (int i = 0; i < maxLength; i++)
        {
            if (System.Math.Abs(currentInput[i] - previousInput[i]) > 0.0)
            {
                result[i] = computeSingle(currentInput, i);
                updatedCount++;
            }
        }

        Interlocked.Increment(ref _totalRecomputations);
        Interlocked.Add(ref _totalValuesUpdated, updatedCount);

        return result;
    }

    /// <summary>Identifies which indices differ between two arrays within a tolerance.</summary>
    /// <param name="previous">The previous array.</param>
    /// <param name="current">The current array.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison.</param>
    /// <returns>An array of indices where the values differ.</returns>
    public static int[] FindChangedIndices(double[] previous, double[] current, double tolerance = 0.0)
    {
        if (previous is null)
        {
            throw new ArgumentNullException(nameof(previous));
        }

        if (current is null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        int length = System.Math.Min(previous.Length, current.Length);
        var changed = new List<int>();

        for (int i = 0; i < length; i++)
        {
            double diff = System.Math.Abs(current[i] - previous[i]);
            if (diff > tolerance)
            {
                changed.Add(i);
            }
        }

        return changed.ToArray();
    }

    /// <summary>Resets the internal counters.</summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _totalRecomputations, 0);
        Interlocked.Exchange(ref _totalValuesUpdated, 0);
    }
}
