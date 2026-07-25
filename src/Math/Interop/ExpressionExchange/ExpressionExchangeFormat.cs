namespace MathVerse.Math.Interop.ExpressionExchange;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core;

/// <summary>
/// Defines the exchange format schema for MathVerse expressions.
/// Implements <see cref="IInteropAdapter"/> for integration with the interop registry.
/// </summary>
public sealed class ExpressionExchangeFormat : IInteropAdapter
{
    private const string FormatId = "mathverse.expression";
    private const string FormatName = "MathVerse Expression Exchange";
    private const string FormatVersionValue = "1.0.0";
    private const string FileExtension = ".mve";

    private readonly ExpressionSerializer _serializer;

    /// <summary>
    /// Gets the format identifier.
    /// </summary>
    public string FormatIdentifier => FormatId;

    /// <summary>
    /// Gets the format display name.
    /// </summary>
    public string FormatDisplayName => FormatName;

    /// <summary>
    /// Gets the format version.
    /// </summary>
    public string FormatVersion => FormatVersionValue;

    /// <summary>
    /// Gets the default file extension.
    /// </summary>
    public string DefaultFileExtension => FileExtension;

    /// <summary>
    /// Gets the capabilities of this exchange format.
    /// </summary>
    public IReadOnlyList<string> Capabilities { get; }

    /// <inheritdoc/>
    public string AdapterId => FormatId;

    /// <inheritdoc/>
    public string DisplayName => FormatName;

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedFormats => new[] { FormatId };

    /// <inheritdoc/>
    public Version Version => new(FormatVersion);

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionExchangeFormat"/> class.
    /// </summary>
    public ExpressionExchangeFormat()
    {
        _serializer = new ExpressionSerializer();
        Capabilities = new[]
        {
            "serialization",
            "deserialization",
            "binary",
            "text",
            "fingerprinting",
            "canonicalization"
        };
    }

    /// <summary>
    /// Serializes an expression node to the exchange format.
    /// </summary>
    /// <param name="expression">The expression node to serialize.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the serialized byte array.</returns>
    public InteropResult<byte[]> SerializeExpression(ExpressionNode expression)
    {
        if (expression is null)
        {
            return InteropResult<byte[]>.Failure("Expression cannot be null.");
        }

        return _serializer.Serialize(expression);
    }

    /// <summary>
    /// Deserializes an expression node from the exchange format.
    /// </summary>
    /// <param name="data">The serialized data.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the deserialized expression node.</returns>
    public InteropResult<ExpressionNode> DeserializeExpression(byte[] data)
    {
        if (data is null || data.Length == 0)
        {
            return InteropResult<ExpressionNode>.Failure("Data cannot be null or empty.");
        }

        return _serializer.Deserialize(data);
    }

    /// <summary>
    /// Writes an expression to a stream in the exchange format.
    /// </summary>
    /// <param name="expression">The expression node to write.</param>
    /// <param name="stream">The target stream.</param>
    /// <returns>An <see cref="InteropResult"/> indicating success or failure.</returns>
    public InteropResult WriteToStream(ExpressionNode expression, Stream stream)
    {
        if (expression is null)
        {
            return InteropResult.Failure("Expression cannot be null.");
        }

        if (stream is null)
        {
            return InteropResult.Failure("Stream cannot be null.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var bytes = _serializer.Serialize(expression);
            if (!bytes.IsSuccess)
            {
                return InteropResult.Failure(bytes.ErrorMessage!);
            }

            stream.Write(bytes.Value!, 0, bytes.Value!.Length);
            sw.Stop();
            return InteropResult.Success();
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult.Failure($"Stream write failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads an expression from a stream in the exchange format.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the deserialized expression node.</returns>
    public InteropResult<ExpressionNode> ReadFromStream(Stream stream)
    {
        if (stream is null)
        {
            return InteropResult<ExpressionNode>.Failure("Stream cannot be null.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            var data = memoryStream.ToArray();

            var result = _serializer.Deserialize(data);
            sw.Stop();
            return result.IsSuccess
                ? InteropResult<ExpressionNode>.Success(result.Value!, duration: sw.Elapsed)
                : InteropResult<ExpressionNode>.Failure(result.ErrorMessage!, result.Error, result.Diagnostics);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<ExpressionNode>.Failure($"Stream read failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public ValueTask<InteropResult> SerializeAsync(object value, Stream stream, InteropOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (value is not ExpressionNode node)
        {
            return new ValueTask<InteropResult>(InteropResult.Failure("Value must be an ExpressionNode."));
        }

        return new ValueTask<InteropResult>(WriteToStream(node, stream));
    }

    /// <inheritdoc/>
    public ValueTask<InteropResult<object>> DeserializeAsync(Stream stream, string? targetType = null, InteropOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = ReadFromStream(stream);
        if (result.IsSuccess)
        {
            return new ValueTask<InteropResult<object>>(InteropResult<object>.Success(result.Value!));
        }

        return new ValueTask<InteropResult<object>>(InteropResult<object>.Failure(result.ErrorMessage!, result.Error));
    }

    /// <inheritdoc/>
    public bool CanHandle(string format)
    {
        return string.Equals(format, FormatId, StringComparison.OrdinalIgnoreCase);
    }
}
