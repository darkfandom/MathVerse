using MathVerse.Math.Foundation.Analysis;
using MathVerse.Math.Foundation.Constants;
using MathVerse.Math.Foundation.Conversion;
using MathVerse.Math.Foundation.Domains;
using MathVerse.Math.Foundation.Units;

namespace MathVerse.Math.Foundation;

public sealed record FoundationServices
{
    public DomainRegistry Domains { get; init; }

    public ConstantRegistry Constants { get; init; }

    public UnitRegistry Units { get; init; }

    public ConversionGraph Conversions { get; init; }

    public DimensionAnalyzer DimensionAnalysis { get; init; }

    public UnitConverter UnitConversion { get; init; }

    public FoundationServices(FoundationOptions? options = null)
    {
        Domains = DomainRegistry.Instance;
        Constants = ConstantRegistry.Instance;
        Units = UnitRegistry.Instance;
        Conversions = ConversionGraph.Instance;
        DimensionAnalysis = DimensionAnalyzer.Instance;
        UnitConversion = UnitConverter.Instance;

        if (options is null || options.EnableConstantCaching)
        {
            RegisterBuiltinConstants();
        }
    }

    private void RegisterBuiltinConstants()
    {
        Constants.Register(BuiltinConstants.Pi);
        Constants.Register(BuiltinConstants.Tau);
        Constants.Register(BuiltinConstants.E);
        Constants.Register(BuiltinConstants.Phi);
        Constants.Register(BuiltinConstants.Gamma);
        Constants.Register(BuiltinConstants.I);
        Constants.Register(BuiltinConstants.Infinity);
        Constants.Register(BuiltinConstants.NaN);
        Constants.Register(BuiltinConstants.Epsilon);
        Constants.Register(BuiltinConstants.Catalan);
        Constants.Register(BuiltinConstants.Apery);
        Constants.Register(BuiltinConstants.FeigenbaumAlpha);
        Constants.Register(BuiltinConstants.FeigenbaumDelta);
    }
}
