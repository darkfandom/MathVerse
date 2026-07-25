using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Foundation.Units;

namespace MathVerse.Math.Foundation.Quantities;

public sealed record PhysicalQuantity : IComparable<PhysicalQuantity>
{
    public double Value { get; init; }

    public Unit Unit { get; init; } = default!;

    public Dimension Dimension { get; init; } = Dimension.None;

    public PhysicalQuantity ConvertTo(Unit target)
    {
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (!IsDimensionallyCompatible(target))
            throw new InvalidOperationException($"Cannot convert from {Unit?.Symbol} to {target.Symbol}: incompatible dimensions");
        var baseValue = Value * (Unit?.ScaleFactor ?? 1.0);
        var convertedValue = baseValue / target.ScaleFactor;
        return this with { Value = convertedValue, Unit = target, Dimension = target.Dimension };
    }

    public PhysicalQuantity ToBase()
    {
        if (Unit is null) return this;
        return this with { Value = Value * Unit.ScaleFactor, Unit = Unit with { ScaleFactor = 1.0 } };
    }

    public bool IsDimensionallyCompatible(PhysicalQuantity other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        return Dimension.IsCompatibleWith(other.Dimension);
    }

    public bool IsDimensionallyCompatible(Unit unit)
    {
        if (unit is null) throw new ArgumentNullException(nameof(unit));
        return Dimension.IsCompatibleWith(unit.Dimension);
    }

    public int CompareTo(PhysicalQuantity? other)
    {
        if (other is null) return 1;
        if (!IsDimensionallyCompatible(other))
            throw new InvalidOperationException("Cannot compare quantities with incompatible dimensions");
        var thisBase = ToBase().Value;
        var otherBase = other.ToBase().Value;
        return thisBase.CompareTo(otherBase);
    }

    public static PhysicalQuantity operator +(PhysicalQuantity left, PhysicalQuantity right)
    {
        if (!left.IsDimensionallyCompatible(right))
            throw new InvalidOperationException("Cannot add quantities with incompatible dimensions");
        var rightInLeft = right.ConvertTo(left.Unit);
        return left with { Value = left.Value + rightInLeft.Value };
    }

    public static PhysicalQuantity operator -(PhysicalQuantity left, PhysicalQuantity right)
    {
        if (!left.IsDimensionallyCompatible(right))
            throw new InvalidOperationException("Cannot subtract quantities with incompatible dimensions");
        var rightInLeft = right.ConvertTo(left.Unit);
        return left with { Value = left.Value - rightInLeft.Value };
    }

    public static PhysicalQuantity operator *(PhysicalQuantity left, PhysicalQuantity right)
    {
        var newDimension = left.Dimension.Multiply(right.Dimension);
        var newUnit = left.Unit;
        return new PhysicalQuantity { Value = left.Value * right.Value, Unit = newUnit, Dimension = newDimension };
    }

    public static PhysicalQuantity operator /(PhysicalQuantity left, PhysicalQuantity right)
    {
        var newDimension = left.Dimension.Divide(right.Dimension);
        var newUnit = left.Unit;
        return new PhysicalQuantity { Value = left.Value / right.Value, Unit = newUnit, Dimension = newDimension };
    }

    public static PhysicalQuantity operator *(PhysicalQuantity quantity, double scalar)
    {
        return quantity with { Value = quantity.Value * scalar };
    }

    public static PhysicalQuantity operator *(double scalar, PhysicalQuantity quantity)
    {
        return quantity with { Value = quantity.Value * scalar };
    }

    public static PhysicalQuantity operator /(PhysicalQuantity quantity, double scalar)
    {
        return quantity with { Value = quantity.Value / scalar };
    }

    public static PhysicalQuantity operator -(PhysicalQuantity quantity)
    {
        return quantity with { Value = -quantity.Value };
    }

    public override string ToString()
    {
        var unitSymbol = Unit?.Symbol ?? "";
        return $"{Value} {unitSymbol}".Trim();
    }
}
