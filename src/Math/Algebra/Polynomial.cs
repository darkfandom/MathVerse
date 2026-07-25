namespace MathVerse.Math.Algebra;

/// <summary>
/// Represents a univariate polynomial with real coefficients.
/// Coefficients are stored as an immutable array indexed by degree.
/// </summary>
public sealed class Polynomial : IEquatable<Polynomial>
{
    /// <summary>
    /// Initializes a polynomial from a variable name and coefficient array.
    /// </summary>
    /// <param name="variable">The variable name.</param>
    /// <param name="coefficients">Coefficient at each degree (index = degree).</param>
    public Polynomial(string variable, ImmutableArray<double> coefficients)
    {
        Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        Coefficients = coefficients.IsDefault ? ImmutableArray<double>.Empty : coefficients;
    }

    /// <summary>Gets the variable name.</summary>
    public string Variable { get; }

    /// <summary>Gets the coefficients indexed by degree.</summary>
    public ImmutableArray<double> Coefficients { get; }

    /// <summary>Gets the degree of the polynomial.</summary>
    public int Degree => Coefficients.Length == 0 ? -1 : Coefficients.Length - 1;

    /// <summary>Gets the leading coefficient.</summary>
    public double LeadingCoefficient => Coefficients.Length == 0 ? 0.0 : Coefficients[^1];

    /// <summary>Gets whether the polynomial is identically zero.</summary>
    public bool IsZero => Coefficients.Length == 0 || Coefficients.All(c => System.Math.Abs(c) < 1e-15);

    /// <summary>Gets whether the polynomial is a non-zero constant.</summary>
    public bool IsConstant => !IsZero && Degree == 0;

    /// <summary>Gets whether the polynomial is of degree 1.</summary>
    public bool IsLinear => Degree == 1;

    /// <summary>Gets whether the polynomial is of degree 2.</summary>
    public bool IsQuadratic => Degree == 2;

    /// <summary>
    /// Gets the coefficient at the specified degree.
    /// </summary>
    /// <param name="degree">The degree to query.</param>
    /// <returns>The coefficient, or 0 if degree exceeds the polynomial.</returns>
    public double CoefficientAt(int degree)
    {
        if (degree < 0 || degree >= Coefficients.Length)
            return 0.0;
        return Coefficients[degree];
    }

    /// <summary>
    /// Evaluates the polynomial at the given value of x using Horner's method.
    /// </summary>
    /// <param name="x">The point at which to evaluate.</param>
    /// <returns>The polynomial value.</returns>
    public double Evaluate(double x)
    {
        if (Coefficients.Length == 0)
            return 0.0;

        double result = Coefficients[^1];
        for (int i = Coefficients.Length - 2; i >= 0; i--)
            result = result * x + Coefficients[i];
        return result;
    }

    /// <summary>
    /// Adds another polynomial to this polynomial.
    /// </summary>
    /// <param name="other">The polynomial to add.</param>
    /// <returns>The sum polynomial.</returns>
    public Polynomial Add(Polynomial other)
    {
        if (other.Variable != Variable)
            throw new ArgumentException("Polynomials must share the same variable.", nameof(other));

        int maxLen = System.Math.Max(Coefficients.Length, other.Coefficients.Length);
        var builder = ImmutableArray.CreateBuilder<double>(maxLen);

        for (int i = 0; i < maxLen; i++)
            builder.Add(CoefficientAt(i) + other.CoefficientAt(i));

        return new Polynomial(Variable, builder.ToImmutable());
    }

    /// <summary>
    /// Subtracts another polynomial from this polynomial.
    /// </summary>
    /// <param name="other">The polynomial to subtract.</param>
    /// <returns>The difference polynomial.</returns>
    public Polynomial Subtract(Polynomial other)
    {
        if (other.Variable != Variable)
            throw new ArgumentException("Polynomials must share the same variable.", nameof(other));

        int maxLen = System.Math.Max(Coefficients.Length, other.Coefficients.Length);
        var builder = ImmutableArray.CreateBuilder<double>(maxLen);

        for (int i = 0; i < maxLen; i++)
            builder.Add(CoefficientAt(i) - other.CoefficientAt(i));

        return new Polynomial(Variable, builder.ToImmutable());
    }

    /// <summary>
    /// Multiplies this polynomial by another polynomial.
    /// </summary>
    /// <param name="other">The polynomial to multiply by.</param>
    /// <returns>The product polynomial.</returns>
    public Polynomial Multiply(Polynomial other)
    {
        if (other.Variable != Variable)
            throw new ArgumentException("Polynomials must share the same variable.", nameof(other));

        if (IsZero || other.IsZero)
            return Zero(Variable);

        int resultLen = Coefficients.Length + other.Coefficients.Length - 1;
        var result = new double[resultLen];

        for (int i = 0; i < Coefficients.Length; i++)
        {
            for (int j = 0; j < other.Coefficients.Length; j++)
                result[i + j] += Coefficients[i] * other.Coefficients[j];
        }

        return new Polynomial(Variable, ImmutableArray.Create(result));
    }

    /// <summary>
    /// Scales the polynomial by a constant factor.
    /// </summary>
    /// <param name="s">The scalar factor.</param>
    /// <returns>The scaled polynomial.</returns>
    public Polynomial Scale(double s)
    {
        var scaled = Coefficients.Select(c => c * s).ToImmutableArray();
        return new Polynomial(Variable, scaled);
    }

    /// <summary>
    /// Negates the polynomial.
    /// </summary>
    /// <returns>The negated polynomial.</returns>
    public Polynomial Negate() => Scale(-1.0);

    /// <summary>
    /// Computes the formal derivative of the polynomial.
    /// </summary>
    /// <returns>The derivative polynomial.</returns>
    public Polynomial Derivative()
    {
        if (Degree <= 0)
            return Zero(Variable);

        var builder = ImmutableArray.CreateBuilder<double>(Coefficients.Length - 1);
        for (int i = 1; i < Coefficients.Length; i++)
            builder.Add(Coefficients[i] * i);

        return new Polynomial(Variable, builder.ToImmutable());
    }

    /// <summary>
    /// Computes the indefinite integral of the polynomial (constant of integration = 0).
    /// </summary>
    /// <returns>The integral polynomial.</returns>
    public Polynomial Integral()
    {
        var builder = ImmutableArray.CreateBuilder<double>(Coefficients.Length + 1);
        builder.Add(0.0);
        for (int i = 0; i < Coefficients.Length; i++)
            builder.Add(Coefficients[i] / (i + 1));

        return new Polynomial(Variable, builder.ToImmutable());
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (IsZero)
            return "0";

        var sb = new StringBuilder();
        bool first = true;

        for (int i = Coefficients.Length - 1; i >= 0; i--)
        {
            double coeff = Coefficients[i];
            if (System.Math.Abs(coeff) < 1e-15)
                continue;

            if (!first)
                sb.Append(coeff > 0 ? " + " : " - ");
            else if (coeff < 0)
                sb.Append('-');

            double absCoeff = System.Math.Abs(coeff);

            if (i == 0 || System.Math.Abs(absCoeff - 1.0) > 1e-15)
                sb.Append(absCoeff);

            if (i == 1)
                sb.Append(Variable);
            else if (i > 1)
                sb.Append(Variable).Append('^').Append(i);

            first = false;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a polynomial from explicit coefficient values.
    /// </summary>
    /// <param name="variable">The variable name.</param>
    /// <param name="coeffs">Coefficients indexed by degree.</param>
    /// <returns>The polynomial.</returns>
    public static Polynomial FromCoefficients(string variable, params double[] coeffs) =>
        new(variable, ImmutableArray.Create(coeffs));

    /// <summary>
    /// Creates the zero polynomial in the given variable.
    /// </summary>
    /// <param name="variable">The variable name.</param>
    /// <returns>The zero polynomial.</returns>
    public static Polynomial Zero(string variable) =>
        new(variable, ImmutableArray<double>.Empty);

    /// <summary>
    /// Creates the constant polynomial 1 in the given variable.
    /// </summary>
    /// <param name="variable">The variable name.</param>
    /// <returns>The constant polynomial 1.</returns>
    public static Polynomial One(string variable) =>
        new(variable, ImmutableArray.Create(1.0));

    /// <summary>
    /// Creates a monomial with a single non-zero coefficient at the given degree.
    /// </summary>
    /// <param name="variable">The variable name.</param>
    /// <param name="degree">The degree of the monomial.</param>
    /// <param name="coefficient">The coefficient.</param>
    /// <returns>The monomial polynomial.</returns>
    public static Polynomial Monomial(string variable, int degree, double coefficient)
    {
        if (degree < 0)
            throw new ArgumentOutOfRangeException(nameof(degree));

        var builder = ImmutableArray.CreateBuilder<double>(degree + 1);
        for (int i = 0; i < degree; i++)
            builder.Add(0.0);
        builder.Add(coefficient);

        return new Polynomial(variable, builder.ToImmutable());
    }

    /// <summary>
    /// Creates a polynomial from its roots using Vieta's formulas.
    /// </summary>
    /// <param name="variable">The variable name.</param>
    /// <param name="roots">The roots of the polynomial.</param>
    /// <returns>The monic polynomial with the given roots.</returns>
    public static Polynomial FromRoots(string variable, params double[] roots)
    {
        Polynomial result = One(variable);
        foreach (double root in roots)
        {
            var factor = FromCoefficients(variable, -root, 1.0);
            result = result.Multiply(factor);
        }
        return result;
    }

    /// <inheritdoc/>
    public bool Equals(Polynomial? other)
    {
        if (other is null || Variable != other.Variable)
            return false;

        int len = System.Math.Max(Coefficients.Length, other.Coefficients.Length);
        for (int i = 0; i < len; i++)
        {
            if (System.Math.Abs(CoefficientAt(i) - other.CoefficientAt(i)) > 1e-15)
                return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Polynomial);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Variable, Degree);
}
