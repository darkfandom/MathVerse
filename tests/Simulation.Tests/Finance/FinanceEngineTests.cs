namespace MathVerse.Simulation.Tests.Finance;

using System.Collections.Immutable;
using SM = global::System.Math;

public class FinanceEngineTests
{
    [Theory]
    [InlineData(1000, 0.05, 10, 1, 1628.89)]
    [InlineData(1000, 0.10, 5, 1, 1610.51)]
    [InlineData(1000, 0.05, 10, 12, 1647.01)]
    public void CompoundInterest_CorrectValues(double principal, double rate, double time, int freq, double expected)
    {
        double result = FinanceEngine.CompoundInterest(principal, rate, time, freq);

        result.Should().BeApproximately(expected, 1.0);
    }

    [Theory]
    [InlineData(1628.89, 0.05, 10, 1000.0)]
    [InlineData(2000, 0.10, 1, 1818.18)]
    public void PresentValue_CorrectValues(double fv, double rate, double time, double expected)
    {
        double result = FinanceEngine.PresentValue(fv, rate, time);

        result.Should().BeApproximately(expected, 1.0);
    }

    [Theory]
    [InlineData(1000, 0.05, 10, 1628.89)]
    [InlineData(1000, 0.10, 5, 1610.51)]
    public void FutureValue_CorrectValues(double pv, double rate, double time, double expected)
    {
        double result = FinanceEngine.FutureValue(pv, rate, time);

        result.Should().BeApproximately(expected, 1.0);
    }

    [Fact]
    public void FutureValue_EqualsCompoundInterest_WithAnnualCompounding()
    {
        double fv = FinanceEngine.FutureValue(1000, 0.05, 10);
        double ci = FinanceEngine.CompoundInterest(1000, 0.05, 10, 1);

        fv.Should().BeApproximately(ci, 1e-10);
    }

    [Fact]
    public void BlackScholesCall_InTheMoney_ProducesPositivePremium()
    {
        double S = 110, K = 100, T = 1.0, r = 0.05, sigma = 0.2;

        double call = FinanceEngine.BlackScholesCall(S, K, T, r, sigma);

        call.Should().BePositive();
    }

    [Fact]
    public void BlackScholesCall_AtTheMoney_ProducesPositivePremium()
    {
        double call = FinanceEngine.BlackScholesCall(100, 100, 1.0, 0.05, 0.2);

        call.Should().BePositive();
    }

    [Fact]
    public void BlackScholesPut_InTheMoney_ProducesPositivePremium()
    {
        double put = FinanceEngine.BlackScholesPut(90, 100, 1.0, 0.05, 0.2);

        put.Should().BePositive();
    }

    [Fact]
    public void BlackScholes_PutCallParity()
    {
        double S = 100, K = 100, T = 1.0, r = 0.05, sigma = 0.2;

        double call = FinanceEngine.BlackScholesCall(S, K, T, r, sigma);
        double put = FinanceEngine.BlackScholesPut(S, K, T, r, sigma);

        double parity = call - put - S + K * SM.Exp(-r * T);
        parity.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void BlackScholesCall_CallPlusPutEqualsForward()
    {
        double S = 100, K = 100, T = 1.0, r = 0.05, sigma = 0.2;

        double call = FinanceEngine.BlackScholesCall(S, K, T, r, sigma);
        double put = FinanceEngine.BlackScholesPut(S, K, T, r, sigma);

        (call - put).Should().BeApproximately(S - K * SM.Exp(-r * T), 1e-6);
    }

    [Fact]
    public void MonteCarloOptionPrice_ProducesPositiveValue()
    {
        Func<double, double> payoff = s => SM.Max(s - 1.0, 0);

        double price = FinanceEngine.MonteCarloOptionPrice(payoff, 100, 0.05, 0.2, 1.0, 10000);

        price.Should().BePositive();
    }

    [Fact]
    public void CompoundGrowth_MonthlyCompounding()
    {
        double result = FinanceEngine.CompoundGrowth(1000, 0.12, 1.0, 12);

        result.Should().BePositive();
        result.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void AnnuityPayment_ProducesPositiveValue()
    {
        double payment = FinanceEngine.AnnuityPayment(100000, 0.05, 30);

        payment.Should().BePositive();
        payment.Should().BeGreaterThan(0);
    }

    [Fact]
    public void NetPresentValue_PositiveCashFlows()
    {
        var cashFlows = ImmutableArray.Create(100.0, 200.0, 300.0);

        double npv = FinanceEngine.NetPresentValue(0.05, cashFlows);

        npv.Should().BePositive();
    }

    [Fact]
    public void NetPresentValue_ZeroRate_EqualsSum()
    {
        var cashFlows = ImmutableArray.Create(100.0, 200.0, 300.0);

        double npv = FinanceEngine.NetPresentValue(0.0, cashFlows);

        npv.Should().BeApproximately(600.0, 1e-6);
    }

    [Fact]
    public void InternalRateOfReturn_KnownCashFlows()
    {
        var cashFlows = ImmutableArray.Create(-1000.0, 300.0, 300.0, 300.0, 300.0);

        double irr = FinanceEngine.InternalRateOfReturn(cashFlows);

        irr.Should().BeGreaterThan(0);
        irr.Should().BeLessThan(1);
    }

    [Fact]
    public void InternalRateOfReturn_ZeroNPV()
    {
        var cashFlows = ImmutableArray.Create(-1000.0, 300.0, 300.0, 300.0, 300.0);

        double irr = FinanceEngine.InternalRateOfReturn(cashFlows);
        double npv = FinanceEngine.NetPresentValue(irr, cashFlows);

        npv.Should().BeApproximately(0, 0.1);
    }

    [Fact]
    public void BondPrice_NearPar()
    {
        double price = FinanceEngine.BondPrice(1000, 0.05, 0.05, 10);

        price.Should().BeApproximately(1000, 1.0);
    }

    [Fact]
    public void Duration_ProducesPositiveValue()
    {
        double dur = FinanceEngine.Duration(1000, 0.05, 0.05, 10);

        dur.Should().BePositive();
    }

    [Fact]
    public void FinancialState_Create_SetsInitialValues()
    {
        var state = FinancialState.Create(10000, 0.03);

        state.Cash.Should().Be(10000);
        state.PortfolioValue.Should().Be(10000);
        state.RiskFreeRate.Should().Be(0.03);
    }

    [Fact]
    public void OptionContract_Record_CanBeCreated()
    {
        var option = new OptionContract
        {
            Underlying = "AAPL",
            Type = OptionType.EuropeanCall,
            Strike = 150,
            Expiration = 0.25,
            Volatility = 0.3,
            RiskFreeRate = 0.05
        };

        option.Strike.Should().Be(150);
        option.Type.Should().Be(OptionType.EuropeanCall);
    }
}
