namespace MathVerse.Simulation.Tests.Finance;

using System.Collections.Immutable;
using SM = global::System.Math;

public sealed class FinancialStateTests
{
    [Fact]
    public void Create_SetsInitialCash()
    {
        var state = FinancialState.Create(10000);
        state.Cash.Should().Be(10000);
    }

    [Fact]
    public void Create_SetsPortfolioValue()
    {
        var state = FinancialState.Create(10000);
        state.PortfolioValue.Should().Be(10000);
    }

    [Fact]
    public void Create_SetsRiskFreeRate()
    {
        var state = FinancialState.Create(10000, 0.03);
        state.RiskFreeRate.Should().Be(0.03);
    }

    [Fact]
    public void Create_DefaultRiskFreeRate()
    {
        var state = FinancialState.Create(10000);
        state.RiskFreeRate.Should().Be(0.05);
    }

    [Fact]
    public void Create_EmptyHoldings()
    {
        var state = FinancialState.Create(10000);
        state.Holdings.Should().BeEmpty();
    }

    [Fact]
    public void Create_ZeroTime()
    {
        var state = FinancialState.Create(10000);
        state.Time.Should().Be(0);
    }

    [Fact]
    public void Create_EmptyHistory()
    {
        var state = FinancialState.Create(10000);
        state.History.Should().BeEmpty();
    }

    [Fact]
    public void Create_ZeroCash()
    {
        var state = FinancialState.Create(0);
        state.Cash.Should().Be(0);
        state.PortfolioValue.Should().Be(0);
    }

    [Fact]
    public void OptionContract_CallType()
    {
        var opt = new OptionContract { Type = OptionType.Call, Strike = 100, Expiration = 0.5 };
        opt.Type.Should().Be(OptionType.Call);
        opt.Strike.Should().Be(100);
    }

    [Fact]
    public void OptionContract_PutType()
    {
        var opt = new OptionContract { Type = OptionType.Put, Strike = 50 };
        opt.Type.Should().Be(OptionType.Put);
    }

    [Fact]
    public void OptionType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<OptionType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void OptionType_ContainsSixValues()
    {
        Enum.GetValues<OptionType>().Should().HaveCount(6);
    }

    [Fact]
    public void Transaction_DefaultValues()
    {
        var tx = new Transaction();
        tx.Asset.Should().Be(string.Empty);
        tx.Quantity.Should().Be(0);
        tx.Price.Should().Be(0);
        tx.Fees.Should().Be(0);
    }

    [Fact]
    public void TransactionType_AllValues_AreDistinct()
    {
        var values = Enum.GetValues<TransactionType>().Cast<int>().ToList();
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Transaction_BuyType()
    {
        var tx = new Transaction { Asset = "AAPL", Quantity = 10, Price = 150, Type = TransactionType.Buy };
        tx.Type.Should().Be(TransactionType.Buy);
        tx.Asset.Should().Be("AAPL");
    }
}
