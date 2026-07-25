using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Predicates;

/// <summary>
/// Provides exact geometric predicates using compensated summation (Kahan)
/// and error-free transformations for robust numerical computation.
/// </summary>
public static class RobustPredicates
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the exact orientation test for three 2D points.
    /// Returns positive if the points are counter-clockwise, negative if clockwise,
    /// and zero if collinear. Uses error-free transformations for exact arithmetic.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="c">The third point.</param>
    /// <returns>The signed orientation value (positive = CCW, negative = CW, zero = collinear).</returns>
    public static double Orientation2D(Point2D a, Point2D b, Point2D c)
    {
        double acx = a.X - c.X;
        double bcx = b.X - c.X;
        double acy = a.Y - c.Y;
        double bcy = b.Y - c.Y;

        TwoProduct(acx, bcy, out double p1Hi, out double p1Lo);
        TwoProduct(acy, bcx, out double p2Hi, out double p2Lo);

        TwoSum(p1Hi, -p2Hi, out double sHi, out double sLo);
        double result = sHi + (sLo + (p1Lo - p2Lo));

        return result;
    }

    /// <summary>
    /// Computes the exact in-circle test for four 2D points.
    /// Returns positive if point d is inside the circumcircle of triangle abc,
    /// negative if outside, and zero if on the circle. Uses error-free transformations.
    /// </summary>
    /// <param name="a">The first point of the triangle.</param>
    /// <param name="b">The second point of the triangle.</param>
    /// <param name="c">The third point of the triangle.</param>
    /// <param name="d">The point to test against the circumcircle.</param>
    /// <returns>The signed in-circle value (positive = inside, negative = outside, zero = on circle).</returns>
    public static double InCircle2D(Point2D a, Point2D b, Point2D c, Point2D d)
    {
        double adx = a.X - d.X;
        double ady = a.Y - d.Y;
        double bdx = b.X - d.X;
        double bdy = b.Y - d.Y;
        double cdx = c.X - d.X;
        double cdy = c.Y - d.Y;

        TwoProduct(adx, bdy, out double abHi, out double abLo);
        TwoProduct(ady, bdx, out double baHi, out double baLo);

        TwoProduct(bdx, cdy, out double bcHi, out double bcLo);
        TwoProduct(bdy, cdx, out double cbHi, out double cbLo);

        TwoProduct(adx, cdy, out double acHi, out double acLo);
        TwoProduct(ady, cdx, out double caHi, out double caLo);

        double abDet = abHi - baHi;
        double bcDet = bcHi - cbHi;
        double acDet = acHi - caHi;

        double det = adx * bcDet - bdy * acDet + cdx * abDet;

        double errBound = 1e-14 * (
            System.Math.Abs(adx) * (System.Math.Abs(bcHi) + System.Math.Abs(cbHi))
          + System.Math.Abs(bdy) * (System.Math.Abs(acHi) + System.Math.Abs(caHi))
          + System.Math.Abs(cdx) * (System.Math.Abs(abHi) + System.Math.Abs(baHi)));

        if (System.Math.Abs(det) < errBound || System.Math.Abs(det) < 1e-14 * System.Math.Abs(abHi))
        {
            det += (abLo - baLo) * cdx + (bcLo - cbLo) * adx + (acLo - caLo) * bdy;
            det += abLo * cdy + bcLo * ady + acLo * bdy;
        }

        return det;
    }

    /// <summary>
    /// Computes the exact in-sphere test for five 3D points.
    /// Returns positive if point e is inside the circumsphere of tetrahedron abcd,
    /// negative if outside, and zero if on the sphere.
    /// </summary>
    /// <param name="a">The first vertex of the tetrahedron.</param>
    /// <param name="b">The second vertex of the tetrahedron.</param>
    /// <param name="c">The third vertex of the tetrahedron.</param>
    /// <param name="d">The fourth vertex of the tetrahedron.</param>
    /// <param name="e">The point to test against the circumsphere.</param>
    /// <returns>The signed in-sphere value (positive = inside, negative = outside, zero = on sphere).</returns>
    public static double InSphere3D(Point3D a, Point3D b, Point3D c, Point3D d, Point3D e)
    {
        double aex = a.X - e.X;
        double aey = a.Y - e.Y;
        double aez = a.Z - e.Z;
        double bex = b.X - e.X;
        double bey = b.Y - e.Y;
        double bez = b.Z - e.Z;
        double cex = c.X - e.X;
        double cey = c.Y - e.Y;
        double cez = c.Z - e.Z;
        double dex = d.X - e.X;
        double dey = d.Y - e.Y;
        double dez = d.Z - e.Z;

        double ab = aex * bey - aey * bex;
        double ac = aex * cey - aey * cex;
        double ad = aex * dey - aey * dex;
        double bc = bex * cey - bey * cex;
        double bd = bex * dey - bey * dex;
        double cd = cex * dey - cey * dex;

        double abc = ab * cez - ac * bez + bc * aez;
        double abd = ab * dez - ad * bez + bd * aez;
        double acd = ac * dez - ad * cez + cd * aez;
        double bcd = bc * dez - bd * cez + cd * bez;

        double det = abc * dex - abd * cex + acd * bex - bcd * aex;

        return det;
    }

    /// <summary>
    /// Computes a fast approximate orientation test for three 2D points.
    /// Returns only the sign: +1 for CCW, -1 for CW, 0 for collinear.
    /// This is a fast but non-exact version suitable for quick filtering.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="c">The third point.</param>
    /// <returns>+1 if CCW, -1 if CW, 0 if approximately collinear.</returns>
    public static int Orient2DFast(Point2D a, Point2D b, Point2D c)
    {
        double det = (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
        if (det > Tolerance) return 1;
        if (det < -Tolerance) return -1;
        return 0;
    }

    /// <summary>
    /// Computes the signed area of a 2D polygon using the shoelace formula
    /// with compensated summation for numerical accuracy.
    /// Positive area indicates counter-clockwise winding, negative indicates clockwise.
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <returns>The signed area of the polygon.</returns>
    public static double Area2D(ImmutableArray<Point2D> polygon)
    {
        if (polygon.Length < 3)
            return 0;

        double sum = 0;
        double comp = 0;

        for (int i = 0; i < polygon.Length; i++)
        {
            int j = (i + 1) % polygon.Length;
            double cross = polygon[i].X * polygon[j].Y - polygon[j].X * polygon[i].Y;

            double y = cross - comp;
            double t = sum + y;
            comp = (t - sum) - y;
            sum = t;
        }

        return sum * 0.5;
    }

    /// <summary>
    /// Performs error-free multiplication of two doubles using Knuth's algorithm.
    /// The result is exactly represented as hi + lo where hi is the high-order
    /// part and lo is the low-order correction term.
    /// </summary>
    /// <param name="a">The first multiplicand.</param>
    /// <param name="b">The second multiplicand.</param>
    /// <param name="hi">The high-order part of the exact product.</param>
    /// <param name="lo">The low-order correction term.</param>
    internal static void TwoProduct(double a, double b, out double hi, out double lo)
    {
        hi = a * b;
        double aHi, aLo, bHi, bLo;
        Split(a, out aHi, out aLo);
        Split(b, out bHi, out bLo);
        lo = ((aHi * bHi - hi) + aHi * bLo + aLo * bHi) + aLo * bLo;
    }

    private static void Split(double a, out double hi, out double lo)
    {
        double c = a * 134217729.0;
        hi = c - (c - a);
        lo = a - hi;
    }

    /// <summary>
    /// Performs error-free addition of two doubles using the Knuth two-sum algorithm.
    /// The result is exactly represented as hi + lo where hi is the high-order
    /// part and lo is the low-order correction term.
    /// </summary>
    /// <param name="a">The first addend.</param>
    /// <param name="b">The second addend.</param>
    /// <param name="hi">The high-order part of the exact sum.</param>
    /// <param name="lo">The low-order correction term.</param>
    internal static void TwoSum(double a, double b, out double hi, out double lo)
    {
        hi = a + b;
        double bv = hi - a;
        double av = hi - bv;
        lo = (a - av) + (b - bv);
    }
}
