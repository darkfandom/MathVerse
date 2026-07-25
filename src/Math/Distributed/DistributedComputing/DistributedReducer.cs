namespace MathVerse.Math.Distributed.DistributedComputing;

using System;

/// <summary>
/// Performs tree reduction across partial results from distributed workers.
/// Combines pairs of result arrays using a provided reducer function in a binary tree pattern,
/// reducing O(n) partial results to a single final result in O(log n) reduction steps.
/// </summary>
public sealed class DistributedReducer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedReducer"/> class.
    /// </summary>
    public DistributedReducer() { }

    /// <summary>
    /// Reduces an array of partial results into a single result using a binary tree reduction pattern.
    /// Pairs of partial results are combined at each level until one final result remains.
    /// </summary>
    /// <param name="partialResults">The partial results from distributed workers.</param>
    /// <param name="reducer">A function that combines two partial results into one.</param>
    /// <returns>The final reduced result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="partialResults"/> or <paramref name="reducer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="partialResults"/> is empty.</exception>
    public double[] Reduce(double[][] partialResults, Func<double[], double[], double[]> reducer)
    {
        if (partialResults == null) throw new ArgumentNullException(nameof(partialResults));
        if (reducer == null) throw new ArgumentNullException(nameof(reducer));
        if (partialResults.Length == 0)
            throw new ArgumentException("Partial results array must not be empty.", nameof(partialResults));

        if (partialResults.Length == 1)
            return partialResults[0];

        var current = partialResults;

        while (current.Length > 1)
        {
            int nextLength = (current.Length + 1) / 2;
            var next = new double[nextLength][];

            for (int i = 0; i < current.Length; i += 2)
            {
                int pairIndex = i / 2;
                if (i + 1 < current.Length)
                {
                    next[pairIndex] = reducer(current[i], current[i + 1]);
                }
                else
                {
                    next[pairIndex] = current[i];
                }
            }

            current = next;
        }

        return current[0];
    }

    /// <summary>
    /// Reduces partial results with an identity element as the initial accumulator.
    /// Useful when the reducer function requires a neutral starting value.
    /// </summary>
    /// <param name="partialResults">The partial results from distributed workers.</param>
    /// <param name="reducer">A function that combines two double arrays into one.</param>
    /// <param name="identity">The identity element (neutral value) for the reduction.</param>
    /// <returns>The final reduced result.</returns>
    public double[] ReduceWithIdentity(
        double[][] partialResults,
        Func<double[], double[], double[]> reducer,
        double[] identity)
    {
        if (partialResults == null) throw new ArgumentNullException(nameof(partialResults));
        if (reducer == null) throw new ArgumentNullException(nameof(reducer));
        if (identity == null) throw new ArgumentNullException(nameof(identity));

        if (partialResults.Length == 0)
            return identity;

        return Reduce(partialResults, reducer);
    }
}
