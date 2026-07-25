namespace MathVerse.Math.Numerics.FloatingPoint;

public readonly struct FloatingPointContext : IEquatable<FloatingPointContext>
{
    public readonly RoundingMode RoundingMode;
    public readonly bool FlushToZero;
    public readonly bool FlushDenormalsToZero;
    public readonly bool TrapOverflow;
    public readonly bool TrapUnderflow;
    public readonly bool TrapInvalid;
    public readonly bool TrapDivideByZero;
    public readonly bool TrapInexact;

    public FloatingPointContext(
        RoundingMode roundingMode = RoundingMode.ToNearest,
        bool flushToZero = false,
        bool flushDenormalsToZero = false,
        bool trapOverflow = false,
        bool trapUnderflow = false,
        bool trapInvalid = false,
        bool trapDivideByZero = false,
        bool trapInexact = false)
    {
        RoundingMode = roundingMode;
        FlushToZero = flushToZero;
        FlushDenormalsToZero = flushDenormalsToZero;
        TrapOverflow = trapOverflow;
        TrapUnderflow = trapUnderflow;
        TrapInvalid = trapInvalid;
        TrapDivideByZero = trapDivideByZero;
        TrapInexact = trapInexact;
    }

    public static FloatingPointContext Default => new();

    public static FloatingPointContext Strict => new(
        RoundingMode.ToNearest,
        flushToZero: false,
        flushDenormalsToZero: false,
        trapOverflow: true,
        trapUnderflow: true,
        trapInvalid: true,
        trapDivideByZero: true,
        trapInexact: true);

    public static FloatingPointContext Fast => new(
        RoundingMode.TowardZero,
        flushToZero: true,
        flushDenormalsToZero: true,
        trapOverflow: false,
        trapUnderflow: false,
        trapInvalid: false,
        trapDivideByZero: false,
        trapInexact: false);

    public static FloatingPointContext StrictBankers => new(
        RoundingMode.Bankers,
        flushToZero: false,
        flushDenormalsToZero: false,
        trapOverflow: true,
        trapUnderflow: true,
        trapInvalid: true,
        trapDivideByZero: true,
        trapInexact: true);

    public static FloatingPointContext FastFlush => new(
        RoundingMode.TowardZero,
        flushToZero: true,
        flushDenormalsToZero: true,
        trapOverflow: false,
        trapUnderflow: false,
        trapInvalid: false,
        trapDivideByZero: false,
        trapInexact: false);

    public FloatingPointContext WithRounding(RoundingMode mode) => new(mode, FlushToZero, FlushDenormalsToZero, TrapOverflow, TrapUnderflow, TrapInvalid, TrapDivideByZero, TrapInexact);

    public FloatingPointContext WithFlushToZero(bool value) => new(RoundingMode, value, FlushDenormalsToZero, TrapOverflow, TrapUnderflow, TrapInvalid, TrapDivideByZero, TrapInexact);

    public FloatingPointContext WithFlushDenormalsToZero(bool value) => new(RoundingMode, FlushToZero, value, TrapOverflow, TrapUnderflow, TrapInvalid, TrapDivideByZero, TrapInexact);

    public FloatingPointContext WithTrapOverflow(bool value) => new(RoundingMode, FlushToZero, FlushDenormalsToZero, value, TrapUnderflow, TrapInvalid, TrapDivideByZero, TrapInexact);

    public FloatingPointContext WithTrapUnderflow(bool value) => new(RoundingMode, FlushToZero, FlushDenormalsToZero, TrapOverflow, value, TrapInvalid, TrapDivideByZero, TrapInexact);

    public FloatingPointContext WithTrapInvalid(bool value) => new(RoundingMode, FlushToZero, FlushDenormalsToZero, TrapOverflow, TrapUnderflow, value, TrapDivideByZero, TrapInexact);

    public FloatingPointContext WithTrapDivideByZero(bool value) => new(RoundingMode, FlushToZero, FlushDenormalsToZero, TrapOverflow, TrapUnderflow, TrapInvalid, value, TrapInexact);

    public FloatingPointContext WithTrapInexact(bool value) => new(RoundingMode, FlushToZero, FlushDenormalsToZero, TrapOverflow, TrapUnderflow, TrapInvalid, TrapDivideByZero, value);

    public bool Equals(FloatingPointContext other)
        => RoundingMode == other.RoundingMode
        && FlushToZero == other.FlushToZero
        && FlushDenormalsToZero == other.FlushDenormalsToZero
        && TrapOverflow == other.TrapOverflow
        && TrapUnderflow == other.TrapUnderflow
        && TrapInvalid == other.TrapInvalid
        && TrapDivideByZero == other.TrapDivideByZero
        && TrapInexact == other.TrapInexact;

    public override bool Equals(object? obj) => obj is FloatingPointContext other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(RoundingMode, FlushToZero, FlushDenormalsToZero, TrapOverflow, TrapUnderflow, TrapInvalid, TrapDivideByZero, TrapInexact);

    public static bool operator ==(FloatingPointContext left, FloatingPointContext right) => left.Equals(right);

    public static bool operator !=(FloatingPointContext left, FloatingPointContext right) => !left.Equals(right);
}