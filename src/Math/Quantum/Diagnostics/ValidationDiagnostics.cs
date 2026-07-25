namespace MathVerse.Math.Quantum.Diagnostics;

/// <summary>
/// Collects validation results including errors and warnings for quantum operations.
/// </summary>
public sealed class ValidationDiagnostics
{
    /// <summary>
    /// Gets a value indicating whether the validation passed without errors.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Gets the list of validation warnings.
    /// </summary>
    public List<string> Warnings { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationDiagnostics"/> class.
    /// </summary>
    public ValidationDiagnostics()
    {
        IsValid = true;
        Errors = new List<string>();
        Warnings = new List<string>();
    }

    /// <summary>
    /// Adds a validation error and marks the validation as invalid.
    /// </summary>
    /// <param name="error">The error message.</param>
    public void AddError(string error)
    {
        Errors.Add(error ?? throw new ArgumentNullException(nameof(error)));
        IsValid = false;
    }

    /// <summary>
    /// Adds a validation warning.
    /// </summary>
    /// <param name="warning">The warning message.</param>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning ?? throw new ArgumentNullException(nameof(warning)));
    }

    /// <summary>
    /// Returns a summary string of the validation diagnostics.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public string GetSummary()
    {
        return $"Validation: {(IsValid ? "PASS" : "FAIL")}, {Errors.Count} errors, {Warnings.Count} warnings";
    }
}
