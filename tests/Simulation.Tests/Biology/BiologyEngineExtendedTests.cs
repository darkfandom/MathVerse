namespace MathVerse.Simulation.Tests.Biology;

using System.Collections.Immutable;
using SM = global::System.Math;

public sealed class BiologyEngineExtendedTests
{
    [Fact]
    public void ExponentialGrowth_RateZero_NoChange()
    {
        double result = BiologyEngine.ExponentialGrowth(100, 0, 1.0);
        result.Should().Be(100);
    }

    [Fact]
    public void GompertzGrowth_SmallPopulation_Grows()
    {
        double result = BiologyEngine.GompertzGrowth(10, 0.1, 1000, 1.0);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LotkaVolterra_ZeroPrey_ZeroPredator()
    {
        var (prey, pred) = BiologyEngine.LotkaVolterra(0, 0, 1, 0.1, 0.5, 0.01, 1.0);
        prey.Should().Be(0);
        pred.Should().Be(0);
    }

    [Fact]
    public void SIRModel_HighRecoveryRate_FewerInfected()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 990, Infected = 10, Recovered = 0, Dead = 0
        };
        var lowRecovery = BiologyEngine.SIRModel(state, 0.3, 0.01, 0, 1.0);
        var highRecovery = BiologyEngine.SIRModel(state, 0.3, 0.5, 0, 1.0);
        highRecovery.Infected.Should().BeLessThan(lowRecovery.Infected);
    }

    [Fact]
    public void SIRModel_AllInfected_RecoveredIncreases()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 0, Infected = 100, Recovered = 0, Dead = 0
        };
        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        result.Recovered.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SIRModel_AllRecovered_NoNewInfections()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 0, Infected = 0, Recovered = 100, Dead = 0
        };
        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        result.Recovered.Should().Be(100);
        result.Infected.Should().Be(0);
    }

    [Fact]
    public void SIRModel_HighDeathRate_MoreDead()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 0, Infected = 100, Recovered = 0, Dead = 0
        };
        var lowMortality = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        var highMortality = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.5, 1.0);
        highMortality.Dead.Should().BeGreaterThan(lowMortality.Dead);
    }

    [Fact]
    public void Diffusion_ZeroGradient_ZeroFlux()
    {
        BiologyEngine.Diffusion(1.0, 0.1, 0.0, 1.0).Should().Be(0);
    }

    [Fact]
    public void Diffusion_HigherCoefficient_HigherFlux()
    {
        double flux1 = BiologyEngine.Diffusion(1.0, 0.1, 1.0, 1.0);
        double flux2 = BiologyEngine.Diffusion(1.0, 1.0, 1.0, 1.0);
        flux2.Should().BeGreaterThan(flux1);
    }

    [Fact]
    public void Migration_ZeroRate_NoMigration()
    {
        BiologyEngine.Migration(100, 0, 1.0).Should().Be(0);
    }

    [Fact]
    public void PopulationModel_DefaultValues()
    {
        var model = new PopulationModel
        {
            Species = ImmutableArray<Species>.Empty,
            Interactions = ImmutableArray<Interaction>.Empty,
            TimeStep = 0.01,
            TotalTime = 10.0
        };
        model.TimeStep.Should().Be(0.01);
        model.TotalTime.Should().Be(10.0);
    }

    [Fact]
    public void Species_Record_AllProperties()
    {
        var s = new Species
        {
            Id = "rabbit",
            Name = "Rabbit",
            Population = 100,
            GrowthRate = 0.5,
            CarryingCapacity = 1000,
            MortalityRate = 0.1,
            DiffusionCoefficient = 0.01,
            BirthRate = 0.6,
            DeathRate = 0.1,
            MigrationRate = 0.05
        };
        s.Id.Should().Be("rabbit");
        s.BirthRate.Should().Be(0.6);
        s.MigrationRate.Should().Be(0.05);
    }

    [Fact]
    public void InteractionType_AllValues()
    {
        InteractionType.Predation.Should().Be(InteractionType.Predation);
        InteractionType.Competition.Should().Be(InteractionType.Competition);
        InteractionType.Mutualism.Should().Be(InteractionType.Mutualism);
        InteractionType.Parasitism.Should().Be(InteractionType.Parasitism);
        InteractionType.Commensalism.Should().Be(InteractionType.Commensalism);
    }

    [Fact]
    public void SIRModel_MultipleSteps_Evolves()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 990, Infected = 10, Recovered = 0, Dead = 0
        };
        var current = state;
        for (int i = 0; i < 10; i++)
            current = BiologyEngine.SIRModel(current, 0.3, 0.1, 0.01, 1.0);
        current.Recovered.Should().BeGreaterThan(state.Recovered);
        current.Susceptible.Should().BeLessThan(state.Susceptible);
    }
}
