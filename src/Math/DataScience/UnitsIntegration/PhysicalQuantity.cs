namespace MathVerse.Math.DataScience.UnitsIntegration;

using System;

/// <summary>
/// Represents a physical quantity with a numeric value, a unit string, and a dimension.
/// Supports arithmetic operations that propagate dimensional analysis.
/// </summary>
public sealed class PhysicalQuantity
{
    /// <summary>
    /// Gets or sets the numeric value of the quantity.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the unit string (e.g., "m", "kg/s").
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dimension of this quantity.
    /// </summary>
    public Dimension Dimension { get; set; } = Dimension.Dimensionless;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalQuantity"/> class.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="unit">The unit string.</param>
    /// <param name="dimension">The physical dimension.</param>
    public PhysicalQuantity(double value, string unit, Dimension? dimension = null)
    {
        Value = value;
        Unit = unit ?? string.Empty;
        Dimension = dimension ?? Dimension.Dimensionless;
    }

    /// <summary>
    /// Converts this quantity to the specified target unit using the <see cref="UnitConverter"/>.
    /// </summary>
    /// <param name="targetUnit">The target unit string.</param>
    /// <returns>A new <see cref="PhysicalQuantity"/> with the converted value and target unit.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the dimensions are incompatible for conversion.</exception>
    public PhysicalQuantity ConvertTo(string targetUnit)
    {
        double converted = UnitConverter.Convert(Value, Unit, targetUnit);
        return new PhysicalQuantity(converted, targetUnit, Dimension);
    }

    /// <summary>
    /// Adds another quantity to this quantity. Both quantities must have compatible dimensions.
    /// The result uses this quantity's unit.
    /// </summary>
    /// <param name="other">The quantity to add.</param>
    /// <returns>A new <see cref="PhysicalQuantity"/> representing the sum.</returns>
    /// <exception cref="ArgumentException">Thrown when the quantities have incompatible dimensions.</exception>
    public PhysicalQuantity Add(PhysicalQuantity other)
    {
        if (!Dimension.IsEquivalentTo(other.Dimension))
            throw new ArgumentException(
                $"Cannot add quantities with incompatible dimensions: {Dimension} and {other.Dimension}.");

        double otherValueInThisUnit = UnitConverter.Convert(other.Value, other.Unit, Unit);
        return new PhysicalQuantity(Value + otherValueInThisUnit, Unit, Dimension);
    }

    /// <summary>
    /// Multiplies this quantity by another quantity. The result dimension is the product of both dimensions.
    /// </summary>
    /// <param name="other">The quantity to multiply by.</param>
    /// <returns>A new <see cref="PhysicalQuantity"/> with the product value and combined dimension.</returns>
    public PhysicalQuantity Multiply(PhysicalQuantity other)
    {
        string resultUnit = string.IsNullOrEmpty(Unit) || Unit == "1"
            ? other.Unit
            : string.IsNullOrEmpty(other.Unit) || other.Unit == "1"
                ? Unit
                : $"{Unit}*{other.Unit}";

        return new PhysicalQuantity(Value * other.Value, resultUnit, Dimension * other.Dimension);
    }

    /// <summary>
    /// Divides this quantity by another quantity. The result dimension is the quotient of both dimensions.
    /// </summary>
    /// <param name="other">The quantity to divide by.</param>
    /// <returns>A new <see cref="PhysicalQuantity"/> with the quotient value and combined dimension.</returns>
    public PhysicalQuantity Divide(PhysicalQuantity other)
    {
        if (System.Math.Abs(other.Value) < 1e-15)
            throw new DivideByZeroException("Cannot divide by a quantity with zero value.");

        string resultUnit = string.IsNullOrEmpty(Unit) || Unit == "1"
            ? other.Unit
            : string.IsNullOrEmpty(other.Unit) || other.Unit == "1"
                ? Unit
                : $"{Unit}/{other.Unit}";

        return new PhysicalQuantity(Value / other.Value, resultUnit, Dimension / other.Dimension);
    }

    /// <summary>
    /// Raises this quantity to the specified power. The result dimension is the original dimension raised to the power.
    /// </summary>
    /// <param name="exp">The exponent.</param>
    /// <returns>A new <see cref="PhysicalQuantity"/> with the powered value and dimension.</returns>
    public PhysicalQuantity Power(double exp)
    {
        string resultUnit = $"({Unit})^{exp.ToString("G")}";
        return new PhysicalQuantity(System.Math.Pow(Value, exp), resultUnit, Dimension ^ exp);
    }

    /// <summary>
    /// Returns a string representation of the quantity.
    /// </summary>
    /// <returns>A string like "9.81 m/s^2 [M L T^-2].".</returns>
    public override string ToString()
    {
        return $"{Value:G} {Unit} {Dimension}";
    }

    /// <summary>
    /// Determines whether this quantity equals another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the values, units, and dimensions are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is PhysicalQuantity other
            && System.Math.Abs(Value - other.Value) < 1e-10
            && Unit == other.Unit
            && Dimension.IsEquivalentTo(other.Dimension);
    }

    /// <summary>
    /// Returns the hash code for this quantity.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            return (Value.GetHashCode() * 397) ^ Unit.GetHashCode();
        }
    }
}
