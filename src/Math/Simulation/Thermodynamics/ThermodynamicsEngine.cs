namespace MathVerse.Math.Simulation.Thermodynamics;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;

public sealed record ThermodynamicState
{
    public double Temperature { get; init; }
    public double Pressure { get; init; }
    public double Volume { get; init; }
    public double InternalEnergy { get; init; }
    public double Enthalpy { get; init; }
    public double Entropy { get; init; }
    public double Mass { get; init; }
    public double SpecificHeatCapacity { get; init; }
    public Phase Phase { get; init; }
    public ImmutableDictionary<string, double> SpeciesConcentrations { get; init; } = ImmutableDictionary<string, double>.Empty;
}

public enum Phase
{
    Solid,
    Liquid,
    Gas,
    Plasma,
    Supercritical
}

public sealed record ThermodynamicProperties
{
    public double SpecificHeatConstantVolume { get; init; }
    public double SpecificHeatConstantPressure { get; init; }
    public double ThermalConductivity { get; init; }
    public double Viscosity { get; init; }
    public double ThermalExpansionCoefficient { get; init; }
    public double Compressibility { get; init; }
    public double LatentHeatFusion { get; init; }
    public double LatentHeatVaporization { get; init; }
}

public sealed record HeatTransfer
{
    public HeatTransferMode Mode { get; init; }
    public double Coefficient { get; init; }
    public double Area { get; init; }
    public double TemperatureDifference { get; init; }

    public double HeatFlux => Mode switch
    {
        HeatTransferMode.Conduction => Coefficient * TemperatureDifference,
        HeatTransferMode.Convection => Coefficient * TemperatureDifference,
        HeatTransferMode.Radiation => StefanBoltzmannConstant * Emissivity * (System.Math.Pow(TemperatureHot, 4) - System.Math.Pow(TemperatureCold, 4)),
        _ => 0
    };

    public double Emissivity { get; init; } = 0.9;
    public double TemperatureHot { get; init; }
    public double TemperatureCold { get; init; }
    public const double StefanBoltzmannConstant = 5.670374419e-8;
}

public enum HeatTransferMode
{
    Conduction,
    Convection,
    Radiation,
    PhaseChange
}

public static class ThermodynamicsEngine
{
    public const double GasConstant = 8.314462618;
    public const double BoltzmannConstant = 1.380649e-23;
    public const double AvogadroNumber = 6.02214076e23;

    public static double IdealGasLaw(double pressure, double volume, double moles, double temperature)
        => pressure * volume / (moles * GasConstant * temperature);

    public static double SpecificHeatRatio(double cv, double cp) => cp / cv;

    public static double AdiabaticIndex(double gamma) => gamma;

    public static double IsentropicRelation(double p1, double p2, double gamma)
        => System.Math.Pow(p2 / p1, (gamma - 1) / gamma);

    public static double CarnotEfficiency(double th, double tc)
        => 1 - tc / th;

    public static double HeatCapacity(double mass, double specificHeat)
        => mass * specificHeat;

    public static double HeatTransfer(HeatTransfer ht)
        => ht.HeatFlux * ht.Area;

    public static double EntropyChange(double q, double t)
        => q / t;

    public static double GibbsFreeEnergy(double h, double t, double s)
        => h - t * s;

    public static double ChemicalPotential(double mu0, double t, double p, double p0)
        => mu0 + GasConstant * t * System.Math.Log(p / p0);

    public static double PhaseTransitionTemperature(double latentHeat, double entropyChange)
        => latentHeat / entropyChange;

    public static double ClausiusClapeyron(double latentHeat, double volumeChange, double temperature)
        => latentHeat / (temperature * volumeChange);

    public static ThermodynamicState UpdateState(ThermodynamicState state, double dt, ImmutableArray<HeatTransfer> transfers)
    {
        double heatAdded = transfers.Sum(t => t.HeatFlux * t.Area * 1.0); // 1.0 = dt placeholder
        double newInternalEnergy = state.InternalEnergy + heatAdded;
        double newTemperature = state.Temperature + heatAdded / (state.Mass * state.SpecificHeatCapacity);
        
        return state with { Temperature = newTemperature, InternalEnergy = newInternalEnergy };
    }
}