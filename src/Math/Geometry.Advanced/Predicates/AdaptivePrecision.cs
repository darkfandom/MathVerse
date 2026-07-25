using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Predicates;

/// <summary>
/// Provides adaptive precision utilities for geometric predicates.
/// Computes error bounds and near-equality tests that adapt to the magnitude of input values.
/// </summary>
public static class AdaptivePrecision
{
    private const double DefaultEpsilon = 1e-10;

    /// <summary>
    /// Computes an adaptive epsilon value based on the magnitude of the input.
    /// Larger magnitudes produce proportionally larger epsilons to account for
    /// floating-point precision loss in operations with large values.
    /// </summary>
    /// <param name="magnitude">The magnitude of the input values.</param>
    /// <returns>An adaptive epsilon suitable for comparisons at the given scale.</returns>
    public static double ComputeEpsilon(double magnitude)
    {
        double absMag = System.Math.Abs(magnitude);
        if (absMag < 1e-15)
            return DefaultEpsilon;
        return absMag * 2.2204460492503131e-16 * 16.0;
    }

    /// <summary>
    /// Tests whether two doubles are approximately equal using an adaptive tolerance.
    /// If no epsilon is specified, a default tolerance of 1e-10 is used.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <param name="epsilon">The tolerance for comparison. Defaults to 1e-10.</param>
    /// <returns><c>true</c> if the values are within epsilon of each other; otherwise, <c>false</c>.</returns>
    public static bool NearlyEqual(double a, double b, double epsilon = 1e-10)
    {
        double diff = System.Math.Abs(a - b);
        double scale = System.Math.Max(System.Math.Abs(a), System.Math.Abs(b));
        double adaptive = System.Math.Max(epsilon, scale * 2.2204460492503131e-16 * 16.0);
        return diff <= adaptive;
    }

    /// <summary>
    /// Tests whether a value is approximately zero using the default tolerance.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns><c>true</c> if the absolute value is less than or equal to 1e-10; otherwise, <c>false</c>.</returns>
    public static bool NearlyZero(double value)
    {
        return System.Math.Abs(value) <= DefaultEpsilon;
    }

    /// <summary>
    /// Computes the determinant of a 2x2 matrix with error bound estimation.
    /// The determinant is computed as ad - bc with Kahan summation for accuracy.
    /// </summary>
    /// <param name="a">The element at position (0,0).</param>
    /// <param name="b">The element at position (0,1).</param>
    /// <param name="c">The element at position (1,0).</param>
    /// <param name="d">The element at position (1,1).</param>
    /// <returns>The determinant value with improved numerical accuracy.</returns>
    public static double Determinant2x2(double a, double b, double c, double d)
    {
        double ad = a * d;
        double bc = b * c;

        double diff = ad - bc;
        double errBound = (System.Math.Abs(ad) + System.Math.Abs(bc)) * 2.2204460492503131e-16;

        if (System.Math.Abs(diff) < errBound)
        {
            double s = ad - bc;
            double d1 = ad - s;
            double d2 = bc + d1;
            diff = s - d2;
        }

        return diff;
    }

    /// <summary>
    /// Computes the determinant of a 3x3 matrix formed by three column vectors
    /// with error bound estimation. The determinant represents the signed volume
    /// of the parallelepiped spanned by the three vectors.
    /// </summary>
    /// <param name="a">The first column vector.</param>
    /// <param name="b">The second column vector.</param>
    /// <param name="c">The third column vector.</param>
    /// <returns>The determinant value with improved numerical accuracy.</returns>
    public static double Determinant3x3(Vector3D a, Vector3D b, Vector3D c)
    {
        double det = a.X * (b.Y * c.Z - b.Z * c.Y)
                   - a.Y * (b.X * c.Z - b.Z * c.X)
                   + a.Z * (b.X * c.Y - b.Y * c.X);

        double errBound = (
            System.Math.Abs(a.X) * (System.Math.Abs(b.Y * c.Z) + System.Math.Abs(b.Z * c.Y))
          + System.Math.Abs(a.Y) * (System.Math.Abs(b.X * c.Z) + System.Math.Abs(b.Z * c.X))
          + System.Math.Abs(a.Z) * (System.Math.Abs(b.X * c.Y) + System.Math.Abs(b.Y * c.X))
        ) * 2.2204460492503131e-16;

        if (System.Math.Abs(det) < errBound)
        {
            double cofactor1 = b.Y * c.Z - b.Z * c.Y;
            double cofactor2 = b.X * c.Z - b.Z * c.X;
            double cofactor3 = b.X * c.Y - b.Y * c.X;

            RobustPredicates.TwoProduct(a.X, cofactor1, out double p1Hi, out double p1Lo);
            RobustPredicates.TwoProduct(a.Y, cofactor2, out double p2Hi, out double p2Lo);
            RobustPredicates.TwoProduct(a.Z, cofactor3, out double p3Hi, out double p3Lo);

            RobustPredicates.TwoSum(p1Hi, -p2Hi, out double s1Hi, out double s1Lo);
            RobustPredicates.TwoSum(s1Hi, p3Hi, out double s2Hi, out double s2Lo);

            det = s2Hi + (s2Lo + (s1Lo + (p1Lo - p2Lo) + p3Lo));
        }

        return det;
    }
}
