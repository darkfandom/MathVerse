namespace MathVerse.Math.Simulation.Electromagnetics;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;
using MathVerse.Math.Numerics.LinearAlgebra;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed record ElectromagneticField
{
    public MVVector ElectricField { get; init; } = MVVector.Zero;
    public MVVector MagneticField { get; init; } = MVVector.Zero;
    public MVVector ElectricDisplacement { get; init; } = MVVector.Zero;
    public MVVector MagneticIntensity { get; init; } = MVVector.Zero;
    public double ChargeDensity { get; init; }
    public MVVector CurrentDensity { get; init; } = MVVector.Zero;
    public double Permittivity { get; init; }
    public double Permeability { get; init; }

    public static ElectromagneticField Vacuum() => new()
    {
        ElectricField = MVVector.Zero,
        MagneticField = MVVector.Zero,
        ElectricDisplacement = MVVector.Zero,
        MagneticIntensity = MVVector.Zero,
        ChargeDensity = 0,
        CurrentDensity = MVVector.Zero,
        Permittivity = 8.854187817e-12,
        Permeability = 4 * System.Math.PI * 1e-7
    };

    public double ElectricEnergyDensity => 0.5 * Permittivity * ElectricField.Dot(ElectricField);
    public double MagneticEnergyDensity => 0.5 * MagneticField.Dot(MagneticField) / Permeability;
    public MVVector PoyntingVector => new MVVector(ElectricField[1]*MagneticIntensity[2]-ElectricField[2]*MagneticIntensity[1], ElectricField[2]*MagneticIntensity[0]-ElectricField[0]*MagneticIntensity[2], ElectricField[0]*MagneticIntensity[1]-ElectricField[1]*MagneticIntensity[0]);
}

public sealed record ElectromagneticSource
{
    public string Id { get; init; } = string.Empty;
    public SourceType Type { get; init; }
    public MVVector Position { get; init; } = MVVector.Zero;
    public double Charge { get; init; }
    public MVVector Current { get; init; } = MVVector.Zero;
    public double Frequency { get; init; }
    public double Amplitude { get; init; }
    public double Phase { get; init; }
}

public enum SourceType
{
    PointCharge,
    CurrentElement,
    Dipole,
    Antenna,
    Capacitor,
    Inductor
}

public static class ElectromagneticsEngine
{
    public const double VacuumPermittivity = 8.854187817e-12;
    public const double VacuumPermeability = 4 * System.Math.PI * 1e-7;
    public const double SpeedOfLight = 299792458;

    public static MVVector CoulombForce(MVVector r, double q1, double q2)
    {
        var rMag = r.Norm();
        if (rMag < 1e-15) return MVVector.Zero;
        var forceMag = q1 * q2 / (4 * System.Math.PI * VacuumPermittivity * rMag * rMag);
        return VectorOperations.Normalize(r).Scale(forceMag);
    }

    public static MVVector LorentzForce(MVVector velocity, MVVector electricField, MVVector magneticField, double charge)
    {
        var electricForce = electricField.Scale(charge);
        var magneticForce = new MVVector(velocity[1]*magneticField[2]-velocity[2]*magneticField[1], velocity[2]*magneticField[0]-velocity[0]*magneticField[2], velocity[0]*magneticField[1]-velocity[1]*magneticField[0]).Scale(charge);
        return electricForce.Add(magneticForce);
    }

    public static MVVector BiotSavart(MVVector currentElement, MVVector r)
    {
        var rMag = r.Norm();
        if (rMag < 1e-15) return MVVector.Zero;
        var cross = new MVVector(currentElement[1]*r[2]-currentElement[2]*r[1], currentElement[2]*r[0]-currentElement[0]*r[2], currentElement[0]*r[1]-currentElement[1]*r[0]);
        return cross.Scale(VacuumPermeability / (4 * System.Math.PI * System.Math.Pow(rMag, 3)));
    }

    public static MVVector MagneticFieldWire(MVVector current, MVVector position, MVVector wirePosition)
    {
        var r = position.Subtract(wirePosition);
        var distance = r.Norm();
        if (distance < 1e-15) return MVVector.Zero;
        var direction = VectorOperations.Normalize(new MVVector(current[1]*r[2]-current[2]*r[1], current[2]*r[0]-current[0]*r[2], current[0]*r[1]-current[1]*r[0]));
        return direction.Scale(VacuumPermeability * current.Norm() / (2 * System.Math.PI * distance));
    }

    public static MVVector ElectricFieldPointCharge(MVVector r, double charge)
    {
        var rMag = r.Norm();
        if (rMag < 1e-15) return MVVector.Zero;
        return VectorOperations.Normalize(r).Scale(charge / (4 * System.Math.PI * VacuumPermittivity * rMag * rMag));
    }

    public static double Inductance(double length, double radius, int turns)
        => VacuumPermeability * turns * turns * System.Math.PI * radius * radius / length;

    public static double Capacitance(double area, double separation, double permittivity)
        => permittivity * area / separation;

    public static double ResonanceFrequency(double inductance, double capacitance)
        => 1.0 / (2 * System.Math.PI * System.Math.Sqrt(inductance * capacitance));

    public static double PoyntingMagnitude(MVVector e, MVVector h) => new MVVector(e[1]*h[2]-e[2]*h[1], e[2]*h[0]-e[0]*h[2], e[0]*h[1]-e[1]*h[0]).Norm();

    public static double WaveImpedance(double epsilon, double mu)
        => System.Math.Sqrt(mu / epsilon);

    public static MVVector SkinDepth(MVVector conductivity, double frequency, double permeability)
    {
        var delta = System.Math.Sqrt(2.0 / (conductivity.Norm() * frequency * VacuumPermeability * permeability));
        return MVVector.One(3).Scale(delta);
    }
}

public sealed record Capacitor
{
    public double Capacitance { get; init; }
    public double Voltage { get; init; }
    public double Charge { get; init; }
    public double Energy => 0.5 * Charge * Voltage;
}

public sealed record Inductor
{
    public double Inductance { get; init; }
    public double Current { get; init; }
    public double Energy => 0.5 * Inductance * Current * Current;
}

public sealed record ElectromagneticWave
{
    public MVVector WaveVector { get; init; } = MVVector.Zero;
    public double Frequency { get; init; }
    public MVVector Polarization { get; init; } = MVVector.Zero;
    public double Amplitude { get; init; }
    public double Phase { get; init; }

    public double Wavelength => Frequency > 0 ? ElectromagneticsEngine.SpeedOfLight / Frequency : 0;
    public double WaveNumber => Frequency > 0 ? 2 * System.Math.PI / Wavelength : 0;
}
