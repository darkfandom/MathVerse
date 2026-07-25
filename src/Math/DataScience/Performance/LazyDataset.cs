namespace MathVerse.Math.DataScience.Performance;

using System;
using Core;

/// <summary>
/// Provides lazy evaluation of datasets. The computation is only executed on first access.
/// Thread-safe using <see cref="Lazy{T}"/>.
/// </summary>
public sealed class LazyDataset
{
    private readonly Lazy<Dataset> _lazy;

    /// <summary>
    /// Gets a value indicating whether the dataset has been computed.
    /// </summary>
    public bool IsComputed => _lazy.IsValueCreated;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyDataset"/> class with a deferred computation.
    /// </summary>
    /// <param name="compute">The function that computes the dataset when first accessed.</param>
    public LazyDataset(Func<Dataset> compute)
    {
        if (compute is null) throw new ArgumentNullException(nameof(compute));
        _lazy = new Lazy<Dataset>(compute, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Creates a new <see cref="LazyDataset"/> instance.
    /// </summary>
    /// <param name="compute">The function that computes the dataset when first accessed.</param>
    /// <returns>A new <see cref="LazyDataset"/> instance.</returns>
    public static LazyDataset Create(Func<Dataset> compute)
    {
        return new LazyDataset(compute);
    }

    /// <summary>
    /// Accesses the dataset. On first call, executes the deferred computation.
    /// Subsequent calls return the cached result.
    /// </summary>
    /// <returns>The computed dataset.</returns>
    public Dataset Access()
    {
        return _lazy.Value;
    }

    /// <summary>
    /// Attempts to access the dataset without triggering computation.
    /// </summary>
    /// <param name="dataset">When this method returns, contains the dataset if already computed; otherwise, null.</param>
    /// <returns>true if the dataset was already computed; otherwise, false.</returns>
    public bool TryGetComputed(out Dataset? dataset)
    {
        if (_lazy.IsValueCreated)
        {
            dataset = _lazy.Value;
            return true;
        }

        dataset = null;
        return false;
    }
}
