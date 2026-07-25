namespace MathVerse.Math.Interop.ExpressionExchange;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Core;

/// <summary>
/// Converts between different expression formats using registered format converters.
/// </summary>
public sealed class ExpressionConverter
{
    private readonly ConcurrentDictionary<string, Func<ExpressionNode, string>> _toConverters = new();
    private readonly ConcurrentDictionary<string, Func<string, ExpressionNode>> _fromConverters = new();
    private readonly ConcurrentDictionary<string, Func<object, string, string, object>> _formatBridgeConverters = new();

    /// <summary>
    /// Gets the number of registered target format converters.
    /// </summary>
    public int RegisteredToFormats => _toConverters.Count;

    /// <summary>
    /// Gets the number of registered source format converters.
    /// </summary>
    public int RegisteredFromFormats => _fromConverters.Count;

    /// <summary>
    /// Registers a converter that produces a string in the specified target format from an <see cref="ExpressionNode"/>.
    /// </summary>
    /// <param name="targetFormat">The target format identifier (e.g., "LaTeX", "MathML").</param>
    /// <param name="converter">The converter function.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetFormat"/> or <paramref name="converter"/> is null.</exception>
    public void RegisterToConverter(string targetFormat, Func<ExpressionNode, string> converter)
    {
        _ = targetFormat ?? throw new ArgumentNullException(nameof(targetFormat));
        _ = converter ?? throw new ArgumentNullException(nameof(converter));
        _toConverters[targetFormat] = converter;
    }

    /// <summary>
    /// Registers a converter that produces an <see cref="ExpressionNode"/> from a string in the specified source format.
    /// </summary>
    /// <param name="sourceFormat">The source format identifier (e.g., "LaTeX", "MathML").</param>
    /// <param name="converter">The converter function.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceFormat"/> or <paramref name="converter"/> is null.</exception>
    public void RegisterFromConverter(string sourceFormat, Func<string, ExpressionNode> converter)
    {
        _ = sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat));
        _ = converter ?? throw new ArgumentNullException(nameof(converter));
        _fromConverters[sourceFormat] = converter;
    }

    /// <summary>
    /// Registers a bidirectional bridge converter between arbitrary object-based formats.
    /// </summary>
    /// <param name="formatId">The format identifier.</param>
    /// <param name="converter">The converter function taking (source, sourceFormat, targetFormat).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatId"/> or <paramref name="converter"/> is null.</exception>
    public void RegisterBridgeConverter(string formatId, Func<object, string, string, object> converter)
    {
        _ = formatId ?? throw new ArgumentNullException(nameof(formatId));
        _ = converter ?? throw new ArgumentNullException(nameof(converter));
        _formatBridgeConverters[formatId] = converter;
    }

    /// <summary>
    /// Converts an expression from one format to another.
    /// If the expression is an <see cref="ExpressionNode"/> and the target format has a registered converter,
    /// it is converted directly. Otherwise, a bridge converter is used if available.
    /// </summary>
    /// <param name="source">The source expression (typically an <see cref="ExpressionNode"/> or string).</param>
    /// <param name="sourceFormat">The source format identifier.</param>
    /// <param name="targetFormat">The target format identifier.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the converted expression as a string.</returns>
    public InteropResult<string> Convert(object source, string sourceFormat, string targetFormat)
    {
        if (source is null)
        {
            return InteropResult<string>.Failure("Source expression cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(sourceFormat))
        {
            return InteropResult<string>.Failure("Source format cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(targetFormat))
        {
            return InteropResult<string>.Failure("Target format cannot be null or empty.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (source is ExpressionNode node && _toConverters.TryGetValue(targetFormat, out var toConverter))
            {
                var result = toConverter(node);
                sw.Stop();
                return InteropResult<string>.Success(result, duration: sw.Elapsed);
            }

            if (source is string text && _fromConverters.TryGetValue(sourceFormat, out var fromConverter))
            {
                var intermediate = fromConverter(text);
                if (_toConverters.TryGetValue(targetFormat, out var bridgeToConverter))
                {
                    var result = bridgeToConverter(intermediate);
                    sw.Stop();
                    return InteropResult<string>.Success(result, duration: sw.Elapsed);
                }
            }

            if (_formatBridgeConverters.TryGetValue(targetFormat, out var bridge))
            {
                var result = bridge(source, sourceFormat, targetFormat);
                sw.Stop();
                return InteropResult<string>.Success(result.ToString() ?? string.Empty, duration: sw.Elapsed);
            }

            sw.Stop();
            return InteropResult<string>.Failure(
                $"No converter registered for source format '{sourceFormat}' to target format '{targetFormat}'.");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return InteropResult<string>.Failure($"Conversion failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Determines whether a converter is registered for the specified target format.
    /// </summary>
    /// <param name="targetFormat">The target format identifier.</param>
    /// <returns>True if a converter is registered.</returns>
    public bool HasToConverter(string targetFormat)
    {
        return !string.IsNullOrWhiteSpace(targetFormat) && _toConverters.ContainsKey(targetFormat);
    }

    /// <summary>
    /// Determines whether a converter is registered for the specified source format.
    /// </summary>
    /// <param name="sourceFormat">The source format identifier.</param>
    /// <returns>True if a converter is registered.</returns>
    public bool HasFromConverter(string sourceFormat)
    {
        return !string.IsNullOrWhiteSpace(sourceFormat) && _fromConverters.ContainsKey(sourceFormat);
    }

    /// <summary>
    /// Gets all registered target format identifiers.
    /// </summary>
    /// <returns>A read-only collection of format identifiers.</returns>
    public IReadOnlyCollection<string> GetRegisteredToFormats()
    {
        return _toConverters.Keys.ToArray();
    }

    /// <summary>
    /// Gets all registered source format identifiers.
    /// </summary>
    /// <returns>A read-only collection of format identifiers.</returns>
    public IReadOnlyCollection<string> GetRegisteredFromFormats()
    {
        return _fromConverters.Keys.ToArray();
    }
}
