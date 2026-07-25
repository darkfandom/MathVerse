using MathVerse.Math.Foundation.Dimensions;

namespace MathVerse.Math.Foundation.Quantities;

public static class QuantityOperations
{
    public static PhysicalQuantity Add(PhysicalQuantity left, PhysicalQuantity right) => left + right;

    public static PhysicalQuantity Subtract(PhysicalQuantity left, PhysicalQuantity right) => left - right;

    public static PhysicalQuantity Multiply(PhysicalQuantity left, PhysicalQuantity right) => left * right;

    public static PhysicalQuantity Divide(PhysicalQuantity left, PhysicalQuantity right) => left / right;

    public static PhysicalQuantity Scale(PhysicalQuantity quantity, double scalar) => quantity * scalar;

    public static PhysicalQuantity Negate(PhysicalQuantity quantity) => -quantity;

    public static PhysicalQuantity Abs(PhysicalQuantity quantity)
    {
        return quantity with { Value = System.Math.Abs(quantity.Value) };
    }

    public static PhysicalQuantity Pow(PhysicalQuantity quantity, double exponent)
    {
        var newDimension = quantity.Dimension.Power(exponent);
        return quantity with { Value = System.Math.Pow(quantity.Value, exponent), Dimension = newDimension };
    }

    public static PhysicalQuantity Sqrt(PhysicalQuantity quantity)
    {
        return Pow(quantity, 0.5);
    }

    public static PhysicalQuantity Max(PhysicalQuantity a, PhysicalQuantity b)
    {
        return a.CompareTo(b) >= 0 ? a : b;
    }

    public static PhysicalQuantity Min(PhysicalQuantity a, PhysicalQuantity b)
    {
        return a.CompareTo(b) <= 0 ? a : b;
    }
}
