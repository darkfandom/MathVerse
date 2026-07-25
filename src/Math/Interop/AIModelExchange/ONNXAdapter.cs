namespace MathVerse.Math.Interop.AIModelExchange;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MathVerse.Math.Interop.Core;

/// <summary>
/// Interoperability adapter for the ONNX model format.
/// Implements <see cref="IInteropAdapter"/> to handle serialization
/// and deserialization of ONNX model data using a dictionary-based structure.
/// </summary>
public sealed class ONNXAdapter : IInteropAdapter
{
    private const string FormatId = "onnx";
    private const string DisplayNameValue = "ONNX Model Adapter";

    /// <inheritdoc />
    public string AdapterId => FormatId;

    /// <inheritdoc />
    public string DisplayName => DisplayNameValue;

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedFormats => new[] { FormatId, "onnx-ml", "onnx-ai" };

    /// <inheritdoc />
    public Version Version => new(1, 0, 0);

    private readonly Dictionary<string, object> _modelStore = new();

    /// <summary>
    /// Stores parsed ONNX model data in the adapter's internal dictionary.
    /// </summary>
    /// <param name="key">The model identifier key.</param>
    /// <param name="data">The model data object to store.</param>
    public void StoreModel(string key, object data)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(data);
        _modelStore[key] = data;
    }

    /// <summary>
    /// Retrieves previously stored ONNX model data.
    /// </summary>
    /// <param name="key">The model identifier key.</param>
    /// <returns>The stored model data, or null if not found.</returns>
    public object? RetrieveModel(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _modelStore.TryGetValue(key, out var data);
        return data;
    }

    /// <inheritdoc />
    public bool CanHandle(string format)
    {
        if (string.IsNullOrEmpty(format)) return false;
        return format.StartsWith("onnx", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async ValueTask<InteropResult> SerializeAsync(
        object value,
        Stream stream,
        InteropOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (value == null)
        {
            return InteropResult.Failure("Value cannot be null.");
        }

        if (stream == null)
        {
            return InteropResult.Failure("Stream cannot be null.");
        }

        try
        {
            var serializer = new ModelSerializer();
            ModelDescriptor? descriptor = value as ModelDescriptor;
            if (descriptor == null)
            {
                return InteropResult.Failure($"Cannot serialize type '{value.GetType().Name}' to ONNX format.");
            }

            var bytes = serializer.SerializeModel(descriptor);
            await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
            return InteropResult.Success();
        }
        catch (Exception ex)
        {
            return InteropResult.Failure($"Serialization failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async ValueTask<InteropResult<object>> DeserializeAsync(
        Stream stream,
        string? targetType = null,
        InteropOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (stream == null)
        {
            return InteropResult<object>.Failure("Stream cannot be null.");
        }

        try
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            var data = ms.ToArray();

            var serializer = new ModelSerializer();
            var descriptor = serializer.DeserializeModel(data);
            return InteropResult<object>.Success(descriptor);
        }
        catch (Exception ex)
        {
            return InteropResult<object>.Failure($"Deserialization failed: {ex.Message}", ex);
        }
    }
}
