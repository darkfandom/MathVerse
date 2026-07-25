namespace MathVerse.Math.Interop.Core;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Diagnostics;
using Performance;
using ScientificWorkflow;

/// <summary>
/// Comprehensive facade providing the complete public API for the Interop module.
/// Delegates to specialized sub-module classes for each category of operation.
/// </summary>
public sealed class InteropEngine
{
    private readonly InteropRegistry _registry;
    private readonly InteropServices _services;
    private readonly InteropContext _context;
    private readonly InteropConfiguration _configuration;
    private readonly InteropDiagnostics _diagnostics;
    private readonly SerializationCache _cache;

    /// <summary>
    /// Gets the interop registry.
    /// </summary>
    public InteropRegistry Registry => _registry;

    /// <summary>
    /// Gets the interop services.
    /// </summary>
    public InteropServices Services => _services;

    /// <summary>
    /// Gets the interop context.
    /// </summary>
    public InteropContext Context => _context;

    /// <summary>
    /// Gets the diagnostics collector.
    /// </summary>
    public InteropDiagnostics Diagnostics => _diagnostics;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteropEngine"/> class.
    /// </summary>
    public InteropEngine() : this(null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteropEngine"/> class with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration, or null for defaults.</param>
    public InteropEngine(InteropConfiguration? configuration)
    {
        _configuration = configuration ?? InteropConfiguration.CreateDefault();
        _registry = new InteropRegistry();
        _context = new InteropContext();
        _services = new InteropServices(_registry, _configuration);
        _diagnostics = new InteropDiagnostics();
        _cache = new SerializationCache();
    }

    /// <summary>
    /// Imports data from a file.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="format">The source format identifier.</param>
    /// <param name="options">Optional import options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the imported object.</returns>
    public ValueTask<InteropResult<object>> Import(string filePath, string format, InteropOptions? options = null, CancellationToken ct = default)
    {
        return _services.ImportFromFileAsync(filePath, format, options, ct);
    }

    /// <summary>
    /// Exports data to a file.
    /// </summary>
    /// <param name="value">The object to export.</param>
    /// <param name="filePath">The target file path.</param>
    /// <param name="format">The target format identifier.</param>
    /// <param name="options">Optional export options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public ValueTask<InteropResult> Export(object value, string filePath, string format, InteropOptions? options = null, CancellationToken ct = default)
    {
        return _services.ExportToFileAsync(value, filePath, format, options, ct);
    }

    /// <summary>
    /// Serializes an object to bytes.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="format">The target format identifier.</param>
    /// <param name="options">Optional serialization options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the serialized bytes.</returns>
    public ValueTask<InteropResult<byte[]>> Serialize(object value, string format, InteropOptions? options = null, CancellationToken ct = default)
    {
        return _services.ExportToBytesAsync(value, format, options, ct);
    }

    /// <summary>
    /// Deserializes bytes to an object.
    /// </summary>
    /// <param name="data">The source data.</param>
    /// <param name="format">The source format identifier.</param>
    /// <param name="options">Optional deserialization options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the deserialized object.</returns>
    public ValueTask<InteropResult<object>> Deserialize(byte[] data, string format, InteropOptions? options = null, CancellationToken ct = default)
    {
        return _services.ImportFromBytesAsync(data, format, options, ct);
    }

    /// <summary>
    /// Converts between formats.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="sourceFormat">The source format identifier.</param>
    /// <param name="targetFormat">The target format identifier.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the converted bytes.</returns>
    public ValueTask<InteropResult<byte[]>> Convert(object value, string sourceFormat, string targetFormat, InteropOptions? options = null, CancellationToken ct = default)
    {
        return _services.ConvertAsync(value, sourceFormat, targetFormat, options, ct);
    }

    /// <summary>
    /// Saves data to a file (alias for Export).
    /// </summary>
    /// <param name="value">The object to save.</param>
    /// <param name="filePath">The target file path.</param>
    /// <param name="format">The target format identifier.</param>
    /// <param name="options">Optional export options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public ValueTask<InteropResult> Save(object value, string filePath, string format, InteropOptions? options = null, CancellationToken ct = default)
    {
        return Export(value, filePath, format, options, ct);
    }

    /// <summary>
    /// Loads data from a file (alias for Import).
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="format">The source format identifier.</param>
    /// <param name="options">Optional import options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the loaded object.</returns>
    public ValueTask<InteropResult<object>> Load(string filePath, string format, InteropOptions? options = null, CancellationToken ct = default)
    {
        return Import(filePath, format, options, ct);
    }

    /// <summary>
    /// Registers an adapter for a format.
    /// </summary>
    /// <param name="formatId">The format identifier.</param>
    /// <param name="factory">The factory function to create adapter instances.</param>
    /// <param name="descriptor">Optional format descriptor.</param>
    public void RegisterAdapter(string formatId, Func<IInteropAdapter> factory, FormatDescriptor? descriptor = null)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        _ = factory ?? throw new ArgumentNullException(nameof(factory));
        _registry.Register(formatId, descriptor ?? new FormatDescriptor { Name = formatId }, factory);
    }

    /// <summary>
    /// Saves a workflow checkpoint.
    /// </summary>
    /// <param name="name">The checkpoint name.</param>
    /// <param name="state">The state object to persist.</param>
    public void SaveCheckpoint(string name, object state)
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));
        _ = state ?? throw new ArgumentNullException(nameof(state));
        _context.SetProperty($"checkpoint:{name}", state);
    }

    /// <summary>
    /// Restores a workflow checkpoint.
    /// </summary>
    /// <param name="name">The checkpoint name.</param>
    /// <returns>The persisted state, or null if not found.</returns>
    public object? RestoreCheckpoint(string name)
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));
        return _context.GetProperty($"checkpoint:{name}");
    }

    /// <summary>
    /// Connects to a remote execution cluster.
    /// </summary>
    /// <param name="endpoint">The remote endpoint URL.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public ValueTask<InteropResult> ConnectRemote(string endpoint, CancellationToken ct = default)
    {
        _ = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _context.SetProperty("remoteEndpoint", endpoint);
        return new ValueTask<InteropResult>(InteropResult.Success());
    }

    /// <summary>
    /// Clears all caches.
    /// </summary>
    public void ClearCaches()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Executes a workflow by invoking the supplied step executor for each step.
    /// </summary>
    /// <param name="workflow">The workflow to execute.</param>
    /// <param name="stepExecutor">A function that executes an individual step and returns its outputs.</param>
    /// <returns>A result indicating success or failure.</returns>
    public InteropResult ExecuteWorkflow(Workflow workflow, Func<WorkflowStep, Dictionary<string, object>, Dictionary<string, object>> stepExecutor)
    {
        _ = workflow ?? throw new ArgumentNullException(nameof(workflow));
        _ = stepExecutor ?? throw new ArgumentNullException(nameof(stepExecutor));
        var executor = new WorkflowExecutor();
        var result = executor.Execute(workflow, stepExecutor);
        return result.Success ? InteropResult.Success() : InteropResult.Failure(result.ErrorMessage ?? "Workflow execution failed");
    }
}
