using MathVerse.Math.Semantics.Binding;

namespace MathVerse.Math.Semantics.Resolution;

/// <summary>
/// Performs constant folding on bound expression trees.
/// Evaluates arithmetic expressions involving only literals and constants at analysis time.
/// </summary>
public sealed class ConstantFolder
{
    private readonly SemanticDiagnosticBag _diagnostics;

    /// <summary>Initializes a constant folder.</summary>
    public ConstantFolder(SemanticDiagnosticBag diagnostics)
    {
        _diagnostics = diagnostics;
    }

    /// <summary>Attempts to fold a bound expression to a constant value.</summary>
    public double? TryFold(BoundExpression expression)
    {
        return expression switch
        {
            BoundLiteralExpression lit => lit.Value,
            BoundConstantExpression c => c.Constant.Value,
            BoundBinaryExpression b => FoldBinary(b),
            BoundUnaryExpression u => FoldUnary(u),
            BoundFunctionCallExpression f => FoldFunctionCall(f),
            _ => null,
        };
    }

    /// <summary>Returns true if the expression is a foldable constant.</summary>
    public bool IsConstant(BoundExpression expression) =>
        TryFold(expression).HasValue;

    /// <summary>Folds the expression and returns a literal if possible.</summary>
    public BoundExpression Fold(BoundExpression expression)
    {
        var value = TryFold(expression);
        return value.HasValue ? new BoundLiteralExpression(value.Value) : expression;
    }

    private double? FoldBinary(BoundBinaryExpression b)
    {
        var l = TryFold(b.Left);
        var r = TryFold(b.Right);
        if (l is null || r is null) return null;

        var sym = b.Operator.Symbol;
        return sym switch
        {
            "+" => l + r,
            "-" => l - r,
            "*" => l * r,
            "/" => r != 0 ? l.Value / r.Value : HandleDivisionByZero(l.Value, r.Value),
            "%" => r != 0 ? l.Value % r.Value : HandleDivisionByZero(l.Value, r.Value),
            "^" or "**" => System.Math.Pow(l.Value, r.Value),
            _ => null,
        };
    }

    private double? FoldUnary(BoundUnaryExpression u)
    {
        var val = TryFold(u.Operand);
        if (val is null) return null;

        return u.Operator.Symbol switch
        {
            "-" => -val,
            "+" => val,
            _ => null,
        };
    }

    private double? FoldFunctionCall(BoundFunctionCallExpression f)
    {
        var args = f.Arguments.Select(TryFold).ToList();
        if (args.Any(a => a is null)) return null;

        var values = args.Select(a => a!.Value).ToList();
        return f.Function.Name switch
        {
            "sin" => System.Math.Sin(values[0]),
            "cos" => System.Math.Cos(values[0]),
            "tan" => System.Math.Tan(values[0]),
            "asin" => System.Math.Asin(values[0]),
            "acos" => System.Math.Acos(values[0]),
            "atan" => System.Math.Atan(values[0]),
            "sinh" => System.Math.Sinh(values[0]),
            "cosh" => System.Math.Cosh(values[0]),
            "tanh" => System.Math.Tanh(values[0]),
            "sqrt" => System.Math.Sqrt(values[0]),
            "cbrt" => System.Math.Cbrt(values[0]),
            "abs" => System.Math.Abs(values[0]),
            "floor" => System.Math.Floor(values[0]),
            "ceil" => System.Math.Ceiling(values[0]),
            "round" => System.Math.Round(values[0]),
            "sign" => System.Math.Sign(values[0]),
            "exp" => System.Math.Exp(values[0]),
            "ln" => System.Math.Log(values[0]),
            "log" => System.Math.Log(values[0]),
            "log2" => System.Math.Log2(values[0]),
            "log10" => System.Math.Log10(values[0]),
            "logbase" => System.Math.Log(values[0], values[1]),
            "pow" => System.Math.Pow(values[0], values[1]),
            "min" => System.Math.Min(values[0], values[1]),
            "max" => System.Math.Max(values[0], values[1]),
            "atan2" => System.Math.Atan2(values[0], values[1]),
            "factorial" => Factorial(values[0]),
            "deg2rad" => values[0] * System.Math.PI / 180.0,
            "rad2deg" => values[0] * 180.0 / System.Math.PI,
            "degrees" => values[0] * 180.0 / System.Math.PI,
            "radians" => values[0] * System.Math.PI / 180.0,
            "gcd" => Gcd(values[0], values[1]),
            "lcm" => Lcm(values[0], values[1]),
            "hypot" => System.Math.Sqrt(values[0] * values[0] + values[1] * values[1]),
            "mod" => values[1] != 0 ? values[0] % values[1] : double.NaN,
            "gamma" => GammaFunction(values[0]),
            "erf" => Erf(values[0]),
            _ => null,
        };
    }

    private double? HandleDivisionByZero(double l, double r)
    {
        _diagnostics.ReportWarning(SemanticDiagnosticCode.DivisionByZero,
            "Division by zero detected during constant folding.");
        return double.NaN;
    }

    private static double Factorial(double n)
    {
        if (n < 0 || n != System.Math.Floor(n)) return double.NaN;
        if (n > 170) return double.PositiveInfinity;
        double result = 1;
        for (int i = 2; i <= (int)n; i++) result *= i;
        return result;
    }

    private static double Gcd(double a, double b)
    {
        a = System.Math.Abs(a); b = System.Math.Abs(b);
        while (b > 0) { var t = b; b = a % b; a = t; }
        return a;
    }

    private static double Lcm(double a, double b) =>
        System.Math.Abs(a * b) / Gcd(a, b);

    private static double GammaFunction(double x)
    {
        if (x <= 0) return double.NaN;
        if (x < 0.5)
            return System.Math.PI / (System.Math.Sin(System.Math.PI * x) * GammaFunction(1.0 - x));
        x -= 1.0;
        double[] g = [0.99999999999980993, 676.5203681218851, -1259.1392167224028,
            771.32342877765313, -176.61502916214059, 12.507343278686905,
            -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7];
        double ag = g[0];
        for (int i = 1; i < g.Length; i++) ag += g[i] / (x + i);
        double t = x + g.Length - 1.5;
        return System.Math.Sqrt(2 * System.Math.PI) * System.Math.Pow(t, x + 0.5) * System.Math.Exp(-t) * ag;
    }

    private static double Erf(double x)
    {
        double t = 1.0 / (1.0 + 0.3275911 * System.Math.Abs(x));
        double poly = t * (0.254829592 + t * (-0.284496736 + t * (1.421413741 + t * (-1.453152027 + t * 1.061405429))));
        double result = 1.0 - poly * System.Math.Exp(-x * x);
        return x >= 0 ? result : -result;
    }
}
