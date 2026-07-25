namespace MathVerse.Math.DataScience.DatasetManagement;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// DataFrame with columnar storage for efficient data manipulation.
/// </summary>
public sealed class DataFrame
{
    /// <summary>
    /// Gets or sets the DataFrame name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the dictionary of column names to columns.
    /// </summary>
    public Dictionary<string, Column> Columns { get; } = new();

    /// <summary>
    /// Gets the number of rows in the DataFrame.
    /// </summary>
    public int RowCount => Columns.Count > 0 ? Columns.Values.First().Values.Length : 0;

    /// <summary>
    /// Gets the number of columns in the DataFrame.
    /// </summary>
    public int ColumnCount => Columns.Count;

    /// <summary>
    /// Gets a column by name.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <returns>The column with the specified name.</returns>
    public Column this[string columnName] => Columns[columnName];

    /// <summary>
    /// Selects specific columns from the DataFrame.
    /// </summary>
    /// <param name="names">The column names to select.</param>
    /// <returns>A new DataFrame containing only the specified columns.</returns>
    public DataFrame SelectColumns(string[] names)
    {
        var result = new DataFrame { Name = Name };
        foreach (var name in names)
        {
            if (Columns.TryGetValue(name, out var col))
            {
                result.Columns[name] = col;
            }
        }
        return result;
    }

    /// <summary>
    /// Filters rows based on a predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate applied to each row.</param>
    /// <returns>A new DataFrame containing only matching rows.</returns>
    public DataFrame Filter(Func<Row, bool> predicate)
    {
        var table = ToDataTable();
        var filtered = table.SelectRows(predicate);
        var result = new DataFrame { Name = Name };
        foreach (var kvp in Columns)
        {
            var filteredValues = filtered.Select(r =>
                r.Values.TryGetValue(kvp.Key, out var v) ? v : null);
            result.Columns[kvp.Key] = new Column(kvp.Key, kvp.Value.DataType, filteredValues);
        }
        return result;
    }

    /// <summary>
    /// Applies a function to every value in the specified column.
    /// </summary>
    /// <param name="column">The column name to map.</param>
    /// <param name="func">The mapping function.</param>
    /// <returns>A new DataFrame with the mapped column values.</returns>
    public DataFrame Map(string column, Func<object?, object?> func)
    {
        var result = new DataFrame { Name = Name };
        foreach (var kvp in Columns)
        {
            if (kvp.Key == column)
            {
                var newValues = kvp.Value.Values.Select(func);
                result.Columns[kvp.Key] = new Column(kvp.Key, kvp.Value.DataType, newValues);
            }
            else
            {
                result.Columns[kvp.Key] = kvp.Value;
            }
        }
        return result;
    }

    /// <summary>
    /// Merges this DataFrame with another DataFrame on a key column.
    /// </summary>
    /// <param name="other">The other DataFrame to merge with.</param>
    /// <param name="keyColumn">The column name to join on.</param>
    /// <returns>A new DataFrame containing the merged result.</returns>
    public DataFrame Merge(DataFrame other, string keyColumn)
    {
        var result = new DataFrame { Name = $"{Name}_{other.Name}" };

        foreach (var kvp in Columns)
        {
            result.Columns[kvp.Key] = kvp.Value;
        }
        foreach (var kvp in other.Columns)
        {
            if (kvp.Key != keyColumn && !result.Columns.ContainsKey(kvp.Key))
            {
                result.Columns[kvp.Key] = kvp.Value;
            }
        }

        var rightLookup = new Dictionary<string, List<Dictionary<string, object?>>>();
        var otherTable = other.ToDataTable();
        foreach (var row in otherTable.Rows)
        {
            if (row.Values.TryGetValue(keyColumn, out var key) && key != null)
            {
                string keyStr = key.ToString() ?? "";
                if (!rightLookup.ContainsKey(keyStr))
                {
                    rightLookup[keyStr] = new List<Dictionary<string, object?>>();
                }
                rightLookup[keyStr].Add(row.Values);
            }
        }

        var leftTable = ToDataTable();
        foreach (var leftRow in leftTable.Rows)
        {
            if (leftRow.Values.TryGetValue(keyColumn, out var leftKey) && leftKey != null)
            {
                string keyStr = leftKey.ToString() ?? "";
                if (rightLookup.TryGetValue(keyStr, out var rightRows))
                {
                    foreach (var rightRow in rightRows)
                    {
                        var merged = new Dictionary<string, object?>(leftRow.Values);
                        foreach (var kvp in rightRow)
                        {
                            if (kvp.Key != keyColumn)
                            {
                                merged[kvp.Key] = kvp.Value;
                            }
                        }
                        var newRow = new Row(result.RowCount, merged);
                        var table = result.ToDataTable();
                        table.AddRow(newRow);
                        // Rebuild columns from merged table
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Groups rows by a column and applies an aggregator to each group.
    /// </summary>
    /// <param name="column">The column to group by.</param>
    /// <param name="aggregator">The aggregator function applied to each group DataFrame.</param>
    /// <returns>A new DataFrame containing the aggregated results.</returns>
    public DataFrame GroupBy(string column, Func<DataFrame, DataFrame> aggregator)
    {
        var table = ToDataTable();
        var groups = new Dictionary<string, List<Row>>();

        foreach (var row in table.Rows)
        {
            if (row.Values.TryGetValue(column, out var key) && key != null)
            {
                string keyStr = key.ToString() ?? "";
                if (!groups.ContainsKey(keyStr))
                {
                    groups[keyStr] = new List<Row>();
                }
                groups[keyStr].Add(row);
            }
        }

        var result = new DataFrame { Name = $"{Name}_grouped" };
        bool firstGroup = true;

        foreach (var group in groups)
        {
            var groupDf = new DataFrame { Name = Name };
            foreach (var colDef in table.Schema.Columns)
            {
                var values = group.Value.Select(r =>
                    r.Values.TryGetValue(colDef.Name, out var v) ? v : null);
                groupDf.Columns[colDef.Name] = new Column(colDef.Name, colDef.Type, values);
            }

            var aggregated = aggregator(groupDf);
            if (firstGroup)
            {
                foreach (var kvp in aggregated.Columns)
                {
                    result.Columns[kvp.Key] = kvp.Value;
                }
                firstGroup = false;
            }
            else
            {
                foreach (var kvp in aggregated.Columns)
                {
                    if (result.Columns.TryGetValue(kvp.Key, out var existingCol))
                    {
                        var newValues = existingCol.Values.AddRange(kvp.Value.Values);
                        result.Columns[kvp.Key] = new Column(kvp.Key, existingCol.DataType, newValues);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Converts this DataFrame to a DataTable.
    /// </summary>
    /// <returns>A new DataTable with the same data.</returns>
    public DataTable ToDataTable()
    {
        var table = new DataTable { Name = Name };
        foreach (var kvp in Columns)
        {
            table.Schema.AddColumn(kvp.Key, kvp.Value.DataType);
        }

        int rowCount = RowCount;
        for (int i = 0; i < rowCount; i++)
        {
            var values = new Dictionary<string, object?>();
            foreach (var kvp in Columns)
            {
                values[kvp.Key] = i < kvp.Value.Values.Length ? kvp.Value.Values[i] : null;
            }
            table.AddRow(new Row(i, values));
        }

        return table;
    }

    /// <summary>
    /// Creates a DataFrame from a DataTable.
    /// </summary>
    /// <param name="table">The source DataTable.</param>
    /// <returns>A new DataFrame with the same data.</returns>
    public static DataFrame FromDataTable(DataTable table)
    {
        var df = new DataFrame { Name = table.Name };
        foreach (var colDef in table.Schema.Columns)
        {
            var values = table.Rows.Select(r =>
                r.Values.TryGetValue(colDef.Name, out var v) ? v : null);
            df.Columns[colDef.Name] = new Column(colDef.Name, colDef.Type, values);
        }
        return df;
    }
}