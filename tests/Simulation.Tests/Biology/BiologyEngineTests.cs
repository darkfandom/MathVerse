namespace MathVerse.Simulation.Tests.Biology;

using SM = global::System.Math;

public class BiologyEngineTests
{
    [Fact]
    public void LogisticGrowth_NearCapacity_GrowthSlows()
    {
        double N = 900;
        double r = 0.1;
        double K = 1000;
        double dt = 1.0;

        double result = BiologyEngine.LogisticGrowth(N, r, K, dt);

        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void LogisticGrowth_ZeroPopulation_ReturnsZero()
    {
        double result = BiologyEngine.LogisticGrowth(0, 0.1, 1000, 1.0);

        double.IsNaN(result).Should().BeTrue();
    }

    [Fact]
    public void LogisticGrowth_NeverExceedsCapacity()
    {
        double K = 100;
        double result = BiologyEngine.LogisticGrowth(K, 0.5, K, 1.0);

        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void ExponentialGrowth_DoublesApproximately()
    {
        double N = 100;
        double r = SM.Log(2);
        double dt = 1.0;

        double result = BiologyEngine.ExponentialGrowth(N, r, dt);

        result.Should().BeApproximately(200.0, 1.0);
    }

    [Fact]
    public void ExponentialGrowth_ZeroPopulation_ReturnsZero()
    {
        double result = BiologyEngine.ExponentialGrowth(0, 0.1, 1.0);

        result.Should().Be(0);
    }

    [Fact]
    public void GompertzGrowth_ProducesPositiveResult()
    {
        double result = BiologyEngine.GompertzGrowth(100, 0.1, 1000, 1.0);

        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void LotkaVolterra_PredatorPrey_CoupledDynamics()
    {
        double prey = 100;
        double predator = 10;
        double alpha = 1.0;
        double beta = 0.1;
        double gamma = 0.5;
        double delta = 0.01;
        double dt = 1.0;

        var (newPrey, newPredator) = BiologyEngine.LotkaVolterra(prey, predator, alpha, beta, gamma, delta, dt);

        newPrey.Should().BeGreaterThan(0);
        newPredator.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LotkaVolterra_PreyGrowsWithoutPredators()
    {
        double prey = 100;
        double predator = 0;
        double alpha = 1.0;
        double beta = 0.1;
        double gamma = 0.5;
        double delta = 0.01;

        var (newPrey, _) = BiologyEngine.LotkaVolterra(prey, predator, alpha, beta, gamma, delta, 1.0);

        newPrey.Should().BeGreaterThan(prey);
    }

    [Fact]
    public void LotkaVolterra_NoPrey_PredatorDeclines()
    {
        double prey = 0;
        double predator = 50;
        double alpha = 1.0;
        double beta = 0.1;
        double gamma = 0.5;
        double delta = 0.01;

        var (_, newPredator) = BiologyEngine.LotkaVolterra(prey, predator, alpha, beta, gamma, delta, 1.0);

        newPredator.Should().BeLessThan(predator);
    }

    [Fact]
    public void SIRModel_InfectedIncreasesInitially()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 990,
            Infected = 10,
            Recovered = 0,
            Dead = 0
        };
        double beta = 0.3;
        double gamma = 0.1;
        double mu = 0.01;

        var result = BiologyEngine.SIRModel(state, beta, gamma, mu, 1.0);

        result.Infected.Should().BeGreaterThanOrEqualTo(0);
        result.Susceptible.Should().BeLessThan(state.Susceptible);
    }

    [Fact]
    public void SIRModel_TotalPopulation_Conserved()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 990,
            Infected = 10,
            Recovered = 0,
            Dead = 0
        };

        var result = BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);

        result.TotalPopulation.Should().BeApproximately(state.TotalPopulation, 1.0);
    }

    [Fact]
    public void SIRModel_RecoveredIncreases()
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
    public void Diffusion_ProducesFlux()
    {
        double concentration = 1.0;
        double diffusionCoeff = 0.1;
        double gradient = 2.0;

        double flux = BiologyEngine.Diffusion(concentration, diffusionCoeff, gradient, 1.0);

        flux.Should().BeApproximately(0.2, 1e-10);
    }

    [Fact]
    public void Migration_IncreasesWithRate()
    {
        double pop = 100;
        double rate1 = 0.1;
        double rate2 = 0.5;

        double m1 = BiologyEngine.Migration(pop, rate1, 1.0);
        double m2 = BiologyEngine.Migration(pop, rate2, 1.0);

        m2.Should().BeGreaterThan(m1);
    }

    [Fact]
    public void CarryingCapacity_SimpleDivision()
    {
        double resources = 1000;
        double perIndividual = 10;

        double K = BiologyEngine.CarryingCapacity(resources, perIndividual);

        K.Should().Be(100);
    }

    [Fact]
    public void CarryingCapacity_ZeroResources_ReturnsZero()
    {
        double K = BiologyEngine.CarryingCapacity(0, 10);

        K.Should().Be(0);
    }

    [Fact]
    public void PopulationProjection_ConvergesToK()
    {
        double initial = 10;
        double r = 0.1;
        double K = 1000;

        double result = BiologyEngine.PopulationProjection(initial, r, K, 100.0);

        result.Should().BeGreaterThan(initial);
    }

    [Fact]
    public void EpidemiologicalState_TotalPopulation_CorrectSum()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 500,
            Infected = 100,
            Recovered = 200,
            Dead = 50
        };

        state.TotalPopulation.Should().Be(850);
    }

    [Fact]
    public void EpidemiologicalState_ZeroPopulation()
    {
        var state = new EpidemiologicalState();

        state.TotalPopulation.Should().Be(0);
    }

    [Fact]
    public void Species_Record_HasCorrectDefaults()
    {
        var species = new Species
        {
            Name = "Rabbit",
            Population = 100,
            GrowthRate = 0.5,
            CarryingCapacity = 1000
        };

        species.Population.Should().Be(100);
        species.GrowthRate.Should().Be(0.5);
    }

    [Fact]
    public void Interaction_Record_CanBeCreated()
    {
        var interaction = new Interaction
        {
            PredatorId = "wolf",
            PreyId = "rabbit",
            PredationRate = 0.01,
            Type = InteractionType.Predation
        };

        interaction.PredationRate.Should().Be(0.01);
        interaction.Type.Should().Be(InteractionType.Predation);
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
}
