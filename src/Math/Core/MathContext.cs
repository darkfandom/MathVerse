namespace MathVerse.Math.Core;

/// <summary>
/// Encapsulates precision and tolerance settings used throughout mathematical operations.
/// An instance of this class is thread-safe and immutable after construction.
/// </summary>
public sealed class MathContext
{
    /// <summary>The default context used when no context is explicitly provided.</summary>
    public static readonly MathContext Default = new();

    /// <summary>A high-precision context with tighter tolerances.</summary>
    public static readonly MathContext HighPrecision = new(
        precisionDigits: 16,
        comparisonTolerance: 1e-14,
        zeroTolerance: 1e-15,
        maxIterations: 200);

    /// <summary>A context suitable for single-precision floating-point arithmetic.</summary>
    public static readonly MathContext SinglePrecision = new(
        precisionDigits: 7,
        comparisonTolerance: 1e-5,
        zeroTolerance: 1e-6,
        maxIterations: 50);

    /// <summary>Initializes a new <see cref="MathContext"/> with the specified parameters.</summary>
    /// <param name="precisionDigits">The number of significant digits for rounding.</param>
    /// <param name="comparisonTolerance">The absolute tolerance for floating-point comparisons.</param>
    /// <param name="zeroTolerance">The absolute tolerance below which a value is considered zero.</param>
    /// <param name="maxIterations">The maximum number of iterations for iterative algorithms.</param>
    public MathContext(
        int precisionDigits = 15,
        double comparisonTolerance = 1e-10,
        double zeroTolerance = 1e-12,
        int maxIterations = 100)
    {
        Guard.GreaterThan(precisionDigits, 0, nameof(precisionDigits));
        Guard.GreaterThan(comparisonTolerance, 0, nameof(comparisonTolerance));
        Guard.GreaterThan(zeroTolerance, 0, nameof(zeroTolerance));
        Guard.GreaterThan(maxIterations, 0, nameof(maxIterations));

        PrecisionDigits = precisionDigits;
        ComparisonTolerance = comparisonTolerance;
        ZeroTolerance = zeroTolerance;
        MaxIterations = maxIterations;
    }

    /// <summary>Gets the number of significant digits used for rounding operations.</summary>
    public int PrecisionDigits { get; }

    /// <summary>Gets the absolute tolerance used when comparing floating-point values for equality.</summary>
    public double ComparisonTolerance { get; }

    /// <summary>
    /// Gets the absolute tolerance below which a value is considered effectively zero.
    /// </summary>
    public double ZeroTolerance { get; }

    /// <summary>Gets the maximum number of iterations for iterative numerical algorithms.</summary>
    public int MaxIterations { get; }

    /// <summary>Determines whether the specified value is effectively zero within this context.</summary>
    /// <param name="value">The value to test.</param>
    /// <returns><c>true</c> if the absolute value is less than or equal to <see cref="ZeroTolerance"/>.</returns>
    public bool IsEffectivelyZero(double value) =>
        System.Math.Abs(value) <= ZeroTolerance;

    /// <summary>
    /// Determines whether two values are approximately equal within this context's comparison tolerance.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns><c>true</c> if the values differ by no more than <see cref="ComparisonTolerance"/>.</returns>
    public bool AreApproximatelyEqual(double a, double b) =>
        System.Math.Abs(a - b) <= ComparisonTolerance;

    /// <summary>Rounds a value to <see cref="PrecisionDigits"/> significant digits.</summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value.</returns>
    public double Round(double value)
    {
        if (value == 0.0) return 0.0;

        var magnitude = (int)System.Math.Floor(System.Math.Log10(System.Math.Abs(value)));
        var factor = System.Math.Pow(10, PrecisionDigits - 1 - magnitude);
        return System.Math.Round(value * factor) / factor;
    }
}
