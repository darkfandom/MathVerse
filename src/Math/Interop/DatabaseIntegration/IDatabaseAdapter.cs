namespace MathVerse.Math.Interop.DatabaseIntegration;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core;
using DataScience.Core;

/// <summary>
/// Defines the interface for database adapters.
/// </summary>
public interface IDatabaseAdapter
{
    /// <summary>
    /// Gets the database type identifier.
    /// </summary>
    string DatabaseType { get; }

    /// <summary>
    /// Gets whether the adapter is currently connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Opens a connection to the database.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    ValueTask<InteropResult> ConnectAsync(string connectionString, CancellationToken ct = default);

    /// <summary>
    /// Closes the connection.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    ValueTask<InteropResult> DisconnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Executes a query and returns results as a list of dictionaries.
    /// </summary>
    /// <param name="query">The query string.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the row data.</returns>
    ValueTask<InteropResult<IReadOnlyList<Dictionary<string, object?>>>> QueryAsync(string query, CancellationToken ct = default);

    /// <summary>
    /// Executes a non-query command.
    /// </summary>
    /// <param name="command">The command string.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the number of affected rows.</returns>
    ValueTask<InteropResult<int>> ExecuteAsync(string command, CancellationToken ct = default);

    /// <summary>
    /// Inserts a dataset into the database.
    /// </summary>
    /// <param name="tableName">The target table name.</param>
    /// <param name="dataset">The dataset to insert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the number of rows inserted.</returns>
    ValueTask<InteropResult<int>> InsertDatasetAsync(string tableName, Dataset dataset, CancellationToken ct = default);

    /// <summary>
    /// Reads a dataset from the database.
    /// </summary>
    /// <param name="query">The query string.</param>
    /// <param name="datasetName">An optional name for the resulting dataset.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the dataset.</returns>
    ValueTask<InteropResult<Dataset>> ReadDatasetAsync(string query, string? datasetName = null, CancellationToken ct = default);
}
