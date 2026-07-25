namespace MathVerse.Math.Interop.DatabaseIntegration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using DataScience.Core;

/// <summary>
/// PostgreSQL-style database adapter using an in-memory table store.
/// </summary>
public sealed class PostgreSQLAdapter : IDatabaseAdapter
{
    private readonly Dictionary<string, List<Dictionary<string, object?>>> _tables = new(StringComparer.OrdinalIgnoreCase);
    private string? _connectionString;
    private bool _connected;

    /// <inheritdoc />
    public string DatabaseType => "postgresql";

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

        var tableName = ExtractTableName(query);
        if (tableName is not null && _tables.TryGetValue(tableName, out var rows))
        {
            var snapshot = rows.Select(r => new Dictionary<string, object?>(r)).ToList();
            return new ValueTask<InteropResult<IReadOnlyList<Dictionary<string, object?>>>>(
                InteropResult<IReadOnlyList<Dictionary<string, object?>>>.Success(snapshot));
        }

        return new ValueTask<InteropResult<IReadOnlyList<Dictionary<string, object?>>>>(
            InteropResult<IReadOnlyList<Dictionary<string, object?>>>.Success(Array.Empty<Dictionary<string, object?>>()));
    }

    /// <inheritdoc />
    public ValueTask<InteropResult<int>> ExecuteAsync(string command, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = command ?? throw new ArgumentNullException(nameof(command));

        var trimmed = command.TrimStart();
        if (trimmed.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
        {
            var tableName = ExtractCreateTableName(trimmed);
            if (tableName is not null && !_tables.ContainsKey(tableName))
            {
                _tables[tableName] = new List<Dictionary<string, object?>>();
            }

            return new ValueTask<InteropResult<int>>(InteropResult<int>.Success(0));
        }

        if (trimmed.StartsWith("DROP TABLE", StringComparison.OrdinalIgnoreCase))
        {
            var tableName = ExtractDropTableName(trimmed);
            if (tableName is not null)
            {
                _tables.Remove(tableName);
            }

            return new ValueTask<InteropResult<int>>(InteropResult<int>.Success(0));
        }

        return new ValueTask<InteropResult<int>>(InteropResult<int>.Success(0));
    }

    /// <inheritdoc />
    public ValueTask<InteropResult<int>> InsertDatasetAsync(string tableName, Dataset dataset, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = tableName ?? throw new ArgumentNullException(nameof(tableName));
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        if (!_tables.TryGetValue(tableName, out var table))
        {
            table = new List<Dictionary<string, object?>>();
            _tables[tableName] = table;
        }

        var inserted = 0;
        foreach (var row in dataset.Rows)
        {
            table.Add(new Dictionary<string, object?>(row));
            inserted++;
        }

        return new ValueTask<InteropResult<int>>(InteropResult<int>.Success(inserted));
    }

    /// <inheritdoc />
    public ValueTask<InteropResult<Dataset>> ReadDatasetAsync(string query, string? datasetName = null, CancellationToken ct = default)
    {
        EnsureConnected();

        _ = query ?? throw new ArgumentNullException(nameof(query));

        var tableName = ExtractTableName(query);
        var dataset = new Dataset { Name = datasetName ?? tableName ?? "query_result" };

        if (tableName is not null && _tables.TryGetValue(tableName, out var rows))
        {
            foreach (var row in rows)
            {
                dataset.Rows.Add(new Dictionary<string, object?>(row));
            }
        }

        return new ValueTask<InteropResult<Dataset>>(InteropResult<Dataset>.Success(dataset));
    }

    private void EnsureConnected()
    {
        if (!_connected)
        {
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
        }
    }

    private static string? ExtractTableName(string query)
    {
        var upper = query.ToUpperInvariant();
        var fromIndex = upper.IndexOf(" FROM ", StringComparison.Ordinal);
        if (fromIndex < 0)
        {
            return null;
        }

        var start = fromIndex + 6;
        var end = query.IndexOf(' ', start);
        if (end < 0)
        {
            end = query.Length;
        }

        return query[start..end].Trim().Trim('"');
    }

    private static string? ExtractCreateTableName(string query)
    {
        var upper = query.ToUpperInvariant();
        var keywordIndex = upper.IndexOf("CREATE TABLE", StringComparison.Ordinal);
        if (keywordIndex < 0)
        {
            return null;
        }

        var start = keywordIndex + 13;
        if (start >= query.Length)
        {
            return null;
        }

        while (start < query.Length && query[start] == ' ')
        {
            start++;
        }

        var end = query.IndexOfAny(new[] { ' ', '(', '\t' }, start);
        if (end < 0)
        {
            end = query.Length;
        }

        return query[start..end].Trim().Trim('"');
    }

    private static string? ExtractDropTableName(string query)
    {
        var upper = query.ToUpperInvariant();
        var keywordIndex = upper.IndexOf("DROP TABLE", StringComparison.Ordinal);
        if (keywordIndex < 0)
        {
            return null;
        }

        var start = keywordIndex + 10;
        if (start >= query.Length)
        {
            return null;
        }

        while (start < query.Length && query[start] == ' ')
        {
            start++;
        }

        var end = query.IndexOf(' ', start);
        if (end < 0)
        {
            end = query.Length;
        }

        return query[start..end].Trim().Trim('"');
    }
}
