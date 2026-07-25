namespace MathVerse.Core;

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
/// <typeparam name="T">The type of the contained value.</typeparam>
public readonly record struct Option<T>
{
    private readonly T? _value;

    private Option(T value)
    {
        IsSome = true;
        _value = value;
    }

    /// <summary>Gets whether the option contains a value.</summary>
    public bool IsSome { get; }

    /// <summary>Gets whether the option is empty.</summary>
    public bool IsNone => !IsSome;

    /// <summary>Gets the contained value. Throws if option is None.</summary>
    public T Value => IsSome ? _value! : throw new InvalidOperationException("Cannot access Value on None option.");

    /// <summary>Creates an option containing the specified value.</summary>
    public static Option<T> Some(T value) => new(value);

    /// <summary>Creates an empty option.</summary>
    public static Option<T> None => default;

    /// <summary>Creates an option from a nullable value.</summary>
    public static Option<T> FromNullable(T? value) =>
        value is not null ? new Option<T>(value) : default;

    /// <summary>Maps the contained value to a new type.</summary>
    public Option<U> Map<U>(Func<T, U> map) =>
        IsSome ? Option<U>.Some(map(_value!)) : default;

    /// <summary>Maps the contained value to a new Option.</summary>
    public Option<U> Bind<U>(Func<T, Option<U>> bind) =>
        IsSome ? bind(_value!) : default;

    /// <summary>Returns the contained value or the specified default.</summary>
    public T Or(T defaultValue) =>
        IsSome ? _value! : defaultValue;

    /// <summary>Returns the contained value or computes it from the specified function.</summary>
    public T OrGet(Func<T> defaultFactory) =>
        IsSome ? _value! : defaultFactory();

    /// <summary>Executes the appropriate function based on option state.</summary>
    public U Match<U>(Func<T, U> onSome, Func<U> onNone) =>
        IsSome ? onSome(_value!) : onNone();

    /// <summary>Executes the appropriate action based on option state.</summary>
    public void Switch(Action<T> onSome, Action onNone)
    {
        if (IsSome) onSome(_value!); else onNone();
    }

    /// <summary>Filters the option based on a predicate.</summary>
    public Option<T> Where(Func<T, bool> predicate) =>
        IsSome && predicate(_value!) ? this : default;

    /// <summary>Converts to a Result with the specified error when None.</summary>
    public Result<T> ToResult(Error error) =>
        IsSome ? Result<T>.Success(_value!) : Result<T>.Failure(error);

    /// <summary>Implicitly converts a nullable value to an Option.</summary>
    public static implicit operator Option<T>(T? value) =>
        value is not null ? Some(value) : default;
}
