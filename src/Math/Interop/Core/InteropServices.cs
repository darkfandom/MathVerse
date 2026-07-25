namespace MathVerse.Math.Interop.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides high-level interoperability services for converting between MathVerse types and external formats.
/// </summary>
public sealed class InteropServices
{
    private readonly InteropRegistry _registry;
    private readonly InteropConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteropServices"/> class.
    /// </summary>
    /// <param name="registry">The interop registry.</param>
    /// <param name="configuration">The configuration.</param>
    public InteropServices(InteropRegistry registry, InteropConfiguration? configuration = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _configuration = configuration ?? InteropConfiguration.CreateDefault();
    }

    /// <summary>
    /// Exports an object to a file in the specified format.
    /// </summary>
    /// <param name="value">The object to export.</param>
    /// <param name="filePath">The target file path.</param>
    /// <param name="format">The target format identifier.</param>
    /// <param name="options">Export options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async ValueTask<InteropResult> ExportToFileAsync(object value, string filePath, string format, InteropOptions? options = null, CancellationToken cancellationToken = default)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));
        _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _ = format ?? throw new ArgumentNullException(nameof(format));

        var adapter = _registry.CreateAdapter(format);
        if (adapter == null)
        {
            return InteropResult.Failure($"No adapter registered for format '{format}'.");
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            var result = await adapter.SerializeAsync(value, stream, options, cancellationToken);
            sw.Stop();
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult.Failure($"Export failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Imports an object from a file in the specified format.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="format">The source format identifier.</param>
    /// <param name="options">Import options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the imported object.</returns>
    public async ValueTask<InteropResult<object>> ImportFromFileAsync(string filePath, string format, InteropOptions? options = null, CancellationToken cancellationToken = default)
    {
        _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _ = format ?? throw new ArgumentNullException(nameof(format));

        if (!File.Exists(filePath))
        {
            return InteropResult<object>.Failure($"File not found: {filePath}");
        }

        var adapter = _registry.CreateAdapter(format);
        if (adapter == null)
        {
            return InteropResult<object>.Failure($"No adapter registered for format '{format}'.");
        }

        try
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            return await adapter.DeserializeAsync(stream, null, options, cancellationToken);
        }
        catch (Exception ex)
        {
            return InteropResult<object>.Failure($"Import failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exports an object to a byte array in the specified format.
    /// </summary>
    /// <param name="value">The object to export.</param>
    /// <param name="format">The target format identifier.</param>
    /// <param name="options">Export options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the serialized bytes.</returns>
    public async ValueTask<InteropResult<byte[]>> ExportToBytesAsync(object value, string format, InteropOptions? options = null, CancellationToken cancellationToken = default)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));
        _ = format ?? throw new ArgumentNullException(nameof(format));

        var adapter = _registry.CreateAdapter(format);
        if (adapter == null)
        {
            return InteropResult<byte[]>.Failure($"No adapter registered for format '{format}'.");
        }

        try
        {
            using var ms = new MemoryStream();
            var result = await adapter.SerializeAsync(value, ms, options, cancellationToken);
            if (!result.IsSuccess)
            {
                return InteropResult<byte[]>.Failure(result.ErrorMessage ?? "Unknown error");
            }
            return InteropResult<byte[]>.Success(ms.ToArray());
        }
        catch (Exception ex)
        {
            return InteropResult<byte[]>.Failure($"Export failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Imports an object from a byte array in the specified format.
    /// </summary>
    /// <param name="data">The source data.</param>
    /// <param name="format">The source format identifier.</param>
    /// <param name="options">Import options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the imported object.</returns>
    public async ValueTask<InteropResult<object>> ImportFromBytesAsync(byte[] data, string format, InteropOptions? options = null, CancellationToken cancellationToken = default)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data));
        _ = format ?? throw new ArgumentNullException(nameof(format));

        var adapter = _registry.CreateAdapter(format);
        if (adapter == null)
        {
            return InteropResult<object>.Failure($"No adapter registered for format '{format}'.");
        }

        try
        {
            using var ms = new MemoryStream(data, false);
            return await adapter.DeserializeAsync(ms, null, options, cancellationToken);
        }
        catch (Exception ex)
        {
            return InteropResult<object>.Failure($"Import failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Converts an object from one format to another.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    /// <param name="options">Conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the converted bytes.</returns>
    public async ValueTask<InteropResult<byte[]>> ConvertAsync(object value, string sourceFormat, string targetFormat, InteropOptions? options = null, CancellationToken cancellationToken = default)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));
        _ = sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat));
        _ = targetFormat ?? throw new ArgumentNullException(nameof(targetFormat));

        var exportResult = await ExportToBytesAsync(value, targetFormat, options, cancellationToken);
        if (!exportResult.IsSuccess)
        {
            return InteropResult<byte[]>.Failure(exportResult.ErrorMessage ?? "Export failed");
        }
        return exportResult;
    }

    /// <summary>
    /// Gets the list of registered format identifiers.
    /// </summary>
    /// <returns>A collection of format identifiers.</returns>
    public IReadOnlyCollection<string> GetRegisteredFormats()
    {
        return _registry.GetRegisteredFormats();
    }
}
