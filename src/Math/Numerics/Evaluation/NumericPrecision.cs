namespace MathVerse.Math.Numerics.Evaluation;

public enum NumericPrecision
{
    Double,
    Decimal,
    Arbitrary,
    Interval,
    Exact
}

public static class NumericPrecisionExtensions
{
    public static int GetDecimalDigits(this NumericPrecision precision) => precision switch
    {
        NumericPrecision.Double => 15,
        NumericPrecision.Decimal => 28,
        NumericPrecision.Arbitrary => 50,
        NumericPrecision.Interval => 15,
        NumericPrecision.Exact => int.MaxValue,
        _ => 15
    };

    public static double GetDefaultTolerance(this NumericPrecision precision) => precision switch
    {
        NumericPrecision.Double => 1e-12,
        NumericPrecision.Decimal => 1e-25,
        NumericPrecision.Arbitrary => 1e-40,
        NumericPrecision.Interval => 1e-12,
        NumericPrecision.Exact => 0,
        _ => 1e-12
    };

    public static bool SupportsComplex(this NumericPrecision precision) => precision is NumericPrecision.Double or NumericPrecision.Decimal or NumericPrecision.Arbitrary;

    public static bool IsExact(this NumericPrecision precision) => precision == NumericPrecision.Exact;

    public static bool IsApproximate(this NumericPrecision precision) => !precision.IsExact();
}