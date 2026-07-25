namespace MathVerse.Math.Core;

/// <summary>
/// Static utility class providing common mathematical helper functions.
/// All methods use <see cref="System.Math"/> explicitly to avoid ambiguity with the Kernel's global static import.
/// </summary>
public static class MathHelper
{
    /// <summary>Computes the greatest common divisor of two integers.</summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>The greatest common divisor of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static int GCD(int a, int b)
    {
        a = System.Math.Abs(a);
        b = System.Math.Abs(b);

        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    /// <summary>Computes the least common multiple of two integers.</summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>The least common multiple of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static int LCM(int a, int b)
    {
        if (a == 0 || b == 0) return 0;

        return System.Math.Abs(a / GCD(a, b) * b);
    }

    /// <summary>Computes the factorial of a non-negative integer.</summary>
    /// <param name="n">A non-negative integer (must be ≤ 170 to avoid overflow).</param>
    /// <returns>n! (n factorial).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> is negative or greater than 170.</exception>
    public static double Factorial(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, 170);

        double result = 1.0;
        for (var i = 2; i <= n; i++)
            result *= i;

        return result;
    }

    /// <summary>Computes the binomial coefficient C(n, k) = n! / (k! * (n-k)!).</summary>
    /// <param name="n">The total number of items (must be ≥ 0).</param>
    /// <param name="k">The number of items to choose (must be ≥ 0 and ≤ <paramref name="n"/>).</param>
    /// <returns>The number of ways to choose <paramref name="k"/> items from <paramref name="n"/> items.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="n"/> or <paramref name="k"/> is negative, or <paramref name="k"/> exceeds <paramref name="n"/>.
    /// </exception>
    public static double Binomial(int n, int k)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        ArgumentOutOfRangeException.ThrowIfNegative(k);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(k, n);

        if (k == 0 || k == n) return 1.0;

        var kEff = System.Math.Min(k, n - k);

        double result = 1.0;
        for (var i = 0; i < kEff; i++)
        {
            result *= (n - i);
            result /= (i + 1);
        }

        return result;
    }

    /// <summary>Determines whether an integer is a prime number.</summary>
    /// <param name="n">The integer to test.</param>
    /// <returns><c>true</c> if <paramref name="n"/> is prime; otherwise <c>false</c>.</returns>
    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n < 4) return true;
        if (n % 2 == 0 || n % 3 == 0) return false;

        for (var i = 5; i * i <= n; i += 6)
        {
            if (n % i == 0 || n % (i + 2) == 0)
                return false;
        }

        return true;
    }

    /// <summary>Returns the smallest prime number greater than or equal to the specified value.</summary>
    /// <param name="n">The starting value (must be ≥ 2).</param>
    /// <returns>The next prime number ≥ <paramref name="n"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> is less than 2.</exception>
    public static int NextPrime(int n)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 2);

        if (n == 2) return 2;

        var candidate = n | 1;
        while (!IsPrime(candidate))
            candidate += 2;

        return candidate;
    }

    /// <summary>Clamps a value to the specified inclusive range.</summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum bound.</param>
    /// <param name="max">The maximum bound.</param>
    /// <returns><paramref name="min"/> if value is less, <paramref name="max"/> if greater, otherwise value.</returns>
    public static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>Returns the sign of a double-precision floating-point number.</summary>
    /// <param name="value">The value whose sign is determined.</param>
    /// <returns>A value that indicates the sign of <paramref name="value"/>: -1, 0, or 1.</returns>
    public static int Sign(double value) => System.Math.Sign(value);

    /// <summary>Returns the absolute value of a double-precision floating-point number.</summary>
    /// <param name="value">A value.</param>
    /// <returns>The absolute value of <paramref name="value"/>.</returns>
    public static double Abs(double value) => System.Math.Abs(value);

    /// <summary>Determines whether two values are approximately equal within a tolerance.</summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <param name="tolerance">The maximum allowed difference. Defaults to <see cref="MathConstants.Epsilon"/>.</param>
    /// <returns><c>true</c> if the values are approximately equal; otherwise <c>false</c>.</returns>
    public static bool Approximately(double a, double b, double tolerance = MathConstants.Epsilon) =>
        System.Math.Abs(a - b) <= tolerance;

    /// <summary>Converts an angle from degrees to radians.</summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The equivalent angle in radians.</returns>
    public static double DegreesToRadians(double degrees) =>
        degrees * System.Math.PI / 180.0;

    /// <summary>Converts an angle from radians to degrees.</summary>
    /// <param name="radians">The angle in radians.</param>
    /// <returns>The equivalent angle in degrees.</returns>
    public static double RadiansToDegrees(double radians) =>
        radians * 180.0 / System.Math.PI;

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="a">The start value (returned when t = 0).</param>
    /// <param name="b">The end value (returned when t = 1).</param>
    /// <param name="t">The interpolation factor, typically in [0, 1].</param>
    /// <returns>The interpolated value.</returns>
    public static double Lerp(double a, double b, double t) =>
        a + (b - a) * t;

    /// <summary>
    /// Computes the floor of a logarithm with an arbitrary base.
    /// </summary>
    /// <param name="value">The value (must be positive).</param>
    /// <param name="baseValue">The logarithm base (must be positive and not 1).</param>
    /// <returns>The floor of the logarithm.</returns>
    public static int LogBase(double value, int baseValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0.0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(baseValue, 1);

        return (int)System.Math.Floor(System.Math.Log(value, baseValue));
    }

    /// <summary>Determines whether a double-precision floating-point value is finite (not infinite or NaN).</summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if <paramref name="value"/> is finite; otherwise <c>false</c>.</returns>
    public static bool IsFinite(double value) => double.IsFinite(value);

    /// <summary>Determines whether a double-precision floating-point value is NaN.</summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if <paramref name="value"/> is NaN; otherwise <c>false</c>.</returns>
    public static bool IsNaN(double value) => double.IsNaN(value);
}
