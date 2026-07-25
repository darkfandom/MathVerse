namespace MathVerse.Math.DataScience.DatasetManagement;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a data table with row-based storage and a defined schema.
/// </summary>
public sealed class DataTable
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the schema defining the columns.
    /// </summary>
    public Schema Schema { get; } = new();

    /// <summary>
    /// Gets the list of rows in the table.
    /// </summary>
    public List<Row> Rows { get; } = new();

    /// <summary>
    /// Gets the number of rows in the table.
    /// </summary>
    public int Count => Rows.Count;

    /// <summary>
    /// Adds a row to the table.
    /// </summary>
    /// <param name="row">The row to add.</param>
    public void AddRow(Row row)
    {
        row.Index = Rows.Count;
        Rows.Add(row);
    }

    /// <summary>
    /// Gets the values of a specific column as a <see cref="Column"/>.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>A column containing the values from all rows.</returns>
    public Column GetColumn(string name)
    {
        var colDef = Schema.GetColumn(name);
        var values = Rows.Select(r => r.Values.TryGetValue(name, out var v) ? v : null);
        return new Column(name, colDef.Type, values);
    }

    /// <summary>
    /// Selects rows that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>A list of matching rows.</returns>
    public List<Row> SelectRows(Func<Row, bool> predicate)
    {
        return Rows.Where(predicate).ToList();
    }

    /// <summary>
    /// Sorts the rows by the specified column.
    /// </summary>
    /// <param name="column">The column name to sort by.</param>
    /// <param name="ascending">If true, sorts ascending; otherwise descending.</param>
    public void Sort(string column, bool ascending = true)
    {
        Func<Row, object?> keySelector = r =>
            r.Values.TryGetValue(column, out var v) ? v : null;

        Rows.Sort((a, b) =>
        {
            var va = keySelector(a);
            var vb = keySelector(b);
            if (va == null && vb == null) return 0;
            if (va == null) return -1;
            if (vb == null) return 1;

            int cmp = string.Compare(va.ToString(), vb.ToString(), StringComparison.Ordinal);
            return ascending ? cmp : -cmp;
        });

        for (int i = 0; i < Rows.Count; i++)
        {
            Rows[i].Index = i;
        }
    }

    /// <summary>
    /// Returns a new table containing only distinct rows.
    /// </summary>
    /// <returns>A new table with duplicate rows removed.</returns>
    public DataTable Distinct()
    {
        var result = new DataTable { Name = Name };
        foreach (var col in Schema.Columns)
        {
            result.Schema.AddColumn(col.Name, col.Type);
        }

        var seen = new HashSet<string>();
        foreach (var row in Rows)
        {
            string key = string.Join("|", row.Values.Select(v => v.Value?.ToString() ?? ""));
            if (seen.Add(key))
            {
                result.AddRow(new Row(result.Rows.Count, new Dictionary<string, object?>(row.Values)));
            }
        }

        return result;
    }
}