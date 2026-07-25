namespace MathVerse.Math.Compiler.Tensor;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

/// <summary>Represents a compiled tensor kernel. Immutable record-like class with parameters, execution logic, and FLOPs estimate.</summary>
public sealed class TensorKernel
{
    /// <summary>The unique name of this kernel.</summary>
    public string Name { get; }
    /// <summary>Parameter names and their shapes (list of dimensions).</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<int>> Parameters { get; }
    /// <summary>Estimated floating-point operations count.</summary>
    public double FLOPs { get; }
    /// <summary>The internal execution delegate.</summary>
    private readonly Action<IReadOnlyDictionary<string, Array>> _execute;

    /// <summary>Initializes a new instance of the <see cref="TensorKernel"/> class.</summary>
    public TensorKernel(string name, IReadOnlyDictionary<string, IReadOnlyList<int>> parameters, Action<IReadOnlyDictionary<string, Array>> execute, double flops = 0)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        FLOPs = flops >= 0 ? flops : throw new ArgumentOutOfRangeException(nameof(flops));
    }

    /// <summary>Executes the kernel with the given named tensor arguments.</summary>
    /// <param name="arguments">Tensors keyed by parameter name.</param>
    public void Execute(IReadOnlyDictionary<string, Array> arguments)
    {
        if (arguments is null) throw new ArgumentNullException(nameof(arguments));
        _execute(arguments);
    }

    /// <summary>Returns a human-readable summary.</summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Name).Append(" (");
        var first = true;
        foreach (var kv in Parameters)
        {
            if (!first) sb.Append(", ");
            first = false;
            sb.Append(kv.Key).Append('[').Append(string.Join("x", kv.Value)).Append(']');
        }
        sb.Append(") ~").Append(FLOPs).Append(" FLOPs");
        return sb.ToString();
    }
}

/// <summary>Simple kernel registry for retrieving compiled kernels.</summary>
public sealed class TensorKernelRegistry
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, TensorKernel> _kernels = new();

    /// <summary>Registers a kernel.</summary>
    public void Register(TensorKernel kernel)
    {
        if (kernel is null) throw new ArgumentNullException(nameof(kernel));
        _kernels[kernel.Name] = kernel;
    }

    /// <summary>Tries to get a registered kernel by name.</summary>
    public bool TryGet(string name, out TensorKernel? kernel)
    {
        return _kernels.TryGetValue(name, out kernel);
    }

    /// <summary>Returns the number of registered kernels.</summary>
    public int Count => _kernels.Count;
}
