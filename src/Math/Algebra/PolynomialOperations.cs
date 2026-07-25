namespace MathVerse.Math.Algebra;

/// <summary>
/// Static operations on <see cref="Polynomial"/> instances including division,
/// GCD/LCM, differentiation, integration, and numerical root finding.
/// </summary>
public static class PolynomialOperations
{
    /// <summary>
    /// Performs polynomial long division.
    /// </summary>
    /// <param name="dividend">The polynomial to divide.</param>
    /// <param name="divisor">The polynomial to divide by.</param>
    /// <returns>A tuple of (quotient, remainder).</returns>
    public static (Polynomial Quotient, Polynomial Remainder) Divide(Polynomial dividend, Polynomial divisor)
    {
        if (dividend.Variable != divisor.Variable)
            throw new ArgumentException("Polynomials must share the same variable.");

        if (divisor.IsZero)
            throw new DivideByZeroException("Cannot divide by the zero polynomial.");

        if (dividend.Degree < divisor.Degree)
            return (Polynomial.Zero(dividend.Variable), dividend);

        int quotientDegree = dividend.Degree - divisor.Degree;
        var quotientCoeffs = new double[quotientDegree + 1];
        var remainder = new double[dividend.Degree + 1];

        for (int i = 0; i <= dividend.Degree; i++)
            remainder[i] = dividend.CoefficientAt(i);

        double leadDivisor = divisor.LeadingCoefficient;

        for (int i = quotientDegree; i >= 0; i--)
        {
            quotientCoeffs[i] = remainder[i + divisor.Degree] / leadDivisor;
            for (int j = 0; j <= divisor.Degree; j++)
                remainder[i + j] -= quotientCoeffs[i] * divisor.CoefficientAt(j);
        }

        var quotient = new Polynomial(dividend.Variable, ImmutableArray.Create(quotientCoeffs));

        int remDegree = remainder.Length - 1;
        while (remDegree > 0 && System.Math.Abs(remainder[remDegree]) < 1e-15)
            remDegree--;
        var remCoeffs = new double[remDegree + 1];
        System.Array.Copy(remainder, remCoeffs, remDegree + 1);
        var rem = new Polynomial(dividend.Variable, ImmutableArray.Create(remCoeffs));

        return (quotient, rem);
    }

    /// <summary>
    /// Computes the greatest common divisor of two polynomials using the Euclidean algorithm.
    /// </summary>
    /// <param name="a">First polynomial.</param>
    /// <param name="b">Second polynomial.</param>
    /// <returns>The monic GCD polynomial.</returns>
    public static Polynomial GCD(Polynomial a, Polynomial b)
    {
        if (a.Variable != b.Variable)
            throw new ArgumentException("Polynomials must share the same variable.");

        while (!b.IsZero)
        {
            var (_, remainder) = Divide(a, b);
            a = b;
            b = remainder;
        }

        if (a.IsZero)
            return Polynomial.Zero(a.Variable);

        double lead = a.LeadingCoefficient;
        return System.Math.Abs(lead) < 1e-15 ? a : a.Scale(1.0 / lead);
    }

    /// <summary>
    /// Computes the least common multiple of two polynomials.
    /// </summary>
    /// <param name="a">First polynomial.</param>
    /// <param name="b">Second polynomial.</param>
    /// <returns>The LCM polynomial.</returns>
    public static Polynomial LCM(Polynomial a, Polynomial b)
    {
        if (a.IsZero || b.IsZero)
            return Polynomial.Zero(a.Variable);

        var gcd = GCD(a, b);
        var product = a.Multiply(b);
        var (quotient, _) = Divide(product, gcd);
        return quotient;
    }

    /// <summary>
    /// Computes the derivative of a polynomial using the power rule.
    /// </summary>
    /// <param name="p">The polynomial to differentiate.</param>
    /// <returns>The derivative polynomial.</returns>
    public static Polynomial EvaluateDerivative(Polynomial p) => p.Derivative();

    /// <summary>
    /// Computes the indefinite integral of a polynomial.
    /// </summary>
    /// <param name="p">The polynomial to integrate.</param>
    /// <returns>The integral polynomial.</returns>
    public static Polynomial EvaluateIntegral(Polynomial p) => p.Integral();

    /// <summary>
    /// Finds a root of the polynomial in the interval [a, b] using the bisection method.
    /// </summary>
    /// <param name="p">The polynomial.</param>
    /// <param name="a">Left endpoint of the bracket.</param>
    /// <param name="b">Right endpoint of the bracket.</param>
    /// <param name="tol">Tolerance for convergence.</param>
    /// <param name="maxIter">Maximum number of iterations.</param>
    /// <returns>A <see cref="Maybe{T}"/> containing the root or the reason it failed.</returns>
    public static Maybe<double> Bisection(Polynomial p, double a, double b, double tol = 1e-10, int maxIter = 1000)
    {
        double fa = p.Evaluate(a);
        double fb = p.Evaluate(b);

        if (System.Math.Abs(fa) < tol)
            return Maybe<double>.Defined(a);
        if (System.Math.Abs(fb) < tol)
            return Maybe<double>.Defined(b);

        if (fa * fb > 0)
            return Maybe<double>.Undefined(MaybeReason.DomainError);

        for (int i = 0; i < maxIter; i++)
        {
            double mid = (a + b) / 2.0;
            double fmid = p.Evaluate(mid);

            if (System.Math.Abs(fmid) < tol || (b - a) / 2.0 < tol)
                return Maybe<double>.Defined(mid);

            if (fa * fmid < 0)
            {
                b = mid;
                fb = fmid;
            }
            else
            {
                a = mid;
                fa = fmid;
            }
        }

        return Maybe<double>.Defined((a + b) / 2.0);
    }

    /// <summary>
    /// Finds a root of the polynomial using the Newton-Raphson method.
    /// </summary>
    /// <param name="p">The polynomial.</param>
    /// <param name="x0">Initial guess.</param>
    /// <param name="tol">Tolerance for convergence.</param>
    /// <param name="maxIter">Maximum number of iterations.</param>
    /// <returns>A <see cref="Maybe{T}"/> containing the root or the reason it failed.</returns>
    public static Maybe<double> NewtonRaphson(Polynomial p, double x0, double tol = 1e-10, int maxIter = 1000)
    {
        var dp = p.Derivative();

        double x = x0;
        for (int i = 0; i < maxIter; i++)
        {
            double fx = p.Evaluate(x);
            double dfx = dp.Evaluate(x);

            if (System.Math.Abs(dfx) < 1e-15)
                return Maybe<double>.Undefined(MaybeReason.DivisionByZero);

            double xNew = x - fx / dfx;

            if (System.Math.Abs(xNew - x) < tol)
                return Maybe<double>.Defined(xNew);

            x = xNew;
        }

        return Maybe<double>.Undefined(MaybeReason.DidNotConverge);
    }
}
