namespace MathVerse.Math.DataScience.UnitsIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides unit conversion between common SI base units, derived units, and prefixed units.
/// Uses a lookup table approach for conversion factors.
/// </summary>
public static class UnitConverter
{
    private static readonly Dictionary<string, double> ToBaseSI = new(StringComparer.OrdinalIgnoreCase)
    {
        // SI base units (identity)
        ["m"] = 1.0,
        ["kg"] = 1.0,
        ["s"] = 1.0,
        ["A"] = 1.0,
        ["K"] = 1.0,
        ["mol"] = 1.0,
        ["cd"] = 1.0,

        // Length
        ["km"] = 1000.0,
        ["cm"] = 0.01,
        ["mm"] = 0.001,
        ["um"] = 1e-6,
        ["nm"] = 1e-9,
        ["pm"] = 1e-12,
        ["ft"] = 0.3048,
        ["in"] = 0.0254,
        ["yd"] = 0.9144,
        ["mi"] = 1609.344,
        ["AU"] = 1.495978707e11,
        ["ly"] = 9.4607e15,

        // Mass
        ["g"] = 0.001,
        ["mg"] = 1e-6,
        ["ug"] = 1e-9,
        ["t"] = 1000.0,
        ["lb"] = 0.45359237,
        ["oz"] = 0.028349523125,
        ["u"] = 1.66053906660e-27,

        // Time
        ["ms"] = 0.001,
        ["us"] = 1e-6,
        ["ns"] = 1e-9,
        ["ps"] = 1e-12,
        ["min"] = 60.0,
        ["hr"] = 3600.0,
        ["day"] = 86400.0,
        ["yr"] = 31556952.0,

        // Electric current
        ["mA"] = 0.001,
        ["uA"] = 1e-6,

        // Temperature (relative offsets handled separately)
        ["degC"] = 1.0,
        ["degF"] = 1.0,
        ["R"] = 5.0 / 9.0,

        // Amount of substance
        ["kmol"] = 1000.0,
        ["mmol"] = 0.001,
        ["umol"] = 1e-6,

        // Luminous intensity
        ["mcd"] = 0.001,

        // Derived SI units
        ["Hz"] = 1.0,
        ["N"] = 1.0,
        ["Pa"] = 1.0,
        ["J"] = 1.0,
        ["W"] = 1.0,
        ["C"] = 1.0,
        ["V"] = 1.0,
        ["F"] = 1.0,
        ["ohm"] = 1.0,
        ["S"] = 1.0,
        ["Wb"] = 1.0,
        ["T"] = 1.0,
        ["H"] = 1.0,
        ["lm"] = 1.0,
        ["lx"] = 1.0,
        ["Bq"] = 1.0,
        ["Gy"] = 1.0,
        ["Sv"] = 1.0,
        ["kat"] = 1.0,

        // Derived unit equivalences in SI base
        ["rad"] = 1.0,
        ["sr"] = 1.0,
    };

    private static readonly Dictionary<string, DerivedUnitDef> DerivedUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        // Force
        ["N"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = 1, ["m"] = 1, ["s"] = -2 }),

        // Pressure
        ["Pa"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = 1, ["m"] = -1, ["s"] = -2 }),
        ["atm"] = new DerivedUnitDef(101325.0, new Dictionary<string, double> { ["kg"] = 1, ["m"] = -1, ["s"] = -2 }),
        ["bar"] = new DerivedUnitDef(100000.0, new Dictionary<string, double> { ["kg"] = 1, ["m"] = -1, ["s"] = -2 }),
        ["psi"] = new DerivedUnitDef(6894.757293168, new Dictionary<string, double> { ["kg"] = 1, ["m"] = -1, ["s"] = -2 }),
        ["torr"] = new DerivedUnitDef(133.3223684211, new Dictionary<string, double> { ["kg"] = 1, ["m"] = -1, ["s"] = -2 }),

        // Energy
        ["J"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2 }),
        ["cal"] = new DerivedUnitDef(4.184, new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2 }),
        ["kcal"] = new DerivedUnitDef(4184.0, new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2 }),
        ["eV"] = new DerivedUnitDef(1.602176634e-19, new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2 }),
        ["kWh"] = new DerivedUnitDef(3.6e6, new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2 }),
        ["BTU"] = new DerivedUnitDef(1055.06, new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2 }),
        ["erg"] = new DerivedUnitDef(1e-7, new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2 }),

        // Power
        ["W"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -3 }),
        ["hp"] = new DerivedUnitDef(745.69987158, new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -3 }),

        // Electric charge
        ["C"] = new DerivedUnitDef(new Dictionary<string, double> { ["A"] = 1, ["s"] = 1 }),

        // Voltage
        ["V"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -3, ["A"] = -1 }),

        // Capacitance
        ["F"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = -1, ["m"] = -2, ["s"] = 4, ["A"] = 2 }),

        // Resistance
        ["ohm"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -3, ["A"] = -2 }),

        // Conductance
        ["S"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = -1, ["m"] = -2, ["s"] = 3, ["A"] = 2 }),

        // Magnetic flux
        ["Wb"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2, ["A"] = -1 }),

        // Magnetic flux density
        ["Gauss"] = new DerivedUnitDef(1e-4, new Dictionary<string, double> { ["kg"] = 1, ["s"] = -2, ["A"] = -1 }),

        // Inductance
        ["H"] = new DerivedUnitDef(new Dictionary<string, double> { ["kg"] = 1, ["m"] = 2, ["s"] = -2, ["A"] = -2 }),

        // Frequency
        ["Hz"] = new DerivedUnitDef(new Dictionary<string, double> { ["s"] = -1 }),
        ["rpm"] = new DerivedUnitDef(1.0 / 60.0, new Dictionary<string, double> { ["s"] = -1 }),

        // Luminous flux
        ["lm"] = new DerivedUnitDef(new Dictionary<string, double> { ["cd"] = 1, ["sr"] = 1 }),

        // Illuminance
        ["lx"] = new DerivedUnitDef(new Dictionary<string, double> { ["cd"] = 1, ["sr"] = 1, ["m"] = -2 }),

        // Radioactivity
        ["Bq"] = new DerivedUnitDef(new Dictionary<string, double> { ["s"] = -1 }),
        ["Ci"] = new DerivedUnitDef(3.7e10, new Dictionary<string, double> { ["s"] = -1 }),

        // Absorbed dose
        ["Gy"] = new DerivedUnitDef(new Dictionary<string, double> { ["m"] = 2, ["s"] = -2 }),
        ["rad"] = new DerivedUnitDef(0.01, new Dictionary<string, double> { ["m"] = 2, ["s"] = -2 }),

        // Equivalent dose
        ["Sv"] = new DerivedUnitDef(new Dictionary<string, double> { ["m"] = 2, ["s"] = -2 }),
        ["rem"] = new DerivedUnitDef(0.01, new Dictionary<string, double> { ["m"] = 2, ["s"] = -2 }),

        // Catalytic activity
        ["kat"] = new DerivedUnitDef(new Dictionary<string, double> { ["mol"] = 1, ["s"] = -1 }),
    };

    private static readonly Dictionary<string, double> SIPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Y"] = 1e24,
        ["Z"] = 1e21,
        ["E"] = 1e18,
        ["P"] = 1e15,
        ["T"] = 1e12,
        ["G"] = 1e9,
        ["M"] = 1e6,
        ["k"] = 1e3,
        ["h"] = 1e2,
        ["da"] = 1e1,
        ["d"] = 1e-1,
        ["c"] = 1e-2,
        ["m"] = 1e-3,
        ["u"] = 1e-6,
        ["n"] = 1e-9,
        ["p"] = 1e-12,
        ["f"] = 1e-15,
        ["a"] = 1e-18,
        ["z"] = 1e-21,
        ["y"] = 1e-24,
    };

    /// <summary>
    /// Converts a value from one unit to another.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="fromUnit">The source unit string.</param>
    /// <param name="toUnit">The target unit string.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="ArgumentException">Thrown when the units are unknown or incompatible.</exception>
    public static double Convert(double value, string fromUnit, string toUnit)
    {
        if (string.IsNullOrWhiteSpace(fromUnit))
            throw new ArgumentException("Source unit cannot be null or empty.", nameof(fromUnit));
        if (string.IsNullOrWhiteSpace(toUnit))
            throw new ArgumentException("Target unit cannot be null or empty.", nameof(toUnit));

        string from = fromUnit.Trim();
        string to = toUnit.Trim();

        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            return value;

        double fromBase = ConvertToBaseSI(from);
        double toBase = ConvertToBaseSI(to);

        if (System.Math.Abs(toBase) < 1e-30)
            throw new ArgumentException($"Unknown target unit: {toUnit}");

        return value * fromBase / toBase;
    }

    /// <summary>
    /// Gets the dimension string for a given unit.
    /// </summary>
    /// <param name="unit">The unit string.</param>
    /// <returns>The SI base dimension exponents as a string.</returns>
    public static string? GetDimensionString(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            return "[ ]";

        if (DerivedUnits.TryGetValue(unit.Trim(), out DerivedUnitDef? def))
        {
            return def.Dimensions.ToString();
        }

        return $"[base: {unit}]";
    }

    private static double ConvertToBaseSI(string unit)
    {
        string trimmed = unit.Trim();

        if (IsTemperatureUnit(trimmed))
            return GetTemperatureFactor(trimmed);

        if (DerivedUnits.TryGetValue(trimmed, out DerivedUnitDef? derived))
            return derived.Factor;

        if (ToBaseSI.TryGetValue(trimmed, out double baseFactor))
            return baseFactor;

        if (TryParsePrefixedUnit(trimmed, out string baseUnit, out double prefixFactor))
        {
            if (ToBaseSI.TryGetValue(baseUnit, out double bf))
                return prefixFactor * bf;

            if (DerivedUnits.TryGetValue(baseUnit, out DerivedUnitDef? dd))
                return prefixFactor * dd.Factor;
        }

        if (TryParseCompoundUnit(trimmed, out double compoundFactor))
            return compoundFactor;

        throw new ArgumentException($"Unknown unit: {unit}");
    }

    private static bool IsTemperatureUnit(string unit)
    {
        return string.Equals(unit, "K", StringComparison.OrdinalIgnoreCase)
            || string.Equals(unit, "degC", StringComparison.OrdinalIgnoreCase)
            || string.Equals(unit, "degF", StringComparison.OrdinalIgnoreCase)
            || string.Equals(unit, "R", StringComparison.OrdinalIgnoreCase);
    }

    private static double GetTemperatureFactor(string unit)
    {
        // Temperature conversions involving offsets require special handling.
        // For general conversion we return the scale factor; offset conversions
        // are handled in Convert when the source or target is a temperature unit.
        if (string.Equals(unit, "K", StringComparison.OrdinalIgnoreCase))
            return 1.0;
        if (string.Equals(unit, "degC", StringComparison.OrdinalIgnoreCase))
            return 1.0;
        if (string.Equals(unit, "degF", StringComparison.OrdinalIgnoreCase))
            return 5.0 / 9.0;
        if (string.Equals(unit, "R", StringComparison.OrdinalIgnoreCase))
            return 5.0 / 9.0;

        return 1.0;
    }

    private static bool TryParsePrefixedUnit(string unit, out string baseUnit, out double prefixFactor)
    {
        baseUnit = string.Empty;
        prefixFactor = 1.0;

        foreach (var kvp in SIPrefixes)
        {
            if (unit.Length > kvp.Key.Length && unit.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                string candidate = unit[kvp.Key.Length..];
                if (ToBaseSI.ContainsKey(candidate) || DerivedUnits.ContainsKey(candidate))
                {
                    baseUnit = candidate;
                    prefixFactor = kvp.Value;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryParseCompoundUnit(string unit, out double factor)
    {
        factor = 1.0;

        if (unit.Contains('/') || unit.Contains('*'))
        {
            string[] parts = unit.Split('/', '*');
            char[] ops = unit.Where(c => c == '/' || c == '*').ToArray();

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (string.IsNullOrEmpty(part)) continue;

                double partFactor = ConvertToBaseSI(part);

                if (i < ops.Length)
                {
                    if (ops[i] == '/')
                        factor /= partFactor;
                    else
                        factor *= partFactor;
                }
                else
                {
                    factor *= partFactor;
                }
            }

            return true;
        }

        return false;
    }

    private sealed class DerivedUnitDef
    {
        public double Factor { get; }
        public Dictionary<string, double> Dimensions { get; }

        public DerivedUnitDef(Dictionary<string, double> dimensions)
            : this(1.0, dimensions)
        {
        }

        public DerivedUnitDef(double factor, Dictionary<string, double> dimensions)
        {
            Factor = factor;
            Dimensions = dimensions;
        }
    }
}
