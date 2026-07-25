using MathVerse.Math.Foundation.Analysis;
using MathVerse.Math.Foundation.Constants;
using MathVerse.Math.Foundation.Conversion;
using MathVerse.Math.Foundation.Domains;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Foundation.Integration;
using MathVerse.Math.Foundation.Quantities;
using MathVerse.Math.Foundation.Units;
using MathVerse.Math.Expressions;

namespace MathVerse.Math.Foundation;

public sealed class FoundationEngine
{
    public FoundationServices Services { get; }

    public FoundationEngine() : this(new FoundationOptions())
    {
    }

    public FoundationEngine(FoundationOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        Services = new FoundationServices(options);
    }

    public MathDomain? GetDomain(DomainKind kind)
    {
        return Services.Domains.Get(kind);
    }

    public MathDomain? GetDomain(string name)
    {
        return Services.Domains.Get(name);
    }

    public bool AreDomainsCompatible(MathDomain a, MathDomain b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        return a.IsCompatibleWith(b);
    }

    public MathConstant? GetConstant(string nameOrSymbol)
    {
        return Services.Constants.Get(nameOrSymbol);
    }

    public double GetConstantValue(string symbol)
    {
        var constant = Services.Constants.Get(symbol);
        if (constant is null)
            throw new ArgumentException($"Unknown constant: {symbol}", nameof(symbol));
        return constant.NumericValue;
    }

    public bool TryGetConstant(string symbol, out double value)
    {
        var constant = Services.Constants.Get(symbol);
        if (constant is not null)
        {
            value = constant.NumericValue;
            return true;
        }
        value = default;
        return false;
    }

    public Unit? GetUnit(string symbol)
    {
        return Services.Units.Get(symbol);
    }

    public IReadOnlyList<Unit> GetUnitsByCategory(UnitCategory cat)
    {
        return Services.Units.GetByCategory(cat);
    }

    public IReadOnlyList<Unit> GetUnitsByDimension(Dimension dim)
    {
        return Services.Units.GetByDimension(dim);
    }

    public PhysicalQuantity CreateQuantity(double value, string unitSymbol)
    {
        return QuantityFactory.Instance.Create(value, unitSymbol);
    }

    public PhysicalQuantity Convert(PhysicalQuantity q, Unit target)
    {
        if (q is null) throw new ArgumentNullException(nameof(q));
        if (target is null) throw new ArgumentNullException(nameof(target));
        return q.ConvertTo(target);
    }

    public Dimension? AnalyzeExpression(Expression expr)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        return Services.DimensionAnalysis.AnalyzeExpression(expr);
    }

    public bool CheckConsistency(Expression expr)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        Services.DimensionAnalysis.CheckDimensionalConsistency(expr);
        return !Services.DimensionAnalysis.Diagnostics.HasErrors;
    }

    public IReadOnlyList<DimensionDiagnostic> GetDiagnostics(Expression expr)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        Services.DimensionAnalysis.CheckDimensionalConsistency(expr);
        return Services.DimensionAnalysis.Diagnostics.Diagnostics;
    }

    public ConversionResult Convert(double value, string fromUnit, string toUnit)
    {
        return Services.UnitConversion.Convert(value, fromUnit, toUnit);
    }

    public bool CanConvert(string fromUnit, string toUnit)
    {
        return Services.UnitConversion.CanConvert(fromUnit, toUnit);
    }

    public Expression WithDimensions(Expression expr, Dictionary<string, Dimension> vars)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        if (vars is null) throw new ArgumentNullException(nameof(vars));
        return expr.WithDimensions(vars);
    }

    public PhysicalQuantity? EvaluateAsQuantity(Expression expr, Dictionary<string, PhysicalQuantity> vars)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));
        if (vars is null) throw new ArgumentNullException(nameof(vars));
        return expr.EvaluateAsQuantity(vars);
    }

    public void Clear()
    {
        Services.DimensionAnalysis.Clear();
    }
}
