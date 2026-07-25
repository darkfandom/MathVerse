namespace MathVerse.Math.Foundation.Constants;

public static class BuiltinConstants
{
    public static MathConstant Pi { get; } = new MathConstant
    {
        Symbol = "\u03C0",
        Name = "Pi",
        Category = ConstantCategory.Transcendental,
        NumericValue = System.Math.PI,
        ComplexValue = new Complex(System.Math.PI, 0),
        Aliases = ImmutableArray.Create("pi", "Pi", "\u03A0"),
        Description = "The ratio of a circle's circumference to its diameter",
        IsExact = true
    };

    public static MathConstant Tau { get; } = new MathConstant
    {
        Symbol = "\u03C4",
        Name = "Tau",
        Category = ConstantCategory.Transcendental,
        NumericValue = System.Math.Tau,
        ComplexValue = new Complex(System.Math.Tau, 0),
        Aliases = ImmutableArray.Create("tau", "Tau", "2pi"),
        Description = "The ratio of a circle's circumference to its radius",
        IsExact = true
    };

    public static MathConstant E { get; } = new MathConstant
    {
        Symbol = "e",
        Name = "E",
        Category = ConstantCategory.Transcendental,
        NumericValue = System.Math.E,
        ComplexValue = new Complex(System.Math.E, 0),
        Aliases = ImmutableArray.Create("Euler", "Euler's number", "exp(1)"),
        Description = "The base of the natural logarithm",
        IsExact = true
    };

    public static MathConstant Phi { get; } = new MathConstant
    {
        Symbol = "\u03C6",
        Name = "Phi",
        Category = ConstantCategory.Fundamental,
        NumericValue = 1.6180339887498948482,
        ComplexValue = new Complex(1.6180339887498948482, 0),
        Aliases = ImmutableArray.Create("phi", "golden ratio", "golden mean", "\u03D5"),
        Description = "The golden ratio, (1 + sqrt(5)) / 2",
        IsExact = true
    };

    public static MathConstant Gamma { get; } = new MathConstant
    {
        Symbol = "\u03B3",
        Name = "Gamma",
        Category = ConstantCategory.Analysis,
        NumericValue = 0.5772156649015328606,
        ComplexValue = new Complex(0.5772156649015328606, 0),
        Aliases = ImmutableArray.Create("gamma", "Euler-Mascheroni", "Euler-Mascheroni constant"),
        Description = "The Euler-Mascheroni constant, limit of harmonic series minus natural log",
        IsExact = true
    };

    public static MathConstant I { get; } = new MathConstant
    {
        Symbol = "i",
        Name = "I",
        Category = ConstantCategory.Fundamental,
        NumericValue = double.NaN,
        ComplexValue = new Complex(0, 1),
        Aliases = ImmutableArray.Create("imaginary unit", "j"),
        Description = "The imaginary unit, sqrt(-1)",
        IsExact = true
    };

    public static MathConstant Infinity { get; } = new MathConstant
    {
        Symbol = "\u221E",
        Name = "Infinity",
        Category = ConstantCategory.Fundamental,
        NumericValue = double.PositiveInfinity,
        ComplexValue = new Complex(double.PositiveInfinity, 0),
        Aliases = ImmutableArray.Create("inf", "Inf", "\u221E"),
        Description = "Positive infinity",
        IsExact = true
    };

    public static MathConstant NaN { get; } = new MathConstant
    {
        Symbol = "NaN",
        Name = "NaN",
        Category = ConstantCategory.Fundamental,
        NumericValue = double.NaN,
        ComplexValue = new Complex(double.NaN, double.NaN),
        Aliases = ImmutableArray.Create("nan", "Not a Number"),
        Description = "Not a Number, result of undefined mathematical operations",
        IsExact = false
    };

    public static MathConstant Epsilon { get; } = new MathConstant
    {
        Symbol = "\u03B5",
        Name = "Epsilon",
        Category = ConstantCategory.Analysis,
        NumericValue = 2.2204460492503131e-16,
        ComplexValue = new Complex(2.2204460492503131e-16, 0),
        Aliases = ImmutableArray.Create("machine epsilon", "eps"),
        Description = "The smallest double such that 1.0 + epsilon != 1.0",
        IsExact = false
    };

    public static MathConstant Catalan { get; } = new MathConstant
    {
        Symbol = "G",
        Name = "Catalan",
        Category = ConstantCategory.NumberTheory,
        NumericValue = 0.9159655941772190150,
        ComplexValue = new Complex(0.9159655941772190150, 0),
        Aliases = ImmutableArray.Create("Catalan's constant"),
        Description = "Catalan's constant, alternating sum of reciprocal odd squares",
        IsExact = true
    };

    public static MathConstant Apery { get; } = new MathConstant
    {
        Symbol = "\u03B6(3)",
        Name = "Apery",
        Category = ConstantCategory.NumberTheory,
        NumericValue = 1.2020569031595942854,
        ComplexValue = new Complex(1.2020569031595942854, 0),
        Aliases = ImmutableArray.Create("Ap\u00E9ry's constant", "zeta(3)", "Zeta(3)"),
        Description = "Ap\u00E9ry's constant, value of the Riemann zeta function at 3",
        IsExact = true
    };

    public static MathConstant FeigenbaumAlpha { get; } = new MathConstant
    {
        Symbol = "\u03B1",
        Name = "FeigenbaumAlpha",
        Category = ConstantCategory.Combinatorics,
        NumericValue = 2.502907875095892822283,
        ComplexValue = new Complex(2.502907875095892822283, 0),
        Aliases = ImmutableArray.Create("Feigenbaum alpha", "feigenbaum alpha", "first Feigenbaum constant"),
        Description = "The first Feigenbaum constant, ratio of successive bifurcation intervals",
        IsExact = true
    };

    public static MathConstant FeigenbaumDelta { get; } = new MathConstant
    {
        Symbol = "\u03B4",
        Name = "FeigenbaumDelta",
        Category = ConstantCategory.Combinatorics,
        NumericValue = 4.669201609102990671853,
        ComplexValue = new Complex(4.669201609102990671853, 0),
        Aliases = ImmutableArray.Create("Feigenbaum delta", "feigenbaum delta", "second Feigenbaum constant"),
        Description = "The second Feigenbaum constant, limiting ratio of bifurcation widths",
        IsExact = true
    };
}
