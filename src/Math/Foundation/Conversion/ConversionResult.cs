namespace MathVerse.Math.Foundation.Conversion;

public sealed record ConversionResult
{
    public bool Success { get; init; }

    public double ConvertedValue { get; init; }

    public ConversionPath? Path { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public static ConversionResult Succeeded(double value, ConversionPath path) => new()
    {
        Success = true,
        ConvertedValue = value,
        Path = path
    };

    public static ConversionResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
