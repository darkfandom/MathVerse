namespace MathVerse.Math.Performance.Parallel;

/// <summary>
/// Schedules parallel expression evaluations across a collection of inputs.
/// </summary>
public sealed class EvaluationScheduler
{
    /// <summary>
    /// Evaluates a function over all inputs in parallel.
    /// </summary>
    /// <typeparam name="TInput">The type of each input.</typeparam>
    /// <typeparam name="TResult">The type of each result.</typeparam>
    /// <param name="inputs">The collection of inputs to evaluate.</param>
    /// <param name="evaluator">The evaluation function to apply to each input.</param>
    /// <param name="options">Optional parallel evaluation options.</param>
    /// <returns>A list of results in the same order as the inputs.</returns>
    public IReadOnlyList<TResult> EvaluateAll<TInput, TResult>(
        IReadOnlyList<TInput> inputs,
        Func<TInput, TResult> evaluator,
        ParallelEvaluationOptions? options = null)
    {
        if (inputs is null)
            throw new ArgumentNullException(nameof(inputs));
        if (evaluator is null)
            throw new ArgumentNullException(nameof(evaluator));

        options ??= ParallelEvaluationOptions.Default;

        var inputCount = inputs.Count;
        if (inputCount == 0)
            return [];

        var results = new TResult[inputCount];

        if (inputCount == 1 || options.MaxDegreeOfParallelism == 1)
        {
            for (var i = 0; i < inputCount; i++)
            {
                options.CancellationToken.ThrowIfCancellationRequested();
                results[i] = evaluator(inputs[i]);
            }

            return results;
        }

        if (options.Deterministic)
        {
            ExecuteDeterministic(inputs, evaluator, results, options);
        }
        else
        {
            ExecuteNondeterministic(inputs, evaluator, results, options);
        }

        return results;
    }

    /// <summary>
    /// Evaluates a function on a single input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="input">The input to evaluate.</param>
    /// <param name="evaluator">The evaluation function.</param>
    /// <returns>The result of evaluating the input.</returns>
    public TResult EvaluateSingle<TInput, TResult>(TInput input, Func<TInput, TResult> evaluator)
    {
        if (evaluator is null)
            throw new ArgumentNullException(nameof(evaluator));

        return evaluator(input);
    }

    private static void ExecuteDeterministic<TInput, TResult>(
        IReadOnlyList<TInput> inputs,
        Func<TInput, TResult> evaluator,
        TResult[] results,
        ParallelEvaluationOptions options)
    {
        var partitionCount = System.Math.Min(options.MaxDegreeOfParallelism, inputs.Count);
        var partitions = TaskPartitioner.Partition(
            Enumerable.Range(0, inputs.Count).ToList(),
            partitionCount);

        var tasks = new Task[partitions.Count];

        for (var p = 0; p < partitions.Count; p++)
        {
            var indices = partitions[p];
            tasks[p] = Task.Run(() =>
            {
                for (var i = 0; i < indices.Count; i++)
                {
                    options.CancellationToken.ThrowIfCancellationRequested();
                    results[indices[i]] = evaluator(inputs[indices[i]]);
                }
            }, options.CancellationToken);
        }

        Task.WhenAll(tasks).GetAwaiter().GetResult();
    }

    private static void ExecuteNondeterministic<TInput, TResult>(
        IReadOnlyList<TInput> inputs,
        Func<TInput, TResult> evaluator,
        TResult[] results,
        ParallelEvaluationOptions options)
    {
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
            CancellationToken = options.CancellationToken
        };

        if (options.Timeout.HasValue)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken);
            timeoutCts.CancelAfter(options.Timeout.Value);
            parallelOptions.CancellationToken = timeoutCts.Token;

            try
            {
                System.Threading.Tasks.Parallel.For(0, inputs.Count, parallelOptions, i =>
                {
                    results[i] = evaluator(inputs[i]);
                });
            }
            catch (OperationCanceledException ex) when (!options.CancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("The parallel evaluation timed out.", ex);
            }
        }
        else
        {
            System.Threading.Tasks.Parallel.For(0, inputs.Count, parallelOptions, i =>
            {
                results[i] = evaluator(inputs[i]);
            });
        }
    }
}
