namespace MathVerse.Math.Interop.DataExchange;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Supports streaming dataset import and export via <see cref="IAsyncEnumerable{T}"/>.
/// </summary>
public sealed class StreamingDatasetExchange
{
    /// <summary>
    /// Reads rows from a stream asynchronously as dictionaries.
    /// </summary>
    /// <param name="stream">The input stream to read from.</param>
    /// <param name="format">The format identifier (csv, tsv, jsonlines).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An async enumerable of row dictionaries.</returns>
    public async IAsyncEnumerable<Dictionary<string, object?>> ReadStreamAsync(
        Stream stream,
        string format,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Format cannot be null or empty.", nameof(format));

        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        string? headerLine = await reader.ReadLineAsync(ct).ConfigureAwait(false);
        if (headerLine is null)
            yield break;

        char delimiter = format.ToLowerInvariant() switch
        {
            "csv" => ',',
            "tsv" => '\t',
            _ => throw new NotSupportedException($"Format '{format}' is not supported for streaming read.")
        };

        string[] headers = SplitLine(headerLine, delimiter);

        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            string? line = await reader.ReadLineAsync(ct).ConfigureAwait(false);
            if (line is null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = SplitLine(line, delimiter);
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                string value = i < fields.Length ? fields[i] : string.Empty;
                row[headers[i].Trim()] = DetectAndParseValue(value);
            }
            yield return row;
        }
    }

    /// <summary>
    /// Writes rows to a stream asynchronously.
    /// </summary>
    /// <param name="stream">The output stream to write to.</param>
    /// <param name="rows">The enumerable of row dictionaries to write.</param>
    /// <param name="format">The format identifier (csv, tsv, jsonlines).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    public async ValueTask WriteStreamAsync(
        Stream stream,
        IEnumerable<Dictionary<string, object?>> rows,
        string format,
        CancellationToken ct = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        if (rows is null)
            throw new ArgumentNullException(nameof(rows));
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Format cannot be null or empty.", nameof(format));

        char delimiter = format.ToLowerInvariant() switch
        {
            "csv" => ',',
            "tsv" => '\t',
            _ => throw new NotSupportedException($"Format '{format}' is not supported for streaming write.")
        };

        using var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
        bool headerWritten = false;
        string[]? headers = null;

        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();

            if (!headerWritten)
            {
                headers = new string[row.Count];
                row.Keys.CopyTo(headers, 0);
                await writer.WriteLineAsync(string.Join(delimiter.ToString(), headers)).ConfigureAwait(false);
                headerWritten = true;
            }

            if (headers is null) continue;
            var values = new string[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                if (row.TryGetValue(headers[i], out var val) && val is not null)
                    values[i] = FormatStreamValue(val);
                else
                    values[i] = string.Empty;
            }
            await writer.WriteLineAsync(string.Join(delimiter.ToString(), values)).ConfigureAwait(false);
        }

        await writer.FlushAsync(ct).ConfigureAwait(false);
    }

    private static string[] SplitLine(string line, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                        inQuotes = false;
                }
                else
                    current.Append(c);
            }
            else
            {
                if (c == '"')
                    inQuotes = true;
                else if (c == delimiter)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                    current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    private static object? DetectAndParseValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        if (value.Equals("null", StringComparison.OrdinalIgnoreCase)) return null;
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
        if (value.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
        if (int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int n))
            return n;
        if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d))
            return d;
        return value;
    }

    private static string FormatStreamValue(object value)
    {
        return value switch
        {
            double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            int n => n.ToString(System.Globalization.CultureInfo.InvariantCulture),
            bool b => b.ToString(),
            _ => value.ToString() ?? string.Empty
        };
    }
}
