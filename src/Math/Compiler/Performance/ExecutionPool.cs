namespace MathVerse.Math.Compiler.Performance;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Thread pool for parallel execution of compiled kernels.</summary>
public sealed class ExecutionPool
{
    private readonly ParallelOptions _options;

    /// <summary>Initializes a new instance of the <see cref="ExecutionPool"/> class.</summary>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. Defaults to processor count.</param>
    public ExecutionPool(int? maxDegreeOfParallelism = null)
    {
        _options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount
        };
    }

    /// <summary>Executes a worker action in parallel across the specified number of iterations.</summary>
    /// <param name="worker">The action to execute. Receives the iteration index.</param>
    /// <param name="iterations">Total number of iterations to process.</param>
    public void Execute(Action<int> worker, int iterations)
    {
        if (worker is null) throw new ArgumentNullException(nameof(worker));
        if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations));
        if (iterations == 0) return;

        Parallel.For(0, iterations, _options, worker);
    }

    /// <summary>Executes a worker action asynchronously in parallel.</summary>
    public Task ExecuteAsync(Action<int> worker, int iterations)
    {
        if (worker is null) throw new ArgumentNullException(nameof(worker));
        if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations));

        return Task.Run(() => Parallel.For(0, iterations, _options, worker));
    }

    /// <summary>Returns immediately; this pool does not track per-execution completion.
    /// Use <see cref="ExecuteAsync"/> to await completion.</summary>
    public void WaitForCompletion()
    {
        Thread.Sleep(0);
    }

    /// <summary>Gets the maximum degree of parallelism for this pool.</summary>
    public int MaxDegreeOfParallelism => _options.MaxDegreeOfParallelism;
}
