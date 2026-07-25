namespace MathVerse.Math.Interop.DatabaseIntegration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using DataScience.Core;

/// <summary>
/// Time-series database adapter with measurement-based storage.
/// </summary>
public sealed class TimeSeriesDatabaseAdapter : IDatabaseAdapter
{
    private readonly Dictionary<string, List<TimeSeriesEntry>> _measurements = new(StringComparer.OrdinalIgnoreCase);
    private string? _connectionString;
    private bool _connected;

    /// <inheritdoc />
    public string DatabaseType => "timeseries";

    /// <inheritdoc />
    public bool IsConnected => _connected;

    /// <inheritdoc />
    public ValueTask<InteropResult> ConnectAsync(string connectionString, CancellationToken ct = default)
    {
        _ = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        if (_connected)
        {
            return new ValueTask<InteropResult>(InteropResult.Failure("Already connected."));
        }

        _connectionString = connectionString;
        _connected = true;
        return new ValueTask<InteropResult>(InteropResult.Success());
    }

    /// <inheritdoc />
    public ValueTask<InteropResult> DisconnectAsync(CancellationToken ct = default)
    {
        if (!_connected)
        {
            return new ValueTask<InteropResult>(InteropResult.Failure("Not connected."));
        }

        _connected = false;
        _connectionString = null;
        return new ValueTask<InteropResult>(InteropResult.Success());
    }

    /// <inheritdoc />
    public ValueTask<InteropResult<IReadOnlyList<Dictionary<string, object?>>>> QueryAsync(string query, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = query ?? throw new ArgumentNullException(nameof(query));

        var results = new List<Dictionary<string, object?>>();
        foreach (var kvp in _measurements)
        {
            foreach (var entry in kvp.Value)
            {
                var row = new Dictionary<string, object?>
                {
                    ["measurement"] = kvp.Key,
                    ["timestamp"] = entry.Timestamp,
                };

                foreach (var field in entry.Fields)
                {
                    row[field.Key] = field.Value;
                }

                results.Add(row);
            }
        }

        return new ValueTask<InteropResult<IReadOnlyList<Dictionary<string, object?>>>>(
            InteropResult<IReadOnlyList<Dictionary<string, object?>>>.Success(results));
    }

    /// <inheritdoc />
    public ValueTask<InteropResult<int>> ExecuteAsync(string command, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = command ?? throw new ArgumentNullException(nameof(command));

        return new ValueTask<InteropResult<int>>(InteropResult<int>.Success(0));
    }

    /// <inheritdoc />
    public ValueTask<InteropResult<int>> InsertDatasetAsync(string tableName, Dataset dataset, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = tableName ?? throw new ArgumentNullException(nameof(tableName));
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var inserted = 0;
        foreach (var row in dataset.Rows)
        {
            var timestamp = DateTimeOffset.UtcNow;
            if (row.TryGetValue("timestamp", out var tsObj) && tsObj is DateTimeOffset ts)
            {
                timestamp = ts;
            }

            var fields = new Dictionary<string, double>();
            foreach (var kvp in row)
            {
                if (string.Equals(kvp.Key, "timestamp", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (kvp.Value is double d)
                {
                    fields[kvp.Key] = d;
                }
                else if (kvp.Value is int intVal)
                {
                    fields[kvp.Key] = intVal;
                }
                else if (kvp.Value is long longVal)
                {
                    fields[kvp.Key] = longVal;
                }
                else if (kvp.Value is float floatVal)
                {
                    fields[kvp.Key] = floatVal;
                }
            }

            InsertEntry(tableName, timestamp, fields);
            inserted++;
        }

        return new ValueTask<InteropResult<int>>(InteropResult<int>.Success(inserted));
    }

    /// <inheritdoc />
    public ValueTask<InteropResult<Dataset>> ReadDatasetAsync(string query, string? datasetName = null, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = query ?? throw new ArgumentNullException(nameof(query));

        var dataset = new Dataset { Name = datasetName ?? "timeseries_result" };

        foreach (var kvp in _measurements)
        {
            foreach (var entry in kvp.Value)
            {
                var row = new Dictionary<string, object?>
                {
                    ["measurement"] = kvp.Key,
                    ["timestamp"] = entry.Timestamp,
                };

                foreach (var field in entry.Fields)
                {
                    row[field.Key] = field.Value;
                }

                dataset.Rows.Add(row);
            }
        }

        return new ValueTask<InteropResult<Dataset>>(InteropResult<Dataset>.Success(dataset));
    }

    /// <summary>
    /// Inserts a single time-series data point.
    /// </summary>
    /// <param name="measurement">The measurement name.</param>
    /// <param name="timestamp">The data point timestamp.</param>
    /// <param name="fields">The field name-value pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public ValueTask<InteropResult> InsertTimeSeriesAsync(string measurement, DateTimeOffset timestamp, Dictionary<string, double> fields, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = measurement ?? throw new ArgumentNullException(nameof(measurement));
        _ = fields ?? throw new ArgumentNullException(nameof(fields));

        InsertEntry(measurement, timestamp, fields);

        return new ValueTask<InteropResult>(InteropResult.Success());
    }

    /// <summary>
    /// Queries time-series data within a time range for a given measurement.
    /// </summary>
    /// <param name="measurement">The measurement name.</param>
    /// <param name="from">The start of the time range (inclusive).</param>
    /// <param name="to">The end of the time range (inclusive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the matching data points.</returns>
    public ValueTask<InteropResult<IReadOnlyList<Dictionary<string, object?>>>> QueryTimeSeriesAsync(string measurement, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = measurement ?? throw new ArgumentNullException(nameof(measurement));

        var results = new List<Dictionary<string, object?>>();
        if (_measurements.TryGetValue(measurement, out var entries))
        {
            foreach (var entry in entries)
            {
                if (entry.Timestamp >= from && entry.Timestamp <= to)
                {
                    var row = new Dictionary<string, object?>
                    {
                        ["measurement"] = measurement,
                        ["timestamp"] = entry.Timestamp,
                    };

                    foreach (var field in entry.Fields)
                    {
                        row[field.Key] = field.Value;
                    }

                    results.Add(row);
                }
            }
        }

        return new ValueTask<InteropResult<IReadOnlyList<Dictionary<string, object?>>>>(
            InteropResult<IReadOnlyList<Dictionary<string, object?>>>.Success(results));
    }

    private void InsertEntry(string measurement, DateTimeOffset timestamp, Dictionary<string, double> fields)
    {
        if (!_measurements.TryGetValue(measurement, out var entries))
        {
            entries = new List<TimeSeriesEntry>();
            _measurements[measurement] = entries;
        }

        entries.Add(new TimeSeriesEntry(timestamp, new Dictionary<string, double>(fields)));
        entries.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
    }

    private void EnsureConnected()
    {
        if (!_connected)
        {
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
        }
    }

    private sealed class TimeSeriesEntry
    {
        public DateTimeOffset Timestamp { get; }

        public Dictionary<string, double> Fields { get; }

        public TimeSeriesEntry(DateTimeOffset timestamp, Dictionary<string, double> fields)
        {
            Timestamp = timestamp;
            Fields = fields;
        }
    }
}
