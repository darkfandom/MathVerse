namespace MathVerse.Core;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public readonly record struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    /// <summary>Gets whether the result represents success.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets whether the result represents failure.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the success value. Throws if result is failure.</summary>
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value on a failed result.");

    /// <summary>Gets the error. Throws if result is success.</summary>
    public Error Error => IsFailure ? _error! : throw new InvalidOperationException("Cannot access Error on a successful result.");

    /// <summary>Creates a successful result with the specified value.</summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>Creates a failed result with the specified error.</summary>
    public static Result<T> Failure(Error error) => new(error);

    /// <summary>Maps the success value to a new type.</summary>
    public Result<U> Map<U>(Func<T, U> map)
    {
        return IsSuccess
            ? Result<U>.Success(map(_value!))
            : Result<U>.Failure(_error!);
    }

    /// <summary>Maps the success value to a new Result.</summary>
    public Result<U> Bind<U>(Func<T, Result<U>> bind)
    {
        return IsSuccess
            ? bind(_value!)
            : Result<U>.Failure(_error!);
    }

    /// <summary>Executes the appropriate function based on result state.</summary>
    public U Match<U>(Func<T, U> onSuccess, Func<Error, U> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }

    /// <summary>Executes the appropriate action based on result state.</summary>
    public void Switch(Action<T> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess) onSuccess(_value!); else onFailure(_error!);
    }

    /// <summary>Implicitly converts a value to a successful result.</summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>Implicitly converts an error to a failed result.</summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation that can succeed or fail (non-generic).
/// </summary>
public readonly record struct Result
{
    private readonly Error? _error;

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>Gets whether the result represents success.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets whether the result represents failure.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error. Throws if result is success.</summary>
    public Error Error => IsFailure ? _error! : throw new InvalidOperationException("Cannot access Error on a successful result.");

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, null);

    /// <summary>Creates a failed result with the specified error.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Executes the appropriate action based on result state.</summary>
    public void Switch(Action onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess) onSuccess(); else onFailure(_error!);
    }
}

/// <summary>
/// Unit type representing void in Result types.
/// </summary>
public readonly record struct Unit
{
    /// <summary>The single instance of Unit.</summary>
    public static readonly Unit Value = new();

    /// <summary>Initializes a new instance of the <see cref="Unit"/> struct.</summary>
    public Unit() { }
}
