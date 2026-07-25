namespace MathVerse.Math.Types.Coercion;

/// <summary>Enumerates coercion kinds.</summary>
public enum CoercionKind
{
    /// <summary>Implicit widening (safe, lossless).</summary>
    ImplicitWidening,
    /// <summary>Implicit narrowing (may lose precision).</summary>
    ImplicitNarrowing,
    /// <summary>Explicit conversion (requires user annotation).</summary>
    Explicit,
    /// <summary>No conversion possible.</summary>
    None,
}

/// <summary>The cost of a type conversion (lower is better).</summary>
public sealed class ConversionCost : IEquatable<ConversionCost>, IComparable<ConversionCost>
{
    /// <summary>No conversion needed.</summary>
    public static readonly ConversionCost Zero = new(0);

    /// <summary>A standard widening conversion.</summary>
    public static readonly ConversionCost Widening = new(1);

    /// <summary>A standard narrowing conversion.</summary>
    public static readonly ConversionCost Narrowing = new(10);

    /// <summary>An explicit conversion.</summary>
    public static readonly ConversionCost Explicit = new(100);

    /// <summary>An impossible conversion.</summary>
    public static readonly ConversionCost Impossible = new(int.MaxValue);

    /// <summary>The cost value.</summary>
    public int Value { get; }

    /// <summary>Creates a conversion cost.</summary>
    public ConversionCost(int value)
    {
        Value = value;
    }

    /// <summary>Whether this cost is zero (no conversion).</summary>
    public bool IsZero => Value == 0;

    /// <summary>Whether conversion is possible.</summary>
    public bool IsPossible => Value < int.MaxValue;

    /// <inheritdoc/>
    public bool Equals(ConversionCost? other) =>
        other is not null && other.Value == Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ConversionCost);

    /// <inheritdoc/>
    public override int GetHashCode() => Value;

    /// <inheritdoc/>
    public int CompareTo(ConversionCost? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    /// <summary>Operator +.</summary>
    public static ConversionCost operator +(ConversionCost a, ConversionCost b) =>
        new(a.Value + b.Value);

    /// <summary>Operator &lt;.</summary>
    public static bool operator <(ConversionCost a, ConversionCost b) =>
        a.Value < b.Value;

    /// <summary>Operator &gt;.</summary>
    public static bool operator >(ConversionCost a, ConversionCost b) =>
        a.Value > b.Value;

    /// <inheritdoc/>
    public override string ToString() => Value switch
    {
        0 => "zero",
        1 => "widening",
        10 => "narrowing",
        100 => "explicit",
        int.MaxValue => "impossible",
        _ => $"cost({Value})",
    };
}

/// <summary>Represents a single coercion rule between two types.</summary>
public sealed class CoercionRule : IEquatable<CoercionRule>
{
    /// <summary>The source type.</summary>
    public MathType From { get; }

    /// <summary>The target type.</summary>
    public MathType To { get; }

    /// <summary>The coercion kind.</summary>
    public CoercionKind Kind { get; }

    /// <summary>The cost of this coercion.</summary>
    public ConversionCost Cost { get; }

    /// <summary>Whether this coercion is implicit.</summary>
    public bool IsImplicit => Kind == CoercionKind.ImplicitWidening || Kind == CoercionKind.ImplicitNarrowing;

    /// <summary>Creates a coercion rule.</summary>
    public CoercionRule(MathType from, MathType to, CoercionKind kind, ConversionCost cost)
    {
        From = from;
        To = to;
        Kind = kind;
        Cost = cost;
    }

    /// <inheritdoc/>
    public bool Equals(CoercionRule? other) =>
        other is not null && other.From.Equals(From) && other.To.Equals(To);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as CoercionRule);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(From, To);

    /// <inheritdoc/>
    public override string ToString() => $"{From.Name} →{Kind} {To.Name} ({Cost})";
}

/// <summary>Represents a coercion conversion between two types.</summary>
public sealed class ImplicitConversion : IEquatable<ImplicitConversion>
{
    /// <summary>The source type.</summary>
    public MathType From { get; }

    /// <summary>The target type.</summary>
    public MathType To { get; }

    /// <summary>The cost.</summary>
    public ConversionCost Cost { get; }

    /// <summary>Creates an implicit conversion.</summary>
    public ImplicitConversion(MathType from, MathType to, ConversionCost cost)
    {
        From = from;
        To = to;
        Cost = cost;
    }

    /// <inheritdoc/>
    public bool Equals(ImplicitConversion? other) =>
        other is not null && other.From.Equals(From) && other.To.Equals(To);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ImplicitConversion);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(From, To);
}

/// <summary>Represents an explicit (cast) conversion between two types.</summary>
public sealed class ExplicitConversion : IEquatable<ExplicitConversion>
{
    /// <summary>The source type.</summary>
    public MathType From { get; }

    /// <summary>The target type.</summary>
    public MathType To { get; }

    /// <summary>Whether data may be lost.</summary>
    public bool MayLoseData { get; }

    /// <summary>Creates an explicit conversion.</summary>
    public ExplicitConversion(MathType from, MathType to, bool mayLoseData = true)
    {
        From = from;
        To = to;
        MayLoseData = mayLoseData;
    }

    /// <inheritdoc/>
    public bool Equals(ExplicitConversion? other) =>
        other is not null && other.From.Equals(From) && other.To.Equals(To);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ExplicitConversion);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(From, To);
}
