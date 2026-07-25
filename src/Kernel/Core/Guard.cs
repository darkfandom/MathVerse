namespace MathVerse.Core;

/// <summary>
/// Provides argument validation helpers.
/// </summary>
public static class Guard
{
    /// <summary>Ensures the specified value is not null.</summary>
    public static T NotNull<T>([System.Diagnostics.CodeAnalysis.NotNull] T? value, string parameterName)
        where T : class
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }

    /// <summary>Ensures the specified value is not null.</summary>
    public static T NotNull<T>([System.Diagnostics.CodeAnalysis.NotNull] T? value, string parameterName)
        where T : struct
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }

    /// <summary>Ensures the specified string is not null or empty.</summary>
    public static string NotNullOrEmpty([System.Diagnostics.CodeAnalysis.NotNull] string? value, string parameterName)
    {
        return string.IsNullOrEmpty(value)
            ? throw new ArgumentException("Value cannot be null or empty.", parameterName)
            : value;
    }

    /// <summary>Ensures the specified string is not null, empty, or whitespace.</summary>
    public static string NotNullOrWhiteSpace([System.Diagnostics.CodeAnalysis.NotNull] string? value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be null, empty, or whitespace.", parameterName)
            : value;
    }

    /// <summary>Ensures the specified value is not the default for its type.</summary>
    public static T NotDefault<T>(T value, string parameterName)
        where T : struct
    {
        return value.Equals(default(T))
            ? throw new ArgumentException("Value cannot be default.", parameterName)
            : value;
    }

    /// <summary>Ensures the specified collection is not null or empty.</summary>
    public static IReadOnlyCollection<T> NotNullOrEmpty<T>([System.Diagnostics.CodeAnalysis.NotNull] IReadOnlyCollection<T>? value, string parameterName)
    {
        return value is null || value.Count == 0
            ? throw new ArgumentException("Collection cannot be null or empty.", parameterName)
            : value;
    }

    /// <summary>Ensures the specified value satisfies the specified condition.</summary>
    public static T Satisfies<T>(T value, Func<T, bool> condition, string parameterName, string? message = null)
    {
        return condition(value)
            ? value
            : throw new ArgumentException(message ?? "Value does not satisfy the required condition.", parameterName);
    }

    /// <summary>Ensures the specified value is greater than the specified minimum.</summary>
    public static T GreaterThan<T>(T value, T minimum, string parameterName)
        where T : IComparable<T>
    {
        return value.CompareTo(minimum) > 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than {minimum}.");
    }

    /// <summary>Ensures the specified value is greater than or equal to the specified minimum.</summary>
    public static T GreaterThanOrEqualTo<T>(T value, T minimum, string parameterName)
        where T : IComparable<T>
    {
        return value.CompareTo(minimum) >= 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than or equal to {minimum}.");
    }

    /// <summary>Ensures the specified value is less than the specified maximum.</summary>
    public static T LessThan<T>(T value, T maximum, string parameterName)
        where T : IComparable<T>
    {
        return value.CompareTo(maximum) < 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than {maximum}.");
    }

    /// <summary>Ensures the specified value is less than or equal to the specified maximum.</summary>
    public static T LessThanOrEqualTo<T>(T value, T maximum, string parameterName)
        where T : IComparable<T>
    {
        return value.CompareTo(maximum) <= 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than or equal to {maximum}.");
    }

    /// <summary>Ensures the specified value is between the specified minimum and maximum (inclusive).</summary>
    public static T Between<T>(T value, T minimum, T maximum, string parameterName)
        where T : IComparable<T>
    {
        return value.CompareTo(minimum) >= 0 && value.CompareTo(maximum) <= 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {minimum} and {maximum}.");
    }

    /// <summary>Ensures the specified value is one of the specified valid values.</summary>
    public static T OneOf<T>(T value, IReadOnlyCollection<T> validValues, string parameterName)
        where T : IEquatable<T>
    {
        return validValues.Contains(value)
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, "Value is not one of the valid values.");
    }
}
