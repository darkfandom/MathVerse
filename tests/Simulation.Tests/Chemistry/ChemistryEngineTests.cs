namespace MathVerse.Simulation.Tests.Chemistry;

using System.Collections.Immutable;

public sealed class ChemistryEngineTests
{
    [Fact]
    public void GasConstant_MatchesKnownValue()
    {
        ChemistryEngine.GasConstant.Should().BeApproximately(8.314462618, 1e-9);
    }

    [Fact]
    public void AvogadroNumber_MatchesKnownValue()
    {
        ChemistryEngine.AvogadroNumber.Should().BeApproximately(6.02214076e23, 1e14);
    }

    [Fact]
    public void BoltzmannConstant_MatchesKnownValue()
    {
        ChemistryEngine.BoltzmannConstant.Should().BeApproximately(1.380649e-23, 1e-30);
    }

    [Fact]
    public void ArrheniusRate_AtHigherTemperature_HigherRate()
    {
        double A = 1;
        double Ea = 50000;
        double rateLow = ChemistryEngine.ArrheniusRate(A, Ea, 300);
        double rateHigh = ChemistryEngine.ArrheniusRate(A, Ea, 600);
        rateHigh.Should().BeGreaterThan(rateLow);
    }

    [Fact]
    public void ArrheniusRate_AtZeroTemperature_ReturnsZero()
    {
        double rate = ChemistryEngine.ArrheniusRate(1e10, 50000, 0);
        rate.Should().Be(0);
    }

    [Fact]
    public void ArrheniusRate_PositiveExponentialFactor()
    {
        double rate = ChemistryEngine.ArrheniusRate(1, 50000, 300);
        rate.Should().BeGreaterThan(0);
    }

    [Fact]
    public void EquilibriumConstant_ZeroDeltaG_ReturnsOne()
    {
        double K = ChemistryEngine.EquilibriumConstant(0, 298.15);
        K.Should().BeApproximately(1.0, 1e-6);
    }

    [Fact]
    public void EquilibriumConstant_PositiveDeltaG_LessThanOne()
    {
        double K = ChemistryEngine.EquilibriumConstant(10000, 298.15);
        K.Should().BeLessThan(1.0);
        K.Should().BeGreaterThan(0);
    }

    [Fact]
    public void EquilibriumConstant_NegativeDeltaG_GreaterThanOne()
    {
        double K = ChemistryEngine.EquilibriumConstant(-10000, 298.15);
        K.Should().BeGreaterThan(1.0);
    }

    [Fact]
    public void EquilibriumConstant_HighTemperature_ApproachesOne()
    {
        double K = ChemistryEngine.EquilibriumConstant(1000, 100000);
        K.Should().BeApproximately(1.0, 0.1);
    }

    [Fact]
    public void ReactionQuotient_EmptyProductsAndReactants_ReturnsOne()
    {
        var products = ImmutableArray<SpeciesCoefficient>.Empty;
        var reactants = ImmutableArray<SpeciesCoefficient>.Empty;
        var conc = ImmutableDictionary<string, double>.Empty;

        double Q = ChemistryEngine.ReactionQuotient(conc, products, reactants);
        Q.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void GibbsFreeEnergy_ExampleCalculation()
    {
        double H = 1000;
        double S = 2;
        double T = 300;
        double G = ChemistryEngine.GibbsFreeEnergy(H, S, T);
        G.Should().BeApproximately(1000 - 300 * 2, 1e-10);
    }

    [Fact]
    public void GibbsFreeEnergy_ZeroEntropy_ReturnsEnthalpy()
    {
        double G = ChemistryEngine.GibbsFreeEnergy(5000, 0, 300);
        G.Should().BeApproximately(5000, 1e-10);
    }

    [Fact]
    public void ReactionGibbsEnergy_ReturnsConsistentValue()
    {
        double result = ChemistryEngine.ReactionGibbsEnergy(1000, 1.0, 298.15);
        result.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void RateConstant_PositivePreExponential()
    {
        double k = ChemistryEngine.RateConstant(1, 50000, 300);
        k.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ReactionRate_SingleSpecies()
    {
        var rate = new ReactionRate
        {
            Rate = 0.5,
            SpeciesRates = ImmutableDictionary<string, double>.Empty.Add("A", 1.0)
        };
        var mixture = new ReactionMixture
        {
            Concentrations = ImmutableDictionary<string, double>.Empty.Add("A", 2.0),
            Temperature = 300
        };

        double result = ChemistryEngine.ReactionRate(rate, mixture);
        result.Should().BeApproximately(0.5 * 2.0, 1e-10);
    }

    [Fact]
    public void ReactionRate_NoSpeciesRates_ReturnsBaseRate()
    {
        var rate = new ReactionRate { Rate = 3.0 };
        var mixture = new ReactionMixture
        {
            Concentrations = ImmutableDictionary<string, double>.Empty.Add("A", 5.0),
            Temperature = 300
        };

        double result = ChemistryEngine.ReactionRate(rate, mixture);
        result.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void ReactionNetwork_StoichiometricMatrix_SimpleReaction()
    {
        var network = new ReactionNetwork
        {
            Reactions = ImmutableArray.Create(new ChemicalReaction
            {
                Reactants = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "A", Coefficient = 2 },
                    new SpeciesCoefficient { SpeciesId = "B", Coefficient = 1 }),
                Products = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "C", Coefficient = 1 })
            })
        };

        var matrix = network.StoichiometricMatrix();
        matrix["A"].Should().Be(-2);
        matrix["B"].Should().Be(-1);
        matrix["C"].Should().Be(1);
    }

    [Fact]
    public void ReactionNetwork_StoichiometricMatrix_MultipleReactions()
    {
        var network = new ReactionNetwork
        {
            Reactions = ImmutableArray.Create(
                new ChemicalReaction
                {
                    Reactants = ImmutableArray.Create(
                        new SpeciesCoefficient { SpeciesId = "A", Coefficient = 1 }),
                    Products = ImmutableArray.Create(
                        new SpeciesCoefficient { SpeciesId = "B", Coefficient = 1 })
                },
                new ChemicalReaction
                {
                    Reactants = ImmutableArray.Create(
                        new SpeciesCoefficient { SpeciesId = "B", Coefficient = 1 }),
                    Products = ImmutableArray.Create(
                        new SpeciesCoefficient { SpeciesId = "C", Coefficient = 1 })
                })
        };

        var matrix = network.StoichiometricMatrix();
        matrix["A"].Should().Be(-1);
        matrix["B"].Should().Be(0);
        matrix["C"].Should().Be(1);
    }

    [Fact]
    public void ReactionNetwork_StoichiometricMatrix_EmptyReactions()
    {
        var network = new ReactionNetwork();
        var matrix = network.StoichiometricMatrix();
        matrix.Should().BeEmpty();
    }

    [Fact]
    public void ChemicalSpecies_DefaultProperties()
    {
        var species = new ChemicalSpecies();
        species.Name.Should().Be(string.Empty);
        species.Formula.Should().Be(string.Empty);
        species.MolarMass.Should().Be(0);
        species.Charge.Should().Be(0);
    }

    [Fact]
    public void ChemicalReaction_DefaultProperties()
    {
        var reaction = new ChemicalReaction();
        reaction.Id.Should().Be(string.Empty);
        reaction.Name.Should().Be(string.Empty);
        reaction.RateConstant.Should().Be(0);
        reaction.ActivationEnergy.Should().Be(0);
        reaction.IsReversible.Should().BeFalse();
    }
}
