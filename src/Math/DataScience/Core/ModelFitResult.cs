namespace MathVerse.Math.DataScience.Core;

using System;
using System.Collections.Generic;

/// <summary>
/// Result of fitting a model to a dataset.
/// </summary>
public sealed class ModelFitResult
{
    /// <summary>
    /// Gets or sets the name of the target column.
    /// </summary>
    public string TargetColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fitting method used.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fitted coefficients.
    /// </summary>
    public double[] Coefficients { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the intercept value.
    /// </summary>
    public double Intercept { get; set; }

    /// <summary>
    /// Gets or sets the R-squared value.
    /// </summary>
    public double RSquared { get; set; }

    /// <summary>
    /// Gets or sets the residual standard error.
    /// </summary>
    public double ResidualStandardError { get; set; }

    /// <summary>
    /// Gets or sets the predictions made by the model.
    /// </summary>
    public double[] Predictions { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the residuals.
    /// </summary>
    public double[] Residuals { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets additional model metadata.
    /// </summary>
    public Dictionary<string, double> Metrics { get; set; } = new();

    /// <summary>
    /// Creates a new <see cref="ModelFitResult"/> instance.
    /// </summary>
    /// <param name="targetColumn">The target column name.</param>
    /// <param name="method">The fitting method.</param>
    /// <param name="coefficients">The fitted coefficients.</param>
    /// <param name="intercept">The intercept value.</param>
    /// <returns>A new model fit result.</returns>
    public static ModelFitResult Create(string targetColumn, string method, double[] coefficients, double intercept)
    {
        return new ModelFitResult
        {
            TargetColumn = targetColumn,
            Method = method,
            Coefficients = coefficients,
            Intercept = intercept
        };
    }
}