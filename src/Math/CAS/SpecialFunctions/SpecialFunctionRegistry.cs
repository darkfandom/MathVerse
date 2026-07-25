using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using MathVerse.Math.Expressions;

namespace MathVerse.Math.CAS.SpecialFunctions;

public sealed class SpecialFunctionRegistry
{
    private static readonly Lazy<SpecialFunctionRegistry> _instance = new(() => new SpecialFunctionRegistry());
    public static SpecialFunctionRegistry Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, SpecialFunction> _functions = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<SpecialFunctionProperty, ImmutableArray<SpecialFunction>> _byProperty = new();

    private SpecialFunctionRegistry()
    {
        RegisterBuiltins();
    }

    private void RegisterBuiltins()
    {
        Register(CreateGamma());
        Register(CreateBeta());
        Register(CreateErf());
        Register(CreateErfc());
        Register(CreateEi());
        Register(CreateSi());
        Register(CreateCi());
        Register(CreateShi());
        Register(CreateChi());
        Register(CreateLi());
        Register(CreateZeta());
        Register(CreateBesselJ());
        Register(CreateBesselY());
        Register(CreateBesselI());
        Register(CreateBesselK());
        Register(CreateLegendreP());
        Register(CreateLegendreQ());
        Register(CreateHermiteH());
        Register(CreateLaguerreL());
        Register(CreateChebyshevT());
        Register(CreateChebyshevU());
        Register(CreateHypergeometric0F1());
        Register(CreateHypergeometric1F1());
        Register(CreateHypergeometric2F1());
        Register(CreateMeijerG());
    }

    public SpecialFunction? Get(string name)
    {
        return _functions.TryGetValue(name, out var func) ? func : null;
    }

    public void Register(SpecialFunction func)
    {
        _functions[func.Name] = func;
        foreach (var alias in func.Aliases)
            _functions[alias] = func;

        foreach (var prop in func.Properties)
        {
            _byProperty.AddOrUpdate(prop,
                _ => [func],
                (_, existing) => existing.Add(func));
        }
    }

    public ImmutableArray<SpecialFunction> GetAll()
        => _functions.Values.Distinct().ToImmutableArray();

    public ImmutableArray<SpecialFunction> GetByProperty(SpecialFunctionProperty prop)
        => _byProperty.TryGetValue(prop, out var funcs) ? funcs : ImmutableArray<SpecialFunction>.Empty;

    private SpecialFunction CreateGamma() => new SpecialFunction
    {
        Name = "Gamma",
        Aliases = ["Γ", "gamma"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Gamma requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(GammaFunction(c.Value));
            }
            return Expr.Call("Gamma", args.ToArray());
        }
    };

    private SpecialFunction CreateBeta() => new SpecialFunction
    {
        Name = "Beta",
        Aliases = ["B", "beta"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("Beta requires 2 arguments");
            if (args[0] is LiteralExpression a && args[1] is LiteralExpression b)
            {
                return Expr.Literal(BetaFunction(a.Value, b.Value));
            }
            return Expr.Call("Beta", args.ToArray());
        }
    };

    private SpecialFunction CreateErf() => new SpecialFunction
    {
        Name = "Erf",
        Aliases = ["erf"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.Odd, SpecialFunctionProperty.RealForReal, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Erf requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(Erf(c.Value));
            }
            return Expr.Call("Erf", args.ToArray());
        }
    };

    private SpecialFunction CreateErfc() => new SpecialFunction
    {
        Name = "Erfc",
        Aliases = ["erfc"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.RealForReal, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Erfc requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(Erfc(c.Value));
            }
            return Expr.Call("Erfc", args.ToArray());
        }
    };

    private SpecialFunction CreateEi() => new SpecialFunction
    {
        Name = "Ei",
        Aliases = ["Ei", "expint"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Ei requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(ExpIntegralEi(c.Value));
            }
            return Expr.Call("Ei", args.ToArray());
        }
    };

    private SpecialFunction CreateSi() => new SpecialFunction
    {
        Name = "Si",
        Aliases = ["Si", "sineintegral"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.Odd, SpecialFunctionProperty.RealForReal, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Si requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(SineIntegral(c.Value));
            }
            return Expr.Call("Si", args.ToArray());
        }
    };

    private SpecialFunction CreateCi() => new SpecialFunction
    {
        Name = "Ci",
        Aliases = ["Ci", "cosineintegral"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.RealForReal, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Ci requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(CosineIntegral(c.Value));
            }
            return Expr.Call("Ci", args.ToArray());
        }
    };

    private SpecialFunction CreateShi() => new SpecialFunction
    {
        Name = "Shi",
        Aliases = ["Shi", "hyperbolicsineintegral"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.Odd, SpecialFunctionProperty.RealForReal, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Shi requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(HyperbolicSineIntegral(c.Value));
            }
            return Expr.Call("Shi", args.ToArray());
        }
    };

    private SpecialFunction CreateChi() => new SpecialFunction
    {
        Name = "Chi",
        Aliases = ["Chi", "hyperboliccosineintegral"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.RealForReal, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Chi requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(HyperbolicCosineIntegral(c.Value));
            }
            return Expr.Call("Chi", args.ToArray());
        }
    };

    private SpecialFunction CreateLi() => new SpecialFunction
    {
        Name = "Polylog",
        Aliases = ["Li", "polylog"],
        Properties = [SpecialFunctionProperty.Analytic, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("Polylog requires 2 arguments (s, z)");
            if (args[0] is LiteralExpression s && args[1] is LiteralExpression z)
            {
                return Expr.Literal(Polylog(s.Value, z.Value));
            }
            return Expr.Call("Polylog", args.ToArray());
        }
    };

    private SpecialFunction CreateZeta() => new SpecialFunction
    {
        Name = "Zeta",
        Aliases = ["ζ", "zeta"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 1) throw new ArgumentException("Zeta requires 1 argument");
            if (args[0] is LiteralExpression c)
            {
                return Expr.Literal(Zeta(c.Value));
            }
            return Expr.Call("Zeta", args.ToArray());
        }
    };

    private SpecialFunction CreateBesselJ() => new SpecialFunction
    {
        Name = "BesselJ",
        Aliases = ["J", "besselj"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("BesselJ requires 2 arguments (nu, z)");
            if (args[0] is LiteralExpression nu && args[1] is LiteralExpression z)
            {
                return Expr.Literal(BesselJ(nu.Value, z.Value));
            }
            return Expr.Call("BesselJ", args.ToArray());
        }
    };

    private SpecialFunction CreateBesselY() => new SpecialFunction
    {
        Name = "BesselY",
        Aliases = ["Y", "bessely"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("BesselY requires 2 arguments (nu, z)");
            if (args[0] is LiteralExpression nu && args[1] is LiteralExpression z)
            {
                return Expr.Literal(BesselY(nu.Value, z.Value));
            }
            return Expr.Call("BesselY", args.ToArray());
        }
    };

    private SpecialFunction CreateBesselI() => new SpecialFunction
    {
        Name = "BesselI",
        Aliases = ["I", "besseli"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("BesselI requires 2 arguments (nu, z)");
            if (args[0] is LiteralExpression nu && args[1] is LiteralExpression z)
            {
                return Expr.Literal(BesselI(nu.Value, z.Value));
            }
            return Expr.Call("BesselI", args.ToArray());
        }
    };

    private SpecialFunction CreateBesselK() => new SpecialFunction
    {
        Name = "BesselK",
        Aliases = ["K", "besselk"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("BesselK requires 2 arguments (nu, z)");
            if (args[0] is LiteralExpression nu && args[1] is LiteralExpression z)
            {
                return Expr.Literal(BesselK(nu.Value, z.Value));
            }
            return Expr.Call("BesselK", args.ToArray());
        }
    };

    private SpecialFunction CreateLegendreP() => new SpecialFunction
    {
        Name = "LegendreP",
        Aliases = ["P", "legendrep"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length < 2 || args.Length > 3) throw new ArgumentException("LegendreP requires 2 or 3 arguments (nu, mu, z)");
            if (args.Length == 3 && args[0] is LiteralExpression nu && args[1] is LiteralExpression mu && args[2] is LiteralExpression z)
            {
                return Expr.Literal(LegendreP(nu.Value, mu.Value, z.Value));
            }
            return Expr.Call("LegendreP", args.ToArray());
        }
    };

    private SpecialFunction CreateLegendreQ() => new SpecialFunction
    {
        Name = "LegendreQ",
        Aliases = ["Q", "legendreq"],
        Properties = [SpecialFunctionProperty.Meromorphic, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length < 2 || args.Length > 3) throw new ArgumentException("LegendreQ requires 2 or 3 arguments (nu, mu, z)");
            if (args.Length == 3 && args[0] is LiteralExpression nu && args[1] is LiteralExpression mu && args[2] is LiteralExpression z)
            {
                return Expr.Literal(LegendreQ(nu.Value, mu.Value, z.Value));
            }
            return Expr.Call("LegendreQ", args.ToArray());
        }
    };

    private SpecialFunction CreateHermiteH() => new SpecialFunction
    {
        Name = "HermiteH",
        Aliases = ["H", "hermiteh"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("HermiteH requires 2 arguments (n, z)");
            if (args[0] is LiteralExpression n && args[1] is LiteralExpression z)
            {
                return Expr.Literal(HermiteH((int)n.Value, z.Value));
            }
            return Expr.Call("HermiteH", args.ToArray());
        }
    };

    private SpecialFunction CreateLaguerreL() => new SpecialFunction
    {
        Name = "LaguerreL",
        Aliases = ["L", "laguerrel"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length < 2 || args.Length > 3) throw new ArgumentException("LaguerreL requires 2 or 3 arguments (n, alpha, z)");
            if (args.Length == 3 && args[0] is LiteralExpression n && args[1] is LiteralExpression alpha && args[2] is LiteralExpression z)
            {
                return Expr.Literal(LaguerreL((int)n.Value, alpha.Value, z.Value));
            }
            return Expr.Call("LaguerreL", args.ToArray());
        }
    };

    private SpecialFunction CreateChebyshevT() => new SpecialFunction
    {
        Name = "ChebyshevT",
        Aliases = ["T", "chebyshevt"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("ChebyshevT requires 2 arguments (n, z)");
            if (args[0] is LiteralExpression n && args[1] is LiteralExpression z)
            {
                return Expr.Literal(ChebyshevT((int)n.Value, z.Value));
            }
            return Expr.Call("ChebyshevT", args.ToArray());
        }
    };

    private SpecialFunction CreateChebyshevU() => new SpecialFunction
    {
        Name = "ChebyshevU",
        Aliases = ["U", "chebyshevu"],
        Properties = [SpecialFunctionProperty.Entire, SpecialFunctionProperty.SatisfiesDE, SpecialFunctionProperty.RealForReal],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("ChebyshevU requires 2 arguments (n, z)");
            if (args[0] is LiteralExpression n && args[1] is LiteralExpression z)
            {
                return Expr.Literal(ChebyshevU((int)n.Value, z.Value));
            }
            return Expr.Call("ChebyshevU", args.ToArray());
        }
    };

    private SpecialFunction CreateHypergeometric0F1() => new SpecialFunction
    {
        Name = "Hypergeometric0F1",
        Aliases = ["0F1", "hypergeometric0f1"],
        Properties = [SpecialFunctionProperty.Analytic, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 2) throw new ArgumentException("Hypergeometric0F1 requires 2 arguments (b, z)");
            if (args[0] is LiteralExpression b && args[1] is LiteralExpression z)
            {
                return Expr.Literal(Hypergeometric0F1(b.Value, z.Value));
            }
            return Expr.Call("Hypergeometric0F1", args.ToArray());
        }
    };

    private SpecialFunction CreateHypergeometric1F1() => new SpecialFunction
    {
        Name = "Hypergeometric1F1",
        Aliases = ["1F1", "hypergeometric1f1", "KummerM"],
        Properties = [SpecialFunctionProperty.Analytic, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 3) throw new ArgumentException("Hypergeometric1F1 requires 3 arguments (a, b, z)");
            if (args[0] is LiteralExpression a && args[1] is LiteralExpression b && args[2] is LiteralExpression z)
            {
                return Expr.Literal(Hypergeometric1F1(a.Value, b.Value, z.Value));
            }
            return Expr.Call("Hypergeometric1F1", args.ToArray());
        }
    };

    private SpecialFunction CreateHypergeometric2F1() => new SpecialFunction
    {
        Name = "Hypergeometric2F1",
        Aliases = ["2F1", "hypergeometric2f1", "GaussHypergeometric"],
        Properties = [SpecialFunctionProperty.Analytic, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            if (args.Length != 4) throw new ArgumentException("Hypergeometric2F1 requires 4 arguments (a, b, c, z)");
            if (args[0] is LiteralExpression a && args[1] is LiteralExpression b && args[2] is LiteralExpression c && args[3] is LiteralExpression z)
            {
                return Expr.Literal(Hypergeometric2F1(a.Value, b.Value, c.Value, z.Value));
            }
            return Expr.Call("Hypergeometric2F1", args.ToArray());
        }
    };

    private SpecialFunction CreateMeijerG() => new SpecialFunction
    {
        Name = "MeijerG",
        Aliases = ["meijerg"],
        Properties = [SpecialFunctionProperty.Analytic, SpecialFunctionProperty.SatisfiesDE],
        Evaluator = args =>
        {
            return Expr.Call("MeijerG", args.ToArray());
        }
    };

    private static double GammaFunction(double x) => System.Math.Exp(LogGamma(x));
    private static double LogGamma(double x) => LogGammaApproximation(x);
    private static double BetaFunction(double a, double b) => System.Math.Exp(LogGamma(a) + LogGamma(b) - LogGamma(a + b));
    private static double Erf(double x) => ErfApproximation(x);
    private static double Erfc(double x) => 1.0 - Erf(x);
    private static double ExpIntegralEi(double x) => x < 0 ? -ExpIntegralE1(-x) : double.NaN;
    private static double ExpIntegralE1(double x) => double.NaN;
    private static double SineIntegral(double x) => double.NaN;
    private static double CosineIntegral(double x) => double.NaN;
    private static double HyperbolicSineIntegral(double x) => double.NaN;
    private static double HyperbolicCosineIntegral(double x) => double.NaN;
    private static double Polylog(double s, double z) => double.NaN;
    private static double Zeta(double s) => double.NaN;
    private static double BesselJ(double nu, double z) => double.NaN;
    private static double BesselY(double nu, double z) => double.NaN;
    private static double BesselI(double nu, double z) => double.NaN;
    private static double BesselK(double nu, double z) => double.NaN;
    private static double LegendreP(double nu, double mu, double z) => double.NaN;
    private static double LegendreQ(double nu, double mu, double z) => double.NaN;
    private static double HermiteH(int n, double z) => double.NaN;
    private static double LaguerreL(int n, double alpha, double z) => double.NaN;
    private static double ChebyshevT(int n, double z) => double.NaN;
    private static double ChebyshevU(int n, double z) => double.NaN;
    private static double Hypergeometric0F1(double b, double z) => double.NaN;
    private static double Hypergeometric1F1(double a, double b, double z) => double.NaN;
    private static double Hypergeometric2F1(double a, double b, double c, double z) => double.NaN;

    private static double LogGammaApproximation(double x)
    {
        if (x <= 0) return double.NaN;
        
        double[] p = {
            0.99999999999980993, 676.5203681218851, -1259.1392167224028,
            771.32342877765313, -176.61502916214059, 12.507343278686905,
            -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7
        };

        if (x < 0.5) return System.Math.Log(System.Math.PI / (System.Math.Sin(System.Math.PI * x) * LogGammaApproximation(1 - x)));

        x -= 1;
        double a = p[0];
        for (int i = 1; i < p.Length; i++)
        {
            a += p[i] / (x + i);
        }

        double t = x + p.Length - 0.5;
        return System.Math.Log(a * System.Math.Sqrt(2 * System.Math.PI)) + (x + 0.5) * System.Math.Log(t) - t;
    }

    private static double ErfApproximation(double x)
    {
        double sign = x >= 0 ? 1 : -1;
        x = System.Math.Abs(x);
        
        double t = 1.0 / (1.0 + 0.3275911 * x);
        double poly = t * (0.254829592 + t * (-0.284496736 + t * (1.421413741 + t * (-1.453152027 + t * 1.061405429))));
        return sign * (1.0 - poly * System.Math.Exp(-x * x));
    }
}