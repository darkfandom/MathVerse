namespace MathVerse.Math.Foundation.Dimensions;

public static class DerivedDimension
{
    public static Dimension Velocity { get; } =
        Dimension.FromBaseDimensions(length: 1, time: -1);

    public static Dimension Acceleration { get; } =
        Dimension.FromBaseDimensions(length: 1, time: -2);

    public static Dimension Force { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: 1, time: -2);

    public static Dimension Energy { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: 2, time: -2);

    public static Dimension Power { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: 2, time: -3);

    public static Dimension Pressure { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: -1, time: -2);

    public static Dimension Frequency { get; } =
        Dimension.FromBaseDimensions(time: -1);

    public static Dimension ElectricCharge { get; } =
        Dimension.FromBaseDimensions(current: 1, time: 1);

    public static Dimension Voltage { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: 2, time: -3, current: -1);

    public static Dimension Resistance { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: 2, time: -3, current: -2);

    public static Dimension Capacitance { get; } =
        Dimension.FromBaseDimensions(mass: -1, length: -2, time: 4, current: 2);

    public static Dimension MagneticFlux { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: 2, time: -2, current: -1);

    public static Dimension MagneticField { get; } =
        Dimension.FromBaseDimensions(mass: 1, time: -2, current: -1);

    public static Dimension Area { get; } =
        Dimension.FromBaseDimensions(length: 2);

    public static Dimension Volume { get; } =
        Dimension.FromBaseDimensions(length: 3);

    public static Dimension Density { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: -3);

    public static Dimension MomentOfForce { get; } =
        Dimension.FromBaseDimensions(mass: 1, length: 2, time: -2);

    public static Dimension Create(Dimension baseDim, double exponent) => baseDim.Power(exponent);

    public static Dimension Multiply(Dimension a, Dimension b) => a.Multiply(b);

    public static Dimension Divide(Dimension a, Dimension b) => a.Divide(b);
}
