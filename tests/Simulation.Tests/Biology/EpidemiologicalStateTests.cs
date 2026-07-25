namespace MathVerse.Simulation.Tests.Biology;

using SM = global::System.Math;

public sealed class EpidemiologicalStateTests
{
    [Fact]
    public void Default_ZeroPopulation()
    {
        var state = new EpidemiologicalState();
        state.TotalPopulation.Should().Be(0);
    }

    [Fact]
    public void TotalPopulation_SumsAllCompartments()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 100,
            Infected = 10,
            Recovered = 5,
            Dead = 2
        };
        state.TotalPopulation.Should().Be(117);
    }

    [Fact]
    public void TotalPopulation_OnlyInfected()
    {
        var state = new EpidemiologicalState { Infected = 50 };
        state.TotalPopulation.Should().Be(50);
    }

    [Fact]
    public void SIRModel_DecreasesSusceptible()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 990,
            Infected = 10,
            Recovered = 0,
            Dead = 0
        };
        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        result.Susceptible.Should().BeLessThan(state.Susceptible);
    }

    [Fact]
    public void SIRModel_IncreasesRecovered()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 990,
            Infected = 10,
            Recovered = 0,
            Dead = 0
        };
        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        result.Recovered.Should().BeGreaterThan(state.Recovered);
    }

    [Fact]
    public void SIRModel_ZeroInfected_NoChange()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 1000,
            Infected = 0,
            Recovered = 0,
            Dead = 0
        };
        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        result.Susceptible.Should().Be(state.Susceptible);
        result.Infected.Should().Be(0);
    }

    [Fact]
    public void SIRModel_HighTransmission_RapidSpread()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 999,
            Infected = 1,
            Recovered = 0,
            Dead = 0
        };
        var lowBeta = BiologyEngine.SIRModel(state, 0.1, 0.1, 0, 1.0);
        var highBeta = BiologyEngine.SIRModel(state, 0.9, 0.1, 0, 1.0);
        highBeta.Infected.Should().BeGreaterThan(lowBeta.Infected);
    }

    [Fact]
    public void SIRModel_TotalPopulationApproximatelyConserved()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 900,
            Infected = 100,
            Recovered = 0,
            Dead = 0
        };
        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        result.TotalPopulation.Should().BeApproximately(state.TotalPopulation, 1.0);
    }

    [Fact]
    public void SIRModel_InfectedNonNegative()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 10,
            Infected = 5,
            Recovered = 0,
            Dead = 0
        };
        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        result.Infected.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void SIRModel_SusceptibleNonNegative()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 2,
            Infected = 1,
            Recovered = 0,
            Dead = 0
        };
        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
        result.Susceptible.Should().BeGreaterThanOrEqualTo(0);
    }
}
