namespace MathVerse.Math.DataScience.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataCleaning;
using Diagnostics;
using VisualizationIntegration;
using StreamingAnalytics;
using DatasetManagement;
using Performance;

/// <summary>
/// Comprehensive facade providing the complete public API for the DataScience module.
/// Delegates to specialized sub-module classes for each category of operation.
/// </summary>
public sealed class DataScienceEngine
{
    private readonly IncrementalCache _cache = new();
    private readonly PipelineDiagnostics _pipelineDiagnostics = new();

    /// <summary>
    /// Gets the incremental cache used for caching computed results.
    /// </summary>
    public IncrementalCache Cache => _cache;

    /// <summary>
    /// Gets the pipeline diagnostics tracker.
    /// </summary>
    public PipelineDiagnostics PipelineDiagnostics => _pipelineDiagnostics;

    /// <summary>
    /// Loads a dataset from the specified file path.
    /// Supports CSV and JSON formats based on file extension.
    /// </summary>
    /// <param name="filePath">The path to the data file.</param>
    /// <returns>The loaded <see cref="Dataset"/>.</returns>
    public Dataset Load(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        if (!File.Exists(filePath)) throw new FileNotFoundException($"Data file not found: {filePath}", filePath);

        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        string content = File.ReadAllText(filePath);

        return extension switch
        {
            ".csv" => ParseCsv(content, Path.GetFileNameWithoutExtension(filePath)),
            ".json" => ParseJson(content, Path.GetFileNameWithoutExtension(filePath)),
            _ => throw new NotSupportedException($"File format '{extension}' is not supported.")
        };
    }

    /// <summary>
    /// Saves a dataset to the specified file path.
    /// </summary>
    /// <param name="ds">The dataset to save.</param>
    /// <param name="filePath">The destination file path.</param>
    public void Save(Dataset ds, string filePath)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        string content = extension switch
        {
            ".csv" => SerializeCsv(ds),
            ".json" => SerializeJson(ds),
            _ => throw new NotSupportedException($"File format '{extension}' is not supported.")
        };

        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// Imports data from a raw string in the specified format.
    /// </summary>
    /// <param name="rawData">The raw data string.</param>
    /// <param name="format">The data format ("csv" or "json").</param>
    /// <param name="name">The name to assign to the dataset.</param>
    /// <returns>The imported <see cref="Dataset"/>.</returns>
    public Dataset Import(string rawData, string format, string name = "")
    {
        if (string.IsNullOrEmpty(rawData)) throw new ArgumentException("Raw data cannot be null or empty.", nameof(rawData));
        if (string.IsNullOrEmpty(format)) throw new ArgumentException("Format cannot be null or empty.", nameof(format));

        return format.ToLowerInvariant() switch
        {
            "csv" => ParseCsv(rawData, name),
            "json" => ParseJson(rawData, name),
            _ => throw new NotSupportedException($"Format '{format}' is not supported.")
        };
    }

    /// <summary>
    /// Exports a dataset to a string in the specified format.
    /// </summary>
    /// <param name="ds">The dataset to export.</param>
    /// <param name="format">The output format ("csv" or "json").</param>
    /// <returns>The serialized string.</returns>
    public string Export(Dataset ds, string format)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(format)) throw new ArgumentException("Format cannot be null or empty.", nameof(format));

        return format.ToLowerInvariant() switch
        {
            "csv" => SerializeCsv(ds),
            "json" => SerializeJson(ds),
            _ => throw new NotSupportedException($"Format '{format}' is not supported.")
        };
    }

    /// <summary>
    /// Performs comprehensive analysis on a dataset, returning column statistics and quality information.
    /// </summary>
    /// <param name="ds">The dataset to analyze.</param>
    /// <returns>An <see cref="AnalysisResult"/> with statistics and quality scores.</returns>
    public AnalysisResult Analyze(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));

        DataQualityReport qualityReport = DataDiagnostics.Analyze(ds);
        Dictionary<string, ColumnStatistics> colStats = ComputeAllColumnStatistics(ds);

        AnalysisResult result = AnalysisResult.Create(ds.Name, ds.Count, ds.Schema.Columns.Count);
        result.ColumnStatistics = colStats;
        result.QualityScore = qualityReport.OverallScore;
        result.Issues = qualityReport.Issues;

        return result;
    }

    /// <summary>
    /// Cleans a dataset by removing duplicates, handling missing values, and correcting types.
    /// </summary>
    /// <param name="ds">The dataset to clean.</param>
    /// <returns>The cleaned <see cref="Dataset"/>.</returns>
    public Dataset Clean(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));

        Dataset result = MissingValueHandler.ImputeMean(ds, ds.Schema.ColumnNames.First());
        result = DuplicateRemover.RemoveExact(result);
        result = TypeCorrector.DetectAndConvert(result);

        return result;
    }

    /// <summary>
    /// Applies transformations to a dataset using a pipeline of steps.
    /// </summary>
    /// <param name="ds">The input dataset.</param>
    /// <param name="transformations">The transformation steps to apply.</param>
    /// <returns>The transformed <see cref="Dataset"/>.</returns>
    public Dataset Transform(Dataset ds, Func<Dataset, Dataset>[] transformations)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (transformations is null) throw new ArgumentNullException(nameof(transformations));

        Dataset result = ds;
        for (int i = 0; i < transformations.Length; i++)
        {
            result = transformations[i](result);
        }
        return result;
    }

    /// <summary>
    /// Normalizes the specified columns using Min-Max normalization.
    /// </summary>
    /// <param name="ds">The dataset to normalize.</param>
    /// <param name="columns">The column names to normalize.</param>
    /// <returns>The normalized <see cref="Dataset"/>.</returns>
    public Dataset Normalize(Dataset ds, string[] columns)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("Columns cannot be null or empty.", nameof(columns));

        return Normalizer.MinMax(ds, columns);
    }

    /// <summary>
    /// Standardizes the specified columns using Z-score standardization.
    /// </summary>
    /// <param name="ds">The dataset to standardize.</param>
    /// <param name="columns">The column names to standardize.</param>
    /// <returns>The standardized <see cref="Dataset"/>.</returns>
    public Dataset Standardize(Dataset ds, string[] columns)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("Columns cannot be null or empty.", nameof(columns));

        return Standardizer.ZScore(ds, columns);
    }

    /// <summary>
    /// Engineers new features from existing columns using the provided feature functions.
    /// </summary>
    /// <param name="ds">The input dataset.</param>
    /// <param name="featureName">The name for the new feature column.</param>
    /// <param name="featureFunc">The function applied to each row to compute the feature value.</param>
    /// <returns>A new <see cref="Dataset"/> with the added feature column.</returns>
    public Dataset EngineerFeatures(Dataset ds, string featureName, Func<Dictionary<string, object?>, double> featureFunc)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(featureName)) throw new ArgumentException("Feature name cannot be null or empty.", nameof(featureName));
        if (featureFunc is null) throw new ArgumentNullException(nameof(featureFunc));

        for (int i = 0; i < ds.Count; i++)
        {
            ds.Rows[i][featureName] = featureFunc(ds.Rows[i]);
        }

        if (!ds.Schema.HasColumn(featureName))
        {
            ds.Schema.AddColumn(featureName, DatasetManagement.ColumnType.Double);
        }

        return ds;
    }

    /// <summary>
    /// Computes descriptive statistics for all numeric columns in the dataset.
    /// </summary>
    /// <param name="ds">The dataset to compute statistics for.</param>
    /// <returns>A <see cref="StatisticsResult"/> with per-column statistics.</returns>
    public StatisticsResult ComputeStatistics(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));

        Dictionary<string, ColumnStatistics> colStats = ComputeAllColumnStatistics(ds);
        return StatisticsResult.Create(colStats, ds.Count, ds.Schema.Columns.Count);
    }

    /// <summary>
    /// Forecasts future values using simple exponential smoothing.
    /// </summary>
    /// <param name="ds">The dataset containing time series data.</param>
    /// <param name="column">The column containing the values to forecast.</param>
    /// <param name="horizon">The number of steps to forecast.</param>
    /// <param name="alpha">The smoothing parameter (0-1). Default is 0.3.</param>
    /// <returns>A <see cref="ForecastResult"/> with the forecasted values.</returns>
    public ForecastResult Forecast(Dataset ds, string column, int horizon, double alpha = 0.3)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));
        if (horizon < 1) throw new ArgumentOutOfRangeException(nameof(horizon), horizon, "Horizon must be at least 1.");
        if (alpha < 0.0 || alpha > 1.0) throw new ArgumentOutOfRangeException(nameof(alpha), alpha, "Alpha must be between 0 and 1.");

        List<double> values = new();
        for (int i = 0; i < ds.Count; i++)
        {
            if (ds.Rows[i].TryGetValue(column, out object? val) && val is not null && IsNumeric(val))
            {
                values.Add(Convert.ToDouble(val));
            }
        }

        if (values.Count == 0)
            return ForecastResult.Create(Array.Empty<double>(), horizon);

        double level = values[0];
        for (int t = 1; t < values.Count; t++)
        {
            level = alpha * values[t] + (1.0 - alpha) * level;
        }

        double[] forecastValues = new double[horizon];
        double[] lowerBound = new double[horizon];
        double[] upperBound = new double[horizon];

        double variance = 0.0;
        double mean = 0.0;
        for (int i = 0; i < values.Count; i++) mean += values[i];
        mean /= values.Count;
        for (int i = 0; i < values.Count; i++)
        {
            double diff = values[i] - mean;
            variance += diff * diff;
        }
        variance /= values.Count;
        double stdDev = System.Math.Sqrt(variance);

        for (int h = 0; h < horizon; h++)
        {
            forecastValues[h] = level;
            lowerBound[h] = level - 1.96 * stdDev * System.Math.Sqrt(h + 1);
            upperBound[h] = level + 1.96 * stdDev * System.Math.Sqrt(h + 1);
        }

        return new ForecastResult
        {
            Values = forecastValues,
            LowerBound = lowerBound,
            UpperBound = upperBound,
            Horizon = horizon,
            Method = "SimpleExponentialSmoothing"
        };
    }

    /// <summary>
    /// Analyzes a signal in the frequency domain using DFT.
    /// </summary>
    /// <param name="signal">The time-domain signal values.</param>
    /// <param name="sampleRate">The sample rate in Hz.</param>
    /// <returns>A <see cref="SignalAnalysisResult"/> with frequency-domain information.</returns>
    public SignalAnalysisResult AnalyzeSignal(double[] signal, double sampleRate)
    {
        if (signal is null) throw new ArgumentNullException(nameof(signal));
        if (signal.Length == 0) throw new ArgumentException("Signal cannot be empty.", nameof(signal));
        if (sampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Sample rate must be positive.");

        int n = signal.Length;
        int halfN = n / 2;

        double[] real = new double[n];
        double[] imag = new double[n];
        System.Array.Copy(signal, real, n);

        Dft(real, imag, n);

        double[] magnitude = new double[halfN];
        double[] phase = new double[halfN];
        double[] frequencies = new double[halfN];
        double[] psd = new double[halfN];

        double dominantFreq = 0.0;
        double maxMag = 0.0;
        double energy = 0.0;
        double sumSquares = 0.0;

        for (int i = 0; i < halfN; i++)
        {
            frequencies[i] = (double)i * sampleRate / n;
            magnitude[i] = System.Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
            phase[i] = System.Math.Atan2(imag[i], real[i]);
            psd[i] = magnitude[i] * magnitude[i] / n;
            energy += psd[i];

            if (magnitude[i] > maxMag && i > 0)
            {
                maxMag = magnitude[i];
                dominantFreq = frequencies[i];
            }
        }

        for (int i = 0; i < n; i++)
        {
            sumSquares += signal[i] * signal[i];
        }
        double rms = System.Math.Sqrt(sumSquares / n);

        return new SignalAnalysisResult
        {
            Magnitude = magnitude,
            Phase = phase,
            Frequencies = frequencies,
            PowerSpectralDensity = psd,
            DominantFrequency = dominantFreq,
            Energy = energy,
            Rms = rms
        };
    }

    /// <summary>
    /// Fits a linear regression model to the dataset.
    /// </summary>
    /// <param name="ds">The training dataset.</param>
    /// <param name="targetColumn">The name of the target column.</param>
    /// <param name="featureColumns">The names of the feature columns.</param>
    /// <returns>A <see cref="ModelFitResult"/> with the fitted model parameters.</returns>
    public ModelFitResult FitModel(Dataset ds, string targetColumn, string[] featureColumns)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(targetColumn)) throw new ArgumentException("Target column cannot be null or empty.", nameof(targetColumn));
        if (featureColumns is null || featureColumns.Length == 0)
            throw new ArgumentException("Feature columns cannot be null or empty.", nameof(featureColumns));

        List<double> targets = new();
        List<double[]> featuresList = new();

        for (int i = 0; i < ds.Count; i++)
        {
            Dictionary<string, object?> row = ds.Rows[i];
            if (!row.TryGetValue(targetColumn, out object? tVal) || tVal is null || !IsNumeric(tVal))
                continue;

            double[] features = new double[featureColumns.Length];
            bool allValid = true;
            for (int j = 0; j < featureColumns.Length; j++)
            {
                if (row.TryGetValue(featureColumns[j], out object? fVal) && fVal is not null && IsNumeric(fVal))
                {
                    features[j] = Convert.ToDouble(fVal);
                }
                else
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid)
            {
                targets.Add(Convert.ToDouble(tVal));
                featuresList.Add(features);
            }
        }

        if (targets.Count < featureColumns.Length + 1)
            throw new InvalidOperationException("Insufficient data points for linear regression.");

        int n = targets.Count;
        int p = featureColumns.Length;

        double[][] x = new double[n][];
        for (int i = 0; i < n; i++)
        {
            x[i] = new double[p + 1];
            x[i][0] = 1.0;
            System.Array.Copy(featuresList[i], 0, x[i], 1, p);
        }

        double[][] xt = new double[p + 1][];
        for (int j = 0; j < p + 1; j++)
        {
            xt[j] = new double[n];
            for (int i = 0; i < n; i++) xt[j][i] = x[i][j];
        }

        double[][] xtx = new double[p + 1][];
        for (int i = 0; i < p + 1; i++)
        {
            xtx[i] = new double[p + 1];
            for (int j = 0; j < p + 1; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++) sum += xt[i][k] * x[k][j];
                xtx[i][j] = sum;
            }
        }

        double[] xty = new double[p + 1];
        for (int i = 0; i < p + 1; i++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++) sum += xt[i][k] * targets[k];
            xty[i] = sum;
        }

        SolveLinearSystem(xtx, xty, p + 1);

        double intercept = xty[0];
        double[] coefficients = new double[p];
        System.Array.Copy(xty, 1, coefficients, 0, p);

        double[] predictions = new double[n];
        double[] residuals = new double[n];
        double ssRes = 0.0;
        double ssTot = 0.0;
        double meanTarget = 0.0;
        for (int i = 0; i < n; i++) meanTarget += targets[i];
        meanTarget /= n;

        for (int i = 0; i < n; i++)
        {
            double pred = intercept;
            for (int j = 0; j < p; j++) pred += coefficients[j] * featuresList[i][j];
            predictions[i] = pred;
            residuals[i] = targets[i] - pred;
            ssRes += residuals[i] * residuals[i];
            ssTot += (targets[i] - meanTarget) * (targets[i] - meanTarget);
        }

        double r2 = ssTot > 1e-10 ? 1.0 - (ssRes / ssTot) : 0.0;
        double rse = n > p + 1 ? System.Math.Sqrt(ssRes / (n - p - 1)) : 0.0;

        return new ModelFitResult
        {
            TargetColumn = targetColumn,
            Method = "LinearRegression",
            Coefficients = coefficients,
            Intercept = intercept,
            RSquared = r2,
            ResidualStandardError = rse,
            Predictions = predictions,
            Residuals = residuals
        };
    }

    /// <summary>
    /// Evaluates a fitted model against actual values.
    /// </summary>
    /// <param name="actual">The actual target values.</param>
    /// <param name="predicted">The predicted values.</param>
    /// <returns>A <see cref="ModelEvaluationResult"/> with evaluation metrics.</returns>
    public ModelEvaluationResult EvaluateModel(double[] actual, double[] predicted)
    {
        if (actual is null) throw new ArgumentNullException(nameof(actual));
        if (predicted is null) throw new ArgumentNullException(nameof(predicted));

        return ModelEvaluationResult.Create(actual, predicted);
    }

    /// <summary>
    /// Generates visualization data for the specified chart type.
    /// </summary>
    /// <param name="ds">The dataset to visualize.</param>
    /// <param name="chartType">The chart type ("histogram", "scatter", "boxplot", "heatmap", "timeseries").</param>
    /// <param name="parameters">Chart-specific parameters (column names, bin count, etc.).</param>
    /// <returns>An object containing the visualization data.</returns>
    public object Visualize(Dataset ds, string chartType, Dictionary<string, object> parameters)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(chartType)) throw new ArgumentException("Chart type cannot be null or empty.", nameof(chartType));
        if (parameters is null) throw new ArgumentNullException(nameof(parameters));

        return chartType.ToLowerInvariant() switch
        {
            "histogram" => DataVisualizer.Histogram(
                ds,
                GetRequiredString(parameters, "column"),
                GetOptionalInt(parameters, "bins", 10)),
            "scatter" => DataVisualizer.Scatter(
                ds,
                GetRequiredString(parameters, "xCol"),
                GetRequiredString(parameters, "yCol")),
            "boxplot" => DataVisualizer.BoxPlot(
                ds,
                GetRequiredStringArray(parameters, "columns")),
            "heatmap" => DataVisualizer.Heatmap(
                ds,
                GetRequiredStringArray(parameters, "columns")),
            "timeseries" => DataVisualizer.TimeSeries(
                ds,
                GetRequiredString(parameters, "timeCol"),
                GetRequiredStringArray(parameters, "valueCols")),
            _ => throw new NotSupportedException($"Chart type '{chartType}' is not supported.")
        };
    }

    /// <summary>
    /// Executes a data processing pipeline on the dataset.
    /// </summary>
    /// <param name="ds">The input dataset.</param>
    /// <param name="pipeline">The pipeline to execute.</param>
    /// <returns>The transformed <see cref="Dataset"/>.</returns>
    public Dataset ExecutePipeline(Dataset ds, DataPipeline pipeline)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));

        _pipelineDiagnostics.SetInitialRowCount(ds.Count);

        Dataset result = pipeline.Execute(ds);

        foreach (var historyEntry in pipeline.ExecutionHistory)
        {
            _pipelineDiagnostics.RecordStep(
                historyEntry.StepName,
                DateTimeOffset.UtcNow - historyEntry.ExecutedAt,
                historyEntry.RowsBefore,
                historyEntry.RowsAfter);
        }

        return result;
    }

    /// <summary>
    /// Executes distributed analytics operations using the specified aggregation function.
    /// </summary>
    /// <param name="ds">The dataset to process.</param>
    /// <param name="groupColumn">The column to partition by.</param>
    /// <param name="aggColumn">The column to aggregate.</param>
    /// <param name="aggregation">The aggregation function.</param>
    /// <returns>A dictionary of group keys to aggregated values.</returns>
    public Dictionary<string, double> ExecuteDistributed(Dataset ds, string groupColumn, string aggColumn, Func<double[], double> aggregation)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(groupColumn)) throw new ArgumentException("Group column cannot be null or empty.", nameof(groupColumn));
        if (string.IsNullOrEmpty(aggColumn)) throw new ArgumentException("Aggregation column cannot be null or empty.", nameof(aggColumn));
        if (aggregation is null) throw new ArgumentNullException(nameof(aggregation));

        return ParallelReduction.GroupAggregate(ds, groupColumn, aggColumn, aggregation);
    }

    /// <summary>
    /// Clears all caches managed by this engine.
    /// </summary>
    public void ClearCaches()
    {
        _cache.Clear();
    }

    #region Private Helpers

    private static Dictionary<string, ColumnStatistics> ComputeAllColumnStatistics(Dataset ds)
    {
        Dictionary<string, ColumnStatistics> result = new();
        for (int c = 0; c < ds.Schema.Columns.Count; c++)
        {
            string col = ds.Schema.Columns[c].Name;
            List<double> values = new();
            int missingCount = 0;

            for (int r = 0; r < ds.Count; r++)
            {
                if (ds.Rows[r].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    values.Add(Convert.ToDouble(val));
                }
                else
                {
                    missingCount++;
                }
            }

            if (values.Count == 0) continue;

            values.Sort();
            double sum = 0.0;
            for (int i = 0; i < values.Count; i++) sum += values[i];
            double mean = sum / values.Count;

            double m2 = 0.0;
            double m3 = 0.0;
            double m4 = 0.0;
            for (int i = 0; i < values.Count; i++)
            {
                double diff = values[i] - mean;
                double diff2 = diff * diff;
                m2 += diff2;
                m3 += diff2 * diff;
                m4 += diff2 * diff2;
            }

            double variance = values.Count > 1 ? m2 / (values.Count - 1) : 0.0;
            double stdDev = System.Math.Sqrt(variance);
            double skewness = stdDev > 1e-15 ? (m3 / values.Count) / (stdDev * stdDev * stdDev) : 0.0;
            double kurtosis = stdDev > 1e-15 ? (m4 / values.Count) / (variance * variance) - 3.0 : 0.0;

            double q1 = Percentile(values, 25.0);
            double q2 = Percentile(values, 50.0);
            double q3 = Percentile(values, 75.0);

            HashSet<double> distinctSet = new();
            for (int i = 0; i < values.Count; i++) distinctSet.Add(values[i]);

            result[col] = new ColumnStatistics
            {
                Mean = mean,
                Median = q2,
                StdDev = stdDev,
                Min = values[0],
                Max = values[^1],
                Q1 = q1,
                Q3 = q3,
                Skewness = skewness,
                Kurtosis = kurtosis,
                MissingCount = missingCount,
                DistinctCount = distinctSet.Count
            };
        }

        return result;
    }

    private static double Percentile(List<double> sortedValues, double percentile)
    {
        double index = (percentile / 100.0) * (sortedValues.Count - 1);
        int lower = (int)System.Math.Floor(index);
        int upper = (int)System.Math.Ceiling(index);

        if (lower == upper) return sortedValues[lower];

        double fraction = index - lower;
        return sortedValues[lower] + fraction * (sortedValues[upper] - sortedValues[lower]);
    }

    private static void SolveLinearSystem(double[][] a, double[] b, int n)
    {
        for (int col = 0; col < n; col++)
        {
            int maxRow = col;
            double maxVal = System.Math.Abs(a[col][col]);
            for (int row = col + 1; row < n; row++)
            {
                if (System.Math.Abs(a[row][col]) > maxVal)
                {
                    maxVal = System.Math.Abs(a[row][col]);
                    maxRow = row;
                }
            }

            if (maxRow != col)
            {
                (a[col], a[maxRow]) = (a[maxRow], a[col]);
                (b[col], b[maxRow]) = (b[maxRow], b[col]);
            }

            if (System.Math.Abs(a[col][col]) < 1e-15) continue;

            for (int row = col + 1; row < n; row++)
            {
                double factor = a[row][col] / a[col][col];
                for (int j = col; j < n; j++)
                {
                    a[row][j] -= factor * a[col][j];
                }
                b[row] -= factor * b[col];
            }
        }

        for (int row = n - 1; row >= 0; row--)
        {
            if (System.Math.Abs(a[row][row]) < 1e-15) continue;
            double sum = b[row];
            for (int j = row + 1; j < n; j++)
            {
                sum -= a[row][j] * b[j];
            }
            b[row] = sum / a[row][row];
        }
    }

    private static void Dft(double[] real, double[] imag, int n)
    {
        for (int k = 0; k < n; k++)
        {
            double sumReal = 0.0;
            double sumImag = 0.0;
            for (int t = 0; t < n; t++)
            {
                double angle = -2.0 * System.Math.PI * k * t / n;
                sumReal += real[t] * System.Math.Cos(angle) - imag[t] * System.Math.Sin(angle);
                sumImag += real[t] * System.Math.Sin(angle) + imag[t] * System.Math.Cos(angle);
            }
            real[k] = sumReal;
            imag[k] = sumImag;
        }
    }

    private static Dataset ParseCsv(string content, string name)
    {
        Dataset ds = new() { Name = name };
        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return ds;

        string[] headers = lines[0].Split(',');
        for (int h = 0; h < headers.Length; h++)
        {
            string header = headers[h].Trim().Trim('"');
            ds.Schema.AddColumn(header, DatasetManagement.ColumnType.String);
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            var row = new Dictionary<string, object?>();
            for (int j = 0; j < headers.Length && j < fields.Length; j++)
            {
                string key = headers[j].Trim().Trim('"');
                string val = fields[j].Trim().Trim('"');

                if (double.TryParse(val, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double numVal))
                {
                    row[key] = numVal;
                }
                else
                {
                    row[key] = val;
                }
            }
            ds.Rows.Add(row);
        }

        return ds;
    }

    private static Dataset ParseJson(string content, string name)
    {
        Dataset ds = new() { Name = name };

        content = content.Trim();
        if (content.StartsWith('[') && content.EndsWith(']'))
        {
            content = content[1..^1];
        }

        string[] objects = content.Split(new[] { "},{" }, StringSplitOptions.RemoveEmptyEntries);
        HashSet<string> headers = new();

        for (int i = 0; i < objects.Length; i++)
        {
            string obj = objects[i].Trim().Trim('{', '}');
            var row = new Dictionary<string, object?>();
            string[] pairs = obj.Split(',');
            foreach (string pair in pairs)
            {
                int colonIdx = pair.IndexOf(':');
                if (colonIdx < 0) continue;

                string key = pair[..colonIdx].Trim().Trim('"');
                string val = pair[(colonIdx + 1)..].Trim().Trim('"');

                headers.Add(key);

                if (double.TryParse(val, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double numVal))
                {
                    row[key] = numVal;
                }
                else
                {
                    row[key] = val;
                }
            }
            ds.Rows.Add(row);
        }

        foreach (string header in headers)
        {
            ds.Schema.AddColumn(header, DatasetManagement.ColumnType.String);
        }

        return ds;
    }

    private static string SerializeCsv(Dataset ds)
    {
        StringBuilder sb = new();
        int colCount = ds.Schema.Columns.Count;
        for (int c = 0; c < colCount; c++)
        {
            if (c > 0) sb.Append(',');
            sb.Append(ds.Schema.Columns[c].Name);
        }
        sb.AppendLine();

        for (int r = 0; r < ds.Count; r++)
        {
            for (int c = 0; c < colCount; c++)
            {
                if (c > 0) sb.Append(',');
                string col = ds.Schema.Columns[c].Name;
                if (ds.Rows[r].TryGetValue(col, out object? val) && val is not null)
                {
                    sb.Append(val.ToString());
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string SerializeJson(Dataset ds)
    {
        StringBuilder sb = new();
        sb.Append('[');

        for (int r = 0; r < ds.Count; r++)
        {
            if (r > 0) sb.Append(',');
            sb.Append('{');

            bool first = true;
            for (int c = 0; c < ds.Schema.Columns.Count; c++)
            {
                if (!first) sb.Append(',');
                first = false;

                string col = ds.Schema.Columns[c].Name;
                sb.Append('"');
                sb.Append(col);
                sb.Append("\":");

                if (ds.Rows[r].TryGetValue(col, out object? val) && val is not null)
                {
                    if (val is string s)
                    {
                        sb.Append('"');
                        sb.Append(s);
                        sb.Append('"');
                    }
                    else
                    {
                        sb.Append(val.ToString());
                    }
                }
                else
                {
                    sb.Append("null");
                }
            }

            sb.Append('}');
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static bool IsNumeric(object value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }

    private static string GetRequiredString(Dictionary<string, object> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out object? val) || val is null)
            throw new ArgumentException($"Required parameter '{key}' is missing.");
        return val.ToString() ?? throw new ArgumentException($"Parameter '{key}' value is null.");
    }

    private static int GetOptionalInt(Dictionary<string, object> parameters, string key, int defaultValue)
    {
        if (parameters.TryGetValue(key, out object? val) && val is int intVal)
            return intVal;
        return defaultValue;
    }

    private static string[] GetRequiredStringArray(Dictionary<string, object> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out object? val))
            throw new ArgumentException($"Required parameter '{key}' is missing.");
        if (val is string[] arr) return arr;
        if (val is string s) return new[] { s };
        if (val is List<string> list) return list.ToArray();
        throw new ArgumentException($"Parameter '{key}' must be a string array.");
    }

    #endregion
}
