namespace MathVerse.Math.Foundation;

public sealed class FoundationConfiguration
{
    private bool _enableDimensionChecking = true;
    private bool _enableAutoConversion = false;
    private string _defaultUnitSystem = "SI";
    private int _maxConversionPathLength = 5;
    private bool _enableConstantCaching = true;

    public FoundationConfiguration EnableDimensionChecking(bool enable)
    {
        _enableDimensionChecking = enable;
        return this;
    }

    public FoundationConfiguration EnableAutoConversion(bool enable)
    {
        _enableAutoConversion = enable;
        return this;
    }

    public FoundationConfiguration WithDefaultUnitSystem(string system)
    {
        _defaultUnitSystem = system;
        return this;
    }

    public FoundationConfiguration WithMaxConversionPathLength(int length)
    {
        _maxConversionPathLength = length;
        return this;
    }

    public FoundationConfiguration EnableConstantCaching(bool enable)
    {
        _enableConstantCaching = enable;
        return this;
    }

    public FoundationOptions Build()
    {
        return new FoundationOptions
        {
            EnableDimensionChecking = _enableDimensionChecking,
            EnableAutoConversion = _enableAutoConversion,
            DefaultUnitSystem = _defaultUnitSystem,
            MaxConversionPathLength = _maxConversionPathLength,
            EnableConstantCaching = _enableConstantCaching
        };
    }
}
