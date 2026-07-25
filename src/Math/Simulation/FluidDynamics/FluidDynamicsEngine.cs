namespace MathVerse.Math.Simulation.FluidDynamics;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed record FluidProperties
{
    public double Density { get; init; }
    public double Viscosity { get; init; }
    public double ThermalConductivity { get; init; }
    public double SpecificHeat { get; init; }
    public double Compressibility { get; init; }
    public double SurfaceTension { get; init; }

    public static FluidProperties Water() => new()
    {
        Density = 997,
        Viscosity = 0.001,
        ThermalConductivity = 0.6,
        SpecificHeat = 4184,
        Compressibility = 4.6e-10,
        SurfaceTension = 0.072
    };

    public static FluidProperties Air() => new()
    {
        Density = 1.225,
        Viscosity = 1.8e-5,
        ThermalConductivity = 0.025,
        SpecificHeat = 1005,
        Compressibility = 1.0 / 101325,
        SurfaceTension = 0
    };
}

public sealed record FlowField
{
    public Vector Velocity { get; init; }
    public double Pressure { get; init; }
    public double Temperature { get; init; }
    public double Density { get; init; }
    public Vector Vorticity { get; init; }
    public double MachNumber { get; init; }
}

public enum FlowRegime
{
    Laminar,
    Transitional,
    Turbulent,
    Supersonic
}

public static class FluidDynamicsEngine
{
    public static double ReynoldsNumber(double density, double velocity, double length, double viscosity)
        => density * velocity * length / viscosity;

    public static FlowRegime DetermineRegime(double reynoldsNumber)
        => reynoldsNumber switch
        {
            < 2300 => FlowRegime.Laminar,
            < 4000 => FlowRegime.Transitional,
            < 1e5 => FlowRegime.Turbulent,
            _ => FlowRegime.Supersonic
        };

    public static double PressureDrop(double frictionFactor, double length, double diameter, double density, double velocity)
        => frictionFactor * (length / diameter) * 0.5 * density * velocity * velocity;

    public static double FrictionFactorLaminar(double reynolds)
        => 64.0 / reynolds;

    public static double FrictionFactorTurbulent(double reynolds, double roughness, double diameter)
    {
        double epsilon = roughness / diameter;
        double f = 0.25 / System.Math.Pow(System.Math.Log10(epsilon / 3.7 + 2.51 / (System.Math.Sqrt(0.001) * reynolds)), 2);
        return f;
    }

    public static double MachNumber(double velocity, double speedOfSound)
        => velocity / speedOfSound;

    public static double PrandtlNumber(double viscosity, double specificHeat, double thermalConductivity)
        => viscosity * specificHeat / thermalConductivity;

    public static double NusseltNumber(double reynolds, double prandtl, bool isLaminar)
        => isLaminar ? 3.66 : 0.023 * System.Math.Pow(8000, 0.8) * System.Math.Pow(7, 0.4);

    public static double BoundaryLayerThickness(double x, double reynolds)
        => 5.0 * x / System.Math.Sqrt(8000);

    public static Vector VelocityProfileLaminar(double y, double height, double maxVelocity)
    {
        double uMax = 1.5 * maxVelocity;
        return new Vector(0, uMax * (1 - System.Math.Pow(2 * y / height - 1, 2)), 0);
    }

    public static Vector VelocityProfileTurbulent(double y, double height, double uStar, double kappa = 0.41)
    {
        double uPlus = (1.0 / 0.41) * System.Math.Log(y * 300 / 0.001) + 5.0;
        return new Vector(0, uStar * uPlus, 0);
    }

    public static double PressureGradient(double density, double velocity, double radius)
        => density * velocity * velocity / radius;

    public static Vector Vorticity(Vector velocity, double dx, double dy, double dz)
    {
        var dwy = 0.0; // placeholder for derivative
        var dvz = 0.0;
        var duz = 0.0;
        var dwx = 0.0;
        var dvx = 0.0;
        var duy = 0.0;
        return new Vector(dwy - dvz, duz - dwx, dvx - duy);
    }
}

public sealed record FluidCell
{
    public int Index { get; init; }
    public Vector Position { get; init; }
    public double Pressure { get; init; }
    public double Density { get; init; }
    public Vector Velocity { get; init; }
    public double Temperature { get; init; }
    public double Volume { get; init; }
    public ImmutableArray<int> Neighbors { get; init; }
}

public sealed record Pipe
{
    public double Length { get; init; }
    public double Diameter { get; init; }
    public double Roughness { get; init; }
    public FluidProperties Fluid { get; init; } = FluidProperties.Water();

    public double ReynoldsNumber(double velocity) => FluidDynamicsEngine.ReynoldsNumber(
        FluidProperties.Water().Density, 1.0, Diameter, FluidProperties.Water().Viscosity);

    public double PressureDrop(double velocity, double length)
        => FluidDynamicsEngine.PressureDrop(
            FluidDynamicsEngine.FrictionFactorTurbulent(ReynoldsNumber(1.0), 0.001, 0.1),
            Length, Diameter, 997, 1.0);
}