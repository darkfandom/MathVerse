namespace MathVerse.Math.Geometry;

/// <summary>
/// Represents the outcome of a geometry operation, including success status and diagnostic information.
/// </summary>
public sealed record GeometryResult
{
    /// <summary>Gets a value indicating whether the operation completed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the error message describing the failure, or <c>null</c> when the operation succeeded.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets the diagnostic type associated with this result.</summary>
    public GeometryDiagnosticType DiagnosticType { get; init; }

    /// <summary>Creates a result representing a successful operation.</summary>
    /// <returns>A <see cref="GeometryResult"/> with <see cref="Success"/> set to <c>true</c>.</returns>
    public static GeometryResult Ok() => new() { Success = true };

    /// <summary>
    /// Creates a result representing a failed operation.
    /// </summary>
    /// <param name="message">A message describing the failure.</param>
    /// <param name="type">The diagnostic type. Defaults to <see cref="GeometryDiagnosticType.General"/>.</param>
    /// <returns>A <see cref="GeometryResult"/> with <see cref="Success"/> set to <c>false</c>.</returns>
    public static GeometryResult Failure(string message, GeometryDiagnosticType type = GeometryDiagnosticType.General)
        => new() { Success = false, ErrorMessage = message, DiagnosticType = type };
}
