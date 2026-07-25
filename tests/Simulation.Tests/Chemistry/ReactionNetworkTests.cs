namespace MathVerse.Simulation.Tests.Chemistry;

using System.Collections.Immutable;

public sealed class ReactionNetworkTests
{
    [Fact]
    public void StoichiometricMatrix_EmptyReactions()
    {
        var network = new ReactionNetwork();
        network.StoichiometricMatrix().Should().BeEmpty();
    }

    [Fact]
    public void StoichiometricMatrix_SingleReaction_ProductPositive()
    {
        var network = new ReactionNetwork
        {
            Reactions = ImmutableArray.Create(new ChemicalReaction
            {
                Products = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "A", Coefficient = 2 })
            })
        };
        var matrix = network.StoichiometricMatrix();
        matrix["A"].Should().Be(2);
    }

    [Fact]
    public void StoichiometricMatrix_SingleReaction_ReactantNegative()
    {
        var network = new ReactionNetwork
        {
            Reactions = ImmutableArray.Create(new ChemicalReaction
            {
                Reactants = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "B", Coefficient = 3 })
            })
        };
        var matrix = network.StoichiometricMatrix();
        matrix["B"].Should().Be(-3);
    }

    [Fact]
    public void StoichiometricMatrix_BalancedReaction()
    {
        var network = new ReactionNetwork
        {
            Reactions = ImmutableArray.Create(new ChemicalReaction
            {
                Reactants = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "N2", Coefficient = 1 },
                    new SpeciesCoefficient { SpeciesId = "H2", Coefficient = 3 }),
                Products = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "NH3", Coefficient = 2 })
            })
        };
        var matrix = network.StoichiometricMatrix();
        matrix["N2"].Should().Be(-1);
        matrix["H2"].Should().Be(-3);
        matrix["NH3"].Should().Be(2);
    }

    [Fact]
    public void StoichiometricMatrix_MultipleReactions_Accumulates()
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
    public void StoichiometricMatrix_Catalyst_NetZero()
    {
        var network = new ReactionNetwork
        {
            Reactions = ImmutableArray.Create(new ChemicalReaction
            {
                Reactants = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "Cat", Coefficient = 1 }),
                Products = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "Cat", Coefficient = 1 })
            })
        };
        var matrix = network.StoichiometricMatrix();
        matrix["Cat"].Should().Be(0);
    }

    [Fact]
    public void StoichiometricMatrix_HigherCoefficients()
    {
        var network = new ReactionNetwork
        {
            Reactions = ImmutableArray.Create(new ChemicalReaction
            {
                Reactants = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "CH4", Coefficient = 1 },
                    new SpeciesCoefficient { SpeciesId = "O2", Coefficient = 2 }),
                Products = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "CO2", Coefficient = 1 },
                    new SpeciesCoefficient { SpeciesId = "H2O", Coefficient = 2 })
            })
        };
        var matrix = network.StoichiometricMatrix();
        matrix["CH4"].Should().Be(-1);
        matrix["O2"].Should().Be(-2);
        matrix["CO2"].Should().Be(1);
        matrix["H2O"].Should().Be(2);
    }

    [Fact]
    public void StoichiometricMatrix_ThreeReactions_Accumulates()
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
                        new SpeciesCoefficient { SpeciesId = "A", Coefficient = 1 }),
                    Products = ImmutableArray.Create(
                        new SpeciesCoefficient { SpeciesId = "C", Coefficient = 1 })
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
        matrix["A"].Should().Be(-2);
        matrix["B"].Should().Be(0);
        matrix["C"].Should().Be(2);
    }

    [Fact]
    public void StoichiometricMatrix_EmptySpecies()
    {
        var network = new ReactionNetwork
        {
            Species = ImmutableArray<ChemicalSpecies>.Empty,
            Reactions = ImmutableArray.Create(new ChemicalReaction
            {
                Reactants = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "X", Coefficient = 1 }),
                Products = ImmutableArray.Create(
                    new SpeciesCoefficient { SpeciesId = "Y", Coefficient = 1 })
            })
        };
        var matrix = network.StoichiometricMatrix();
        matrix.Should().ContainKey("X");
        matrix.Should().ContainKey("Y");
    }
}
