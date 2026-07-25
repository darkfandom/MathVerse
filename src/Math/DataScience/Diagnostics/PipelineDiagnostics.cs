namespace MathVerse.Math.DataScience.Diagnostics;

using System;
using System.Collections.Generic;

/// <summary>
/// Tracks and reports on the execution of data processing pipeline steps.
/// </summary>
public sealed class PipelineDiagnostics
{
    /// <summary>
    /// Represents a single recorded pipeline step execution.
    /// </summary>
    public sealed class StepRecord
    {
        /// <summary>
        /// Gets or sets the name of the pipeline step.
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time elapsed for this step.
        /// </summary>
        public TimeSpan Elapsed { get; set; }

        /// <summary>
        /// Gets or sets the number of input rows.
        /// </summary>
        public int InputRows { get; set; }

        /// <summary>
        /// Gets or sets the number of output rows.
        /// </summary>
        public int OutputRows { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the step was executed.
        /// </summary>
        public DateTimeOffset ExecutedAt { get; set; }

        /// <summary>
        /// Gets the row filter ratio (output / input).
        /// </summary>
        public double RowRatio => InputRows > 0 ? (double)OutputRows / InputRows : 0.0;

        /// <summary>
        /// Gets the number of rows removed (input - output).
        /// </summary>
        public int RowsRemoved => InputRows - OutputRows;
    }

    /// <summary>
    /// Represents the complete pipeline diagnostic report.
    /// </summary>
    public sealed class PipelineReport
    {
        /// <summary>
        /// Gets or sets the list of all recorded step executions.
        /// </summary>
        public List<StepRecord> Steps { get; set; } = new();

        /// <summary>
        /// Gets or sets the total elapsed time across all steps.
        /// </summary>
        public TimeSpan TotalElapsed { get; set; }

        /// <summary>
        /// Gets or sets the initial row count before the pipeline.
        /// </summary>
        public int InitialRowCount { get; set; }

        /// <summary>
        /// Gets or sets the final row count after the pipeline.
        /// </summary>
        public int FinalRowCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of steps executed.
        /// </summary>
        public int StepCount => Steps.Count;

        /// <summary>
        /// Gets or sets the peak processing time across all steps.
        /// </summary>
        public TimeSpan SlowestStep { get; set; }

        /// <summary>
        /// Gets or sets the name of the slowest step.
        /// </summary>
        public string SlowestStepName { get; set; } = string.Empty;
    }

    private readonly List<StepRecord> _records = new();
    private int _initialRowCount;

    /// <summary>
    /// Gets the number of recorded steps.
    /// </summary>
    public int RecordCount => _records.Count;

    /// <summary>
    /// Sets the initial row count before the pipeline starts.
    /// </summary>
    /// <param name="rowCount">The initial row count.</param>
    public void SetInitialRowCount(int rowCount)
    {
        _initialRowCount = rowCount;
    }

    /// <summary>
    /// Records the execution of a pipeline step.
    /// </summary>
    /// <param name="stepName">The name of the step.</param>
    /// <param name="elapsed">The time elapsed for the step.</param>
    /// <param name="inputRows">The number of rows entering the step.</param>
    /// <param name="outputRows">The number of rows leaving the step.</param>
    public void RecordStep(string stepName, TimeSpan elapsed, int inputRows, int outputRows)
    {
        if (string.IsNullOrEmpty(stepName)) throw new ArgumentException("Step name cannot be null or empty.", nameof(stepName));

        _records.Add(new StepRecord
        {
            StepName = stepName,
            Elapsed = elapsed,
            InputRows = inputRows,
            OutputRows = outputRows,
            ExecutedAt = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Gets the complete diagnostic report for all recorded pipeline steps.
    /// </summary>
    /// <returns>A <see cref="PipelineReport"/> containing all recorded data.</returns>
    public PipelineReport GetReport()
    {
        TimeSpan totalElapsed = TimeSpan.Zero;
        TimeSpan slowest = TimeSpan.Zero;
        string slowestName = string.Empty;
        int finalRows = _initialRowCount;

        for (int i = 0; i < _records.Count; i++)
        {
            StepRecord record = _records[i];
            totalElapsed += record.Elapsed;

            if (record.Elapsed > slowest)
            {
                slowest = record.Elapsed;
                slowestName = record.StepName;
            }

            finalRows = record.OutputRows;
        }

        return new PipelineReport
        {
            Steps = new List<StepRecord>(_records),
            TotalElapsed = totalElapsed,
            InitialRowCount = _initialRowCount,
            FinalRowCount = finalRows,
            SlowestStep = slowest,
            SlowestStepName = slowestName
        };
    }

    /// <summary>
    /// Clears all recorded step data.
    /// </summary>
    public void Clear()
    {
        _records.Clear();
        _initialRowCount = 0;
    }
}
