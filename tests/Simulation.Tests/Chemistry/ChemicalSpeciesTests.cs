namespace MathVerse.Simulation.Tests.Chemistry;

using System.Collections.Immutable;
using ChemPhase = MathVerse.Math.Simulation.Chemistry.Phase;

public sealed class ChemicalSpeciesTests
{
    [Fact]
    public void Properties_CanBeSet()
    {
        var species = new ChemicalSpecies
        {
            Name = "Water",
            Formula = "H2O",
            MolarMass = 18.015,
            StandardEnthalpy = -285830,
            StandardEntropy = 69.91,
            StandardGibbsEnergy = -237130,
            Charge = 0,
            Phase = ChemPhase.Liquid
        };

        species.Name.Should().Be("Water");
        species.Formula.Should().Be("H2O");
        species.MolarMass.Should().Be(18.015);
        species.StandardEnthalpy.Should().Be(-285830);
        species.StandardEntropy.Should().Be(69.91);
        species.StandardGibbsEnergy.Should().Be(-237130);
        species.Charge.Should().Be(0);
        species.Phase.Should().Be(ChemPhase.Liquid);
    }

    [Fact]
    public void DefaultProperties_AreEmpty()
    {
        var species = new ChemicalSpecies();
        species.Properties.Should().BeEmpty();
    }

    [Fact]
    public void Phase_SupportsAllValues()
    {
        ChemPhase.Solid.Should().Be(ChemPhase.Solid);
        ChemPhase.Liquid.Should().Be(ChemPhase.Liquid);
        ChemPhase.Gas.Should().Be(ChemPhase.Gas);
        ChemPhase.Aqueous.Should().Be(ChemPhase.Aqueous);
        ChemPhase.Plasma.Should().Be(ChemPhase.Plasma);
    }

    [Fact]
    public void ChemicalReaction_ReactantsAndProducts()
    {
        var reaction = new ChemicalReaction
        {
            Id = "r1",
            Name = "Combustion",
            Reactants = ImmutableArray.Create(
                new SpeciesCoefficient { SpeciesId = "CH4", Coefficient = 1 },
                new SpeciesCoefficient { SpeciesId = "O2", Coefficient = 2 }),
            Products = ImmutableArray.Create(
                new SpeciesCoefficient { SpeciesId = "CO2", Coefficient = 1 },
                new SpeciesCoefficient { SpeciesId = "H2O", Coefficient = 2 }),
            RateConstant = 0.05,
            ActivationEnergy = 80000,
            PreExponentialFactor = 1e8,
            IsReversible = false,
            Order = ReactionOrder.Second
        };

        reaction.Reactants.Should().HaveCount(2);
        reaction.Products.Should().HaveCount(2);
        reaction.Reactants[0].SpeciesId.Should().Be("CH4");
        reaction.Products[0].SpeciesId.Should().Be("CO2");
    }

    [Fact]
    public void SpeciesCoefficient_DefaultValues()
    {
        var sc = new SpeciesCoefficient();
        sc.SpeciesId.Should().Be(string.Empty);
        sc.Coefficient.Should().Be(0);
    }

    [Fact]
    public void ChemicalReaction_Reversible()
    {
        var reaction = new ChemicalReaction
        {
            IsReversible = true,
            EquilibriumConstant = 1.5
        };
        reaction.IsReversible.Should().BeTrue();
        reaction.EquilibriumConstant.Should().Be(1.5);
    }

    [Fact]
    public void ChemicalReaction_Parameters_DefaultIsEmpty()
    {
        var reaction = new ChemicalReaction();
        reaction.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void ChemicalSpecies_ChargedSpecies()
    {
        var species = new ChemicalSpecies
        {
            Name = "Sodium Ion",
            Formula = "Na+",
            MolarMass = 22.990,
            Charge = 1,
            Phase = ChemPhase.Aqueous
        };
        species.Charge.Should().Be(1);
        species.Phase.Should().Be(ChemPhase.Aqueous);
    }

    [Fact]
    public void ReactionOrder_VariousValues()
    {
        ReactionOrder.Zero.Should().Be(ReactionOrder.Zero);
        ReactionOrder.First.Should().Be(ReactionOrder.First);
        ReactionOrder.Second.Should().Be(ReactionOrder.Second);
        ReactionOrder.Third.Should().Be(ReactionOrder.Third);
        ReactionOrder.Fractional.Should().Be(ReactionOrder.Fractional);
        ReactionOrder.Complex.Should().Be(ReactionOrder.Complex);
    }

    [Fact]
    public void ReactionMixture_DefaultProperties()
    {
        var mixture = new ReactionMixture();
        mixture.Concentrations.Should().BeEmpty();
        mixture.Temperature.Should().Be(0);
        mixture.Pressure.Should().Be(0);
        mixture.Volume.Should().Be(0);
    }
}
