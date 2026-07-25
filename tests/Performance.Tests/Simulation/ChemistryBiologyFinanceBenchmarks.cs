using BenchmarkDotNet.Attributes;

namespace MathVerse.Performance.Tests.Simulation;

[MemoryDiagnoser]
public class ChemistryBiologyFinanceBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
    }

    [Benchmark]
    public double EquilibriumConstant_StandardConditions()
        => ChemistryEngine.EquilibriumConstant(0.0, 298.15);

    [Benchmark]
    public double EquilibriumConstant_HighTemp()
        => ChemistryEngine.EquilibriumConstant(-5000.0, 500.0);

    [Benchmark]
    public double GibbsFreeEnergy_Formula()
        => ChemistryEngine.GibbsFreeEnergy(-200000.0, -200.0, 298.15);

    [Benchmark]
    public double ArrheniusRate_HighActivation()
        => ChemistryEngine.ArrheniusRate(1.0e10, 80000.0, 300.0);

    [Benchmark]
    public double ArrheniusRate_LowActivation()
        => ChemistryEngine.ArrheniusRate(1.0e8, 20000.0, 300.0);

    [Benchmark]
    public double ArrheniusRate_HighTemp()
        => ChemistryEngine.ArrheniusRate(1.0e10, 50000.0, 1000.0);

    [Benchmark]
    public double ArrheniusRate_LowTemp()
        => ChemistryEngine.ArrheniusRate(1.0e10, 50000.0, 200.0);

    [Benchmark]
    public double ReactionQuotient_SimpleReaction()
    {
        var products = ImmutableArray.Create(
            new SpeciesCoefficient { SpeciesId = "C", Coefficient = 1.0 },
            new SpeciesCoefficient { SpeciesId = "D", Coefficient = 2.0 });
        var reactants = ImmutableArray.Create(
            new SpeciesCoefficient { SpeciesId = "A", Coefficient = 1.0 },
            new SpeciesCoefficient { SpeciesId = "B", Coefficient = 1.0 });
        var concentrations = ImmutableDictionary<string, double>.Empty
            .Add("A", 0.5).Add("B", 0.3).Add("C", 0.8).Add("D", 0.2);
        return ChemistryEngine.ReactionQuotient(concentrations, products, reactants);
    }

    [Benchmark]
    public double ChemistryEngine_Constants()
    {
        double gc = ChemistryEngine.GasConstant;
        double an = ChemistryEngine.AvogadroNumber;
        double bk = ChemistryEngine.BoltzmannConstant;
        return gc + an + bk;
    }

    [Benchmark]
    public double LogisticGrowth_SmallPopulation()
        => BiologyEngine.LogisticGrowth(10.0, 0.5, 1000.0, 0.1);

    [Benchmark]
    public double LogisticGrowth_LargePopulation()
        => BiologyEngine.LogisticGrowth(800.0, 0.5, 1000.0, 0.1);

    [Benchmark]
    public double ExponentialGrowth_SmallRate()
        => BiologyEngine.ExponentialGrowth(100.0, 0.01, 1.0);

    [Benchmark]
    public double ExponentialGrowth_LargeRate()
        => BiologyEngine.ExponentialGrowth(100.0, 0.9, 1.0);

    [Benchmark]
    public double GompertzGrowth()
        => BiologyEngine.GompertzGrowth(500.0, 0.3, 10000.0, 1.0);

    [Benchmark]
    public (double prey, double predator) LotkaVolterra_Step()
        => BiologyEngine.LotkaVolterra(100.0, 50.0, 1.0, 0.1, 0.075, 0.02, 0.01);

    [Benchmark]
    public EpidemiologicalState SIRModel_Step()
    {
        var state = new EpidemiologicalState
        {
            Susceptible = 9990.0,
            Infected = 10.0,
            Recovered = 0.0,
            Dead = 0.0
        };
        return BiologyEngine.SIRModel(state, 0.3, 0.1, 0.01, 1.0);
    }

    [Benchmark]
    public EpidemiologicalState EpidemiologicalState_Creation()
        => new EpidemiologicalState
        {
            Susceptible = 5000.0,
            Infected = 100.0,
            Recovered = 200.0,
            Dead = 10.0
        };

    [Benchmark]
    public Species Species_Creation()
        => new Species
        {
            Id = "rabbit",
            Name = "Rabbit",
            Population = 500.0,
            GrowthRate = 0.4,
            CarryingCapacity = 2000.0,
            MortalityRate = 0.05,
            BirthRate = 0.6,
            DeathRate = 0.2
        };

    [Benchmark]
    public double BlackScholesCall_ITM()
        => FinanceEngine.BlackScholesCall(120.0, 100.0, 1.0, 0.05, 0.2);

    [Benchmark]
    public double BlackScholesPut_ITM()
        => FinanceEngine.BlackScholesPut(80.0, 100.0, 1.0, 0.05, 0.2);

    [Benchmark]
    public double BlackScholesCall_OTM()
        => FinanceEngine.BlackScholesCall(80.0, 100.0, 1.0, 0.05, 0.2);

    [Benchmark]
    public double BlackScholesPut_OTM()
        => FinanceEngine.BlackScholesPut(120.0, 100.0, 1.0, 0.05, 0.2);

    [Benchmark]
    public double CompoundInterest_Short()
        => FinanceEngine.CompoundInterest(10000.0, 0.05, 5.0, 12);

    [Benchmark]
    public double CompoundInterest_Long()
        => FinanceEngine.CompoundInterest(10000.0, 0.08, 30.0, 365);

    [Benchmark]
    public double PresentValue_Formula()
        => FinanceEngine.PresentValue(50000.0, 0.06, 10.0);

    [Benchmark]
    public double FutureValue_Formula()
        => FinanceEngine.FutureValue(10000.0, 0.07, 20.0);

    [Benchmark]
    public double NetPresentValue_3Flows()
    {
        var cashFlows = ImmutableArray.Create(-10000.0, 3000.0, 4000.0, 5000.0);
        return FinanceEngine.NetPresentValue(0.08, cashFlows);
    }
}
