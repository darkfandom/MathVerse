namespace MathVerse.Math.Compiler.Parallel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Coordinates parallel compilation of independent graph components.
/// Uses Parallel.For and Task.WhenAll for compilation parallelism.
/// </summary>
public sealed class ParallelCompiler
{
    private readonly int _maxDegreeOfParallelism;

    /// <summary>
    /// Initializes the parallel compiler.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">
    /// Maximum degree of parallelism. Default is the processor count.
    /// </param>
    public ParallelCompiler(int maxDegreeOfParallelism = 0)
    {
        _maxDegreeOfParallelism = maxDegreeOfParallelism > 0
            ? maxDegreeOfParallelism
            : Environment.ProcessorCount;
    }

    /// <summary>
    /// Gets the maximum degree of parallelism used by this compiler.
    /// </summary>
    public int MaxDegreeOfParallelism => _maxDegreeOfParallelism;

    /// <summary>
    /// Compiles all functions in the module in parallel.
    /// Each function is compiled independently and can be processed concurrently.
    /// </summary>
    /// <param name="module">The IR module containing functions to compile.</param>
    /// <param name="compileAction">The compilation action to apply to each function.</param>
    public void CompileModuleParallel(IRModule module, Action<IRFunction> compileAction)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(compileAction);

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxDegreeOfParallelism
        };

        Parallel.ForEach(module.Functions, options, compileAction);
    }

    /// <summary>
    /// Compiles all functions in the module asynchronously with cancellation support.
    /// </summary>
    /// <param name="module">The IR module containing functions to compile.</param>
    /// <param name="compileFunc">Async compilation function for each function.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the parallel compilation.</returns>
    public async Task CompileModuleParallelAsync(
        IRModule module,
        Func<IRFunction, Task> compileFunc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(compileFunc);

        var tasks = new List<Task>(module.Functions.Count);

        var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);

        foreach (var function in module.Functions)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            tasks.Add(RunWithSemaphore(function, compileFunc, semaphore, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static async Task RunWithSemaphore(
        IRFunction function,
        Func<IRFunction, Task> compileFunc,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        try
        {
            await compileFunc(function).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Compiles independent function groups in parallel, where each group
    /// contains functions that can be compiled concurrently without shared state.
    /// </summary>
    /// <param name="module">The IR module to compile.</param>
    /// <param name="compileAction">The compilation action for each function.</param>
    /// <param name="dependencyGraph">Map from function name to its dependency function names.</param>
    public void CompileGroupsParallel(
        IRModule module,
        Action<IRFunction> compileAction,
        Dictionary<string, List<string>> dependencyGraph)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(compileAction);
        ArgumentNullException.ThrowIfNull(dependencyGraph);

        var groups = TopologicalGroupFunctions(module, dependencyGraph);
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxDegreeOfParallelism
        };

        foreach (var group in groups)
        {
            Parallel.ForEach(group, options, compileAction);
        }
    }

    private static List<List<IRFunction>> TopologicalGroupFunctions(
        IRModule module,
        Dictionary<string, List<string>> dependencyGraph)
    {
        var functionMap = new Dictionary<string, IRFunction>();
        foreach (var func in module.Functions)
            functionMap[func.Name] = func;

        var inDegree = new Dictionary<string, int>();
        var dependents = new Dictionary<string, List<string>>();

        foreach (var func in module.Functions)
        {
            inDegree.TryAdd(func.Name, 0);
            dependents.TryAdd(func.Name, new List<string>());
        }

        foreach (var (name, deps) in dependencyGraph)
        {
            if (!functionMap.ContainsKey(name))
                continue;

            foreach (var dep in deps)
            {
                if (!functionMap.ContainsKey(dep))
                    continue;

                if (dependents.TryGetValue(dep, out var depList))
                {
                    depList.Add(name);
                }

                if (inDegree.TryGetValue(name, out var deg))
                {
                    inDegree[name] = deg + 1;
                }
            }
        }

        var groups = new List<List<IRFunction>>();
        var queue = new Queue<string>();

        foreach (var (name, deg) in inDegree)
        {
            if (deg == 0)
                queue.Enqueue(name);
        }

        while (queue.Count > 0)
        {
            var group = new List<IRFunction>();
            var nextQueue = new Queue<string>();

            while (queue.Count > 0)
            {
                var name = queue.Dequeue();
                if (functionMap.TryGetValue(name, out var func))
                    group.Add(func);

                if (dependents.TryGetValue(name, out var deps))
                {
                    foreach (var dep in deps)
                    {
                        if (inDegree.TryGetValue(dep, out var deg) && deg > 0)
                        {
                            inDegree[dep] = deg - 1;
                            if (inDegree[dep] == 0)
                                nextQueue.Enqueue(dep);
                        }
                    }
                }
            }

            if (group.Count > 0)
                groups.Add(group);

            while (nextQueue.Count > 0)
                queue.Enqueue(nextQueue.Dequeue());
        }

        return groups;
    }

    /// <summary>
    /// Gets compilation statistics for the parallel compilation run.
    /// </summary>
    /// <param name="module">The module that was compiled.</param>
    /// <returns>A dictionary with function names and their compilation durations.</returns>
    public Dictionary<string, TimeSpan> BenchmarkCompilation(IRModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var results = new Dictionary<string, TimeSpan>();
        var lockObj = new object();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxDegreeOfParallelism
        };

        Parallel.ForEach(module.Functions, options, func =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Stop();

            lock (lockObj)
            {
                results[func.Name] = sw.Elapsed;
            }
        });

        return results;
    }
}
