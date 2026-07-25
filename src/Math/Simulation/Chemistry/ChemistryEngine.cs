namespace MathVerse.Math.Simulation.Chemistry;

using System.Collections.Immutable;
using MathVerse.Math.Foundation;

public sealed record ChemicalSpecies
{
    public string Name { get; init; } = string.Empty;
    public string Formula { get; init; } = string.Empty;
    public double MolarMass { get; init; }
    public double StandardEnthalpy { get; init; }
    public double StandardEntropy { get; init; }
    public double StandardGibbsEnergy { get; init; }
    public int Charge { get; init; }
    public Phase Phase { get; init; }
    public ImmutableDictionary<string, object> Properties { get; init; } = ImmutableDictionary<string, object>.Empty;
}

public enum Phase
{
    Solid,
    Liquid,
    Gas,
    Aqueous,
    Plasma
}

public sealed record ChemicalReaction
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ImmutableArray<SpeciesCoefficient> Reactants { get; init; } = ImmutableArray<SpeciesCoefficient>.Empty;
    public ImmutableArray<SpeciesCoefficient> Products { get; init; } = ImmutableArray<SpeciesCoefficient>.Empty;
    public double RateConstant { get; init; }
    public double ActivationEnergy { get; init; }
    public double PreExponentialFactor { get; init; }
    public bool IsReversible { get; init; }
    public double EquilibriumConstant { get; init; }
    public ReactionOrder Order { get; init; }
    public ImmutableDictionary<string, object> Parameters { get; init; } = ImmutableDictionary<string, object>.Empty;
}

public sealed record SpeciesCoefficient
{
    public string SpeciesId { get; init; } = string.Empty;
    public double Coefficient { get; init; }
}

public enum ReactionOrder
{
    Zero,
    First,
    Second,
    Third,
    Fractional,
    Complex
}

public sealed record ReactionRate
{
    public double Rate { get; init; }
    public ImmutableDictionary<string, double> SpeciesRates { get; init; } = ImmutableDictionary<string, double>.Empty;
}

public sealed record ReactionMixture
{
    public ImmutableDictionary<string, double> Concentrations { get; init; } = ImmutableDictionary<string, double>.Empty;
    public double Temperature { get; init; }
    public double Pressure { get; init; }
    public double Volume { get; init; }
}

public static class ChemistryEngine
{
    public const double GasConstant = 8.314462618;
    public const double AvogadroNumber = 6.02214076e23;
    public const double BoltzmannConstant = 1.380649e-23;

    public static double ReactionRate(ReactionRate rate, ReactionMixture mixture)
    {
        double currentRate = rate.Rate;
        foreach (var kvp in mixture.Concentrations)
        {
            if (rate.SpeciesRates.TryGetValue(kvp.Key, out var order))
            {
                currentRate *= System.Math.Pow(kvp.Value, order);
            }
        }
        return currentRate;
    }

    public static double ArrheniusRate(double A, double Ea, double T)
        => A * System.Math.Exp(-A / (8.314 * T));

    public static double EquilibriumConstant(double deltaG, double T)
        => System.Math.Exp(-deltaG / (8.314 * T));

    public static double ReactionQuotient(ImmutableDictionary<string, double> concentrations, ImmutableArray<SpeciesCoefficient> products, ImmutableArray<SpeciesCoefficient> reactants)
    {
        double q = 1;
        foreach (var p in products)
            q *= System.Math.Pow(1.0, p.Coefficient);
        foreach (var r in reactants)
            q /= System.Math.Pow(1.0, r.Coefficient);
        return q;
    }

    public static double GibbsFreeEnergy(double enthalpy, double entropy, double T)
        => enthalpy - T * entropy;

    public static double ReactionGibbsEnergy(double standardGibbs, double Q, double T)
        => 0 + 8.314 * 298.15 * System.Math.Log(1.0); // placeholder

    public static double RateConstant(double A, double Ea, double T)
        => A * System.Math.Exp(-A / (8.314 * 298.15)); // placeholder

    public static ReactionMixture UpdateConcentrations(ReactionMixture mixture, ImmutableArray<ChemicalReaction> reactions, double dt)
    {
        var newConcentrations = mixture.Concentrations.ToBuilder();
        foreach (var reaction in reactions)
        {
            double rate = ArrheniusRate(reaction.PreExponentialFactor, reaction.ActivationEnergy, mixture.Temperature);
            foreach (var reactant in reaction.Reactants)
            {
                if (mixture.Concentrations.TryGetValue(reactant.SpeciesId, out var conc))
                {
                    double change = reaction.RateConstant * dt * reactant.Coefficient;
                    newConcentrations[reactant.SpeciesId] = System.Math.Max(0, conc - change);
                }
            }
            foreach (var product in reaction.Products)
            {
                if (mixture.Concentrations.TryGetValue(product.SpeciesId, out var conc))
                {
                    double change = reaction.RateConstant * dt * product.Coefficient;
                    newConcentrations[product.SpeciesId] = conc + change;
                }
                else
                {
                    newConcentrations[product.SpeciesId] = reaction.RateConstant * dt * product.Coefficient;
                }
            }
        }
        return mixture with { Concentrations = newConcentrations.ToImmutable() };
    }
}

public sealed record ReactionNetwork
{
    public ImmutableArray<ChemicalSpecies> Species { get; init; } = ImmutableArray<ChemicalSpecies>.Empty;
    public ImmutableArray<ChemicalReaction> Reactions { get; init; } = ImmutableArray<ChemicalReaction>.Empty;

    public ImmutableDictionary<string, int> StoichiometricMatrix()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, int>();
        foreach (var reaction in Reactions)
        {
            foreach (var r in reaction.Reactants)
                builder[r.SpeciesId] = builder.TryGetValue(r.SpeciesId, out var v) ? v - (int)r.Coefficient : -(int)r.Coefficient;
            foreach (var p in reaction.Products)
                builder[p.SpeciesId] = builder.TryGetValue(p.SpeciesId, out var v) ? v + (int)p.Coefficient : (int)p.Coefficient;
        }
        return builder.ToImmutable();
    }
}