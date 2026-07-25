namespace MathVerse.Math.Foundation.Dimensions;

public enum BaseDimension
{
    Length,
    Mass,
    Time,
    ElectricCurrent,
    Temperature,
    AmountOfSubstance,
    LuminousIntensity
}

public static class BaseDimensionExtensions
{
    public static string Symbol(this BaseDimension dim) => dim switch
    {
        BaseDimension.Length => "L",
        BaseDimension.Mass => "M",
        BaseDimension.Time => "T",
        BaseDimension.ElectricCurrent => "I",
        BaseDimension.Temperature => "K",
        BaseDimension.AmountOfSubstance => "N",
        BaseDimension.LuminousIntensity => "J",
        _ => throw new ArgumentOutOfRangeException(nameof(dim))
    };

    public static string DisplayName(this BaseDimension dim) => dim switch
    {
        BaseDimension.Length => "Length",
        BaseDimension.Mass => "Mass",
        BaseDimension.Time => "Time",
        BaseDimension.ElectricCurrent => "Electric Current",
        BaseDimension.Temperature => "Temperature",
        BaseDimension.AmountOfSubstance => "Amount of Substance",
        BaseDimension.LuminousIntensity => "Luminous Intensity",
        _ => throw new ArgumentOutOfRangeException(nameof(dim))
    };

    public static BaseDimension FromSymbol(string symbol) => symbol switch
    {
        "L" => BaseDimension.Length,
        "M" => BaseDimension.Mass,
        "T" => BaseDimension.Time,
        "I" => BaseDimension.ElectricCurrent,
        "K" => BaseDimension.Temperature,
        "N" => BaseDimension.AmountOfSubstance,
        "J" => BaseDimension.LuminousIntensity,
        _ => throw new ArgumentOutOfRangeException(nameof(symbol))
    };
}
