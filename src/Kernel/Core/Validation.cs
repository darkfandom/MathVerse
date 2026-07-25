namespace MathVerse.Core;

/// <summary>
/// Represents a validation result that accumulates multiple errors.
/// </summary>
/// <typeparam name="T">The type of the value being validated.</typeparam>
public sealed class Validation<T>
{
    private readonly List<Error> _errors;

    private Validation(T value)
    {
        IsValid = true;
        Value = value;
        _errors = [];
    }

    private Validation(IReadOnlyList<Error> errors)
    {
        IsValid = false;
        Value = default;
        _errors = [.. errors];
    }

    /// <summary>Gets whether the validation succeeded.</summary>
    public bool IsValid { get; }

    /// <summary>Gets whether the validation failed.</summary>
    public bool IsInvalid => !IsValid;

    /// <summary>Gets the validated value. Returns default if invalid.</summary>
    public T? Value { get; }

    /// <summary>Gets the validation errors.</summary>
    public IReadOnlyList<Error> Errors => _errors;

    /// <summary>Creates a valid result with the specified value.</summary>
    public static Validation<T> Valid(T value) => new(value);

    /// <summary>Creates an invalid result with the specified errors.</summary>
    public static Validation<T> Invalid(IReadOnlyList<Error> errors) => new(errors);

    /// <summary>Creates an invalid result with a single error.</summary>
    public static Validation<T> Invalid(Error error) => new([error]);

    /// <summary>Combines multiple validation results.</summary>
    public static Validation<T> Combine(params Validation<T>[] validations)
    {
        var errors = new List<Error>();
        T? lastValue = default;

        foreach (var validation in validations)
        {
            if (validation.IsInvalid)
                errors.AddRange(validation.Errors);
            else
                lastValue = validation.Value;
        }

        return errors.Count > 0
            ? Invalid(errors)
            : Valid(lastValue!);
    }

    /// <summary>Maps the validated value to a new type.</summary>
    public Validation<U> Map<U>(Func<T, U> map) =>
        IsValid ? Validation<U>.Valid(map(Value!)) : Validation<U>.Invalid(_errors);

    /// <summary>Maps the validated value to a new Validation.</summary>
    public Validation<U> Bind<U>(Func<T, Validation<U>> bind) =>
        IsValid ? bind(Value!) : Validation<U>.Invalid(_errors);

    /// <summary>Executes the appropriate function based on validation state.</summary>
    public U Match<U>(Func<T, U> onValid, Func<IReadOnlyList<Error>, U> onInvalid) =>
        IsValid ? onValid(Value!) : onInvalid(_errors);

    /// <summary>Executes the appropriate action based on validation state.</summary>
    public void Switch(Action<T> onValid, Action<IReadOnlyList<Error>> onInvalid)
    {
        if (IsValid) onValid(Value!); else onInvalid(_errors);
    }

    /// <summary>Converts to a Result.</summary>
    public Result<T> ToResult() =>
        IsValid
            ? Result<T>.Success(Value!)
            : Result<T>.Failure(_errors.First());
}

/// <summary>
/// Provides validation helpers.
/// </summary>
public static class Validation
{
    /// <summary>Validates the specified value using the specified condition.</summary>
    public static Validation<T> Validate<T>(T value, Func<T, bool> condition, string code, string message)
    {
        return condition(value)
            ? Validation<T>.Valid(value)
            : Validation<T>.Invalid(Error.Validation(code, message));
    }

    /// <summary>Validates the specified value using multiple rules.</summary>
    public static Validation<T> ValidateAll<T>(T value, IReadOnlyList<ValidationRule<T>> rules)
    {
        var errors = new List<Error>();

        foreach (var rule in rules)
        {
            if (!rule.Condition(value))
                errors.Add(rule.Error);
        }

        return errors.Count > 0
            ? Validation<T>.Invalid(errors)
            : Validation<T>.Valid(value);
    }
}

/// <summary>
/// Defines a validation rule.
/// </summary>
/// <typeparam name="T">The type of the value being validated.</typeparam>
public sealed record ValidationRule<T>
{
    /// <summary>Gets the validation condition.</summary>
    public required Func<T, bool> Condition { get; init; }

    /// <summary>Gets the error to return if the condition fails.</summary>
    public required Error Error { get; init; }
}
