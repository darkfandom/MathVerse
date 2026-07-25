namespace MathVerse.Core;

/// <summary>
/// Represents a value that may not exist due to mathematical operations.
/// Unlike Option, Maybe carries context about why the value might not exist.
/// </summary>
/// <typeparam name="T">The type of the contained value.</typeparam>
public readonly record struct Maybe<T>
{
    private readonly T? _value;
    private readonly MaybeReason _reason;

    private Maybe(T value)
    {
        IsDefined = true;
        _value = value;
        _reason = MaybeReason.Defined;
    }

    private Maybe(MaybeReason reason)
    {
        IsDefined = false;
        _value = default;
        _reason = reason;
    }

    /// <summary>Gets whether the value is defined.</summary>
    public bool IsDefined { get; }

    /// <summary>Gets whether the value is undefined.</summary>
    public bool IsUndefined => !IsDefined;

    /// <summary>Gets the reason the value is undefined.</summary>
    public MaybeReason Reason => IsDefined ? MaybeReason.Defined : _reason;

    /// <summary>Gets the contained value. Throws if undefined.</summary>
    public T Value => IsDefined
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value on undefined Maybe. Reason: {_reason}");

    /// <summary>Creates a defined Maybe containing the specified value.</summary>
    public static Maybe<T> Defined(T value) => new(value);

    /// <summary>Creates an undefined Maybe with the specified reason.</summary>
    public static Maybe<T> Undefined(MaybeReason reason) => new(reason);

    /// <summary>Creates an undefined Maybe due to division by zero.</summary>
    public static Maybe<T> DivisionByZero => Undefined(MaybeReason.DivisionByZero);

    /// <summary>Creates an undefined Maybe due to overflow.</summary>
    public static Maybe<T> Overflow => Undefined(MaybeReason.Overflow);

    /// <summary>Creates an undefined Maybe due to domain error.</summary>
    public static Maybe<T> DomainError => Undefined(MaybeReason.DomainError);

    /// <summary>Maps the contained value to a new type.</summary>
    public Maybe<U> Map<U>(Func<T, U> map) =>
        IsDefined ? Maybe<U>.Defined(map(_value!)) : Maybe<U>.Undefined(_reason);

    /// <summary>Maps the contained value to a new Maybe.</summary>
    public Maybe<U> Bind<U>(Func<T, Maybe<U>> bind) =>
        IsDefined ? bind(_value!) : Maybe<U>.Undefined(_reason);

    /// <summary>Returns the contained value or the specified default.</summary>
    public T Or(T defaultValue) =>
        IsDefined ? _value! : defaultValue;

    /// <summary>Returns the contained value or computes it from the specified function.</summary>
    public T OrGet(Func<T> defaultFactory) =>
        IsDefined ? _value! : defaultFactory();

    /// <summary>Executes the appropriate function based on Maybe state.</summary>
    public U Match<U>(Func<T, U> onDefined, Func<MaybeReason, U> onUndefined) =>
        IsDefined ? onDefined(_value!) : onUndefined(_reason);

    /// <summary>Executes the appropriate action based on Maybe state.</summary>
    public void Switch(Action<T> onDefined, Action<MaybeReason> onUndefined)
    {
        if (IsDefined) onDefined(_value!); else onUndefined(_reason);
    }

    /// <summary>Converts to a Result with an error when undefined.</summary>
    public Result<T> ToResult() =>
        IsDefined
            ? Result<T>.Success(_value!)
            : Result<T>.Failure(Error.Internal("MAYBE_UNDEFINED", $"Value is undefined. Reason: {_reason}"));

    /// <summary>Converts to an Option, discarding the reason.</summary>
    public Option<T> ToOption() =>
        IsDefined ? Option<T>.Some(_value!) : default;

    /// <summary>Implicitly converts a nullable value to a Maybe.</summary>
    public static implicit operator Maybe<T>(T? value) =>
        value is not null ? Defined(value) : Undefined(MaybeReason.NullValue);
}

/// <summary>
/// Describes why a Maybe value is undefined.
/// </summary>
public enum MaybeReason
{
    /// <summary>The value is defined.</summary>
    Defined,

    /// <summary>The value is null.</summary>
    NullValue,

    /// <summary>Division by zero occurred.</summary>
    DivisionByZero,

    /// <summary>Arithmetic overflow occurred.</summary>
    Overflow,

    /// <summary>A domain error occurred.</summary>
    DomainError,

    /// <summary>The value is out of a valid range.</summary>
    OutOfRange,

    /// <summary>The computation was undefined.</summary>
    Undefined,

    /// <summary>The computation did not converge.</summary>
    DidNotConverge,

    /// <summary>The operation was not supported.</summary>
    NotSupported
}
